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
	/// Packet manager class to manage a cyclic byte buffer.
	/// </summary>
	[Obsolete ("This class is deprecated. The similarly named class within the GAF.Network package, should be used instead.")]
	public class Queue : IQueue
	{
		/// <summary>
		/// The default size of the queue.
		/// </summary>
		protected const int DefaultSize = 1048576;

		private byte[] _byteQueue;
		private int _inPtr = 0;
		private int _outPtr = 0;
		private int _bytesUsed = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.PacketManager"/> class
		/// with a default buffer size of 1Mb.
		/// </summary>
		public Queue () : this (DefaultSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GAF.Net.PacketManager"/> class.
		/// </summary>
		/// <param name="maxQueueSize">Max queue size.</param>
		public Queue (int maxQueueSize)
		{
			// initialise
			MaxQueueSize = maxQueueSize;

			_byteQueue = new byte[maxQueueSize];
		}

		#region interface methods

		/// <summary>
		/// Adds data to the internal buffer.
		/// </summary>
		/// <param name="data">Data.</param>
		public void Enqueue (byte[] data)
		{
			var dataLength = data.Length;

			if (dataLength > MaxQueueSize - _bytesUsed) {
				throw new ArgumentOutOfRangeException ("data", "Buffer overflow.");
			}

			if (dataLength > 0) {


				var bytesBeforeWrapping = MaxQueueSize - _inPtr;

				//if enough room without wrapping simply copy the array into
				//the buffer and update the inPtr.
				if (bytesBeforeWrapping > dataLength) {

					Array.Copy (data, 0, _byteQueue, _inPtr, dataLength);

				} else {

					//if wrapping is required copy in two steps
					Array.Copy (data, 0, _byteQueue, _inPtr, bytesBeforeWrapping);
					Array.Copy (data, bytesBeforeWrapping, _byteQueue, 0, dataLength - bytesBeforeWrapping);

				}

				IncrementPointer (ref _inPtr, dataLength);
				_bytesUsed += dataLength;
			}

		}

		/// <summary>
		/// Adds data to the internal buffer.
		/// </summary>
		/// <param name="data">Data.</param>
		public void Enqueue (byte data)
		{ 
			var dataArray = new byte[1] { data };
			Enqueue (dataArray);
		}

		/// <summary>
		/// Gets the maximum size of the queue.
		/// </summary>
		/// <value>The size of the queue.</value>
		public int MaxQueueSize { private set; get; }

		/// <summary>
		/// Dequeue the specified number of bytes.
		/// </summary>
		/// <param name="count">Count.</param>
		public byte[] Dequeue (int count)
		{
			//need to ensure there bytes waiting to be dequeued
			if (count > this.Count) {
				throw new ArgumentOutOfRangeException ("count", "Not enough bytes on the queue to be Dequeued.");
			}

			var result = new byte[count];

			var bytesBeforeWrapping = MaxQueueSize - _outPtr;

			if (bytesBeforeWrapping > count) {
				Array.Copy (_byteQueue, _outPtr, result, 0, count);
			} else {
				Array.Copy (_byteQueue, _outPtr, result, 0, bytesBeforeWrapping);
				Array.Copy (_byteQueue, 0, result, bytesBeforeWrapping, count - bytesBeforeWrapping);
			}

			IncrementPointer (ref _outPtr, count);
			_bytesUsed -= count;

			return result;
		}

		/// <summary>
		/// Peek the specified number of bytes. This is similar to Dequeue
		/// but no bytes are removed from the queue.
		/// </summary>
		/// <param name="startIndex">Start index.</param>
		/// <param name="count">Count.</param>
		public byte[] Peek (int startIndex, int count)
		{
			//need to ensure there bytes waiting to be dequeued
			if (count + startIndex > this.Count) {
				throw new ArgumentOutOfRangeException ("count", "Not enough bytes on the queue to be Peeked.");
			}

			var result = new byte[count];
			var bytesBeforeWrapping = MaxQueueSize - _outPtr;

			if (bytesBeforeWrapping > count + startIndex) {
				Array.Copy (_byteQueue, _outPtr + startIndex, result, 0, count);
			} else {
				Array.Copy (_byteQueue, _outPtr + startIndex, result, 0, bytesBeforeWrapping - startIndex);
				Array.Copy (_byteQueue, 0, result, bytesBeforeWrapping - startIndex, count - bytesBeforeWrapping + startIndex);
			}
				
			return result;
		}

		/// <summary>
		/// Gets the number of bytes available in the queue.
		/// </summary>
		/// <value>The size of the queue.</value>

		public int BytesAvailable {
			get {
				return MaxQueueSize - _bytesUsed;
			}
		}

		/// <summary>
		/// Gets the count of bytes in the queue.
		/// </summary>
		/// <value>The count.</value>
		public int Count {
		
			get { 
				return MaxQueueSize - BytesAvailable;
			}
		}

		#endregion

		#region private methods

		private bool IncrementPointer (ref int pointer, int count)
		{
			bool wrapped = false;

			pointer += count;
			var overflow = pointer % MaxQueueSize;

			if (overflow >= 0) {
				pointer = overflow;
				wrapped = true;

			}

			return wrapped;
		}
			
		#endregion
	}
}

#endif

