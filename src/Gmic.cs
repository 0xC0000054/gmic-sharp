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
using System.Collections.Generic;
using System.Threading;

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
        private CancellationToken cancellationToken;
        private bool disposed;
        private int progressUpdating;
        private object updateProgressTimerCookie;
#pragma warning disable IDE0032 // Use auto property
        private IReadOnlyList<TGmicBitmap> outputGmicBitmaps;
        private string hostName;
#pragma warning restore IDE0032 // Use auto property

        private readonly IGmicOutputImageFactory<TGmicBitmap> outputImageFactory;
        private readonly GmicRunner gmicRunner;

        /// <summary>
        /// The default host name that gmic-sharp-native uses.
        /// </summary>
        private const string DefaultHostName = "gmic-sharp";

        /// <summary>
        /// Initializes a new instance of the <see cref="Gmic{TGmicBitmap}" /> class.
        /// </summary>
        /// <param name="outputImageFactory">The factory that creates the output images.</param>
        /// <exception cref="ArgumentNullException"><paramref name="outputImageFactory"/> is null.</exception>
        /// <exception cref="GmicException">Unable to create the G'MIC image list.</exception>
        public Gmic(IGmicOutputImageFactory<TGmicBitmap> outputImageFactory)
        {
            if (outputImageFactory is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(outputImageFactory));
            }

            this.outputImageFactory = outputImageFactory;
            gmicImages = new GmicImageList();
            gmicRunner = new GmicRunner();
            gmicRunner.GmicCompleted += new EventHandler<GmicCompletedEventArgs>(GmicRenderingCompleted);

        }

        /// <summary>
        /// Occurs when G'MIC has finished processing.
        /// </summary>
        public event EventHandler<GmicCompletedEventArgs> GmicDone;

        /// <summary>
        /// Occurs when G'MIC reports rendering progress.
        /// </summary>
        public event EventHandler<GmicProgressEventArgs> GmicProgress;

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
        public bool GmicRunning => gmicRunner.IsRunning;

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
        /// Gets the output images.
        /// </summary>
        /// <value>
        /// The output images.
        /// </value>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public IReadOnlyList<TGmicBitmap> OutputImages
        {
            get
            {
                VerifyNotDisposed();

                return outputGmicBitmaps;
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

            if (gmicRunner.IsRunning)
            {
                ExceptionUtil.ThrowInvalidOperationException("Cannot add an input image when G'MIC is running.");
            }

            gmicImages.Add(bitmap, name);
        }

        /// <summary>
        /// Clears the input images that have been added this G'MIC instance.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        public void ClearInputImages()
        {
            VerifyNotDisposed();

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

                if (outputGmicBitmaps != null)
                {
                    for (int i = 0; i < outputGmicBitmaps.Count; i++)
                    {
                        outputGmicBitmaps[i].Dispose();
                    }
                    outputGmicBitmaps = null;
                }
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
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public void RunGmic(string command)
        {
            RunGmic(command, CancellationToken.None);
        }

        /// <summary>
        /// Executes G'MIC with the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="command"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="command"/> is a empty or contains only white space.</exception>
        /// <exception cref="GmicException">An error occurred when running G'MIC.</exception>
        /// <exception cref="InvalidOperationException">
        /// This G'MIC instance is already running.
        /// </exception>
        /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public void RunGmic(string command, CancellationToken cancellationToken)
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

            if (gmicRunner.IsRunning)
            {
                ExceptionUtil.ThrowInvalidOperationException("This G'MIC instance is already running.");
            }

            bool hasProgressEvent = GmicProgress != null;
            this.cancellationToken = cancellationToken;

            if (hasProgressEvent || cancellationToken.CanBeCanceled)
            {
                StartUpdateProgressTimer();
            }

            gmicRunner.Start(command,
                             CustomResourcePath,
                             hostName,
                             gmicImages,
                             cancellationToken,
                             hasProgressEvent);
        }

        private void GmicRenderingCompleted(object sender, GmicCompletedEventArgs e)
        {
            StopUpdateProgressTimer();

            try
            {
                if (e.Error == null && !e.Canceled)
                {
                    if (outputGmicBitmaps != null)
                    {
                        for (int i = 0; i < outputGmicBitmaps.Count; i++)
                        {
                            outputGmicBitmaps[i].Dispose();
                        }
                        outputGmicBitmaps = null;
                    }

                    outputGmicBitmaps = CreateOutputBitmaps();

                    for (int i = 0; i < outputGmicBitmaps.Count; i++)
                    {
                        gmicImages.CopyToOutput((uint)i, outputGmicBitmaps[i]);
                    }
                }
                gmicImages.Clear();
            }
            catch (Exception ex)
            {
                OnGmicDone(new GmicCompletedEventArgs(ex, false));
                return;
            }

            OnGmicDone(e);
        }

        private void OnGmicDone(GmicCompletedEventArgs e)
        {
            GmicDone?.Invoke(this, e);
        }

        private IReadOnlyList<TGmicBitmap> CreateOutputBitmaps()
        {
            List<TGmicBitmap> gmicBitmaps = new List<TGmicBitmap>((int)gmicImages.Count);

            for (int i = 0; i < gmicBitmaps.Capacity; i++)
            {
                Interop.GmicImageListItemInfo itemInfo;

                gmicImages.GetImageInfo((uint)i, out itemInfo);

                if (itemInfo.width > int.MaxValue || itemInfo.height > int.MaxValue)
                {
                    throw new NotSupportedException($"The output G'MIC image dimensions exceed { int.MaxValue }.");
                }

                int width = (int)itemInfo.width;
                int height = (int)itemInfo.height;

                GmicPixelFormat gmicPixelFormat;
                switch (itemInfo.format)
                {
                    case Interop.NativeImageFormat.Gray8:
                        gmicPixelFormat = GmicPixelFormat.Gray;
                        break;
                    case Interop.NativeImageFormat.Bgr888:
                        gmicPixelFormat = GmicPixelFormat.Bgr24;
                        break;
                    case Interop.NativeImageFormat.Bgr888x:
                        gmicPixelFormat = GmicPixelFormat.Bgr32;
                        break;
                    case Interop.NativeImageFormat.Bgra8888:
                        gmicPixelFormat = GmicPixelFormat.Bgra32;
                        break;
                    case Interop.NativeImageFormat.Rgb888:
                        gmicPixelFormat = GmicPixelFormat.Rgb24;
                        break;
                    case Interop.NativeImageFormat.Rgb888x:
                        gmicPixelFormat = GmicPixelFormat.Rgb32;
                        break;
                    case Interop.NativeImageFormat.Rgba8888:
                        gmicPixelFormat = GmicPixelFormat.Rgba32;
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported { nameof(Interop.NativeImageFormat) } value: { itemInfo.format }.");
                }

                gmicBitmaps.Add(outputImageFactory.Create(width, height, gmicPixelFormat));
            }

            return gmicBitmaps;
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

                GmicProgress?.Invoke(this, new GmicProgressEventArgs((int)progress));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                gmicRunner.SignalCancelRequest();
            }

            progressUpdating = 0;
        }

        private void StartUpdateProgressTimer()
        {
            if (updateProgressTimer == null)
            {
                updateProgressTimerCookie = new object();
                updateProgressTimer = new Timer(OnUpdateProgress, updateProgressTimerCookie, 1000, 250);
            }
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
    }
}
