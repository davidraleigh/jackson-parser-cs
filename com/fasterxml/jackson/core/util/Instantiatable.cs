using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Add-on interface used to indicate things that may be "blueprint" objects
	/// which can not be used as is, but are used for creating usable per-process
	/// (serialization, deserialization) instances, using
	/// <see cref="Instantiatable{T}.createInstance()"/>
	/// method.
	/// <p>
	/// Note that some implementations may choose to implement
	/// <see cref="Instantiatable{T}.createInstance()"/>
	/// by simply returning 'this': this is acceptable if instances are stateless.
	/// </summary>
	/// <seealso cref="DefaultPrettyPrinter"/>
	/// <since>2.1</since>
	public interface Instantiatable<T>
	{
		/// <summary>
		/// Method called to ensure that we have a non-blueprint object to use;
		/// it is either this object (if stateless), or a newly created object
		/// with separate state.
		/// </summary>
		T createInstance();
	}
}
