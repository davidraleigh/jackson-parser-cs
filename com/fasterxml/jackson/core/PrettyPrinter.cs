/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Interface for objects that implement pretty printer functionality, such
	/// as indentation.
	/// </summary>
	/// <remarks>
	/// Interface for objects that implement pretty printer functionality, such
	/// as indentation.
	/// Pretty printers are used to add white space in output JSON content,
	/// to make results more human readable. Usually this means things like adding
	/// linefeeds and indentation.
	/// <p>
	/// Note: since Jackson 2.1, stateful implementations MUST implement
	/// <see cref="com.fasterxml.jackson.core.util.Instantiatable{T}"/>
	/// interface,
	/// to allow for constructing  per-generation instances and avoid
	/// state corruption (see [JACKSON-851] for details).
	/// Stateless implementations need not do this; but those are less common.
	/// </remarks>
	public interface PrettyPrinter
	{
		/*
		/**********************************************************
		/* First methods that act both as events, and expect
		/* output for correct functioning (i.e something gets
		/* output even when not pretty-printing)
		/**********************************************************
		*/
		// // // Root-level handling:
		/// <summary>
		/// Method called after a root-level value has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after a root-level value has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default
		/// handling (without pretty-printing) will output a space, to
		/// allow values to be parsed correctly. Pretty-printer is
		/// to output some other suitable and nice-looking separator
		/// (tab(s), space(s), linefeed(s) or any combination thereof).
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeRootValueSeparator(com.fasterxml.jackson.core.JsonGenerator jg);

		// // Object handling
		/// <summary>
		/// Method called when an Object value is to be output, before
		/// any fields are output.
		/// </summary>
		/// <remarks>
		/// Method called when an Object value is to be output, before
		/// any fields are output.
		/// <p>
		/// Default handling (without pretty-printing) will output
		/// the opening curly bracket.
		/// Pretty-printer is
		/// to output a curly bracket as well, but can surround that
		/// with other (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeStartObject(com.fasterxml.jackson.core.JsonGenerator gen);

		/// <summary>
		/// Method called after an Object value has been completely output
		/// (minus closing curly bracket).
		/// </summary>
		/// <remarks>
		/// Method called after an Object value has been completely output
		/// (minus closing curly bracket).
		/// <p>
		/// Default handling (without pretty-printing) will output
		/// the closing curly bracket.
		/// Pretty-printer is
		/// to output a curly bracket as well, but can surround that
		/// with other (white-space) decoration.
		/// </remarks>
		/// <param name="nrOfEntries">
		/// Number of direct members of the array that
		/// have been output
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeEndObject(com.fasterxml.jackson.core.JsonGenerator gen, int nrOfEntries
			);

		/// <summary>
		/// Method called after an object entry (field:value) has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after an object entry (field:value) has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// comma to separate the two. Pretty-printer is
		/// to output a comma as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeObjectEntrySeparator(com.fasterxml.jackson.core.JsonGenerator gen);

		/// <summary>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// </summary>
		/// <remarks>
		/// Method called after an object field has been output, but
		/// before the value is output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// colon to separate the two. Pretty-printer is
		/// to output a colon as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeObjectFieldValueSeparator(com.fasterxml.jackson.core.JsonGenerator gen);

		// // // Array handling
		/// <summary>
		/// Method called when an Array value is to be output, before
		/// any member/child values are output.
		/// </summary>
		/// <remarks>
		/// Method called when an Array value is to be output, before
		/// any member/child values are output.
		/// <p>
		/// Default handling (without pretty-printing) will output
		/// the opening bracket.
		/// Pretty-printer is
		/// to output a bracket as well, but can surround that
		/// with other (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeStartArray(com.fasterxml.jackson.core.JsonGenerator gen);

		/// <summary>
		/// Method called after an Array value has been completely output
		/// (minus closing bracket).
		/// </summary>
		/// <remarks>
		/// Method called after an Array value has been completely output
		/// (minus closing bracket).
		/// <p>
		/// Default handling (without pretty-printing) will output
		/// the closing bracket.
		/// Pretty-printer is
		/// to output a bracket as well, but can surround that
		/// with other (white-space) decoration.
		/// </remarks>
		/// <param name="nrOfValues">
		/// Number of direct members of the array that
		/// have been output
		/// </param>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeEndArray(com.fasterxml.jackson.core.JsonGenerator gen, int nrOfValues);

		/// <summary>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after an array value has been completely
		/// output, and before another value is to be output.
		/// <p>
		/// Default handling (without pretty-printing) will output a single
		/// comma to separate the two. Pretty-printer is
		/// to output a comma as well, but can surround that with other
		/// (white-space) decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void writeArrayValueSeparator(com.fasterxml.jackson.core.JsonGenerator gen);

		/*
		/**********************************************************
		/* Then events that by default do not produce any output
		/* but that are often overridden to add white space
		/* in pretty-printing mode
		/**********************************************************
		*/
		/// <summary>
		/// Method called after array start marker has been output,
		/// and right before the first value is to be output.
		/// </summary>
		/// <remarks>
		/// Method called after array start marker has been output,
		/// and right before the first value is to be output.
		/// It is <b>not</b> called for arrays with no values.
		/// <p>
		/// Default handling does not output anything, but pretty-printer
		/// is free to add any white space decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void beforeArrayValues(com.fasterxml.jackson.core.JsonGenerator gen);

		/// <summary>
		/// Method called after object start marker has been output,
		/// and right before the field name of the first entry is
		/// to be output.
		/// </summary>
		/// <remarks>
		/// Method called after object start marker has been output,
		/// and right before the field name of the first entry is
		/// to be output.
		/// It is <b>not</b> called for objects without entries.
		/// <p>
		/// Default handling does not output anything, but pretty-printer
		/// is free to add any white space decoration.
		/// </remarks>
		/// <exception cref="System.IO.IOException"/>
		/// <exception cref="com.fasterxml.jackson.core.JsonGenerationException"/>
		void beforeObjectEntries(com.fasterxml.jackson.core.JsonGenerator gen);
	}
}
