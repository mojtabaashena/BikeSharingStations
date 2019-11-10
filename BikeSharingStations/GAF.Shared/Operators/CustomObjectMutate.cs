/*
	Genetic Algorithm Framework for .Net
	Copyright (C) 2016  James Andrew-Smith

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
	/// The Custom Object Mutation Operator, when enabled, traverses each gene 
	/// in the population and, based on the probability applies a custom method
	/// to the GeneType object genes. The aim of this opperator is to allow
	/// mutation of custom objects by inheriting this class in other projects.
	/// This operator can only be used with genes of type Object.
	/// </summary>
	[Obsolete("This class is deprecated, consider using MutationBase instead.")]
	public abstract class CustomObjectMutate : IGeneticOperator
	{
		private double _mutationProbabilityS;
		private bool _allowDuplicatesS;
		private readonly object _syncLock = new object();

		/// <summary>
		/// Internal Constructor for unit Testing.
		/// </summary>
		internal CustomObjectMutate() : this (1.0)
		{
            
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mutationProbability"></param>
		public CustomObjectMutate(double mutationProbability)
		{
			_mutationProbabilityS = mutationProbability;
			_allowDuplicatesS = true;
			Enabled = true;
			RequiresEvaluatedPopulation = false;
		}			 

		/// <summary>
		/// Enabled property. Diabling this operator will cause the population to 'pass through' unaltered.
		/// </summary>
		public bool Enabled { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:GAF.Operators.CustomObjectMutate"/> requires evaluated population.
		/// </summary>
		/// <value><c>true</c> if requires evaluated population; otherwise, <c>false</c>.</value>
		public bool RequiresEvaluatedPopulation { set; get; }

		/// <summary>
		/// This is the method that invokes the operator. This should not normally be called explicitly.
		/// 
		/// This method is virtual and allows the consumer to override and extend 
		/// the functionality of the operator to be extended        /// </summary>
		/// <param name="currentPopulation"></param>
		/// <param name="newPopulation"></param>
		/// <param name="fitnessFunctionDelegate"></param>
		public virtual void Invoke (Population currentPopulation, ref Population newPopulation, FitnessFunction fitnessFunctionDelegate)
		{

			if (newPopulation == null) {
				newPopulation = currentPopulation.CreateEmptyCopy ();			
			}

			if (!Enabled)
				return;

			var solutionsToProcess = currentPopulation.GetNonElites ();
			foreach (var chromosome in solutionsToProcess) {

				var mutationProbability = MutationProbability >= 0 ? MutationProbability : 0.0;			
				Mutate (chromosome, mutationProbability);
			}
				
			//copy everything accross including elites
			newPopulation.Solutions.Clear ();
			newPopulation.Solutions.AddRange (currentPopulation.Solutions);

		}

		/// <summary>
		/// Internal Method for Unit test purposes.
		/// </summary>
		/// <param name="child"></param>
		internal void Mutate (Chromosome child)
		{
			Mutate (child, MutationProbability);
		}

		/// <summary>
		/// This method is virtual and allows the consumer to override and extend 
		/// the functionality of the operator to be extended within a derived class.
		/// </summary>
		/// <param name="child"></param>
		/// <param name="mutationProbability"></param>
		protected virtual void Mutate (Chromosome child, double mutationProbability)
		{
			//cannot mutate elites or else we will ruin them
			if (child.IsElite)
				return;

			if (child == null || child.Genes == null) {
				throw new ArgumentException ("The Chromosome is either null or the Chromosomes Genes are null.");
			}

			foreach (var gene in child.Genes) {
				if (gene.GeneType != GeneType.Object) {
					throw new OperatorException ("Genes without a GeneType of Object cannot be mutated by the CustomObjectMutate operator.");
				}

				//Debug.WriteLine("Calculated Mutation Probability: {0}", workingProbability);

				//check probability by generating a random number between zero and one and if 
				//this number is less than or equal to the given mutation probability 
				//e.g. 0.001 then the bit value is changed.
				var rd = RandomProvider.GetThreadRandom ().NextDouble ();

				if (rd <= mutationProbability) {
					//we are changing a chromosome so any existing fitness is now inappropriate
					child.Fitness = 0;
					child.FitnessNormalised = 0;

                    gene.ObjectValue = CustomMutate(gene.ObjectValue);                     
				}
			}
		}

		/// <summary>
		/// Override this function to mutate an object.
		/// </summary>
		/// <returns>Object.</returns>
		/// <param name="objectValue">Object value.</param>
        protected abstract object CustomMutate(object objectValue);

		/// <summary>
		/// Returns the number of evaluations performed by this operator.
		/// </summary>
		/// <returns></returns>
		public int GetOperatorInvokedEvaluations ()
		{
			return 0;
		}

		/// <summary>
		/// Sets/gets the Mutation probabilty. The setting and getting of this property is thread safe.
		/// </summary>
		public double MutationProbability {
			get {
				lock (_syncLock) {
					//this only locks the object, not its members
					//this is ok as the MutationProbability object is immutable.
					return _mutationProbabilityS;
				}
			}
			set {
				lock (_syncLock) {
					_mutationProbabilityS = value;
				}
			}
		}
			
		/// <summary>
		/// Sets/Gets whether duplicates are allowed in the population. 
		/// The setting and getting of this property is thread safe.
		/// </summary>
		[Obsolete("This property is no longer supported and will be removed from future versions. If a unique population needs to be maintained, consider using the RandomReplace operator instead.")]
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
	}
}
