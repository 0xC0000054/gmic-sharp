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
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class GmicImageListImageData
    {
        public uint width;
        public uint height;
        public GmicImageListPixelData pixels;
        public NativeImageFormat format;

        public GmicImageListImageData()
        {
            width = 0;
            height = 0;
            pixels = new GmicImageListPixelData();
            format = NativeImageFormat.Gray8;
        }
    }
}
