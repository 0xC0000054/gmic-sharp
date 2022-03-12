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
using System.Collections;
using System.Collections.Generic;

namespace GmicSharp
{
    /// <summary>
    /// Represents a collection of output images.
    /// </summary>
    /// <typeparam name="TGmicBitmap">The type of the G'MIC bitmap.</typeparam>
    /// <seealso cref="IReadOnlyList{TGmicBitmap}" />
    /// <seealso cref="IDisposable" />
    public sealed class OutputImageCollection<TGmicBitmap> : IReadOnlyList<TGmicBitmap>, IDisposable where TGmicBitmap : GmicBitmap
    {
        private readonly IReadOnlyList<TGmicBitmap> images;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputImageCollection{TGmicBitmap}"/> class.
        /// </summary>
        /// <param name="images">The images.</param>
        /// <exception cref="ArgumentNullException"><paramref name="images"/> is null.</exception>
        public OutputImageCollection(IReadOnlyList<TGmicBitmap> images)
        {
            if (images is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(images));
            }

            this.images = images;
            disposed = false;
        }

        /// <summary>
        /// Gets the <typeparamref name="TGmicBitmap"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <typeparamref name="TGmicBitmap"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The <typeparamref name="TGmicBitmap"/> at the specified index.</returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public TGmicBitmap this[int index]
        {
            get
            {
                VerifyNotDisposed();
                return images[index];
            }
        }

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public int Count
        {
            get
            {
                VerifyNotDisposed();
                return images.Count;
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

                for (int i = 0; i < images.Count; i++)
                {
                    images[i]?.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public IEnumerator<TGmicBitmap> GetEnumerator()
        {
            VerifyNotDisposed();
            return images.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            VerifyNotDisposed();
            return images.GetEnumerator();
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(OutputImageCollection<TGmicBitmap>));
            }
        }
    }
}
