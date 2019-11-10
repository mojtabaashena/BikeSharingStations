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
	
using System.Collections.Generic;
using System.Collections;

#if !PCL

using System;

namespace GAF.Net
{
	/// <summary>
	/// Synchronised queue.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class SynchronisedQueue : Queue
	{
		private readonly object _syncLock = new object ();

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.SynchronisedQueue"/> class.
		/// </summary>
		public SynchronisedQueue () : base (DefaultSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.SynchronisedQueue"/> class.
		/// </summary>
		/// <param name="maxQueueSize">Max queue size.</param>
		public SynchronisedQueue (int maxQueueSize) : base (maxQueueSize)
		{
		}

		/// <summary>
		/// Enqueue the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public new void Enqueue (byte[] data)
		{
			lock (_syncLock) {
				base.Enqueue (data);
			}
		}

		/// <summary>
		/// Enqueue the specified data.
		/// </summary>
		/// <param name="data">Data.</param>
		public new void Enqueue (byte data)
		{
			lock (_syncLock) {
				base.Enqueue (data);
			}
		}

		/// <summary>
		/// Dequeue the specified number of bytes.
		/// </summary>
		/// <param name="count">Count.</param>
		public new byte[] Dequeue (int count)
		{
			lock (_syncLock) {
				return base.Dequeue (count);
			}
		}

		/// <summary>
		/// Peek the specified number of bytes. This is similar to Dequeue
		/// but no bytes are removed from the queue.
		/// </summary>
		/// <param name = "startIndex"></param>
		/// <param name="length">Length.</param>
		public new byte[] Peek (int startIndex, int length)
		{
			lock (_syncLock) {
				return base.Peek (startIndex, length);
			}
		}

		/// <summary>
		/// Gets the size of the max queue.
		/// </summary>
		/// <value>The size of the max queue.</value>
		public new int MaxQueueSize { 
			get {
				lock (_syncLock) {
					return base.MaxQueueSize;
				}
			}
		}

		/// <summary>
		/// Gets the bytes available.
		/// </summary>
		/// <value>The bytes available.</value>
		public new int BytesAvailable { 
			get{
				lock (_syncLock) {
					return base.BytesAvailable;
				}
			}
		}

		/// <summary>
		/// Gets the count of bytes in the queue.
		/// </summary>
		/// <value>The count.</value>
		public  new int Count {
			get {
				lock (_syncLock) {
					return base.Count;
				}
			}
		}
	}
}

#endif