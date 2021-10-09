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

namespace GmicSharp.Interop
{
    internal class BsdNativeLibrary : UnixNativeLibrary
    {
        public BsdNativeLibrary()
        {
        }

        protected sealed override NativeMethodLocation LocationOfNativeMethods => NativeMethodLocation.Libc;
    }
}
