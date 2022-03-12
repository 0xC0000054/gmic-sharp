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

using Microsoft.Win32.SafeHandles;

namespace GmicSharp.Interop
{
    internal sealed class SafeGmicImageList : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGmicImageList() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            GmicNativeMethods.Instance.GmicImageListDestroy(handle);
            return true;
        }
    }
}
