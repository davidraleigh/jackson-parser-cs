using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// A caching symbol table implementation used for canonicalizing JSON field
	/// names (as
	/// <see cref="Name"/>
	/// s which are constructed directly from a byte-based
	/// input source).
	/// Complications arise from trying to do efficient reuse and merging of
	/// symbol tables, to be able to make use of usually shared vocabulary
	/// of subsequent parsing runs.
	/// </summary>
	[System.ObsoleteAttribute(@"Since 2.6, replaced by ByteQuadsCanonicalizer")]
	public sealed class BytesToNameCanonicalizer
	{
		private const int DEFAULT_T_SIZE = 64;

		/// <summary>
		/// Let's not expand symbol tables past some maximum size;
		/// this should protected against OOMEs caused by large documents
		/// with unique (~= random) names.
		/// </summary>
		private const int MAX_T_SIZE = unchecked((int)(0x10000));

		/// <summary>Let's only share reasonably sized symbol tables.</summary>
		/// <remarks>
		/// Let's only share reasonably sized symbol tables. Max size set to 3/4 of 16k;
		/// this corresponds to 64k main hash index. This should allow for enough distinct
		/// names for almost any case.
		/// </remarks>
		internal const int MAX_ENTRIES_FOR_REUSE = 6000;

		/// <summary>
		/// Also: to thwart attacks based on hash collisions (which may or may not
		/// be cheap to calculate), we will need to detect "too long"
		/// collision chains.
		/// </summary>
		/// <remarks>
		/// Also: to thwart attacks based on hash collisions (which may or may not
		/// be cheap to calculate), we will need to detect "too long"
		/// collision chains.
		/// <p>
		/// Note: longest chain we have been able to produce without malicious
		/// intent has been 10 (with "com.fasterxml.jackson.core.sym.TestSymbolTables");
		/// our setting should be reasonable here. Also note that overflow
		/// chains are shared between multiple primary cells, which could cause
		/// problems for lower values.
		/// <p>
		/// Also note that value was lowered from 255 (2.3 and earlier) to 100 for 2.4,
		/// but raised again to 200 for 2.5.2 (as per [core#187])
		/// </remarks>
		/// <since>2.1</since>
		private const int MAX_COLL_CHAIN_LENGTH = 200;

		/// <summary>No point in trying to construct tiny tables, just need to resize soon.</summary>
		internal const int MIN_HASH_SIZE = 16;

		/// <summary>
		/// We will also need to define initial size for collision list,
		/// when copying it.
		/// </summary>
		internal const int INITIAL_COLLISION_LEN = 32;

		/// <summary>
		/// Bucket index is 8 bits, and value 0 is reserved to represent
		/// 'empty' status.
		/// </summary>
		internal const int LAST_VALID_BUCKET = unchecked((int)(0xFE));

		/// <summary>
		/// Reference to the root symbol table, for child tables, so
		/// that they can merge table information back as necessary.
		/// </summary>
		protected internal readonly com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer
			 _parent;

		/// <summary>
		/// Member that is only used by the root table instance: root
		/// passes immutable state into child instances, and children
		/// may return new state if they add entries to the table.
		/// </summary>
		/// <remarks>
		/// Member that is only used by the root table instance: root
		/// passes immutable state into child instances, and children
		/// may return new state if they add entries to the table.
		/// Child tables do NOT use the reference.
		/// </remarks>
		protected internal readonly java.util.concurrent.atomic.AtomicReference<com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo
			> _tableInfo;

		/// <summary>
		/// Seed value we use as the base to make hash codes non-static between
		/// different runs, but still stable for lifetime of a single symbol table
		/// instance.
		/// </summary>
		/// <remarks>
		/// Seed value we use as the base to make hash codes non-static between
		/// different runs, but still stable for lifetime of a single symbol table
		/// instance.
		/// This is done for security reasons, to avoid potential DoS attack via
		/// hash collisions.
		/// </remarks>
		/// <since>2.1</since>
		private readonly int _seed;

		/// <summary>
		/// Whether canonical symbol Strings are to be intern()ed before added
		/// to the table or not.
		/// </summary>
		/// <remarks>
		/// Whether canonical symbol Strings are to be intern()ed before added
		/// to the table or not.
		/// <p>
		/// NOTE: non-final to allow disabling intern()ing in case of excessive
		/// collisions.
		/// </remarks>
		protected internal bool _intern;

		/// <summary>
		/// Flag that indicates whether we should throw an exception if enough
		/// hash collisions are detected (true); or just worked around (false).
		/// </summary>
		/// <since>2.4</since>
		protected internal readonly bool _failOnDoS;

		/// <summary>
		/// Total number of Names in the symbol table;
		/// only used for child tables.
		/// </summary>
		protected internal int _count;

		/// <summary>
		/// We need to keep track of the longest collision list; this is needed
		/// both to indicate problems with attacks and to allow flushing for
		/// other cases.
		/// </summary>
		/// <since>2.1</since>
		protected internal int _longestCollisionList;

		/// <summary>
		/// Mask used to truncate 32-bit hash value to current hash array
		/// size; essentially, hash array size - 1 (since hash array sizes
		/// are 2^N).
		/// </summary>
		protected internal int _hashMask;

		/// <summary>
		/// Array of 2^N size, which contains combination
		/// of 24-bits of hash (0 to indicate 'empty' slot),
		/// and 8-bit collision bucket index (0 to indicate empty
		/// collision bucket chain; otherwise subtract one from index)
		/// </summary>
		protected internal int[] _hash;

		/// <summary>
		/// Array that contains <code>Name</code> instances matching
		/// entries in <code>_mainHash</code>.
		/// </summary>
		/// <remarks>
		/// Array that contains <code>Name</code> instances matching
		/// entries in <code>_mainHash</code>. Contains nulls for unused
		/// entries.
		/// </remarks>
		protected internal com.fasterxml.jackson.core.sym.Name[] _mainNames;

		/// <summary>Array of heads of collision bucket chains; size dynamically</summary>
		protected internal com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket
			[] _collList;

		/// <summary>
		/// Total number of Names in collision buckets (included in
		/// <code>_count</code> along with primary entries)
		/// </summary>
		protected internal int _collCount;

		/// <summary>
		/// Index of the first unused collision bucket entry (== size of
		/// the used portion of collision list): less than
		/// or equal to 0xFF (255), since max number of entries is 255
		/// (8-bit, minus 0 used as 'empty' marker)
		/// </summary>
		protected internal int _collEnd;

		/// <summary>
		/// This flag is set if, after adding a new entry, it is deemed
		/// that a rehash is warranted if any more entries are to be added.
		/// </summary>
		[System.NonSerialized]
		private bool _needRehash;

		/// <summary>
		/// Flag that indicates whether underlying data structures for
		/// the main hash area are shared or not.
		/// </summary>
		/// <remarks>
		/// Flag that indicates whether underlying data structures for
		/// the main hash area are shared or not. If they are, then they
		/// need to be handled in copy-on-write way, i.e. if they need
		/// to be modified, a copy needs to be made first; at this point
		/// it will not be shared any more, and can be modified.
		/// <p>
		/// This flag needs to be checked both when adding new main entries,
		/// and when adding new collision list queues (i.e. creating a new
		/// collision list head entry)
		/// </remarks>
		private bool _hashShared;

		private bool _namesShared;

		/// <summary>
		/// Flag that indicates whether underlying data structures for
		/// the collision list are shared or not.
		/// </summary>
		/// <remarks>
		/// Flag that indicates whether underlying data structures for
		/// the collision list are shared or not. If they are, then they
		/// need to be handled in copy-on-write way, i.e. if they need
		/// to be modified, a copy needs to be made first; at this point
		/// it will not be shared any more, and can be modified.
		/// <p>
		/// This flag needs to be checked when adding new collision entries.
		/// </remarks>
		private bool _collListShared;

		/// <summary>
		/// Lazily constructed structure that is used to keep track of
		/// collision buckets that have overflowed once: this is used
		/// to detect likely attempts at denial-of-service attacks that
		/// uses hash collisions.
		/// </summary>
		/// <since>2.4</since>
		protected internal Sharpen.BitSet _overflows;

		/// <summary>
		/// Constructor used for creating per-<code>JsonFactory</code> "root"
		/// symbol tables: ones used for merging and sharing common symbols
		/// </summary>
		/// <param name="sz">Initial hash area size</param>
		/// <param name="intern">
		/// Whether Strings contained should be
		/// <see cref="string.Intern()"/>
		/// ed
		/// </param>
		/// <param name="seed">
		/// Random seed valued used to make it more difficult to cause
		/// collisions (used for collision-based DoS attacks).
		/// </param>
		private BytesToNameCanonicalizer(int sz, bool intern, int seed, bool failOnDoS)
		{
			// 64k entries == 256k mem
			/*
			/**********************************************************
			/* Linkage, needed for merging symbol tables
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Configuration
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Main table state
			/**********************************************************
			*/
			// // // First, global information
			// // // Then information regarding primary hash array and its
			// // // matching Name array
			// // // Then the collision/spill-over area info
			// // // Info regarding pending rehashing...
			/*
			/**********************************************************
			/* Sharing, versioning
			/**********************************************************
			*/
			// // // Which of the buffers may be shared (and are copy-on-write)?
			/*
			/**********************************************************
			/* Bit of DoS detection goodness
			/**********************************************************
			*/
			/*
			/**********************************************************
			/* Life-cycle: constructors
			/**********************************************************
			*/
			_parent = null;
			_seed = seed;
			_intern = intern;
			_failOnDoS = failOnDoS;
			// Sanity check: let's now allow hash sizes below certain minimum value
			if (sz < MIN_HASH_SIZE)
			{
				sz = MIN_HASH_SIZE;
			}
			else
			{
				/* Also; size must be 2^N; otherwise hash algorithm won't
				* work... so let's just pad it up, if so
				*/
				if ((sz & (sz - 1)) != 0)
				{
					// only true if it's 2^N
					int curr = MIN_HASH_SIZE;
					while (curr < sz)
					{
						curr += curr;
					}
					sz = curr;
				}
			}
			_tableInfo = new java.util.concurrent.atomic.AtomicReference<com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo
				>(initTableInfo(sz));
		}

		/// <summary>Constructor used when creating a child instance</summary>
		private BytesToNameCanonicalizer(com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer
			 parent, bool intern, int seed, bool failOnDoS, com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo
			 state)
		{
			_parent = parent;
			_seed = seed;
			_intern = intern;
			_failOnDoS = failOnDoS;
			_tableInfo = null;
			// not used by child tables
			// Then copy shared state
			_count = state.count;
			_hashMask = state.mainHashMask;
			_hash = state.mainHash;
			_mainNames = state.mainNames;
			_collList = state.collList;
			_collCount = state.collCount;
			_collEnd = state.collEnd;
			_longestCollisionList = state.longestCollisionList;
			// and then set other state to reflect sharing status
			_needRehash = false;
			_hashShared = true;
			_namesShared = true;
			_collListShared = true;
		}

		/*
		public TableInfo(int count, int mainHashMask, int[] mainHash, Name[] mainNames,
		Bucket[] collList, int collCount, int collEnd, int longestCollisionList)
		*/
		private com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo initTableInfo
			(int sz)
		{
			return new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo(0, sz
				 - 1, new int[sz], new com.fasterxml.jackson.core.sym.Name[sz], null, 0, 0, 0);
		}

		// count
		// mainHashMask
		// mainHash
		// mainNames
		// collList
		// collCount,
		// collEnd
		// longestCollisionList
		/*
		/**********************************************************
		/* Life-cycle: factory methods, merging
		/**********************************************************
		*/
		/// <summary>
		/// Factory method to call to create a symbol table instance with a
		/// randomized seed value.
		/// </summary>
		public static com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer createRoot(
			)
		{
			/* [Issue-21]: Need to use a variable seed, to thwart hash-collision
			* based attacks.
			*/
			long now = Sharpen.Runtime.currentTimeMillis();
			// ensure it's not 0; and might as well require to be odd so:
			int seed = (((int)now) + ((int)((long)(((ulong)now) >> 32)))) | 1;
			return createRoot(seed);
		}

		/// <summary>
		/// Factory method that should only be called from unit tests, where seed
		/// value should remain the same.
		/// </summary>
		protected internal static com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer
			 createRoot(int seed)
		{
			return new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer(DEFAULT_T_SIZE
				, true, seed, true);
		}

		/// <summary>
		/// Factory method used to create actual symbol table instance to
		/// use for parsing.
		/// </summary>
		public com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer makeChild(int flags
			)
		{
			return new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer(this, com.fasterxml.jackson.core.JsonFactory.Feature
				.INTERN_FIELD_NAMES.enabledIn(flags), _seed, com.fasterxml.jackson.core.JsonFactory.Feature
				.FAIL_ON_SYMBOL_HASH_OVERFLOW.enabledIn(flags), _tableInfo.get());
		}

		[System.Obsolete]
		public com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer makeChild(bool canonicalize
			, bool intern)
		{
			// since 2.4
			return new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer(this, intern, 
				_seed, true, _tableInfo.get());
		}

		// JsonFactory.Feature.FAIL_ON_SYMBOL_HASH_OVERFLOW
		/// <summary>
		/// Method called by the using code to indicate it is done
		/// with this instance.
		/// </summary>
		/// <remarks>
		/// Method called by the using code to indicate it is done
		/// with this instance. This lets instance merge accumulated
		/// changes into parent (if need be), safely and efficiently,
		/// and without calling code having to know about parent
		/// information
		/// </remarks>
		public void release()
		{
			// we will try to merge if child table has new entries
			if (_parent != null && maybeDirty())
			{
				_parent.mergeChild(new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo
					(this));
				/* Let's also mark this instance as dirty, so that just in
				* case release was too early, there's no corruption of possibly shared data.
				*/
				_hashShared = true;
				_namesShared = true;
				_collListShared = true;
			}
		}

		private void mergeChild(com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo
			 childState)
		{
			int childCount = childState.count;
			com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.TableInfo currState = _tableInfo
				.get();
			/* Should usually grow; but occasionally could also shrink if
			* (but only if) collision list overflow ends up clearing
			* some collision lists.
			*/
			if (childCount == currState.count)
			{
				return;
			}
			/* One caveat: let's try to avoid problems with
			* degenerate cases of documents with generated "random"
			* names: for these, symbol tables would bloat indefinitely.
			* One way to do this is to just purge tables if they grow
			* too large, and that's what we'll do here.
			*/
			if (childCount > MAX_ENTRIES_FOR_REUSE)
			{
				/* Should there be a way to get notified about this
				* event, to log it or such? (as it's somewhat abnormal
				* thing to happen)
				*/
				// At any rate, need to clean up the tables
				childState = initTableInfo(DEFAULT_T_SIZE);
			}
			_tableInfo.compareAndSet(currState, childState);
		}

		/*
		/**********************************************************
		/* API, accessors
		/**********************************************************
		*/
		public int size()
		{
			if (_tableInfo != null)
			{
				// root table
				return _tableInfo.get().count;
			}
			// nope, child table
			return _count;
		}

		/// <since>2.1</since>
		public int bucketCount()
		{
			return _hash.Length;
		}

		/// <summary>
		/// Method called to check to quickly see if a child symbol table
		/// may have gotten additional entries.
		/// </summary>
		/// <remarks>
		/// Method called to check to quickly see if a child symbol table
		/// may have gotten additional entries. Used for checking to see
		/// if a child table should be merged into shared table.
		/// </remarks>
		public bool maybeDirty()
		{
			return !_hashShared;
		}

		/// <since>2.1</since>
		public int hashSeed()
		{
			return _seed;
		}

		/// <summary>
		/// Method mostly needed by unit tests; calculates number of
		/// entries that are in collision list.
		/// </summary>
		/// <remarks>
		/// Method mostly needed by unit tests; calculates number of
		/// entries that are in collision list. Value can be at most
		/// (
		/// <see cref="size()"/>
		/// - 1), but should usually be much lower, ideally 0.
		/// </remarks>
		/// <since>2.1</since>
		public int collisionCount()
		{
			return _collCount;
		}

		/// <summary>
		/// Method mostly needed by unit tests; calculates length of the
		/// longest collision chain.
		/// </summary>
		/// <remarks>
		/// Method mostly needed by unit tests; calculates length of the
		/// longest collision chain. This should typically be a low number,
		/// but may be up to
		/// <see cref="size()"/>
		/// - 1 in the pathological case
		/// </remarks>
		/// <since>2.1</since>
		public int maxCollisionLength()
		{
			return _longestCollisionList;
		}

		/*
		/**********************************************************
		/* Public API, accessing symbols:
		/**********************************************************
		*/
		public static com.fasterxml.jackson.core.sym.Name getEmptyName()
		{
			return com.fasterxml.jackson.core.sym.Name1.getEmptyName();
		}

		/// <summary>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table.
		/// </summary>
		/// <remarks>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table.
		/// If not, will return null.
		/// <p>
		/// Note: separate methods to optimize common case of
		/// short element/attribute names (4 or less ascii characters)
		/// </remarks>
		/// <param name="q1">
		/// int32 containing first 4 bytes of the name;
		/// if the whole name less than 4 bytes, padded with zero bytes
		/// in front (zero MSBs, ie. right aligned)
		/// </param>
		/// <returns>
		/// Name matching the symbol passed (or constructed for
		/// it)
		/// </returns>
		public com.fasterxml.jackson.core.sym.Name findName(int q1)
		{
			int hash = calcHash(q1);
			int ix = (hash & _hashMask);
			int val = _hash[ix];
			/* High 24 bits of the value are low 24 bits of hash (low 8 bits
			* are bucket index)... match?
			*/
			if ((((val >> 8) ^ hash) << 8) == 0)
			{
				// match
				// Ok, but do we have an actual match?
				com.fasterxml.jackson.core.sym.Name name = _mainNames[ix];
				if (name == null)
				{
					// main slot empty; can't find
					return null;
				}
				if (name.equals(q1))
				{
					return name;
				}
			}
			else
			{
				if (val == 0)
				{
					// empty slot? no match
					return null;
				}
			}
			// Maybe a spill-over?
			val &= unchecked((int)(0xFF));
			if (val > 0)
			{
				// 0 means 'empty'
				val -= 1;
				// to convert from 1-based to 0...
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket bucket = _collList
					[val];
				if (bucket != null)
				{
					return bucket.find(hash, q1, 0);
				}
			}
			// Nope, no match whatsoever
			return null;
		}

		/// <summary>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table.
		/// </summary>
		/// <remarks>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table.
		/// If not, will return null.
		/// <p>
		/// Note: separate methods to optimize common case of relatively
		/// short element/attribute names (8 or less ascii characters)
		/// </remarks>
		/// <param name="q1">int32 containing first 4 bytes of the name.</param>
		/// <param name="q2">
		/// int32 containing bytes 5 through 8 of the
		/// name; if less than 8 bytes, padded with up to 3 zero bytes
		/// in front (zero MSBs, ie. right aligned)
		/// </param>
		/// <returns>Name matching the symbol passed (or constructed for it)</returns>
		public com.fasterxml.jackson.core.sym.Name findName(int q1, int q2)
		{
			int hash = (q2 == 0) ? calcHash(q1) : calcHash(q1, q2);
			int ix = (hash & _hashMask);
			int val = _hash[ix];
			/* High 24 bits of the value are low 24 bits of hash (low 8 bits
			* are bucket index)... match?
			*/
			if ((((val >> 8) ^ hash) << 8) == 0)
			{
				// match
				// Ok, but do we have an actual match?
				com.fasterxml.jackson.core.sym.Name name = _mainNames[ix];
				if (name == null)
				{
					// main slot empty; can't find
					return null;
				}
				if (name.equals(q1, q2))
				{
					return name;
				}
			}
			else
			{
				if (val == 0)
				{
					// empty slot? no match
					return null;
				}
			}
			// Maybe a spill-over?
			val &= unchecked((int)(0xFF));
			if (val > 0)
			{
				// 0 means 'empty'
				val -= 1;
				// to convert from 1-based to 0...
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket bucket = _collList
					[val];
				if (bucket != null)
				{
					return bucket.find(hash, q1, q2);
				}
			}
			// Nope, no match whatsoever
			return null;
		}

		public com.fasterxml.jackson.core.sym.Name findName(int q1, int q2, int q3)
		{
			int hash = calcHash(q1, q2, q3);
			int ix = (hash & _hashMask);
			int val = _hash[ix];
			if ((((val >> 8) ^ hash) << 8) == 0)
			{
				// match
				// Ok, but do we have an actual match?
				com.fasterxml.jackson.core.sym.Name name = _mainNames[ix];
				if (name == null)
				{
					// main slot empty; can't find
					return null;
				}
				if (name.equals(q1, q2, q3))
				{
					return name;
				}
			}
			else
			{
				if (val == 0)
				{
					// empty slot? no match
					return null;
				}
			}
			// Maybe a spill-over?
			val &= unchecked((int)(0xFF));
			if (val > 0)
			{
				// 0 means 'empty'
				val -= 1;
				// to convert from 1-based to 0...
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket bucket = _collList
					[val];
				if (bucket != null)
				{
					return bucket.find(hash, q1, q2, q3);
				}
			}
			// Nope, no match whatsoever
			return null;
		}

		/// <summary>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table; or if not, creates name object,
		/// adds to the table, and returns it.
		/// </summary>
		/// <remarks>
		/// Finds and returns name matching the specified symbol, if such
		/// name already exists in the table; or if not, creates name object,
		/// adds to the table, and returns it.
		/// <p>
		/// Note: this is the general purpose method that can be called for
		/// names of any length. However, if name is less than 9 bytes long,
		/// it is preferable to call the version optimized for short
		/// names.
		/// </remarks>
		/// <param name="q">
		/// Array of int32s, each of which contain 4 bytes of
		/// encoded name
		/// </param>
		/// <param name="qlen">
		/// Number of int32s, starting from index 0, in quads
		/// parameter
		/// </param>
		/// <returns>Name matching the symbol passed (or constructed for it)</returns>
		public com.fasterxml.jackson.core.sym.Name findName(int[] q, int qlen)
		{
			if (qlen < 4)
			{
				// another sanity check
				if (qlen == 3)
				{
					return findName(q[0], q[1], q[2]);
				}
				return findName(q[0], (qlen < 2) ? 0 : q[1]);
			}
			int hash = calcHash(q, qlen);
			// (for rest of comments regarding logic, see method above)
			int ix = (hash & _hashMask);
			int val = _hash[ix];
			if ((((val >> 8) ^ hash) << 8) == 0)
			{
				com.fasterxml.jackson.core.sym.Name name = _mainNames[ix];
				if (name == null || name.equals(q, qlen))
				{
					// main slot empty; no collision list then either
					// should be match, let's verify
					return name;
				}
			}
			else
			{
				if (val == 0)
				{
					// empty slot? no match
					return null;
				}
			}
			val &= unchecked((int)(0xFF));
			if (val > 0)
			{
				// 0 means 'empty'
				val -= 1;
				// to convert from 1-based to 0...
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket bucket = _collList
					[val];
				if (bucket != null)
				{
					return bucket.find(hash, q, qlen);
				}
			}
			return null;
		}

		/*
		/**********************************************************
		/* API, mutators
		/**********************************************************
		*/
		public com.fasterxml.jackson.core.sym.Name addName(string name, int q1, int q2)
		{
			if (_intern)
			{
				name = com.fasterxml.jackson.core.util.InternCache.instance.intern(name);
			}
			int hash = (q2 == 0) ? calcHash(q1) : calcHash(q1, q2);
			com.fasterxml.jackson.core.sym.Name symbol = constructName(hash, name, q1, q2);
			_addSymbol(hash, symbol);
			return symbol;
		}

		public com.fasterxml.jackson.core.sym.Name addName(string name, int[] q, int qlen
			)
		{
			if (_intern)
			{
				name = com.fasterxml.jackson.core.util.InternCache.instance.intern(name);
			}
			int hash;
			if (qlen < 4)
			{
				if (qlen == 1)
				{
					hash = calcHash(q[0]);
				}
				else
				{
					if (qlen == 2)
					{
						hash = calcHash(q[0], q[1]);
					}
					else
					{
						hash = calcHash(q[0], q[1], q[2]);
					}
				}
			}
			else
			{
				hash = calcHash(q, qlen);
			}
			com.fasterxml.jackson.core.sym.Name symbol = constructName(hash, name, q, qlen);
			_addSymbol(hash, symbol);
			return symbol;
		}

		private const int MULT = 33;

		private const int MULT2 = 65599;

		private const int MULT3 = 31;

		/*
		/**********************************************************
		/* Helper methods
		/**********************************************************
		*/
		/* Note on hash calculation: we try to make it more difficult to
		* generate collisions automatically; part of this is to avoid
		* simple "multiply-add" algorithm (like JDK String.hashCode()),
		* and add bit of shifting. And other part is to make this
		* non-linear, at least for shorter symbols.
		*/
		// JDK uses 31; other fine choices are 33 and 65599, let's use 33
		// as it seems to give fewest collisions for us
		// (see [http://www.cse.yorku.ca/~oz/hash.html] for details)
		public int calcHash(int q1)
		{
			int hash = q1 ^ _seed;
			hash += ((int)(((uint)hash) >> 15));
			// to xor hi- and low- 16-bits
			hash ^= ((int)(((uint)hash) >> 9));
			// as well as lowest 2 bytes
			return hash;
		}

		public int calcHash(int q1, int q2)
		{
			// For two quads, let's change algorithm a bit, to spice
			// things up (can do bit more processing anyway)
			int hash = q1;
			hash ^= ((int)(((uint)hash) >> 15));
			// try mixing first and second byte pairs first
			hash += (q2 * MULT);
			// then add second quad
			hash ^= _seed;
			hash += ((int)(((uint)hash) >> 7));
			// and shuffle some more
			// 26-Mar-2015, tatu: As per [core#187] need bit more shuffling. This may
			//   seem like a magical number (and in a way, it is), but it was the sweet
			//   spot for some reason (5 and 3 work ok but converges for 4, for tested case)
			hash ^= ((int)(((uint)hash) >> 4));
			return hash;
		}

		public int calcHash(int q1, int q2, int q3)
		{
			// use same algorithm as multi-byte, tested to work well
			int hash = q1 ^ _seed;
			hash += ((int)(((uint)hash) >> 9));
			hash *= MULT;
			hash += q2;
			hash *= MULT2;
			hash += ((int)(((uint)hash) >> 15));
			hash ^= q3;
			hash += ((int)(((uint)hash) >> 17));
			// and finally shuffle some more once done
			hash += ((int)(((uint)hash) >> 15));
			// to get high-order bits to mix more
			hash ^= (hash << 9);
			// as well as lowest 2 bytes
			return hash;
		}

		public int calcHash(int[] q, int qlen)
		{
			if (qlen < 4)
			{
				throw new System.ArgumentException();
			}
			/* And then change handling again for "multi-quad" case; mostly
			* to make calculation of collisions less fun. For example,
			* add seed bit later in the game, and switch plus/xor around,
			* use different shift lengths.
			*/
			int hash = q[0] ^ _seed;
			hash += ((int)(((uint)hash) >> 9));
			hash *= MULT;
			hash += q[1];
			hash *= MULT2;
			hash += ((int)(((uint)hash) >> 15));
			hash ^= q[2];
			hash += ((int)(((uint)hash) >> 17));
			for (int i = 3; i < qlen; ++i)
			{
				hash = (hash * MULT3) ^ q[i];
				// for longer entries, mess a bit in-between too
				hash += ((int)(((uint)hash) >> 3));
				hash ^= (hash << 7);
			}
			// and finally shuffle some more once done
			hash += ((int)(((uint)hash) >> 15));
			// to get high-order bits to mix more
			hash ^= (hash << 9);
			// as well as lowest 2 bytes
			return hash;
		}

		// Method only used by unit tests
		protected internal static int[] calcQuads(byte[] wordBytes)
		{
			int blen = wordBytes.Length;
			int[] result = new int[(blen + 3) / 4];
			for (int i = 0; i < blen; ++i)
			{
				int x = wordBytes[i] & unchecked((int)(0xFF));
				if (++i < blen)
				{
					x = (x << 8) | (wordBytes[i] & unchecked((int)(0xFF)));
					if (++i < blen)
					{
						x = (x << 8) | (wordBytes[i] & unchecked((int)(0xFF)));
						if (++i < blen)
						{
							x = (x << 8) | (wordBytes[i] & unchecked((int)(0xFF)));
						}
					}
				}
				result[i >> 2] = x;
			}
			return result;
		}

		/*
		/**********************************************************
		/* Standard methods
		/**********************************************************
		*/
		/*
		@Override
		public String toString()
		{
		StringBuilder sb = new StringBuilder();
		sb.append("[BytesToNameCanonicalizer, size: ");
		sb.append(_count);
		sb.append('/');
		sb.append(_mainHash.length);
		sb.append(", ");
		sb.append(_collCount);
		sb.append(" coll; avg length: ");
		
		// Average length: minimum of 1 for all (1 == primary hit);
		// and then 1 per each traversal for collisions/buckets
		//int maxDist = 1;
		int pathCount = _count;
		for (int i = 0; i < _collEnd; ++i) {
		int spillLen = _collList[i].length();
		for (int j = 1; j <= spillLen; ++j) {
		pathCount += j;
		}
		}
		double avgLength;
		
		if (_count == 0) {
		avgLength = 0.0;
		} else {
		avgLength = (double) pathCount / (double) _count;
		}
		// let's round up a bit (two 2 decimal places)
		//avgLength -= (avgLength % 0.01);
		
		sb.append(avgLength);
		sb.append(']');
		return sb.toString();
		}
		*/
		/*
		/**********************************************************
		/* Internal methods
		/**********************************************************
		*/
		private void _addSymbol(int hash, com.fasterxml.jackson.core.sym.Name symbol)
		{
			if (_hashShared)
			{
				// always have to modify main entry
				unshareMain();
			}
			// First, do we need to rehash?
			if (_needRehash)
			{
				rehash();
			}
			++_count;
			/* Ok, enough about set up: now we need to find the slot to add
			* symbol in:
			*/
			int ix = (hash & _hashMask);
			if (_mainNames[ix] == null)
			{
				// primary empty?
				_hash[ix] = (hash << 8);
				if (_namesShared)
				{
					unshareNames();
				}
				_mainNames[ix] = symbol;
			}
			else
			{
				// nope, it's a collision, need to spill over
				/* How about spill-over area... do we already know the bucket
				* (is the case if it's not the first collision)
				*/
				if (_collListShared)
				{
					unshareCollision();
				}
				// also allocates if list was null
				++_collCount;
				int entryValue = _hash[ix];
				int bucket = entryValue & unchecked((int)(0xFF));
				if (bucket == 0)
				{
					// first spill over?
					if (_collEnd <= LAST_VALID_BUCKET)
					{
						// yup, still unshared bucket
						bucket = _collEnd;
						++_collEnd;
						// need to expand?
						if (bucket >= _collList.Length)
						{
							expandCollision();
						}
					}
					else
					{
						// nope, have to share... let's find shortest?
						bucket = findBestBucket();
					}
					// Need to mark the entry... and the spill index is 1-based
					_hash[ix] = (entryValue & ~unchecked((int)(0xFF))) | (bucket + 1);
				}
				else
				{
					--bucket;
				}
				// 1-based index in value
				// And then just need to link the new bucket entry in
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket newB = new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket
					(symbol, _collList[bucket]);
				int collLen = newB.length;
				if (collLen > MAX_COLL_CHAIN_LENGTH)
				{
					/* 23-May-2014, tatu: Instead of throwing an exception right away, let's handle
					*   in bit smarter way.
					*/
					_handleSpillOverflow(bucket, newB);
				}
				else
				{
					_collList[bucket] = newB;
					// but, be careful wrt attacks
					_longestCollisionList = System.Math.max(newB.length, _longestCollisionList);
				}
			}
			{
				/* Ok. Now, do we need a rehash next time? Need to have at least
				* 50% fill rate no matter what:
				*/
				int hashSize = _hash.Length;
				if (_count > (hashSize >> 1))
				{
					int hashQuarter = (hashSize >> 2);
					/* And either strictly above 75% (the usual) or
					* just 50%, and collision count >= 25% of total hash size
					*/
					if (_count > (hashSize - hashQuarter))
					{
						_needRehash = true;
					}
					else
					{
						if (_collCount >= hashQuarter)
						{
							_needRehash = true;
						}
					}
				}
			}
		}

		private void _handleSpillOverflow(int bindex, com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket
			 newBucket)
		{
			if (_overflows == null)
			{
				_overflows = new Sharpen.BitSet();
				_overflows.set(bindex);
			}
			else
			{
				if (_overflows.get(bindex))
				{
					// Has happened once already, so not a coincident...
					if (_failOnDoS)
					{
						reportTooManyCollisions(MAX_COLL_CHAIN_LENGTH);
					}
					// but even if we don't fail, we will stop intern()ing
					_intern = false;
				}
				else
				{
					_overflows.set(bindex);
				}
			}
			// regardless, if we get this far, clear up the bucket, adjust size appropriately.
			_collList[bindex] = null;
			_count -= (newBucket.length);
			// we could calculate longest; but for now just mark as invalid
			_longestCollisionList = -1;
		}

		private void rehash()
		{
			_needRehash = false;
			// Note: since we'll make copies, no need to unshare, can just mark as such:
			_namesShared = false;
			/* And then we can first deal with the main hash area. Since we
			* are expanding linearly (double up), we know there'll be no
			* collisions during this phase.
			*/
			int[] oldMainHash = _hash;
			int len = oldMainHash.Length;
			int newLen = len + len;
			/* 13-Mar-2010, tatu: Let's guard against OOME that could be caused by
			*    large documents with unique (or mostly so) names
			*/
			if (newLen > MAX_T_SIZE)
			{
				nukeSymbols();
				return;
			}
			_hash = new int[newLen];
			_hashMask = (newLen - 1);
			com.fasterxml.jackson.core.sym.Name[] oldNames = _mainNames;
			_mainNames = new com.fasterxml.jackson.core.sym.Name[newLen];
			int symbolsSeen = 0;
			// let's do a sanity check
			for (int i = 0; i < len; ++i)
			{
				com.fasterxml.jackson.core.sym.Name symbol = oldNames[i];
				if (symbol != null)
				{
					++symbolsSeen;
					int hash = symbol.GetHashCode();
					int ix = (hash & _hashMask);
					_mainNames[ix] = symbol;
					_hash[ix] = hash << 8;
				}
			}
			// will clear spill index
			/* And then the spill area. This may cause collisions, although
			* not necessarily as many as there were earlier. Let's allocate
			* same amount of space, however
			*/
			int oldEnd = _collEnd;
			if (oldEnd == 0)
			{
				// no prior collisions...
				_longestCollisionList = 0;
				return;
			}
			_collCount = 0;
			_collEnd = 0;
			_collListShared = false;
			int maxColl = 0;
			com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] oldBuckets = _collList;
			_collList = new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[oldBuckets
				.Length];
			for (int i_1 = 0; i_1 < oldEnd; ++i_1)
			{
				for (com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket curr = oldBuckets
					[i_1]; curr != null; curr = curr.next)
				{
					++symbolsSeen;
					com.fasterxml.jackson.core.sym.Name symbol = curr.name;
					int hash = symbol.GetHashCode();
					int ix = (hash & _hashMask);
					int val = _hash[ix];
					if (_mainNames[ix] == null)
					{
						// no primary entry?
						_hash[ix] = (hash << 8);
						_mainNames[ix] = symbol;
					}
					else
					{
						// nope, it's a collision, need to spill over
						++_collCount;
						int bucket = val & unchecked((int)(0xFF));
						if (bucket == 0)
						{
							// first spill over?
							if (_collEnd <= LAST_VALID_BUCKET)
							{
								// yup, still unshared bucket
								bucket = _collEnd;
								++_collEnd;
								// need to expand?
								if (bucket >= _collList.Length)
								{
									expandCollision();
								}
							}
							else
							{
								// nope, have to share... let's find shortest?
								bucket = findBestBucket();
							}
							// Need to mark the entry... and the spill index is 1-based
							_hash[ix] = (val & ~unchecked((int)(0xFF))) | (bucket + 1);
						}
						else
						{
							--bucket;
						}
						// 1-based index in value
						// And then just need to link the new bucket entry in
						com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket newB = new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket
							(symbol, _collList[bucket]);
						_collList[bucket] = newB;
						maxColl = System.Math.max(maxColl, newB.length);
					}
				}
			}
			// for (... buckets in the chain ...)
			// for (... list of bucket heads ... )
			_longestCollisionList = maxColl;
			if (symbolsSeen != _count)
			{
				// sanity check
				throw new Sharpen.RuntimeException("Internal error: count after rehash " + symbolsSeen
					 + "; should be " + _count);
			}
		}

		/// <summary>
		/// Helper method called to empty all shared symbols, but to leave
		/// arrays allocated
		/// </summary>
		private void nukeSymbols()
		{
			_count = 0;
			_longestCollisionList = 0;
			java.util.Arrays.fill(_hash, 0);
			java.util.Arrays.fill(_mainNames, null);
			java.util.Arrays.fill(_collList, null);
			_collCount = 0;
			_collEnd = 0;
		}

		/// <summary>
		/// Method called to find the best bucket to spill a Name over to:
		/// usually the first bucket that has only one entry, but in general
		/// first one of the buckets with least number of entries
		/// </summary>
		private int findBestBucket()
		{
			com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] buckets = _collList;
			int bestCount = int.MaxValue;
			int bestIx = -1;
			for (int i = 0; i < len; ++i)
			{
				com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket b = buckets[i];
				// [#145] may become null due to long overflow chain
				if (b == null)
				{
					return i;
				}
				int count = b.length;
				if (count < bestCount)
				{
					if (count == 1)
					{
						// best possible
						return i;
					}
					bestCount = count;
					bestIx = i;
				}
			}
			return bestIx;
		}

		/// <summary>
		/// Method that needs to be called, if the main hash structure
		/// is (may be) shared.
		/// </summary>
		/// <remarks>
		/// Method that needs to be called, if the main hash structure
		/// is (may be) shared. This happens every time something is added,
		/// even if addition is to the collision list (since collision list
		/// index comes from lowest 8 bits of the primary hash entry)
		/// </remarks>
		private void unshareMain()
		{
			int[] old = _hash;
			_hash = java.util.Arrays.copyOf(old, old.Length);
			_hashShared = false;
		}

		private void unshareCollision()
		{
			com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] old = _collList;
			if (old == null)
			{
				_collList = new com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[INITIAL_COLLISION_LEN
					];
			}
			else
			{
				_collList = java.util.Arrays.copyOf(old, old.Length);
			}
			_collListShared = false;
		}

		private void unshareNames()
		{
			com.fasterxml.jackson.core.sym.Name[] old = _mainNames;
			_mainNames = java.util.Arrays.copyOf(old, old.Length);
			_namesShared = false;
		}

		private void expandCollision()
		{
			com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] old = _collList;
			_collList = java.util.Arrays.copyOf(old, old.Length * 2);
		}

		/*
		/**********************************************************
		/* Constructing name objects
		/**********************************************************
		*/
		private static com.fasterxml.jackson.core.sym.Name constructName(int hash, string
			 name, int q1, int q2)
		{
			if (q2 == 0)
			{
				// one quad only?
				return new com.fasterxml.jackson.core.sym.Name1(name, hash, q1);
			}
			return new com.fasterxml.jackson.core.sym.Name2(name, hash, q1, q2);
		}

		private static com.fasterxml.jackson.core.sym.Name constructName(int hash, string
			 name, int[] quads, int qlen)
		{
			if (qlen < 4)
			{
				switch (qlen)
				{
					case 1:
					{
						// Need to check for 3 quad one, can do others too
						return new com.fasterxml.jackson.core.sym.Name1(name, hash, quads[0]);
					}

					case 2:
					{
						return new com.fasterxml.jackson.core.sym.Name2(name, hash, quads[0], quads[1]);
					}

					case 3:
					default:
					{
						return new com.fasterxml.jackson.core.sym.Name3(name, hash, quads[0], quads[1], quads
							[2]);
					}
				}
			}
			return com.fasterxml.jackson.core.sym.NameN.construct(name, hash, quads, qlen);
		}

		/*
		/**********************************************************
		/* Other helper methods
		/**********************************************************
		*/
		/// <since>2.1</since>
		protected internal void reportTooManyCollisions(int maxLen)
		{
			throw new System.InvalidOperationException("Longest collision chain in symbol table (of size "
				 + _count + ") now exceeds maximum, " + maxLen + " -- suspect a DoS attack based on hash collisions"
				);
		}

		/// <summary>
		/// Immutable value class used for sharing information as efficiently
		/// as possible, by only require synchronization of reference manipulation
		/// but not access to contents.
		/// </summary>
		/// <since>2.1</since>
		private sealed class TableInfo
		{
			public readonly int count;

			public readonly int mainHashMask;

			public readonly int[] mainHash;

			public readonly com.fasterxml.jackson.core.sym.Name[] mainNames;

			public readonly com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] 
				collList;

			public readonly int collCount;

			public readonly int collEnd;

			public readonly int longestCollisionList;

			public TableInfo(int count, int mainHashMask, int[] mainHash, com.fasterxml.jackson.core.sym.Name
				[] mainNames, com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket[] collList
				, int collCount, int collEnd, int longestCollisionList)
			{
				/*
				/**********************************************************
				/* Helper classes
				/**********************************************************
				*/
				this.count = count;
				this.mainHashMask = mainHashMask;
				this.mainHash = mainHash;
				this.mainNames = mainNames;
				this.collList = collList;
				this.collCount = collCount;
				this.collEnd = collEnd;
				this.longestCollisionList = longestCollisionList;
			}

			public TableInfo(com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer src)
			{
				count = src._count;
				mainHashMask = src._hashMask;
				mainHash = src._hash;
				mainNames = src._mainNames;
				collList = src._collList;
				collCount = src._collCount;
				collEnd = src._collEnd;
				longestCollisionList = src._longestCollisionList;
			}
		}

		private sealed class Bucket
		{
			public readonly com.fasterxml.jackson.core.sym.Name name;

			public readonly com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket next;

			public readonly int hash;

			public readonly int length;

			internal Bucket(com.fasterxml.jackson.core.sym.Name name, com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket
				 next)
			{
				this.name = name;
				this.next = next;
				length = (next == null) ? 1 : next.length + 1;
				hash = name.GetHashCode();
			}

			public com.fasterxml.jackson.core.sym.Name find(int h, int firstQuad, int secondQuad
				)
			{
				if (hash == h)
				{
					if (name.equals(firstQuad, secondQuad))
					{
						return name;
					}
				}
				for (com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket curr = next; 
					curr != null; curr = curr.next)
				{
					if (curr.hash == h)
					{
						com.fasterxml.jackson.core.sym.Name currName = curr.name;
						if (currName.equals(firstQuad, secondQuad))
						{
							return currName;
						}
					}
				}
				return null;
			}

			public com.fasterxml.jackson.core.sym.Name find(int h, int q1, int q2, int q3)
			{
				if (hash == h)
				{
					if (name.equals(q1, q2, q3))
					{
						return name;
					}
				}
				for (com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket curr = next; 
					curr != null; curr = curr.next)
				{
					if (curr.hash == h)
					{
						com.fasterxml.jackson.core.sym.Name currName = curr.name;
						if (currName.equals(q1, q2, q3))
						{
							return currName;
						}
					}
				}
				return null;
			}

			public com.fasterxml.jackson.core.sym.Name find(int h, int[] quads, int qlen)
			{
				if (hash == h)
				{
					if (name.equals(quads, qlen))
					{
						return name;
					}
				}
				for (com.fasterxml.jackson.core.sym.BytesToNameCanonicalizer.Bucket curr = next; 
					curr != null; curr = curr.next)
				{
					if (curr.hash == h)
					{
						com.fasterxml.jackson.core.sym.Name currName = curr.name;
						if (currName.equals(quads, qlen))
						{
							return currName;
						}
					}
				}
				return null;
			}
		}
	}
}
