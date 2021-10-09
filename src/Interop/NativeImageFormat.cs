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
    internal enum NativeImageFormat
    {
        // 8-bit grayscale with no alpha channel
        Gray8 = 0,
        // 8-bit grayscale with an 8-bit alpha channel
        GrayAlpha88,
        // Opaque 24-bit color using the RGB format, 8 bits per component
        Rgb888,
        // 32-bit color using the RGBA format, 8 bits per component
        Rgba8888
    }
}
