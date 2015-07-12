using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Helper class that can be used to sequence multiple physical
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// s to create a single logical sequence of
	/// tokens, as a single
	/// <see cref="com.fasterxml.jackson.core.JsonParser"/>
	/// .
	/// <p>
	/// Fairly simple use of
	/// <see cref="JsonParserDelegate"/>
	/// : only need
	/// to override
	/// <see cref="nextToken()"/>
	/// to handle transition
	/// </summary>
	public class JsonParserSequence : com.fasterxml.jackson.core.util.JsonParserDelegate
	{
		/// <summary>
		/// Parsers other than the first one (which is initially assigned
		/// as delegate)
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.JsonParser[] _parsers;

		/// <summary>
		/// Index of the next parser in
		/// <see cref="_parsers"/>
		/// .
		/// </summary>
		protected internal int _nextParser;

		protected internal JsonParserSequence(com.fasterxml.jackson.core.JsonParser[] parsers
			)
			: base(parsers[0])
		{
			/*
			*******************************************************
			* Construction
			*******************************************************
			*/
			_parsers = parsers;
			_nextParser = 1;
		}

		/// <summary>
		/// Method that will construct a parser (possibly a sequence) that
		/// contains all given sub-parsers.
		/// </summary>
		/// <remarks>
		/// Method that will construct a parser (possibly a sequence) that
		/// contains all given sub-parsers.
		/// All parsers given are checked to see if they are sequences: and
		/// if so, they will be "flattened", that is, contained parsers are
		/// directly added in a new sequence instead of adding sequences
		/// within sequences. This is done to minimize delegation depth,
		/// ideally only having just a single level of delegation.
		/// </remarks>
		public static com.fasterxml.jackson.core.util.JsonParserSequence createFlattened(
			com.fasterxml.jackson.core.JsonParser first, com.fasterxml.jackson.core.JsonParser
			 second)
		{
			if (!(first is com.fasterxml.jackson.core.util.JsonParserSequence || second is com.fasterxml.jackson.core.util.JsonParserSequence
				))
			{
				// simple:
				return new com.fasterxml.jackson.core.util.JsonParserSequence(new com.fasterxml.jackson.core.JsonParser
					[] { first, second });
			}
			Sharpen.AList<com.fasterxml.jackson.core.JsonParser> p = new Sharpen.AList<com.fasterxml.jackson.core.JsonParser
				>();
			if (first is com.fasterxml.jackson.core.util.JsonParserSequence)
			{
				((com.fasterxml.jackson.core.util.JsonParserSequence)first).addFlattenedActiveParsers
					(p);
			}
			else
			{
				p.Add(first);
			}
			if (second is com.fasterxml.jackson.core.util.JsonParserSequence)
			{
				((com.fasterxml.jackson.core.util.JsonParserSequence)second).addFlattenedActiveParsers
					(p);
			}
			else
			{
				p.Add(second);
			}
			return new com.fasterxml.jackson.core.util.JsonParserSequence(Sharpen.Collections.ToArray
				(p, new com.fasterxml.jackson.core.JsonParser[p.Count]));
		}

		protected internal virtual void addFlattenedActiveParsers(System.Collections.Generic.IList
			<com.fasterxml.jackson.core.JsonParser> result)
		{
			for (int i = _nextParser - 1; i < len; ++i)
			{
				com.fasterxml.jackson.core.JsonParser p = _parsers[i];
				if (p is com.fasterxml.jackson.core.util.JsonParserSequence)
				{
					((com.fasterxml.jackson.core.util.JsonParserSequence)p).addFlattenedActiveParsers
						(result);
				}
				else
				{
					result.Add(p);
				}
			}
		}

		/*
		*******************************************************
		* Overridden methods, needed: cases where default
		* delegation does not work
		*******************************************************
		*/
		/// <exception cref="System.IO.IOException"/>
		public override void close()
		{
			do
			{
				delegate_.close();
			}
			while (switchToNext());
		}

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonParseException"/>
		public override com.fasterxml.jackson.core.JsonToken nextToken()
		{
			com.fasterxml.jackson.core.JsonToken t = delegate_.nextToken();
			if (t != null)
			{
				return t;
			}
			while (switchToNext())
			{
				t = delegate_.nextToken();
				if (t != null)
				{
					return t;
				}
			}
			return null;
		}

		/*
		/*******************************************************
		/* Additional extended API
		/*******************************************************
		*/
		/// <summary>
		/// Method that is most useful for debugging or testing;
		/// returns actual number of underlying parsers sequence
		/// was constructed with (nor just ones remaining active)
		/// </summary>
		public virtual int containedParsersCount()
		{
			return _parsers.Length;
		}

		/*
		/*******************************************************
		/* Helper methods
		/*******************************************************
		*/
		/// <summary>
		/// Method that will switch active parser from the current one
		/// to next parser in sequence, if there is another parser left,
		/// making this the new delegate.
		/// </summary>
		/// <remarks>
		/// Method that will switch active parser from the current one
		/// to next parser in sequence, if there is another parser left,
		/// making this the new delegate. Old delegate is returned if
		/// switch succeeds.
		/// </remarks>
		/// <returns>True if switch succeeded; false otherwise</returns>
		protected internal virtual bool switchToNext()
		{
			if (_nextParser >= _parsers.Length)
			{
				return false;
			}
			delegate_ = _parsers[_nextParser++];
			return true;
		}
	}
}
