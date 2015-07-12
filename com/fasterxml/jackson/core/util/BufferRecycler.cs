using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// This is a small utility class, whose main functionality is to allow
	/// simple reuse of raw byte/char buffers.
	/// </summary>
	/// <remarks>
	/// This is a small utility class, whose main functionality is to allow
	/// simple reuse of raw byte/char buffers. It is usually used through
	/// <code>ThreadLocal</code> member of the owning class pointing to
	/// instance of this class through a <code>SoftReference</code>. The
	/// end result is a low-overhead GC-cleanable recycling: hopefully
	/// ideal for use by stream readers.
	/// </remarks>
	public class BufferRecycler
	{
		/// <summary>Buffer used for reading byte-based input.</summary>
		public const int BYTE_READ_IO_BUFFER = 0;

		/// <summary>
		/// Buffer used for temporarily storing encoded content; used
		/// for example by UTF-8 encoding writer
		/// </summary>
		public const int BYTE_WRITE_ENCODING_BUFFER = 1;

		/// <summary>
		/// Buffer used for temporarily concatenating output; used for
		/// example when requesting output as byte array.
		/// </summary>
		public const int BYTE_WRITE_CONCAT_BUFFER = 2;

		/// <summary>
		/// Buffer used for concatenating binary data that is either being
		/// encoded as base64 output, or decoded from base64 input.
		/// </summary>
		/// <since>2.1</since>
		public const int BYTE_BASE64_CODEC_BUFFER = 3;

		public const int CHAR_TOKEN_BUFFER = 0;

		public const int CHAR_CONCAT_BUFFER = 1;

		public const int CHAR_TEXT_BUFFER = 2;

		public const int CHAR_NAME_COPY_BUFFER = 3;

		private static readonly int[] BYTE_BUFFER_LENGTHS = new int[] { 8000, 8000, 2000, 
			2000 };

		private static readonly int[] CHAR_BUFFER_LENGTHS = new int[] { 4000, 4000, 200, 
			200 };

		protected internal readonly byte[][] _byteBuffers;

		protected internal readonly char[][] _charBuffers;

		/// <summary>
		/// Default constructor used for creating instances of this default
		/// implementation.
		/// </summary>
		public BufferRecycler()
			: this(4, 4)
		{
		}

		/// <summary>
		/// Alternate constructor to be used by sub-classes, to allow customization
		/// of number of low-level buffers in use.
		/// </summary>
		/// <since>2.4</since>
		protected internal BufferRecycler(int bbCount, int cbCount)
		{
			// Tokenizable input
			// concatenated output
			// Text content from input
			// Temporary buffer for getting name characters
			// Buffer lengths, defined in 2.4 (smaller before that)
			/*
			/**********************************************************
			/* Construction
			/**********************************************************
			*/
			_byteBuffers = new byte[bbCount][];
			_charBuffers = new char[cbCount][];
		}

		/*
		/**********************************************************
		/* Public API, byte buffers
		/**********************************************************
		*/
		/// <param name="ix">One of <code>READ_IO_BUFFER</code> constants.</param>
		public byte[] allocByteBuffer(int ix)
		{
			return allocByteBuffer(ix, 0);
		}

		public virtual byte[] allocByteBuffer(int ix, int minSize)
		{
			int DEF_SIZE = byteBufferLength(ix);
			if (minSize < DEF_SIZE)
			{
				minSize = DEF_SIZE;
			}
			byte[] buffer = _byteBuffers[ix];
			if (buffer == null || buffer.Length < minSize)
			{
				buffer = balloc(minSize);
			}
			else
			{
				_byteBuffers[ix] = null;
			}
			return buffer;
		}

		public void releaseByteBuffer(int ix, byte[] buffer)
		{
			_byteBuffers[ix] = buffer;
		}

		/*
		/**********************************************************
		/* Public API, char buffers
		/**********************************************************
		*/
		public char[] allocCharBuffer(int ix)
		{
			return allocCharBuffer(ix, 0);
		}

		public virtual char[] allocCharBuffer(int ix, int minSize)
		{
			int DEF_SIZE = charBufferLength(ix);
			if (minSize < DEF_SIZE)
			{
				minSize = DEF_SIZE;
			}
			char[] buffer = _charBuffers[ix];
			if (buffer == null || buffer.Length < minSize)
			{
				buffer = calloc(minSize);
			}
			else
			{
				_charBuffers[ix] = null;
			}
			return buffer;
		}

		public virtual void releaseCharBuffer(int ix, char[] buffer)
		{
			_charBuffers[ix] = buffer;
		}

		/*
		/**********************************************************
		/* Overridable helper methods
		/**********************************************************
		*/
		protected internal virtual int byteBufferLength(int ix)
		{
			return BYTE_BUFFER_LENGTHS[ix];
		}

		protected internal virtual int charBufferLength(int ix)
		{
			return CHAR_BUFFER_LENGTHS[ix];
		}

		/*
		/**********************************************************
		/* Actual allocations separated for easier debugging/profiling
		/**********************************************************
		*/
		protected internal virtual byte[] balloc(int size)
		{
			return new byte[size];
		}

		protected internal virtual char[] calloc(int size)
		{
			return new char[size];
		}
	}
}
