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

using GmicSharp.Interop;
using System;

namespace GmicSharp
{
    /// <summary>
    /// The native G'MIC image list.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    internal sealed class GmicImageList : IDisposable
    {
#pragma warning disable IDE0032 // Use auto property
        private readonly SafeGmicImageList nativeImageList;
#pragma warning restore IDE0032 // Use auto property
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicImageList"/> class.
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
        /// <param name="name">The name.</param>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to add the image.</exception>
        public void Add(GmicBitmap bitmap, string name)
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            VerifyNotDisposed();

            uint width = (uint)bitmap.Width;
            uint height = (uint)bitmap.Height;
            GmicPixelFormat format = bitmap.GetGmicPixelFormat();

            GmicBitmapLock bitmapLock = bitmap.Lock();

            try
            {
                IntPtr scan0 = bitmapLock.Scan0;
                uint stride = (uint)bitmapLock.Stride;

                GmicNative.GmicImageListAdd(nativeImageList, width, height, stride, scan0, format, name);
            }
            finally
            {
                bitmap.Unlock();
            }

        }

        /// <summary>
        /// Clears the image list.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void Clear()
        {
            VerifyNotDisposed();

            GmicNative.GmicImageListClear(nativeImageList);
        }

        /// <summary>
        /// Gets the image information.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="info">The information.</param>
        /// <exception cref="GmicException">The image list index is invalid.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void GetImageInfo(uint index, out GmicImageListItemInfo info)
        {
            VerifyNotDisposed();

            GmicNative.GmicImageListGetImageInfo(nativeImageList, index, out info);
        }

        /// <summary>
        /// Copies the image at the specified index to the output bitmap.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="bitmap">The bitmap.</param>
        /// <exception cref="GmicException">The image list index is invalid.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void CopyToOutput(uint index, GmicBitmap bitmap)
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            VerifyNotDisposed();

            uint width = (uint)bitmap.Width;
            uint height = (uint)bitmap.Height;
            GmicPixelFormat format = bitmap.GetGmicPixelFormat();

            GmicBitmapLock bitmapLock = bitmap.Lock();

            try
            {
                IntPtr scan0 = bitmapLock.Scan0;
                uint stride = (uint)bitmapLock.Stride;

                GmicNative.GmicImageListCopyToOutput(nativeImageList, index, width, height, stride, scan0, format);
            }
            finally
            {
                bitmap.Unlock();
            }

        }

        /// <summary>
        /// Verifies that the class has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GmicImageList));
            }
        }
    }
}
