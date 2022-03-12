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

#if NETCOREAPP3_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class DotNetNativeLibrary : PlatformNativeLibrary
    {
        public DotNetNativeLibrary()
        {
        }

        internal override IntPtr GetExport(IntPtr libraryHandle, string name) =>
            NativeLibrary.TryGetExport(libraryHandle, name, out IntPtr address) ? address : IntPtr.Zero;

        internal override LoadLibraryResult Load(string path)
        {
            LoadLibraryResult result;

            try
            {
                result = new LoadLibraryResult(NativeLibrary.Load(path));
            }
            catch (Exception ex)
            {
                result = new LoadLibraryResult(ex);
            }

            return result;
        }
    }
}
#endif
