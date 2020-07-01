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
using System.Runtime.InteropServices;

namespace GmicSharp.Interop
{
    internal sealed class GmicNativeMethods
    {
        private readonly GetLibraryVersionDelegate getLibraryVersion;
        private readonly GmicImageListCreateDelegate gmicImageListCreate;
        private readonly GmicImageListDestroyDelegate gmicImageListDestroy;
        private readonly GmicImageListClearDelegate gmicImageListClear;
        private readonly GmicImageListGetCountDelegate gmicImageListGetCount;
        private readonly GmicImageListGetImageDataDelegate gmicImageListGetImageData;
        private readonly GmicImageListAddDelegate gmicImageListAdd;
        private readonly RunGmicDelegate runGmic;

        private static GmicNativeMethods instance;

        private const CallingConvention DllCallConv = CallingConvention.Cdecl;

        #region Delegates
        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate void GetLibraryVersionDelegate(out int major, out int minor, out int patch);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate SafeGmicImageList GmicImageListCreateDelegate();

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate void GmicImageListDestroyDelegate(IntPtr handle);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate void GmicImageListClearDelegate(SafeGmicImageList list);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate uint GmicImageListGetCountDelegate(SafeGmicImageList list);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate GmicStatus GmicImageListGetImageDataDelegate(SafeGmicImageList list,
                                                                      uint index,
                                                                      [In, Out] GmicImageListImageData info);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate GmicStatus GmicImageListAddDelegate(SafeGmicImageList list,
                                                             uint width,
                                                             uint height,
                                                             NativeImageFormat format,
                                                             [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
                                                             [In, Out] GmicImageListPixelData pixelData);

        [UnmanagedFunctionPointer(DllCallConv)]
        private delegate GmicStatus RunGmicDelegate(SafeGmicImageList list,
                                                    GmicOptions options,
                                                    [In, Out] GmicErrorInfo errorInfo);
        #endregion

        private GmicNativeMethods(IntPtr libraryHandle, LibraryLoader loader)
        {
            getLibraryVersion = loader.GetExport<GetLibraryVersionDelegate>(libraryHandle, "GetLibraryVersion");
            gmicImageListCreate = loader.GetExport<GmicImageListCreateDelegate>(libraryHandle, "GmicImageListCreate");
            gmicImageListDestroy = loader.GetExport<GmicImageListDestroyDelegate>(libraryHandle, "GmicImageListDestroy");
            gmicImageListClear = loader.GetExport<GmicImageListClearDelegate>(libraryHandle, "GmicImageListClear");
            gmicImageListGetCount = loader.GetExport<GmicImageListGetCountDelegate>(libraryHandle, "GmicImageListGetCount");
            gmicImageListGetImageData = loader.GetExport<GmicImageListGetImageDataDelegate>(libraryHandle, "GmicImageListGetImageData");
            gmicImageListAdd = loader.GetExport<GmicImageListAddDelegate>(libraryHandle, "GmicImageListAdd");
            runGmic = loader.GetExport<RunGmicDelegate>(libraryHandle, "RunGmic");
        }


        public static GmicNativeMethods Instance
        {
            get
            {
                if (instance == null)
                {
                    ExceptionUtil.ThrowInvalidOperationException("Must call Initialize() before using this property.");
                }

                return instance;
            }
        }

        public static void Initialize(IntPtr libraryHandle, LibraryLoader loader)
        {
            if (libraryHandle == IntPtr.Zero)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(libraryHandle));
            }

            if (loader is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(loader));
            }

            if (instance == null)
            {
                instance = new GmicNativeMethods(libraryHandle, loader);
            }
        }


        internal void GetLibraryVersion(out int major, out int minor, out int patch)
        {
            getLibraryVersion(out major, out minor, out patch);
        }

        internal SafeGmicImageList GmicImageListCreate()
        {
            return gmicImageListCreate();
        }

        internal void GmicImageListDestroy(IntPtr handle)
        {
            gmicImageListDestroy(handle);
        }

        internal void GmicImageListClear(SafeGmicImageList list)
        {
            gmicImageListClear(list);
        }

        internal uint GmicImageListGetCount(SafeGmicImageList list)
        {
            return gmicImageListGetCount(list);
        }

        internal GmicStatus GmicImageListGetImageData(SafeGmicImageList list,
                                                      uint index,
                                                      GmicImageListImageData info)
        {
            return gmicImageListGetImageData(list, index, info);
        }

        internal GmicStatus GmicImageListAdd(SafeGmicImageList list,
                                             uint width,
                                             uint height,
                                             NativeImageFormat format,
                                             string name,
                                             GmicImageListPixelData pixelData)
        {
            return gmicImageListAdd(list, width, height, format, name, pixelData);
        }

        internal GmicStatus RunGmic(SafeGmicImageList list,
                                    GmicOptions options,
                                    GmicErrorInfo errorInfo)
        {
            return runGmic(list, options, errorInfo);
        }
    }
}
