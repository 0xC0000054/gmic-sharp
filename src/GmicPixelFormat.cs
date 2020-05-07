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

namespace GmicSharp
{
    /// <summary>
    /// The G'MIC pixel formats
    /// </summary>
    public enum GmicPixelFormat
    {
        /// <summary>
        /// An 8-bits-per-pixel gray scale format.
        /// </summary>
        Gray,

        /// <summary>
        /// An interleaved 24-bits-per-pixel format that uses BGR color ordering.
        /// </summary>
        Bgr24,

        /// <summary>
        /// An interleaved 24-bits-per-pixel format that uses RGB color ordering.
        /// </summary>
        Rgb24,

        /// <summary>
        /// An interleaved 32-bits-per-pixel format that uses BGR color ordering.
        /// </summary>
        Bgr32,

        /// <summary>
        /// An interleaved 32-bits-per-pixel format that uses RGB color ordering.
        /// </summary>
        Rgb32,

        /// <summary>
        /// An interleaved 32-bits-per-pixel format that uses BGRA color ordering.
        /// </summary>
        Bgra32,

        /// <summary>
        /// An interleaved 32-bits-per-pixel format that uses RGBA color ordering.
        /// </summary>
        Rgba32
    }
}
