using IO.Swagger.Model;
using System.Collections.Generic;

namespace DpvClassLibrary.Receivers
{
	public class DataPackageEnvelopeQueue : IDataPackageEnvelopeReceiver
	{
		private object _lockObject = new object();

		private Queue<DataPackageEnvelope> _queue = new Queue<DataPackageEnvelope>();

		public int EnvelopesInQueue => _queue.Count;

		public void AddDataPackageEnvelope(DataPackageEnvelope packageEnvelope)
		{
			lock (_lockObject)
			{
				_queue.Enqueue(packageEnvelope);
			}
		}

		public DataPackageEnvelope DequeueNextDataPackageEnvelopeToSend()
		{
			lock (_lockObject)
			{
				return _queue.Dequeue();
			}
		}

		internal DataPackageEnvelope PeekNextDataPackageEnvelope()
		{
			lock (_lockObject)
			{
				return _queue.Peek();
			}
		}
	}
}
