using DpvClassLibrary.Receivers;
using IO.Swagger.Model;
using System;
using System.Collections.Generic;
using static IO.Swagger.Model.DataPackage;
using static IO.Swagger.Model.DataPackageEnvelope;

namespace DpvClassLibrary.Workers
{
	public class HeartbeatWorker : DataPackageEnvelopeWorkerBase
	{
		protected override ActiveColsEnum ActiveColType => (ActiveColsEnum)1;

		public HeartbeatWorker(IDataPackageEnvelopeReceiver receiver)
			: base(receiver)
		{
		}

		protected override List<DataPackage> GetDataPackages()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected O, but got Unknown
			List<DataPackage> list = new List<DataPackage>();
			list.Add(new DataPackage((ColTypeEnum)1, (bool?)false, new byte[0], (DateTime?)DataPackageEnvelopeAwsReceiver.ServerTime, (long?)GetAndIncrementWorkSequence()));
			return list;
		}

		public void TryPerformSignIn()
		{
			base.ShouldPerformSignInOnNextDoWork = true;
			DoWork();
		}

		public void TryPerformSignOut()
		{
			base.ShouldPerformSignOutOnNextDoWork = true;
			DoWork();
		}

	}
}
