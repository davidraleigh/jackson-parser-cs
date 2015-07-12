using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	public sealed class NumberInput
	{
		/// <summary>
		/// Textual representation of a double constant that can cause nasty problems
		/// with JDK (see http://www.exploringbinary.com/java-hangs-when-converting-2-2250738585072012e-308).
		/// </summary>
		public const string NASTY_SMALL_DOUBLE = "2.2250738585072012e-308";

		/// <summary>Constants needed for parsing longs from basic int parsing methods</summary>
		internal const long L_BILLION = 1000000000;

		internal static readonly string MIN_LONG_STR_NO_SIGN = Sharpen.Runtime.substring(
			long.MinValue.ToString(), 1);

		internal static readonly string MAX_LONG_STR = long.MaxValue.ToString();

		/// <summary>
		/// Fast method for parsing integers that are known to fit into
		/// regular 32-bit signed int type.
		/// </summary>
		/// <remarks>
		/// Fast method for parsing integers that are known to fit into
		/// regular 32-bit signed int type. This means that length is
		/// between 1 and 9 digits (inclusive)
		/// <p>
		/// Note: public to let unit tests call it
		/// </remarks>
		public static int parseInt(char[] ch, int off, int len)
		{
			int num = ch[off] - '0';
			if (len > 4)
			{
				num = (num * 10) + (ch[++off] - '0');
				num = (num * 10) + (ch[++off] - '0');
				num = (num * 10) + (ch[++off] - '0');
				num = (num * 10) + (ch[++off] - '0');
				len -= 4;
				if (len > 4)
				{
					num = (num * 10) + (ch[++off] - '0');
					num = (num * 10) + (ch[++off] - '0');
					num = (num * 10) + (ch[++off] - '0');
					num = (num * 10) + (ch[++off] - '0');
					return num;
				}
			}
			if (len > 1)
			{
				num = (num * 10) + (ch[++off] - '0');
				if (len > 2)
				{
					num = (num * 10) + (ch[++off] - '0');
					if (len > 3)
					{
						num = (num * 10) + (ch[++off] - '0');
					}
				}
			}
			return num;
		}

		/// <summary>
		/// Helper method to (more) efficiently parse integer numbers from
		/// String values.
		/// </summary>
		public static int parseInt(string s)
		{
			/* Ok: let's keep strategy simple: ignoring optional minus sign,
			* we'll accept 1 - 9 digits and parse things efficiently;
			* otherwise just defer to JDK parse functionality.
			*/
			char c = s[0];
			int len = s.Length;
			bool neg = (c == '-');
			int offset = 1;
			// must have 1 - 9 digits after optional sign:
			// negative?
			if (neg)
			{
				if (len == 1 || len > 10)
				{
					return System.Convert.ToInt32(s);
				}
				c = s[offset++];
			}
			else
			{
				if (len > 9)
				{
					return System.Convert.ToInt32(s);
				}
			}
			if (c > '9' || c < '0')
			{
				return System.Convert.ToInt32(s);
			}
			int num = c - '0';
			if (offset < len)
			{
				c = s[offset++];
				if (c > '9' || c < '0')
				{
					return System.Convert.ToInt32(s);
				}
				num = (num * 10) + (c - '0');
				if (offset < len)
				{
					c = s[offset++];
					if (c > '9' || c < '0')
					{
						return System.Convert.ToInt32(s);
					}
					num = (num * 10) + (c - '0');
					// Let's just loop if we have more than 3 digits:
					if (offset < len)
					{
						do
						{
							c = s[offset++];
							if (c > '9' || c < '0')
							{
								return System.Convert.ToInt32(s);
							}
							num = (num * 10) + (c - '0');
						}
						while (offset < len);
					}
				}
			}
			return neg ? -num : num;
		}

		public static long parseLong(char[] ch, int off, int len)
		{
			// Note: caller must ensure length is [10, 18]
			int len1 = len - 9;
			long val = parseInt(ch, off, len1) * L_BILLION;
			return val + (long)parseInt(ch, off + len1, 9);
		}

		public static long parseLong(string s)
		{
			/* Ok, now; as the very first thing, let's just optimize case of "fake longs";
			* that is, if we know they must be ints, call int parsing
			*/
			int length = s.Length;
			if (length <= 9)
			{
				return (long)parseInt(s);
			}
			// !!! TODO: implement efficient 2-int parsing...
			return System.Convert.ToInt64(s);
		}

		/// <summary>
		/// Helper method for determining if given String representation of
		/// an integral number would fit in 64-bit Java long or not.
		/// </summary>
		/// <remarks>
		/// Helper method for determining if given String representation of
		/// an integral number would fit in 64-bit Java long or not.
		/// Note that input String must NOT contain leading minus sign (even
		/// if 'negative' is set to true).
		/// </remarks>
		/// <param name="negative">
		/// Whether original number had a minus sign (which is
		/// NOT passed to this method) or not
		/// </param>
		public static bool inLongRange(char[] ch, int off, int len, bool negative)
		{
			string cmpStr = negative ? MIN_LONG_STR_NO_SIGN : MAX_LONG_STR;
			int cmpLen = cmpStr.Length;
			if (len < cmpLen)
			{
				return true;
			}
			if (len > cmpLen)
			{
				return false;
			}
			for (int i = 0; i < cmpLen; ++i)
			{
				int diff = ch[off + i] - cmpStr[i];
				if (diff != 0)
				{
					return (diff < 0);
				}
			}
			return true;
		}

		/// <summary>
		/// Similar to
		/// <see cref="inLongRange(char[], int, int, bool)"/>
		/// , but
		/// with String argument
		/// </summary>
		/// <param name="negative">
		/// Whether original number had a minus sign (which is
		/// NOT passed to this method) or not
		/// </param>
		public static bool inLongRange(string s, bool negative)
		{
			string cmp = negative ? MIN_LONG_STR_NO_SIGN : MAX_LONG_STR;
			int cmpLen = cmp.Length;
			int alen = s.Length;
			if (alen < cmpLen)
			{
				return true;
			}
			if (alen > cmpLen)
			{
				return false;
			}
			// could perhaps just use String.compareTo()?
			for (int i = 0; i < cmpLen; ++i)
			{
				int diff = s[i] - cmp[i];
				if (diff != 0)
				{
					return (diff < 0);
				}
			}
			return true;
		}

		public static int parseAsInt(string s, int def)
		{
			if (s == null)
			{
				return def;
			}
			s = Sharpen.Extensions.Trim(s);
			int len = s.Length;
			if (len == 0)
			{
				return def;
			}
			// One more thing: use integer parsing for 'simple'
			int i = 0;
			if (i < len)
			{
				// skip leading sign:
				char c = s[0];
				if (c == '+')
				{
					// for plus, actually physically remove
					s = Sharpen.Runtime.substring(s, 1);
					len = s.Length;
				}
				else
				{
					if (c == '-')
					{
						// minus, just skip for checks, must retain
						++i;
					}
				}
			}
			for (; i < len; ++i)
			{
				char c = s[i];
				// if other symbols, parse as Double, coerce
				if (c > '9' || c < '0')
				{
					try
					{
						return (int)parseDouble(s);
					}
					catch (System.FormatException)
					{
						return def;
					}
				}
			}
			try
			{
				return System.Convert.ToInt32(s);
			}
			catch (System.FormatException)
			{
			}
			return def;
		}

		public static long parseAsLong(string s, long def)
		{
			if (s == null)
			{
				return def;
			}
			s = Sharpen.Extensions.Trim(s);
			int len = s.Length;
			if (len == 0)
			{
				return def;
			}
			// One more thing: use long parsing for 'simple'
			int i = 0;
			if (i < len)
			{
				// skip leading sign:
				char c = s[0];
				if (c == '+')
				{
					// for plus, actually physically remove
					s = Sharpen.Runtime.substring(s, 1);
					len = s.Length;
				}
				else
				{
					if (c == '-')
					{
						// minus, just skip for checks, must retain
						++i;
					}
				}
			}
			for (; i < len; ++i)
			{
				char c = s[i];
				// if other symbols, parse as Double, coerce
				if (c > '9' || c < '0')
				{
					try
					{
						return (long)parseDouble(s);
					}
					catch (System.FormatException)
					{
						return def;
					}
				}
			}
			try
			{
				return System.Convert.ToInt64(s);
			}
			catch (System.FormatException)
			{
			}
			return def;
		}

		public static double parseAsDouble(string s, double def)
		{
			if (s == null)
			{
				return def;
			}
			s = Sharpen.Extensions.Trim(s);
			int len = s.Length;
			if (len == 0)
			{
				return def;
			}
			try
			{
				return parseDouble(s);
			}
			catch (System.FormatException)
			{
			}
			return def;
		}

		/// <exception cref="System.FormatException"/>
		public static double parseDouble(string s)
		{
			// [JACKSON-486]: avoid some nasty float representations... but should it be MIN_NORMAL or MIN_VALUE?
			/* as per [JACKSON-827], let's use MIN_VALUE as it is available on all JDKs; normalized
			* only in JDK 1.6. In practice, should not really matter.
			*/
			if (NASTY_SMALL_DOUBLE.Equals(s))
			{
				return double.MinValue;
			}
			return double.Parse(s);
		}

		/// <exception cref="System.FormatException"/>
		public static java.math.BigDecimal parseBigDecimal(string s)
		{
			try
			{
				return new java.math.BigDecimal(s);
			}
			catch (System.FormatException)
			{
				throw _badBD(s);
			}
		}

		/// <exception cref="System.FormatException"/>
		public static java.math.BigDecimal parseBigDecimal(char[] b)
		{
			return parseBigDecimal(b, 0, b.Length);
		}

		/// <exception cref="System.FormatException"/>
		public static java.math.BigDecimal parseBigDecimal(char[] b, int off, int len)
		{
			try
			{
				return new java.math.BigDecimal(b, off, len);
			}
			catch (System.FormatException)
			{
				throw _badBD(new string(b, off, len));
			}
		}

		private static System.FormatException _badBD(string s)
		{
			return new System.FormatException("Value \"" + s + "\" can not be represented as BigDecimal"
				);
		}
	}
}
