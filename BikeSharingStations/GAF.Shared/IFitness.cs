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

	namespace GAF
	{
		/// <summary>
		/// This interface is provided to support external programs such as the GAF.Lab GUI application.
		/// </summary>
		public interface IFitness
		{
			/// <summary>
			/// This method should implement the GA's evaluation function.
			/// </summary>
			/// <returns>The fitness.</returns>
			/// <param name="chromosome">Chromosome.</param>
			double EvaluateFitness (Chromosome chromosome);

		}
	}



