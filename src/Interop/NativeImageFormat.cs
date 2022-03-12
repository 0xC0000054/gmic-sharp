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

namespace GmicSharp.Interop
{
    internal enum NativeImageFormat
    {
        // Gray scale without an alpha channel
        Gray = 0,
        // Gray scale with an alpha channel
        GrayAlpha,
        // Red, green and blue color channels without an alpha channel
        Rgb,
        // Red, green and blue color channels with an alpha channel
        RgbAlpha
    }
}
