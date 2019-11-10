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
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.CompilerServices;

namespace GAF
{
	/// <summary>
	/// An Enum definition that indicates the gene type.
	/// </summary>
	public enum GeneType
	{
		/// <summary>
		/// Binary genes within the chromosome.
		/// </summary>
		Binary,
		/// <summary>
		/// Genes as integers within the chromosome.
		/// </summary>
		Integer,
		/// <summary>
		/// Genes as real numbers (double) within the chromosome.
		/// </summary>
		Real,
		/// <summary>
		/// Custom object provided as a gene.
		/// </summary>
		Object
	}

	/// <summary>
	/// This class represents a gene.
	/// </summary>
#if !PCL
	[Serializable]
#endif
	public class Gene
	{
		private object _objectValue = null;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Gene ()
		{
			Id = Guid.NewGuid ();
		}


		/// <summary>
		/// Constructor that accepts an object as a gene type. Passing a boolean, 
		/// integer or double will set the GeneType appropriately. 
		/// Any other type will be considered to be an object.
		/// </summary>
		/// <param name="value"></param>
		public Gene (object value)
		{
			Id = Guid.NewGuid ();
			_objectValue = value;
			SetValue (ref value);
		}

		/// <summary>
		/// Gets or sets the identifier.
		/// </summary>
		/// <value>The identifier.</value>
		public Guid Id { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public object ObjectValue {
			set {
				_objectValue = value;
				SetValue (ref value);
			}
			get { return _objectValue; }
		}

		private void SetValue (ref object value)
		{
			//determine Gene Type
			if (value is int) {
				this.GeneType = GeneType.Integer;
			} else if (value is bool) {
				this.GeneType = GeneType.Binary;
			} else if (value is double) {
				this.GeneType = GeneType.Real;
			} else {
				this.GeneType = GeneType.Object;
			}

		}

		/// <summary>
		/// Returns the Gene Type.
		/// </summary>
		public GeneType GeneType { get; set; }

		/// <summary>
		/// Returns a ObjectValue of 0 or 1 as an integer. For genes 
		/// based on Real numbers, a ObjectValue greater than 0 will 
		/// return a 1. For object based genes 1 will be returned for
		/// non-null objects and 0 for null objects.
		/// </summary>
		public int BinaryValue {
			get {
				switch (this.GeneType) {
				case GeneType.Real:
				case GeneType.Binary:
				case GeneType.Integer: {
						var convertedValue = Convert.ToDouble (ObjectValue);
						return convertedValue > 0 ? 1 : 0;
					}
				case GeneType.Object: {
						return ObjectValue != null ? 1 : 0;
					}
				default: {
						throw new GeneException ("Unable to determine the gene type.");
					}
				}
			}
		}

		/// <summary>
		/// Returns the Real number ObjectValue. For genes based on 
		/// binary numbers, 0 or 1 will be returned as a double.
		/// For object based genes NaN will be returned.
		/// </summary>
		public double RealValue {
			get {
				double convertedValue;
				switch (this.GeneType) {
				case GeneType.Integer:
				case GeneType.Real:
					convertedValue = Convert.ToDouble (ObjectValue);
					break;

				case GeneType.Binary:
					convertedValue = (double)this.BinaryValue;
					break;

				case GeneType.Object:
					convertedValue = double.NaN;
					//convertedValue = this.ObjectValue == null ? 0.0 : 1.0;
					break;

				default:
					convertedValue = double.NaN;
					break;
				}

				return convertedValue;
			}
		}

		/// <summary>
		/// Returns a new object, cloned from this instance.
		/// </summary>
		/// <returns>The clone.</returns>
		public Gene DeepClone ()
		{
			var result = new Gene ();
			result.GeneType = this.GeneType;
			result.ObjectValue = this.ObjectValue;

			if (result.GeneType != this.GeneType)
				throw new GeneCloneException ("Deep Clone of Gene is inconsistent.");

			return result;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="T:GAF.Gene"/> object.
		/// </summary>
		/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.</returns>
		public override int GetHashCode ()
		{
			//this returns a hash based on the object instance. 
			//It does not change if the properties change
			return RuntimeHelpers.GetHashCode (this);
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:GAF.Gene"/>.
		/// </summary>
		/// <param name="obj">The <see cref="object"/> to compare with the current <see cref="T:GAF.Gene"/>.</param>
		/// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current <see cref="T:GAF.Gene"/>; otherwise, <c>false</c>.</returns>
		public override bool Equals (object obj)
		{
			Gene gene = obj as Gene;

            if (gene == null)
				return false;
			
			return Object.ReferenceEquals (this, obj);
		}
	}
}
	

