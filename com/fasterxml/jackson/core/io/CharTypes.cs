using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	public sealed class CharTypes
	{
		private static readonly char[] HC = "0123456789ABCDEF".ToCharArray();

		private static readonly byte[] HB;

		static CharTypes()
		{
			int len = HC.Length;
			HB = new byte[len];
			for (int i = 0; i < len; ++i)
			{
				HB[i] = unchecked((byte)HC[i]);
			}
		}

		/// <summary>
		/// Lookup table used for determining which input characters
		/// need special handling when contained in text segment.
		/// </summary>
		internal static readonly int[] sInputCodes;

		static CharTypes()
		{
			/* 96 would do for most cases (backslash is ASCII 94)
			* but if we want to do lookups by raw bytes it's better
			* to have full table
			*/
			int[] table = new int[256];
			// Control chars and non-space white space are not allowed unquoted
			for (int i = 0; i < 32; ++i)
			{
				table[i] = -1;
			}
			// And then string end and quote markers are special too
			table['"'] = 1;
			table['\\'] = 1;
			sInputCodes = table;
		}

		/// <summary>
		/// Additionally we can combine UTF-8 decoding info into similar
		/// data table.
		/// </summary>
		internal static readonly int[] sInputCodesUTF8;

		static CharTypes()
		{
			int[] table = new int[sInputCodes.Length];
			System.Array.Copy(sInputCodes, 0, table, 0, table.Length);
			for (int c = 128; c < 256; ++c)
			{
				int code;
				// We'll add number of bytes needed for decoding
				if ((c & unchecked((int)(0xE0))) == unchecked((int)(0xC0)))
				{
					// 2 bytes (0x0080 - 0x07FF)
					code = 2;
				}
				else
				{
					if ((c & unchecked((int)(0xF0))) == unchecked((int)(0xE0)))
					{
						// 3 bytes (0x0800 - 0xFFFF)
						code = 3;
					}
					else
					{
						if ((c & unchecked((int)(0xF8))) == unchecked((int)(0xF0)))
						{
							// 4 bytes; double-char with surrogates and all...
							code = 4;
						}
						else
						{
							// And -1 seems like a good "universal" error marker...
							code = -1;
						}
					}
				}
				table[c] = code;
			}
			sInputCodesUTF8 = table;
		}

		/// <summary>
		/// To support non-default (and -standard) unquoted field names mode,
		/// need to have alternate checking.
		/// </summary>
		/// <remarks>
		/// To support non-default (and -standard) unquoted field names mode,
		/// need to have alternate checking.
		/// Basically this is list of 8-bit ASCII characters that are legal
		/// as part of Javascript identifier
		/// </remarks>
		internal static readonly int[] sInputCodesJsNames;

		static CharTypes()
		{
			int[] table = new int[256];
			// Default is "not a name char", mark ones that are
			java.util.Arrays.fill(table, -1);
			// Assume rules with JS same as Java (change if/as needed)
			for (int i = 33; i < 256; ++i)
			{
				if (char.isJavaIdentifierPart((char)i))
				{
					table[i] = 0;
				}
			}
			/* As per [JACKSON-267], '@', '#' and '*' are also to be accepted as well.
			* And '-' (for hyphenated names); and '+' for sake of symmetricity...
			*/
			table['@'] = 0;
			table['#'] = 0;
			table['*'] = 0;
			table['-'] = 0;
			table['+'] = 0;
			sInputCodesJsNames = table;
		}

		/// <summary>
		/// This table is similar to Latin-1, except that it marks all "high-bit"
		/// code as ok.
		/// </summary>
		/// <remarks>
		/// This table is similar to Latin-1, except that it marks all "high-bit"
		/// code as ok. They will be validated at a later point, when decoding
		/// name
		/// </remarks>
		internal static readonly int[] sInputCodesUtf8JsNames;

		static CharTypes()
		{
			int[] table = new int[256];
			// start with 8-bit JS names
			System.Array.Copy(sInputCodesJsNames, 0, table, 0, table.Length);
			java.util.Arrays.fill(table, 128, 128, 0);
			sInputCodesUtf8JsNames = table;
		}

		/// <summary>
		/// Decoding table used to quickly determine characters that are
		/// relevant within comment content.
		/// </summary>
		internal static readonly int[] sInputCodesComment;

		static CharTypes()
		{
			int[] buf = new int[256];
			// but first: let's start with UTF-8 multi-byte markers:
			System.Array.Copy(sInputCodesUTF8, 128, buf, 128, 128);
			// default (0) means "ok" (skip); -1 invalid, others marked by char itself
			java.util.Arrays.fill(buf, 0, 32, -1);
			// invalid white space
			buf['\t'] = 0;
			// tab is still fine
			buf['\n'] = '\n';
			// lf/cr need to be observed, ends cpp comment
			buf['\r'] = '\r';
			buf['*'] = '*';
			// end marker for c-style comments
			sInputCodesComment = buf;
		}

		/// <summary>Decoding table used for skipping white space and comments.</summary>
		/// <since>2.3</since>
		internal static readonly int[] sInputCodesWS;

		static CharTypes()
		{
			// but first: let's start with UTF-8 multi-byte markers:
			int[] buf = new int[256];
			System.Array.Copy(sInputCodesUTF8, 128, buf, 128, 128);
			// default (0) means "not whitespace" (end); 1 "whitespace", -1 invalid,
			// 2-4 UTF-8 multi-bytes, others marked by char itself
			//
			java.util.Arrays.fill(buf, 0, 32, -1);
			// invalid white space
			buf[' '] = 1;
			buf['\t'] = 1;
			buf['\n'] = '\n';
			// lf/cr need to be observed, ends cpp comment
			buf['\r'] = '\r';
			buf['/'] = '/';
			// start marker for c/cpp comments
			buf['#'] = '#';
			// start marker for YAML comments
			sInputCodesWS = buf;
		}

		/// <summary>
		/// Lookup table used for determining which output characters in
		/// 7-bit ASCII range need to be quoted.
		/// </summary>
		internal static readonly int[] sOutputEscapes128;

		static CharTypes()
		{
			int[] table = new int[128];
			// Control chars need generic escape sequence
			for (int i = 0; i < 32; ++i)
			{
				// 04-Mar-2011, tatu: Used to use "-(i + 1)", replaced with constant
				table[i] = com.fasterxml.jackson.core.io.CharacterEscapes.ESCAPE_STANDARD;
			}
			/* Others (and some within that range too) have explicit shorter
			* sequences
			*/
			table['"'] = '"';
			table['\\'] = '\\';
			// Escaping of slash is optional, so let's not add it
			table[unchecked((int)(0x08))] = 'b';
			table[unchecked((int)(0x09))] = 't';
			table[unchecked((int)(0x0C))] = 'f';
			table[unchecked((int)(0x0A))] = 'n';
			table[unchecked((int)(0x0D))] = 'r';
			sOutputEscapes128 = table;
		}

		/// <summary>
		/// Lookup table for the first 128 Unicode characters (7-bit ASCII)
		/// range.
		/// </summary>
		/// <remarks>
		/// Lookup table for the first 128 Unicode characters (7-bit ASCII)
		/// range. For actual hex digits, contains corresponding value;
		/// for others -1.
		/// </remarks>
		internal static readonly int[] sHexValues = new int[128];

		static CharTypes()
		{
			java.util.Arrays.fill(sHexValues, -1);
			for (int i = 0; i < 10; ++i)
			{
				sHexValues['0' + i] = i;
			}
			for (int i_1 = 0; i_1 < 6; ++i_1)
			{
				sHexValues['a' + i_1] = 10 + i_1;
				sHexValues['A' + i_1] = 10 + i_1;
			}
		}

		public static int[] getInputCodeLatin1()
		{
			return sInputCodes;
		}

		public static int[] getInputCodeUtf8()
		{
			return sInputCodesUTF8;
		}

		public static int[] getInputCodeLatin1JsNames()
		{
			return sInputCodesJsNames;
		}

		public static int[] getInputCodeUtf8JsNames()
		{
			return sInputCodesUtf8JsNames;
		}

		public static int[] getInputCodeComment()
		{
			return sInputCodesComment;
		}

		public static int[] getInputCodeWS()
		{
			return sInputCodesWS;
		}

		/// <summary>
		/// Accessor for getting a read-only encoding table for first 128 Unicode
		/// code points (single-byte UTF-8 characters).
		/// </summary>
		/// <remarks>
		/// Accessor for getting a read-only encoding table for first 128 Unicode
		/// code points (single-byte UTF-8 characters).
		/// Value of 0 means "no escaping"; other positive values that value is character
		/// to use after backslash; and negative values that generic (backslash - u)
		/// escaping is to be used.
		/// </remarks>
		public static int[] get7BitOutputEscapes()
		{
			return sOutputEscapes128;
		}

		public static int charToHex(int ch)
		{
			return (ch > 127) ? -1 : sHexValues[ch];
		}

		public static void appendQuoted(System.Text.StringBuilder sb, string content)
		{
			int[] escCodes = sOutputEscapes128;
			int escLen = escCodes.Length;
			for (int i = 0; i < len; ++i)
			{
				char c = content[i];
				if (c >= escLen || escCodes[c] == 0)
				{
					sb.Append(c);
					continue;
				}
				sb.Append('\\');
				int escCode = escCodes[c];
				if (escCode < 0)
				{
					// generic quoting (hex value)
					// The only negative value sOutputEscapes128 returns
					// is CharacterEscapes.ESCAPE_STANDARD, which mean
					// appendQuotes should encode using the Unicode encoding;
					// not sure if this is the right way to encode for
					// CharacterEscapes.ESCAPE_CUSTOM or other (future)
					// CharacterEscapes.ESCAPE_XXX values.
					// We know that it has to fit in just 2 hex chars
					sb.Append('u');
					sb.Append('0');
					sb.Append('0');
					int value = c;
					// widening
					sb.Append(HC[value >> 4]);
					sb.Append(HC[value & unchecked((int)(0xF))]);
				}
				else
				{
					// "named", i.e. prepend with slash
					sb.Append((char)escCode);
				}
			}
		}

		public static char[] copyHexChars()
		{
			return (char[])HC.MemberwiseClone();
		}

		public static byte[] copyHexBytes()
		{
			return (byte[])HB.MemberwiseClone();
		}
	}
}
