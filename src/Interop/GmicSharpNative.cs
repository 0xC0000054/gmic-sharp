////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021, 2022 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;

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
        /// </exception>
        public static void Initialize()
        {
            if (!initialized)
            {
                LoadNativeLibrary();

                initialized = true;
            }
        }

        private static void LoadNativeLibrary()
        {
            // Exit early if the library has already been loaded.
            if (nativeLibraryHandle != IntPtr.Zero)
            {
                return;
            }

            LibraryLoader loader = new LibraryLoader();

            nativeLibraryHandle = loader.LoadNativeLibrary();

            GmicNativeMethods.Initialize(nativeLibraryHandle, loader);
        }
    }
}
