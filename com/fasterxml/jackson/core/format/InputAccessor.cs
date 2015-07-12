using Sharpen;

namespace com.fasterxml.jackson.core.format
{
	/// <summary>
	/// Interface used to expose beginning of a data file to data format
	/// detection code.
	/// </summary>
	public class InputAccessor
	{
		/// <summary>Method to call to check if more input is available.</summary>
		/// <remarks>
		/// Method to call to check if more input is available.
		/// Since this may result in more content to be read (at least
		/// one more byte), a
		/// <see cref="System.IO.IOException"/>
		/// may get thrown.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		bool hasMoreBytes();

		/// <summary>
		/// Returns next byte available, if any; if no more bytes are
		/// available, will throw
		/// <see cref="Sharpen.EOFException"/>
		/// .
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		byte nextByte();

		/// <summary>
		/// Method that can be called to reset accessor to read from beginning
		/// of input.
		/// </summary>
		void reset();

		/// <summary>
		/// Basic implementation that reads data from given
		/// <see cref="Sharpen.InputStream"/>
		/// and buffers it as necessary.
		/// </summary>
		public class Std : com.fasterxml.jackson.core.format.InputAccessor
		{
			protected internal readonly Sharpen.InputStream _in;

			protected internal readonly byte[] _buffer;

			protected internal readonly int _bufferedStart;

			/// <summary>End of valid bytes in the buffer (points to one past last valid)</summary>
			protected internal int _bufferedEnd;

			/// <summary>
			/// Pointer to next available buffered byte in
			/// <see cref="_buffer"/>
			/// .
			/// </summary>
			protected internal int _ptr;

			/// <summary>
			/// Constructor used when content to check is available via
			/// input stream and must be read.
			/// </summary>
			public Std(Sharpen.InputStream @in, byte[] buffer)
			{
				/*
				/**********************************************************
				/* Standard implementation
				/**********************************************************
				*/
				_in = @in;
				_buffer = buffer;
				_bufferedStart = 0;
				_ptr = 0;
				_bufferedEnd = 0;
			}

			/// <summary>
			/// Constructor used when the full input (or at least enough leading bytes
			/// of full input) is available.
			/// </summary>
			public Std(byte[] inputDocument)
			{
				_in = null;
				_buffer = inputDocument;
				// we have it all:
				_bufferedStart = 0;
				_bufferedEnd = inputDocument.Length;
			}

			/// <summary>
			/// Constructor used when the full input (or at least enough leading bytes
			/// of full input) is available.
			/// </summary>
			/// <since>2.1</since>
			public Std(byte[] inputDocument, int start, int len)
			{
				_in = null;
				_buffer = inputDocument;
				_ptr = start;
				_bufferedStart = start;
				_bufferedEnd = start + len;
			}

			/// <exception cref="System.IO.IOException"/>
			public virtual bool hasMoreBytes()
			{
				if (_ptr < _bufferedEnd)
				{
					// already got more
					return true;
				}
				if (_in == null)
				{
					// nowhere to read from
					return false;
				}
				int amount = _buffer.Length - _ptr;
				if (amount < 1)
				{
					// can not load any more
					return false;
				}
				int count = _in.read(_buffer, _ptr, amount);
				if (count <= 0)
				{
					// EOF
					return false;
				}
				_bufferedEnd += count;
				return true;
			}

			/// <exception cref="System.IO.IOException"/>
			public virtual byte nextByte()
			{
				// should we just try loading more automatically?
				if (_ptr >= _bufferedEnd)
				{
					if (!hasMoreBytes())
					{
						throw new Sharpen.EOFException("Failed auto-detect: could not read more than " + 
							_ptr + " bytes (max buffer size: " + _buffer.Length + ")");
					}
				}
				return _buffer[_ptr++];
			}

			public virtual void reset()
			{
				_ptr = _bufferedStart;
			}

			/*
			/**********************************************************
			/* Extended API for DataFormatDetector/Matcher
			/**********************************************************
			*/
			public virtual com.fasterxml.jackson.core.format.DataFormatMatcher createMatcher(
				com.fasterxml.jackson.core.JsonFactory match, com.fasterxml.jackson.core.format.MatchStrength
				 matchStrength)
			{
				return new com.fasterxml.jackson.core.format.DataFormatMatcher(_in, _buffer, _bufferedStart
					, (_bufferedEnd - _bufferedStart), match, matchStrength);
			}
		}
	}

	public static class InputAccessorConstants
	{
	}
}
