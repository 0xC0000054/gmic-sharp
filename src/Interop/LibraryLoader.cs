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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class LibraryLoader
    {
        private readonly string libraryName;
        private readonly PlatformNativeLibrary nativeLibrary;

        private const string DllName = "libGmicSharpNative";

        public LibraryLoader()
        {
            try
            {
                nativeLibrary = PlatformNativeLibrary.CreateInstance(PlatformHelper.CurrentPlatform);
            }
            catch (PlatformNotSupportedException ex)
            {
                throw CreatePlatformNotSupportedException(ex);
            }

            string dllFileExtension;

            switch (PlatformHelper.CurrentPlatform)
            {
                case Platform.Windows:
                    dllFileExtension = ".dll";
                    break;
                case Platform.MacOS:
                    dllFileExtension = ".dylib";
                    break;
                case Platform.Unix:
                    dllFileExtension = ".so";
                    break;
                case Platform.Unknown:
                default:
                    throw CreatePlatformNotSupportedException();
            }

            libraryName = DllName + dllFileExtension;
        }

        public TDelegate GetExport<TDelegate>(IntPtr libraryHandle, string name) where TDelegate : Delegate
        {
            if (libraryHandle == IntPtr.Zero)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(libraryHandle));
            }

            if (name is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(name));
            }

            IntPtr symbol = nativeLibrary.GetExport(libraryHandle, name);

            if (symbol == IntPtr.Zero)
            {
                throw new GmicException($"The entrypoint '{ name }' was not found in '{ libraryName }'");
            }

            return Marshal.GetDelegateForFunctionPointer<TDelegate>(symbol);
        }

        /// <summary>
        /// Loads the native library.
        /// </summary>
        /// <returns>The native library handle</returns>
        /// <exception cref="GmicException">Unable to load the gmic-sharp native library.</exception>
        public IntPtr LoadNativeLibrary()
        {
            IntPtr handle = IntPtr.Zero;

            List<string> librarySearchPaths = GetLibrarySearchPaths(libraryName);

            foreach (string path in librarySearchPaths)
            {
                if (File.Exists(path))
                {
                    PlatformNativeLibrary.LoadLibraryResult result = nativeLibrary.Load(path);

                    if (result.Handle != IntPtr.Zero)
                    {
                        handle = result.Handle;
                        break;
                    }
                    else
                    {
                        throw new GmicException($"Unable to load the gmic-sharp native library from { path }", result.Error);
                    }
                }
            }

            if (handle == IntPtr.Zero)
            {
                throw new GmicException($"The gmic-sharp native library was not found. SearchPaths={ librarySearchPaths.Aggregate((a, b) => a + ";" + b) }");
            }

            return handle;
        }

        private static GmicException CreatePlatformNotSupportedException(Exception inner = null) =>
            new GmicException("The gmic-sharp native library is not supported on the current platform.", inner);

        private static List<string> GetLibrarySearchPaths(string libraryName)
        {
            string assemblyDir = Path.GetDirectoryName(typeof(LibraryLoader).Assembly.Location);
            string targetPlatformId = GetTargetPlatfromIdentifer();

            return new List<string>
            {
                // The main path used for local deployments.
                Path.Combine(assemblyDir, "GmicSharpNative", targetPlatformId, libraryName),
                // This is the native dependency format used for NuGet packages.
                Path.Combine(assemblyDir, "runtimes", targetPlatformId, "native", libraryName)
            };
        }

        private static string GetTargetPlatfromIdentifer()
        {
            string platformName;
            string processorArchitecture;

            switch (PlatformHelper.CurrentPlatform)
            {
                case Platform.Windows:
                    platformName = "win";
                    break;
                case Platform.MacOS:
                    platformName = "osx";
                    break;
                case Platform.Unix:
                    platformName = "linux";
                    break;
                case Platform.Unknown:
                default:
                    throw CreatePlatformNotSupportedException();
            }

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                    processorArchitecture = "x86";
                    break;
                case Architecture.X64:
                    processorArchitecture = "x64";
                    break;
                case Architecture.Arm:
                    processorArchitecture = "arm";
                    break;
                case Architecture.Arm64:
                    processorArchitecture = "arm64";
                    break;
                default:
                    throw new PlatformNotSupportedException($"Unsupported process architecture: { RuntimeInformation.ProcessArchitecture }.");
            }

            return platformName + "-" + processorArchitecture;
        }
    }
}
