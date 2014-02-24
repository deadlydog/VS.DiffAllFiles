using System;

namespace VS_DiffAllFiles
{
    static class Guids
    {
		// VS 2013 Guids
        public const string guidVS_DiffAllFiles_VS2013PkgString = "039e6c26-071e-40ea-a890-ac3e7601e83d";
        public const string guidVS_DiffAllFiles_VS2013CmdSetString = "452d92f1-4676-4351-8d1e-68c8a1f7e203";
        public static readonly Guid guidVS_DiffAllFiles_VS2013CmdSet = new Guid(guidVS_DiffAllFiles_VS2013CmdSetString);

		// VS 2012 Guids
		public const string guidVS_DiffAllFiles_VS2012PkgString = "18CCAB91-171E-4A41-B273-67C56B1312A9";
		public const string guidVS_DiffAllFiles_VS2012CmdSetString = "4BC6419D-2C2C-4B6F-B5A1-276EC5CA9D2D";
		public static readonly Guid guidVS_DiffAllFiles_VS2012CmdSet = new Guid(guidVS_DiffAllFiles_VS2012CmdSetString);
    };
}