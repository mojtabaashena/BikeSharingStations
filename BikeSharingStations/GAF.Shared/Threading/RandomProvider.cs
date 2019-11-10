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
using System.Threading;

namespace GAF.Threading
{
    /// <summary>
    /// Provides one Random class per thread.
    /// </summary>
    public static class RandomProvider
    {
        private static int _seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> randomWrapper =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));
        /// <summary>
        /// Returns a thread safe System.Random class
        /// </summary>
        /// <returns></returns>
        public static Random GetThreadRandom()
        {
            return randomWrapper.Value;
        }
    }
}
