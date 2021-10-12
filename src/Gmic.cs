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
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharp
{
    /// <summary>
    /// The class used to run G'MIC commands.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="IDisposable" />
    public sealed class Gmic<TGmicBitmap> where TGmicBitmap : GmicBitmap
    {
#pragma warning disable IDE0032 // Use auto property
        private string hostName;
#pragma warning restore IDE0032 // Use auto property

        private readonly IGmicOutputImageFactory<TGmicBitmap> outputImageFactory;

        /// <summary>
        /// The default host name that gmic-sharp-native uses.
        /// </summary>
        private const string DefaultHostName = "gmic-sharp";

        /// <summary>
        /// Initializes a new instance of the <see cref="Gmic{TGmicBitmap}" /> class.
        /// </summary>
        /// <param name="outputImageFactory">The factory that creates the output images.</param>
        /// <exception cref="ArgumentNullException"><paramref name="outputImageFactory"/> is null.</exception>
        /// <exception cref="GmicException">
        /// The native library could not be found or loaded.
        ///
        /// or
        ///
        /// The GmicSharp and libGmicSharpNative versions do not match.
        ///
        /// or
        ///
        /// Unable to create the G'MIC image list.
        /// </exception>
        public Gmic(IGmicOutputImageFactory<TGmicBitmap> outputImageFactory)
        {
            if (outputImageFactory is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(outputImageFactory));
            }

            this.outputImageFactory = outputImageFactory;
        }

        /// <summary>
        /// Gets or sets the G'MIC custom resource folder path.
        /// </summary>
        /// <value>
        /// The G'MIC custom resource folder path.
        /// </value>
        public string CustomResourcePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the host application.
        /// </summary>
        /// <value>
        /// The name of the host application.
        /// </value>
        /// <remarks>
        /// G'MIC scripts can use this value to customize their behavior based
        /// on the on the supported features of a specific host application.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The host name is null.</exception>
        /// <exception cref="ArgumentException">The host name is empty or contains only white space.</exception>
        public string HostName
        {
            get => string.IsNullOrWhiteSpace(hostName) ? DefaultHostName : hostName;
            set
            {
                if (value is null)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(value));
                }

                if (value.IsEmptyOrWhiteSpace())
                {
                    ExceptionUtil.ThrowArgumentException("The host name is empty or contains only white space.");
                }

                hostName = value;
            }
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputImages">The input images.</param>
        /// <returns>An <see cref="OutputImageCollection{TGmicBitmap}"/> containing the processed images.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when running G'MIC.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the G'MIC image list.</exception>
        public OutputImageCollection<TGmicBitmap> RunGmic(string command, IEnumerable<TGmicBitmap> inputImages)
        {
            Task<OutputImageCollection<TGmicBitmap>> task = RunGmicAsync(command, inputImages);

            // Using WaitAny allows any exception that occurred
            // during the task execution to be examined.
            Task.WaitAny(task);

            if (task.IsFaulted)
            {
                Exception exception = task.Exception.GetBaseException();

                if (exception is GmicException)
                {
                    System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
                    // The following return statement will never be executed,
                    // it is only present for compiler appeasement.
                    return null;
                }
                else
                {
                    throw new GmicException(exception.Message, exception);
                }
            }
            else
            {
                return task.Result;
            }
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputImages">The input images.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when creating the G'MIC image list.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the G'MIC image list.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicAsync(string command, IEnumerable<TGmicBitmap> inputImages)
        {
            return RunGmicAsync(command, inputImages, CancellationToken.None, null);
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputImages">The input images.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when creating the G'MIC image list.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the G'MIC image list.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicAsync(string command,
                                                                     IEnumerable<TGmicBitmap> inputImages,
                                                                     CancellationToken cancellationToken)
        {
            return RunGmicAsync(command, inputImages, cancellationToken, null);
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="inputImages">The input images.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress callback.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when creating the G'MIC image list.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the G'MIC image list.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicAsync(string command,
                                                                     IEnumerable<TGmicBitmap> inputImages,
                                                                     CancellationToken cancellationToken,
                                                                     IProgress<int> progress)
        {
            if (command is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(command));
            }

            if (command.IsEmptyOrWhiteSpace())
            {
                ExceptionUtil.ThrowArgumentException("The G'MIC command is empty or all white space characters.");
            }

            GmicRunner<TGmicBitmap> gmicRunner = new GmicRunner<TGmicBitmap>(command,
                                                                             CustomResourcePath,
                                                                             HostName,
                                                                             inputImages,
                                                                             outputImageFactory,
                                                                             cancellationToken,
                                                                             progress);

            gmicRunner.StartAsync();

            return gmicRunner.Task;
        }
    }
}
