/*
	Genetic Algorithm Framework for .Net
	Copyright (C) 2016  John Newcombe

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Lesser General Public License as published by
	the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Lesser General Public License for more details.

		You should have received a copy of the GNU Lesser General Public License
		along with this program.  If not, see <http://www.gnu.org/licenses/>.

	http://johnnewcombe.net
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace GAF.Operators
{
	/// <summary>
	/// The Elite operator, when enabled, ensures that a specified percentage of the fittest 
	/// chromosomes are passed to the next generation without modification.
	/// 
	/// The Elite operator ensures that each generation produces a solution that is at least 
	/// as good as that produced by the previous generation.
	/// 
	/// This operator will append the selected percentatge of solutions (Chromosomes) from the 
	/// current population to the new population. The selected number (%) of solutions will be copied
	/// to the new population as long as there is space within the population, irrespective of the 
	/// specified percentage to copy. 
	/// 
	/// This operator will only copy as many as are required to fill the population.
	/// This operator does not consider duplicates. To maintain a unique population, ensure that this operator
	/// is used before any operators that modify the solutions.
	/// 
	/// </summary>
	public class Elite : OperatorBase, IGeneticOperator
	{
		private int _percentageS;
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="elitismPercentage"></param>
		public Elite (int elitismPercentage)
		{
			_percentageS = elitismPercentage;
			Enabled = true;
		}

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly.
		/// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnessFunctionDelegate"></param>
		public override void Invoke (Population currentPopulation, ref Population newPopulation, FitnessFunction fitnessFunctionDelegate)
		{
			if (currentPopulation.Solutions == null || currentPopulation.Solutions.Count == 0) {
				throw new ArgumentException ("There are no Solutions in the current Population.");
			}

			if (newPopulation == null) {
				newPopulation = currentPopulation.CreateEmptyCopy ();
			}

			CurrentPopulation = currentPopulation;
			NewPopulation = newPopulation;
			FitnessFunction = fitnessFunctionDelegate;

			if (!Enabled)
				return;
			
			CopyElites (currentPopulation, ref newPopulation, Percentage);
		}

		internal void CopyElites (Population currentPopulation, ref Population newPopulation, int percentage)
		{

			newPopulation.Solutions.Clear ();
			newPopulation.Solutions.AddRange (currentPopulation.Solutions);

			//reset
			foreach (var chromosome in newPopulation.Solutions) {
				chromosome.IsElite = false;
			}

			//get the top n% and set as elites
			var chromosomes = newPopulation.GetTopPercent (percentage);
			foreach (var chromosome in chromosomes) {
				chromosome.IsElite = true;
			}

			var max = newPopulation.MaximumFitness;
		
		}

		/// <summary>
		/// Sets/Gets the Percentage Elites. The setting and getting of this property is thread safe.
		/// </summary>
		public int Percentage {
			get {
				//not really needed as 32bit int updates are atomic on 32bit systems 
				lock (_syncLock) {
					return _percentageS;
				}
			}

			set {
				lock (_syncLock) {
					_percentageS = value;
				}
			}
		}
	}
}
