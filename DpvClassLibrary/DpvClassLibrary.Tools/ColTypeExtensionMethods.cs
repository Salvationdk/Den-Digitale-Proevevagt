using IO.Swagger.Model;
using System;
using static IO.Swagger.Model.CollectorConfig;
using static IO.Swagger.Model.DataPackageEnvelope;

namespace DpvClassLibrary.Tools
{
	public static class ColTypeExtensionMethods
	{
		public static ActiveColsEnum ToActiveColsEnum(this ColTypeEnum colType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected I4, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			switch (colType - 1)
			{
			case 0:
				return (ActiveColsEnum)1;
			case (ColTypeEnum)1:
				return (ActiveColsEnum)2;
			case (ColTypeEnum)2:
				return (ActiveColsEnum)3;
			case (ColTypeEnum)3:
				return (ActiveColsEnum)4;
			case (ColTypeEnum)4:
				return (ActiveColsEnum)5;
			case (ColTypeEnum)5:
				return (ActiveColsEnum)6;
			default:
				throw new ArgumentException("Unable to convert type " + colType + " to ActiveColsEnum");
			}
		}

		public static ColTypeEnum ToColTypeEnum(this ActiveColsEnum activeColType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected I4, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			switch ((int)activeColType - 1)
			{
			case 0:
				return (ColTypeEnum) 1;
			case 1:
                    return (ColTypeEnum)2;
			case 2:
                    return (ColTypeEnum)3;
			case 3:
                    return (ColTypeEnum)4;
			case 4:
                    return (ColTypeEnum)5;
			case 5:
                    return (ColTypeEnum)6;
			default:
				throw new ArgumentException("Unable to convert type " + activeColType + " to ColTypeEnum");
			}
		}
	}
}
