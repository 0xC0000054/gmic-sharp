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
    internal abstract class UnixNativeLibrary : PlatformNativeLibrary
    {
        protected enum NativeMethodLocation
        {
            Libdl = 0,
            Libc
        }

        protected virtual NativeMethodLocation LocationOfNativeMethods => NativeMethodLocation.Libdl;

        internal sealed override LoadLibraryResult Load(string path)
        {
            IntPtr handle = UnixLoadLibrary(path, NativeConstants.RTLD_NOW);

            if (handle != IntPtr.Zero)
            {
                return new LoadLibraryResult(handle);
            }
            else
            {
                string message = Marshal.PtrToStringAnsi(UnixGetErrorMessage()) ?? string.Empty;

                return new LoadLibraryResult(new ExternalException(message));
            }
        }

        internal sealed override IntPtr GetExport(IntPtr libraryHandle, string name)
        {
            return UnixGetExportedSymbol(libraryHandle, name);
        }

        private IntPtr UnixLoadLibrary(string fileName, int flags)
        {
            switch (LocationOfNativeMethods)
            {
                case NativeMethodLocation.Libdl:
                    return NativeMethods.LibDl.LoadLibrary(fileName, flags);
                case NativeMethodLocation.Libc:
                    return NativeMethods.LibC.LoadLibrary(fileName, flags);
                default:
                    throw new InvalidOperationException($"Unsupported { nameof(NativeMethodLocation) } value: { LocationOfNativeMethods }.");
            }
        }

        private IntPtr UnixGetErrorMessage()
        {
            switch (LocationOfNativeMethods)
            {
                case NativeMethodLocation.Libdl:
                    return NativeMethods.LibDl.GetErrorMessage();
                case NativeMethodLocation.Libc:
                    return NativeMethods.LibC.GetErrorMessage();
                default:
                    throw new InvalidOperationException($"Unsupported { nameof(NativeMethodLocation) } value: { LocationOfNativeMethods }.");
            }
        }

        private IntPtr UnixGetExportedSymbol(IntPtr handle, string symbol)
        {
            switch (LocationOfNativeMethods)
            {
                case NativeMethodLocation.Libdl:
                    return NativeMethods.LibDl.GetExportedSymbol(handle, symbol);
                case NativeMethodLocation.Libc:
                    return NativeMethods.LibC.GetExportedSymbol(handle, symbol);
                default:
                    throw new InvalidOperationException($"Unsupported { nameof(NativeMethodLocation) } value: { LocationOfNativeMethods }.");
            }
        }

        private static class NativeConstants
        {
            internal const int RTLD_NOW = 2;
        }

        private static class NativeMethods
        {
            internal static class LibDl
            {

                [DllImport("libdl", EntryPoint = "dlopen")]
                internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

                [DllImport("libdl", EntryPoint = "dlerror")]
                internal static extern IntPtr GetErrorMessage();

                [DllImport("libdl", EntryPoint = "dlsym")]
                internal static extern IntPtr GetExportedSymbol(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);
            }

            internal static class LibC
            {

                [DllImport("libc", EntryPoint = "dlopen")]
                internal static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

                [DllImport("libc", EntryPoint = "dlerror")]
                internal static extern IntPtr GetErrorMessage();

                [DllImport("libc", EntryPoint = "dlsym")]
                internal static extern IntPtr GetExportedSymbol(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);
            }
        }
    }
}
