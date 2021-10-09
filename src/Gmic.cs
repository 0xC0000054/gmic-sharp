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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharp
{
    /// <summary>
    /// The class used to run G'MIC commands.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <seealso cref="IDisposable" />
    public sealed class Gmic<TGmicBitmap> : IDisposable where TGmicBitmap : GmicBitmap
    {
        private GmicImageList gmicImages;
        private Timer updateProgressTimer;
        private bool disposed;
        private int progressUpdating;
        private object updateProgressTimerCookie;
        private int lastProgressValue;
#pragma warning disable IDE0032 // Use auto property
        private string hostName;
#pragma warning restore IDE0032 // Use auto property
        private AsyncOperation asyncOp;

        private readonly IGmicOutputImageFactory<TGmicBitmap> outputImageFactory;
        private readonly GmicRunner<TGmicBitmap> gmicRunner;
        private readonly SendOrPostCallback completedCallback;
        private readonly SendOrPostCallback progressCallback;

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
            gmicImages = new GmicImageList();
            gmicRunner = new GmicRunner<TGmicBitmap>(GmicRunnerCompleted);
            completedCallback = new SendOrPostCallback(RunGmicCompletedCallback);
            progressCallback = new SendOrPostCallback(RunGmicProgressCallback);
        }

        /// <summary>
        /// Occurs when G'MIC has finished processing.
        /// </summary>
        public event EventHandler<RunGmicCompletedEventArgs<TGmicBitmap>> RunGmicCompleted;

        /// <summary>
        /// Occurs when G'MIC reports rendering progress.
        /// </summary>
        public event EventHandler<RunGmicProgressChangedEventArgs> RunGmicProgressChanged;

        /// <summary>
        /// Gets or sets the G'MIC custom resource folder path.
        /// </summary>
        /// <value>
        /// The G'MIC custom resource folder path.
        /// </value>
        public string CustomResourcePath { get; set; }

        /// <summary>
        /// Gets a value indicating whether G'MIC is running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if G'MIC is running; otherwise, <c>false</c>.
        /// </value>
        public bool IsBusy => gmicRunner.IsBusy;

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
        /// Adds the input image to this 'G'MIC instance.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Cannot add an input image when G'MIC is running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to add the image.</exception>
        public void AddInputImage(TGmicBitmap bitmap)
        {
            AddInputImage(bitmap, null);
        }

        /// <summary>
        /// Adds the input image to this 'G'MIC instance.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="name">The image name.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bitmap"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Cannot add an input image when G'MIC is running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to add the image.</exception>
        public void AddInputImage(TGmicBitmap bitmap, string name)
        {
            if (bitmap == null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(bitmap));
            }

            VerifyNotDisposed();

            if (gmicRunner.IsBusy)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot add an input image when G'MIC is running.");
            }

            gmicImages.Add(bitmap, name);
        }

        /// <summary>
        /// Clears the input images that have been added this G'MIC instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">Cannot clear the input images when G'MIC is running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void ClearInputImages()
        {
            VerifyNotDisposed();

            if (gmicRunner.IsBusy)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot clear the input images when G'MIC is running.");
            }

            gmicImages.Clear();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                if (gmicImages != null)
                {
                    gmicImages.Dispose();
                    gmicImages = null;
                }

                if (updateProgressTimer != null)
                {
                    updateProgressTimerCookie = null;
                    updateProgressTimer.Dispose();
                    updateProgressTimer = null;
                }
            }
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>An <see cref="OutputImageCollection{TGmicBitmap}"/> containing the processed images.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when running G'MIC.</exception>
        /// <exception cref="InvalidOperationException">
        /// This G'MIC instance is already running.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public OutputImageCollection<TGmicBitmap> RunGmic(string command)
        {
            Task<OutputImageCollection<TGmicBitmap>> task = RunGmicTaskAsync(command);

            // Using WaitAny allows any exception that occurred
            // during the task execution to be examined.
            Task.WaitAny(task);

            if (task.IsFaulted)
            {
                Exception exception = task.Exception.GetBaseException();

                if (exception is GmicException)
                {
                    throw exception;
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
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when running G'MIC.</exception>
        /// <exception cref="InvalidOperationException">
        /// This G'MIC instance is already running.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void RunGmicAsync(string command)
        {
            if (command is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(command));
            }

            if (command.IsEmptyOrWhiteSpace())
            {
                ExceptionUtil.ThrowArgumentException("The G'MIC command is empty or all white space characters.");
            }

            VerifyNotDisposed();

            if (gmicRunner.IsBusy)
            {
                ExceptionUtil.ThrowInvalidOperationException("This G'MIC instance is already running.");
            }

            bool hasProgressEvent = RunGmicProgressChanged != null;

            if (hasProgressEvent)
            {
                StartUpdateProgressTimer(new UpdateProgressState());
            }

            asyncOp = AsyncOperationManager.CreateOperation(null);

            gmicRunner.StartAsync(command,
                                  CustomResourcePath,
                                  hostName,
                                  gmicImages,
                                  hasProgressEvent);
        }

        /// <summary>
        /// Cancels the asynchronous G'MIC processing.
        /// </summary>
        public void RunGmicAsyncCancel()
        {
            gmicRunner.SignalCancelRequest();
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="InvalidOperationException">This G'MIC instance is already running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicTaskAsync(string command)
        {
            return RunGmicTaskAsync(command, CancellationToken.None, null);
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="InvalidOperationException">This G'MIC instance is already running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicTaskAsync(string command, CancellationToken cancellationToken)
        {
            return RunGmicTaskAsync(command, cancellationToken, null);
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="progress">The progress callback.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the work queued to execute in the thread pool.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="command" /> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command" /> is a empty or contains only white space.</exception>
        /// <exception cref="InvalidOperationException">This G'MIC instance is already running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public Task<OutputImageCollection<TGmicBitmap>> RunGmicTaskAsync(string command, CancellationToken cancellationToken, IProgress<int> progress)
        {
            if (command is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(command));
            }

            if (command.IsEmptyOrWhiteSpace())
            {
                ExceptionUtil.ThrowArgumentException("The G'MIC command is empty or all white space characters.");
            }

            VerifyNotDisposed();

            if (gmicRunner.IsBusy)
            {
                ExceptionUtil.ThrowInvalidOperationException("This G'MIC instance is already running.");
            }

            bool hasProgressEvent = progress != null;

            if (hasProgressEvent)
            {
                StartUpdateProgressTimer(new UpdateProgressState(progress));
            }

            TaskCompletionSource<OutputImageCollection<TGmicBitmap>> completionSource = new TaskCompletionSource<OutputImageCollection<TGmicBitmap>>();

            try
            {
                gmicRunner.StartAsync(command,
                                      CustomResourcePath,
                                      hostName,
                                      gmicImages,
                                      hasProgressEvent,
                                      new GmicRunnerTaskState<TGmicBitmap>(completionSource, cancellationToken));
            }
            catch (OperationCanceledException)
            {
                completionSource.TrySetCanceled();
            }
            catch (Exception ex)
            {
                completionSource.TrySetException(ex);
            }

            return completionSource.Task;
        }

        private void GmicRunnerCompleted(Exception error, bool canceled, TaskCompletionSource<OutputImageCollection<TGmicBitmap>> task)
        {
            StopUpdateProgressTimer();

            OutputImageCollection<TGmicBitmap> outputGmicBitmaps = null;
            try
            {
                if (error == null && !canceled)
                {
                    outputGmicBitmaps = CreateOutputBitmaps();
                }
                gmicImages.Clear();
            }
            catch (Exception ex)
            {
                if (task != null)
                {
                    task.TrySetException(ex);
                }
                else
                {
                    RaiseRunGmicCompleted(null, ex, false);
                }
                return;
            }

            if (task != null)
            {
                if (error != null)
                {
                    task.TrySetException(error);
                }
                else if (canceled)
                {
                    task.TrySetCanceled();
                }
                else
                {
                    task.TrySetResult(outputGmicBitmaps);
                }
            }
            else
            {
                RaiseRunGmicCompleted(outputGmicBitmaps, error, canceled);
            }
        }

        private void RaiseRunGmicCompleted(OutputImageCollection<TGmicBitmap> images, Exception error, bool canceled)
        {
            asyncOp.PostOperationCompleted(completedCallback, new RunGmicCompletedEventArgs<TGmicBitmap>(images, error, canceled));
            asyncOp = null;
        }

        private void RunGmicCompletedCallback(object args)
        {
            OnRunGmicCompleted((RunGmicCompletedEventArgs<TGmicBitmap>)args);
        }

        private void OnRunGmicCompleted(RunGmicCompletedEventArgs<TGmicBitmap> e)
        {
            RunGmicCompleted?.Invoke(this, e);
        }

        private OutputImageCollection<TGmicBitmap> CreateOutputBitmaps()
        {
            if (gmicImages.Count > int.MaxValue)
            {
                throw new NotSupportedException($"The number of output G'MIC images exceeds { int.MaxValue }.");
            }

            List<TGmicBitmap> gmicBitmaps = new List<TGmicBitmap>((int)gmicImages.Count);

            for (int i = 0; i < gmicBitmaps.Capacity; i++)
            {
                uint index = (uint)i;

                Interop.GmicImageListImageData imageData;

                gmicImages.GetImageData(index, out imageData);

                if (imageData.width > int.MaxValue || imageData.height > int.MaxValue)
                {
                    throw new NotSupportedException($"The output G'MIC image dimensions exceed { int.MaxValue }.");
                }

                int width = (int)imageData.width;
                int height = (int)imageData.height;

                GmicPixelFormat gmicPixelFormat;
                switch (imageData.format)
                {
                    case Interop.NativeImageFormat.Gray8:
                        gmicPixelFormat = GmicPixelFormat.Gray8;
                        break;
                    case Interop.NativeImageFormat.GrayAlpha88:
                        gmicPixelFormat = GmicPixelFormat.GrayAlpha16;
                        break;
                    case Interop.NativeImageFormat.Rgb888:
                        gmicPixelFormat = GmicPixelFormat.Rgb24;
                        break;
                    case Interop.NativeImageFormat.Rgba8888:
                        gmicPixelFormat = GmicPixelFormat.Rgba32;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported { nameof(Interop.NativeImageFormat) } value: { imageData.format }.");
                }

                // G'MIC uses a planar format, so the stride between rows is the image width.
                int planeStride = width;

                TGmicBitmap bitmap = outputImageFactory.Create(width, height, gmicPixelFormat);

                bitmap.CopyFromGmicImage(imageData.format, imageData.pixels, planeStride);

                gmicBitmaps.Add(bitmap);
            }

            return new OutputImageCollection<TGmicBitmap>(gmicBitmaps);
        }

        private void RaiseRunGmicProgressChanged(int progress)
        {
            asyncOp.Post(progressCallback, new RunGmicProgressChangedEventArgs(progress));
        }

        private void RunGmicProgressCallback(object args)
        {
            OnRunGmicProgressChanged((RunGmicProgressChangedEventArgs)args);
        }

        private void OnRunGmicProgressChanged(RunGmicProgressChangedEventArgs e)
        {
            RunGmicProgressChanged?.Invoke(this, e);
        }

        private void OnUpdateProgress(object state)
        {
            // The timer has been stopped.
            if (state != updateProgressTimerCookie)
            {
                return;
            }

            // Detect reentrant calls to this method.
            if (Interlocked.CompareExchange(ref progressUpdating, 1, 0) != 0)
            {
                return;
            }

            UpdateProgressState updateProgressState = (UpdateProgressState)state;

            float progress = gmicRunner.GetProgress();

            if (progress > -1f)
            {
                if (progress < 0f)
                {
                    progress = 0f;
                }
                else if (progress > 100f)
                {
                    progress = 100f;
                }

                int progressPercentage = (int)progress;

                if (progressPercentage != lastProgressValue)
                {
                    lastProgressValue = progressPercentage;

                    if (updateProgressState.TaskProgress != null)
                    {
                        updateProgressState.TaskProgress.Report(progressPercentage);
                    }
                    else
                    {
                        RaiseRunGmicProgressChanged(progressPercentage);
                    }
                }
            }

            progressUpdating = 0;
        }

        private void StartUpdateProgressTimer(UpdateProgressState updateProgressState)
        {
            lastProgressValue = -1;
            updateProgressTimerCookie = updateProgressState;
            updateProgressTimer?.Dispose();
            updateProgressTimer = new Timer(OnUpdateProgress, updateProgressTimerCookie, 1000, 250);
        }

        private void StopUpdateProgressTimer()
        {
            if (updateProgressTimer != null)
            {
                updateProgressTimerCookie = null;
                updateProgressTimer.Dispose();
                updateProgressTimer = null;
            }
        }

        private void VerifyNotDisposed()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(Gmic<TGmicBitmap>));
            }
        }

        private sealed class UpdateProgressState
        {
            public UpdateProgressState()
            {
                TaskProgress = null;
            }

            public UpdateProgressState(IProgress<int> taskProgress)
            {
                if (taskProgress is null)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(taskProgress));
                }

                TaskProgress = taskProgress;
            }

            public IProgress<int> TaskProgress { get; }
        }
    }
}
