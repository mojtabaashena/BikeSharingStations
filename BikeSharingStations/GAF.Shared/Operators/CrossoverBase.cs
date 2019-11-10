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
using GAF.Threading;
using GAF.Extensions;

namespace GAF.Operators
{

	/// <summary>
	/// This operator will expand the new population to the same size 
	/// as the current population using the Crossover genetic operation.
	/// Properties of the class allow for a single or double point crossover
	/// and either 'Generational' or 'Delete Last' replacement mechanism.
	/// </summary>
	public abstract class CrossoverBase : OperatorBase, IGeneticOperator
	{
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Delegage definition for the CrossoverComplete event handler.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        public delegate void CrossoverCompleteHandler (object sender, CrossoverEventArgs e);

		/// <summary>
		/// Event definition for the CrossoverComplete event handler.
		/// </summary>
		public event CrossoverCompleteHandler OnCrossoverComplete;

		private double _crossoverProbabilityS = 1.0;
		private int _evaluations;
		private bool _allowDuplicatesS;
		private CrossoverType _crossoverTypeS;
		private ReplacementMethod _replacementMethodS;
		private int _currentPopulationSize;
		private int _numberOfChildrenToGenerate;

		///// <summary>
		///// Constructor.
		///// </summary>
		//internal CrossoverBase ()
		//	: this (1.0)
		//{
		//}

		///// <summary>
		///// Constructor.
		///// </summary>
		///// <param name="crossOverProbability"></param>
		//public CrossoverBase (double crossOverProbability)
		//	: this (crossOverProbability, true)
		//{
		//}

		///// <summary>
		///// Constructor.
		///// </summary>
		///// <param name="crossOverProbability"></param>
		///// <param name="allowDuplicates"></param>
		//public CrossoverBase (double crossOverProbability, bool allowDuplicates)
		//	: this (crossOverProbability, allowDuplicates, CrossoverType.SinglePoint)
		//{
		//}

		///// <summary>
		///// Constructor.
		///// </summary>
		///// <param name="crossOverProbability"></param>
		///// <param name="allowDuplicates"></param>
		///// <param name="crossoverType"></param>
		//public CrossoverBase (double crossOverProbability, bool allowDuplicates, CrossoverType crossoverType)
		//	: this (crossOverProbability, allowDuplicates, crossoverType, Operators.ReplacementMethod.GenerationalReplacement)
		//{
		//}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="crossOverProbability"></param>
		/// <param name="allowDuplicates"></param>
		/// <param name="crossoverType"></param>
		/// <param name="replacementMethod"></param>
		public CrossoverBase (double crossOverProbability, bool allowDuplicates, CrossoverType crossoverType, ReplacementMethod replacementMethod)
		{
			this.CrossoverProbability = crossOverProbability;
			this.AllowDuplicates = allowDuplicates;
			this.ReplacementMethod = replacementMethod;
			this.CrossoverType = crossoverType;

			Enabled = true;
		}

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly.
		/// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnessFunctionDelegate"></param>
		public new virtual void Invoke (Population currentPopulation, ref Population newPopulation, FitnessFunction fitnessFunctionDelegate)
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

			//TODO: Investigate this bizare code!!!
			//need to store this as we cannot handle a change until once this generation has started
			//_replacementMethodS = ReplacementMethod;





			int maxLoop = 100;
			int eliteCount = 0;

			//reset the number of evaluations
			_evaluations = 0;

			//now that we know how many children we are to be creating, in the case of 'Delete Last'
			//we copy the current generation across and add new children where they are greater than
			//the worst solution.
			if (_replacementMethodS == ReplacementMethod.DeleteLast) {
				//copy everything accross including the elites
				NewPopulation.Solutions.Clear ();
				NewPopulation.Solutions.AddRange (CurrentPopulation.Solutions);
			} else {
				//just copy the elites, this will take all elites

				//TODO: Sort out what we do if we overfill the population with elites
				var elites = CurrentPopulation.GetElites ();
				eliteCount = elites.Count ();
				if (elites != null && eliteCount > 0) {
					NewPopulation.Solutions.AddRange (elites);
				}
			}
			_currentPopulationSize = CurrentPopulation.Solutions.Count;

			_numberOfChildrenToGenerate =
				_currentPopulationSize - eliteCount;

			while (_numberOfChildrenToGenerate > 0) {

				//emergency exit
				if (maxLoop <= 0) {
					throw new ChromosomeException ("Unable to create a suitable child. If duplicates have been disallowed then consider increasing the chromosome length or increasing the number of elites.");
				}

				//these will hold the children
				Chromosome c1 = null;
				Chromosome c2 = null;

				//select some parents
				var parents = CurrentPopulation.SelectParents ();
				var p1 = parents [0];
				var p2 = parents [1];

				//crossover
				var crossoverData = CreateCrossoverData (p1.Genes.Count, CrossoverType);
				PerformCrossover (p1, p2, crossoverData, out c1, out c2);

				//pass the children out to derived classes 
				//(e.g. CrossoverMutate class uses this to perform mutation)
				if (OnCrossoverComplete != null) {
					var eventArgs = new CrossoverEventArgs (crossoverData, p1, p2, c1, c2);
					OnCrossoverComplete (this, eventArgs);
				}

				if (AddChild (c1)) {
					_numberOfChildrenToGenerate--;
				} else {
					//unable to create child
					maxLoop--;
				}

				//see if we can add the secomd
				if (_numberOfChildrenToGenerate > 0) {
					if (AddChild (c2)) {
						_numberOfChildrenToGenerate--;
					} else {
						//unable to create child
						maxLoop--;
					}
				}
			}

			//TODO: Test this, we shouldn't need it.
			newPopulation = NewPopulation;
		}

		/// <summary>
		/// Returns the number of evaluations that were undertaken as part of the operators Invocation.
		/// For example the 'Steady State' reproduction method, compares new childrem with those already
		/// in the population and therefore performs the analysis as part of the operators invocation.
		/// </summary>
		/// <returns></returns>
		public new int GetOperatorInvokedEvaluations ()
		{
			return _evaluations;
		}

		internal void PerformCrossover (Chromosome p1, Chromosome p2, double crossoverProbability, CrossoverType crossoverType, CrossoverData crossoverData, out Chromosome c1, out Chromosome c2)
		{
			this.CrossoverProbability = crossoverProbability;
			this.CrossoverType = crossoverType;

			this.PerformCrossover (p1, p2, crossoverData, out c1, out c2);
		}

		/// <summary>
		/// Performs a crossover.
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="crossoverData">Crossover data.</param>
		/// <param name="c1">C1.</param>
		/// <param name="c2">C2.</param>
		protected void PerformCrossover (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out Chromosome c1, out Chromosome c2)
		{
			var chromosomeLength = p1.Genes.Count;

			if (chromosomeLength != p2.Genes.Count) {
				throw new ArgumentException ("Parent chromosomes are not the same length.");
			}

			List<Gene> cg1 = null; //new List<Gene> ();
			List<Gene> cg2 = null; //new List<Gene> ();

			//check probability by generating a random number between zero and one and if 
			//this number is less than or equal to the given crossover probability 
			//then crossover takes place."
			var rd = RandomProvider.GetThreadRandom ().NextDouble ();
			if (rd <= this.CrossoverProbability) {
				switch (this.CrossoverType) {
				case CrossoverType.SinglePoint: {
						PerformCrossoverSinglePoint (p1, p2, crossoverData, out cg1, out cg2);
						break;
					}
				case CrossoverType.DoublePoint: {
						PerformCrossoverDoublePoint (p1, p2, crossoverData, out cg1, out cg2);
						break;
					}
				case CrossoverType.DoublePointOrdered: {
						PerformCrossoverDoublePointOrdered (p1, p2, crossoverData, out cg1, out cg2);
						break;
					}
				}
			} else {

				cg1 = new List<Gene> ();
				cg2 = new List<Gene> ();

				//crossover probaility dictates that these pass through to the next generation untouched (except for an ID change.
				//get the existing parent genes and treat them as the new children 
				cg1.AddRangeCloned (p1.Genes);
				cg2.AddRangeCloned (p2.Genes);

			}

			if (cg1.Count != chromosomeLength || cg2.Count != chromosomeLength) {
				throw new ChromosomeCorruptException ("Chromosome is corrupt!");
			}
			c1 = new Chromosome (cg1);
			c2 = new Chromosome (cg2);

		}

        /// <summary>
        /// Abstract Method
        /// </summary>
        protected abstract void PerformCrossoverSinglePoint (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2);
        /// <summary>
        /// Abstract Method
        /// </summary>
		protected abstract void PerformCrossoverDoublePoint (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2);
        /// <summary>
        /// Abstract Method
        /// </summary>
		protected abstract void PerformCrossoverDoublePointOrdered (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2);

		internal CrossoverData CreateCrossoverData (int chromosomeLength, CrossoverType crossoverType)
		{
			var result = new CrossoverData (chromosomeLength);

			//this is like double point except that the values are all taken from one parent
			//first the centre section of the parent selection is taken to the child
			//the remaining values of the same parent are passed to the child in the order in
			//which they appear in the second parent. If the second parent does not include the value
			//an exception is thrown.

			//these can bring back the same number, this is ok as the values will be both inclusive
			//so if crossoverPoint1 and crossoverPoint2 are the same, one gene will form the center section.
			switch (crossoverType) {
			case CrossoverType.SinglePoint:
                    //0 is invalid, range for single point is 1 to [length]
				result.Points.Add (RandomProvider.GetThreadRandom ().Next (1, chromosomeLength));
				break;

			case CrossoverType.DoublePoint:
			case CrossoverType.DoublePointOrdered:
                    //0 is invalid, range for double needs to leave room for right segment and is 1 to [length]

				int point1;
				int point2;
				do {
					point2 = RandomProvider.GetThreadRandom ().Next (1, chromosomeLength);
					point1 = RandomProvider.GetThreadRandom ().Next (1, chromosomeLength);
				} while (point2 == point1);

				result.Points.Add (System.Math.Min (point2, point1));
				result.Points.Add (System.Math.Max (point2, point1));

				break;
			}

			return result;
		}

		/// <summary>
		/// Adds a child to the new population depending upon the criteria set in relation to replacement
		/// method and duplicate handling. The method updates the evaluation count and returns true if a 
		/// child was added to the new population.
		/// </summary>
		/// <param name="child"></param>
		/// <returns></returns>
		private bool AddChild (Chromosome child)
		{
			var result = false;

			if (_replacementMethodS == ReplacementMethod.DeleteLast) {
				child.Evaluate (FitnessFunction);
				_evaluations++;

				if (child.Genes != null && child.Fitness > CurrentPopulation.MinimumFitness) {
					//add the child if there is still space
					if (AllowDuplicates || !NewPopulation.SolutionExists (child)) {
						//add the new child and remove the last
						NewPopulation.Solutions.Add (child);
						if (NewPopulation.Solutions.Count > _currentPopulationSize) {
							NewPopulation.Solutions.Sort ();
							NewPopulation.Solutions.RemoveAt (_currentPopulationSize - 1);
							result = true;
						} else {
							//we return true whether we actually added or not what we are effectively
							//doing here is adding the original child from the current solution
							result = true;
						}
					}
				}
			} else {
				//we need to cater for the user switching from delete last to Generational Replacement
				//in this scenrio we will have a full population but with still some children to generate
				if (NewPopulation.Solutions.Count + _numberOfChildrenToGenerate > _currentPopulationSize) {
					//assume all done for this generation
					_numberOfChildrenToGenerate = 0;
					return false;
				}
					
				if (child.Genes != null) {
					//add the child if there is still space
					if (this.AllowDuplicates || !NewPopulation.SolutionExists (child)) {
						NewPopulation.Solutions.Add (child);
						result = true;
					}
				}
			}
			return result;
		}

		#region Public Properties

		/// <summary>
		/// Sets/Gets whether duplicates are allowed in the population. 
		/// The setting and getting of this property is thread safe.
		/// </summary>
		public bool AllowDuplicates {
			get {
				lock (_syncLock) {
					return _allowDuplicatesS;
				}
			}
			set {
				lock (_syncLock) {
					_allowDuplicatesS = value;
				}
			}
		}

		/// <summary>
		/// Sets/Gets the type of crossover operation. 
		/// The setting and getting of this property is thread safe.
		/// </summary>
		public CrossoverType CrossoverType {
			set {
				lock (_syncLock) {
					_crossoverTypeS = value;
				}
			}
			get {
				lock (_syncLock) {
					return _crossoverTypeS;
				}
			}
		}

		/// <summary>
		/// Sets/Gets the method used for the deletion of chromosomes from the population.
		/// The setting and getting of this property is thread safe.
		/// </summary>
		public ReplacementMethod ReplacementMethod {
			set {
				lock (_syncLock) {
					_replacementMethodS = value;
				}
			}
			get {
				lock (_syncLock) {
					return _replacementMethodS;
				}
			}
		}

		/// <summary>
		/// Sets/gets the current crossover probability. 
		/// The setting and getting of this property is thread safe.
		/// </summary>
		public double CrossoverProbability {
			get {
				lock (_syncLock) {
					return _crossoverProbabilityS;
				}
			}
			set {
				lock (_syncLock) {
					_crossoverProbabilityS = value;
				}
			}
		}

		#endregion
	}

}

