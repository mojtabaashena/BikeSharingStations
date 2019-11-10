using System;
using System.Collections.Generic;

namespace GAF.Operators
{
	/// <summary>
	/// Data object used to pass data via the Crossover event arguments.
	/// </summary>
	public class CrossoverData
	{
		readonly List<int> _points = new List<int> ();
		readonly int _chromosomeLength = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:GAF.Operators.CrossoverData"/> class.
		/// </summary>
		/// <param name="chromosomeLength">Chromosome length.</param>
		public CrossoverData (int chromosomeLength)
		{
			_chromosomeLength = chromosomeLength;
		}

		/// <summary>
		/// Returns a list of crossover points. For single point crossover 
		/// one point would be present, 
		/// for double point crossover methods two would be present.
		/// </summary>
		public List<int> Points {
			get { return _points; }
		}

		/// <summary>
		/// Gets the length of the chromosome.
		/// </summary>
		/// <value>The length of the chromosome.</value>
		public int ChromosomeLength {
			get { return _chromosomeLength; }
		}
	}
}
