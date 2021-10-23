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

namespace GmicSharp
{
    /// <summary>
    /// The native G'MIC image list.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal sealed class GmicImageList<TGmicBitmap> : IDisposable where TGmicBitmap : GmicBitmap
    {
        private readonly SafeGmicImageList nativeImageList;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicImageList{TGmicBitmap}"/> class.
        /// </summary>
        /// <exception cref="GmicException">
        /// The native library could not be found or loaded.
        ///
        /// or
        ///
        /// The GmicSharp and libGmicSharpNative versions do not match.
        ///
        /// or
        ///
        /// Failed to create the native G'MIC image list.
        /// </exception>
        public GmicImageList()
        {
            GmicSharpNative.Initialize();
            nativeImageList = GmicNative.CreateGmicImageList();

            if (nativeImageList == null || nativeImageList.IsInvalid)
            {
                throw new GmicException("Failed to create the native G'MIC image list.");
            }
            disposed = false;
        }

        /// <summary>
        /// Gets the number of images in the list.
        /// </summary>
        /// <value>
        /// The number of images in the list.
        /// </value>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public uint Count
        {
            get
            {
                VerifyNotDisposed();
                return GmicNative.GmicImageListGetCount(nativeImageList);
            }
        }

        /// <summary>
        /// Gets the safe image list handle.
        /// </summary>
        /// <value>
        /// The safe image list handle.
        /// </value>
        public SafeGmicImageList SafeImageListHandle
        {
            get
            {
                VerifyNotDisposed();
                return nativeImageList;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                nativeImageList?.Dispose();
            }
        }

        /// <summary>
        /// Adds the specified bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="GmicException">An error occurred when adding the image.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to add the image.</exception>
        public void Add(TGmicBitmap bitmap)
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            VerifyNotDisposed();

            uint width = (uint)bitmap.Width;
            uint height = (uint)bitmap.Height;
            GmicPixelFormat format = bitmap.GetGmicPixelFormat();

            // G'MIC uses a planar format, so the stride between rows is the image width.
            int planeStride = (int)width;

            // Add a new image to the native G'MIC image list.
            GmicNative.GmicImageListAdd(nativeImageList,
                                        width,
                                        height,
                                        format,
                                        bitmap.Name,
                                        out GmicImageListPixelData pixelData,
                                        out NativeImageFormat nativeImageFormat);

            // Copy the pixel data to the native image.
            bitmap.CopyToGmicImage(nativeImageFormat, pixelData, planeStride);
        }

        /// <summary>
        /// Gets the image data.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The image data.</param>
        /// <exception cref="GmicException">The image list index is invalid.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void GetImageData(uint index, out GmicImageListImageData data)
        {
            VerifyNotDisposed();

            GmicNative.GmicImageListGetImageData(nativeImageList, index, out data);
        }

        /// <summary>
        /// Verifies that the class has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GmicImageList<TGmicBitmap>));
            }
        }
    }
}
