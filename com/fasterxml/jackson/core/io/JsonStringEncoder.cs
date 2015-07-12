using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// Helper class used for efficient encoding of JSON String values (including
	/// JSON field names) into Strings or UTF-8 byte arrays.
	/// </summary>
	/// <remarks>
	/// Helper class used for efficient encoding of JSON String values (including
	/// JSON field names) into Strings or UTF-8 byte arrays.
	/// <p>
	/// Note that methods in here are somewhat optimized, but not ridiculously so.
	/// Reason is that conversion method results are expected to be cached so that
	/// these methods will not be hot spots during normal operation.
	/// </remarks>
	public sealed class JsonStringEncoder
	{
		private static readonly char[] HC = com.fasterxml.jackson.core.io.CharTypes.copyHexChars
			();

		private static readonly byte[] HB = com.fasterxml.jackson.core.io.CharTypes.copyHexBytes
			();

		private const int SURR1_FIRST = unchecked((int)(0xD800));

		private const int SURR1_LAST = unchecked((int)(0xDBFF));

		private const int SURR2_FIRST = unchecked((int)(0xDC00));

		private const int SURR2_LAST = unchecked((int)(0xDFFF));

		/// <summary>
		/// This <code>ThreadLocal</code> contains a
		/// <see cref="Sharpen.SoftReference{T}"/>
		/// to a
		/// <see cref="com.fasterxml.jackson.core.util.BufferRecycler"/>
		/// used to provide a low-cost
		/// buffer recycling between reader and writer instances.
		/// </summary>
		protected internal static readonly java.lang.ThreadLocal<Sharpen.SoftReference<com.fasterxml.jackson.core.io.JsonStringEncoder
			>> _threadEncoder = new java.lang.ThreadLocal<Sharpen.SoftReference<com.fasterxml.jackson.core.io.JsonStringEncoder
			>>();

		/// <summary>
		/// Lazily constructed text buffer used to produce JSON encoded Strings
		/// as characters (without UTF-8 encoding)
		/// </summary>
		protected internal com.fasterxml.jackson.core.util.TextBuffer _text;

		/// <summary>
		/// Lazily-constructed builder used for UTF-8 encoding of text values
		/// (quoted and unquoted)
		/// </summary>
		protected internal com.fasterxml.jackson.core.util.ByteArrayBuilder _bytes;

		/// <summary>Temporary buffer used for composing quote/escape sequences</summary>
		protected internal readonly char[] _qbuf;

		public JsonStringEncoder()
		{
			//    private final static int INT_BACKSLASH = '\\';
			//    private final static int INT_U = 'u';
			//    private final static int INT_0 = '0';
			/*
			/**********************************************************
			/* Construction, instance access
			/**********************************************************
			*/
			_qbuf = new char[6];
			_qbuf[0] = '\\';
			_qbuf[2] = '0';
			_qbuf[3] = '0';
		}

		/// <summary>
		/// Factory method for getting an instance; this is either recycled per-thread instance,
		/// or a newly constructed one.
		/// </summary>
		public static com.fasterxml.jackson.core.io.JsonStringEncoder getInstance()
		{
			Sharpen.SoftReference<com.fasterxml.jackson.core.io.JsonStringEncoder> @ref = _threadEncoder
				.get();
			com.fasterxml.jackson.core.io.JsonStringEncoder enc = (@ref == null) ? null : @ref
				.get();
			if (enc == null)
			{
				enc = new com.fasterxml.jackson.core.io.JsonStringEncoder();
				_threadEncoder.set(new Sharpen.SoftReference<com.fasterxml.jackson.core.io.JsonStringEncoder
					>(enc));
			}
			return enc;
		}

		/*
		/**********************************************************
		/* Public API
		/**********************************************************
		*/
		/// <summary>
		/// Method that will quote text contents using JSON standard quoting,
		/// and return results as a character array
		/// </summary>
		public char[] quoteAsString(string input)
		{
			com.fasterxml.jackson.core.util.TextBuffer textBuffer = _text;
			if (textBuffer == null)
			{
				// no allocator; can add if we must, shouldn't need to
				_text = textBuffer = new com.fasterxml.jackson.core.util.TextBuffer(null);
			}
			char[] outputBuffer = textBuffer.emptyAndGetCurrentSegment();
			int[] escCodes = com.fasterxml.jackson.core.io.CharTypes.get7BitOutputEscapes();
			int escCodeCount = escCodes.Length;
			int inPtr = 0;
			int inputLen = input.Length;
			int outPtr = 0;
			while (inPtr < inputLen)
			{
				while (true)
				{
					char c = input[inPtr];
					if (c < escCodeCount && escCodes[c] != 0)
					{
						goto tight_loop_break;
					}
					if (outPtr >= outputBuffer.Length)
					{
						outputBuffer = textBuffer.finishCurrentSegment();
						outPtr = 0;
					}
					outputBuffer[outPtr++] = c;
					if (++inPtr >= inputLen)
					{
						goto outer_break;
					}
tight_loop_continue: ;
				}
tight_loop_break: ;
				// something to escape; 2 or 6-char variant? 
				char d = input[inPtr++];
				int escCode = escCodes[d];
				int length = (escCode < 0) ? _appendNumeric(d, _qbuf) : _appendNamed(escCode, _qbuf
					);
				if ((outPtr + length) > outputBuffer.Length)
				{
					int first = outputBuffer.Length - outPtr;
					if (first > 0)
					{
						System.Array.Copy(_qbuf, 0, outputBuffer, outPtr, first);
					}
					outputBuffer = textBuffer.finishCurrentSegment();
					int second = length - first;
					System.Array.Copy(_qbuf, first, outputBuffer, 0, second);
					outPtr = second;
				}
				else
				{
					System.Array.Copy(_qbuf, 0, outputBuffer, outPtr, length);
					outPtr += length;
				}
outer_continue: ;
			}
outer_break: ;
			textBuffer.setCurrentLength(outPtr);
			return textBuffer.contentsAsArray();
		}

		/// <summary>
		/// Will quote given JSON String value using standard quoting, encode
		/// results as UTF-8, and return result as a byte array.
		/// </summary>
		public byte[] quoteAsUTF8(string text)
		{
			com.fasterxml.jackson.core.util.ByteArrayBuilder bb = _bytes;
			if (bb == null)
			{
				// no allocator; can add if we must, shouldn't need to
				_bytes = bb = new com.fasterxml.jackson.core.util.ByteArrayBuilder(null);
			}
			int inputPtr = 0;
			int inputEnd = text.Length;
			int outputPtr = 0;
			byte[] outputBuffer = bb.resetAndGetFirstSegment();
			while (inputPtr < inputEnd)
			{
				int[] escCodes = com.fasterxml.jackson.core.io.CharTypes.get7BitOutputEscapes();
				// ASCII and escapes
				while (true)
				{
					int ch = text[inputPtr];
					if (ch > unchecked((int)(0x7F)) || escCodes[ch] != 0)
					{
						goto inner_loop_break;
					}
					if (outputPtr >= outputBuffer.Length)
					{
						outputBuffer = bb.finishCurrentSegment();
						outputPtr = 0;
					}
					outputBuffer[outputPtr++] = unchecked((byte)ch);
					if (++inputPtr >= inputEnd)
					{
						goto main_break;
					}
inner_loop_continue: ;
				}
inner_loop_break: ;
				if (outputPtr >= outputBuffer.Length)
				{
					outputBuffer = bb.finishCurrentSegment();
					outputPtr = 0;
				}
				// Ok, so what did we hit?
				int ch_1 = (int)text[inputPtr++];
				if (ch_1 <= unchecked((int)(0x7F)))
				{
					// needs quoting
					int escape = escCodes[ch_1];
					// ctrl-char, 6-byte escape...
					outputPtr = _appendByte(ch_1, escape, bb, outputPtr);
					outputBuffer = bb.getCurrentSegment();
					goto main_continue;
				}
				if (ch_1 <= unchecked((int)(0x7FF)))
				{
					// fine, just needs 2 byte output
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (ch_1 >> 6)
						));
					ch_1 = (unchecked((int)(0x80)) | (ch_1 & unchecked((int)(0x3f))));
				}
				else
				{
					// 3 or 4 bytes
					// Surrogates?
					if (ch_1 < SURR1_FIRST || ch_1 > SURR2_LAST)
					{
						// nope
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xe0)) | (ch_1 >> 12
							)));
						if (outputPtr >= outputBuffer.Length)
						{
							outputBuffer = bb.finishCurrentSegment();
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((ch_1 >> 6
							) & unchecked((int)(0x3f)))));
						ch_1 = (unchecked((int)(0x80)) | (ch_1 & unchecked((int)(0x3f))));
					}
					else
					{
						// yes, surrogate pair
						if (ch_1 > SURR1_LAST)
						{
							// must be from first range
							_illegal(ch_1);
						}
						// and if so, followed by another from next range
						if (inputPtr >= inputEnd)
						{
							_illegal(ch_1);
						}
						ch_1 = _convert(ch_1, text[inputPtr++]);
						if (ch_1 > unchecked((int)(0x10FFFF)))
						{
							// illegal, as per RFC 4627
							_illegal(ch_1);
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xf0)) | (ch_1 >> 18
							)));
						if (outputPtr >= outputBuffer.Length)
						{
							outputBuffer = bb.finishCurrentSegment();
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((ch_1 >> 12
							) & unchecked((int)(0x3f)))));
						if (outputPtr >= outputBuffer.Length)
						{
							outputBuffer = bb.finishCurrentSegment();
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((ch_1 >> 6
							) & unchecked((int)(0x3f)))));
						ch_1 = (unchecked((int)(0x80)) | (ch_1 & unchecked((int)(0x3f))));
					}
				}
				if (outputPtr >= outputBuffer.Length)
				{
					outputBuffer = bb.finishCurrentSegment();
					outputPtr = 0;
				}
				outputBuffer[outputPtr++] = unchecked((byte)ch_1);
main_continue: ;
			}
main_break: ;
			return _bytes.completeAndCoalesce(outputPtr);
		}

		/// <summary>
		/// Will encode given String as UTF-8 (without any quoting), return
		/// resulting byte array.
		/// </summary>
		public byte[] encodeAsUTF8(string text)
		{
			com.fasterxml.jackson.core.util.ByteArrayBuilder byteBuilder = _bytes;
			if (byteBuilder == null)
			{
				// no allocator; can add if we must, shouldn't need to
				_bytes = byteBuilder = new com.fasterxml.jackson.core.util.ByteArrayBuilder(null);
			}
			int inputPtr = 0;
			int inputEnd = text.Length;
			int outputPtr = 0;
			byte[] outputBuffer = byteBuilder.resetAndGetFirstSegment();
			int outputEnd = outputBuffer.Length;
			while (inputPtr < inputEnd)
			{
				int c = text[inputPtr++];
				// first tight loop for ascii
				while (c <= unchecked((int)(0x7F)))
				{
					if (outputPtr >= outputEnd)
					{
						outputBuffer = byteBuilder.finishCurrentSegment();
						outputEnd = outputBuffer.Length;
						outputPtr = 0;
					}
					outputBuffer[outputPtr++] = unchecked((byte)c);
					if (inputPtr >= inputEnd)
					{
						goto main_loop_break;
					}
					c = text[inputPtr++];
				}
				// then multi-byte...
				if (outputPtr >= outputEnd)
				{
					outputBuffer = byteBuilder.finishCurrentSegment();
					outputEnd = outputBuffer.Length;
					outputPtr = 0;
				}
				if (c < unchecked((int)(0x800)))
				{
					// 2-byte
					outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xc0)) | (c >> 6)));
				}
				else
				{
					// 3 or 4 bytes
					// Surrogates?
					if (c < SURR1_FIRST || c > SURR2_LAST)
					{
						// nope
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xe0)) | (c >> 12)));
						if (outputPtr >= outputEnd)
						{
							outputBuffer = byteBuilder.finishCurrentSegment();
							outputEnd = outputBuffer.Length;
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) &
							 unchecked((int)(0x3f)))));
					}
					else
					{
						// yes, surrogate pair
						if (c > SURR1_LAST)
						{
							// must be from first range
							_illegal(c);
						}
						// and if so, followed by another from next range
						if (inputPtr >= inputEnd)
						{
							_illegal(c);
						}
						c = _convert(c, text[inputPtr++]);
						if (c > unchecked((int)(0x10FFFF)))
						{
							// illegal, as per RFC 4627
							_illegal(c);
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0xf0)) | (c >> 18)));
						if (outputPtr >= outputEnd)
						{
							outputBuffer = byteBuilder.finishCurrentSegment();
							outputEnd = outputBuffer.Length;
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 12) 
							& unchecked((int)(0x3f)))));
						if (outputPtr >= outputEnd)
						{
							outputBuffer = byteBuilder.finishCurrentSegment();
							outputEnd = outputBuffer.Length;
							outputPtr = 0;
						}
						outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | ((c >> 6) &
							 unchecked((int)(0x3f)))));
					}
				}
				if (outputPtr >= outputEnd)
				{
					outputBuffer = byteBuilder.finishCurrentSegment();
					outputEnd = outputBuffer.Length;
					outputPtr = 0;
				}
				outputBuffer[outputPtr++] = unchecked((byte)(unchecked((int)(0x80)) | (c & unchecked(
					(int)(0x3f)))));
main_loop_continue: ;
			}
main_loop_break: ;
			return _bytes.completeAndCoalesce(outputPtr);
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		private int _appendNumeric(int value, char[] qbuf)
		{
			qbuf[1] = 'u';
			// We know it's a control char, so only the last 2 chars are non-0
			qbuf[4] = HC[value >> 4];
			qbuf[5] = HC[value & unchecked((int)(0xF))];
			return 6;
		}

		private int _appendNamed(int esc, char[] qbuf)
		{
			qbuf[1] = (char)esc;
			return 2;
		}

		private int _appendByte(int ch, int esc, com.fasterxml.jackson.core.util.ByteArrayBuilder
			 bb, int ptr)
		{
			bb.setCurrentSegmentLength(ptr);
			bb.append('\\');
			if (esc < 0)
			{
				// standard escape
				bb.append('u');
				if (ch > unchecked((int)(0xFF)))
				{
					int hi = (ch >> 8);
					bb.append(HB[hi >> 4]);
					bb.append(HB[hi & unchecked((int)(0xF))]);
					ch &= unchecked((int)(0xFF));
				}
				else
				{
					bb.append('0');
					bb.append('0');
				}
				bb.append(HB[ch >> 4]);
				bb.append(HB[ch & unchecked((int)(0xF))]);
			}
			else
			{
				// 2-char simple escape
				bb.append(unchecked((byte)esc));
			}
			return bb.getCurrentSegmentLength();
		}

		private static int _convert(int p1, int p2)
		{
			// Ok, then, is the second part valid?
			if (p2 < SURR2_FIRST || p2 > SURR2_LAST)
			{
				throw new System.ArgumentException("Broken surrogate pair: first char 0x" + Sharpen.Extensions.ToHexString
					(p1) + ", second 0x" + Sharpen.Extensions.ToHexString(p2) + "; illegal combination"
					);
			}
			return unchecked((int)(0x10000)) + ((p1 - SURR1_FIRST) << 10) + (p2 - SURR2_FIRST
				);
		}

		private static void _illegal(int c)
		{
			throw new System.ArgumentException(com.fasterxml.jackson.core.io.UTF8Writer.illegalSurrogateDesc
				(c));
		}
	}
}
