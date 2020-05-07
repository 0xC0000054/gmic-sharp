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

using Microsoft.Win32.SafeHandles;

namespace GmicSharp.Interop
{
    internal abstract class SafeGmicImageList : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeGmicImageList(bool ownsHandle) : base(ownsHandle)
        {
        }
    }

    internal sealed class SafeGmicImageListX64 : SafeGmicImageList
    {
        private SafeGmicImageListX64() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            Gmic_X64.GmicImageListDestroy(handle);
            return true;
        }
    }

    internal sealed class SafeGmicImageListX86 : SafeGmicImageList
    {
        private SafeGmicImageListX86() : base(true)
        {
        }

        protected override bool ReleaseHandle()
        {
            Gmic_X86.GmicImageListDestroy(handle);
            return true;
        }
    }
}
