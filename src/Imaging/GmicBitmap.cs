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

namespace GmicSharp
{
    /// <summary>
    /// Represents a bitmap image that G'MIC can process
    /// </summary>
    /// <seealso cref="IDisposable" />
    public abstract class GmicBitmap : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmicBitmap"/> class.
        /// </summary>
        protected GmicBitmap()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicBitmap"/> class.
        /// </summary>
        /// <param name="width">The bitmap width.</param>
        /// <param name="height">The bitmap height.</param>
        protected GmicBitmap(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
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
        /// Locks the bitmap in memory for unsafe access to the pixel data.
        /// </summary>
        /// <returns>A <see cref="GmicBitmapLock"/> instance.</returns>
        public abstract GmicBitmapLock Lock();

        /// <summary>
        /// Unlocks the bitmap.
        /// </summary>
        public abstract void Unlock();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
