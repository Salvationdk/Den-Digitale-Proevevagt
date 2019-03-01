using IO.Swagger.Model;
using System;

namespace DpvClassLibrary.Receivers
{
	public class ResponseFromServerEventArgs : EventArgs
	{
		public enum ServerResponseStatus
		{
			Empty = 0,
			SuccessfulLogin = 1,
			ExamcodeUnknown = 10,
			ExamcodeExpired = 11
		}

		public ServerResponseStatus Status
		{
			get;
			private set;
		} = ServerResponseStatus.Empty;


		public DataPackageReceipt Receipt
		{
			get;
			set;
		}

		public ResponseFromServerEventArgs(DataPackageReceipt receipt)
		{
			Receipt = receipt;
			if (receipt.get_Status().HasValue)
			{
				Status = (ServerResponseStatus)receipt.get_Status().Value;
			}
		}
	}
}
