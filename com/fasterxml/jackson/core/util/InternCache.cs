using Sharpen;

namespace com.fasterxml.jackson.core.util
{
	/// <summary>
	/// Singleton class that adds a simple first-level cache in front of
	/// regular String.intern() functionality.
	/// </summary>
	/// <remarks>
	/// Singleton class that adds a simple first-level cache in front of
	/// regular String.intern() functionality. This is done as a minor
	/// performance optimization, to avoid calling native intern() method
	/// in cases where same String is being interned multiple times.
	/// <p>
	/// Note: that this class extends
	/// <see cref="java.util.LinkedHashMap{K, V}"/>
	/// is an implementation
	/// detail -- no code should ever directly call Map methods.
	/// </remarks>
	[System.Serializable]
	public sealed class InternCache : java.util.concurrent.ConcurrentHashMap<string, 
		string>
	{
		/// <summary>
		/// Size to use is somewhat arbitrary, so let's choose something that's
		/// neither too small (low hit ratio) nor too large (waste of memory).
		/// </summary>
		/// <remarks>
		/// Size to use is somewhat arbitrary, so let's choose something that's
		/// neither too small (low hit ratio) nor too large (waste of memory).
		/// <p>
		/// One consideration is possible attack via colliding
		/// <see cref="string.GetHashCode()"/>
		/// ;
		/// because of this, limit to reasonably low setting.
		/// </remarks>
		private const int MAX_ENTRIES = 180;

		public static readonly com.fasterxml.jackson.core.util.InternCache instance = new 
			com.fasterxml.jackson.core.util.InternCache();

		/// <summary>
		/// As minor optimization let's try to avoid "flush storms",
		/// cases where multiple threads might try to concurrently
		/// flush the map.
		/// </summary>
		private readonly object Lock = new object();

		private InternCache()
			: base(MAX_ENTRIES, 0.8f, 4)
		{
		}

		// since 2.3
		public string intern(string input)
		{
			string result = this[input];
			if (result != null)
			{
				return result;
			}
			/* 18-Sep-2013, tatu: We used to use LinkedHashMap, which has simple LRU
			*   method. No such functionality exists with CHM; and let's use simplest
			*   possible limitation: just clear all contents. This because otherwise
			*   we are simply likely to keep on clearing same, commonly used entries.
			*/
			if (Count >= MAX_ENTRIES)
			{
				/* Not incorrect wrt well-known double-locking anti-pattern because underlying
				* storage gives close enough answer to real one here; and we are
				* more concerned with flooding than starvation.
				*/
				lock (Lock)
				{
					if (Count >= MAX_ENTRIES)
					{
						clear();
					}
				}
			}
			result = string.Intern(input);
			this[result] = result;
			return result;
		}
	}
}
