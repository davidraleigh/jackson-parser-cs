/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Exception type for exceptions during JSON writing, such as trying
	/// to output  content in wrong context (non-matching end-array or end-object,
	/// for example).
	/// </summary>
	[System.Serializable]
	public class JsonGenerationException : com.fasterxml.jackson.core.JsonProcessingException
	{
		private const long serialVersionUID = 123;

		public JsonGenerationException(System.Exception rootCause)
			: base(rootCause)
		{
		}

		public JsonGenerationException(string msg)
			: base(msg, (com.fasterxml.jackson.core.JsonLocation)null)
		{
		}

		public JsonGenerationException(string msg, System.Exception rootCause)
			: base(msg, null, rootCause)
		{
		}
		// Stupid eclipse...
	}
}
