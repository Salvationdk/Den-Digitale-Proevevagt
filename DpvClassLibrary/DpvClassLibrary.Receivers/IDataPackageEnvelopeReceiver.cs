using IO.Swagger.Model;

namespace DpvClassLibrary.Receivers
{
	public interface IDataPackageEnvelopeReceiver
	{
		void AddDataPackageEnvelope(DataPackageEnvelope packageEnvelope);
	}
}
