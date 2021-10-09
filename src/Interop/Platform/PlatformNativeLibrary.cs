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
        /// <summary>
        /// Loads a native library from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>
        /// A <see cref="LoadLibraryResult"/> containing the result of the operation.
        /// </returns>
        internal abstract LoadLibraryResult Load(string path);

        /// <summary>
        /// Gets the exported symbol with the specified name.
        /// </summary>
        /// <param name="libraryHandle">The library handle.</param>
        /// <param name="name">The name of the exported symbol.</param>
        /// <returns>
        /// The address of the exported symbol; otherwise, <see cref="IntPtr.Zero"/> if the symbol was not found.
        /// </returns>
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