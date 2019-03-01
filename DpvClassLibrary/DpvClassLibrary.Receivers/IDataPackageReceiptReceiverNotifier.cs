namespace DpvClassLibrary.Receivers
{
	public interface IDataPackageReceiptReceiverNotifier
	{
		event ResponseFromServerDelegate ResponseReceived;
	}
}
