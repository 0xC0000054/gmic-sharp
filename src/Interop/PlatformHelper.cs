////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal static class PlatformHelper
    {
        private static readonly Lazy<Platform> currentPlatform = new Lazy<Platform>(GetCurrentPlatform);

        public static Platform CurrentPlatform => currentPlatform.Value;

        private static Platform GetCurrentPlatform()
        {
            Platform platform = Platform.Unknown;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = Platform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                platform = Platform.MacOS;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                platform = Platform.Unix;
            }

            return platform;
        }
    }
}
