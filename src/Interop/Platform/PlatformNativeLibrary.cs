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

namespace GmicSharp.Interop
{
    internal abstract class PlatformNativeLibrary
    {
        internal abstract LoadLibraryResult Load(string path);

        internal abstract IntPtr GetExport(IntPtr libraryHandle, string name);


        internal readonly struct LoadLibraryResult
        {
            public LoadLibraryResult(Exception error)
            {
                if (error is null)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(error));
                }

                Error = error;
                Handle = IntPtr.Zero;
            }

            public LoadLibraryResult(IntPtr handle)
            {
                if (handle == IntPtr.Zero)
                {
                    ExceptionUtil.ThrowArgumentNullException(nameof(handle));
                }

                Error = null;
                Handle = handle;
            }

            public Exception Error { get; }

            public IntPtr Handle { get; }
        }

    }
}