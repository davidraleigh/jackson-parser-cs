using Sharpen;

namespace com.fasterxml.jackson.core.type
{
	/// <summary>
	/// This generic abstract class is used for obtaining full generics type information
	/// by sub-classing; it must be converted to
	/// <see cref="ResolvedType"/>
	/// implementation
	/// (implemented by <code>JavaType</code> from "databind" bundle) to be used.
	/// Class is based on ideas from
	/// &lt;a href="http://gafter.blogspot.com/2006/12/super-type-tokens.html"
	/// &gt;http://gafter.blogspot.com/2006/12/super-type-tokens.html</a>,
	/// Additional idea (from a suggestion made in comments of the article)
	/// is to require bogus implementation of <code>Comparable</code>
	/// (any such generic interface would do, as long as it forces a method
	/// with generic type to be implemented).
	/// to ensure that a Type argument is indeed given.
	/// <p>
	/// Usage is by sub-classing: here is one way to instantiate reference
	/// to generic type <code>List&lt;Integer&gt;</code>:
	/// <pre>
	/// TypeReference ref = new TypeReference&lt;List&lt;Integer&gt;&gt;() { };
	/// </pre>
	/// which can be passed to methods that accept TypeReference, or resolved
	/// using <code>TypeFactory</code> to obtain
	/// <see cref="ResolvedType"/>
	/// .
	/// </summary>
	public abstract class TypeReference<T> : Sharpen.Comparable<com.fasterxml.jackson.core.type.TypeReference
		<T>>
	{
		protected internal readonly Sharpen.reflect.Type _type;

		protected internal TypeReference()
		{
			Sharpen.reflect.Type superClass = GetType().getGenericSuperclass();
			if (superClass is System.Type)
			{
				// sanity check, should never happen
				throw new System.ArgumentException("Internal error: TypeReference constructed without actual type information"
					);
			}
			/* 22-Dec-2008, tatu: Not sure if this case is safe -- I suspect
			*   it is possible to make it fail?
			*   But let's deal with specific
			*   case when we know an actual use case, and thereby suitable
			*   workarounds for valid case(s) and/or error to throw
			*   on invalid one(s).
			*/
			_type = ((Sharpen.reflect.ParameterizedType)superClass).getActualTypeArguments()[
				0];
		}

		public virtual Sharpen.reflect.Type getType()
		{
			return _type;
		}

		/// <summary>
		/// The only reason we define this method (and require implementation
		/// of <code>Comparable</code>) is to prevent constructing a
		/// reference without type information.
		/// </summary>
		public virtual int compareTo(com.fasterxml.jackson.core.type.TypeReference<T> o)
		{
			return 0;
		}
		// just need an implementation, not a good one... hence ^^^
	}
}
