////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class GmicErrorInfo
    {
        private const int CommandNameSize = 256;
        private const int ErrorMessageSize = 256;

        public GmicErrorInfo()
        {
            commandName = new byte[CommandNameSize];
            errorMessage = new byte[ErrorMessageSize];
        }

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = CommandNameSize)]
        public byte[] commandName;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ErrorMessageSize)]
        public byte[] errorMessage;
    };
}
