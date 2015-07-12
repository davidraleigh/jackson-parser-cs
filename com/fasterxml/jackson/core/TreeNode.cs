/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Marker interface used to denote JSON Tree nodes, as far as
	/// the core package knows them (which is very little): mostly
	/// needed to allow
	/// <see cref="ObjectCodec"/>
	/// to have some level
	/// of interoperability.
	/// Most functionality is within <code>JsonNode</code>
	/// base class in <code>mapper</code> package.
	/// <p>
	/// Note that in Jackson 1.x <code>JsonNode</code> itself
	/// was part of core package: Jackson 2.x refactored this
	/// since conceptually Tree Model is part of mapper package,
	/// and so part visible to <code>core</code> package should
	/// be minimized.
	/// <p>
	/// NOTE: starting with Jackson 2.2, there is more functionality
	/// available via this class, and the intent is that this should
	/// form actual base for multiple alternative tree representations;
	/// for example, immutable trees could use different implementation
	/// than mutable trees. It should also be possible to move actual
	/// Tree Model implementation out of databind package eventually
	/// (Jackson 3?).
	/// </summary>
	/// <since>2.2</since>
	public interface TreeNode
	{
		/*
		/**********************************************************
		/* Minimal introspection methods
		/**********************************************************
		*/
		/// <summary>
		/// Method that can be used for efficient type detection
		/// when using stream abstraction for traversing nodes.
		/// </summary>
		/// <remarks>
		/// Method that can be used for efficient type detection
		/// when using stream abstraction for traversing nodes.
		/// Will return the first
		/// <see cref="JsonToken"/>
		/// that equivalent
		/// stream event would produce (for most nodes there is just
		/// one token but for structured/container types multiple)
		/// </remarks>
		com.fasterxml.jackson.core.JsonToken asToken();

		/// <summary>
		/// If this node is a numeric type (as per
		/// <see cref="JsonToken.isNumeric()"/>
		/// ),
		/// returns native type that node uses to store the numeric value;
		/// otherwise returns null.
		/// </summary>
		/// <returns>
		/// Type of number contained, if any; or null if node does not
		/// contain numeric value.
		/// </returns>
		com.fasterxml.jackson.core.JsonParser.NumberType numberType();

		/// <summary>
		/// Method that returns number of child nodes this node contains:
		/// for Array nodes, number of child elements, for Object nodes,
		/// number of fields, and for all other nodes 0.
		/// </summary>
		/// <returns>
		/// For non-container nodes returns 0; for arrays number of
		/// contained elements, and for objects number of fields.
		/// </returns>
		/// <since>2.2</since>
		int size();

		/// <summary>
		/// Method that returns true for all value nodes: ones that
		/// are not containers, and that do not represent "missing" nodes
		/// in the path.
		/// </summary>
		/// <remarks>
		/// Method that returns true for all value nodes: ones that
		/// are not containers, and that do not represent "missing" nodes
		/// in the path. Such value nodes represent String, Number, Boolean
		/// and null values from JSON.
		/// <p>
		/// Note: one and only one of methods
		/// <see cref="isValueNode()"/>
		/// ,
		/// <see cref="isContainerNode()"/>
		/// and
		/// <see cref="isMissingNode()"/>
		/// ever
		/// returns true for any given node.
		/// </remarks>
		/// <since>2.2</since>
		bool isValueNode();

		/// <summary>Method that returns true for container nodes: Arrays and Objects.</summary>
		/// <remarks>
		/// Method that returns true for container nodes: Arrays and Objects.
		/// <p>
		/// Note: one and only one of methods
		/// <see cref="isValueNode()"/>
		/// ,
		/// <see cref="isContainerNode()"/>
		/// and
		/// <see cref="isMissingNode()"/>
		/// ever
		/// returns true for any given node.
		/// </remarks>
		/// <since>2.2</since>
		bool isContainerNode();

		/// <summary>
		/// Method that returns true for "virtual" nodes which represent
		/// missing entries constructed by path accessor methods when
		/// there is no actual node matching given criteria.
		/// </summary>
		/// <remarks>
		/// Method that returns true for "virtual" nodes which represent
		/// missing entries constructed by path accessor methods when
		/// there is no actual node matching given criteria.
		/// <p>
		/// Note: one and only one of methods
		/// <see cref="isValueNode()"/>
		/// ,
		/// <see cref="isContainerNode()"/>
		/// and
		/// <see cref="isMissingNode()"/>
		/// ever
		/// returns true for any given node.
		/// </remarks>
		/// <since>2.2</since>
		bool isMissingNode();

		/// <summary>
		/// Method that returns true if this node is an Array node, false
		/// otherwise.
		/// </summary>
		/// <remarks>
		/// Method that returns true if this node is an Array node, false
		/// otherwise.
		/// Note that if true is returned,
		/// <see cref="isContainerNode()"/>
		/// must also return true.
		/// </remarks>
		/// <since>2.2</since>
		bool isArray();

		/// <summary>
		/// Method that returns true if this node is an Object node, false
		/// otherwise.
		/// </summary>
		/// <remarks>
		/// Method that returns true if this node is an Object node, false
		/// otherwise.
		/// Note that if true is returned,
		/// <see cref="isContainerNode()"/>
		/// must also return true.
		/// </remarks>
		/// <since>2.2</since>
		bool isObject();

		/*
		/**********************************************************
		/* Basic traversal through structured entries (Arrays, Objects)
		/**********************************************************
		*/
		/// <summary>
		/// Method for accessing value of the specified field of
		/// an object node.
		/// </summary>
		/// <remarks>
		/// Method for accessing value of the specified field of
		/// an object node. If this node is not an object (or it
		/// does not have a value for specified field name), or
		/// if there is no field with such name, null is returned.
		/// <p>
		/// NOTE: handling of explicit null values may vary between
		/// implementations; some trees may retain explicit nulls, others
		/// not.
		/// </remarks>
		/// <returns>
		/// Node that represent value of the specified field,
		/// if this node is an object and has value for the specified
		/// field. Null otherwise.
		/// </returns>
		/// <since>2.2</since>
		com.fasterxml.jackson.core.TreeNode get(string fieldName);

		/// <summary>
		/// Method for accessing value of the specified element of
		/// an array node.
		/// </summary>
		/// <remarks>
		/// Method for accessing value of the specified element of
		/// an array node. For other nodes, null is returned.
		/// <p>
		/// For array nodes, index specifies
		/// exact location within array and allows for efficient iteration
		/// over child elements (underlying storage is guaranteed to
		/// be efficiently indexable, i.e. has random-access to elements).
		/// If index is less than 0, or equal-or-greater than
		/// <code>node.size()</code>, null is returned; no exception is
		/// thrown for any index.
		/// </remarks>
		/// <returns>
		/// Node that represent value of the specified element,
		/// if this node is an array and has specified element.
		/// Null otherwise.
		/// </returns>
		/// <since>2.2</since>
		com.fasterxml.jackson.core.TreeNode get(int index);

		/// <summary>
		/// Method for accessing value of the specified field of
		/// an object node.
		/// </summary>
		/// <remarks>
		/// Method for accessing value of the specified field of
		/// an object node.
		/// For other nodes, a "missing node" (virtual node
		/// for which
		/// <see cref="isMissingNode()"/>
		/// returns true) is returned.
		/// </remarks>
		/// <returns>
		/// Node that represent value of the specified field,
		/// if this node is an object and has value for the specified field;
		/// otherwise "missing node" is returned.
		/// </returns>
		/// <since>2.2</since>
		com.fasterxml.jackson.core.TreeNode path(string fieldName);

		/// <summary>
		/// Method for accessing value of the specified element of
		/// an array node.
		/// </summary>
		/// <remarks>
		/// Method for accessing value of the specified element of
		/// an array node.
		/// For other nodes, a "missing node" (virtual node
		/// for which
		/// <see cref="isMissingNode()"/>
		/// returns true) is returned.
		/// <p>
		/// For array nodes, index specifies
		/// exact location within array and allows for efficient iteration
		/// over child elements (underlying storage is guaranteed to
		/// be efficiently indexable, i.e. has random-access to elements).
		/// If index is less than 0, or equal-or-greater than
		/// <code>node.size()</code>, "missing node" is returned; no exception is
		/// thrown for any index.
		/// </remarks>
		/// <returns>
		/// Node that represent value of the specified element,
		/// if this node is an array and has specified element;
		/// otherwise "missing node" is returned.
		/// </returns>
		/// <since>2.2</since>
		com.fasterxml.jackson.core.TreeNode path(int index);

		/// <summary>
		/// Method for accessing names of all fields for this node, iff
		/// this node is an Object node.
		/// </summary>
		/// <remarks>
		/// Method for accessing names of all fields for this node, iff
		/// this node is an Object node. Number of field names accessible
		/// will be
		/// <see cref="size()"/>
		/// .
		/// </remarks>
		/// <since>2.2</since>
		System.Collections.Generic.IEnumerator<string> fieldNames();

		/// <summary>Method for locating node specified by given JSON pointer instances.</summary>
		/// <remarks>
		/// Method for locating node specified by given JSON pointer instances.
		/// Method will never return null; if no matching node exists,
		/// will return a node for which
		/// <see cref="isMissingNode()"/>
		/// returns true.
		/// </remarks>
		/// <returns>
		/// Node that matches given JSON Pointer: if no match exists,
		/// will return a node for which
		/// <see cref="isMissingNode()"/>
		/// returns true.
		/// </returns>
		/// <since>2.3</since>
		com.fasterxml.jackson.core.TreeNode at(com.fasterxml.jackson.core.JsonPointer ptr
			);

		/// <summary>
		/// Convenience method that is functionally equivalent to:
		/// <pre>
		/// return at(JsonPointer.valueOf(jsonPointerExpression));
		/// </pre>
		/// <p>
		/// Note that if the same expression is used often, it is preferable to construct
		/// <see cref="JsonPointer"/>
		/// instance once and reuse it: this method will not perform
		/// any caching of compiled expressions.
		/// </summary>
		/// <param name="jsonPointerExpression">
		/// Expression to compile as a
		/// <see cref="JsonPointer"/>
		/// instance
		/// </param>
		/// <returns>
		/// Node that matches given JSON Pointer: if no match exists,
		/// will return a node for which
		/// <see cref="isMissingNode()"/>
		/// returns true.
		/// </returns>
		/// <since>2.3</since>
		/// <exception cref="System.ArgumentException"/>
		com.fasterxml.jackson.core.TreeNode at(string jsonPointerExpression);

		/*
		/**********************************************************
		/* Converting to/from Streaming API
		/**********************************************************
		*/
		/// <summary>
		/// Method for constructing a
		/// <see cref="JsonParser"/>
		/// instance for
		/// iterating over contents of the tree that this node is root of.
		/// Functionally equivalent to first serializing tree using
		/// <see cref="ObjectCodec"/>
		/// and then re-parsing but
		/// more efficient.
		/// <p>
		/// NOTE: constructed parser instance will NOT initially point to a token,
		/// so before passing it to deserializers, it is typically necessary to
		/// advance it to the first available token by calling
		/// <see cref="JsonParser.nextToken()"/>
		/// .
		/// <p>
		/// Also note that calling this method will <b>NOT</b> pass
		/// <see cref="ObjectCodec"/>
		/// reference, so data-binding callback methods like
		/// <see cref="JsonParser.readValueAs{T}(System.Type{T})"/>
		/// will not work with calling
		/// <see cref="JsonParser.setCodec(ObjectCodec)"/>
		/// ).
		/// It is often better to call
		/// <see cref="traverse(ObjectCodec)"/>
		/// to pass the codec explicitly.
		/// </summary>
		com.fasterxml.jackson.core.JsonParser traverse();

		/// <summary>
		/// Same as
		/// <see cref="traverse()"/>
		/// , but additionally passes
		/// <see cref="ObjectCodec"/>
		/// to use if
		/// <see cref="JsonParser.readValueAs{T}(System.Type{T})"/>
		/// is used (otherwise caller must call
		/// <see cref="JsonParser.setCodec(ObjectCodec)"/>
		/// on response explicitly).
		/// <p>
		/// NOTE: constructed parser instance will NOT initially point to a token,
		/// so before passing it to deserializers, it is typically necessary to
		/// advance it to the first available token by calling
		/// <see cref="JsonParser.nextToken()"/>
		/// .
		/// </summary>
		/// <since>2.1</since>
		com.fasterxml.jackson.core.JsonParser traverse(com.fasterxml.jackson.core.ObjectCodec
			 codec);
	}
}
