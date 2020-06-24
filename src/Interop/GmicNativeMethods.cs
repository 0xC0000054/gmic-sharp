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
    internal static class GmicNativeMethods
    {
        private const string DllName = LibraryLoader.DllName;
        private const CallingConvention DllCallConv = CallingConvention.Cdecl;

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern void GetLibraryVersion(out int major, out int minor, out int patch);


        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern SafeGmicImageList GmicImageListCreate();

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern void GmicImageListDestroy(IntPtr handle);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern uint GmicImageListClear(SafeGmicImageList list);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern uint GmicImageListGetCount(SafeGmicImageList list);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus GmicImageListGetImageData(SafeGmicImageList list,
                                                                    uint index,
                                                                    [In, Out] GmicImageListImageData info);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus GmicImageListAdd(SafeGmicImageList list,
                                                           uint width,
                                                           uint height,
                                                           NativeImageFormat format,
                                                           [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
                                                           [In, Out] GmicImageListPixelData pixelData);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus RunGmic(SafeGmicImageList list,
                                                  GmicOptions options,
                                                  [In, Out] GmicErrorInfo errorInfo);
    }
}
