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

using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct RedGrayUnion
    {
        [FieldOffset(0)]
        public float* red;
        [FieldOffset(0)]
        public float* gray;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe class GmicImageListPixelData
    {
        public RedGrayUnion redGrayUnion;
        public float* green;
        public float* blue;
        public float* alpha;
    }
}
