using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>
	/// Abstract base class that defines interface for customizing character
	/// escaping aspects for String values, for formats that use escaping.
	/// </summary>
	/// <remarks>
	/// Abstract base class that defines interface for customizing character
	/// escaping aspects for String values, for formats that use escaping.
	/// For JSON this applies to both property names and String values.
	/// </remarks>
	[System.Serializable]
	public abstract class CharacterEscapes
	{
		/// <summary>
		/// Value used for lookup tables to indicate that matching characters
		/// do not need to be escaped.
		/// </summary>
		public const int ESCAPE_NONE = 0;

		/// <summary>
		/// Value used for lookup tables to indicate that matching characters
		/// are to be escaped using standard escaping; for JSON this means
		/// (for example) using "backslash - u" escape method.
		/// </summary>
		public const int ESCAPE_STANDARD = -1;

		/// <summary>
		/// Value used for lookup tables to indicate that matching characters
		/// will need custom escapes; and that another call
		/// to
		/// <see cref="getEscapeSequence(int)"/>
		/// is needed to figure out exact escape
		/// sequence to output.
		/// </summary>
		public const int ESCAPE_CUSTOM = -2;

		// since 2.1
		/// <summary>
		/// Method generators can call to get lookup table for determining
		/// escape handling for first 128 characters of Unicode (ASCII
		/// characters.
		/// </summary>
		/// <remarks>
		/// Method generators can call to get lookup table for determining
		/// escape handling for first 128 characters of Unicode (ASCII
		/// characters. Caller is not to modify contents of this array, since
		/// this is expected to be a shared copy.
		/// </remarks>
		/// <returns>
		/// Array with size of at least 128, where first 128 entries
		/// have either one of <code>ESCAPE_xxx</code> constants, or non-zero positive
		/// integer (meaning of which is data format specific; for JSON it means
		/// that combination of backslash and character with that value is to be used)
		/// to indicate that specific escape sequence is to be used.
		/// </returns>
		public abstract int[] getEscapeCodesForAscii();

		/// <summary>
		/// Method generators can call to get lookup table for determining
		/// exact escape sequence to use for given character.
		/// </summary>
		/// <remarks>
		/// Method generators can call to get lookup table for determining
		/// exact escape sequence to use for given character.
		/// It can be called for any character, but typically is called for
		/// either for ASCII characters for which custom escape
		/// sequence is needed; or for any non-ASCII character.
		/// </remarks>
		public abstract com.fasterxml.jackson.core.SerializableString getEscapeSequence(int
			 ch);

		/// <summary>
		/// Helper method that can be used to get a copy of standard JSON
		/// escape definitions; this is useful when just wanting to slightly
		/// customize definitions.
		/// </summary>
		/// <remarks>
		/// Helper method that can be used to get a copy of standard JSON
		/// escape definitions; this is useful when just wanting to slightly
		/// customize definitions. Caller can modify this array as it sees
		/// fit and usually returns modified instance via
		/// <see cref="getEscapeCodesForAscii()"/>
		/// </remarks>
		public static int[] standardAsciiEscapesForJSON()
		{
			int[] esc = com.fasterxml.jackson.core.io.CharTypes.get7BitOutputEscapes();
			return java.util.Arrays.copyOf(esc, esc.Length);
		}
	}
}
