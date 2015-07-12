/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Helper class that is similar to
	/// <see cref="Sharpen.ByteArrayOutputStream"/>
	/// in usage, but more geared to Jackson use cases internally.
	/// Specific changes include segment storage (no need to have linear
	/// backing buffer, can avoid reallocations, copying), as well API
	/// not based on
	/// <see cref="Sharpen.OutputStream"/>
	/// . In short, a very much
	/// specialized builder object.
	/// <p>
	/// Also implements
	/// <see cref="Sharpen.OutputStream"/>
	/// to allow
	/// efficient aggregation of output content as a byte array, similar
	/// to how
	/// <see cref="Sharpen.ByteArrayOutputStream"/>
	/// works, but somewhat more
	/// efficiently for many use cases.
	/// </summary>
	public sealed class ByteArrayBuilder : Sharpen.OutputStream
	{
		public static readonly byte[] NO_BYTES = new byte[0];

		private const int INITIAL_BLOCK_SIZE = 500;

		private const int MAX_BLOCK_SIZE = (1 << 18);

		internal const int DEFAULT_BLOCK_ARRAY_SIZE = 40;

		private readonly com.fasterxml.jackson.core.util.BufferRecycler _bufferRecycler;

		private readonly System.Collections.Generic.List<byte[]> _pastBlocks = new System.Collections.Generic.List
			<byte[]>();

		private int _pastLen;

		private byte[] _currBlock;

		private int _currBlockPtr;

		public ByteArrayBuilder()
			: this(null)
		{
		}

		public ByteArrayBuilder(com.fasterxml.jackson.core.util.BufferRecycler br)
			: this(br, INITIAL_BLOCK_SIZE)
		{
		}

		public ByteArrayBuilder(int firstBlockSize)
			: this(null, firstBlockSize)
		{
		}

		public ByteArrayBuilder(com.fasterxml.jackson.core.util.BufferRecycler br, int firstBlockSize
			)
		{
			// Size of the first block we will allocate.
			// Maximum block size we will use for individual non-aggregated
			// blocks. Let's limit to using 256k chunks.
			// Optional buffer recycler instance that we can use for allocating the first block.
			// Number of bytes within byte arrays in {@link _pastBlocks}.
			_bufferRecycler = br;
			_currBlock = (br == null) ? new byte[firstBlockSize] : br.allocByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler
				.BYTE_WRITE_CONCAT_BUFFER);
		}

		public void reset()
		{
			_pastLen = 0;
			_currBlockPtr = 0;
			if (!_pastBlocks.isEmpty())
			{
				_pastBlocks.clear();
			}
		}

		/// <summary>
		/// Clean up method to call to release all buffers this object may be
		/// using.
		/// </summary>
		/// <remarks>
		/// Clean up method to call to release all buffers this object may be
		/// using. After calling the method, no other accessors can be used (and
		/// attempt to do so may result in an exception)
		/// </remarks>
		public void release()
		{
			reset();
			if (_bufferRecycler != null && _currBlock != null)
			{
				_bufferRecycler.releaseByteBuffer(com.fasterxml.jackson.core.util.BufferRecycler.
					BYTE_WRITE_CONCAT_BUFFER, _currBlock);
				_currBlock = null;
			}
		}

		public void append(int i)
		{
			if (_currBlockPtr >= _currBlock.Length)
			{
				_allocMore();
			}
			_currBlock[_currBlockPtr++] = unchecked((byte)i);
		}

		public void appendTwoBytes(int b16)
		{
			if ((_currBlockPtr + 1) < _currBlock.Length)
			{
				_currBlock[_currBlockPtr++] = unchecked((byte)(b16 >> 8));
				_currBlock[_currBlockPtr++] = unchecked((byte)b16);
			}
			else
			{
				append(b16 >> 8);
				append(b16);
			}
		}

		public void appendThreeBytes(int b24)
		{
			if ((_currBlockPtr + 2) < _currBlock.Length)
			{
				_currBlock[_currBlockPtr++] = unchecked((byte)(b24 >> 16));
				_currBlock[_currBlockPtr++] = unchecked((byte)(b24 >> 8));
				_currBlock[_currBlockPtr++] = unchecked((byte)b24);
			}
			else
			{
				append(b24 >> 16);
				append(b24 >> 8);
				append(b24);
			}
		}

		/// <summary>
		/// Method called when results are finalized and we can get the
		/// full aggregated result buffer to return to the caller
		/// </summary>
		public byte[] toByteArray()
		{
			int totalLen = _pastLen + _currBlockPtr;
			if (totalLen == 0)
			{
				// quick check: nothing aggregated?
				return NO_BYTES;
			}
			byte[] result = new byte[totalLen];
			int offset = 0;
			foreach (byte[] block in _pastBlocks)
			{
				int len = block.Length;
				System.Array.Copy(block, 0, result, offset, len);
				offset += len;
			}
			System.Array.Copy(_currBlock, 0, result, offset, _currBlockPtr);
			offset += _currBlockPtr;
			if (offset != totalLen)
			{
				// just a sanity check
				throw new Sharpen.RuntimeException("Internal error: total len assumed to be " + totalLen
					 + ", copied " + offset + " bytes");
			}
			// Let's only reset if there's sizable use, otherwise will get reset later on
			if (!_pastBlocks.isEmpty())
			{
				reset();
			}
			return result;
		}

		/*
		/**********************************************************
		/* Non-stream API (similar to TextBuffer), since 1.6
		/**********************************************************
		*/
		/// <summary>
		/// Method called when starting "manual" output: will clear out
		/// current state and return the first segment buffer to fill
		/// </summary>
		public byte[] resetAndGetFirstSegment()
		{
			reset();
			return _currBlock;
		}

		/// <summary>
		/// Method called when the current segment buffer is full; will
		/// append to current contents, allocate a new segment buffer
		/// and return it
		/// </summary>
		public byte[] finishCurrentSegment()
		{
			_allocMore();
			return _currBlock;
		}

		/// <summary>
		/// Method that will complete "manual" output process, coalesce
		/// content (if necessary) and return results as a contiguous buffer.
		/// </summary>
		/// <param name="lastBlockLength">
		/// Amount of content in the current segment
		/// buffer.
		/// </param>
		/// <returns>Coalesced contents</returns>
		public byte[] completeAndCoalesce(int lastBlockLength)
		{
			_currBlockPtr = lastBlockLength;
			return toByteArray();
		}

		public byte[] getCurrentSegment()
		{
			return _currBlock;
		}

		public void setCurrentSegmentLength(int len)
		{
			_currBlockPtr = len;
		}

		public int getCurrentSegmentLength()
		{
			return _currBlockPtr;
		}

		/*
		/**********************************************************
		/* OutputStream implementation
		/**********************************************************
		*/
		public override void write(byte[] b)
		{
			write(b, 0, b.Length);
		}

		public override void write(byte[] b, int off, int len)
		{
			while (true)
			{
				int max = _currBlock.Length - _currBlockPtr;
				int toCopy = System.Math.min(max, len);
				if (toCopy > 0)
				{
					System.Array.Copy(b, off, _currBlock, _currBlockPtr, toCopy);
					off += toCopy;
					_currBlockPtr += toCopy;
					len -= toCopy;
				}
				if (len <= 0)
				{
					break;
				}
				_allocMore();
			}
		}

		public override void write(int b)
		{
			append(b);
		}

		public override void close()
		{
		}

		/* NOP */
		public override void flush()
		{
		}

		/* NOP */
		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		private void _allocMore()
		{
			_pastLen += _currBlock.Length;
			/* Let's allocate block that's half the total size, except
			* never smaller than twice the initial block size.
			* The idea is just to grow with reasonable rate, to optimize
			* between minimal number of chunks and minimal amount of
			* wasted space.
			*/
			int newSize = System.Math.max((_pastLen >> 1), (INITIAL_BLOCK_SIZE + INITIAL_BLOCK_SIZE
				));
			// plus not to exceed max we define...
			if (newSize > MAX_BLOCK_SIZE)
			{
				newSize = MAX_BLOCK_SIZE;
			}
			_pastBlocks.Add(_currBlock);
			_currBlock = new byte[newSize];
			_currBlockPtr = 0;
		}
	}
}
