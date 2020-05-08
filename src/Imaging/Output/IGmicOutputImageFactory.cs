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
    public interface IGmicOutputImageFactory<TGmicBitmap> where TGmicBitmap : GmicBitmap
    {
        /// <summary>
        /// Creates a bitmap from specified output image information.
        /// </summary>
        /// <param name="width">The output image width.</param>
        /// <param name="height">The output image height.</param>
        /// <param name="gmicPixelFormat">The output image G'MIC pixel format.</param>
        /// <returns>The created bitmap.</returns>
        TGmicBitmap Create(int width, int height, GmicPixelFormat gmicPixelFormat);
    }
}
