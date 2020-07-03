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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal abstract class LibraryLoader
    {
        private readonly string libraryName;

        private const string DllName = "libGmicSharpNative";

        protected LibraryLoader(string dllFileExtension)
        {
            if (dllFileExtension is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(dllFileExtension));
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

            IntPtr symbol = ResolveExportedSymbol(libraryHandle, name);

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
                    LoadLibraryResult result = LoadLibrary(path);

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

        protected abstract LoadLibraryResult LoadLibrary(string path);

        protected abstract IntPtr ResolveExportedSymbol(IntPtr libraryHandle, string name);

        private static IReadOnlyList<string> GetLibrarySearchPaths(string libraryName)
        {
            string assemblyDir = Path.GetDirectoryName(typeof(LibraryLoader).Assembly.Location);
            string targetPlatformId = GetTargetPlatfromIdentifer();

            return new List<string>
            {
                // This is the native dependency format used for NuGet packages.
                Path.Combine(assemblyDir, "runtimes", targetPlatformId, "native", libraryName),
                // An alternative path that can be used for local deployments.
                Path.Combine(assemblyDir, "GmicSharpNative", targetPlatformId, libraryName)
            };
        }

        private static string GetTargetPlatfromIdentifer()
        {
            if (PlatformHelper.IsWindows)
            {
                return Environment.Is64BitProcess ? "win-x64" : "win-x86";
            }
            else if (PlatformHelper.IsLinux)
            {
                return Environment.Is64BitProcess ? "linux-x64" : "linux-x86";
            }
            else if (PlatformHelper.IsMac)
            {
                // .NET Core does not support the x86 versions of macOS.
                return "osx-x64";
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        protected readonly struct LoadLibraryResult
        {
            public LoadLibraryResult(Exception error)
            {
                if (error is null)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(error));
                }

                Error = error;
                Handle = IntPtr.Zero;
            }

            public LoadLibraryResult(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(handle));
                }

                Error = null;
                Handle = handle;
            }

            public Exception Error { get; }

            public IntPtr Handle { get; }
        }
    }
}
