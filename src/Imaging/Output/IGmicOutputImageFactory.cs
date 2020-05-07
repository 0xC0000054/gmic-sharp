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
    /// A factory interface for the output images.
    /// </summary>
    public interface IGmicOutputImageFactory
    {
        /// <summary>
        /// Creates a bitmap from specified output image information.
        /// </summary>
        /// <param name="outputImageInfo">The output image information.</param>
        /// <returns>The created bitmap.</returns>
        GmicBitmap Create(OutputImageInfo outputImageInfo);
    }
}
