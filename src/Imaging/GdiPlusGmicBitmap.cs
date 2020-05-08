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
#pragma warning disable IDE0032 // Use auto property
        private Bitmap bitmap;
#pragma warning restore IDE0032 // Use auto property
        private BitmapData bitmapData;

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPlusGmicBitmap"/> class.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="ArgumentException">The GDI+ pixel format is not supported.</exception>
        public GdiPlusGmicBitmap(Bitmap bitmap) : base()
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            CheckPixelFormat(bitmap.PixelFormat);

            this.bitmap = (Bitmap)bitmap.Clone();
            bitmapData = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPlusGmicBitmap"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The GDI+ pixel format.</param>
        /// <exception cref="ArgumentException">The GDI+ pixel format is not supported.</exception>
        public GdiPlusGmicBitmap(int width, int height, PixelFormat format) : base()
        {
            CheckPixelFormat(format);

            bitmap = new Bitmap(width, height, format);
            bitmapData = null;
        }

        /// <summary>
        /// Gets the GSI+ Bitmap.
        /// </summary>
        /// <value>
        /// The image.
        /// </value>
        public Bitmap Image => bitmap;

        /// <summary>
        /// Gets the bitmap width.
        /// </summary>
        /// <value>
        /// The bitmap width.
        /// </value>
        public override int Width => bitmap.Width;

        /// <summary>
        /// Gets the bitmap height.
        /// </summary>
        /// <value>
        /// The bitmap height.
        /// </value>
        public override int Height => bitmap.Height;

        /// <summary>
        /// Gets the G'MIC pixel format.
        /// </summary>
        /// <returns>The G'MIC pixel format.</returns>
        public override GmicPixelFormat GetGmicPixelFormat()
        {
            GmicPixelFormat gmicPixelFormat;

            switch (bitmap.PixelFormat)
            {
                case PixelFormat.Format24bppRgb:
                    gmicPixelFormat = GmicPixelFormat.Bgr24;
                    break;
                case PixelFormat.Format32bppRgb:
                    gmicPixelFormat = GmicPixelFormat.Bgr32;
                    break;
                case PixelFormat.Format32bppArgb:
                    gmicPixelFormat = GmicPixelFormat.Bgra32;
                    break;
                default:
                    throw new InvalidOperationException("The GDI+ PixelFormat must be Format24bppRgb, Format32bppRgb or Format32bppArgb.");
            }

            return gmicPixelFormat;
        }

        /// <summary>
        /// Locks the bitmap in memory for unsafe access to the pixel data.
        /// </summary>
        /// <returns>A <see cref="GmicBitmapLock"/> instance.</returns>
        /// <exception cref="InvalidOperationException">The bitmap is already locked.</exception>
        public override GmicBitmapLock Lock()
        {
            if (bitmapData != null)
            {
                ExceptionUtil.ThrowInvalidOperationException("The bitmap is already locked.");
            }

            bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                         ImageLockMode.ReadWrite,
                                         bitmap.PixelFormat);

            return new GmicBitmapLock(bitmapData.Scan0, bitmapData.Stride);
        }

        /// <summary>
        /// Unlocks the bitmap.
        /// </summary>
        public override void Unlock()
        {
            if (bitmapData != null)
            {
                bitmap.UnlockBits(bitmapData);
                bitmapData = null;
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
        /// Checks that the GDI+ pixel format is supported.
        /// </summary>
        /// <param name="format">The GDI+ pixel format.</param>
        /// <exception cref="ArgumentException">The GDI+ pixel format is not supported.</exception>
        private static void CheckPixelFormat(PixelFormat format)
        {
            if (format != PixelFormat.Format24bppRgb &&
                format != PixelFormat.Format32bppRgb &&
                format != PixelFormat.Format32bppArgb)
            {
                ExceptionUtil.ThrowArgumentException("The GDI+ PixelFormat must be Format24bppRgb, Format32bppRgb or Format32bppArgb.");
            }
        }
    }
}
