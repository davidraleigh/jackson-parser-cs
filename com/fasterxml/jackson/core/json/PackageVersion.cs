using Sharpen;

namespace com.fasterxml.jackson.core.json
{
	/// <summary>
	/// Automatically generated from PackageVersion.java.in during
	/// packageVersion-generate execution of maven-replacer-plugin in
	/// pom.xml.
	/// </summary>
	public sealed class PackageVersion : com.fasterxml.jackson.core.Versioned
	{
		public static readonly com.fasterxml.jackson.core.Version VERSION = com.fasterxml.jackson.core.util.VersionUtil
			.parseVersion("2", "5", "bullshit");

		public com.fasterxml.jackson.core.Version version()
		{
			return VERSION;
		}
	}
}
