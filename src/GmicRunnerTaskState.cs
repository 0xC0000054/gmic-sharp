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
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharp
{
    internal sealed class GmicRunnerTaskState<TGmicBitmap> where TGmicBitmap : GmicBitmap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmicRunnerTaskState{TGmicBitmap}" /> class.
        /// </summary>
        /// <param name="completionSource">The completion source.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="completionSource" /> is null.</exception>
        public GmicRunnerTaskState(TaskCompletionSource<OutputImageCollection<TGmicBitmap>> completionSource,
                                   CancellationToken cancellationToken)
        {
            if (completionSource is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(completionSource));
            }

            CompletionSource = completionSource;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public TaskCompletionSource<OutputImageCollection<TGmicBitmap>> CompletionSource { get; }
    }
}
