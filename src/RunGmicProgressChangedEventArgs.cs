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
using System.ComponentModel;

namespace GmicSharp
{
    /// <summary>
    /// The <see cref="EventArgs"/> used by the <see cref="Gmic{TGmicBitmap}.RunGmicProgressChanged"/> event.
    /// </summary>
    /// <seealso cref="ProgressChangedEventArgs" />
    public sealed class RunGmicProgressChangedEventArgs : ProgressChangedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunGmicProgressChangedEventArgs"/> class.
        /// </summary>
        /// <param name="progress">The progress.</param>
        public RunGmicProgressChangedEventArgs(int progress) : base(progress, null)
        {
        }
    }
}
