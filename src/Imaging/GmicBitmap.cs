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

using GmicSharp.Interop;
using System;
using System.ComponentModel;
using System.Threading;

namespace GmicSharp
{
    /// <summary>
    /// Represents a bitmap image that G'MIC can process
    /// </summary>
    /// <seealso cref="IDisposable" />
    public abstract class GmicBitmap : IDisposable
    {
        private int isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicBitmap"/> class.
        /// </summary>
        protected GmicBitmap()
        {
            isDisposed = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicBitmap"/> class.
        /// </summary>
        /// <param name="width">The bitmap width.</param>
        /// <param name="height">The bitmap height.</param>
        protected GmicBitmap(int width, int height)
        {
            isDisposed = 0;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="GmicBitmap"/> class.
        /// </summary>
        ~GmicBitmap()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) == 0)
            {
                Dispose(disposing: false);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) == 0)
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Gets the bitmap width.
        /// </summary>
        /// <value>
        /// The bitmap width.
        /// </value>
        public virtual int Width { get; }

        /// <summary>
        /// Gets the bitmap height.
        /// </summary>
        /// <value>
        /// The bitmap height.
        /// </value>
        public virtual int Height { get; }

        /// <summary>
        /// Gets the G'MIC pixel format.
        /// </summary>
        /// <returns>The G'MIC pixel format.</returns>
        public abstract GmicPixelFormat GetGmicPixelFormat();

        /// <summary>
        /// Copies the pixel data from a G/MIC image into this instance.
        /// </summary>
        /// <param name="outputImageFormat">The output image format.</param>
        /// <param name="pixelData">The pixel data.</param>
        /// <param name="planeStride">The plane stride.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="outputImageFormat"/> is not valid.</exception>
        internal unsafe void CopyFromGmicImage(NativeImageFormat outputImageFormat, GmicImageListPixelData pixelData, int planeStride)
        {
            switch (outputImageFormat)
            {
                case NativeImageFormat.Gray8:
                    CopyFromGmicImageGray(pixelData.redGrayUnion.gray, planeStride);
                    break;
                case NativeImageFormat.GrayAlpha88:
                    CopyFromGmicImageGrayAlpha(pixelData.redGrayUnion.gray, pixelData.alpha, planeStride);
                    break;
                case NativeImageFormat.Rgb888:
                    CopyFromGmicImageRGB(pixelData.redGrayUnion.red, pixelData.green, pixelData.blue, planeStride);
                    break;
                case NativeImageFormat.Rgba8888:
                    CopyFromGmicImageRGBA(pixelData.redGrayUnion.red, pixelData.green, pixelData.blue, pixelData.alpha, planeStride);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(outputImageFormat), (int)outputImageFormat, typeof(NativeImageFormat));
            }
        }

        /// <summary>
        /// Copies the pixel data from this instance into a G/MIC image.
        /// </summary>
        /// <param name="gmicImageFormat">The G'MIC image format.</param>
        /// <param name="pixelData">The pixel data.</param>
        /// <param name="planeStride">The plane stride.</param>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="gmicImageFormat"/> is not valid.</exception>
        internal unsafe void CopyToGmicImage(NativeImageFormat gmicImageFormat, GmicImageListPixelData pixelData, int planeStride)
        {
            switch (gmicImageFormat)
            {
                case NativeImageFormat.Gray8:
                    CopyToGmicImageGray(pixelData.redGrayUnion.gray, planeStride);
                    break;
                case NativeImageFormat.GrayAlpha88:
                    CopyToGmicImageGrayAlpha(pixelData.redGrayUnion.gray, pixelData.alpha, planeStride);
                    break;
                case NativeImageFormat.Rgb888:
                    CopyToGmicImageRGB(pixelData.redGrayUnion.red, pixelData.green, pixelData.blue, planeStride);
                    break;
                case NativeImageFormat.Rgba8888:
                    CopyToGmicImageRGBA(pixelData.redGrayUnion.red, pixelData.green, pixelData.blue, pixelData.alpha, planeStride);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(gmicImageFormat), (int)gmicImageFormat, typeof(NativeImageFormat));
            }
        }

        /// <summary>
        /// Converts a byte to a G'MIC float.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The converted value.</returns>
        protected static float ByteToGmicFloat(byte value)
        {
            // The G'MIC float uses the range of [0, 255] for 8-bit-per-channel images.
            return value;
        }

        /// <summary>
        /// Converts a G'MIC float to a byte.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The converted value.</returns>
        protected static byte GmicFloatToByte(float value)
        {
            // The G'MIC float uses the range of [0, 255] for 8-bit-per-channel images.
            return (byte)(value > 255f ? 255 : value < 0f ? 0 : value);
        }

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a gray-scale format into this instance.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyFromGmicImageGray(float* grayPlane, int planeStride);

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a gray-scale with alpha format into this instance.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyFromGmicImageGrayAlpha(float* grayPlane, float* alphaPlane, int planeStride);

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a RGB format into this instance.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyFromGmicImageRGB(float* redPlane,
                                                            float* greenPlane,
                                                            float* bluePlane,
                                                            int planeStride);

        /// <summary>
        /// Copies the pixel data from a G'MIC image that uses a RGBA format into this instance.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyFromGmicImageRGBA(float* redPlane,
                                                             float* greenPlane,
                                                             float* bluePlane,
                                                             float* alphaPlane,
                                                             int planeStride);

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a gray-scale format.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyToGmicImageGray(float* grayPlane, int planeStride);

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a gray-scale with alpha format.
        /// </summary>
        /// <param name="grayPlane">The gray plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyToGmicImageGrayAlpha(float* grayPlane,
                                                                float* alphaPlane,
                                                                int planeStride);

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a RGB format.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyToGmicImageRGB(float* redPlane,
                                                          float* greenPlane,
                                                          float* bluePlane,
                                                          int planeStride);

        /// <summary>
        /// Copies the pixel data from this instance into a G'MIC image that uses a RGBA format.
        /// </summary>
        /// <param name="redPlane">The red plane.</param>
        /// <param name="greenPlane">The green plane.</param>
        /// <param name="bluePlane">The blue plane.</param>
        /// <param name="alphaPlane">The alpha plane.</param>
        /// <param name="planeStride">The plane stride.</param>
        protected abstract unsafe void CopyToGmicImageRGBA(float* redPlane,
                                                           float* greenPlane,
                                                           float* bluePlane,
                                                           float* alphaPlane,
                                                           int planeStride);

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
