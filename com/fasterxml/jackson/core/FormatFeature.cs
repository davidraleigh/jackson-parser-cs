using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>Marker interface that is to be implemented by data format - specific features.
	/// 	</summary>
	/// <remarks>
	/// Marker interface that is to be implemented by data format - specific features.
	/// Interface used since Java Enums can not extend classes or other Enums, but
	/// they can implement interfaces; and as such we may be able to use limited
	/// amount of generic functionality.
	/// <p>
	/// Note that this type is only implemented by non-JSON formats:
	/// types
	/// <see cref="Feature"/>
	/// and
	/// <see cref="Feature"/>
	/// do NOT
	/// implement it. This is to make it easier to avoid ambiguity with method
	/// calls.
	/// </remarks>
	/// <since>2.6 (to be fully used in 2.7 and beyond)</since>
	public interface FormatFeature
	{
		/// <summary>Accessor for checking whether this feature is enabled by default.</summary>
		bool enabledByDefault();

		/// <summary>
		/// Returns bit mask for this feature instance; must be a single bit,
		/// that is of form <code>(1 &lt;&lt; N)</code>
		/// </summary>
		int getMask();

		/// <summary>Convenience method for checking whether feature is enabled in given bitmask
		/// 	</summary>
		bool enabledIn(int flags);
	}
}
