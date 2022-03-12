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

using System;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class GmicImageListImageData
    {
        public int version;
        public uint width;
        public uint height;
        public NativeImageFormat format;
        public GmicImageListPixelData pixels;
        public IntPtr name;
        public int nameLength;

        public GmicImageListImageData()
        {
            version = 1;
            width = 0;
            height = 0;
            format = NativeImageFormat.Gray;
            pixels = new GmicImageListPixelData();
            name = IntPtr.Zero;
            nameLength = 0;
        }
    }
}
