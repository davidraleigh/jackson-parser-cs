using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// TextBuffer is a class similar to
	/// <see cref="System.Text.StringBuilder"/>
	/// , with
	/// following differences:
	/// <ul>
	/// <li>TextBuffer uses segments character arrays, to avoid having
	/// to do additional array copies when array is not big enough.
	/// This means that only reallocating that is necessary is done only once:
	/// if and when caller
	/// wants to access contents in a linear array (char[], String).
	/// </li>
	/// <li>TextBuffer can also be initialized in "shared mode", in which
	/// it will just act as a wrapper to a single char array managed
	/// by another object (like parser that owns it)
	/// </li>
	/// <li>TextBuffer is not synchronized.
	/// </li>
	/// </ul>
	/// </summary>
	public sealed class TextBuffer
	{
		internal static readonly char[] NO_CHARS = new char[0];

		/// <summary>Let's start with sizable but not huge buffer, will grow as necessary</summary>
		internal const int MIN_SEGMENT_LEN = 1000;

		/// <summary>
		/// Let's limit maximum segment length to something sensible
		/// like 256k
		/// </summary>
		internal const int MAX_SEGMENT_LEN = unchecked((int)(0x40000));

		private readonly com.fasterxml.jackson.core.util.BufferRecycler _allocator;

		/// <summary>
		/// Shared input buffer; stored here in case some input can be returned
		/// as is, without being copied to collector's own buffers.
		/// </summary>
		/// <remarks>
		/// Shared input buffer; stored here in case some input can be returned
		/// as is, without being copied to collector's own buffers. Note that
		/// this is read-only for this Object.
		/// </remarks>
		private char[] _inputBuffer;

		/// <summary>
		/// Character offset of first char in input buffer; -1 to indicate
		/// that input buffer currently does not contain any useful char data
		/// </summary>
		private int _inputStart;

		private int _inputLen;

		/// <summary>List of segments prior to currently active segment.</summary>
		private Sharpen.AList<char[]> _segments;

		/// <summary>Flag that indicates whether _seqments is non-empty</summary>
		private bool _hasSegments = false;

		/// <summary>
		/// Amount of characters in segments in
		/// <see cref="_segments"/>
		/// </summary>
		private int _segmentSize;

		private char[] _currentSegment;

		/// <summary>Number of characters in currently active (last) segment</summary>
		private int _currentSize;

		/// <summary>
		/// String that will be constructed when the whole contents are
		/// needed; will be temporarily stored in case asked for again.
		/// </summary>
		private string _resultString;

		private char[] _resultArray;

		public TextBuffer(com.fasterxml.jackson.core.util.BufferRecycler allocator)
		{
			/*
			/**********************************************************
			/* Configuration:
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Shared input buffers
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Aggregation segments (when not using input buf)
			/**********************************************************
			*/
			// // // Currently used segment; not (yet) contained in _seqments
			/*
			/**********************************************************
			/* Caching of results
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_allocator = allocator;
		}

		/// <summary>
		/// Method called to indicate that the underlying buffers should now
		/// be recycled if they haven't yet been recycled.
		/// </summary>
		/// <remarks>
		/// Method called to indicate that the underlying buffers should now
		/// be recycled if they haven't yet been recycled. Although caller
		/// can still use this text buffer, it is not advisable to call this
		/// method if that is likely, since next time a buffer is needed,
		/// buffers need to reallocated.
		/// Note: calling this method automatically also clears contents
		/// of the buffer.
		/// </remarks>
		public void releaseBuffers()
		{
			if (_allocator == null)
			{
				resetWithEmpty();
			}
			else
			{
				if (_currentSegment != null)
				{
					// First, let's get rid of all but the largest char array
					resetWithEmpty();
					// And then return that array
					char[] buf = _currentSegment;
					_currentSegment = null;
					_allocator.releaseCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler.CHAR_TEXT_BUFFER
						, buf);
				}
			}
		}

		/// <summary>
		/// Method called to clear out any content text buffer may have, and
		/// initializes buffer to use non-shared data.
		/// </summary>
		public void resetWithEmpty()
		{
			_inputStart = -1;
			// indicates shared buffer not used
			_currentSize = 0;
			_inputLen = 0;
			_inputBuffer = null;
			_resultString = null;
			_resultArray = null;
			// And then reset internal input buffers, if necessary:
			if (_hasSegments)
			{
				clearSegments();
			}
		}

		/// <summary>
		/// Method called to initialize the buffer with a shared copy of data;
		/// this means that buffer will just have pointers to actual data.
		/// </summary>
		/// <remarks>
		/// Method called to initialize the buffer with a shared copy of data;
		/// this means that buffer will just have pointers to actual data. It
		/// also means that if anything is to be appended to the buffer, it
		/// will first have to unshare it (make a local copy).
		/// </remarks>
		public void resetWithShared(char[] buf, int start, int len)
		{
			// First, let's clear intermediate values, if any:
			_resultString = null;
			_resultArray = null;
			// Then let's mark things we need about input buffer
			_inputBuffer = buf;
			_inputStart = start;
			_inputLen = len;
			// And then reset internal input buffers, if necessary:
			if (_hasSegments)
			{
				clearSegments();
			}
		}

		public void resetWithCopy(char[] buf, int start, int len)
		{
			_inputBuffer = null;
			_inputStart = -1;
			// indicates shared buffer not used
			_inputLen = 0;
			_resultString = null;
			_resultArray = null;
			// And then reset internal input buffers, if necessary:
			if (_hasSegments)
			{
				clearSegments();
			}
			else
			{
				if (_currentSegment == null)
				{
					_currentSegment = buf(len);
				}
			}
			_currentSize = _segmentSize = 0;
			append(buf, start, len);
		}

		public void resetWithString(string value)
		{
			_inputBuffer = null;
			_inputStart = -1;
			_inputLen = 0;
			_resultString = value;
			_resultArray = null;
			if (_hasSegments)
			{
				clearSegments();
			}
			_currentSize = 0;
		}

		/// <summary>
		/// Helper method used to find a buffer to use, ideally one
		/// recycled earlier.
		/// </summary>
		private char[] buf(int needed)
		{
			if (_allocator != null)
			{
				return _allocator.allocCharBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					CHAR_TEXT_BUFFER, needed);
			}
			return new char[System.Math.max(needed, MIN_SEGMENT_LEN)];
		}

		private void clearSegments()
		{
			_hasSegments = false;
			/* Let's start using _last_ segment from list; for one, it's
			* the biggest one, and it's also most likely to be cached
			*/
			/* 28-Aug-2009, tatu: Actually, the current segment should
			*   be the biggest one, already
			*/
			//_currentSegment = _segments.get(_segments.size() - 1);
			_segments.clear();
			_currentSize = _segmentSize = 0;
		}

		/*
		/**********************************************************
		/* Accessors for implementing public interface
		/**********************************************************
		*/
		/// <returns>Number of characters currently stored by this collector</returns>
		public int size()
		{
			if (_inputStart >= 0)
			{
				// shared copy from input buf
				return _inputLen;
			}
			if (_resultArray != null)
			{
				return _resultArray.Length;
			}
			if (_resultString != null)
			{
				return _resultString.Length;
			}
			// local segmented buffers
			return _segmentSize + _currentSize;
		}

		public int getTextOffset()
		{
			/* Only shared input buffer can have non-zero offset; buffer
			* segments start at 0, and if we have to create a combo buffer,
			* that too will start from beginning of the buffer
			*/
			return (_inputStart >= 0) ? _inputStart : 0;
		}

		/// <summary>
		/// Method that can be used to check whether textual contents can
		/// be efficiently accessed using
		/// <see cref="getTextBuffer()"/>
		/// .
		/// </summary>
		public bool hasTextAsCharacters()
		{
			// if we have array in some form, sure
			if (_inputStart >= 0 || _resultArray != null)
			{
				return true;
			}
			// not if we have String as value
			if (_resultString != null)
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// Accessor that may be used to get the contents of this buffer in a single
		/// <code>char</code> array regardless of whether they were collected in a segmented
		/// fashion or not.
		/// </summary>
		public char[] getTextBuffer()
		{
			// Are we just using shared input buffer?
			if (_inputStart >= 0)
			{
				return _inputBuffer;
			}
			if (_resultArray != null)
			{
				return _resultArray;
			}
			if (_resultString != null)
			{
				return (_resultArray = _resultString.ToCharArray());
			}
			// Nope; but does it fit in just one segment?
			if (!_hasSegments)
			{
				return (_currentSegment == null) ? NO_CHARS : _currentSegment;
			}
			// Nope, need to have/create a non-segmented array and return it
			return contentsAsArray();
		}

		/*
		/**********************************************************
		/* Other accessors:
		/**********************************************************
		*/
		public string contentsAsString()
		{
			if (_resultString == null)
			{
				// Has array been requested? Can make a shortcut, if so:
				if (_resultArray != null)
				{
					_resultString = new string(_resultArray);
				}
				else
				{
					// Do we use shared array?
					if (_inputStart >= 0)
					{
						if (_inputLen < 1)
						{
							return (_resultString = string.Empty);
						}
						_resultString = new string(_inputBuffer, _inputStart, _inputLen);
					}
					else
					{
						// nope... need to copy
						// But first, let's see if we have just one buffer
						int segLen = _segmentSize;
						int currLen = _currentSize;
						if (segLen == 0)
						{
							// yup
							_resultString = (currLen == 0) ? string.Empty : new string(_currentSegment, 0, currLen
								);
						}
						else
						{
							// no, need to combine
							System.Text.StringBuilder sb = new System.Text.StringBuilder(segLen + currLen);
							// First stored segments
							if (_segments != null)
							{
								for (int i = 0; i < len; ++i)
								{
									char[] curr = _segments[i];
									sb.Append(curr, 0, curr.Length);
								}
							}
							// And finally, current segment:
							sb.Append(_currentSegment, 0, _currentSize);
							_resultString = sb.ToString();
						}
					}
				}
			}
			return _resultString;
		}

		public char[] contentsAsArray()
		{
			char[] result = _resultArray;
			if (result == null)
			{
				_resultArray = result = resultArray();
			}
			return result;
		}

		/// <summary>
		/// Convenience method for converting contents of the buffer
		/// into a
		/// <see cref="java.math.BigDecimal"/>
		/// .
		/// </summary>
		/// <exception cref="System.FormatException"/>
		public java.math.BigDecimal contentsAsDecimal()
		{
			// Already got a pre-cut array?
			if (_resultArray != null)
			{
				return com.fasterxml.jackson.core.io.NumberInput.parseBigDecimal(_resultArray);
			}
			// Or a shared buffer?
			if ((_inputStart >= 0) && (_inputBuffer != null))
			{
				return com.fasterxml.jackson.core.io.NumberInput.parseBigDecimal(_inputBuffer, _inputStart
					, _inputLen);
			}
			// Or if not, just a single buffer (the usual case)
			if ((_segmentSize == 0) && (_currentSegment != null))
			{
				return com.fasterxml.jackson.core.io.NumberInput.parseBigDecimal(_currentSegment, 
					0, _currentSize);
			}
			// If not, let's just get it aggregated...
			return com.fasterxml.jackson.core.io.NumberInput.parseBigDecimal(contentsAsArray(
				));
		}

		/// <summary>
		/// Convenience method for converting contents of the buffer
		/// into a Double value.
		/// </summary>
		/// <exception cref="System.FormatException"/>
		public double contentsAsDouble()
		{
			return com.fasterxml.jackson.core.io.NumberInput.parseDouble(contentsAsString());
		}

		/*
		/**********************************************************
		/* Public mutators:
		/**********************************************************
		*/
		/// <summary>
		/// Method called to make sure that buffer is not using shared input
		/// buffer; if it is, it will copy such contents to private buffer.
		/// </summary>
		public void ensureNotShared()
		{
			if (_inputStart >= 0)
			{
				unshare(16);
			}
		}

		public void append(char c)
		{
			// Using shared buffer so far?
			if (_inputStart >= 0)
			{
				unshare(16);
			}
			_resultString = null;
			_resultArray = null;
			// Room in current segment?
			char[] curr = _currentSegment;
			if (_currentSize >= curr.Length)
			{
				expand(1);
				curr = _currentSegment;
			}
			curr[_currentSize++] = c;
		}

		public void append(char[] c, int start, int len)
		{
			// Can't append to shared buf (sanity check)
			if (_inputStart >= 0)
			{
				unshare(len);
			}
			_resultString = null;
			_resultArray = null;
			// Room in current segment?
			char[] curr = _currentSegment;
			int max = curr.Length - _currentSize;
			if (max >= len)
			{
				System.Array.Copy(c, start, curr, _currentSize, len);
				_currentSize += len;
				return;
			}
			// No room for all, need to copy part(s):
			if (max > 0)
			{
				System.Array.Copy(c, start, curr, _currentSize, max);
				start += max;
				len -= max;
			}
			do
			{
				/* And then allocate new segment; we are guaranteed to now
				* have enough room in segment.
				*/
				// Except, as per [Issue-24], not for HUGE appends... so:
				expand(len);
				int amount = System.Math.min(_currentSegment.Length, len);
				System.Array.Copy(c, start, _currentSegment, 0, amount);
				_currentSize += amount;
				start += amount;
				len -= amount;
			}
			while (len > 0);
		}

		public void append(string str, int offset, int len)
		{
			// Can't append to shared buf (sanity check)
			if (_inputStart >= 0)
			{
				unshare(len);
			}
			_resultString = null;
			_resultArray = null;
			// Room in current segment?
			char[] curr = _currentSegment;
			int max = curr.Length - _currentSize;
			if (max >= len)
			{
				Sharpen.Runtime.getCharsForString(str, offset, offset + len, curr, _currentSize);
				_currentSize += len;
				return;
			}
			// No room for all, need to copy part(s):
			if (max > 0)
			{
				Sharpen.Runtime.getCharsForString(str, offset, offset + max, curr, _currentSize);
				len -= max;
				offset += max;
			}
			do
			{
				/* And then allocate new segment; we are guaranteed to now
				* have enough room in segment.
				*/
				// Except, as per [Issue-24], not for HUGE appends... so:
				expand(len);
				int amount = System.Math.min(_currentSegment.Length, len);
				Sharpen.Runtime.getCharsForString(str, offset, offset + amount, _currentSegment, 
					0);
				_currentSize += amount;
				offset += amount;
				len -= amount;
			}
			while (len > 0);
		}

		/*
		/**********************************************************
		/* Raw access, for high-performance use:
		/**********************************************************
		*/
		public char[] getCurrentSegment()
		{
			/* Since the intention of the caller is to directly add stuff into
			* buffers, we should NOT have anything in shared buffer... ie. may
			* need to unshare contents.
			*/
			if (_inputStart >= 0)
			{
				unshare(1);
			}
			else
			{
				char[] curr = _currentSegment;
				if (curr == null)
				{
					_currentSegment = buf(0);
				}
				else
				{
					if (_currentSize >= curr.Length)
					{
						// Plus, we better have room for at least one more char
						expand(1);
					}
				}
			}
			return _currentSegment;
		}

		public char[] emptyAndGetCurrentSegment()
		{
			// inlined 'resetWithEmpty()'
			_inputStart = -1;
			// indicates shared buffer not used
			_currentSize = 0;
			_inputLen = 0;
			_inputBuffer = null;
			_resultString = null;
			_resultArray = null;
			// And then reset internal input buffers, if necessary:
			if (_hasSegments)
			{
				clearSegments();
			}
			char[] curr = _currentSegment;
			if (curr == null)
			{
				_currentSegment = curr = buf(0);
			}
			return curr;
		}

		public int getCurrentSegmentSize()
		{
			return _currentSize;
		}

		public void setCurrentLength(int len)
		{
			_currentSize = len;
		}

		/// <since>2.6</since>
		public string setCurrentAndReturn(int len)
		{
			_currentSize = len;
			// We can simplify handling here compared to full `contentsAsString()`:
			if (_segmentSize > 0)
			{
				// longer text; call main method
				return contentsAsString();
			}
			// more common case: single segment
			int currLen = _currentSize;
			string str = (currLen == 0) ? string.Empty : new string(_currentSegment, 0, currLen
				);
			_resultString = str;
			return str;
		}

		public char[] finishCurrentSegment()
		{
			if (_segments == null)
			{
				_segments = new Sharpen.AList<char[]>();
			}
			_hasSegments = true;
			_segments.Add(_currentSegment);
			int oldLen = _currentSegment.Length;
			_segmentSize += oldLen;
			_currentSize = 0;
			// Let's grow segments by 50%
			int newLen = oldLen + (oldLen >> 1);
			if (newLen < MIN_SEGMENT_LEN)
			{
				newLen = MIN_SEGMENT_LEN;
			}
			else
			{
				if (newLen > MAX_SEGMENT_LEN)
				{
					newLen = MAX_SEGMENT_LEN;
				}
			}
			char[] curr = carr(newLen);
			_currentSegment = curr;
			return curr;
		}

		/// <summary>
		/// Method called to expand size of the current segment, to
		/// accommodate for more contiguous content.
		/// </summary>
		/// <remarks>
		/// Method called to expand size of the current segment, to
		/// accommodate for more contiguous content. Usually only
		/// used when parsing tokens like names if even then.
		/// </remarks>
		public char[] expandCurrentSegment()
		{
			char[] curr = _currentSegment;
			// Let's grow by 50% by default
			int len = curr.Length;
			int newLen = len + (len >> 1);
			// but above intended maximum, slow to increase by 25%
			if (newLen > MAX_SEGMENT_LEN)
			{
				newLen = len + (len >> 2);
			}
			return (_currentSegment = java.util.Arrays.copyOf(curr, newLen));
		}

		/// <summary>
		/// Method called to expand size of the current segment, to
		/// accommodate for more contiguous content.
		/// </summary>
		/// <remarks>
		/// Method called to expand size of the current segment, to
		/// accommodate for more contiguous content. Usually only
		/// used when parsing tokens like names if even then.
		/// </remarks>
		/// <param name="minSize">Required minimum strength of the current segment</param>
		/// <since>2.4.0</since>
		public char[] expandCurrentSegment(int minSize)
		{
			char[] curr = _currentSegment;
			if (curr.Length >= minSize)
			{
				return curr;
			}
			_currentSegment = curr = java.util.Arrays.copyOf(curr, minSize);
			return curr;
		}

		/*
		/**********************************************************
		/* Standard methods:
		/**********************************************************
		*/
		/// <summary>
		/// Note: calling this method may not be as efficient as calling
		/// <see cref="contentsAsString()"/>
		/// , since it's not guaranteed that resulting
		/// String is cached.
		/// </summary>
		public override string ToString()
		{
			return contentsAsString();
		}

		/*
		/**********************************************************
		/* Internal methods:
		/**********************************************************
		*/
		/// <summary>
		/// Method called if/when we need to append content when we have been
		/// initialized to use shared buffer.
		/// </summary>
		private void unshare(int needExtra)
		{
			int sharedLen = _inputLen;
			_inputLen = 0;
			char[] inputBuf = _inputBuffer;
			_inputBuffer = null;
			int start = _inputStart;
			_inputStart = -1;
			// Is buffer big enough, or do we need to reallocate?
			int needed = sharedLen + needExtra;
			if (_currentSegment == null || needed > _currentSegment.Length)
			{
				_currentSegment = buf(needed);
			}
			if (sharedLen > 0)
			{
				System.Array.Copy(inputBuf, start, _currentSegment, 0, sharedLen);
			}
			_segmentSize = 0;
			_currentSize = sharedLen;
		}

		/// <summary>
		/// Method called when current segment is full, to allocate new
		/// segment.
		/// </summary>
		private void expand(int minNewSegmentSize)
		{
			// First, let's move current segment to segment list:
			if (_segments == null)
			{
				_segments = new Sharpen.AList<char[]>();
			}
			char[] curr = _currentSegment;
			_hasSegments = true;
			_segments.Add(curr);
			_segmentSize += curr.Length;
			_currentSize = 0;
			int oldLen = curr.Length;
			// Let's grow segments by 50% minimum
			int newLen = oldLen + (oldLen >> 1);
			if (newLen < MIN_SEGMENT_LEN)
			{
				newLen = MIN_SEGMENT_LEN;
			}
			else
			{
				if (newLen > MAX_SEGMENT_LEN)
				{
					newLen = MAX_SEGMENT_LEN;
				}
			}
			_currentSegment = carr(newLen);
		}

		private char[] resultArray()
		{
			if (_resultString != null)
			{
				// Can take a shortcut...
				return _resultString.ToCharArray();
			}
			// Do we use shared array?
			if (_inputStart >= 0)
			{
				int len = _inputLen;
				if (len < 1)
				{
					return NO_CHARS;
				}
				int start = _inputStart;
				if (start == 0)
				{
					return java.util.Arrays.copyOf(_inputBuffer, len);
				}
				return java.util.Arrays.copyOfRange(_inputBuffer, start, start + len);
			}
			// nope, not shared
			int size = size();
			if (size < 1)
			{
				return NO_CHARS;
			}
			int offset = 0;
			char[] result = carr(size);
			if (_segments != null)
			{
				for (int i = 0; i < len; ++i)
				{
					char[] curr = _segments[i];
					int currLen = curr.Length;
					System.Array.Copy(curr, 0, result, offset, currLen);
					offset += currLen;
				}
			}
			System.Array.Copy(_currentSegment, 0, result, offset, _currentSize);
			return result;
		}

		private char[] carr(int len)
		{
			return new char[len];
		}
	}
}
