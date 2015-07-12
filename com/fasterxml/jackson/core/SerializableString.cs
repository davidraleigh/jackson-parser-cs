/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Interface that defines how Jackson package can interact with efficient
	/// pre-serialized or lazily-serialized and reused String representations.
	/// </summary>
	/// <remarks>
	/// Interface that defines how Jackson package can interact with efficient
	/// pre-serialized or lazily-serialized and reused String representations.
	/// Typically implementations store possible serialized version(s) so that
	/// serialization of String can be done more efficiently, especially when
	/// used multiple times.
	/// <p>
	/// Note that "quoted" in methods means quoting of 'special' characters using
	/// JSON backlash notation (and not use of actual double quotes).
	/// </remarks>
	/// <seealso cref="com.fasterxml.jackson.core.io.SerializedString"/>
	public interface SerializableString
	{
		/// <summary>
		/// Returns unquoted String that this object represents (and offers
		/// serialized forms for)
		/// </summary>
		string getValue();

		/// <summary>Returns length of the (unquoted) String as characters.</summary>
		/// <remarks>
		/// Returns length of the (unquoted) String as characters.
		/// Functionally equvalent to:
		/// <pre>
		/// getValue().length();
		/// </pre>
		/// </remarks>
		int charLength();

		/*
		/**********************************************************
		/* Accessors for byte sequences
		/**********************************************************
		*/
		/// <summary>Returns JSON quoted form of the String, as character array.</summary>
		/// <remarks>
		/// Returns JSON quoted form of the String, as character array.
		/// Result can be embedded as-is in textual JSON as property name or JSON String.
		/// </remarks>
		char[] asQuotedChars();

		/// <summary>Returns UTF-8 encoded version of unquoted String.</summary>
		/// <remarks>
		/// Returns UTF-8 encoded version of unquoted String.
		/// Functionally equivalent to (but more efficient than):
		/// <pre>
		/// getValue().getBytes("UTF-8");
		/// </pre>
		/// </remarks>
		byte[] asUnquotedUTF8();

		/// <summary>Returns UTF-8 encoded version of JSON-quoted String.</summary>
		/// <remarks>
		/// Returns UTF-8 encoded version of JSON-quoted String.
		/// Functionally equivalent to (but more efficient than):
		/// <pre>
		/// new String(asQuotedChars()).getBytes("UTF-8");
		/// </pre>
		/// </remarks>
		byte[] asQuotedUTF8();

		/*
		/**********************************************************
		/* Helper methods for appending byte/char sequences
		/**********************************************************
		*/
		/// <summary>
		/// Method that will append quoted UTF-8 bytes of this String into given
		/// buffer, if there is enough room; if not, returns -1.
		/// </summary>
		/// <remarks>
		/// Method that will append quoted UTF-8 bytes of this String into given
		/// buffer, if there is enough room; if not, returns -1.
		/// Functionally equivalent to:
		/// <pre>
		/// byte[] bytes = str.asQuotedUTF8();
		/// System.arraycopy(bytes, 0, buffer, offset, bytes.length);
		/// return bytes.length;
		/// </pre>
		/// </remarks>
		/// <returns>Number of bytes appended, if successful, otherwise -1</returns>
		int appendQuotedUTF8(byte[] buffer, int offset);

		/// <summary>
		/// Method that will append quoted characters of this String into given
		/// buffer.
		/// </summary>
		/// <remarks>
		/// Method that will append quoted characters of this String into given
		/// buffer. Functionally equivalent to:
		/// <pre>
		/// char[] ch = str.asQuotedChars();
		/// System.arraycopy(ch, 0, buffer, offset, ch.length);
		/// return ch.length;
		/// </pre>
		/// </remarks>
		/// <returns>Number of characters appended, if successful, otherwise -1</returns>
		int appendQuoted(char[] buffer, int offset);

		/// <summary>
		/// Method that will append unquoted ('raw') UTF-8 bytes of this String into given
		/// buffer.
		/// </summary>
		/// <remarks>
		/// Method that will append unquoted ('raw') UTF-8 bytes of this String into given
		/// buffer. Functionally equivalent to:
		/// <pre>
		/// byte[] bytes = str.asUnquotedUTF8();
		/// System.arraycopy(bytes, 0, buffer, offset, bytes.length);
		/// return bytes.length;
		/// </pre>
		/// </remarks>
		/// <returns>Number of bytes appended, if successful, otherwise -1</returns>
		int appendUnquotedUTF8(byte[] buffer, int offset);

		/// <summary>
		/// Method that will append unquoted characters of this String into given
		/// buffer.
		/// </summary>
		/// <remarks>
		/// Method that will append unquoted characters of this String into given
		/// buffer. Functionally equivalent to:
		/// <pre>
		/// char[] ch = str.getValue().toCharArray();
		/// System.arraycopy(bytes, 0, buffer, offset, ch.length);
		/// return ch.length;
		/// </pre>
		/// </remarks>
		/// <returns>Number of characters appended, if successful, otherwise -1</returns>
		int appendUnquoted(char[] buffer, int offset);

		/*
		/**********************************************************
		/* Helper methods for writing out byte sequences
		/**********************************************************
		*/
		/// <returns>Number of bytes written</returns>
		/// <exception cref="System.IO.IOException"/>
		int writeQuotedUTF8(Sharpen.OutputStream @out);

		/// <returns>Number of bytes written</returns>
		/// <exception cref="System.IO.IOException"/>
		int writeUnquotedUTF8(Sharpen.OutputStream @out);

		/// <returns>Number of bytes put, if successful, otherwise -1</returns>
		/// <exception cref="System.IO.IOException"/>
		int putQuotedUTF8(Sharpen.ByteBuffer buffer);

		/// <returns>Number of bytes put, if successful, otherwise -1</returns>
		/// <exception cref="System.IO.IOException"/>
		int putUnquotedUTF8(Sharpen.ByteBuffer @out);
	}
}
