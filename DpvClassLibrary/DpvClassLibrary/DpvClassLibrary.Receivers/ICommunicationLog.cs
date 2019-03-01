namespace DpvClassLibrary.Receivers
{
	public interface ICommunicationLog
	{
		string Request
		{
			get;
			set;
		}

		string Response
		{
			get;
			set;
		}
	}
}
