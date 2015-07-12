/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Container for commonly used Base64 variants:
	/// <ul>
	/// <li>
	/// <see cref="MIME"/>
	/// <li>
	/// <see cref="MIME_NO_LINEFEEDS"/>
	/// <li>
	/// <see cref="PEM"/>
	/// <li>
	/// <see cref="MODIFIED_FOR_URL"/>
	/// </ul>
	/// </summary>
	/// <author>Tatu Saloranta</author>
	public sealed class Base64Variants
	{
		internal const string STD_BASE64_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

		/// <summary>
		/// This variant is what most people would think of "the standard"
		/// Base64 encoding.
		/// </summary>
		/// <remarks>
		/// This variant is what most people would think of "the standard"
		/// Base64 encoding.
		/// <p>
		/// See <a href="http://en.wikipedia.org/wiki/Base64">wikipedia Base64 entry</a> for details.
		/// <p>
		/// Note that although this can be thought of as the standard variant,
		/// it is <b>not</b> the default for Jackson: no-linefeeds alternative
		/// is because of JSON requirement of escaping all linefeeds.
		/// </remarks>
		public static readonly com.fasterxml.jackson.core.Base64Variant MIME;

		static Base64Variants()
		{
			MIME = new com.fasterxml.jackson.core.Base64Variant("MIME", STD_BASE64_ALPHABET, 
				true, '=', 76);
		}

		/// <summary>
		/// Slightly non-standard modification of
		/// <see cref="MIME"/>
		/// which does not
		/// use linefeeds (max line length set to infinite). Useful when linefeeds
		/// wouldn't work well (possibly in attributes), or for minor space savings
		/// (save 1 linefeed per 76 data chars, ie. ~1.4% savings).
		/// </summary>
		public static readonly com.fasterxml.jackson.core.Base64Variant MIME_NO_LINEFEEDS;

		static Base64Variants()
		{
			MIME_NO_LINEFEEDS = new com.fasterxml.jackson.core.Base64Variant(MIME, "MIME-NO-LINEFEEDS"
				, int.MaxValue);
		}

		/// <summary>
		/// This variant is the one that predates
		/// <see cref="MIME"/>
		/// : it is otherwise
		/// identical, except that it mandates shorter line length.
		/// </summary>
		public static readonly com.fasterxml.jackson.core.Base64Variant PEM = new com.fasterxml.jackson.core.Base64Variant
			(MIME, "PEM", true, '=', 64);

		/// <summary>
		/// This non-standard variant is usually used when encoded data needs to be
		/// passed via URLs (such as part of GET request).
		/// </summary>
		/// <remarks>
		/// This non-standard variant is usually used when encoded data needs to be
		/// passed via URLs (such as part of GET request). It differs from the
		/// base
		/// <see cref="MIME"/>
		/// variant in multiple ways.
		/// First, no padding is used: this also means that it generally can not
		/// be written in multiple separate but adjacent chunks (which would not
		/// be the usual use case in any case). Also, no linefeeds are used (max
		/// line length set to infinite). And finally, two characters (plus and
		/// slash) that would need quoting in URLs are replaced with more
		/// optimal alternatives (hyphen and underscore, respectively).
		/// </remarks>
		public static readonly com.fasterxml.jackson.core.Base64Variant MODIFIED_FOR_URL;

		static Base64Variants()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(STD_BASE64_ALPHABET);
			// Replace plus with hyphen, slash with underscore (and no padding)
			Sharpen.Runtime.setCharAt(sb, sb.indexOf("+"), '-');
			Sharpen.Runtime.setCharAt(sb, sb.indexOf("/"), '_');
			/* And finally, let's not split lines either, wouldn't work too
			* well with URLs
			*/
			MODIFIED_FOR_URL = new com.fasterxml.jackson.core.Base64Variant("MODIFIED-FOR-URL"
				, sb.ToString(), false, com.fasterxml.jackson.core.Base64Variant.PADDING_CHAR_NONE
				, int.MaxValue);
		}

		/// <summary>
		/// Method used to get the default variant ("MIME_NO_LINEFEEDS") for cases
		/// where caller does not explicitly specify the variant.
		/// </summary>
		/// <remarks>
		/// Method used to get the default variant ("MIME_NO_LINEFEEDS") for cases
		/// where caller does not explicitly specify the variant.
		/// We will prefer no-linefeed version because linefeeds in JSON values
		/// must be escaped, making linefeed-containing variants sub-optimal.
		/// </remarks>
		public static com.fasterxml.jackson.core.Base64Variant getDefaultVariant()
		{
			return MIME_NO_LINEFEEDS;
		}

		/// <since>2.1</since>
		/// <exception cref="System.ArgumentException"/>
		public static com.fasterxml.jackson.core.Base64Variant valueOf(string name)
		{
			if (MIME._name.Equals(name))
			{
				return MIME;
			}
			if (MIME_NO_LINEFEEDS._name.Equals(name))
			{
				return MIME_NO_LINEFEEDS;
			}
			if (PEM._name.Equals(name))
			{
				return PEM;
			}
			if (MODIFIED_FOR_URL._name.Equals(name))
			{
				return MODIFIED_FOR_URL;
			}
			if (name == null)
			{
				name = "<null>";
			}
			else
			{
				name = "'" + name + "'";
			}
			throw new System.ArgumentException("No Base64Variant with name " + name);
		}
	}
}
