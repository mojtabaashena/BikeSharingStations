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
	/// This operator will append the selected percentatge of solutions (Chromosomes) from the 
	/// current population to the new population. The selected number (%) of solutions will be copied
	/// to the new population as long as there is space within the population. Irrespective of the 
	/// specified percentage to copy, This operator will only copy as many as required to fill the population.
	/// This operator does not consider duplicates,however if the current population is unique, this will be 
	/// maintained.
	/// </summary>
	[Obsolete("This operator has no real value in the current version of the GAF. The same copy operations are acheived through crossover probability and by using the Elite and Random Replace operators.", false)]
	public class Copy : OperatorBase, IGeneticOperator
	{
		private int _percentageS;
		private readonly object _syncLock = new object ();
		private CopyMethod _copyMethod;

		/// <summary>
		/// Constructor used for Unit Testing.
		/// </summary>
		internal Copy ()
			: this (100, CopyMethod.Fittest)
		{            
		}

		/// <summary>
		/// Fills any spare capaity by copying the solutions from the current population to the new population
		/// using the specified copy method.
		/// </summary>
		/// <param name="copyMethod"></param>
		public Copy (CopyMethod copyMethod)
			: this (100, copyMethod)
		{
		}

		/// <summary>
		/// Copies the specified percentage of solutions from the current population to the new population
		/// using the 'Fittest' copy method. 
		/// </summary>
		/// <param name="copyPercentage"></param>
		public Copy (int copyPercentage)
			: this (copyPercentage, CopyMethod.Fittest)
		{
		}

		/// <summary>
		/// Copies the specified percentage of solutions from the current population to the new population
		/// using the specified copy method. 
		/// </summary>
		/// <param name="copyPercentage"></param>
		/// <param name="copyMethod"></param>
		public Copy (int copyPercentage, CopyMethod copyMethod)
		{
			_percentageS = copyPercentage;
			_copyMethod = copyMethod;
		}

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly.
		/// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnessFunctionDelegate"></param>
		public override void Invoke (Population currentPopulation, ref Population newPopulation,
		                         FitnessFunction fitnessFunctionDelegate)
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

			//get the pool of solutions to choose from
			//this is safe as the current population also has the elite property set by the Elite operator
			//var populationSize = currentPopulation.Solutions.Count(s => !s.IsElite);
			var populationSize = currentPopulation.Solutions.Count ();

			var chromosomes = currentPopulation.Solutions;
			var numberToCopy = (int)System.Math.Round ((chromosomes.Count () / 100.0) * this.Percentage); //get local copy to saves locking this member unnecessarily
            
			if (numberToCopy > populationSize)
				numberToCopy = populationSize;
			
			switch (_copyMethod) {
			case CopyMethod.Fittest:
				{
					CopyFittest (currentPopulation, ref newPopulation, numberToCopy);
					break;
				}
			case CopyMethod.Random:
				{
					CopyRandom (currentPopulation, ref newPopulation, numberToCopy);

					break;
				}
			default:
				{
					CopyFittest (currentPopulation, ref newPopulation, numberToCopy);
					break;
				}
			}
		}

		internal void CopyRandom (Population currentPopulation, ref Population newPopulation, int numberToCopy)
		{
			//random select
			var availableSolutions = currentPopulation.Solutions.Where (s => !s.IsElite).ToList ();
			var availableSolutionCount = availableSolutions.Count ();

			if (availableSolutionCount > 0) {
				//need to get random children and move to the newPopulation
				var randomSelections = new int[numberToCopy];
				for (var index = 0; index < numberToCopy; index++) {
					randomSelections [index] = Threading.RandomProvider.GetThreadRandom ().Next (0, availableSolutionCount);
				}

				foreach (var selection in randomSelections) {
					var selectedSolution = availableSolutions [selection];
					newPopulation.Solutions.Add (selectedSolution);
				}
			}
		}

		internal void CopyFittest (Population currentPopulation, ref Population newPopulation, int numberToCopy)
		{
			//select fittest
			var availableSolutions = currentPopulation.Solutions.Where (s => !s.IsElite).ToList ();
			var availableSolutionCount = availableSolutions.Count ();

			if (availableSolutionCount > 0) {
				availableSolutions.Sort ();
				newPopulation.Solutions.AddRange (availableSolutions.Take (numberToCopy));
			}
		}

		/// <summary>
		/// Sets/Gets the Percentage value of copy. The setting and getting of this property is thread safe.
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

		/// <summary>
		/// Sets/Gets the Copy Method of copy. The setting and getting of this property is thread safe.
		/// </summary>
		public CopyMethod CopyMethod {
			get {
				//not really needed as 32bit int updates are atomic on 32bit systems 
				lock (_syncLock) {
					return _copyMethod;
				}
			}

			set {
				lock (_syncLock) {
					_copyMethod = value;
				}
			}
		}

	}


}
