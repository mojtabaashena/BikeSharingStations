using System;
using System.Collections.Generic;
using System.Linq;
using GAF.Extensions;
using GAF.Threading;

namespace GAF.Operators
{
	/// <summary>
	/// Crossover.
	/// </summary>
	public class Crossover : CrossoverBase, IGeneticOperator
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		internal Crossover ()
			: this (1.0)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="crossOverProbability"></param>
		public Crossover (double crossOverProbability)
			: this (crossOverProbability, true)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="crossOverProbability"></param>
		/// <param name="allowDuplicates"></param>
		public Crossover (double crossOverProbability, bool allowDuplicates)
			: this (crossOverProbability, allowDuplicates, CrossoverType.SinglePoint)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="crossOverProbability"></param>
		/// <param name="allowDuplicates"></param>
		/// <param name="crossoverType"></param>
		public Crossover (double crossOverProbability, bool allowDuplicates, CrossoverType crossoverType)
			: this (crossOverProbability, allowDuplicates, crossoverType, Operators.ReplacementMethod.GenerationalReplacement)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="crossOverProbability"></param>
		/// <param name="allowDuplicates"></param>
		/// <param name="crossoverType"></param>
		/// <param name="replacementMethod"></param>
		public Crossover (double crossOverProbability, bool allowDuplicates, CrossoverType crossoverType, ReplacementMethod replacementMethod)
			:base(crossOverProbability,allowDuplicates,crossoverType,replacementMethod)
		{
		}

		/// <summary>
		/// Performs a single point crossover.
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="crossoverData">Crossover data.</param>
		/// <param name="cg1">Cg1.</param>
		/// <param name="cg2">Cg2.</param>
		protected override void PerformCrossoverSinglePoint (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2)
		{
			if (crossoverData.Points == null || crossoverData.Points.Count < 1) {
				throw new ArgumentException (
					"The CrossoverData.Points property is either null or is missing the required crossover points.");
			}
			cg1 = new List<Gene> ();
			cg2 = new List<Gene> ();

			var crossoverPoint1 = crossoverData.Points [0];
			var chromosomeLength = crossoverData.ChromosomeLength;

			//create the two children							
			cg1.AddRangeCloned (p1.Genes.Take (crossoverPoint1).ToList ());
			cg1.AddRangeCloned (p2.Genes.Skip (crossoverPoint1).Take (chromosomeLength - crossoverPoint1));

			cg2.AddRangeCloned (p2.Genes.Take (crossoverPoint1).ToList ());
			cg2.AddRangeCloned (p1.Genes.Skip (crossoverPoint1).Take (chromosomeLength - crossoverPoint1));

		} 

		/// <summary>
		/// Performs a double point crossover.
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="crossoverData">Crossover data.</param>
		/// <param name="cg1">Cg1.</param>
		/// <param name="cg2">Cg2.</param>
		protected override void PerformCrossoverDoublePoint (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2)
		{
			if (crossoverData.Points == null || crossoverData.Points.Count < 2) {
				throw new ArgumentException (
					"The CrossoverData.Points property is either null or is missing the required crossover points.");
			}

			var firstPoint = crossoverData.Points [0];
			var secondPoint = crossoverData.Points [1];
			var chromosomeLength = crossoverData.ChromosomeLength;

			cg1 = new List<Gene> ();
			cg2 = new List<Gene> ();


			//first child
			//first part of Parent 1
			cg1.AddRangeCloned (p1.Genes.Take (firstPoint).ToList ());
			//middle part pf Parent 2
			cg1.AddRangeCloned (p2.Genes.Skip (firstPoint).Take (secondPoint - firstPoint));
			//last part of Parent 1
			cg1.AddRangeCloned (p1.Genes.Skip (secondPoint).Take (chromosomeLength - secondPoint));

			//second child
			//first part of Parent 2
			cg2.AddRangeCloned (p2.Genes.Take (firstPoint).ToList ());
			//middle part pf Parent 1
			cg2.AddRangeCloned (p1.Genes.Skip (firstPoint).Take (secondPoint - firstPoint));
			//last part of Parent 2
			cg2.AddRangeCloned (p2.Genes.Skip (secondPoint).Take (chromosomeLength - secondPoint));

		}

		/// <summary>
		/// Performs a double point ordered crossover.
		/// </summary>
		/// <param name="p1">P1.</param>
		/// <param name="p2">P2.</param>
		/// <param name="crossoverData">Crossover data.</param>
		/// <param name="cg1">Cg1.</param>
		/// <param name="cg2">Cg2.</param>
		protected override void PerformCrossoverDoublePointOrdered (Chromosome p1, Chromosome p2, CrossoverData crossoverData, out List<Gene> cg1, out List<Gene> cg2)
		{
			if (crossoverData.Points == null || crossoverData.Points.Count < 2) {
				throw new ArgumentException (
					"The CrossoverData.Points property is either null or is missing the required crossover points.");
			}

			//this is like double point except that the values are all taken from one parent
			//first the centre section of the parent selection is taken to the child
			//the remaining values of the same parent are passed to the child in the order in
			//which they appear in the second parent. If the second parent does not include the value
			//an exception is thrown.

			//these can bring back the same number, this is ok as the values will be both inclusive
			//so if crossoverPoint1 and crossoverPoint2 are the same, one gene will form the center section.
			var firstPoint = crossoverData.Points [0];
			var secondPoint = crossoverData.Points [1];

			cg1 = new List<Gene> ();
			cg2 = new List<Gene> ();

			//pass the middle part of Parent 1 to child 1
			cg1.AddRangeCloned (p1.Genes.Skip (firstPoint).Take (secondPoint - firstPoint)); //+1 make this exclusive e.g. 4-6 include 3 genes. 

			//pass the middle part of Parent 2 to child 2
			cg2.AddRangeCloned (p2.Genes.Skip (firstPoint).Take (secondPoint - firstPoint));

			//create hash sets for parent1 and child1
			var p1Hash = new HashSet<object> ();
			var cg1Hash = new HashSet<object> ();

			//populate the parent hash set with object values (not the gene itself)
			foreach (var gene in p1.Genes) {
				p1Hash.Add (gene.ObjectValue);
			}

			//populate the child hash set with object values (not the gene itself)
			//at this point only the center section of P1 will be in C1
			foreach (var gene in cg1) {
				cg1Hash.Add (gene.ObjectValue);
			}

			//run through the P2 adding to C1 those that exist in P1 but not yet in C1
			//add them in the order found in P2. This has to be done by value as the first parent
			//is used but the order id determined by the second parent. Can't use Guid as the second 
			//parent has a different set of genes guids.
			foreach (var gene in p2.Genes) {
				//if we have the value in P1 and it is not already in C1, then add it.
				bool existsInP1 = p1Hash.Contains (gene.ObjectValue);
				bool existsInCg1 = cg1Hash.Contains (gene.ObjectValue);

				if (existsInP1 && !existsInCg1) {
					cg1.AddCloned (gene);
				}

			}

			var p2Hash = new HashSet<object> ();
			var cg2Hash = new HashSet<object> ();

			foreach (var gene in p2.Genes) {
				p2Hash.Add (gene.ObjectValue);
			}

			foreach (var gene in cg2) {
				cg2Hash.Add (gene.ObjectValue);
			}


			//run through the P1 adding to C2 those that exist in P2 but not yet in C2
			//add them in the order found in P1. This has to be done by value as the first parent
			//is used but the order id determined by the second parent. Can't use Guid as the second 
			//parent has a different set of genes (guids)
			foreach (var gene in p1.Genes) {
				//if we have the value in P1 and it is not already in C1, then add it.
				bool existsInP2 = p2Hash.Contains (gene.ObjectValue);
				bool existsInCg2 = cg2Hash.Contains (gene.ObjectValue);

				if (existsInP2 && !existsInCg2) {
					cg2.AddCloned (gene);
				}
			}

			//if at this point we do not have a complete child, raise an exception
			if (cg1.Count != p1.Count || cg2.Count != p2.Count) {
				throw new CrossoverTypeIncompatibleException ("The parent Chromosomes were not suitable for Ordered Crossover as they do not contain the same set of values. Consider using a different crossover type, or ensure all solutions are build with the same set of values.");
			}
		}

	}
}
