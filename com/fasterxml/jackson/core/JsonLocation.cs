/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Object that encapsulates Location information used for reporting
	/// parsing (or potentially generation) errors, as well as current location
	/// within input streams.
	/// </summary>
	[System.Serializable]
	public class JsonLocation
	{
		private const long serialVersionUID = 1L;

		/// <summary>
		/// Shared immutable "N/A location" that can be returned to indicate
		/// that no location information is available
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonLocation NA = new com.fasterxml.jackson.core.JsonLocation
			("N/A", -1L, -1L, -1, -1);

		internal readonly long _totalBytes;

		internal readonly long _totalChars;

		internal readonly int _lineNr;

		internal readonly int _columnNr;

		/// <summary>Displayable description for input source: file path, URL.</summary>
		/// <remarks>
		/// Displayable description for input source: file path, URL.
		/// <p>
		/// NOTE: <code>transient</code> since 2.2 so that Location itself is Serializable.
		/// </remarks>
		[System.NonSerialized]
		internal readonly object _sourceRef;

		public JsonLocation(object srcRef, long totalChars, int lineNr, int colNr)
			: this(srcRef, -1L, totalChars, lineNr, colNr)
		{
		}

		public JsonLocation(object sourceRef, long totalBytes, long totalChars, int lineNr
			, int columnNr)
		{
			// as per [JACKSON-168]
			/* Unfortunately, none of legal encodings are straight single-byte
			* encodings. Could determine offset for UTF-16/UTF-32, but the
			* most important one is UTF-8...
			* so for now, we'll just not report any real byte count
			*/
			_sourceRef = sourceRef;
			_totalBytes = totalBytes;
			_totalChars = totalChars;
			_lineNr = lineNr;
			_columnNr = columnNr;
		}

		/// <summary>Reference to the original resource being read, if one available.</summary>
		/// <remarks>
		/// Reference to the original resource being read, if one available.
		/// For example, when a parser has been constructed by passing
		/// a
		/// <see cref="Sharpen.FilePath"/>
		/// instance, this method would return
		/// that File. Will return null if no such reference is available,
		/// for example when
		/// <see cref="Sharpen.InputStream"/>
		/// was used to
		/// construct the parser instance.
		/// </remarks>
		public virtual object getSourceRef()
		{
			return _sourceRef;
		}

		/// <returns>Line number of the location (1-based)</returns>
		public virtual int getLineNr()
		{
			return _lineNr;
		}

		/// <returns>Column number of the location (1-based)</returns>
		public virtual int getColumnNr()
		{
			return _columnNr;
		}

		/// <returns>
		/// Character offset within underlying stream, reader or writer,
		/// if available; -1 if not.
		/// </returns>
		public virtual long getCharOffset()
		{
			return _totalChars;
		}

		/// <returns>
		/// Byte offset within underlying stream, reader or writer,
		/// if available; -1 if not.
		/// </returns>
		public virtual long getByteOffset()
		{
			return _totalBytes;
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(80);
			sb.Append("[Source: ");
			if (_sourceRef == null)
			{
				sb.Append("UNKNOWN");
			}
			else
			{
				sb.Append(_sourceRef.ToString());
			}
			sb.Append("; line: ");
			sb.Append(_lineNr);
			sb.Append(", column: ");
			sb.Append(_columnNr);
			sb.Append(']');
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			int hash = (_sourceRef == null) ? 1 : _sourceRef.GetHashCode();
			hash ^= _lineNr;
			hash += _columnNr;
			hash ^= (int)_totalChars;
			hash += (int)_totalBytes;
			return hash;
		}

		public override bool Equals(object other)
		{
			if (other == this)
			{
				return true;
			}
			if (other == null)
			{
				return false;
			}
			if (!(other is com.fasterxml.jackson.core.JsonLocation))
			{
				return false;
			}
			com.fasterxml.jackson.core.JsonLocation otherLoc = (com.fasterxml.jackson.core.JsonLocation
				)other;
			if (_sourceRef == null)
			{
				if (otherLoc._sourceRef != null)
				{
					return false;
				}
			}
			else
			{
				if (!_sourceRef.Equals(otherLoc._sourceRef))
				{
					return false;
				}
			}
			return (_lineNr == otherLoc._lineNr) && (_columnNr == otherLoc._columnNr) && (_totalChars
				 == otherLoc._totalChars) && (getByteOffset() == otherLoc.getByteOffset());
		}
	}
}
