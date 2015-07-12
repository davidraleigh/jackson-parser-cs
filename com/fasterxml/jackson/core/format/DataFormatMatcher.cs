using Sharpen;

namespace com.fasterxml.jackson.core.format
{
	/// <summary>
	/// Result object constructed by
	/// <see cref="DataFormatDetector"/>
	/// when requested
	/// to detect format of given input data.
	/// </summary>
	public class DataFormatMatcher
	{
		protected internal readonly Sharpen.InputStream _originalStream;

		/// <summary>Content read during format matching process</summary>
		protected internal readonly byte[] _bufferedData;

		/// <summary>Pointer to the first byte in buffer available for reading</summary>
		protected internal readonly int _bufferedStart;

		/// <summary>Number of bytes available in buffer.</summary>
		protected internal readonly int _bufferedLength;

		/// <summary>Factory that produced sufficient match (if any)</summary>
		protected internal readonly com.fasterxml.jackson.core.JsonFactory _match;

		/// <summary>
		/// Strength of match with
		/// <see cref="_match"/>
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.format.MatchStrength _matchStrength;

		protected internal DataFormatMatcher(Sharpen.InputStream @in, byte[] buffered, int
			 bufferedStart, int bufferedLength, com.fasterxml.jackson.core.JsonFactory match
			, com.fasterxml.jackson.core.format.MatchStrength strength)
		{
			_originalStream = @in;
			_bufferedData = buffered;
			_bufferedStart = bufferedStart;
			_bufferedLength = bufferedLength;
			_match = match;
			_matchStrength = strength;
		}

		/*
		/**********************************************************
		/* Public API, simple accessors
		/**********************************************************
		*/
		/// <summary>
		/// Accessor to use to see if any formats matched well enough with
		/// the input data.
		/// </summary>
		public virtual bool hasMatch()
		{
			return _match != null;
		}

		/// <summary>
		/// Method for accessing strength of the match, if any; if no match,
		/// will return
		/// <see cref="MatchStrength.INCONCLUSIVE"/>
		/// .
		/// </summary>
		public virtual com.fasterxml.jackson.core.format.MatchStrength getMatchStrength()
		{
			return (_matchStrength == null) ? com.fasterxml.jackson.core.format.MatchStrength
				.INCONCLUSIVE : _matchStrength;
		}

		/// <summary>
		/// Accessor for
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// that represents format that data matched.
		/// </summary>
		public virtual com.fasterxml.jackson.core.JsonFactory getMatch()
		{
			return _match;
		}

		/// <summary>
		/// Accessor for getting brief textual name of matched format if any (null
		/// if none).
		/// </summary>
		/// <remarks>
		/// Accessor for getting brief textual name of matched format if any (null
		/// if none). Equivalent to:
		/// <pre>
		/// return hasMatch() ? getMatch().getFormatName() : null;
		/// </pre>
		/// </remarks>
		public virtual string getMatchedFormatName()
		{
			return _match.getFormatName();
		}

		/*
		/**********************************************************
		/* Public API, factory methods
		/**********************************************************
		*/
		/// <summary>
		/// Convenience method for trying to construct a
		/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
		/// for
		/// parsing content which is assumed to be in detected data format.
		/// If no match was found, returns null.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.JsonParser createParserWithMatch()
		{
			if (_match == null)
			{
				return null;
			}
			if (_originalStream == null)
			{
				return _match.createParser(_bufferedData, _bufferedStart, _bufferedLength);
			}
			return _match.createParser(getDataStream());
		}

		/// <summary>Method to use for accessing input for which format detection has been done.
		/// 	</summary>
		/// <remarks>
		/// Method to use for accessing input for which format detection has been done.
		/// This <b>must</b> be used instead of using stream passed to detector
		/// unless given stream itself can do buffering.
		/// Stream will return all content that was read during matching process, as well
		/// as remaining contents of the underlying stream.
		/// </remarks>
		public virtual Sharpen.InputStream getDataStream()
		{
			if (_originalStream == null)
			{
				return new Sharpen.ByteArrayInputStream(_bufferedData, _bufferedStart, _bufferedLength
					);
			}
			return new com.fasterxml.jackson.core.io.MergedStream(null, _originalStream, _bufferedData
				, _bufferedStart, _bufferedLength);
		}
	}
}
