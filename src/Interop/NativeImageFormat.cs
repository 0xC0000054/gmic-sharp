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
    internal enum NativeImageFormat
    {
        // 8-bit grayscale with no alpha channel
        Gray8 = 0,
        // Opaque 24-bit color using the BGR format, 8 bits per component
        Bgr888,
        // Opaque 32-bit color using the BGR format, 8 bits per component
        Bgr888x,
        // 32-bit color using the BGRA format, 8 bits per component
        Bgra8888,
        // Opaque 24-bit color using the RGB format, 8 bits per component
        Rgb888,
        // Opaque 32-bit color using the RGB format, 8 bits per component
        Rgb888x,
        // 32-bit color using the RGBA format, 8 bits per component
        Rgba8888
    }
}
