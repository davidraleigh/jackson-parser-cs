using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// Since JDK does not come with UTF-32/UCS-4, let's implement a simple
	/// decoder to use.
	/// </summary>
	public class UTF32Reader : System.IO.StreamReader
	{
		/// <summary>
		/// JSON actually limits available Unicode range in the high end
		/// to the same as xml (to basically limit UTF-8 max byte sequence
		/// length to 4)
		/// </summary>
		protected internal const int LAST_VALID_UNICODE_CHAR = unchecked((int)(0x10FFFF));

		protected internal const char NC = (char)0;

		protected internal readonly com.fasterxml.jackson.core.io.IOContext _context;

		protected internal Sharpen.InputStream _in;

		protected internal byte[] _buffer;

		protected internal int _ptr;

		protected internal int _length;

		protected internal readonly bool _bigEndian;

		/// <summary>
		/// Although input is fine with full Unicode set, Java still uses
		/// 16-bit chars, so we may have to split high-order chars into
		/// surrogate pairs.
		/// </summary>
		protected internal char _surrogate = NC;

		/// <summary>Total read character count; used for error reporting purposes</summary>
		protected internal int _charCount = 0;

		/// <summary>Total read byte count; used for error reporting purposes</summary>
		protected internal int _byteCount = 0;

		protected internal readonly bool _managedBuffers;

		public UTF32Reader(com.fasterxml.jackson.core.io.IOContext ctxt, Sharpen.InputStream
			 @in, byte[] buf, int ptr, int len, bool isBigEndian)
		{
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_context = ctxt;
			_in = @in;
			_buffer = buf;
			_ptr = ptr;
			_length = len;
			_bigEndian = isBigEndian;
			_managedBuffers = (@in != null);
		}

		/*
		/**********************************************************
		/* Public API
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			Sharpen.InputStream @in = _in;
			if (@in != null)
			{
				_in = null;
				freeBuffers();
				@in.close();
			}
		}

		protected internal char[] _tmpBuf = null;

		/// <summary>
		/// Although this method is implemented by the base class, AND it should
		/// never be called by main code, let's still implement it bit more
		/// efficiently just in case
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public override int read()
		{
			if (_tmpBuf == null)
			{
				_tmpBuf = new char[1];
			}
			if (read(_tmpBuf, 0, 1) < 1)
			{
				return -1;
			}
			return _tmpBuf[0];
		}

		/// <exception cref="System.IO.IOException"/>
		public override int read(char[] cbuf, int start, int len)
		{
			// Already EOF?
			if (_buffer == null)
			{
				return -1;
			}
			if (len < 1)
			{
				return len;
			}
			// Let's then ensure there's enough room...
			if (start < 0 || (start + len) > cbuf.Length)
			{
				reportBounds(cbuf, start, len);
			}
			len += start;
			int outPtr = start;
			// Ok, first; do we have a surrogate from last round?
			if (_surrogate != NC)
			{
				cbuf[outPtr++] = _surrogate;
				_surrogate = NC;
			}
			else
			{
				// No need to load more, already got one char
				/* Note: we'll try to avoid blocking as much as possible. As a
				* result, we only need to get 4 bytes for a full char.
				*/
				int left = (_length - _ptr);
				if (left < 4)
				{
					if (!loadMore(left))
					{
						// (legal) EOF?
						return -1;
					}
				}
			}
			while (outPtr < len)
			{
				int ptr = _ptr;
				int ch;
				if (_bigEndian)
				{
					ch = (_buffer[ptr] << 24) | ((_buffer[ptr + 1] & unchecked((int)(0xFF))) << 16) |
						 ((_buffer[ptr + 2] & unchecked((int)(0xFF))) << 8) | (_buffer[ptr + 3] & unchecked(
						(int)(0xFF)));
				}
				else
				{
					ch = (_buffer[ptr] & unchecked((int)(0xFF))) | ((_buffer[ptr + 1] & unchecked((int
						)(0xFF))) << 8) | ((_buffer[ptr + 2] & unchecked((int)(0xFF))) << 16) | (_buffer
						[ptr + 3] << 24);
				}
				_ptr += 4;
				// Does it need to be split to surrogates?
				// (also, we can and need to verify illegal chars)
				if (ch > unchecked((int)(0xFFFF)))
				{
					// need to split into surrogates?
					if (ch > LAST_VALID_UNICODE_CHAR)
					{
						reportInvalid(ch, outPtr - start, "(above " + Sharpen.Extensions.ToHexString(LAST_VALID_UNICODE_CHAR
							) + ") ");
					}
					ch -= unchecked((int)(0x10000));
					// to normalize it starting with 0x0
					cbuf[outPtr++] = (char)(unchecked((int)(0xD800)) + (ch >> 10));
					// hmmh. can this ever be 0? (not legal, at least?)
					ch = (unchecked((int)(0xDC00)) | (ch & unchecked((int)(0x03FF))));
					// Room for second part?
					if (outPtr >= len)
					{
						// nope
						_surrogate = (char)ch;
						goto main_loop_break;
					}
				}
				cbuf[outPtr++] = (char)ch;
				if (_ptr >= _length)
				{
					goto main_loop_break;
				}
main_loop_continue: ;
			}
main_loop_break: ;
			len = outPtr - start;
			_charCount += len;
			return len;
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		private void reportUnexpectedEOF(int gotBytes, int needed)
		{
			int bytePos = _byteCount + gotBytes;
			int charPos = _charCount;
			throw new Sharpen.CharConversionException("Unexpected EOF in the middle of a 4-byte UTF-32 char: got "
				 + gotBytes + ", needed " + needed + ", at char #" + charPos + ", byte #" + bytePos
				 + ")");
		}

		/// <exception cref="System.IO.IOException"/>
		private void reportInvalid(int value, int offset, string msg)
		{
			int bytePos = _byteCount + _ptr - 1;
			int charPos = _charCount + offset;
			throw new Sharpen.CharConversionException("Invalid UTF-32 character 0x" + Sharpen.Extensions.ToHexString
				(value) + msg + " at char #" + charPos + ", byte #" + bytePos + ")");
		}

		/// <param name="available">Number of "unused" bytes in the input buffer</param>
		/// <returns>
		/// True, if enough bytes were read to allow decoding of at least
		/// one full character; false if EOF was encountered instead.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		private bool loadMore(int available)
		{
			_byteCount += (_length - available);
			// Bytes that need to be moved to the beginning of buffer?
			if (available > 0)
			{
				if (_ptr > 0)
				{
					System.Array.Copy(_buffer, _ptr, _buffer, 0, available);
					_ptr = 0;
				}
				_length = available;
			}
			else
			{
				/* Ok; here we can actually reasonably expect an EOF,
				* so let's do a separate read right away:
				*/
				_ptr = 0;
				int count = (_in == null) ? -1 : _in.read(_buffer);
				if (count < 1)
				{
					_length = 0;
					if (count < 0)
					{
						// -1
						if (_managedBuffers)
						{
							freeBuffers();
						}
						// to help GC?
						return false;
					}
					// 0 count is no good; let's err out
					reportStrangeStream();
				}
				_length = count;
			}
			/* Need at least 4 bytes; if we don't get that many, it's an
			* error.
			*/
			while (_length < 4)
			{
				int count = (_in == null) ? -1 : _in.read(_buffer, _length, _buffer.Length - _length
					);
				if (count < 1)
				{
					if (count < 0)
					{
						// -1, EOF... no good!
						if (_managedBuffers)
						{
							freeBuffers();
						}
						// to help GC?
						reportUnexpectedEOF(_length, 4);
					}
					// 0 count is no good; let's err out
					reportStrangeStream();
				}
				_length += count;
			}
			return true;
		}

		/// <summary>
		/// This method should be called along with (or instead of) normal
		/// close.
		/// </summary>
		/// <remarks>
		/// This method should be called along with (or instead of) normal
		/// close. After calling this method, no further reads should be tried.
		/// Method will try to recycle read buffers (if any).
		/// </remarks>
		private void freeBuffers()
		{
			byte[] buf = _buffer;
			if (buf != null)
			{
				_buffer = null;
				_context.releaseReadIOBuffer(buf);
			}
		}

		/// <exception cref="System.IO.IOException"/>
		private void reportBounds(char[] cbuf, int start, int len)
		{
			throw new System.IndexOutOfRangeException("read(buf," + start + "," + len + "), cbuf["
				 + cbuf.Length + "]");
		}

		/// <exception cref="System.IO.IOException"/>
		private void reportStrangeStream()
		{
			throw new System.IO.IOException("Strange I/O stream, returned 0 bytes on read");
		}
	}
}
