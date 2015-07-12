using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Interface defined to contain ids accessible with
	/// <see cref="JsonToken.id()"/>
	/// .
	/// Needed because it is impossible to define these constants in
	/// <see cref="JsonToken"/>
	/// itself, as static constants (oddity of how Enums
	/// are implemented by JVM).
	/// </summary>
	/// <since>2.3</since>
	public interface JsonTokenId
	{
	}

	public static class JsonTokenIdConstants
	{
		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.NOT_AVAILABLE"/>
		/// , used in
		/// cases where a token may become available when more input
		/// is available: this occurs in non-blocking use cases.
		/// </summary>
		public const int ID_NOT_AVAILABLE = -1;

		/// <summary>
		/// Id used to represent the case where no
		/// <see cref="JsonToken"/>
		/// is available: either because
		/// <see cref="JsonParser"/>
		/// has not been
		/// advanced to first token, or because no more tokens will be
		/// available (end-of-input or explicit closing of parser}.
		/// </summary>
		public const int ID_NO_TOKEN = 0;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.START_OBJECT"/>
		/// </summary>
		public const int ID_START_OBJECT = 1;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.END_OBJECT"/>
		/// </summary>
		public const int ID_END_OBJECT = 2;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.START_ARRAY"/>
		/// </summary>
		public const int ID_START_ARRAY = 3;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.END_ARRAY"/>
		/// </summary>
		public const int ID_END_ARRAY = 4;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.FIELD_NAME"/>
		/// </summary>
		public const int ID_FIELD_NAME = 5;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_STRING"/>
		/// </summary>
		public const int ID_STRING = 6;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_NUMBER_INT"/>
		/// </summary>
		public const int ID_NUMBER_INT = 7;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_NUMBER_FLOAT"/>
		/// </summary>
		public const int ID_NUMBER_FLOAT = 8;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_TRUE"/>
		/// </summary>
		public const int ID_TRUE = 9;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_FALSE"/>
		/// </summary>
		public const int ID_FALSE = 10;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_NULL"/>
		/// </summary>
		public const int ID_NULL = 11;

		/// <summary>
		/// Id used to represent
		/// <see cref="JsonToken.VALUE_EMBEDDED_OBJECT"/>
		/// </summary>
		public const int ID_EMBEDDED_OBJECT = 12;
	}
}
