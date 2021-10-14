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

using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace GmicSharp
{
    /// <summary>
    /// A <see cref="GmicBitmap"/> that uses the GDI+ <see cref="Bitmap"/> class.
    /// </summary>
    /// <seealso cref="GmicBitmap" />
    /// <seealso cref="Bitmap"/>
    public sealed class GdiPlusGmicBitmap : GmicBitmap
    {
        private Bitmap bitmap;

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPlusGmicBitmap"/> class.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        public GdiPlusGmicBitmap(Bitmap bitmap) : base()
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            this.bitmap = CloneOrConvertBitmap(bitmap);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPlusGmicBitmap"/> class.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <exception cref="ArgumentNullException"><paramref name="image"/> is null.</exception>
        public GdiPlusGmicBitmap(Image image) : base()
        {
            if (image is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(image));
            }

            bitmap = ConvertImageToBitmap(image);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPlusGmicBitmap"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The GDI+ pixel format.</param>
        /// <exception cref="ArgumentException">The GDI+ pixel format is not supported.</exception>
        internal GdiPlusGmicBitmap(int width, int height, PixelFormat format) : base()
        {
            if (!IsSupportedPixelFormat(format))
            {
                ExceptionUtil.ThrowArgumentException("The GDI+ PixelFormat must be Format24bppRgb or Format32bppArgb.");
            }

            bitmap = new Bitmap(width, height, format);
        }

        /// <summary>
        /// Gets the GDI+ Bitmap.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public Bitmap Image
        {
            get
            {
                VerifyNotDisposed();
                return bitmap;
            }
        }

        /// <summary>
        /// Gets the bitmap width.
        /// </summary>
        /// <value>
        /// The bitmap width.
        /// </value>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public override int Width
        {
            get
            {
                VerifyNotDisposed();
                return bitmap.Width;
            }
        }

        /// <summary>
        /// Gets the bitmap height.
        /// </summary>
        /// <value>
        /// The bitmap height.
        /// </value>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public override int Height
        {
            get
            {
                VerifyNotDisposed();
                return bitmap.Height;
            }
        }

        /// <summary>
        /// Gets the G'MIC pixel format.
        /// </summary>
        /// <returns>The G'MIC pixel format.</returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public override GmicPixelFormat GetGmicPixelFormat()
        {
            VerifyNotDisposed();

            GmicPixelFormat gmicPixelFormat;

            AnalyzeImageResult analyzeImageResult = AnalyzeImage();

            if (analyzeImageResult.HasTransparency)
            {
                gmicPixelFormat = analyzeImageResult.IsGrayscale ? GmicPixelFormat.GrayAlpha16 : GmicPixelFormat.Rgba32;
            }
            else
            {
                gmicPixelFormat = analyzeImageResult.IsGrayscale ? GmicPixelFormat.Gray8 : GmicPixelFormat.Rgb24;
            }

            return gmicPixelFormat;
        }

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a gray-scale format into this instance.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyFromGmicImageGray(float* grayPlane, int planeStride)
        {
            // Gray-scale images are treated as Format24bppRgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    float* src = grayPlane + (y * planeStride);
                    byte* dst = scan0 + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        dst[0] = dst[1] = dst[2] = GmicFloatToByte(*src);

                        src++;
                        dst += 3;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a gray-scale with alpha format into this instance.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyFromGmicImageGrayAlpha(float* grayPlane, float* alphaPlane, int planeStride)
        {
            // Gray-scale images with alpha are treated as Format32bppArgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    float* srcGray = grayPlane + (y * planeStride);
                    float* srcAlpha = alphaPlane + (y * planeStride);
                    byte* dst = scan0 + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        dst[0] = dst[1] = dst[2] = GmicFloatToByte(*srcGray);
                        dst[3] = GmicFloatToByte(*srcAlpha);

                        srcGray++;
                        srcAlpha++;
                        dst += 4;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a RGB format into this instance.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyFromGmicImageRGB(float* redPlane, float* greenPlane, float* bluePlane, int planeStride)
        {
            // RGB images are treated as Format24bppRgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    float* srcR = redPlane + (y * planeStride);
                    float* srcG = greenPlane + (y * planeStride);
                    float* srcB = bluePlane + (y * planeStride);
                    byte* dst = scan0 + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        // Swap RGB to BGR.
                        dst[2] = GmicFloatToByte(*srcR);
                        dst[1] = GmicFloatToByte(*srcG);
                        dst[0] = GmicFloatToByte(*srcB);

                        srcR++;
                        srcG++;
                        srcB++;
                        dst += 3;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }


        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a RGBA format into this instance.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyFromGmicImageRGBA(float* redPlane, float* greenPlane, float* bluePlane, float* alphaPlane, int planeStride)
        {
            // RGBA images are treated as Format32bppArgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    float* srcR = redPlane + (y * planeStride);
                    float* srcG = greenPlane + (y * planeStride);
                    float* srcB = bluePlane + (y * planeStride);
                    float* srcA = alphaPlane + (y * planeStride);
                    byte* dst = scan0 + (y * stride);

                    for (int x = 0; x < width; x++)
                    {
                        // Swap RGB to BGR.
                        dst[2] = GmicFloatToByte(*srcR);
                        dst[1] = GmicFloatToByte(*srcG);
                        dst[0] = GmicFloatToByte(*srcB);
                        dst[3] = GmicFloatToByte(*srcA);

                        srcR++;
                        srcG++;
                        srcB++;
                        srcA++;
                        dst += 4;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a gray-scale format.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyToGmicImageGray(float* grayPlane, int planeStride)
        {
            // GDI+ does not have a gray-scale format, but a gray-scale Format24bppRgb image
            // will use this to save memory when running G'MIC.

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* src = scan0 + (y * stride);
                    float* dstGray = grayPlane + (y * planeStride);

                    for (int x = 0; x < width; x++)
                    {
                        *dstGray = ByteToGmicFloat(src[0]);

                        dstGray++;
                        src += 3;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a gray-scale with alpha format.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyToGmicImageGrayAlpha(float* grayPlane, float* alphaPlane, int planeStride)
        {
            // GDI+ does not have a gray-scale with alpha format, but a gray-scale Format32bppArgb image
            // will use this to save memory when running G'MIC.

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* src = scan0 + (y * stride);
                    float* dstGray = grayPlane + (y * planeStride);
                    float* dstAlpha = alphaPlane + (y * planeStride);

                    for (int x = 0; x < width; x++)
                    {
                        *dstGray = ByteToGmicFloat(src[0]);
                        *dstAlpha = ByteToGmicFloat(src[3]);

                        dstGray++;
                        dstAlpha++;
                        src += 4;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a RGB format.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyToGmicImageRGB(float* redPlane, float* greenPlane, float* bluePlane, int planeStride)
        {
            // RGB images are treated as Format24bppRgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* src = scan0 + (y * stride);
                    float* dstR = redPlane + (y * planeStride);
                    float* dstG = greenPlane + (y * planeStride);
                    float* dstB = bluePlane + (y * planeStride);

                    for (int x = 0; x < width; x++)
                    {
                        // Swap BGR to RGB.
                        *dstR = ByteToGmicFloat(src[2]);
                        *dstG = ByteToGmicFloat(src[1]);
                        *dstB = ByteToGmicFloat(src[0]);

                        dstR++;
                        dstG++;
                        dstB++;
                        src += 3;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a RGBA format.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected override unsafe void CopyToGmicImageRGBA(float* redPlane, float* greenPlane, float* bluePlane, float* alphaPlane, int planeStride)
        {
            // RGBA images are treated as Format32bppArgb.
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            try
            {
                int width = bitmapData.Width;
                int height = bitmapData.Height;
                int stride = bitmapData.Stride;
                byte* scan0 = (byte*)bitmapData.Scan0;

                for (int y = 0; y < height; y++)
                {
                    byte* src = scan0 + (y * stride);
                    float* dstR = redPlane + (y * planeStride);
                    float* dstG = greenPlane + (y * planeStride);
                    float* dstB = bluePlane + (y * planeStride);
                    float* dstA = alphaPlane + (y * planeStride);

                    for (int x = 0; x < width; x++)
                    {
                        // Swap BGR to RGB.
                        *dstR = ByteToGmicFloat(src[2]);
                        *dstG = ByteToGmicFloat(src[1]);
                        *dstB = ByteToGmicFloat(src[0]);
                        *dstA = ByteToGmicFloat(src[3]);

                        dstR++;
                        dstG++;
                        dstB++;
                        dstA++;
                        src += 4;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Clones the bitmap or converts it to a supported format.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns>The cloned or converted bitmap.</returns>
        private static Bitmap CloneOrConvertBitmap(Bitmap bitmap)
        {
            PixelFormat format = bitmap.PixelFormat;

            if (IsSupportedPixelFormat(format))
            {
                return (Bitmap)bitmap.Clone();
            }
            else
            {
                PixelFormat destinationFormat;
                if (System.Drawing.Image.IsAlphaPixelFormat(format))
                {
                    destinationFormat = PixelFormat.Format32bppArgb;
                }
                else
                {
                    destinationFormat = PixelFormat.Format24bppRgb;
                }

                return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), destinationFormat);
            }
        }

        /// <summary>
        /// Converts the image to a bitmap.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns>The converted bitmap.</returns>
        private static Bitmap ConvertImageToBitmap(Image image)
        {
            Bitmap asBitmap = image as Bitmap;
            if (asBitmap != null)
            {
                return CloneOrConvertBitmap(asBitmap);
            }
            else
            {
                PixelFormat destinationFormat;
                if (System.Drawing.Image.IsAlphaPixelFormat(image.PixelFormat))
                {
                    destinationFormat = PixelFormat.Format32bppArgb;
                }
                else
                {
                    destinationFormat = PixelFormat.Format24bppRgb;
                }

                Bitmap bitmap = new Bitmap(image.Width, image.Height, destinationFormat);

                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.DrawImage(image, 0, 0);
                }

                return bitmap;
            }
        }

        /// <summary>
        /// Checks that the GDI+ pixel format is supported.
        /// </summary>
        /// <param name="format">The GDI+ pixel format.</param>
        /// <returns><c>true</c> if the GDI+ pixel format is supported; otherwise, <c>false</c></returns>
        private static bool IsSupportedPixelFormat(PixelFormat format)
        {
            return format == PixelFormat.Format24bppRgb ||
                   format == PixelFormat.Format32bppArgb;
        }

        private unsafe AnalyzeImageResult AnalyzeImage()
        {
            bool hasTransparency = false;
            bool isGrayscale = true;

            if (bitmap.PixelFormat == PixelFormat.Format32bppArgb)
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                        ImageLockMode.ReadOnly,
                                                        PixelFormat.Format32bppArgb);
                try
                {
                    int width = bitmapData.Width;
                    int height = bitmapData.Height;
                    int stride = bitmapData.Stride;
                    byte* scan0 = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        byte* src = scan0 + (y * stride);

                        for (int x = 0; x < width; x++)
                        {
                            if (src[3] < 255)
                            {
                                hasTransparency = true;
                            }

                            if (!(src[0] == src[1] && src[1] == src[2]))
                            {
                                isGrayscale = false;
                            }

                            src += 4;
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
            else
            {
                BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                        ImageLockMode.ReadOnly,
                                                        PixelFormat.Format24bppRgb);
                try
                {
                    int width = bitmapData.Width;
                    int height = bitmapData.Height;
                    int stride = bitmapData.Stride;
                    byte* scan0 = (byte*)bitmapData.Scan0;

                    for (int y = 0; y < height; y++)
                    {
                        byte* src = scan0 + (y * stride);

                        for (int x = 0; x < width; x++)
                        {
                            if (!(src[0] == src[1] && src[1] == src[2]))
                            {
                                isGrayscale = false;
                            }

                            src += 3;
                        }
                    }
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }

            return new AnalyzeImageResult(hasTransparency, isGrayscale);
        }

        /// <summary>
        /// Verifies that the class has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        private void VerifyNotDisposed()
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GdiPlusGmicBitmap));
            }
        }

        private readonly struct AnalyzeImageResult
        {
            public AnalyzeImageResult(bool hasTransparency, bool isGrayscale)
            {
                HasTransparency = hasTransparency;
                IsGrayscale = isGrayscale;
            }

            public bool HasTransparency { get; }

            public bool IsGrayscale { get; }
        }
    }
}
