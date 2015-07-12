using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Functionality for supporting exposing of component
	/// <see cref="com.fasterxml.jackson.core.Version"/>
	/// s.
	/// Also contains other misc methods that have no other place to live in.
	/// <p>
	/// Note that this class can be used in two roles: first, as a static
	/// utility class for loading purposes, and second, as a singleton
	/// loader of per-module version information.
	/// <p>
	/// Note that method for accessing version information changed between versions
	/// 2.1 and 2.2; earlier code used file named "VERSION.txt"; but this has serious
	/// performance issues on some platforms (Android), so a replacement system
	/// was implemented to use class generation and dynamic class loading.
	/// <p>
	/// Note that functionality for reading "VERSION.txt" was removed completely
	/// from Jackson 2.6.
	/// </summary>
	public class VersionUtil
	{
		private static readonly Sharpen.Pattern V_SEP = Sharpen.Pattern.compile("[-_./;:]"
			);

		private readonly com.fasterxml.jackson.core.Version _v;

		protected internal VersionUtil()
		{
			/*
			/**********************************************************
			/* Instance life-cycle
			/**********************************************************
			*/
			com.fasterxml.jackson.core.Version v = null;
			try
			{
				/* Class we pass only matters for resource-loading: can't use this Class
				* (as it's just being loaded at this point), nor anything that depends on it.
				*/
				v = com.fasterxml.jackson.core.util.VersionUtil.versionFor(GetType());
			}
			catch (System.Exception)
			{
				// not good to dump to stderr; but that's all we have at this low level
				System.Console.Error.WriteLine("ERROR: Failed to load Version information from " 
					+ GetType());
			}
			if (v == null)
			{
				v = com.fasterxml.jackson.core.Version.unknownVersion();
			}
			_v = v;
		}

		public virtual com.fasterxml.jackson.core.Version version()
		{
			return _v;
		}

		/*
		/**********************************************************
		/* Static load methods
		/**********************************************************
		*/
		/// <summary>
		/// Helper method that will try to load version information for specified
		/// class.
		/// </summary>
		/// <remarks>
		/// Helper method that will try to load version information for specified
		/// class. Implementation is as follows:
		/// First, tries to load version info from a class named
		/// "PackageVersion" in the same package as the class.
		/// If no version information is found,
		/// <see cref="com.fasterxml.jackson.core.Version.unknownVersion()"/>
		/// is returned.
		/// </remarks>
		public static com.fasterxml.jackson.core.Version versionFor(System.Type cls)
		{
			return packageVersionFor(cls);
		}

		/// <summary>
		/// Loads version information by introspecting a class named
		/// "PackageVersion" in the same package as the given class.
		/// </summary>
		/// <remarks>
		/// Loads version information by introspecting a class named
		/// "PackageVersion" in the same package as the given class.
		/// If the class could not be found or does not have a public
		/// static Version field named "VERSION", returns null.
		/// </remarks>
		public static com.fasterxml.jackson.core.Version packageVersionFor(System.Type cls
			)
		{
			try
			{
				string versionInfoClassName = cls.Assembly.getName() + ".PackageVersion";
				System.Type vClass = Sharpen.Runtime.GetType(versionInfoClassName, true, cls.getClassLoader
					());
				// However, if class exists, it better work correctly, no swallowing exceptions
				try
				{
					return ((com.fasterxml.jackson.core.Versioned)System.Activator.CreateInstance(vClass
						)).version();
				}
				catch (System.Exception)
				{
					throw new System.ArgumentException("Failed to get Versioned out of " + vClass);
				}
			}
			catch (System.Exception)
			{
				// ok to be missing (not good but acceptable)
				return null;
			}
		}

		/// <summary>
		/// Will attempt to load the maven version for the given groupId and
		/// artifactId.
		/// </summary>
		/// <remarks>
		/// Will attempt to load the maven version for the given groupId and
		/// artifactId.  Maven puts a pom.properties file in
		/// META-INF/maven/groupId/artifactId, containing the groupId,
		/// artifactId and version of the library.
		/// </remarks>
		/// <param name="cl">the ClassLoader to load the pom.properties file from</param>
		/// <param name="groupId">the groupId of the library</param>
		/// <param name="artifactId">the artifactId of the library</param>
		/// <returns>The version</returns>
		[System.ObsoleteAttribute(@"Since 2.6: functionality not used by any official Jackson component, should be moved out if anyone needs it"
			)]
		public static com.fasterxml.jackson.core.Version mavenVersionFor(Sharpen.ClassLoader
			 cl, string groupId, string artifactId)
		{
			// since 2.6
			Sharpen.InputStream pomProperties = cl.getResourceAsStream("META-INF/maven/" + groupId
				.replaceAll("\\.", "/") + "/" + artifactId + "/pom.properties");
			if (pomProperties != null)
			{
				try
				{
					Sharpen.Properties props = new Sharpen.Properties();
					props.load(pomProperties);
					string versionStr = props.getProperty("version");
					string pomPropertiesArtifactId = props.getProperty("artifactId");
					string pomPropertiesGroupId = props.getProperty("groupId");
					return parseVersion(versionStr, pomPropertiesGroupId, pomPropertiesArtifactId);
				}
				catch (System.IO.IOException)
				{
				}
				finally
				{
					// Ignore
					_close(pomProperties);
				}
			}
			return com.fasterxml.jackson.core.Version.unknownVersion();
		}

		/// <summary>Method used by <code>PackageVersion</code> classes to decode version injected by Maven build.
		/// 	</summary>
		public static com.fasterxml.jackson.core.Version parseVersion(string s, string groupId
			, string artifactId)
		{
			if (s != null && (s = Sharpen.Extensions.Trim(s)).Length > 0)
			{
				string[] parts = V_SEP.split(s);
				return new com.fasterxml.jackson.core.Version(parseVersionPart(parts[0]), (parts.
					Length > 1) ? parseVersionPart(parts[1]) : 0, (parts.Length > 2) ? parseVersionPart
					(parts[2]) : 0, (parts.Length > 3) ? parts[3] : null, groupId, artifactId);
			}
			return null;
		}

		protected internal static int parseVersionPart(string s)
		{
			int number = 0;
			for (int i = 0; i < len; ++i)
			{
				char c = s[i];
				if (c > '9' || c < '0')
				{
					break;
				}
				number = (number * 10) + (c - '0');
			}
			return number;
		}

		private static void _close(System.IDisposable c)
		{
			try
			{
				c.close();
			}
			catch (System.IO.IOException)
			{
			}
		}

		/*
		/**********************************************************
		/* Orphan utility methods
		/**********************************************************
		*/
		public static void throwInternal()
		{
			throw new Sharpen.RuntimeException("Internal error: this code path should never get executed"
				);
		}
	}
}
