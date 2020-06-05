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
using System.Threading.Tasks;

namespace GmicSharp
{
    internal sealed class GmicRunner
    {
        private readonly Action<Exception, bool> workerCompleted;

        private float progress;
        private byte shouldAbort;

        /// <summary>
        /// Initializes a new instance of the <see cref="GmicRunner"/> class.
        /// </summary>
        /// <param name="workerCompleted">The worker completed callback.</param>
        /// <exception cref="ArgumentNullException"><paramref name="workerCompleted"/> is null.</exception>
        public GmicRunner(Action<Exception, bool> workerCompleted)
        {
            if (workerCompleted is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(workerCompleted));
            }

            this.workerCompleted = workerCompleted;
        }

        public bool IsBusy { get; private set; }

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
        /// <param name="hostName">The host application name.</param>
        /// <param name="imageList">The image list.</param>
        /// <param name="hasProgressEvent"><c>true</c> if the caller whats progress reports; otherwise, <c>false</c>.</param>
        /// <exception cref="InvalidOperationException">This G'MIC instance is already running.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public void StartAsync(string command,
                               string customResourcePath,
                               string hostName,
                               GmicImageList imageList,
                               bool hasProgressEvent)
        {
            if (IsBusy)
            {
                ExceptionUtil.ThrowInvalidOperationException("This G'MIC instance is already running.");
            }

            progress = -1;
            shouldAbort = 0;

            GmicWorkerArgs args = new GmicWorkerArgs(command,
                                                     customResourcePath,
                                                     hostName,
                                                     imageList,
                                                     hasProgressEvent);
            Task task = Task.Run(() => GmicWorker(args));

            IsBusy = TaskIsRunning(task);
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


            GmicOptions options = new GmicOptions(args.Command,
                                                  args.CustomResourcePath,
                                                  args.HostName);
            try
            {
                if (args.HasProgressEvent)
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
                    fixed (byte* pShouldAbort = &shouldAbort)
                    {
                        options.abort = pShouldAbort;

                        GmicNative.RunGmic(args.ImageList.SafeImageListHandle, options);
                    }

                    canceled = shouldAbort != 0;
                }
            }
            catch (Exception ex)
            {
                error = ex;
            }

            GmicWorkerCompleted(error, canceled);
        }

        private void GmicWorkerCompleted(Exception error, bool canceled)
        {
            IsBusy = false;

            workerCompleted.Invoke(error, canceled);
        }

        private sealed class GmicWorkerArgs
        {
            public GmicWorkerArgs(string command,
                                  string customResourcePath,
                                  string hostName,
                                  GmicImageList imageList,
                                  bool hasProgressEvent)
            {
                Command = command;
                CustomResourcePath = customResourcePath;
                HostName = hostName;
                ImageList = imageList;
                HasProgressEvent = hasProgressEvent;
            }

            public string Command { get; }

            public string CustomResourcePath { get; }

            public string HostName { get; }

            public GmicImageList ImageList { get;  }

            public bool HasProgressEvent { get; }
        }
    }
}
