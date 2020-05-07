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
    /// The output image information.
    /// </summary>
    public struct OutputImageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputImageInfo"/> structure.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        public OutputImageInfo(int width, int height, GmicPixelFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        /// Gets the output image width.
        /// </summary>
        /// <value>
        /// The output image width.
        /// </value>
        public int Width { get; }

        /// <summary>
        /// Gets the output image height.
        /// </summary>
        /// <value>
        /// The output image height.
        /// </value>
        public int Height { get; }

        /// <summary>
        /// Gets the output image pixel format.
        /// </summary>
        /// <value>
        /// The output image pixel format.
        /// </value>
        public GmicPixelFormat Format { get; }
    }
}
