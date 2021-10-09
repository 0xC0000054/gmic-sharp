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
using System.ComponentModel;

namespace GmicSharp
{
    /// <summary>
    /// The <see cref="EventArgs"/> for the <see cref="Gmic{TGmicBitmap}.RunGmicCompleted"/> event.
    /// </summary>
    /// <seealso cref="AsyncCompletedEventArgs" />
    public sealed class RunGmicCompletedEventArgs<TGmicBitmap> : AsyncCompletedEventArgs where TGmicBitmap : GmicBitmap
    {
        private readonly OutputImageCollection<TGmicBitmap> outputImages;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunGmicCompletedEventArgs{TGmicBitmap}"/> class.
        /// </summary>
        /// <param name="outputImages">The output images.</param>
        /// <param name="error">The error.</param>
        /// <param name="canceled"><c>true</c> if G'MIC was canceled; otherwise, <c>false</c>.</param>
        public RunGmicCompletedEventArgs(OutputImageCollection<TGmicBitmap> outputImages, Exception error, bool canceled)
            : base(error, canceled, null)
        {
            this.outputImages = outputImages;
        }

        /// <summary>
        /// Gets the output images.
        /// </summary>
        /// <value>
        /// The output images.
        /// </value>
        public OutputImageCollection<TGmicBitmap> OutputImages
        {
            get
            {
                RaiseExceptionIfNecessary();
                return outputImages;
            }
        }
    }
}
