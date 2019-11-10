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
	/// The Binary Mutation Operator, when enabled, traverses each gene 
	/// in the population and, based on the probability swaps a gene 
	/// from one state to the other. The aim of this opperator is to 
	/// better reflect nature and add diversity to the population.
	/// This operator cannot be used with genes of type Object.
	/// </summary>
	public class BinaryMutate : MutateBase, IGeneticOperator
	{
		private bool _allowDuplicates;
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mutationProbability"></param>
		public BinaryMutate (double mutationProbability) : this (mutationProbability, true)
		{
			// mutate does not require an evaluated population, setting this to false could
			// save many evaluations
			RequiresEvaluatedPopulation = false;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="mutationProbability"></param>
		/// <param name="allowDuplicates"></param>
		public BinaryMutate (double mutationProbability, bool allowDuplicates) : base (mutationProbability)
		{
			_allowDuplicates = allowDuplicates;

			// mutate does not require an evaluated population, setting this to false could
			// save many evaluations
			RequiresEvaluatedPopulation = false;
		}

		/// <summary>
		/// Method to Mutate the specified child in derived classes. The method would not normally
		/// be called directly. It will be called by the framework as required.
		/// </summary>
		/// <param name="child"></param>
		protected override void Mutate (Chromosome child)
		{
			// this method calls the base class later which handles elites
			// duplicates are handled here

			Chromosome childToMutate = null;

			if (AllowDuplicates) {
				childToMutate = child;
			} else {

				//We have to clone the chromosome before we mutate it as it may
				//not be usable i.e. if it is a duplicate if we didn't clone it 
				//and we created a duplicate through mutation we would have to 
				//undo the mutation. This way is easier.
				childToMutate = child.DeepClone ();
			}

			//call the default mutation behaviour, this will inturn call the abstract
			//class 'MutateGene' implemented in this class below.
			base.Mutate (childToMutate);

			//only add the mutated chromosome if it does not exist otherwise do nothing
			if (!AllowDuplicates && !NewPopulation.SolutionExists (childToMutate)) {

				//swap existing genes for the mutated onese
				child.Genes = childToMutate.Genes;

			}
		}

		/// <summary>
		/// Mutates the gene.
		/// </summary>
		/// <param name="gene">Gene.</param>
		protected override void MutateGene (Gene gene)
		{

			if (gene.GeneType == GeneType.Object) {
				throw new OperatorException ("Genes with a GeneType of Object cannot be mutated by the BinaryMutate operator.");
			}

			switch (gene.GeneType) {
			case GeneType.Binary: {
					gene.ObjectValue = !(bool)gene.ObjectValue;
					break;
				}
			case GeneType.Real: {
					gene.ObjectValue = (double)gene.ObjectValue * -1;
					break;
				}
			case GeneType.Integer: {
                        //gene.ObjectValue = (int)gene.ObjectValue * -1;
                        gene.ObjectValue = (int)gene.ObjectValue;
                        break;
				}
			}
		}

		/// <summary>
		/// Sets/Gets whether duplicates are allowed in the population. 
		/// The setting and getting of this property is thread safe.
		/// </summary>
		public bool AllowDuplicates {
			get {
				lock (_syncLock) {
					return _allowDuplicates;
				}
			}
			set {
				lock (_syncLock) {
					_allowDuplicates = value;
				}
			}
		}
	}
}
