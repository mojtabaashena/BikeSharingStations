using System;
using System.Collections.Generic;

namespace GAF
{
	/// <summary>
	/// Chromosome comparer.
	/// </summary>
	public class ChromosomeComparer: IEqualityComparer<Chromosome>
	{
		/// <summary>
		/// Equals the specified x and y.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public bool Equals (Chromosome x, Chromosome y)
		{
			if (Object.ReferenceEquals (x, y)) return true;
			if (Object.ReferenceEquals (x, null) || Object.ReferenceEquals (y, null))
				return false;

			return x.ToBinaryString () == y.ToBinaryString ();
		}

		/// <summary>
		/// Gets the hash code.
		/// </summary>
		/// <returns>The hash code.</returns>
		/// <param name="product">Product.</param>
		public int GetHashCode (Chromosome product)
		{
			if (Object.ReferenceEquals (product, null)) return 0;
			int hashBinaryString = product.ToBinaryString () == null ? 0 : product.ToBinaryString ().GetHashCode ();
			return hashBinaryString;
		}
	}
}
