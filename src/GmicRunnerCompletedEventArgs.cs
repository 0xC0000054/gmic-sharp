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
    internal sealed class GmicRunnerCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmicRunnerCompletedEventArgs"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <param name="canceled"><c>true</c> if G'MIC was canceled; otherwise, <c>false</c>.</param>
        public GmicRunnerCompletedEventArgs(Exception error, bool canceled)
        {
            Error = error;
            Canceled = canceled;
        }

        /// <summary>
        /// Gets the error that occurred when running G'MIC.
        /// </summary>
        /// <value>
        /// The error that occurred when running G'MIC.
        /// </value>
        public Exception Error { get; }

        /// <summary>
        /// Gets a value indicating whether G'MIC was canceled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if G'MIC was canceled; otherwise, <c>false</c>.
        /// </value>
        public bool Canceled { get; }
    }
}
