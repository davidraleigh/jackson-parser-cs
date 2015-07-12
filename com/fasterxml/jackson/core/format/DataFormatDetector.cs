using Sharpen;

namespace com.fasterxml.jackson.core.format
{
	/// <summary>
	/// Simple helper class that allows data format (content type) auto-detection,
	/// given an ordered set of
	/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
	/// instances to use for actual low-level
	/// detection.
	/// </summary>
	public class DataFormatDetector
	{
		/// <summary>
		/// By default we will look ahead at most 64 bytes; in most cases,
		/// much less (4 bytes or so) is needed, but we will allow bit more
		/// leniency to support data formats that need more complex heuristics.
		/// </summary>
		public const int DEFAULT_MAX_INPUT_LOOKAHEAD = 64;

		/// <summary>
		/// Ordered list of factories which both represent data formats to
		/// detect (in precedence order, starting with highest) and are used
		/// for actual detection.
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.JsonFactory[] _detectors;

		/// <summary>
		/// Strength of match we consider to be good enough to be used
		/// without checking any other formats.
		/// </summary>
		/// <remarks>
		/// Strength of match we consider to be good enough to be used
		/// without checking any other formats.
		/// Default value is
		/// <see cref="MatchStrength.SOLID_MATCH"/>
		/// ,
		/// </remarks>
		protected internal readonly com.fasterxml.jackson.core.format.MatchStrength _optimalMatch;

		/// <summary>
		/// Strength of minimal match we accept as the answer, unless
		/// better matches are found.
		/// </summary>
		/// <remarks>
		/// Strength of minimal match we accept as the answer, unless
		/// better matches are found.
		/// Default value is
		/// <see cref="MatchStrength.WEAK_MATCH"/>
		/// ,
		/// </remarks>
		protected internal readonly com.fasterxml.jackson.core.format.MatchStrength _minimalMatch;

		/// <summary>
		/// Maximum number of leading bytes of the input that we can read
		/// to determine data format.
		/// </summary>
		/// <remarks>
		/// Maximum number of leading bytes of the input that we can read
		/// to determine data format.
		/// <p>
		/// Default value is
		/// <see cref="DEFAULT_MAX_INPUT_LOOKAHEAD"/>
		/// .
		/// </remarks>
		protected internal readonly int _maxInputLookahead;

		public DataFormatDetector(params com.fasterxml.jackson.core.JsonFactory[] detectors
			)
			: this(detectors, com.fasterxml.jackson.core.format.MatchStrength.SOLID_MATCH, com.fasterxml.jackson.core.format.MatchStrength
				.WEAK_MATCH, DEFAULT_MAX_INPUT_LOOKAHEAD)
		{
		}

		public DataFormatDetector(System.Collections.Generic.ICollection<com.fasterxml.jackson.core.JsonFactory
			> detectors)
			: this(Sharpen.Collections.ToArray(detectors, new com.fasterxml.jackson.core.JsonFactory
				[detectors.Count]))
		{
		}

		/*
		/**********************************************************
		/* Construction
		/**********************************************************
		*/
		/// <summary>
		/// Method that will return a detector instance that uses given
		/// optimal match level (match that is considered sufficient to return, without
		/// trying to find stronger matches with other formats).
		/// </summary>
		public virtual com.fasterxml.jackson.core.format.DataFormatDetector withOptimalMatch
			(com.fasterxml.jackson.core.format.MatchStrength optMatch)
		{
			if (optMatch == _optimalMatch)
			{
				return this;
			}
			return new com.fasterxml.jackson.core.format.DataFormatDetector(_detectors, optMatch
				, _minimalMatch, _maxInputLookahead);
		}

		/// <summary>
		/// Method that will return a detector instance that uses given
		/// minimal match level; match that may be returned unless a stronger match
		/// is found with other format detectors.
		/// </summary>
		public virtual com.fasterxml.jackson.core.format.DataFormatDetector withMinimalMatch
			(com.fasterxml.jackson.core.format.MatchStrength minMatch)
		{
			if (minMatch == _minimalMatch)
			{
				return this;
			}
			return new com.fasterxml.jackson.core.format.DataFormatDetector(_detectors, _optimalMatch
				, minMatch, _maxInputLookahead);
		}

		/// <summary>
		/// Method that will return a detector instance that allows detectors to
		/// read up to specified number of bytes when determining format match strength.
		/// </summary>
		public virtual com.fasterxml.jackson.core.format.DataFormatDetector withMaxInputLookahead
			(int lookaheadBytes)
		{
			if (lookaheadBytes == _maxInputLookahead)
			{
				return this;
			}
			return new com.fasterxml.jackson.core.format.DataFormatDetector(_detectors, _optimalMatch
				, _minimalMatch, lookaheadBytes);
		}

		private DataFormatDetector(com.fasterxml.jackson.core.JsonFactory[] detectors, com.fasterxml.jackson.core.format.MatchStrength
			 optMatch, com.fasterxml.jackson.core.format.MatchStrength minMatch, int maxInputLookahead
			)
		{
			_detectors = detectors;
			_optimalMatch = optMatch;
			_minimalMatch = minMatch;
			_maxInputLookahead = maxInputLookahead;
		}

		/*
		/**********************************************************
		/* Public API
		/**********************************************************
		*/
		/// <summary>
		/// Method to call to find format that content (accessible via given
		/// <see cref="Sharpen.InputStream"/>
		/// ) given has, as per configuration of this detector
		/// instance.
		/// </summary>
		/// <returns>
		/// Matcher object which contains result; never null, even in cases
		/// where no match (with specified minimal match strength) is found.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.format.DataFormatMatcher findFormat(Sharpen.InputStream
			 @in)
		{
			return _findFormat(new com.fasterxml.jackson.core.format.InputAccessor.Std(@in, new 
				byte[_maxInputLookahead]));
		}

		/// <summary>
		/// Method to call to find format that given content (full document)
		/// has, as per configuration of this detector instance.
		/// </summary>
		/// <returns>
		/// Matcher object which contains result; never null, even in cases
		/// where no match (with specified minimal match strength) is found.
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.format.DataFormatMatcher findFormat(byte
			[] fullInputData)
		{
			return _findFormat(new com.fasterxml.jackson.core.format.InputAccessor.Std(fullInputData
				));
		}

		/// <summary>
		/// Method to call to find format that given content (full document)
		/// has, as per configuration of this detector instance.
		/// </summary>
		/// <returns>
		/// Matcher object which contains result; never null, even in cases
		/// where no match (with specified minimal match strength) is found.
		/// </returns>
		/// <since>2.1</since>
		/// <exception cref="System.IO.IOException"/>
		public virtual com.fasterxml.jackson.core.format.DataFormatMatcher findFormat(byte
			[] fullInputData, int offset, int len)
		{
			return _findFormat(new com.fasterxml.jackson.core.format.InputAccessor.Std(fullInputData
				, offset, len));
		}

		/*
		/**********************************************************
		/* Overrides
		/**********************************************************
		*/
		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append('[');
			int len = _detectors.Length;
			if (len > 0)
			{
				sb.Append(_detectors[0].getFormatName());
				for (int i = 1; i < len; ++i)
				{
					sb.Append(", ");
					sb.Append(_detectors[i].getFormatName());
				}
			}
			sb.Append(']');
			return sb.ToString();
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		private com.fasterxml.jackson.core.format.DataFormatMatcher _findFormat(com.fasterxml.jackson.core.format.InputAccessor.Std
			 acc)
		{
			com.fasterxml.jackson.core.JsonFactory bestMatch = null;
			com.fasterxml.jackson.core.format.MatchStrength bestMatchStrength = null;
			foreach (com.fasterxml.jackson.core.JsonFactory f in _detectors)
			{
				acc.reset();
				com.fasterxml.jackson.core.format.MatchStrength strength = f.hasFormat(acc);
				// if not better than what we have so far (including minimal level limit), skip
				if (strength == null || (int)(strength) < (int)(_minimalMatch))
				{
					continue;
				}
				// also, needs to better match than before
				if (bestMatch != null)
				{
					if ((int)(bestMatchStrength) >= (int)(strength))
					{
						continue;
					}
				}
				// finally: if it's good enough match, we are done
				bestMatch = f;
				bestMatchStrength = strength;
				if ((int)(strength) >= (int)(_optimalMatch))
				{
					break;
				}
			}
			return acc.createMatcher(bestMatch, bestMatchStrength);
		}
	}
}
