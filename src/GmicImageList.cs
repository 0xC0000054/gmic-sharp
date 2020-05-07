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
    internal sealed class GmicImageList : IDisposable
    {
#pragma warning disable IDE0032 // Use auto property
        private readonly SafeGmicImageList nativeImageList;
#pragma warning restore IDE0032 // Use auto property
        private bool disposed;

        public GmicImageList()
        {
            nativeImageList = GmicNative.CreateGmicImageList();

            if (nativeImageList == null || nativeImageList.IsInvalid)
            {
                throw new GmicException("Failed to create the native G'MIC image list.");
            }
            disposed = false;
        }

        public uint Count
        {
            get
            {
                EnsureNativeImageListIsValid();
                return GmicNative.GmicImageListGetCount(nativeImageList);
            }
        }

        public SafeGmicImageList SafeImageListHandle => nativeImageList;

        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                nativeImageList?.Dispose();
            }
        }

        public void Add(GmicBitmap bitmap, string name)
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            EnsureNativeImageListIsValid();

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

        public void Clear()
        {
            EnsureNativeImageListIsValid();

            GmicNative.GmicImageListClear(nativeImageList);
        }

        public void GetImageInfo(uint index, out GmicImageListItemInfo info)
        {
            EnsureNativeImageListIsValid();

            GmicNative.GmicImageListGetImageInfo(nativeImageList, index, out info);
        }

        public void CopyToOutput(uint index, GmicBitmap bitmap)
        {
            if (bitmap is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            EnsureNativeImageListIsValid();

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

        private void EnsureNativeImageListIsValid()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GmicImageList));
            }

            if (nativeImageList.IsClosed || nativeImageList.IsInvalid)
            {
                ExceptionUtil.ThrowInvalidOperationException($"The { nameof(SafeGmicImageList) } handle is closed or invalid.");
            }
        }
    }
}
