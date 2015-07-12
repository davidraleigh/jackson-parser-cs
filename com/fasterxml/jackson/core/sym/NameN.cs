using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// Generic implementation of PName used for "long" names, where long
	/// means that its byte (UTF-8) representation is 13 bytes or more.
	/// </summary>
	public sealed class NameN : com.fasterxml.jackson.core.sym.Name
	{
		private readonly int q1;

		private readonly int q2;

		private readonly int q3;

		private readonly int q4;

		private readonly int qlen;

		private readonly int[] q;

		internal NameN(string name, int hash, int q1, int q2, int q3, int q4, int[] quads
			, int quadLen)
			: base(name, hash)
		{
			// first four quads
			// total number of quads (4 + q.length)
			this.q1 = q1;
			this.q2 = q2;
			this.q3 = q3;
			this.q4 = q4;
			q = quads;
			qlen = quadLen;
		}

		public static com.fasterxml.jackson.core.sym.NameN construct(string name, int hash
			, int[] q, int qlen)
		{
			/* We have specialized implementations for shorter
			* names, so let's not allow runt instances here
			*/
			if (qlen < 4)
			{
				throw new System.ArgumentException();
			}
			int q1 = q[0];
			int q2 = q[1];
			int q3 = q[2];
			int q4 = q[3];
			int rem = qlen - 4;
			int[] buf;
			if (rem > 0)
			{
				buf = java.util.Arrays.copyOfRange(q, 4, qlen);
			}
			else
			{
				buf = null;
			}
			return new com.fasterxml.jackson.core.sym.NameN(name, hash, q1, q2, q3, q4, buf, 
				qlen);
		}

		// Implies quad length == 1, never matches
		public override bool equals(int quad)
		{
			return false;
		}

		// Implies quad length == 2, never matches
		public override bool equals(int quad1, int quad2)
		{
			return false;
		}

		// Implies quad length == 3, never matches
		public override bool equals(int quad1, int quad2, int quad3)
		{
			return false;
		}

		public override bool equals(int[] quads, int len)
		{
			if (len != qlen)
			{
				return false;
			}
			// Will always have >= 4 quads, can unroll
			if (quads[0] != q1)
			{
				return false;
			}
			if (quads[1] != q2)
			{
				return false;
			}
			if (quads[2] != q3)
			{
				return false;
			}
			if (quads[3] != q4)
			{
				return false;
			}
			switch (len)
			{
				default:
				{
					return _equals2(quads);
				}

				case 8:
				{
					if (quads[7] != q[3])
					{
						return false;
					}
					goto case 7;
				}

				case 7:
				{
					if (quads[6] != q[2])
					{
						return false;
					}
					goto case 6;
				}

				case 6:
				{
					if (quads[5] != q[1])
					{
						return false;
					}
					goto case 5;
				}

				case 5:
				{
					if (quads[4] != q[0])
					{
						return false;
					}
					goto case 4;
				}

				case 4:
				{
					break;
				}
			}
			return true;
		}

		private bool _equals2(int[] quads)
		{
			int end = qlen - 4;
			for (int i = 0; i < end; ++i)
			{
				if (quads[i + 4] != q[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}
