using IO.Swagger.Model;
using System;


namespace DpvClassLibrary.Tools
{
    public static class ColTypeExtensionMethods
    {
        public static DataPackageEnvelope.ActiveColsEnum ToActiveColsEnum(this DataPackage.ColTypeEnum colType)
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
                    return (DataPackageEnvelope.ActiveColsEnum)1;
                case (DataPackage.ColTypeEnum)1:
                    return (DataPackageEnvelope.ActiveColsEnum)2;
                case (DataPackage.ColTypeEnum)2:
                    return (DataPackageEnvelope.ActiveColsEnum)3;
                case (DataPackage.ColTypeEnum)3:
                    return (DataPackageEnvelope.ActiveColsEnum)4;
                case (DataPackage.ColTypeEnum)4:
                    return (DataPackageEnvelope.ActiveColsEnum)5;
                case (DataPackage.ColTypeEnum)5:
                    return (DataPackageEnvelope.ActiveColsEnum)6;
                default:
                    throw new ArgumentException("Unable to convert type " + colType + " to ActiveColsEnum");
            }
        }

        public static DataPackage.ColTypeEnum ToColTypeEnum(this DataPackageEnvelope.ActiveColsEnum activeColType)
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
                    return (DataPackage.ColTypeEnum)1;
                case 1:
                    return (DataPackage.ColTypeEnum)2;
                case 2:
                    return (DataPackage.ColTypeEnum)3;
                case 3:
                    return (DataPackage.ColTypeEnum)4;
                case 4:
                    return (DataPackage.ColTypeEnum)5;
                case 5:
                    return (DataPackage.ColTypeEnum)6;
                default:
                    throw new ArgumentException("Unable to convert type " + activeColType + " to ColTypeEnum");
            }
        }
    }
}
