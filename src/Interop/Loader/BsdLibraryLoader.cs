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

namespace GmicSharp.Interop
{
    internal class BsdLibraryLoader : UnixLibraryLoader
    {
        public BsdLibraryLoader() : base(".so")
        {
        }

        protected BsdLibraryLoader(string dllFileExtension) : base(dllFileExtension)
        {
        }

        protected sealed override NativeMethodLocation LocationOfNativeMethods => NativeMethodLocation.Libc;
    }
}
