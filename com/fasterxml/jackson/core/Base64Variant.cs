/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Abstract base class used to define specific details of which
	/// variant of Base64 encoding/decoding is to be used.
	/// </summary>
	/// <remarks>
	/// Abstract base class used to define specific details of which
	/// variant of Base64 encoding/decoding is to be used. Although there is
	/// somewhat standard basic version (so-called "MIME Base64"), other variants
	/// exists, see <a href="http://en.wikipedia.org/wiki/Base64">Base64 Wikipedia entry</a> for details.
	/// </remarks>
	/// <author>Tatu Saloranta</author>
	[System.Serializable]
	public sealed class Base64Variant
	{
		private const int INT_SPACE = unchecked((int)(0x20));

		private const long serialVersionUID = 1L;

		/// <summary>
		/// Placeholder used by "no padding" variant, to be used when a character
		/// value is needed.
		/// </summary>
		internal const char PADDING_CHAR_NONE = '\0';

		/// <summary>
		/// Marker used to denote ascii characters that do not correspond
		/// to a 6-bit value (in this variant), and is not used as a padding
		/// character.
		/// </summary>
		public const int BASE64_VALUE_INVALID = -1;

		/// <summary>
		/// Marker used to denote ascii character (in decoding table) that
		/// is the padding character using this variant (if any).
		/// </summary>
		public const int BASE64_VALUE_PADDING = -2;

		/// <summary>Decoding table used for base 64 decoding.</summary>
		[System.NonSerialized]
		private readonly int[] _asciiToBase64 = new int[128];

		/// <summary>
		/// Encoding table used for base 64 decoding when output is done
		/// as characters.
		/// </summary>
		[System.NonSerialized]
		private readonly char[] _base64ToAsciiC = new char[64];

		/// <summary>
		/// Alternative encoding table used for base 64 decoding when output is done
		/// as ascii bytes.
		/// </summary>
		[System.NonSerialized]
		private readonly byte[] _base64ToAsciiB = new byte[64];

		/// <summary>Symbolic name of variant; used for diagnostics/debugging.</summary>
		/// <remarks>
		/// Symbolic name of variant; used for diagnostics/debugging.
		/// <p>
		/// Note that this is the only non-transient field; used when reading
		/// back from serialized state
		/// </remarks>
		protected internal readonly string _name;

		/// <summary>Whether this variant uses padding or not.</summary>
		[System.NonSerialized]
		protected internal readonly bool _usesPadding;

		/// <summary>
		/// Characted used for padding, if any (
		/// <see cref="PADDING_CHAR_NONE"/>
		/// if not).
		/// </summary>
		[System.NonSerialized]
		protected internal readonly char _paddingChar;

		/// <summary>
		/// Maximum number of encoded base64 characters to output during encoding
		/// before adding a linefeed, if line length is to be limited
		/// (
		/// <see cref="int.MaxValue"/>
		/// if not limited).
		/// <p>
		/// Note: for some output modes (when writing attributes) linefeeds may
		/// need to be avoided, and this value ignored.
		/// </summary>
		[System.NonSerialized]
		protected internal readonly int _maxLineLength;

		public Base64Variant(string name, string base64Alphabet, bool usesPadding, char paddingChar
			, int maxLineLength)
		{
			// We'll only serialize name
			/*
			/**********************************************************
			/* Encoding/decoding tables
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Other configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle
			/**********************************************************
			*/
			_name = name;
			_usesPadding = usesPadding;
			_paddingChar = paddingChar;
			_maxLineLength = maxLineLength;
			// Ok and then we need to create codec tables.
			// First the main encoding table:
			int alphaLen = base64Alphabet.Length;
			if (alphaLen != 64)
			{
				throw new System.ArgumentException("Base64Alphabet length must be exactly 64 (was "
					 + alphaLen + ")");
			}
			// And then secondary encoding table and decoding table:
			Sharpen.Runtime.getCharsForString(base64Alphabet, 0, alphaLen, _base64ToAsciiC, 0
				);
			java.util.Arrays.fill(_asciiToBase64, BASE64_VALUE_INVALID);
			for (int i = 0; i < alphaLen; ++i)
			{
				char alpha = _base64ToAsciiC[i];
				_base64ToAsciiB[i] = unchecked((byte)alpha);
				_asciiToBase64[alpha] = i;
			}
			// Plus if we use padding, add that in too
			if (usesPadding)
			{
				_asciiToBase64[(int)paddingChar] = BASE64_VALUE_PADDING;
			}
		}

		/// <summary>
		/// "Copy constructor" that can be used when the base alphabet is identical
		/// to one used by another variant except for the maximum line length
		/// (and obviously, name).
		/// </summary>
		public Base64Variant(com.fasterxml.jackson.core.Base64Variant @base, string name, 
			int maxLineLength)
			: this(@base, name, @base._usesPadding, @base._paddingChar, maxLineLength)
		{
		}

		/// <summary>
		/// "Copy constructor" that can be used when the base alphabet is identical
		/// to one used by another variant, but other details (padding, maximum
		/// line length) differ
		/// </summary>
		public Base64Variant(com.fasterxml.jackson.core.Base64Variant @base, string name, 
			bool usesPadding, char paddingChar, int maxLineLength)
		{
			_name = name;
			byte[] srcB = @base._base64ToAsciiB;
			System.Array.Copy(srcB, 0, this._base64ToAsciiB, 0, srcB.Length);
			char[] srcC = @base._base64ToAsciiC;
			System.Array.Copy(srcC, 0, this._base64ToAsciiC, 0, srcC.Length);
			int[] srcV = @base._asciiToBase64;
			System.Array.Copy(srcV, 0, this._asciiToBase64, 0, srcV.Length);
			_usesPadding = usesPadding;
			_paddingChar = paddingChar;
			_maxLineLength = maxLineLength;
		}

		/*
		/**********************************************************
		/* Serializable overrides
		/**********************************************************
		*/
		/// <summary>
		/// Method used to "demote" deserialized instances back to
		/// canonical ones
		/// </summary>
		protected internal object readResolve()
		{
			return com.fasterxml.jackson.core.Base64Variants.valueOf(_name);
		}

		/*
		/**********************************************************
		/* Public accessors
		/**********************************************************
		*/
		public string getName()
		{
			return _name;
		}

		public bool usesPadding()
		{
			return _usesPadding;
		}

		public bool usesPaddingChar(char c)
		{
			return c == _paddingChar;
		}

		public bool usesPaddingChar(int ch)
		{
			return ch == (int)_paddingChar;
		}

		public char getPaddingChar()
		{
			return _paddingChar;
		}

		public byte getPaddingByte()
		{
			return unchecked((byte)_paddingChar);
		}

		public int getMaxLineLength()
		{
			return _maxLineLength;
		}

		/*
		/**********************************************************
		/* Decoding support
		/**********************************************************
		*/
		/// <returns>6-bit decoded value, if valid character;</returns>
		public int decodeBase64Char(char c)
		{
			int ch = (int)c;
			return (ch <= 127) ? _asciiToBase64[ch] : BASE64_VALUE_INVALID;
		}

		public int decodeBase64Char(int ch)
		{
			return (ch <= 127) ? _asciiToBase64[ch] : BASE64_VALUE_INVALID;
		}

		public int decodeBase64Byte(byte b)
		{
			int ch = (int)b;
			return (ch <= 127) ? _asciiToBase64[ch] : BASE64_VALUE_INVALID;
		}

		/*
		/**********************************************************
		/* Encoding support
		/**********************************************************
		*/
		public char encodeBase64BitsAsChar(int value)
		{
			/* Let's assume caller has done necessary checks; this
			* method must be fast and inlinable
			*/
			return _base64ToAsciiC[value];
		}

		/// <summary>
		/// Method that encodes given right-aligned (LSB) 24-bit value
		/// into 4 base64 characters, stored in given result buffer.
		/// </summary>
		public int encodeBase64Chunk(int b24, char[] buffer, int ptr)
		{
			buffer[ptr++] = _base64ToAsciiC[(b24 >> 18) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiC[(b24 >> 12) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiC[(b24 >> 6) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiC[b24 & unchecked((int)(0x3F))];
			return ptr;
		}

		public void encodeBase64Chunk(System.Text.StringBuilder sb, int b24)
		{
			sb.Append(_base64ToAsciiC[(b24 >> 18) & unchecked((int)(0x3F))]);
			sb.Append(_base64ToAsciiC[(b24 >> 12) & unchecked((int)(0x3F))]);
			sb.Append(_base64ToAsciiC[(b24 >> 6) & unchecked((int)(0x3F))]);
			sb.Append(_base64ToAsciiC[b24 & unchecked((int)(0x3F))]);
		}

		/// <summary>
		/// Method that outputs partial chunk (which only encodes one
		/// or two bytes of data).
		/// </summary>
		/// <remarks>
		/// Method that outputs partial chunk (which only encodes one
		/// or two bytes of data). Data given is still aligned same as if
		/// it as full data; that is, missing data is at the "right end"
		/// (LSB) of int.
		/// </remarks>
		/// <param name="outputBytes">Number of encoded bytes included (either 1 or 2)</param>
		public int encodeBase64Partial(int bits, int outputBytes, char[] buffer, int outPtr
			)
		{
			buffer[outPtr++] = _base64ToAsciiC[(bits >> 18) & unchecked((int)(0x3F))];
			buffer[outPtr++] = _base64ToAsciiC[(bits >> 12) & unchecked((int)(0x3F))];
			if (_usesPadding)
			{
				buffer[outPtr++] = (outputBytes == 2) ? _base64ToAsciiC[(bits >> 6) & unchecked((
					int)(0x3F))] : _paddingChar;
				buffer[outPtr++] = _paddingChar;
			}
			else
			{
				if (outputBytes == 2)
				{
					buffer[outPtr++] = _base64ToAsciiC[(bits >> 6) & unchecked((int)(0x3F))];
				}
			}
			return outPtr;
		}

		public void encodeBase64Partial(System.Text.StringBuilder sb, int bits, int outputBytes
			)
		{
			sb.Append(_base64ToAsciiC[(bits >> 18) & unchecked((int)(0x3F))]);
			sb.Append(_base64ToAsciiC[(bits >> 12) & unchecked((int)(0x3F))]);
			if (_usesPadding)
			{
				sb.Append((outputBytes == 2) ? _base64ToAsciiC[(bits >> 6) & unchecked((int)(0x3F
					))] : _paddingChar);
				sb.Append(_paddingChar);
			}
			else
			{
				if (outputBytes == 2)
				{
					sb.Append(_base64ToAsciiC[(bits >> 6) & unchecked((int)(0x3F))]);
				}
			}
		}

		public byte encodeBase64BitsAsByte(int value)
		{
			// As with above, assuming it is 6-bit value
			return _base64ToAsciiB[value];
		}

		/// <summary>
		/// Method that encodes given right-aligned (LSB) 24-bit value
		/// into 4 base64 bytes (ascii), stored in given result buffer.
		/// </summary>
		public int encodeBase64Chunk(int b24, byte[] buffer, int ptr)
		{
			buffer[ptr++] = _base64ToAsciiB[(b24 >> 18) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiB[(b24 >> 12) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiB[(b24 >> 6) & unchecked((int)(0x3F))];
			buffer[ptr++] = _base64ToAsciiB[b24 & unchecked((int)(0x3F))];
			return ptr;
		}

		/// <summary>
		/// Method that outputs partial chunk (which only encodes one
		/// or two bytes of data).
		/// </summary>
		/// <remarks>
		/// Method that outputs partial chunk (which only encodes one
		/// or two bytes of data). Data given is still aligned same as if
		/// it as full data; that is, missing data is at the "right end"
		/// (LSB) of int.
		/// </remarks>
		/// <param name="outputBytes">Number of encoded bytes included (either 1 or 2)</param>
		public int encodeBase64Partial(int bits, int outputBytes, byte[] buffer, int outPtr
			)
		{
			buffer[outPtr++] = _base64ToAsciiB[(bits >> 18) & unchecked((int)(0x3F))];
			buffer[outPtr++] = _base64ToAsciiB[(bits >> 12) & unchecked((int)(0x3F))];
			if (_usesPadding)
			{
				byte pb = unchecked((byte)_paddingChar);
				buffer[outPtr++] = (outputBytes == 2) ? _base64ToAsciiB[(bits >> 6) & unchecked((
					int)(0x3F))] : pb;
				buffer[outPtr++] = pb;
			}
			else
			{
				if (outputBytes == 2)
				{
					buffer[outPtr++] = _base64ToAsciiB[(bits >> 6) & unchecked((int)(0x3F))];
				}
			}
			return outPtr;
		}

		/*
		/**********************************************************
		/* Convenience conversion methods for String to/from bytes
		/* use case.
		/**********************************************************
		*/
		/// <summary>
		/// Convenience method for converting given byte array as base64 encoded
		/// String using this variant's settings.
		/// </summary>
		/// <remarks>
		/// Convenience method for converting given byte array as base64 encoded
		/// String using this variant's settings.
		/// Resulting value is "raw", that is, not enclosed in double-quotes.
		/// </remarks>
		/// <param name="input">Byte array to encode</param>
		public string encode(byte[] input)
		{
			return encode(input, false);
		}

		/// <summary>
		/// Convenience method for converting given byte array as base64 encoded String
		/// using this variant's settings,
		/// optionally enclosed in double-quotes.
		/// </summary>
		/// <param name="input">Byte array to encode</param>
		/// <param name="addQuotes">Whether to surround resulting value in double quotes or not
		/// 	</param>
		public string encode(byte[] input, bool addQuotes)
		{
			int inputEnd = input.Length;
			System.Text.StringBuilder sb;
			{
				// let's approximate... 33% overhead, ~= 3/8 (0.375)
				int outputLen = inputEnd + (inputEnd >> 2) + (inputEnd >> 3);
				sb = new System.Text.StringBuilder(outputLen);
			}
			if (addQuotes)
			{
				sb.Append('"');
			}
			int chunksBeforeLF = getMaxLineLength() >> 2;
			// Ok, first we loop through all full triplets of data:
			int inputPtr = 0;
			int safeInputEnd = inputEnd - 3;
			// to get only full triplets
			while (inputPtr <= safeInputEnd)
			{
				// First, mash 3 bytes into lsb of 32-bit int
				int b24 = ((int)input[inputPtr++]) << 8;
				b24 |= ((int)input[inputPtr++]) & unchecked((int)(0xFF));
				b24 = (b24 << 8) | (((int)input[inputPtr++]) & unchecked((int)(0xFF)));
				encodeBase64Chunk(sb, b24);
				if (--chunksBeforeLF <= 0)
				{
					// note: must quote in JSON value, so not really useful...
					sb.Append('\\');
					sb.Append('n');
					chunksBeforeLF = getMaxLineLength() >> 2;
				}
			}
			// And then we may have 1 or 2 leftover bytes to encode
			int inputLeft = inputEnd - inputPtr;
			// 0, 1 or 2
			if (inputLeft > 0)
			{
				// yes, but do we have room for output?
				int b24 = ((int)input[inputPtr++]) << 16;
				if (inputLeft == 2)
				{
					b24 |= (((int)input[inputPtr++]) & unchecked((int)(0xFF))) << 8;
				}
				encodeBase64Partial(sb, b24, inputLeft);
			}
			if (addQuotes)
			{
				sb.Append('"');
			}
			return sb.ToString();
		}

		/// <summary>
		/// Convenience method for decoding contents of a Base64-encoded String,
		/// using this variant's settings.
		/// </summary>
		/// <param name="input"/>
		/// <since>2.2.3</since>
		/// <exception cref="System.ArgumentException">if input is not valid base64 encoded data
		/// 	</exception>
		public byte[] decode(string input)
		{
			com.fasterxml.jackson.core.util.ByteArrayBuilder b = new com.fasterxml.jackson.core.util.ByteArrayBuilder
				();
			decode(input, b);
			return b.toByteArray();
		}

		/// <summary>
		/// Convenience method for decoding contents of a Base64-encoded String,
		/// using this variant's settings
		/// and appending decoded binary data using provided
		/// <see cref="com.fasterxml.jackson.core.util.ByteArrayBuilder"/>
		/// .
		/// <p>
		/// NOTE: builder will NOT be reset before decoding (nor cleared afterwards);
		/// assumption is that caller will ensure it is given in proper state, and
		/// used as appropriate afterwards.
		/// </summary>
		/// <since>2.2.3</since>
		/// <exception cref="System.ArgumentException">if input is not valid base64 encoded data
		/// 	</exception>
		public void decode(string str, com.fasterxml.jackson.core.util.ByteArrayBuilder builder
			)
		{
			int ptr = 0;
			int len = str.Length;
			while (ptr < len)
			{
				// first, we'll skip preceding white space, if any
				char ch;
				do
				{
					ch = str[ptr++];
					if (ptr >= len)
					{
						goto main_loop_break;
					}
				}
				while (ch <= INT_SPACE);
				int bits = decodeBase64Char(ch);
				if (bits < 0)
				{
					_reportInvalidBase64(ch, 0, null);
				}
				int decodedData = bits;
				// then second base64 char; can't get padding yet, nor ws
				if (ptr >= len)
				{
					_reportBase64EOF();
				}
				ch = str[ptr++];
				bits = decodeBase64Char(ch);
				if (bits < 0)
				{
					_reportInvalidBase64(ch, 1, null);
				}
				decodedData = (decodedData << 6) | bits;
				// third base64 char; can be padding, but not ws
				if (ptr >= len)
				{
					// but as per [JACKSON-631] can be end-of-input, iff not using padding
					if (!usesPadding())
					{
						decodedData >>= 4;
						builder.append(decodedData);
						break;
					}
					_reportBase64EOF();
				}
				ch = str[ptr++];
				bits = decodeBase64Char(ch);
				// First branch: can get padding (-> 1 byte)
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						_reportInvalidBase64(ch, 2, null);
					}
					// Ok, must get padding
					if (ptr >= len)
					{
						_reportBase64EOF();
					}
					ch = str[ptr++];
					if (!usesPaddingChar(ch))
					{
						_reportInvalidBase64(ch, 3, "expected padding character '" + getPaddingChar() + "'"
							);
					}
					// Got 12 bits, only need 8, need to shift
					decodedData >>= 4;
					builder.append(decodedData);
					continue;
				}
				// Nope, 2 or 3 bytes
				decodedData = (decodedData << 6) | bits;
				// fourth and last base64 char; can be padding, but not ws
				if (ptr >= len)
				{
					// but as per [JACKSON-631] can be end-of-input, iff not using padding
					if (!usesPadding())
					{
						decodedData >>= 2;
						builder.appendTwoBytes(decodedData);
						break;
					}
					_reportBase64EOF();
				}
				ch = str[ptr++];
				bits = decodeBase64Char(ch);
				if (bits < 0)
				{
					if (bits != com.fasterxml.jackson.core.Base64Variant.BASE64_VALUE_PADDING)
					{
						_reportInvalidBase64(ch, 3, null);
					}
					decodedData >>= 2;
					builder.appendTwoBytes(decodedData);
				}
				else
				{
					// otherwise, our triple is now complete
					decodedData = (decodedData << 6) | bits;
					builder.appendThreeBytes(decodedData);
				}
main_loop_continue: ;
			}
main_loop_break: ;
		}

		/*
		/**********************************************************
		/* Overridden standard methods
		/**********************************************************
		*/
		public override string ToString()
		{
			return _name;
		}

		public override bool Equals(object o)
		{
			// identity comparison should be dine
			return (o == this);
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		/*
		/**********************************************************
		/* Internal helper methods
		/**********************************************************
		*/
		/// <param name="bindex">
		/// Relative index within base64 character unit; between 0
		/// and 3 (as unit has exactly 4 characters)
		/// </param>
		/// <exception cref="System.ArgumentException"/>
		protected internal void _reportInvalidBase64(char ch, int bindex, string msg)
		{
			string @base;
			if (ch <= INT_SPACE)
			{
				@base = "Illegal white space character (code 0x" + Sharpen.Extensions.ToHexString
					(ch) + ") as character #" + (bindex + 1) + " of 4-char base64 unit: can only used between units";
			}
			else
			{
				if (usesPaddingChar(ch))
				{
					@base = "Unexpected padding character ('" + getPaddingChar() + "') as character #"
						 + (bindex + 1) + " of 4-char base64 unit: padding only legal as 3rd or 4th character";
				}
				else
				{
					if (!char.isDefined(ch) || char.isISOControl(ch))
					{
						// Not sure if we can really get here... ? (most illegal xml chars are caught at lower level)
						@base = "Illegal character (code 0x" + Sharpen.Extensions.ToHexString(ch) + ") in base64 content";
					}
					else
					{
						@base = "Illegal character '" + ch + "' (code 0x" + Sharpen.Extensions.ToHexString
							(ch) + ") in base64 content";
					}
				}
			}
			if (msg != null)
			{
				@base = @base + ": " + msg;
			}
			throw new System.ArgumentException(@base);
		}

		/// <exception cref="System.ArgumentException"/>
		protected internal void _reportBase64EOF()
		{
			throw new System.ArgumentException("Unexpected end-of-String in base64 content");
		}
	}
}
