/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Abstract class that defines the interface that
	/// <see cref="JsonParser"/>
	/// and
	/// <see cref="JsonGenerator"/>
	/// use to serialize and deserialize regular
	/// Java objects (POJOs aka Beans).
	/// <p>
	/// The standard implementation of this class is
	/// <code>com.fasterxml.jackson.databind.ObjectMapper</code>,
	/// defined in the "jackson-databind".
	/// </summary>
	public abstract class ObjectCodec : com.fasterxml.jackson.core.TreeCodec, com.fasterxml.jackson.core.Versioned
	{
		protected internal ObjectCodec()
		{
		}

		// since 2.3
		// since 2.3
		// Since 2.3: need baseline implementation to avoid backwards compatibility
		public virtual com.fasterxml.jackson.core.Version version()
		{
			return com.fasterxml.jackson.core.Version.unknownVersion();
		}

		/*
		/**********************************************************
		/* API for de-serialization (JSON-to-Object)
		/**********************************************************
		*/
		/// <summary>
		/// Method to deserialize JSON content into a non-container
		/// type (it can be an array type, however): typically a bean, array
		/// or a wrapper type (like
		/// <see cref="bool"/>
		/// ).
		/// <p>
		/// Note: this method should NOT be used if the result type is a
		/// container (
		/// <see cref="System.Collections.ICollection{E}"/>
		/// or
		/// <see cref="System.Collections.IDictionary{K, V}"/>
		/// .
		/// The reason is that due to type erasure, key and value types
		/// can not be introspected when using this method.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract T readValue<T>(com.fasterxml.jackson.core.JsonParser jp);

		/// <summary>
		/// Method to deserialize JSON content into a Java type, reference
		/// to which is passed as argument.
		/// </summary>
		/// <remarks>
		/// Method to deserialize JSON content into a Java type, reference
		/// to which is passed as argument. Type is passed using so-called
		/// "super type token"
		/// and specifically needs to be used if the root type is a
		/// parameterized (generic) container type.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract T readValue<T, _T1>(com.fasterxml.jackson.core.JsonParser jp, com.fasterxml.jackson.core.type.TypeReference
			<_T1> valueTypeRef);

		/// <summary>
		/// Method to deserialize JSON content into a POJO, type specified
		/// with fully resolved type object (so it can be a generic type,
		/// including containers like
		/// <see cref="System.Collections.ICollection{E}"/>
		/// and
		/// <see cref="System.Collections.IDictionary{K, V}"/>
		/// ).
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract T readValue<T>(com.fasterxml.jackson.core.JsonParser jp, com.fasterxml.jackson.core.type.ResolvedType
			 valueType);

		/// <summary>
		/// Method for reading sequence of Objects from parser stream,
		/// all with same specified value type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract System.Collections.Generic.IEnumerator<T> readValues<T>(com.fasterxml.jackson.core.JsonParser
			 jp);

		/// <summary>
		/// Method for reading sequence of Objects from parser stream,
		/// all with same specified value type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract System.Collections.Generic.IEnumerator<T> readValues<T, _T1>(com.fasterxml.jackson.core.JsonParser
			 jp, com.fasterxml.jackson.core.type.TypeReference<_T1> valueTypeRef);

		/// <summary>
		/// Method for reading sequence of Objects from parser stream,
		/// all with same specified value type.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract System.Collections.Generic.IEnumerator<T> readValues<T>(com.fasterxml.jackson.core.JsonParser
			 jp, com.fasterxml.jackson.core.type.ResolvedType valueType);

		/*
		/**********************************************************
		/* API for serialization (Object-to-JSON)
		/**********************************************************
		*/
		/// <summary>
		/// Method to serialize given Java Object, using generator
		/// provided.
		/// </summary>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract void writeValue(com.fasterxml.jackson.core.JsonGenerator jgen, object
			 value);

		/*
		/**********************************************************
		/* TreeCodec pass-through methods
		/**********************************************************
		*/
		/// <summary>
		/// Method to deserialize JSON content as tree expressed
		/// using set of
		/// <see cref="TreeNode"/>
		/// instances. Returns
		/// root of the resulting tree (where root can consist
		/// of just a single node if the current event is a
		/// value event, not container). Empty or whitespace
		/// documents return null.
		/// </summary>
		/// <returns>next tree from jp, or null if empty.</returns>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract override T readTree<T>(com.fasterxml.jackson.core.JsonParser jp);
			//where T : com.fasterxml.jackson.core.TreeNode;

		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract override void writeTree(com.fasterxml.jackson.core.JsonGenerator 
			jg, com.fasterxml.jackson.core.TreeNode tree);

		/// <summary>
		/// Method for construct root level Object nodes
		/// for Tree Model instances.
		/// </summary>
		public abstract override com.fasterxml.jackson.core.TreeNode createObjectNode();

		/// <summary>
		/// Method for construct root level Array nodes
		/// for Tree Model instances.
		/// </summary>
		public abstract override com.fasterxml.jackson.core.TreeNode createArrayNode();

		/// <summary>
		/// Method for constructing a
		/// <see cref="JsonParser"/>
		/// for reading
		/// contents of a JSON tree, as if it was external serialized
		/// JSON content.
		/// </summary>
		public abstract override com.fasterxml.jackson.core.JsonParser treeAsTokens(com.fasterxml.jackson.core.TreeNode
			 n);

		/*
		/**********************************************************
		/* Extended tree conversions beyond TreeCodec
		/**********************************************************
		*/
		/// <summary>
		/// Convenience method for converting given JSON tree into instance of specified
		/// value type.
		/// </summary>
		/// <remarks>
		/// Convenience method for converting given JSON tree into instance of specified
		/// value type. This is equivalent to first constructing a
		/// <see cref="JsonParser"/>
		/// to
		/// iterate over contents of the tree, and using that parser for data binding.
		/// </remarks>
		/// <exception cref="com.fasterxml.jackson.core.JsonProcessingException"/>
		public abstract T treeToValue<T>(com.fasterxml.jackson.core.TreeNode n);

		/*
		/**********************************************************
		/* Basic accessors
		/**********************************************************
		*/
		[System.ObsoleteAttribute(@"Since 2.1: Use getFactory() instead.")]
		public virtual com.fasterxml.jackson.core.JsonFactory getJsonFactory()
		{
			return getFactory();
		}

		/// <summary>
		/// Accessor for finding underlying data format factory
		/// (
		/// <see cref="JsonFactory"/>
		/// ) codec will use for data binding.
		/// </summary>
		/// <since>2.1</since>
		public virtual com.fasterxml.jackson.core.JsonFactory getFactory()
		{
			return getJsonFactory();
		}
	}
}
