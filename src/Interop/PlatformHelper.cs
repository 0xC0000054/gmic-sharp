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

using System;

namespace GmicSharp.Interop
{
    internal static class PlatformHelper
    {
        private static readonly Lazy<bool> isLinux = new Lazy<bool>(IsRunningOnLinux);
        private static readonly Lazy<bool> isMac = new Lazy<bool>(IsRunningOnOSX);
        private static readonly Lazy<bool> isWindows = new Lazy<bool>(IsRunningOnWindows);

        public static bool IsLinux => isLinux.Value;

        public static bool IsMac => isMac.Value;

        public static bool IsWindows => isWindows.Value;

        private static bool IsRunningOnLinux()
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
        }

        private static bool IsRunningOnOSX()
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
        }

        private static bool IsRunningOnWindows()
        {
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        }
    }
}
