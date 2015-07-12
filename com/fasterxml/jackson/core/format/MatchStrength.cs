using Sharpen;

namespace com.fasterxml.jackson.core.format
{
	/// <summary>
	/// Enumeration used to indicate strength of match between data format
	/// and piece of data (typically beginning of a data file).
	/// </summary>
	/// <remarks>
	/// Enumeration used to indicate strength of match between data format
	/// and piece of data (typically beginning of a data file).
	/// Values are in increasing match strength; and detectors should return
	/// "strongest" value: that is, it should start with strongest match
	/// criteria, and downgrading if criteria is not fulfilled.
	/// </remarks>
	public enum MatchStrength
	{
		NO_MATCH,
		INCONCLUSIVE,
		WEAK_MATCH,
		SOLID_MATCH,
		FULL_MATCH
	}
}
