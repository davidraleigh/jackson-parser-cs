/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>Interface that those Jackson components that are explicitly versioned will implement.
	/// 	</summary>
	/// <remarks>
	/// Interface that those Jackson components that are explicitly versioned will implement.
	/// Intention is to allow both plug-in components (custom extensions) and applications and
	/// frameworks that use Jackson to detect exact version of Jackson in use.
	/// This may be useful for example for ensuring that proper Jackson version is deployed
	/// (beyond mechanisms that deployment system may have), as well as for possible
	/// workarounds.
	/// </remarks>
	public interface Versioned
	{
		/// <summary>
		/// Method called to detect version of the component that implements this interface;
		/// returned version should never be null, but may return specific "not available"
		/// instance (see
		/// <see cref="Version"/>
		/// for details).
		/// </summary>
		com.fasterxml.jackson.core.Version version();
	}
}
