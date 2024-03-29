﻿////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021, 2022 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using GmicSharp.Interop;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharp
{
    internal sealed class GmicRunner<TGmicBitmap> where TGmicBitmap : GmicBitmap
    {
        private float progress;
        private byte shouldAbort;

        private readonly string command;
        private readonly string customResourcePath;
        private readonly string hostName;
        private readonly IGmicOutputImageFactory<TGmicBitmap> outputImageFactory;
        private readonly TaskCompletionSource<OutputImageCollection<TGmicBitmap>> taskCompletionSource;
        private readonly CancellationToken cancellationToken;
        private readonly IProgress<int> taskProgress;
        private readonly SynchronizationContext synchronizationContext;

        private GmicImageList<TGmicBitmap> gmicImages;
        private Timer updateProgressTimer;
        private bool disposed;
        private int progressUpdating;
        private object updateProgressTimerCookie;
        private int lastProgressValue;
        private Thread gmicWorkerThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicRunner{TGmicBitmap}" /> class.
        /// </summary>
        /// <param name="command">The G'MIC command.</param>
        /// <param name="customResourcePath">The custom resource path.</param>
        /// <param name="hostName">The host application name.</param>
        /// <param name="inputImages">The input images.</param>
        /// <param name="outputImageFactory">The output image factory.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="taskProgress">The task progress reporting interface.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="command"/> is <see langword="null"/>.
        /// or
        /// <paramref name="outputImageFactory"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="GmicException">An error occurred when creating the G'MIC image list.</exception>
        /// <exception cref="OutOfMemoryException">Insufficient memory to create the G'MIC image list.</exception>
        public GmicRunner(string command,
                          string customResourcePath,
                          string hostName,
                          IEnumerable<TGmicBitmap> inputImages,
                          IGmicOutputImageFactory<TGmicBitmap> outputImageFactory,
                          CancellationToken cancellationToken,
                          IProgress<int> taskProgress)
        {
            this.command = command ?? throw new ArgumentNullException(nameof(command));
            this.customResourcePath = customResourcePath;
            this.hostName = hostName;
            this.outputImageFactory = outputImageFactory ?? throw new ArgumentNullException(nameof(outputImageFactory));
            taskCompletionSource = new TaskCompletionSource<OutputImageCollection<TGmicBitmap>>();
            this.cancellationToken = cancellationToken;
            this.taskProgress = taskProgress;

            if (SynchronizationContext.Current is null)
            {
                SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            }

            synchronizationContext = SynchronizationContext.Current;

            gmicImages = new GmicImageList<TGmicBitmap>();

            if (inputImages != null)
            {
                try
                {
                    foreach (TGmicBitmap item in inputImages)
                    {
                        if (item.IsDisposed)
                        {
                            throw new GmicException("An input image has been disposed.");
                        }

                        gmicImages.Add(item);
                    }
                }
                catch (Exception)
                {
                    gmicImages.Dispose();
                    throw;
                }
            }
        }

        public Task<OutputImageCollection<TGmicBitmap>> Task => taskCompletionSource.Task;

        /// <summary>
        /// Runs the specified G'MIC command.
        /// </summary>
        /// <exception cref="InvalidOperationException">The G'MIC worker thread is already running.</exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void StartAsync()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GmicRunner<TGmicBitmap>));
            }

            if (gmicWorkerThread != null)
            {
                ExceptionUtil.ThrowInvalidOperationException("The G'MIC worker thread is already running.");
            }

            try
            {
                // G'MIC can use a large amount of stack space, so we will attempt to set the
                // maximum stack size of worker thread to 8 megabytes.
                // On Windows, the default thread stack size is set in the executable header
                // and is typically 1 megabyte.
                const int GmicThreadStackSize = 8 * 1024 * 1024;

                gmicWorkerThread = new Thread(GmicWorker, GmicThreadStackSize);
                gmicWorkerThread.Start();
            }
            catch (Exception ex)
            {
                Dispose();
                taskCompletionSource.TrySetException(ex);
            }
        }

        private OutputImageCollection<TGmicBitmap> CreateOutputBitmaps()
        {
            uint gmicImageListCount = gmicImages.Count;
            if (gmicImageListCount > int.MaxValue)
            {
                throw new NotSupportedException($"The number of output G'MIC images exceeds { int.MaxValue }.");
            }

            List<TGmicBitmap> gmicBitmaps = new List<TGmicBitmap>((int)gmicImageListCount);

            for (uint index = 0; index < gmicImageListCount; index++)
            {
                gmicImages.GetImageData(index, out GmicImageListImageData imageData);

                if (imageData.width > int.MaxValue || imageData.height > int.MaxValue)
                {
                    throw new NotSupportedException($"The output G'MIC image dimensions exceed { int.MaxValue }.");
                }

                int width = (int)imageData.width;
                int height = (int)imageData.height;

                GmicPixelFormat gmicPixelFormat;
                switch (imageData.format)
                {
                    case NativeImageFormat.Gray:
                        gmicPixelFormat = GmicPixelFormat.Gray;
                        break;
                    case NativeImageFormat.GrayAlpha:
                        gmicPixelFormat = GmicPixelFormat.GrayAlpha;
                        break;
                    case NativeImageFormat.Rgb:
                        gmicPixelFormat = GmicPixelFormat.Rgb;
                        break;
                    case NativeImageFormat.RgbAlpha:
                        gmicPixelFormat = GmicPixelFormat.RgbAlpha;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported { nameof(NativeImageFormat) } value: { imageData.format }.");
                }

                // G'MIC uses a planar format, so the stride between rows is the image width.
                int planeStride = width;

                TGmicBitmap bitmap = outputImageFactory.Create(width, height, gmicPixelFormat);

                bitmap.CopyFromGmicImage(imageData.format, imageData.pixels, planeStride);

                if (imageData.name != IntPtr.Zero)
                {
                    unsafe
                    {
                        bitmap.Name = System.Text.Encoding.UTF8.GetString((byte*)imageData.name, imageData.nameLength);
                    }
                }

                gmicBitmaps.Add(bitmap);
            }

            return new OutputImageCollection<TGmicBitmap>(gmicBitmaps);
        }

        private void Dispose()
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
                    updateProgressTimer.Dispose();
                    updateProgressTimer = null;
                }

                gmicWorkerThread = null;
            }
        }

        private unsafe void GmicWorker()
        {
            Exception error = null;
            bool canceled = false;

            progress = -1;
            shouldAbort = 0;

            CancellationTokenRegistration cancellationTokenRegistration = new CancellationTokenRegistration();
            try
            {
                if (cancellationToken.CanBeCanceled)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        canceled = true;
                    }
                    else
                    {
                        cancellationTokenRegistration = cancellationToken.Register(SignalCancelRequest);
                    }
                }

                if (!canceled)
                {
                    GmicOptions options = new GmicOptions(command,
                                                          customResourcePath,
                                                          hostName);

                    if (taskProgress != null)
                    {
                        StartUpdateProgressTimer();

                        if (cancellationToken.CanBeCanceled)
                        {
                            fixed (float* pProgress = &progress)
                            fixed (byte* pShouldAbort = &shouldAbort)
                            {
                                options.progress = pProgress;
                                options.abort = pShouldAbort;

                                GmicNative.RunGmic(gmicImages.SafeImageListHandle, options);
                            }

                            canceled = shouldAbort != 0;
                        }
                        else
                        {
                            fixed (float* pProgress = &progress)
                            {
                                options.progress = pProgress;

                                GmicNative.RunGmic(gmicImages.SafeImageListHandle, options);
                            }
                        }
                    }
                    else if (cancellationToken.CanBeCanceled)
                    {
                        fixed (byte* pShouldAbort = &shouldAbort)
                        {
                            options.abort = pShouldAbort;

                            GmicNative.RunGmic(gmicImages.SafeImageListHandle, options);
                        }

                        canceled = shouldAbort != 0;
                    }
                    else
                    {
                        GmicNative.RunGmic(gmicImages.SafeImageListHandle, options);
                    }
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }
            finally
            {
                cancellationTokenRegistration.Dispose();
            }

            synchronizationContext.Post(GmicWorkerCompletedCallback,
                                        new GmicWorkerCompletedState(error, canceled));
        }

        private void GmicWorkerCompletedCallback(object state)
        {
            gmicWorkerThread.Join();
            gmicWorkerThread = null;

            GmicWorkerCompletedState args = (GmicWorkerCompletedState)state;

            GmicWorkerCompleted(args.Error, args.Canceled);
        }

        private void GmicWorkerCompleted(Exception error, bool canceled)
        {
            StopUpdateProgressTimer();

            if (error != null)
            {
                Dispose();
                taskCompletionSource.TrySetException(error);
            }
            else if (canceled)
            {
                Dispose();
                taskCompletionSource.TrySetCanceled();
            }
            else
            {
                try
                {
                    OutputImageCollection<TGmicBitmap> outputGmicBitmaps = CreateOutputBitmaps();

                    Dispose();

                    taskCompletionSource.TrySetResult(outputGmicBitmaps);
                }
                catch (Exception ex)
                {
                    Dispose();
                    taskCompletionSource.TrySetException(ex);
                }
            }
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

                    taskProgress.Report(progressPercentage);
                }
            }

            progressUpdating = 0;
        }

        private void SignalCancelRequest()
        {
            shouldAbort = 1;
        }

        private void StartUpdateProgressTimer()
        {
            lastProgressValue = -1;
            updateProgressTimerCookie = new object();
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

        private sealed class GmicWorkerCompletedState
        {
            public GmicWorkerCompletedState(Exception error, bool canceled)
            {
                Error = error;
                Canceled = canceled;
            }

            public Exception Error { get; }

            public bool Canceled { get; }
        }
    }
}
