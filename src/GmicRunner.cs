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

using GmicSharp.Interop;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace GmicSharp
{
    internal sealed class GmicRunner
    {
        private AsyncOperation asyncOperation;
#pragma warning disable IDE0032 // Use auto property
        private bool isRunning;
#pragma warning restore IDE0032 // Use auto property
        private readonly SendOrPostCallback workerCompleted;

        private float progress;
        private byte shouldAbort;

        public GmicRunner()
        {
            asyncOperation = null;
            workerCompleted = new SendOrPostCallback(GmicWorkerCompleted);
        }

        public event EventHandler<GmicCompletedEventArgs> GmicCompleted;

        public bool IsRunning => isRunning;

        public float GetProgress()
        {
            return progress;
        }

        public void SignalCancelRequest()
        {
            shouldAbort = 1;
        }

        /// <summary>
        /// Runs the specified G'MIC command.
        /// </summary>
        /// <param name="command">The G'MIC command.</param>
        /// <param name="customResourcePath">The custom resource path.</param>
        /// <param name="customUserPath">The custom user path.</param>
        /// <param name="imageList">The image list.</param>
        /// <param name="token">The cancellation token.</param>
        /// <param name="hasProgressEvent"><c>true</c> if the caller whats progress reports; otherwise, <c>false</c>.</param>
        /// <exception cref="InvalidOperationException">This G'MIC instance is already running.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public void Start(string command,
                          string customResourcePath,
                          string customUserPath,
                          GmicImageList imageList,
                          CancellationToken token,
                          bool hasProgressEvent)
        {
            if (isRunning)
            {
                ExceptionUtil.ThrowInvalidOperationException("This G'MIC instance is already running.");
            }

            token.ThrowIfCancellationRequested();

            asyncOperation = AsyncOperationManager.CreateOperation(null);

            GmicWorkerArgs args = new GmicWorkerArgs(command,
                                                     customResourcePath,
                                                     customUserPath,
                                                     imageList,
                                                     token,
                                                     hasProgressEvent);
            Task task = Task.Run(() => GmicWorker(args), token);

            isRunning = TaskIsRunning(task);
        }

        private static bool TaskIsRunning(Task task)
        {
            if (!task.IsCompleted)
            {
                switch (task.Status)
                {
                    case TaskStatus.Created:
                    case TaskStatus.WaitingForActivation:
                    case TaskStatus.WaitingToRun:
                    case TaskStatus.Running:
                        return true;
                }
            }

            return false;
        }

        private unsafe void GmicWorker(GmicWorkerArgs args)
        {
            Exception error = null;
            bool canceled = false;

            if (args.Token.IsCancellationRequested)
            {
                canceled = true;
            }
            else
            {
                progress = -1;
                shouldAbort = 0;

                GmicOptions options = new GmicOptions(args.Command,
                                                      args.CustomResourcePath,
                                                      args.CustomUserPath);

                try
                {
                    if (args.HasProgressEvent)
                    {
                        if (args.Token.CanBeCanceled)
                        {
                            fixed (float* pProgress = &progress)
                            fixed (byte* pShouldAbort = &shouldAbort)
                            {
                                options.progress = pProgress;
                                options.abort = pShouldAbort;

                                GmicNative.RunGmic(args.ImageList.SafeImageListHandle, options);
                            }

                            canceled = shouldAbort != 0;
                        }
                        else
                        {
                            fixed (float* pProgress = &progress)
                            {
                                options.progress = pProgress;

                                GmicNative.RunGmic(args.ImageList.SafeImageListHandle, options);
                            }
                        }
                    }
                    else if (args.Token.CanBeCanceled)
                    {
                        fixed (byte* pShouldAbort = &shouldAbort)
                        {
                            options.abort = pShouldAbort;

                            GmicNative.RunGmic(args.ImageList.SafeImageListHandle, options);
                        }

                        canceled = shouldAbort != 0;
                    }
                    else
                    {
                        GmicNative.RunGmic(args.ImageList.SafeImageListHandle, options);
                    }
                }
                catch (Exception ex)
                {
                    error = ex;
                }
            }

            asyncOperation.PostOperationCompleted(workerCompleted, new WorkerCompletedArgs(error, canceled));
        }

        private void GmicWorkerCompleted(object state)
        {
            isRunning = false;

            WorkerCompletedArgs args = (WorkerCompletedArgs)state;

            GmicCompleted?.Invoke(this, new GmicCompletedEventArgs(args.Error, args.Canceled));
        }

        private sealed class GmicWorkerArgs
        {
            public GmicWorkerArgs(string command,
                                  string customResourcePath,
                                  string customUserPath,
                                  GmicImageList imageList,
                                  CancellationToken token,
                                  bool hasProgressEvent)
            {
                Command = command;
                CustomResourcePath = customResourcePath;
                CustomUserPath = customUserPath;
                ImageList = imageList;
                Token = token;
                HasProgressEvent = hasProgressEvent;
            }

            public string Command { get; }

            public string CustomResourcePath { get; }

            public string CustomUserPath { get; }

            public GmicImageList ImageList { get;  }

            public CancellationToken Token { get; }

            public bool HasProgressEvent { get; }
        }

        private sealed class WorkerCompletedArgs
        {
            public WorkerCompletedArgs(Exception error, bool canceled)
            {
                Error = error;
                Canceled = canceled;
            }

            public Exception Error { get; }

            public bool Canceled { get; }
        }
    }
}
