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

namespace GmicSharp
{
    internal sealed class GmicRunner
    {
        private AsyncOperation asyncOperation;
#pragma warning disable IDE0032 // Use auto property
        private bool isRunning;
#pragma warning restore IDE0032 // Use auto property
        private readonly GmicWorkerDelegate workerDelegate;
        private readonly SendOrPostCallback workerCompleted;

        private float progress;
        private byte shouldAbort;

        private delegate void GmicWorkerDelegate(GmicWorkerArgs args);

        public GmicRunner()
        {
            asyncOperation = null;
            workerDelegate = new GmicWorkerDelegate(GmicWorker);
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

            if (token.IsCancellationRequested)
            {
                return;
            }

            isRunning = true;

            asyncOperation = AsyncOperationManager.CreateOperation(null);

            GmicWorkerArgs args = new GmicWorkerArgs(command,
                                                     customResourcePath,
                                                     customUserPath,
                                                     imageList,
                                                     token,
                                                     hasProgressEvent);
            workerDelegate.BeginInvoke(args, null, null);
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
