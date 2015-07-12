using Sharpen;

namespace com.fasterxml.jackson.core.io
{
	/// <summary>Handler class that can be used to decorate input sources.</summary>
	/// <remarks>
	/// Handler class that can be used to decorate input sources.
	/// Typical use is to use a filter abstraction (filtered stream,
	/// reader) around original input source, and apply additional
	/// processing during read operations.
	/// </remarks>
	[System.Serializable]
	public abstract class InputDecorator
	{
		private const long serialVersionUID = 1L;

		// since 2.1
		/// <summary>
		/// Method called by
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// instance when
		/// creating parser given an
		/// <see cref="Sharpen.InputStream"/>
		/// , when this decorator
		/// has been registered.
		/// </summary>
		/// <param name="ctxt">
		/// IO context in use (provides access to declared encoding).
		/// NOTE: at this point context may not have all information initialized;
		/// specifically auto-detected encoding is only available once parsing starts,
		/// which may occur only after this method is called.
		/// </param>
		/// <param name="in">Original input source</param>
		/// <returns>
		/// InputStream to use; either 'in' as is, or decorator
		/// version that typically delogates to 'in'
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract Sharpen.InputStream decorate(com.fasterxml.jackson.core.io.IOContext
			 ctxt, Sharpen.InputStream @in);

		/// <summary>
		/// Method called by
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// instance when
		/// creating parser on given "raw" byte source.
		/// Method can either construct a
		/// <see cref="Sharpen.InputStream"/>
		/// for reading; or return
		/// null to indicate that no wrapping should occur.
		/// </summary>
		/// <param name="ctxt">
		/// IO context in use (provides access to declared encoding)
		/// NOTE: at this point context may not have all information initialized;
		/// specifically auto-detected encoding is only available once parsing starts,
		/// which may occur only after this method is called.
		/// </param>
		/// <param name="src">Input buffer that contains contents to parse</param>
		/// <param name="offset">Offset of the first available byte in the input buffer</param>
		/// <param name="length">Number of bytes available in the input buffer</param>
		/// <returns>
		/// Either
		/// <see cref="Sharpen.InputStream"/>
		/// to use as input source; or null to indicate
		/// that contents are to be processed as-is by caller
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract Sharpen.InputStream decorate(com.fasterxml.jackson.core.io.IOContext
			 ctxt, byte[] src, int offset, int length);

		/// <summary>
		/// Method called by
		/// <see cref="com.fasterxml.jackson.core.JsonFactory"/>
		/// instance when
		/// creating parser given an
		/// <see cref="System.IO.StreamReader"/>
		/// , when this decorator
		/// has been registered.
		/// </summary>
		/// <param name="ctxt">
		/// IO context in use (provides access to declared encoding)
		/// NOTE: at this point context may not have all information initialized;
		/// specifically auto-detected encoding is only available once parsing starts,
		/// which may occur only after this method is called.
		/// </param>
		/// <param name="r">Original reader</param>
		/// <returns>
		/// Reader to use; either passed in argument, or something that
		/// calls it (for example, a
		/// <see cref="Sharpen.FilterReader"/>
		/// )
		/// </returns>
		/// <exception cref="System.IO.IOException"/>
		public abstract System.IO.StreamReader decorate(com.fasterxml.jackson.core.io.IOContext
			 ctxt, System.IO.StreamReader r);
	}
}
