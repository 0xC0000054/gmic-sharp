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

namespace GmicSharp
{
    /// <summary>
    /// The G'MIC pixel formats
    /// </summary>
    public enum GmicPixelFormat
    {
        /// <summary>
        /// An 8 bits-per-pixel grayscale format.
        /// </summary>
        Gray8,

        /// <summary>
        /// A 16 bits-per-pixel format; Each channel (gray and alpha) is allocated 8 bits.
        /// </summary>
        GrayAlpha16,

        /// <summary>
        /// A 24 bits-per-pixel format; Each channel (red, green and blue) is allocated 8 bits.
        /// </summary>
        Rgb24,

        /// <summary>
        /// A 32 bits-per-pixel format; Each channel (red, green, blue and alpha) is allocated 8 bits.
        /// </summary>
        Rgba32
    }
}
