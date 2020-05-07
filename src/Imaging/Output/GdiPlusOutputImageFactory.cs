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
using System.Drawing.Imaging;

namespace GmicSharp
{
    internal sealed class GdiPlusOutputImageFactory : IGmicOutputImageFactory
    {
        public GmicBitmap Create(OutputImageInfo outputImageInfo)
        {
            int width = outputImageInfo.Width;
            int height = outputImageInfo.Height;

            PixelFormat format;

            switch (outputImageInfo.Format)
            {
                case GmicPixelFormat.Gray:
                case GmicPixelFormat.Bgr24:
                case GmicPixelFormat.Rgb24:
                    format = PixelFormat.Format24bppRgb;
                    break;
                case GmicPixelFormat.Bgr32:
                case GmicPixelFormat.Rgb32:
                    format = PixelFormat.Format32bppRgb;
                    break;
                case GmicPixelFormat.Bgra32:
                case GmicPixelFormat.Rgba32:
                    format = PixelFormat.Format32bppArgb;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported { nameof(GmicPixelFormat) } value: { outputImageInfo.Format }.");
            }

            return new GdiPlusGmicBitmap(width, height, format);
        }
    }
}
