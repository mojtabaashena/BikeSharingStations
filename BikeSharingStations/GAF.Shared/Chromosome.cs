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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using GAF.Threading;
using GAF.Exceptions;
using System.Diagnostics;
using System.Runtime.Serialization;
using GAF.Extensions;

namespace GAF
{
	/// <summary>
	/// This is the delegate definition for the Fitness function.
	/// </summary>
	/// <param name="solution"></param>
	/// <returns></returns>
	public delegate double FitnessFunction (Chromosome solution);

	/// <summary>
	/// This clas represents a chromosome.
	/// </summary>
#if !PCL
	[Serializable]
	[DataContract]
#endif
	public class Chromosome : IEnumerable, IComparable<Chromosome>
	{
		/// <summary>
		/// This is the delegate type that is passed to the Evaluate function.
		/// </summary>
		/// <returns></returns>
		private Guid _id = Guid.NewGuid ();
		private List<Gene> _genes = new List<Gene> ();

		/// <summary>
		/// Constructor. Create a Chromosome with no Genes.
		/// </summary>
		public Chromosome ()
		{

		}

		/// <summary>
		/// Constructor. Specify the chromosome length, i.e. the number of genes.
		/// </summary>
		/// <param name="length"></param>
		public Chromosome (int length)
		{
			//creates a random chromosome with values between -1 and +1
			//this gives a randomly created chromosome for both real and
			//binary construction
			for (var index = 0; index < length; index++) {
				var value = RandomProvider.GetThreadRandom ().NextDouble ();

				//change range to -1 to +1
				value = (value - 0.5) * 2;

				_genes.Add (new Gene () { ObjectValue = value > 0 });
			}

		}

		/// <summary>
		/// Constructor that accepts a binary string.
		/// </summary>
		/// <param name="binaryString"></param>
		public Chromosome (string binaryString)
		{
			try {
				foreach (Char digit in binaryString) {
					this.Add (new Gene () { ObjectValue = digit == '1' });
				}
			} catch (Exception ex) {
				throw new ArgumentException ("Invalid string.", ex);
			}
		}

		/// <summary>
		/// Constructor that acceps a list or array of real numbers.
		/// </summary>
		/// <param name="reals"></param>
		public Chromosome (IEnumerable<double> reals)
		{
			try {
				foreach (var digit in reals) {
					this.Add (new Gene () { ObjectValue = digit });
				}
			} catch (Exception ex) {
				throw new ArgumentException ("Invalid range.", ex);
			}
		}

		/// <summary>
		/// Constructor that acceps list or array of integer numbers.
		/// </summary>
		/// <param name="ints"></param>
		public Chromosome (IEnumerable<int> ints)
		{
			try {
				foreach (var digit in ints) {
					this.Add (new Gene () { ObjectValue = digit });
				}
			} catch (Exception ex) {
				throw new ArgumentException ("Invalid range.", ex);
			}
		}

		/// <summary>
		/// Constructor that acceps list or array of Genes.
		/// </summary>
		/// <param name="genes"></param>
		public Chromosome (IEnumerable<Gene> genes)
		{
			try {
				_genes.AddRange (genes);
			} catch (Exception ex) {
				throw new ArgumentException ("Invalid range.", ex);
			}

		}

		/// <summary>
		/// Add the specified gene.
		/// </summary>
		/// <param name="gene">Gene.</param>
		public void Add (Gene gene)
		{
			_genes.Add (gene);
		}

		/// <summary>
		/// Adds a range of genes to the Chromosome.
		/// </summary>
		/// <param name="genes"></param>
		public void AddRange (List<Gene> genes)
		{
			_genes.AddRange (genes);
		}

		/// <summary>
		/// Removes all the Genes from the Chromosome.
		/// </summary>
		public void Clear ()
		{
			_genes.Clear ();
		}

		/// <summary>
		/// Returns the Last gene in the chromosome. 
		/// Helper method that is typically used when Non-pheno type genes exist within the chromosome.
		/// </summary>
		/// <returns></returns>
		public Gene LastGene ()
		{
			return Genes.Last ();
		}

		/// <summary>
		/// Returns the Last gene in the chromosome.
		/// Helper method that is typically used when Non-pheno type genes exist within the chromosome.
		/// </summary>
		/// <returns></returns>
		public Gene FirstGene ()
		{
			return Genes.First ();
		}

		/// <summary>
		/// Returns the number of genes in the chromosome.
		/// </summary>
		public int Count {
			get { return _genes.Count; }
		}

		/// <summary>
		/// Returns the enumerator.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return _genes.GetEnumerator ();
		}

		/// <summary>
		/// Returns the globally unique ID for this chromosome.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		public Guid Id {
			get { return _id; }
			internal set { _id = value; }
		}

		/// <summary>
		/// Internal property used to indicate whether this chromosome was evaluated by an operator.
		/// This is used to prevent unnessesary evaluations occuring.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		internal bool EvaluatedByOperator { set; get; }

		/// <summary>
		/// Stores the fitness value of the most recent evaluation.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		public double Fitness { get; internal set; }

		/// <summary>
		/// Stores the linearly normalised fitness value of the most recent evaluation.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		public double FitnessNormalised { get; internal set; }

		/// <summary>
		/// Returns the genes.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		public List<Gene> Genes {
			get { return _genes; }
			internal set { _genes = value; }
		}

		/// <summary>
		/// This gets set by the selection process and is available for use by
		/// custom Genetic Operators to determine if the Chromosome was selected 
		/// as an Elite chromosome.
		/// </summary>
		#if !PCL
		[DataMember]
		#endif
		public bool IsElite { set; get; }

		/// <summary>
		/// Returns a binary string representation of the Chromosome.
		/// </summary>
		/// <returns></returns>
		public string ToBinaryString ()
		{
			var binaryString = new StringBuilder ();

			foreach (var gene in _genes) {
				binaryString.Append (gene.BinaryValue.ToString (CultureInfo.InvariantCulture));
			}

			return binaryString.ToString ();
		}

		/// <summary>
		/// Returns a binary string representation of the Chromosome. 
		/// </summary>
		/// <param name="startIndex"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		public string ToBinaryString (int startIndex, int length)
		{
			return this.ToBinaryString ().Substring (startIndex, length);
		}

		/// <summary>
		/// Returns a string representation of the Chromosome.
		/// </summary>
		/// <returns></returns>
		public new string ToString ()
		{
			var toString = new StringBuilder ();

			foreach (var gene in _genes) {

				switch (gene.GeneType) {

				case GeneType.Binary: {
						toString.Append (gene.BinaryValue.ToString (CultureInfo.InvariantCulture));
						break;
					}
				case GeneType.Object: {
						toString.Append (gene.ObjectValue.ToString ());
						toString.Append (" ");
						break;
					}
				case GeneType.Real: {
						toString.Append (gene.RealValue.ToString (CultureInfo.InvariantCulture));
						toString.Append (" ");
						break;

					}
				case GeneType.Integer: {
						toString.Append (((int)(System.Math.Round (gene.RealValue))).ToString (CultureInfo.InvariantCulture));
						toString.Append (" ");
						break;
					}
				}


				//if (gene.GeneType == GeneType.Object) {
				//	toString.Append (gene.ObjectValue.ToString ());
				//} else {
				//	toString.Append (gene.RealValue.ToString (CultureInfo.InvariantCulture));
				//}


			}

			return toString.ToString ().TrimEnd(" ".ToCharArray());
		}

		/// <summary>
		/// Returns a DeepClone of the current instance with the option of clearing the fitness.
		/// </summary>
		/// <returns>The clone.</returns>
		/// <param name="clearFitness">Reset fitness.</param>
		public Chromosome DeepClone (bool clearFitness)
		{
			var result = new Chromosome ();

			if (clearFitness) {
				ClearFitness ();
			} else {
				result.Fitness = this.Fitness;
			}

			result.FitnessNormalised = this.FitnessNormalised;
			result.AddRangeCloned (this.Genes);
			result.EvaluatedByOperator = this.EvaluatedByOperator;
			result.Id = this.Id;
			result.IsElite = this.IsElite;
			result.Tag = this.Tag;

			return result;
		}

		/// <summary>
		/// Returns a DeepClone of the current instance.		/// </summary>
		/// <returns>The clone.</returns>
		public Chromosome DeepClone ()
		{
			return DeepClone (false);
		}

		/// <summary>
		/// Resets the fitness value this method also clears the normalised fitness.
		/// </summary>
		/// <returns>The fitness.</returns>
		public void ClearFitness ()
		{
			this.Fitness = 0;
			this.FitnessNormalised = 0;
		}

		/// <summary>
		/// Evaluates the Chromosome by invoking the specified delegate method.
		/// The fitness function should return a higher 
		/// value for those chromosomes that are deemed fitter.
		/// </summary>
		/// <param name="fitnessFunctionDelegate"></param>
		/// <returns></returns>
		public double Evaluate (FitnessFunction fitnessFunctionDelegate)
		{
			if (fitnessFunctionDelegate == null)
				throw new ArgumentNullException ("nameof (fitnessFunctionDelegate)");
			
			var fitness = fitnessFunctionDelegate.Invoke (this);
			if (fitness < 0 || fitness > 1.0)
				throw new EvaluationException ("The fitness value must be within the range 0.0 to 1.0.");

			Fitness = fitness;
			return fitness;
		}

		/// <summary>
		/// Creates a new GUID for the Chromosome
		/// </summary>
		internal void NewId ()
		{
			_id = Guid.NewGuid ();
		}

		/// <summary>
		/// Clones and adds the specified range.
		/// </summary>
		/// <param name="genes">Genes.</param>
		public void AddRangeCloned(IEnumerable<Gene> genes)
		{
			foreach (var gene in genes) {

				this.Genes.Add(gene.DeepClone());
			}
		}

		/// <summary>
		/// IComparable implementation.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo (Chromosome other)
		{
			return other.Fitness.CompareTo (this.Fitness);
		}

		/// <summary>
		/// Gets or sets the tag property. The tag property is designed to store chromosome meta data
		/// and can be used for any purpose.
		/// Typically the tag property would be used to pass any chromosome related data to the fitness function.
		/// </summary>
		/// <value>The tag.</value>
		#if !PCL
		[DataMember]
		#endif
		public object Tag { get; set; }
	}
}

