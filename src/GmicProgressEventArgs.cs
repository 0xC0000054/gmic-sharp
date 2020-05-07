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
    /// The <see cref="EventArgs"/> used by the <see cref="Gmic.GmicProgress"/> event.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public sealed class GmicProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmicProgressEventArgs"/> class.
        /// </summary>
        /// <param name="progress">The progress.</param>
        public GmicProgressEventArgs(int progress)
        {
            Progress = progress;
        }

        /// <summary>
        /// Gets the progress.
        /// </summary>
        /// <value>
        /// The progress.
        /// </value>
        public int Progress { get; }
    }
}
