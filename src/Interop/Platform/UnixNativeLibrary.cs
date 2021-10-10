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

// When targeting .NET Core 3.0 or greater the DotNetNativeLibrary class will be used instead
// of this platform-specific implementation.
// The DotNetNativeLibrary class uses System.Runtime.InteropServices.NativeLibrary to support
// cross-platform loading of native libraries. See DotNetNativeLibrary.cs
#if !NETCOREAPP3_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class UnixNativeLibrary : PlatformNativeLibrary
    {
        internal sealed override LoadLibraryResult Load(string path)
        {
            IntPtr handle = NativeMethods.LoadLibrary(path, NativeConstants.RTLD_NOW);

            if (handle != IntPtr.Zero)
            {
                return new LoadLibraryResult(handle);
            }
            else
            {
                string message = Marshal.PtrToStringAnsi(NativeMethods.GetErrorMessage()) ?? string.Empty;

                return new LoadLibraryResult(new ExternalException(message));
            }
        }

        internal sealed override IntPtr GetExport(IntPtr libraryHandle, string name)
        {
            return NativeMethods.GetExportedSymbol(libraryHandle, name);
        }

        private static class NativeConstants
        {
            internal const int RTLD_NOW = 2;
        }

        private static class NativeMethods
        {
            [DllImport("libdl", EntryPoint = "dlopen")]
            internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

            [DllImport("libdl", EntryPoint = "dlerror")]
            internal static extern IntPtr GetErrorMessage();

            [DllImport("libdl", EntryPoint = "dlsym")]
            internal static extern IntPtr GetExportedSymbol(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);
        }
    }
}
#endif
