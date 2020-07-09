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
using System.Globalization;
using System.Linq;

namespace GmicSharp.Interop
{
    internal static class GmicSharpNative
    {
        // Manually loading the native library into the current process allows P/Invokes to work
        // when the library is located in a custom path.
        // The operating system will unload the library when the process exits.
        private static IntPtr nativeLibraryHandle;
        private static bool initialized = false;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <exception cref="GmicException">
        /// The native library could not be found or loaded.
        ///
        /// or
        ///
        /// The GmicSharp and libGmicSharpNative versions do not match.
        /// </exception>
        public static void Initialize()
        {
            if (!initialized)
            {
                LoadNativeLibrary();
                CheckLibraryVersion();

                initialized = true;
            }
        }

        private static void CheckLibraryVersion()
        {
            if (nativeLibraryHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Must call LoadNativeLibrary() before this method.");
            }

            GmicNativeMethods.Instance.GetLibraryVersion(out int nativeLibMajorVersion, out int nativeLibMinorVersion, out _);

            Version managedLibVersion = AssemblyVersionInfo.LibraryVersion;

            if (managedLibVersion.Major == 0)
            {
                // If the major version is 0 the major and minor versions must be the same.
                // TODO: Remove this branch after version 1.0.0 is released.

                if (managedLibVersion.Major != nativeLibMajorVersion ||
                    managedLibVersion.Minor != nativeLibMinorVersion)
                {
                    string message = string.Format(CultureInfo.InvariantCulture,
                                                   "Version mismatch between GmicSharp {0}.{1} and libGmicSharpNative {2}.{3}. The major and minor versions must be the same.",
                                                   managedLibVersion.Major,
                                                   managedLibVersion.Minor,
                                                   nativeLibMajorVersion,
                                                   nativeLibMinorVersion);

                    throw new GmicException(message);
                }
            }
            else
            {
                if (managedLibVersion.Major != nativeLibMajorVersion)
                {
                    string message = string.Format(CultureInfo.InvariantCulture,
                                                   "Version mismatch between GmicSharp {0} and libGmicSharpNative {1}. The major versions must be the same.",
                                                   managedLibVersion.Major,
                                                   nativeLibMajorVersion);

                    throw new GmicException(message);
                }
            }
        }

        private static void LoadNativeLibrary()
        {
            // Exit early if the library has already been loaded.
            if (nativeLibraryHandle != IntPtr.Zero)
            {
                return;
            }

            LibraryLoader loader;

            if (PlatformHelper.IsWindows)
            {
                loader = new WindowsLibraryLoader();
            }
            else if (PlatformHelper.IsLinux)
            {
                loader = new LinuxLibraryLoader();
            }
            else if (PlatformHelper.IsMac)
            {
                loader = new MacLibraryLoader();
            }
            else
            {
                throw new GmicException("The gmic-sharp native library is not supported on the current platform.");
            }

            nativeLibraryHandle = loader.LoadNativeLibrary();

            if (nativeLibraryHandle == IntPtr.Zero)
            {
                throw new GmicException($"The gmic-sharp native library was not found. SearchPaths={ loader.LibrarySearchPaths.Aggregate((a, b) => a + ";" + b) }");
            }

            GmicNativeMethods.Initialize(nativeLibraryHandle, loader);
        }
    }
}
