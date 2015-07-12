using Sharpen;

namespace com.fasterxml.jackson.core.filter
{
	/// <summary>
	/// Simple
	/// <see cref="TokenFilter"/>
	/// implementation that takes a single
	/// <see cref="com.fasterxml.jackson.core.JsonPointer"/>
	/// and matches a single value accordingly.
	/// Instances are immutable and fully thread-safe, shareable,
	/// and efficient to use.
	/// </summary>
	/// <since>2.6</since>
	public class JsonPointerBasedFilter : com.fasterxml.jackson.core.filter.TokenFilter
	{
		protected internal readonly com.fasterxml.jackson.core.JsonPointer _pathToMatch;

		public JsonPointerBasedFilter(string ptrExpr)
			: this(com.fasterxml.jackson.core.JsonPointer.compile(ptrExpr))
		{
		}

		public JsonPointerBasedFilter(com.fasterxml.jackson.core.JsonPointer match)
		{
			_pathToMatch = match;
		}

		public override com.fasterxml.jackson.core.filter.TokenFilter includeElement(int 
			index)
		{
			com.fasterxml.jackson.core.JsonPointer next = _pathToMatch.matchElement(index);
			if (next == null)
			{
				return null;
			}
			if (next.matches())
			{
				return com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL;
			}
			return new com.fasterxml.jackson.core.filter.JsonPointerBasedFilter(next);
		}

		public override com.fasterxml.jackson.core.filter.TokenFilter includeProperty(string
			 name)
		{
			com.fasterxml.jackson.core.JsonPointer next = _pathToMatch.matchProperty(name);
			if (next == null)
			{
				return null;
			}
			if (next.matches())
			{
				return com.fasterxml.jackson.core.filter.TokenFilter.INCLUDE_ALL;
			}
			return new com.fasterxml.jackson.core.filter.JsonPointerBasedFilter(next);
		}

		public override com.fasterxml.jackson.core.filter.TokenFilter filterStartArray()
		{
			return this;
		}

		public override com.fasterxml.jackson.core.filter.TokenFilter filterStartObject()
		{
			return this;
		}

		protected internal override bool _includeScalar()
		{
			// should only occur for root-level scalars, path "/"
			return _pathToMatch.matches();
		}

		public override string ToString()
		{
			return "[JsonPointerFilter at: " + _pathToMatch + "]";
		}
	}
}
