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
using System.Collections;

namespace GAF
{
	/// <summary>
	/// This class is a simple math helper class.
	/// </summary>
	public static class Math
	{
		/// <summary>
		/// Rounds a number to r=the nearest even whole number.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static int RoundEven (double value)
		{
			if (value < 0) {
				throw new ArgumentException ("A negative number was specified");
			}

			var result = System.Math.Round (value, MidpointRounding.AwayFromZero);

			if (result % 2 > 0) {
				if (value < result) {
					result--;
				} else {
					result++;
				}
			}

			return Convert.ToInt32 (result);
		}

		/// <summary>
		/// Returns a posive integer value, i.e. -10 becomes 10.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static long Positive (long value)
		{
			if (value < 0) value = value * -1;
			return value;
		}
		/// <summary>
		/// Returns a range constant that can be used for normalisation.
		/// </summary>
		/// <returns>The range constant.</returns>
		/// <param name="range">Range.</param>
		/// <param name="numberOfBits">Number of bits.</param>
		public static double GetRangeConstant (double range, int numberOfBits)
		{
			return range / (System.Math.Pow (2, numberOfBits) - 1);
		}

		/// <summary>
		/// Compares two double values for equality.
		/// </summary>
		/// <returns><c>true</c>, if equal was abouted, <c>false</c> otherwise.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public static bool AboutEqual (double x, double y)
		{
			double epsilon = System.Math.Max (System.Math.Abs (x), System.Math.Abs (y)) * 1E-15;
			return System.Math.Abs (x - y) <= epsilon;
		}

		/// <summary>
		/// Changes the range of a number whilst maintaining ratio.
		/// </summary>
		/// <returns>The range.</returns>
		/// <param name="oldValue">Old value.</param>
		/// <param name="oldMin">Old minimum.</param>
		/// <param name="oldMax">Old max.</param>
		/// <param name="newMin">New minimum.</param>
		/// <param name="newMax">New max.</param>
		public static double ReRange (double oldValue, double oldMin, double oldMax, double newMin, double newMax)
		{
			return (((oldValue - oldMin) * (newMax - newMin)) / (oldMax - oldMin)) + newMin;
		}

		/// <summary>
		/// Calculates the Euclidean distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double EuclideanDistance (IList<double> vector1, IList<double> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");
			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = vector1 [i] - vector2 [i];
				sum += d * d;
			}
			return System.Math.Sqrt (sum);
		}

		/// <summary>
		/// Calculates the Euclidean distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double EuclideanDistance (IList<int> vector1, IList<int> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");

			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = vector1 [i] - vector2 [i];
				sum += d * d;
			}
			return System.Math.Sqrt (sum);
		}

		/// <summary>
		/// Calculates the Manhattan distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double ManhattanDistance (IList<int> vector1, IList<int> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");

			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = System.Math.Abs (vector1 [i] - vector2 [i]);
				sum += d;
			}

			return sum;
		}

		/// <summary>
		/// Calculates the Manhattan distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double ManhattanDistance (IList<double> vector1, IList<double> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");

			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = System.Math.Abs (vector1 [i] - vector2 [i]);
				sum += d;
			}

			return sum;
		}

		/// <summary>
		/// Calculates the Chebyshevs distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double ChebyshevDistance (IList<int> vector1, IList<int> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");

			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = System.Math.Abs (vector1 [i] - vector2 [i]);
				sum = System.Math.Max (sum, d);
			}

			return sum;

		}

		/// <summary>
		/// Calculates the Chebyshevs distance between two vector elements.
		/// </summary>
		/// <returns>The distance.</returns>
		/// <param name="vector1">Vector1.</param>
		/// <param name="vector2">Vector2.</param>
		public static double ChebyshevDistance (IList<double> vector1, IList<double> vector2)
		{
			var count = vector1.Count;
			if (vector2.Count != count)
				throw new ArgumentException ("Vectors must be of equal length.");

			var sum = 0.0;
			for (int i = 0; i < count; i++) {
				var d = System.Math.Abs (vector1 [i] - vector2 [i]);
				sum = System.Math.Max (sum, d);
			}

			return sum;

		}
	}
}
