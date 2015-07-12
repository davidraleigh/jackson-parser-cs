/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>Object that encapsulates versioning information of a component.</summary>
	/// <remarks>
	/// Object that encapsulates versioning information of a component.
	/// Version information includes not just version number but also
	/// optionally group and artifact ids of the component being versioned.
	/// <p>
	/// Note that optional group and artifact id properties are new with Jackson 2.0:
	/// if provided, they should align with Maven artifact information.
	/// </remarks>
	[System.Serializable]
	public class Version : Sharpen.Comparable<com.fasterxml.jackson.core.Version>
	{
		private const long serialVersionUID = 1L;

		private static readonly com.fasterxml.jackson.core.Version UNKNOWN_VERSION = new 
			com.fasterxml.jackson.core.Version(0, 0, 0, null, null, null);

		protected internal readonly int _majorVersion;

		protected internal readonly int _minorVersion;

		protected internal readonly int _patchLevel;

		protected internal readonly string _groupId;

		protected internal readonly string _artifactId;

		/// <summary>
		/// Additional information for snapshot versions; null for non-snapshot
		/// (release) versions.
		/// </summary>
		protected internal readonly string _snapshotInfo;

		/// <since>2.1</since>
		[System.ObsoleteAttribute(@"Use variant that takes group and artifact ids")]
		public Version(int major, int minor, int patchLevel, string snapshotInfo)
			: this(major, minor, patchLevel, snapshotInfo, null, null)
		{
		}

		public Version(int major, int minor, int patchLevel, string snapshotInfo, string 
			groupId, string artifactId)
		{
			_majorVersion = major;
			_minorVersion = minor;
			_patchLevel = patchLevel;
			_snapshotInfo = snapshotInfo;
			_groupId = (groupId == null) ? string.Empty : groupId;
			_artifactId = (artifactId == null) ? string.Empty : artifactId;
		}

		/// <summary>
		/// Method returns canonical "not known" version, which is used as version
		/// in cases where actual version information is not known (instead of null).
		/// </summary>
		public static com.fasterxml.jackson.core.Version unknownVersion()
		{
			return UNKNOWN_VERSION;
		}

		public virtual bool isUknownVersion()
		{
			return (this == UNKNOWN_VERSION);
		}

		public virtual bool isSnapshot()
		{
			return (_snapshotInfo != null && _snapshotInfo.Length > 0);
		}

		public virtual int getMajorVersion()
		{
			return _majorVersion;
		}

		public virtual int getMinorVersion()
		{
			return _minorVersion;
		}

		public virtual int getPatchLevel()
		{
			return _patchLevel;
		}

		public virtual string getGroupId()
		{
			return _groupId;
		}

		public virtual string getArtifactId()
		{
			return _artifactId;
		}

		public virtual string toFullString()
		{
			return _groupId + '/' + _artifactId + '/' + ToString();
		}

		public override string ToString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append(_majorVersion).Append('.');
			sb.Append(_minorVersion).Append('.');
			sb.Append(_patchLevel);
			if (isSnapshot())
			{
				sb.Append('-').Append(_snapshotInfo);
			}
			return sb.ToString();
		}

		public override int GetHashCode()
		{
			return _artifactId.GetHashCode() ^ _groupId.GetHashCode() + _majorVersion - _minorVersion
				 + _patchLevel;
		}

		public override bool Equals(object o)
		{
			if (o == this)
			{
				return true;
			}
			if (o == null)
			{
				return false;
			}
			if (o.GetType() != GetType())
			{
				return false;
			}
			com.fasterxml.jackson.core.Version other = (com.fasterxml.jackson.core.Version)o;
			return (other._majorVersion == _majorVersion) && (other._minorVersion == _minorVersion
				) && (other._patchLevel == _patchLevel) && other._artifactId.Equals(_artifactId)
				 && other._groupId.Equals(_groupId);
		}

		public virtual int compareTo(com.fasterxml.jackson.core.Version other)
		{
			if (other == this)
			{
				return 0;
			}
			int diff = string.CompareOrdinal(_groupId, other._groupId);
			if (diff == 0)
			{
				diff = string.CompareOrdinal(_artifactId, other._artifactId);
				if (diff == 0)
				{
					diff = _majorVersion - other._majorVersion;
					if (diff == 0)
					{
						diff = _minorVersion - other._minorVersion;
						if (diff == 0)
						{
							diff = _patchLevel - other._patchLevel;
						}
					}
				}
			}
			return diff;
		}
	}
}
