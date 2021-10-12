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

        private GmicImageList<TGmicBitmap> gmicImages;
        private Timer updateProgressTimer;
        private bool disposed;
        private int progressUpdating;
        private object updateProgressTimerCookie;
        private int lastProgressValue;

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
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void StartAsync()
        {
            if (disposed)
            {
                ExceptionUtil.ThrowObjectDisposedException(nameof(GmicRunner<TGmicBitmap>));
            }

            try
            {
                _ = System.Threading.Tasks.Task.Run(GmicWorker, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                Dispose();
                taskCompletionSource.TrySetCanceled();
            }
            catch (Exception ex)
            {
                Dispose();
                taskCompletionSource.TrySetException(ex);
            }
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
                    case NativeImageFormat.Gray8:
                        gmicPixelFormat = GmicPixelFormat.Gray8;
                        break;
                    case NativeImageFormat.GrayAlpha88:
                        gmicPixelFormat = GmicPixelFormat.GrayAlpha16;
                        break;
                    case NativeImageFormat.Rgb888:
                        gmicPixelFormat = GmicPixelFormat.Rgb24;
                        break;
                    case NativeImageFormat.Rgba8888:
                        gmicPixelFormat = GmicPixelFormat.Rgba32;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported { nameof(NativeImageFormat) } value: { imageData.format }.");
                }

                // G'MIC uses a planar format, so the stride between rows is the image width.
                int planeStride = width;

                TGmicBitmap bitmap = outputImageFactory.Create(width, height, gmicPixelFormat);

                bitmap.CopyFromGmicImage(imageData.format, imageData.pixels, planeStride);

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

            GmicWorkerCompleted(error, canceled);
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
    }
}
