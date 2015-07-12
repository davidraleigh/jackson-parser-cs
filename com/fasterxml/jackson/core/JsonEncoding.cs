/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Enumeration that defines legal encodings that can be used
	/// for JSON content, based on list of allowed encodings from
	/// <a href="http://www.ietf.org/rfc/rfc4627.txt">JSON specification</a>.
	/// </summary>
	/// <remarks>
	/// Enumeration that defines legal encodings that can be used
	/// for JSON content, based on list of allowed encodings from
	/// <a href="http://www.ietf.org/rfc/rfc4627.txt">JSON specification</a>.
	/// <p>
	/// Note: if application want to explicitly disregard Encoding
	/// limitations (to read in JSON encoded using an encoding not
	/// listed as allowed), they can use
	/// <see cref="System.IO.StreamReader"/>
	/// /
	/// <see cref="System.IO.TextWriter"/>
	/// instances as input
	/// </remarks>
	[System.Serializable]
	public sealed class JsonEncoding
	{
		public static readonly com.fasterxml.jackson.core.JsonEncoding UTF8 = new com.fasterxml.jackson.core.JsonEncoding
			("UTF-8", false, 8);

		public static readonly com.fasterxml.jackson.core.JsonEncoding UTF16_BE = new com.fasterxml.jackson.core.JsonEncoding
			("UTF-16BE", true, 16);

		public static readonly com.fasterxml.jackson.core.JsonEncoding UTF16_LE = new com.fasterxml.jackson.core.JsonEncoding
			("UTF-16LE", false, 16);

		public static readonly com.fasterxml.jackson.core.JsonEncoding UTF32_BE = new com.fasterxml.jackson.core.JsonEncoding
			("UTF-32BE", true, 32);

		public static readonly com.fasterxml.jackson.core.JsonEncoding UTF32_LE = new com.fasterxml.jackson.core.JsonEncoding
			("UTF-32LE", false, 32);

		protected internal readonly string _javaName;

		protected internal readonly bool _bigEndian;

		protected internal readonly int _bits;

		internal JsonEncoding(string javaName, bool bigEndian, int bits)
		{
			// N/A for big-endian, really
			com.fasterxml.jackson.core.JsonEncoding._javaName = javaName;
			com.fasterxml.jackson.core.JsonEncoding._bigEndian = bigEndian;
			com.fasterxml.jackson.core.JsonEncoding._bits = bits;
		}

		/// <summary>Method for accessing encoding name that JDK will support.</summary>
		/// <returns>Matching encoding name that JDK will support.</returns>
		public string getJavaName()
		{
			return com.fasterxml.jackson.core.JsonEncoding._javaName;
		}

		/// <summary>
		/// Whether encoding is big-endian (if encoding supports such
		/// notion).
		/// </summary>
		/// <remarks>
		/// Whether encoding is big-endian (if encoding supports such
		/// notion). If no such distinction is made (as is the case for
		/// <see cref="UTF8"/>
		/// ), return value is undefined.
		/// </remarks>
		/// <returns>
		/// True for big-endian encodings; false for little-endian
		/// (or if not applicable)
		/// </returns>
		public bool isBigEndian()
		{
			return com.fasterxml.jackson.core.JsonEncoding._bigEndian;
		}

		public int bits()
		{
			return com.fasterxml.jackson.core.JsonEncoding._bits;
		}
	}
}
