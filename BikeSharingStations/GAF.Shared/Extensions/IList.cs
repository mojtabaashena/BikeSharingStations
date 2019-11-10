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

namespace GAF.Extensions
{
    /// <summary>
    /// Extension class for the IList&lt;T&gt; type.
    /// </summary>
    public static class IList
    {

        /// <summary>
        /// This method performs a simple fast Fisher Yates Shuffle.
        /// </summary>
        public static void ShuffleFast<T>(this IList<T> list)
        {
            //Random rng = new Random();
			var rng = Threading.RandomProvider.GetThreadRandom();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>
        /// This method performs a simple fast Fisher Yates Shuffle.
        /// </summary>
        public static void ShuffleFast<T>(this IList<T> list, Random rng)
        {

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        } 

    }
}
