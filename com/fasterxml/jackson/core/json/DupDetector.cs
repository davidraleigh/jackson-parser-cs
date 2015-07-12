using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// Helper class used if
	/// <see cref="com.fasterxml.jackson.core.JsonParser.Feature.STRICT_DUPLICATE_DETECTION
	/// 	"/>
	/// is enabled.
	/// Optimized to try to limit memory usage and processing overhead for smallest
	/// entries, but without adding trashing (immutable objects would achieve optimal
	/// memory usage but lead to significant number of discarded temp objects for
	/// scopes with large number of entries). Another consideration is trying to limit
	/// actual number of compiled classes as it contributes significantly to overall
	/// jar size (due to linkage etc).
	/// </summary>
	/// <since>2.3</since>
	public class DupDetector
	{
		/// <summary>We need to store a back-reference here to parser/generator, unfortunately.
		/// 	</summary>
		protected internal readonly object _source;

		protected internal string _firstName;

		protected internal string _secondName;

		/// <summary>Lazily constructed set of names already seen within this context.</summary>
		protected internal System.Collections.Generic.HashSet<string> _seen;

		private DupDetector(object src)
		{
			_source = src;
		}

		public static com.fasterxml.jackson.core.json.DupDetector rootDetector(com.fasterxml.jackson.core.JsonParser
			 p)
		{
			return new com.fasterxml.jackson.core.json.DupDetector(p);
		}

		public static com.fasterxml.jackson.core.json.DupDetector rootDetector(com.fasterxml.jackson.core.JsonGenerator
			 g)
		{
			return new com.fasterxml.jackson.core.json.DupDetector(g);
		}

		public virtual com.fasterxml.jackson.core.json.DupDetector child()
		{
			return new com.fasterxml.jackson.core.json.DupDetector(_source);
		}

		public virtual void reset()
		{
			_firstName = null;
			_secondName = null;
			_seen = null;
		}

		public virtual com.fasterxml.jackson.core.JsonLocation findLocation()
		{
			// ugly but:
			if (_source is com.fasterxml.jackson.core.JsonParser)
			{
				return ((com.fasterxml.jackson.core.JsonParser)_source).getCurrentLocation();
			}
			// do generators have a way to provide Location? Apparently not...
			return null;
		}

		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public virtual bool isDup(string name)
		{
			if (_firstName == null)
			{
				_firstName = name;
				return false;
			}
			if (name.Equals(_firstName))
			{
				return true;
			}
			if (_secondName == null)
			{
				_secondName = name;
				return false;
			}
			if (name.Equals(_secondName))
			{
				return true;
			}
			if (_seen == null)
			{
				_seen = new System.Collections.Generic.HashSet<string>(16);
				// 16 is default, seems reasonable
				_seen.Add(_firstName);
				_seen.Add(_secondName);
			}
			return !_seen.Add(name);
		}
	}
}
