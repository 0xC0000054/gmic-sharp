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
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class WindowsLibraryLoader : LibraryLoader
    {
        private const string DllFileExtension = ".dll";

        public WindowsLibraryLoader() : base(DllFileExtension)
        {
        }

        protected override LoadLibraryResult LoadLibrary(string path)
        {
            IntPtr handle = NativeMethods.LoadLibraryW(path);

            if (handle != IntPtr.Zero)
            {
                return new LoadLibraryResult(handle);
            }
            else
            {
                return new LoadLibraryResult(new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", EntryPoint = "LoadLibraryW")]
            public static extern IntPtr LoadLibraryW([In(), MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);
        }
    }
}
