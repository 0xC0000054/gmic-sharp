﻿////////////////////////////////////////////////////////////////////////
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
            string dllFileExtension;

            if (PlatformHelper.IsWindows)
            {
                nativeLibrary = new WindowsNativeLibrary();
                dllFileExtension = ".dll";
            }
            else if (PlatformHelper.IsLinux)
            {
                nativeLibrary = new LinuxNativeLibrary();
                dllFileExtension = ".so";
            }
            else if (PlatformHelper.IsMac)
            {
                nativeLibrary = new MacNativeLibrary();
                dllFileExtension = ".dylib";
            }
            else if (PlatformHelper.IsBsd)
            {
                nativeLibrary = new BsdNativeLibrary();
                dllFileExtension = ".so";
            }
            else
            {
                throw new GmicException("The gmic-sharp native library is not supported on the current platform.");
            }

            libraryName = DllName + dllFileExtension;
            LibrarySearchPaths = GetLibrarySearchPaths(libraryName);
        }

        public IReadOnlyList<string> LibrarySearchPaths { get; }

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

        public IntPtr LoadNativeLibrary()
        {
            IntPtr handle = IntPtr.Zero;

            foreach (string path in LibrarySearchPaths)
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
                        throw new GmicException($"Unable to the gmic-sharp native library from { path }", result.Error);
                    }
                }
            }

            return handle;
        }

        private static IReadOnlyList<string> GetLibrarySearchPaths(string libraryName)
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

            if (PlatformHelper.IsWindows)
            {
                platformName = "win";
            }
            else if (PlatformHelper.IsLinux)
            {
                platformName = "linux";
            }
            else if (PlatformHelper.IsMac)
            {
                platformName = "osx";
            }
            else if (PlatformHelper.IsBsd)
            {
                platformName = "unix";
            }
            else
            {
                throw new PlatformNotSupportedException();
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