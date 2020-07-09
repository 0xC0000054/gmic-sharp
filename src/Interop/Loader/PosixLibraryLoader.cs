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
using System.Reflection;
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal abstract class PosixLibraryLoader : LibraryLoader
    {
        protected PosixLibraryLoader(string dllFileExtension) : base(dllFileExtension)
        {
        }

        protected sealed override LoadLibraryResult LoadLibrary(string path)
        {
            if (NativeLibraryHelper.IsSupported)
            {
                return NativeLibraryHelper.Load(path);
            }
            else
            {
                IntPtr handle = NativeMethods.LoadLibary(path, NativeConstants.RTLD_NOW);

                if (handle != IntPtr.Zero)
                {
                    return new LoadLibraryResult(handle);
                }
                else
                {
                    string message = Marshal.PtrToStringAnsi(NativeMethods.GetErrorMessage()) ?? string.Empty;

                    return new LoadLibraryResult(new ExternalException(message));
                }
            }
        }

        protected sealed override IntPtr ResolveExportedSymbol(IntPtr libraryHandle, string name)
        {
            if (NativeLibraryHelper.IsSupported)
            {
                return NativeLibraryHelper.GetExport(libraryHandle, name);
            }
            else
            {
                return NativeMethods.GetExportedSymbol(libraryHandle, name);
            }
        }

        private static class NativeConstants
        {
            internal const int RTLD_NOW = 2;
        }

        private static class NativeMethods
        {
            [DllImport("libdl", EntryPoint = "dlopen")]
            internal static extern IntPtr LoadLibary([MarshalAs(UnmanagedType.LPStr)] string fileName, int flags);

            [DllImport("libdl", EntryPoint = "dlerror")]
            internal static extern IntPtr GetErrorMessage();

            [DllImport("libdl", EntryPoint = "dlsym")]
            internal static extern IntPtr GetExportedSymbol(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string symbol);
        }

        private static class NativeLibraryHelper
        {
            private delegate IntPtr NativeLibraryLoad(string path);
            private delegate bool NativeLibraryTryGetExport(IntPtr handle, string symbolName, out IntPtr address);

            private static readonly NativeLibraryLoad load;
            private static readonly NativeLibraryTryGetExport tryGetExport;

            static NativeLibraryHelper()
            {
                // The NativeLibrary class was introduced in .NET Core 3.0.
                // We use it via reflection so that we can target .NET Standard 2.1.

                Type nativeLibraryType = Type.GetType("System.Runtime.InteropServices.NativeLibrary");

                if (nativeLibraryType != null)
                {
                    MethodInfo loadMethodInfo = nativeLibraryType.GetMethod("Load",
                                                                            BindingFlags.Public | BindingFlags.Static,
                                                                            null,
                                                                            CallingConventions.Any,
                                                                            new Type[] { typeof(string) },
                                                                            null);

                    if (loadMethodInfo != null)
                    {
                        load = (NativeLibraryLoad)Delegate.CreateDelegate(typeof(NativeLibraryLoad), loadMethodInfo);
                    }

                    MethodInfo tryGetExportMethodInfo = nativeLibraryType.GetMethod("TryGetExport",
                                                                                    BindingFlags.Public | BindingFlags.Static,
                                                                                    null,
                                                                                    CallingConventions.Any,
                                                                                    new Type[] { typeof(IntPtr), typeof(string), typeof(IntPtr).MakeByRefType() },
                                                                                    null);

                    if (tryGetExportMethodInfo != null)
                    {
                        tryGetExport = (NativeLibraryTryGetExport)Delegate.CreateDelegate(typeof(NativeLibraryTryGetExport), tryGetExportMethodInfo);
                    }
                }

                IsSupported = nativeLibraryType != null && load != null && tryGetExport != null;
            }

            public static bool IsSupported { get; }

            public static LoadLibraryResult Load(string path)
            {
                if (!IsSupported)
                {
                    ExceptionUtil.ThrowInvalidOperationException("The NativeLibary class is not supported by the current runtime.");
                }

                try
                {
                    IntPtr handle = load.Invoke(path);
                    return new LoadLibraryResult(handle);
                }
                catch (Exception ex)
                {
                    return new LoadLibraryResult(ex);
                }
            }

            public static IntPtr GetExport(IntPtr handle, string symbolName)
            {
                if (!IsSupported)
                {
                    ExceptionUtil.ThrowInvalidOperationException("The NativeLibary class is not supported by the current runtime.");
                }

                return tryGetExport.Invoke(handle, symbolName, out IntPtr address) ? address : IntPtr.Zero;
            }
        }
    }
}
