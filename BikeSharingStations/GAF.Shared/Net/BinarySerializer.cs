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

#if !PCL

using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace GAF.Net
{
	/// <summary>
	/// Binary serializer.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public static class BinarySerializer
	{
		/// <summary>
		/// Deserializes specified bytes.
		/// </summary>
		/// <returns>The serialize.</returns>
		/// <param name="byteData">Byte data.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T DeSerialize<T> (byte[] byteData)
		{

			var binaryFormatter = new BinaryFormatter ();
			var memoryStream = new MemoryStream (byteData, 0, byteData.Length);

			var chromosome = (T)binaryFormatter.Deserialize (memoryStream);
			memoryStream.Close ();

			return chromosome;

		}
		/// <summary>
		/// Serialize the specified obj.
		/// </summary>
		/// <param name="obj">Object.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static byte[] Serialize<T> (object obj)
		{
			var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();
			var memoryStream = new MemoryStream ();

			binaryFormatter.Serialize (memoryStream, (T)obj);
			memoryStream.Seek (0, SeekOrigin.Begin);

			var buffer = memoryStream.GetBuffer ();
			memoryStream.Close ();

			return buffer;
		}
	}
}

#endif