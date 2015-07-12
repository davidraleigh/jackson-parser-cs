using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>Handler class that can be used to decorate output destinations.</summary>
	/// <remarks>
	/// Handler class that can be used to decorate output destinations.
	/// Typical use is to use a filter abstraction (filtered output stream,
	/// writer) around original output destination, and apply additional
	/// processing during write operations.
	/// </remarks>
	[System.Serializable]
	public abstract class OutputDecorator
	{
		// since 2.1
		/// <summary>
		/// Method called by
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// instance when
		/// creating generator for given
		/// <see cref="Sharpen.OutputStream"/>
		/// , when this decorator
		/// has been registered.
		/// </summary>
		/// <param name="ctxt">IO context in use (provides access to declared encoding)</param>
		/// <param name="out">Original output destination</param>
		/// <returns>
		/// OutputStream to use; either passed in argument, or something that
		/// calls it
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract Sharpen.OutputStream decorate(com.fasterxml.jackson.core.io.IOContext
			 ctxt, Sharpen.OutputStream @out);

		/// <summary>
		/// Method called by
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// instance when
		/// creating generator for given
		/// <see cref="System.IO.TextWriter"/>
		/// , when this decorator
		/// has been registered.
		/// </summary>
		/// <param name="ctxt">IO context in use (provides access to declared encoding)</param>
		/// <param name="w">Original output writer</param>
		/// <returns>Writer to use; either passed in argument, or something that calls it</returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract System.IO.TextWriter decorate(com.fasterxml.jackson.core.io.IOContext
			 ctxt, System.IO.TextWriter w);
	}
}
