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

namespace GAF.Net
{
	/// <summary>
	/// IQueue interface.
	/// </summary>
	[Obsolete ("This interface is deprecated. The similarly named interface within the GAF.Network package, should be used instead.")]
	public interface IQueue
	{
		/// <summary>
		/// Enqueue the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		void Enqueue (byte[] data);

		/// <summary>
		/// Enqueue the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		void Enqueue (byte data);

		/// <summary>
		/// Dequeue the specified number of bytes.
		/// </summary>
		/// <param name="count">Count.</param>
		byte[] Dequeue (int count);

		/// <summary>
		/// Peek the specified number of bytes. This is similar to Dequeue
		/// but no bytes are removed from the queue.
		/// </summary>
		/// <param name = "startIndex"></param>
		/// <param name="length">Length.</param>
		byte[] Peek (int startIndex, int length);

		/// <summary>
		/// Gets the size of the max queue.
		/// </summary>
		/// <value>The size of the max queue.</value>
		int MaxQueueSize{ get; }

		/// <summary>
		/// Gets the bytes available.
		/// </summary>
		/// <value>The bytes available.</value>
		int BytesAvailable { get;}

		/// <summary>
		/// Gets the count of bytes in the queue.
		/// </summary>
		/// <value>The count.</value>
		int Count { get;}
	}
}

#endif