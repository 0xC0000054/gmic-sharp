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

#if NETCOREAPP3_1
using System;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal abstract class NetCoreNativeLibraryLoader : LibraryLoader
    {
        protected NetCoreNativeLibraryLoader(string dllFileExtension) : base(dllFileExtension)
        {
        }

        protected sealed override LoadLibraryResult LoadLibrary(string path)
        {
            try
            {
                IntPtr handle = NativeLibrary.Load(path);
                return new LoadLibraryResult(handle);
            }
            catch (Exception ex)
            {
                return new LoadLibraryResult(ex);
            }
        }

        protected sealed override IntPtr ResolveExportedSymbol(IntPtr libraryHandle, string name)
        {
            if (NativeLibrary.TryGetExport(libraryHandle, name, out IntPtr address))
            {
                return address;
            }
            else
            {
                return IntPtr.Zero;
            }
        }
    }
}
#endif
