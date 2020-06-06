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
    internal static class Gmic_X64
    {
        private const string DllName = "GmicSharpNative_x64.dll";
        private const CallingConvention DllCallConv = CallingConvention.Cdecl;

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern SafeGmicImageListX64 GmicImageListCreate();

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern void GmicImageListDestroy(IntPtr handle);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern uint GmicImageListClear(SafeGmicImageList list);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern uint GmicImageListGetCount(SafeGmicImageList list);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus GmicImageListGetImageInfo(SafeGmicImageList list,
                                                                    uint index,
                                                                    out GmicImageListItemInfo info);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus GmicImageListAdd(SafeGmicImageList list,
                                                           uint width,
                                                           uint height,
                                                           uint stride,
                                                           IntPtr scan0,
                                                           NativeImageFormat format,
                                                           [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus GmicImageListCopyToOutput(SafeGmicImageList list,
                                                                    uint index,
                                                                    uint width,
                                                                    uint height,
                                                                    uint stride,
                                                                    IntPtr scan0,
                                                                    NativeImageFormat format);

        [DllImport(DllName, CallingConvention = DllCallConv)]
        internal static extern GmicStatus RunGmic(SafeGmicImageList list,
                                                  GmicOptions options,
                                                  [In, Out] GmicErrorInfo errorInfo);
    }
}
