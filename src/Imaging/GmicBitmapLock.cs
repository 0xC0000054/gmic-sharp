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

using System;

namespace GmicSharp
{
    /// <summary>
    /// Represents that starting pointer and stride of a locked <see cref="GmicBitmap"/>.
    /// </summary>
    public struct GmicBitmapLock
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmicBitmapLock"/> struct.
        /// </summary>
        /// <param name="scan0">The pointer to the start of the image data.</param>
        /// <param name="stride">The stride of the image data.</param>
        public GmicBitmapLock(IntPtr scan0, int stride)
        {
            Scan0 = scan0;
            Stride = stride;
        }

        /// <summary>
        /// Gets the pointer to the start of the image data.
        /// </summary>
        /// <value>
        /// The pointer to the start of the image data.
        /// </value>
        public IntPtr Scan0 { get; }

        /// <summary>
        /// Gets the stride of the image data.
        /// </summary>
        /// <value>
        /// The stride of the image data.
        /// </value>
        public int Stride { get; }
    }
}
