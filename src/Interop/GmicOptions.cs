﻿////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021, 2022 Nicholas Hayes
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
        public unsafe GmicOptions(string commandLine,
                                  string customResourcePath,
                                  string hostName)
        {
            if (commandLine is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(commandLine));
            }

            this.commandLine = commandLine;
            this.customResourcePath = string.IsNullOrWhiteSpace(customResourcePath) ? null : customResourcePath;
            // The host name can be null, this makes the native code use its default host name.
            this.hostName = hostName;
            progress = null;
            abort = null;
        }

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string commandLine;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string customResourcePath;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string hostName;

        public unsafe float* progress;

        public unsafe byte* abort;
    }
}
