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

using System;
using System.Drawing.Imaging;

namespace GmicSharp
{
    /// <summary>
    /// The output image factory for <see cref="GdiPlusGmicBitmap"/>.
    /// </summary>
    /// <seealso cref="IGmicOutputImageFactory{TGmicBitmap}" />
    public sealed class GdiPlusOutputImageFactory : IGmicOutputImageFactory<GdiPlusGmicBitmap>
    {

        /// <summary>
        /// Creates a <see cref="GdiPlusGmicBitmap"/> from specified output image information.
        /// </summary>
        /// <param name="width">The output image width.</param>
        /// <param name="height">The output image height.</param>
        /// <param name="gmicPixelFormat">The output image G'MIC pixel format.</param>
        /// <returns>The created bitmap.</returns>
        public GdiPlusGmicBitmap Create(int width, int height, GmicPixelFormat gmicPixelFormat)
        {
            PixelFormat format;

            switch (gmicPixelFormat)
            {
                case GmicPixelFormat.Gray8:
                case GmicPixelFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;
                case GmicPixelFormat.GrayAlpha16:
                case GmicPixelFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported { nameof(GmicPixelFormat) } value: { gmicPixelFormat }.");
            }

            return new GdiPlusGmicBitmap(width, height, format);
        }
    }
}
