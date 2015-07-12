using Sharpen;

namespace com.fasterxml.jackson.core.type
{
	/// <summary>
	/// Type abstraction that represents Java type that has been resolved
	/// (i.e.
	/// </summary>
	/// <remarks>
	/// Type abstraction that represents Java type that has been resolved
	/// (i.e. has all generic information, if any, resolved to concrete
	/// types).
	/// Note that this is an intermediate type, and all concrete instances
	/// MUST be of type <code>JavaType</code> from "databind" bundle -- this
	/// abstraction is only needed so that types can be passed through
	/// <see cref="com.fasterxml.jackson.core.JsonParser.readValueAs{T}(System.Type{T})"/
	/// 	>
	/// methods.
	/// </remarks>
	/// <since>2.0</since>
	public abstract class ResolvedType
	{
		/*
		/**********************************************************
		/* Public API, simple property accessors
		/**********************************************************
		*/
		/// <summary>
		/// Accessor for type-erased
		/// <see cref="System.Type{T}"/>
		/// of resolved type.
		/// </summary>
		public abstract System.Type getRawClass();

		public abstract bool hasRawClass(System.Type clz);

		public abstract bool isAbstract();

		public abstract bool isConcrete();

		public abstract bool isThrowable();

		public abstract bool isArrayType();

		public abstract bool isEnumType();

		public abstract bool isInterface();

		public abstract bool isPrimitive();

		public abstract bool isFinal();

		public abstract bool isContainerType();

		public abstract bool isCollectionLikeType();

		/// <summary>
		/// Whether this type is a referential type, meaning that values are
		/// basically pointers to "real" values (or null) and not regular
		/// values themselves.
		/// </summary>
		/// <remarks>
		/// Whether this type is a referential type, meaning that values are
		/// basically pointers to "real" values (or null) and not regular
		/// values themselves. Typical examples include things like
		/// <see cref="java.util.concurrent.atomic.AtomicReference{V}"/>
		/// , and various
		/// <code>Optional</code> types (in JDK8, Guava).
		/// </remarks>
		/// <since>2.6</since>
		public virtual bool isReferenceType()
		{
			return getReferencedType() != null;
		}

		public abstract bool isMapLikeType();

		/*
		/**********************************************************
		/* Public API, type parameter access
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to find out if the type directly declares generic
		/// parameters (for its direct super-class and/or super-interfaces).
		/// </summary>
		public abstract bool hasGenericTypes();

		/// <summary>
		/// Accessor that can be used to find out type for which parameterization
		/// is applied: this is often NOT same as what
		/// <see cref="getRawClass()"/>
		/// returns,
		/// but rather one of it supertype.
		/// <p>
		/// For example: for type like
		/// <see cref="System.Collections.Hashtable{K, V}"/>
		/// , raw type is
		/// <see cref="System.Collections.Hashtable{K, V}"/>
		/// ; but this method would return
		/// <see cref="System.Collections.IDictionary{K, V}"/>
		/// , because relevant type parameters that are
		/// resolved (and accessible using
		/// <see cref="containedType(int)"/>
		/// and
		/// <see cref="getKeyType()"/>
		/// ) are parameter for
		/// <see cref="System.Collections.IDictionary{K, V}"/>
		/// (which may or may not be same as type parameters for subtype;
		/// in case of
		/// <see cref="System.Collections.Hashtable{K, V}"/>
		/// they are, but for further
		/// subtypes they may be different parameters or possibly none at all).
		/// </summary>
		/// <since>2.5</since>
		public virtual System.Type getParameterSource()
		{
			return null;
		}

		/// <summary>
		/// Method for accessing key type for this type, assuming type
		/// has such a concept (only Map types do)
		/// </summary>
		public abstract com.fasterxml.jackson.core.type.ResolvedType getKeyType();

		/// <summary>
		/// Method for accessing content type of this type, if type has
		/// such a thing: simple types do not, structured types do
		/// (like arrays, Collections and Maps)
		/// </summary>
		public abstract com.fasterxml.jackson.core.type.ResolvedType getContentType();

		/// <summary>
		/// Method for accessing type of value that instances of this
		/// type references, if any.
		/// </summary>
		/// <returns>Referenced type, if any; null if not.</returns>
		/// <since>2.6</since>
		public abstract com.fasterxml.jackson.core.type.ResolvedType getReferencedType();

		/// <summary>
		/// Method for checking how many contained types this type
		/// has.
		/// </summary>
		/// <remarks>
		/// Method for checking how many contained types this type
		/// has. Contained types are usually generic types, so that
		/// generic Maps have 2 contained types.
		/// </remarks>
		public abstract int containedTypeCount();

		/// <summary>
		/// Method for accessing definitions of contained ("child")
		/// types.
		/// </summary>
		/// <param name="index">Index of contained type to return</param>
		/// <returns>
		/// Contained type at index, or null if no such type
		/// exists (no exception thrown)
		/// </returns>
		public abstract com.fasterxml.jackson.core.type.ResolvedType containedType(int index
			);

		/// <summary>
		/// Method for accessing name of type variable in indicated
		/// position.
		/// </summary>
		/// <remarks>
		/// Method for accessing name of type variable in indicated
		/// position. If no name is bound, will use placeholders (derived
		/// from 0-based index); if no type variable or argument exists
		/// with given index, null is returned.
		/// </remarks>
		/// <param name="index">Index of contained type to return</param>
		/// <returns>
		/// Contained type at index, or null if no such type
		/// exists (no exception thrown)
		/// </returns>
		public abstract string containedTypeName(int index);

		/*
		/**********************************************************
		/* Public API, other
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used to serialize type into form from which
		/// it can be fully deserialized from at a later point (using
		/// <code>TypeFactory</code> from mapper package).
		/// </summary>
		/// <remarks>
		/// Method that can be used to serialize type into form from which
		/// it can be fully deserialized from at a later point (using
		/// <code>TypeFactory</code> from mapper package).
		/// For simple types this is same as calling
		/// <see cref="System.Type{T}.FullName()"/>
		/// , but for structured types it may additionally
		/// contain type information about contents.
		/// </remarks>
		public abstract string toCanonical();
	}
}
