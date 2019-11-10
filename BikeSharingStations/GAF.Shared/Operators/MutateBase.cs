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
using System.Security;
using System.Text;
using System.Threading.Tasks;
using GAF.Threading;
using GAF.Extensions;

namespace GAF.Operators
{
	/// <summary>
	/// A derived operator, when enabled, traverses each gene 
	/// in the population and, based on the probability applies a custom method
	/// to the GeneType object genes. The aim of this opperator is to allow
	/// mutation of custom objects by inheriting this class in other projects.
	/// This operator can only be used with genes of type Object.
	/// </summary>
	public abstract class MutateBase : OperatorBase, IGeneticOperator
	{
		private double _mutationProbability;
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mutationProbability"></param>
		public MutateBase (double mutationProbability): base()
		{
			//_mutationProbability = mutationProbability >= 0 ? mutationProbability : 0.0;
			if (mutationProbability > 1.0)
				MutationProbability = 1.0;
			else if (mutationProbability < 0)
				MutationProbability = 0.0;
			else
				MutationProbability = mutationProbability;

		}

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly as
		/// it will be called by the framework as required.
		/// This method is virtual and allows the derived class to override and extend 
		/// the functionality of the method.       
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

			//copy everything accross including elite
			newPopulation.Solutions.Clear ();
			newPopulation.Solutions.AddRange (currentPopulation.Solutions);

			var solutionsToProcess = newPopulation.GetNonElites ();

			foreach (var chromosome in solutionsToProcess) {

				if (chromosome == null || chromosome.Genes == null) {
					throw new ArgumentException ("The Chromosome is either null or the Chromosome's Genes are null.");
				}

				Mutate (chromosome);
			
			}
		}

		/// <summary>
		/// Internal method to assist with unit testing. 
		/// </summary>
		/// <param name="child">Child.</param>
		/// <param name="mutationProbability">Mutation probability.</param>
		internal void Mutate (Chromosome child, double mutationProbability)
		{
			this.MutationProbability = mutationProbability;
			this.Mutate (child);
		}

		/// <summary>
		/// This is the default behaviour for mutate, the method calls the MutateGene
		/// method for each gene selected (by probability) in the chromosome. 
		/// The method is virtual and can be overriden in derived classes.
		/// </summary>
		/// <param name="child">Child.</param>
		protected virtual void Mutate (Chromosome child)
		{
			//cannot mutate elites or else we will ruin them
			if (child.IsElite)
				return;

			if (child == null || child.Genes == null) {
				throw new ArgumentException ("The Chromosome is either null or the Chromosomes Genes are null.");
			}

			foreach (var gene in child.Genes) {

				//check probability by generating a random number between zero and one and if 
				//this number is less than or equal to the given mutation probability 
				//e.g. 0.001 then the bit value is changed.
				var rd = RandomProvider.GetThreadRandom ().NextDouble ();

				if (rd <= MutationProbability) {

					//we are changing a chromosome so any existing fitness is now inappropriate
					child.Fitness = 0;
					child.FitnessNormalised = 0;

					MutateGene (gene);
				}
			}
		}

		/// <summary>
		/// Mutates the gene.
		/// </summary>
		/// <param name="gene">Child.</param>
		protected abstract void MutateGene (Gene gene);

		/// <summary>
		/// Sets/gets the Mutation probabilty. The setting and getting of this property is thread safe.
		/// </summary>
		public double MutationProbability {
			get {
				lock (_syncLock) {
					//this only locks the object, not its members
					//this is ok as the MutationProbability object is immutable.
					return _mutationProbability;
				}
			}
			set {
				lock (_syncLock) {
					_mutationProbability = value;
				}
			}
		}

	}
}
