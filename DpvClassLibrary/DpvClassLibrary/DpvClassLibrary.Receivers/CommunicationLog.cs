namespace DpvClassLibrary.Receivers
{
	public class CommunicationLog : ICommunicationLog
	{
		public string Request
		{
			get;
			set;
		}

		public string Response
		{
			get;
			set;
		}

		public override string ToString()
		{
			return "CommunicationLog = {Request = {'" + Request + "'}, Response = {'" + Response + "'} }";
		}
	}
}
