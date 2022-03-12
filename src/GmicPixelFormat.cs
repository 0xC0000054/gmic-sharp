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

namespace GmicSharp
{
    /// <summary>
    /// The G'MIC pixel formats
    /// </summary>
    public enum GmicPixelFormat
    {
        /// <summary>
        /// Gray scale without an alpha channel.
        /// </summary>
        Gray,

        /// <summary>
        /// Gray scale with an alpha channel.
        /// </summary>
        GrayAlpha,

        /// <summary>
        /// Red, green and blue color channels without an alpha channel.
        /// </summary>
        Rgb,

        /// <summary>
        /// Red, green and blue color channels with an alpha channel.
        /// </summary>
        RgbAlpha
    }
}
