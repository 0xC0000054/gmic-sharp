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
    internal sealed class GmicOptions
    {
        public unsafe GmicOptions(string commandLine, string customResourcePath, string customUserPath)
        {
            if (commandLine is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(commandLine));
            }

            this.commandLine = commandLine;
            this.customResourcePath = string.IsNullOrWhiteSpace(customResourcePath) ? null : customResourcePath;
            this.customUserPath = string.IsNullOrWhiteSpace(customUserPath) ? null : customUserPath;
            progress = null;
            abort = null;
        }

        [MarshalAs(UnmanagedType.LPStr)]
        public string commandLine;

        [MarshalAs(UnmanagedType.LPStr)]
        public string customResourcePath;

        [MarshalAs(UnmanagedType.LPStr)]
        public string customUserPath;

        public unsafe float* progress;

        public unsafe byte* abort;
    }
}
