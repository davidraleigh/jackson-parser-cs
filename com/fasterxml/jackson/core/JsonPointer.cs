using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Implementation of
	/// <a href="http://tools.ietf.org/html/draft-ietf-appsawg-json-pointer-03">JSON Pointer</a>
	/// specification.
	/// </summary>
	/// <remarks>
	/// Implementation of
	/// <a href="http://tools.ietf.org/html/draft-ietf-appsawg-json-pointer-03">JSON Pointer</a>
	/// specification.
	/// Pointer instances can be used to locate logical JSON nodes for things like
	/// tree traversal (see
	/// <see cref="TreeNode.at(JsonPointer)"/>
	/// ).
	/// It may be used in future for filtering of streaming JSON content
	/// as well (not implemented yet for 2.3).
	/// <p>
	/// Instances are fully immutable and can be shared, cached.
	/// </remarks>
	/// <author>Tatu Saloranta</author>
	/// <since>2.3</since>
	public class JsonPointer
	{
		/// <summary>
		/// Marker instance used to represent segment that matches current
		/// node or position (that is, returns true for
		/// <see cref="matches()"/>
		/// ).
		/// </summary>
		protected internal static readonly com.fasterxml.jackson.core.JsonPointer EMPTY = 
			new com.fasterxml.jackson.core.JsonPointer();

		/// <summary>
		/// Reference to rest of the pointer beyond currently matching
		/// segment (if any); null if this pointer refers to the matching
		/// segment.
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.JsonPointer _nextSegment;

		/// <summary>
		/// Reference from currently matching segment (if any) to node
		/// before leaf.
		/// </summary>
		/// <remarks>
		/// Reference from currently matching segment (if any) to node
		/// before leaf.
		/// Lazily constructed if/as needed.
		/// <p>
		/// NOTE: we'll use `volatile` here assuming that this is unlikely to
		/// become a performance bottleneck. If it becomes one we can probably
		/// just drop it and things still should work (despite warnings as per JMM
		/// regarding visibility (and lack thereof) of unguarded changes).
		/// </remarks>
		/// <since>2.5</since>
		protected internal volatile com.fasterxml.jackson.core.JsonPointer _head;

		/// <summary>
		/// We will retain representation of the pointer, as a String,
		/// so that
		/// <see cref="ToString()"/>
		/// should be as efficient as possible.
		/// </summary>
		protected internal readonly string _asString;

		protected internal readonly string _matchingPropertyName;

		protected internal readonly int _matchingElementIndex;

		/// <summary>
		/// Constructor used for creating "empty" instance, used to represent
		/// state that matches current node.
		/// </summary>
		protected internal JsonPointer()
		{
			/*
			/**********************************************************
			/* Cosntruction
			/**********************************************************
			*/
			_nextSegment = null;
			_matchingPropertyName = string.Empty;
			_matchingElementIndex = -1;
			_asString = string.Empty;
		}

		/// <summary>Constructor used for creating non-empty Segments</summary>
		protected internal JsonPointer(string fullString, string segment, com.fasterxml.jackson.core.JsonPointer
			 next)
		{
			_asString = fullString;
			_nextSegment = next;
			// Ok; may always be a property
			_matchingPropertyName = segment;
			// but could be an index, if parsable
			_matchingElementIndex = _parseIndex(segment);
		}

		/// <since>2.5</since>
		protected internal JsonPointer(string fullString, string segment, int matchIndex, 
			com.fasterxml.jackson.core.JsonPointer next)
		{
			_asString = fullString;
			_nextSegment = next;
			_matchingPropertyName = segment;
			_matchingElementIndex = matchIndex;
		}

		/*
		/**********************************************************
		/* Factory methods
		/**********************************************************
		*/
		/// <summary>
		/// Factory method that parses given input and construct matching pointer
		/// instance, if it represents a valid JSON Pointer: if not, a
		/// <see cref="System.ArgumentException"/>
		/// is thrown.
		/// </summary>
		/// <exception cref="System.ArgumentException">
		/// Thrown if the input does not present a valid JSON Pointer
		/// expression: currently the only such expression is one that does NOT start with
		/// a slash ('/').
		/// </exception>
		public static com.fasterxml.jackson.core.JsonPointer compile(string input)
		{
			// First quick checks for well-known 'empty' pointer
			if ((input == null) || input.Length == 0)
			{
				return EMPTY;
			}
			// And then quick validity check:
			if (input[0] != '/')
			{
				throw new System.ArgumentException("Invalid input: JSON Pointer expression must start with '/': "
					 + "\"" + input + "\"");
			}
			return _parseTail(input);
		}

		/// <summary>
		/// Alias for
		/// <see cref="compile(string)"/>
		/// ; added to make instances automatically
		/// deserializable by Jackson databind.
		/// </summary>
		public static com.fasterxml.jackson.core.JsonPointer valueOf(string input)
		{
			return compile(input);
		}

		/* Factory method that composes a pointer instance, given a set
		* of 'raw' segments: raw meaning that no processing will be done,
		* no escaping may is present.
		*
		* @param segments
		*
		* @return Constructed path instance
		*/
		/* TODO!
		public static JsonPointer fromSegment(String... segments)
		{
		if (segments.length == 0) {
		return EMPTY;
		}
		JsonPointer prev = null;
		
		for (String segment : segments) {
		JsonPointer next = new JsonPointer()
		}
		}
		*/
		/*
		/**********************************************************
		/* Public API
		/**********************************************************
		*/
		public virtual bool matches()
		{
			return _nextSegment == null;
		}

		public virtual string getMatchingProperty()
		{
			return _matchingPropertyName;
		}

		public virtual int getMatchingIndex()
		{
			return _matchingElementIndex;
		}

		public virtual bool mayMatchProperty()
		{
			return _matchingPropertyName != null;
		}

		public virtual bool mayMatchElement()
		{
			return _matchingElementIndex >= 0;
		}

		/// <summary>Returns the leaf of current JSON Pointer expression.</summary>
		/// <remarks>
		/// Returns the leaf of current JSON Pointer expression.
		/// Leaf is the last non-null segment of current JSON Pointer.
		/// </remarks>
		/// <since>2.5</since>
		public virtual com.fasterxml.jackson.core.JsonPointer last()
		{
			com.fasterxml.jackson.core.JsonPointer current = this;
			if (current == EMPTY)
			{
				return null;
			}
			com.fasterxml.jackson.core.JsonPointer next;
			while ((next = current._nextSegment) != com.fasterxml.jackson.core.JsonPointer.EMPTY
				)
			{
				current = next;
			}
			return current;
		}

		public virtual com.fasterxml.jackson.core.JsonPointer append(com.fasterxml.jackson.core.JsonPointer
			 tail)
		{
			if (this == EMPTY)
			{
				return tail;
			}
			if (tail == EMPTY)
			{
				return this;
			}
			string currentJsonPointer = _asString;
			if (currentJsonPointer.EndsWith("/"))
			{
				//removes final slash
				currentJsonPointer = Sharpen.Runtime.substring(currentJsonPointer, 0, currentJsonPointer
					.Length - 1);
			}
			return compile(currentJsonPointer + tail._asString);
		}

		/// <summary>
		/// Method that may be called to see if the pointer would match property
		/// (of a JSON Object) with given name.
		/// </summary>
		/// <since>2.5</since>
		public virtual bool matchesProperty(string name)
		{
			return (_nextSegment != null) && _matchingPropertyName.Equals(name);
		}

		public virtual com.fasterxml.jackson.core.JsonPointer matchProperty(string name)
		{
			if ((_nextSegment != null) && _matchingPropertyName.Equals(name))
			{
				return _nextSegment;
			}
			return null;
		}

		/// <summary>
		/// Method that may be called to see if the pointer would match
		/// array element (of a JSON Array) with given index.
		/// </summary>
		/// <since>2.5</since>
		public virtual bool matchesElement(int index)
		{
			return (index == _matchingElementIndex) && (index >= 0);
		}

		/// <since>2.6</since>
		public virtual com.fasterxml.jackson.core.JsonPointer matchElement(int index)
		{
			if ((index != _matchingElementIndex) || (index < 0))
			{
				return null;
			}
			return _nextSegment;
		}

		/// <summary>
		/// Accessor for getting a "sub-pointer", instance where current segment
		/// has been removed and pointer includes rest of segments.
		/// </summary>
		/// <remarks>
		/// Accessor for getting a "sub-pointer", instance where current segment
		/// has been removed and pointer includes rest of segments.
		/// For matching state, will return null.
		/// </remarks>
		public virtual com.fasterxml.jackson.core.JsonPointer tail()
		{
			return _nextSegment;
		}

		/// <summary>
		/// Accessor for getting a pointer instance that is identical to this
		/// instance except that the last segment has been dropped.
		/// </summary>
		/// <remarks>
		/// Accessor for getting a pointer instance that is identical to this
		/// instance except that the last segment has been dropped.
		/// For example, for JSON Point "/root/branch/leaf", this method would
		/// return pointer "/root/branch" (compared to
		/// <see cref="tail()"/>
		/// that
		/// would return "/branch/leaf").
		/// For leaf
		/// </remarks>
		/// <since>2.5</since>
		public virtual com.fasterxml.jackson.core.JsonPointer head()
		{
			com.fasterxml.jackson.core.JsonPointer h = _head;
			if (h == null)
			{
				if (this != EMPTY)
				{
					h = _constructHead();
				}
				_head = h;
			}
			return h;
		}

		/*
		/**********************************************************
		/* Standard method overrides
		/**********************************************************
		*/
		public override string ToString()
		{
			return _asString;
		}

		public override int GetHashCode()
		{
			return _asString.GetHashCode();
		}

		public override bool Equals(object o)
		{
			if (o == this)
			{
				return true;
			}
			if (o == null)
			{
				return false;
			}
			if (!(o is com.fasterxml.jackson.core.JsonPointer))
			{
				return false;
			}
			return _asString.Equals(((com.fasterxml.jackson.core.JsonPointer)o)._asString);
		}

		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		private static int _parseIndex(string str)
		{
			int len = str.Length;
			// [core#133]: beware of super long indexes; assume we never
			// have arrays over 2 billion entries so ints are fine.
			if (len == 0 || len > 10)
			{
				return -1;
			}
			// [core#176]: no leading zeroes allowed
			char c = str[0];
			if (c <= '0')
			{
				return (len == 1 && c == '0') ? 0 : -1;
			}
			if (c > '9')
			{
				return -1;
			}
			for (int i = 1; i < len; ++i)
			{
				c = str[i];
				if (c > '9' || c < '0')
				{
					return -1;
				}
			}
			if (len == 10)
			{
				long l = com.fasterxml.jackson.core.io.NumberInput.parseLong(str);
				if (l > int.MaxValue)
				{
					return -1;
				}
			}
			return com.fasterxml.jackson.core.io.NumberInput.parseInt(str);
		}

		protected internal static com.fasterxml.jackson.core.JsonPointer _parseTail(string
			 input)
		{
			int end = input.Length;
			// first char is the contextual slash, skip
			for (int i = 1; i < end; )
			{
				char c = input[i];
				if (c == '/')
				{
					// common case, got a segment
					return new com.fasterxml.jackson.core.JsonPointer(input, Sharpen.Runtime.substring
						(input, 1, i), _parseTail(Sharpen.Runtime.substring(input, i)));
				}
				++i;
				// quoting is different; offline this case
				if (c == '~' && i < end)
				{
					// possibly, quote
					return _parseQuotedTail(input, i);
				}
			}
			// otherwise, loop on
			// end of the road, no escapes
			return new com.fasterxml.jackson.core.JsonPointer(input, Sharpen.Runtime.substring
				(input, 1), EMPTY);
		}

		/// <summary>
		/// Method called to parse tail of pointer path, when a potentially
		/// escaped character has been seen.
		/// </summary>
		/// <param name="input">Full input for the tail being parsed</param>
		/// <param name="i">Offset to character after tilde</param>
		protected internal static com.fasterxml.jackson.core.JsonPointer _parseQuotedTail
			(string input, int i)
		{
			int end = input.Length;
			System.Text.StringBuilder sb = new System.Text.StringBuilder(System.Math.max(16, 
				end));
			if (i > 2)
			{
				sb.AppendRange(input, 1, i - 1);
			}
			_appendEscape(sb, input[i++]);
			while (i < end)
			{
				char c = input[i];
				if (c == '/')
				{
					// end is nigh!
					return new com.fasterxml.jackson.core.JsonPointer(input, sb.ToString(), _parseTail
						(Sharpen.Runtime.substring(input, i)));
				}
				++i;
				if (c == '~' && i < end)
				{
					_appendEscape(sb, input[i++]);
					continue;
				}
				sb.Append(c);
			}
			// end of the road, last segment
			return new com.fasterxml.jackson.core.JsonPointer(input, sb.ToString(), EMPTY);
		}

		protected internal virtual com.fasterxml.jackson.core.JsonPointer _constructHead(
			)
		{
			// ok; find out who we are to drop
			com.fasterxml.jackson.core.JsonPointer last = last();
			if (last == this)
			{
				return EMPTY;
			}
			// and from that, length of suffix to drop
			int suffixLength = last._asString.Length;
			com.fasterxml.jackson.core.JsonPointer next = _nextSegment;
			return new com.fasterxml.jackson.core.JsonPointer(Sharpen.Runtime.substring(_asString
				, 0, _asString.Length - suffixLength), _matchingPropertyName, _matchingElementIndex
				, next._constructHead(suffixLength, last));
		}

		protected internal virtual com.fasterxml.jackson.core.JsonPointer _constructHead(
			int suffixLength, com.fasterxml.jackson.core.JsonPointer last)
		{
			if (this == last)
			{
				return EMPTY;
			}
			com.fasterxml.jackson.core.JsonPointer next = _nextSegment;
			string str = _asString;
			return new com.fasterxml.jackson.core.JsonPointer(Sharpen.Runtime.substring(str, 
				0, str.Length - suffixLength), _matchingPropertyName, _matchingElementIndex, next
				._constructHead(suffixLength, last));
		}

		private static void _appendEscape(System.Text.StringBuilder sb, char c)
		{
			if (c == '0')
			{
				c = '~';
			}
			else
			{
				if (c == '1')
				{
					c = '/';
				}
				else
				{
					sb.Append('~');
				}
			}
			sb.Append(c);
		}
	}
}
