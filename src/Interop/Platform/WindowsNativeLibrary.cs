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
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class WindowsNativeLibrary : PlatformNativeLibrary
    {
        public WindowsNativeLibrary()
        {
        }

        internal override LoadLibraryResult Load(string path)
        {
            LoadLibraryResult result;

            // Disable the error dialog that LoadLibrary shows if it cannot find a DLL dependency.
            using (new DisableLoadLibraryErrorDialog())
            {
                IntPtr handle = NativeMethods.LoadLibraryW(path);

                if (handle != IntPtr.Zero)
                {
                    result = new LoadLibraryResult(handle);
                }
                else
                {
                    int lastError = Marshal.GetLastWin32Error();
                    result = new LoadLibraryResult(new Win32Exception(lastError));
                }
            }

            return result;
        }

        internal override IntPtr GetExport(IntPtr libraryHandle, string name)
        {
            return NativeMethods.GetProcAddress(libraryHandle, name);
        }

        private static class NativeConstants
        {
            public const uint SEM_FAILCRITICALERRORS = 1;
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW")]
            public static extern IntPtr LoadLibraryW([In(), MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);

            [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
            public static extern IntPtr GetProcAddress([In()] IntPtr hModule, [In(), MarshalAs(UnmanagedType.LPStr)] string lpProcName);

            [DllImport("kernel32.dll", EntryPoint = "SetErrorMode")]
            public static extern uint SetErrorMode([In()] uint uMode);

            [DllImport("kernel32.dll", EntryPoint = "SetThreadErrorMode")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool SetThreadErrorMode([In()] uint dwNewMode, [Out()] out uint lpOldMode);
        }

        private sealed class DisableLoadLibraryErrorDialog : IDisposable
        {
            private static readonly bool isWindows7OrLater = IsWindows7OrLater();

            private readonly uint oldMode;

            public DisableLoadLibraryErrorDialog()
            {
                oldMode = SetErrorMode(NativeConstants.SEM_FAILCRITICALERRORS);
            }

            private static bool IsWindows7OrLater()
            {
                OperatingSystem operatingSystem = Environment.OSVersion;

                return operatingSystem.Platform == PlatformID.Win32NT && operatingSystem.Version >= new Version(6, 1);
            }

            private static uint SetErrorMode(uint newMode)
            {
                uint oldMode;

                if (isWindows7OrLater)
                {
                    NativeMethods.SetThreadErrorMode(newMode, out oldMode);
                }
                else
                {
                    oldMode = NativeMethods.SetErrorMode(0);
                    NativeMethods.SetErrorMode(oldMode | newMode);
                }

                return oldMode;
            }

            public void Dispose()
            {
                SetErrorMode(oldMode);
            }
        }

    }
}
#endif
