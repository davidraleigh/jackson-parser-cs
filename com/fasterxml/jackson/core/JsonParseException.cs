/* Jackson JSON-processor.
*
* Copyright (c) 2007- Tatu Saloranta, tatu.saloranta@iki.fi
*/
using Sharpen;

namespace com.fasterxml.jackson.core
{
	/// <summary>
	/// Exception type for parsing problems, used when non-well-formed content
	/// (content that does not conform to JSON syntax as per specification)
	/// is encountered.
	/// </summary>
	[System.Serializable]
	public class JsonParseException : com.fasterxml.jackson.core.JsonProcessingException
	{
		private const long serialVersionUID = 1L;

		public JsonParseException(string msg, com.fasterxml.jackson.core.JsonLocation loc
			)
			: base(msg, loc)
		{
		}

		public JsonParseException(string msg, com.fasterxml.jackson.core.JsonLocation loc
			, System.Exception root)
			: base(msg, loc, root)
		{
		}
	}
}
