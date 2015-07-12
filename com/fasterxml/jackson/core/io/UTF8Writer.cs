using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	public sealed class UTF8Writer : System.IO.TextWriter
	{
		internal const int SURR1_FIRST = unchecked((int)(0xD800));

		internal const int SURR1_LAST = unchecked((int)(0xDBFF));

		internal const int SURR2_FIRST = unchecked((int)(0xDC00));

		internal const int SURR2_LAST = unchecked((int)(0xDFFF));

		private readonly com.fasterxml.jackson.core.io.IOContext _context;

		private Sharpen.OutputStream _out;

		private byte[] _outBuffer;

		private readonly int _outBufferEnd;

		private int _outPtr;

		/// <summary>When outputting chars from BMP, surrogate pairs need to be coalesced.</summary>
		/// <remarks>
		/// When outputting chars from BMP, surrogate pairs need to be coalesced.
		/// To do this, both pairs must be known first; and since it is possible
		/// pairs may be split, we need temporary storage for the first half
		/// </remarks>
		private int _surrogate = 0;

		public UTF8Writer(com.fasterxml.jackson.core.io.IOContext ctxt, Sharpen.OutputStream
			 @out)
		{
			_context = ctxt;
			_out = @out;
			_outBuffer = ctxt.allocWriteEncodingBuffer();
			/* Max. expansion for a single char (in unmodified UTF-8) is
			* 4 bytes (or 3 depending on how you view it -- 4 when recombining
			* surrogate pairs)
			*/
			_outBufferEnd = _outBuffer.Length - 4;
			_outPtr = 0;
		}

		/// <exception cref="System.IO.IOException"/>
		public override System.IO.TextWriter Append(char c)
		{
			write(c);
			return this;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			if (_out != null)
			{
				if (_outPtr > 0)
				{
					_out.write(_outBuffer, 0, _outPtr);
					_outPtr = 0;
				}
				Sharpen.OutputStream @out = _out;
				_out = null;
				byte[] buf = _outBuffer;
				if (buf != null)
				{
					_outBuffer = null;
					_context.releaseWriteEncodingBuffer(buf);
				}
				@out.close();
				/* Let's 'flush' orphan surrogate, no matter what; but only
				* after cleanly closing everything else.
				*/
				int code = _surrogate;
				_surrogate = 0;
				if (code > 0)
				{
					illegalSurrogate(code);
				}
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void flush()
		{
			if (_out != null)
			{
				if (_outPtr > 0)
				{
					_out.write(_outBuffer, 0, _outPtr);
					_outPtr = 0;
				}
				_out.flush();
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(char[] cbuf)
		{
			write(cbuf, 0, cbuf.Length);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(char[] cbuf, int off, int len)
		{
			if (len < 2)
			{
				if (len == 1)
				{
					write(cbuf[off]);
				}
				return;
			}
			// First: do we have a leftover surrogate to deal with?
			if (_surrogate > 0)
			{
				char second = cbuf[off++];
				--len;
				write(convertSurrogate(second));
			}
			// will have at least one more char
			int outPtr = _outPtr;
			byte[] outBuf = _outBuffer;
			int outBufLast = _outBufferEnd;
			// has 4 'spare' bytes
			// All right; can just loop it nice and easy now:
			len += off;
			// len will now be the end of input buffer
			for (; off < len; )
			{
				/* First, let's ensure we can output at least 4 bytes
				* (longest UTF-8 encoded codepoint):
				*/
				if (outPtr >= outBufLast)
				{
					_out.write(outBuf, 0, outPtr);
					outPtr = 0;
				}
				int c = cbuf[off++];
				// And then see if we have an Ascii char:
				if (c < unchecked((int)(0x80)))
				{
					// If so, can do a tight inner loop:
					outBuf[outPtr++] = unchecked((byte)c);
					// Let's calc how many ascii chars we can copy at most:
					int maxInCount = (len - off);
					int maxOutCount = (outBufLast - outPtr);
					if (maxInCount > maxOutCount)
					{
						maxInCount = maxOutCount;
					}
					maxInCount += off;
					while (true)
					{
						if (off >= maxInCount)
						{
							// done with max. ascii seq
							goto output_loop_continue;
						}
						c = cbuf[off++];
						if (c >= unchecked((int)(0x80)))
						{
							goto ascii_loop_break;
						}
						outBuf[outPtr++] = unchecked((byte)c);
ascii_loop_continue: ;
					}
ascii_loop_break: ;
				}
				// Nope, multi-byte:
				if (c < unchecked((int)(0x800)))
				{
					// 2-byte
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (c >> 6)));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
						(0x3f)))));
				}
				else
				{
					// 3 or 4 bytes
					// Surrogates?
					if (c < SURR1_FIRST || c > SURR2_LAST)
					{
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xe0)) | (c >> 12)));
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
							(int)(0x3f)))));
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
							(0x3f)))));
						continue;
					}
					// Yup, a surrogate:
					if (c > SURR1_LAST)
					{
						// must be from first range
						_outPtr = outPtr;
						illegalSurrogate(c);
					}
					_surrogate = c;
					// and if so, followed by another from next range
					if (off >= len)
					{
						// unless we hit the end?
						break;
					}
					c = convertSurrogate(cbuf[off++]);
					if (c > unchecked((int)(0x10FFFF)))
					{
						// illegal in JSON as well as in XML
						_outPtr = outPtr;
						illegalSurrogate(c);
					}
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xf0)) | (c >> 18)));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 12) & unchecked(
						(int)(0x3f)))));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
						(int)(0x3f)))));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
						(0x3f)))));
				}
output_loop_continue: ;
			}
output_loop_break: ;
			_outPtr = outPtr;
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(int c)
		{
			// First; do we have a left over surrogate?
			if (_surrogate > 0)
			{
				c = convertSurrogate(c);
			}
			else
			{
				// If not, do we start with a surrogate?
				if (c >= SURR1_FIRST && c <= SURR2_LAST)
				{
					// Illegal to get second part without first:
					if (c > SURR1_LAST)
					{
						illegalSurrogate(c);
					}
					// First part just needs to be held for now
					_surrogate = c;
					return;
				}
			}
			if (_outPtr >= _outBufferEnd)
			{
				// let's require enough room, first
				_out.write(_outBuffer, 0, _outPtr);
				_outPtr = 0;
			}
			if (c < unchecked((int)(0x80)))
			{
				// ascii
				_outBuffer[_outPtr++] = unchecked((byte)c);
			}
			else
			{
				int ptr = _outPtr;
				if (c < unchecked((int)(0x800)))
				{
					// 2-byte
					_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0xc0)) | (c >> 6)));
					_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int
						)(0x3f)))));
				}
				else
				{
					if (c <= unchecked((int)(0xFFFF)))
					{
						// 3 bytes
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0xe0)) | (c >> 12)));
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
							(int)(0x3f)))));
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int
							)(0x3f)))));
					}
					else
					{
						// 4 bytes
						if (c > unchecked((int)(0x10FFFF)))
						{
							// illegal
							illegalSurrogate(c);
						}
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0xf0)) | (c >> 18)));
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 12) & unchecked(
							(int)(0x3f)))));
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
							(int)(0x3f)))));
						_outBuffer[ptr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int
							)(0x3f)))));
					}
				}
				_outPtr = ptr;
			}
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(string str)
		{
			write(str, 0, str.Length);
		}

		/// <exception cref="System.IO.IOException"/>
		public override void write(string str, int off, int len)
		{
			if (len < 2)
			{
				if (len == 1)
				{
					write(str[off]);
				}
				return;
			}
			// First: do we have a leftover surrogate to deal with?
			if (_surrogate > 0)
			{
				char second = str[off++];
				--len;
				write(convertSurrogate(second));
			}
			// will have at least one more char (case of 1 char was checked earlier on)
			int outPtr = _outPtr;
			byte[] outBuf = _outBuffer;
			int outBufLast = _outBufferEnd;
			// has 4 'spare' bytes
			// All right; can just loop it nice and easy now:
			len += off;
			// len will now be the end of input buffer
			for (; off < len; )
			{
				/* First, let's ensure we can output at least 4 bytes
				* (longest UTF-8 encoded codepoint):
				*/
				if (outPtr >= outBufLast)
				{
					_out.write(outBuf, 0, outPtr);
					outPtr = 0;
				}
				int c = str[off++];
				// And then see if we have an Ascii char:
				if (c < unchecked((int)(0x80)))
				{
					// If so, can do a tight inner loop:
					outBuf[outPtr++] = unchecked((byte)c);
					// Let's calc how many ascii chars we can copy at most:
					int maxInCount = (len - off);
					int maxOutCount = (outBufLast - outPtr);
					if (maxInCount > maxOutCount)
					{
						maxInCount = maxOutCount;
					}
					maxInCount += off;
					while (true)
					{
						if (off >= maxInCount)
						{
							// done with max. ascii seq
							goto output_loop_continue;
						}
						c = str[off++];
						if (c >= unchecked((int)(0x80)))
						{
							goto ascii_loop_break;
						}
						outBuf[outPtr++] = unchecked((byte)c);
ascii_loop_continue: ;
					}
ascii_loop_break: ;
				}
				// Nope, multi-byte:
				if (c < unchecked((int)(0x800)))
				{
					// 2-byte
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (c >> 6)));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
						(0x3f)))));
				}
				else
				{
					// 3 or 4 bytes
					// Surrogates?
					if (c < SURR1_FIRST || c > SURR2_LAST)
					{
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xe0)) | (c >> 12)));
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
							(int)(0x3f)))));
						outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
							(0x3f)))));
						continue;
					}
					// Yup, a surrogate:
					if (c > SURR1_LAST)
					{
						// must be from first range
						_outPtr = outPtr;
						illegalSurrogate(c);
					}
					_surrogate = c;
					// and if so, followed by another from next range
					if (off >= len)
					{
						// unless we hit the end?
						break;
					}
					c = convertSurrogate(str[off++]);
					if (c > unchecked((int)(0x10FFFF)))
					{
						// illegal, as per RFC 4627
						_outPtr = outPtr;
						illegalSurrogate(c);
					}
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0xf0)) | (c >> 18)));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 12) & unchecked(
						(int)(0x3f)))));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) & unchecked(
						(int)(0x3f)))));
					outBuf[outPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked((int)
						(0x3f)))));
				}
output_loop_continue: ;
			}
output_loop_break: ;
			_outPtr = outPtr;
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		/// <summary>Method called to calculate UTF codepoint, from a surrogate pair.</summary>
		/// <exception cref="System.IO.IOException"/>
		protected internal int convertSurrogate(int secondPart)
		{
			int firstPart = _surrogate;
			_surrogate = 0;
			// Ok, then, is the second part valid?
			if (secondPart < SURR2_FIRST || secondPart > SURR2_LAST)
			{
				throw new System.IO.IOException("Broken surrogate pair: first char 0x" + Sharpen.Extensions.ToHexString
					(firstPart) + ", second 0x" + Sharpen.Extensions.ToHexString(secondPart) + "; illegal combination"
					);
			}
			return unchecked((int)(0x10000)) + ((firstPart - SURR1_FIRST) << 10) + (secondPart
				 - SURR2_FIRST);
		}

		/// <exception cref="System.IO.IOException"/>
		protected internal static void illegalSurrogate(int code)
		{
			throw new System.IO.IOException(illegalSurrogateDesc(code));
		}

		protected internal static string illegalSurrogateDesc(int code)
		{
			if (code > unchecked((int)(0x10FFFF)))
			{
				// over max?
				return "Illegal character point (0x" + Sharpen.Extensions.ToHexString(code) + ") to output; max is 0x10FFFF as per RFC 4627";
			}
			if (code >= SURR1_FIRST)
			{
				if (code <= SURR1_LAST)
				{
					// Unmatched first part (closing without second part?)
					return "Unmatched first part of surrogate pair (0x" + Sharpen.Extensions.ToHexString
						(code) + ")";
				}
				return "Unmatched second part of surrogate pair (0x" + Sharpen.Extensions.ToHexString
					(code) + ")";
			}
			// should we ever get this?
			return "Illegal character point (0x" + Sharpen.Extensions.ToHexString(code) + ") to output";
		}
	}
}
