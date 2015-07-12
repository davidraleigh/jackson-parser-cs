/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Enumeration for basic token types used for returning results
	/// of parsing JSON content.
	/// </summary>
	[System.Serializable]
	public sealed class JsonToken
	{
		/// <summary>
		/// NOT_AVAILABLE can be returned if
		/// <see cref="JsonParser"/>
		/// implementation can not currently return the requested
		/// token (usually next one), or even if any will be
		/// available, but that may be able to determine this in
		/// future. This is the case with non-blocking parsers --
		/// they can not block to wait for more data to parse and
		/// must return something.
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken NOT_AVAILABLE = new com.fasterxml.jackson.core.JsonToken
			(null, JsonTokenIdConstants.ID_NOT_AVAILABLE);

		/// <summary>
		/// START_OBJECT is returned when encountering '{'
		/// which signals starting of an Object value.
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken START_OBJECT = new com.fasterxml.jackson.core.JsonToken
			("{", JsonTokenIdConstants.ID_START_OBJECT);

		/// <summary>
		/// END_OBJECT is returned when encountering '}'
		/// which signals ending of an Object value
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken END_OBJECT = new com.fasterxml.jackson.core.JsonToken
			("}", JsonTokenIdConstants.ID_END_OBJECT);

		/// <summary>
		/// START_ARRAY is returned when encountering '['
		/// which signals starting of an Array value
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken START_ARRAY = new com.fasterxml.jackson.core.JsonToken
			("[", JsonTokenIdConstants.ID_START_ARRAY);

		/// <summary>
		/// END_ARRAY is returned when encountering ']'
		/// which signals ending of an Array value
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken END_ARRAY = new com.fasterxml.jackson.core.JsonToken
			("]", JsonTokenIdConstants.ID_END_ARRAY);

		/// <summary>
		/// FIELD_NAME is returned when a String token is encountered
		/// as a field name (same lexical value, different function)
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken FIELD_NAME = new com.fasterxml.jackson.core.JsonToken
			(null, JsonTokenIdConstants.ID_FIELD_NAME);

		/// <summary>
		/// Placeholder token returned when the input source has a concept
		/// of embedded Object that are not accessible as usual structure
		/// (of starting with
		/// <see cref="START_OBJECT"/>
		/// , having values, ending with
		/// <see cref="END_OBJECT"/>
		/// ), but as "raw" objects.
		/// <p>
		/// Note: this token is never returned by regular JSON readers, but
		/// only by readers that expose other kinds of source (like
		/// <code>JsonNode</code>-based JSON trees, Maps, Lists and such).
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_EMBEDDED_OBJECT
			 = new com.fasterxml.jackson.core.JsonToken(null, JsonTokenIdConstants.ID_EMBEDDED_OBJECT
			);

		/// <summary>
		/// VALUE_STRING is returned when a String token is encountered
		/// in value context (array element, field value, or root-level
		/// stand-alone value)
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_STRING = new com.fasterxml.jackson.core.JsonToken
			(null, JsonTokenIdConstants.ID_STRING);

		/// <summary>
		/// VALUE_NUMBER_INT is returned when an integer numeric token is
		/// encountered in value context: that is, a number that does
		/// not have floating point or exponent marker in it (consists
		/// only of an optional sign, followed by one or more digits)
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_NUMBER_INT = new 
			com.fasterxml.jackson.core.JsonToken(null, JsonTokenIdConstants.ID_NUMBER_INT);

		/// <summary>
		/// VALUE_NUMBER_INT is returned when a numeric token other
		/// that is not an integer is encountered: that is, a number that does
		/// have floating point or exponent marker in it, in addition
		/// to one or more digits.
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_NUMBER_FLOAT = 
			new com.fasterxml.jackson.core.JsonToken(null, JsonTokenIdConstants.ID_NUMBER_FLOAT
			);

		/// <summary>
		/// VALUE_TRUE is returned when encountering literal "true" in
		/// value context
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_TRUE = new com.fasterxml.jackson.core.JsonToken
			("true", JsonTokenIdConstants.ID_TRUE);

		/// <summary>
		/// VALUE_FALSE is returned when encountering literal "false" in
		/// value context
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_FALSE = new com.fasterxml.jackson.core.JsonToken
			("false", JsonTokenIdConstants.ID_FALSE);

		/// <summary>
		/// VALUE_NULL is returned when encountering literal "null" in
		/// value context
		/// </summary>
		public static readonly com.fasterxml.jackson.core.JsonToken VALUE_NULL = new com.fasterxml.jackson.core.JsonToken
			("null", JsonTokenIdConstants.ID_NULL);

		internal readonly string _serialized;

		internal readonly char[] _serializedChars;

		internal readonly byte[] _serializedBytes;

		internal readonly int _id;

		internal readonly bool _isStructStart;

		internal readonly bool _isStructEnd;

		internal readonly bool _isNumber;

		internal readonly bool _isBoolean;

		internal readonly bool _isScalar;

		/// <param name="token">
		/// representation for this token, if there is a
		/// single static representation; null otherwise
		/// </param>
		internal JsonToken(string token, int id)
		{
			/* Some notes on implementation:
			*
			* - Entries are to be ordered such that start/end array/object
			*   markers come first, then field name marker (if any), and
			*   finally scalar value tokens. This is assumed by some
			*   typing checks.
			*/
			if (token == null)
			{
				com.fasterxml.jackson.core.JsonToken._serialized = null;
				com.fasterxml.jackson.core.JsonToken._serializedChars = null;
				com.fasterxml.jackson.core.JsonToken._serializedBytes = null;
			}
			else
			{
				com.fasterxml.jackson.core.JsonToken._serialized = token;
				com.fasterxml.jackson.core.JsonToken._serializedChars = token.ToCharArray();
				// It's all in ascii, can just case...
				int len = com.fasterxml.jackson.core.JsonToken._serializedChars.Length;
				com.fasterxml.jackson.core.JsonToken._serializedBytes = new byte[len];
				for (int i = 0; i < len; ++i)
				{
					com.fasterxml.jackson.core.JsonToken._serializedBytes[i] = unchecked((byte)com.fasterxml.jackson.core.JsonToken
						._serializedChars[i]);
				}
			}
			com.fasterxml.jackson.core.JsonToken._id = id;
			com.fasterxml.jackson.core.JsonToken._isBoolean = (id == JsonTokenIdConstants.ID_FALSE
				 || id == JsonTokenIdConstants.ID_TRUE);
			com.fasterxml.jackson.core.JsonToken._isNumber = (id == JsonTokenIdConstants.ID_NUMBER_INT
				 || id == JsonTokenIdConstants.ID_NUMBER_FLOAT);
			com.fasterxml.jackson.core.JsonToken._isStructStart = (id == JsonTokenIdConstants
				.ID_START_OBJECT || id == JsonTokenIdConstants.ID_START_ARRAY);
			com.fasterxml.jackson.core.JsonToken._isStructEnd = (id == JsonTokenIdConstants.ID_END_OBJECT
				 || id == JsonTokenIdConstants.ID_END_ARRAY);
			com.fasterxml.jackson.core.JsonToken._isScalar = !com.fasterxml.jackson.core.JsonToken
				._isStructStart && !com.fasterxml.jackson.core.JsonToken._isStructEnd && (id != 
				JsonTokenIdConstants.ID_FIELD_NAME) && (id != JsonTokenIdConstants.ID_NOT_AVAILABLE
				);
		}

		public int id()
		{
			return com.fasterxml.jackson.core.JsonToken._id;
		}

		public string asString()
		{
			return com.fasterxml.jackson.core.JsonToken._serialized;
		}

		public char[] asCharArray()
		{
			return com.fasterxml.jackson.core.JsonToken._serializedChars;
		}

		public byte[] asByteArray()
		{
			return com.fasterxml.jackson.core.JsonToken._serializedBytes;
		}

		public bool isNumeric()
		{
			return com.fasterxml.jackson.core.JsonToken._isNumber;
		}

		/// <summary>
		/// Accessor that is functionally equivalent to:
		/// <code>
		/// this == JsonToken.START_OBJECT || this == JsonToken.START_ARRAY
		/// </code>
		/// </summary>
		/// <since>2.3</since>
		public bool isStructStart()
		{
			return com.fasterxml.jackson.core.JsonToken._isStructStart;
		}

		/// <summary>
		/// Accessor that is functionally equivalent to:
		/// <code>
		/// this == JsonToken.END_OBJECT || this == JsonToken.END_ARRAY
		/// </code>
		/// </summary>
		/// <since>2.3</since>
		public bool isStructEnd()
		{
			return com.fasterxml.jackson.core.JsonToken._isStructEnd;
		}

		/// <summary>
		/// Method that can be used to check whether this token represents
		/// a valid non-structured value.
		/// </summary>
		/// <remarks>
		/// Method that can be used to check whether this token represents
		/// a valid non-structured value. This means all tokens other than
		/// Object/Array start/end markers all field names.
		/// </remarks>
		public bool isScalarValue()
		{
			return com.fasterxml.jackson.core.JsonToken._isScalar;
		}

		public bool isBoolean()
		{
			return com.fasterxml.jackson.core.JsonToken._isBoolean;
		}
	}
}
