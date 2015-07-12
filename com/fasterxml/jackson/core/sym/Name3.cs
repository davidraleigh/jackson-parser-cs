using Sharpen;

namespace com.fasterxml.jackson.core.sym
{
	/// <summary>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of 9 to 12 bytes.
	/// </summary>
	/// <remarks>
	/// Specialized implementation of PName: can be used for short Strings
	/// that consists of 9 to 12 bytes. It's the longest special purpose
	/// implementaion; longer ones are expressed using
	/// <see cref="NameN"/>
	/// .
	/// </remarks>
	public sealed class Name3 : com.fasterxml.jackson.core.sym.Name
	{
		private readonly int q1;

		private readonly int q2;

		private readonly int q3;

		internal Name3(string name, int hash, int i1, int i2, int i3)
			: base(name, hash)
		{
			q1 = i1;
			q2 = i2;
			q3 = i3;
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

		public override bool equals(int quad1, int quad2, int quad3)
		{
			return (q1 == quad1) && (q2 == quad2) && (q3 == quad3);
		}

		public override bool equals(int[] quads, int qlen)
		{
			return (qlen == 3) && (quads[0] == q1) && (quads[1] == q2) && (quads[2] == q3);
		}
	}
}
