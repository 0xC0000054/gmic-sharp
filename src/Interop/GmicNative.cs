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
using System.ComponentModel;
using System.Text;

namespace GmicSharp.Interop
{
    internal static class GmicNative
    {
        internal static SafeGmicImageList CreateGmicImageList()
        {
            return GmicNativeMethods.GmicImageListCreate();
        }

        internal static void GmicImageListClear(SafeGmicImageList list)
        {
            ValidateGmicImageList(list);

            GmicNativeMethods.GmicImageListClear(list);
        }

        internal static uint GmicImageListGetCount(SafeGmicImageList list)
        {
            ValidateGmicImageList(list);

            return GmicNativeMethods.GmicImageListGetCount(list);
        }

        internal static void GmicImageListGetImageData(SafeGmicImageList list, uint index, out GmicImageListImageData data)
        {
            ValidateGmicImageList(list);

            data = new GmicImageListImageData();

            GmicStatus status = GmicNativeMethods.GmicImageListGetImageData(list, index, data);

            if (status != GmicStatus.Ok)
            {
                HandleError(status);
            }
        }

        internal static void GmicImageListAdd(SafeGmicImageList list,
                                              uint width,
                                              uint height,
                                              GmicPixelFormat format,
                                              string name,
                                              out GmicImageListPixelData pixelData,
                                              out NativeImageFormat nativeImageFormat)
        {
            ValidateGmicImageList(list);

            pixelData = new GmicImageListPixelData();
            nativeImageFormat = ConvertToNativeImageFormat(format);

            GmicStatus status = GmicNativeMethods.GmicImageListAdd(list,
                                                                   width,
                                                                   height,
                                                                   nativeImageFormat,
                                                                   string.IsNullOrWhiteSpace(name) ? null : name,
                                                                   pixelData);

            if (status != GmicStatus.Ok)
            {
                HandleError(status);
            }
        }

        internal static void RunGmic(SafeGmicImageList list, GmicOptions options)
        {
            if (options is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(options));
            }

            ValidateGmicImageList(list);

            GmicErrorInfo errorInfo = new GmicErrorInfo();

            GmicStatus status = GmicNativeMethods.RunGmic(list, options, errorInfo);

            if (status != GmicStatus.Ok)
            {
                HandleError(status, errorInfo);
            }
        }

        private static NativeImageFormat ConvertToNativeImageFormat(GmicPixelFormat format)
        {
            switch (format)
            {
                case GmicPixelFormat.Gray8:
                    return NativeImageFormat.Gray8;
                case GmicPixelFormat.Rgb24:
                    return NativeImageFormat.Rgb888;
                case GmicPixelFormat.Rgba32:
                    return NativeImageFormat.Rgba8888;
                default:
                    throw new InvalidEnumArgumentException(nameof(format), (int)format, typeof(GmicPixelFormat));
            }
        }

        private static void HandleError(GmicStatus status)
        {
            HandleError(status, null);
        }

        private static void HandleError(GmicStatus status, GmicErrorInfo errorInfo)
        {
            switch (status)
            {
                case GmicStatus.Ok:
                    break;
                case GmicStatus.InvalidParameter:
                    throw new GmicException("An invalid parameter was passed to a native function.");
                case GmicStatus.OutOfMemory:
                    throw new OutOfMemoryException();
                case GmicStatus.UnknownImageFormat:
                    throw new GmicException("The image uses an unknown format.");
                case GmicStatus.GmicError:
                    if (errorInfo != null && TryGetMessageFromErrorInfo(errorInfo, out string message))
                    {
                        string command = GetStringFromByteArray(errorInfo.commandName);

                        throw new GmicException(message, command);
                    }
                    else
                    {
                        throw new GmicException("An unspecified error occurred when executing G'MIC.");
                    }
                case GmicStatus.GmicResourcePathInitFailed:
                    break;
                case GmicStatus.GmicUnsupportedChannelCount:
                    throw new GmicException("The output G'MIC image has an unsupported number of channels.");
                case GmicStatus.ImageListIndexOutOfRange:
                    throw new GmicException("The image list index is not valid.");
                case GmicStatus.UnknownError:
                default:
                    throw new GmicException("An unspecified error occurred.");
            }
        }

        private static bool TryGetMessageFromErrorInfo(GmicErrorInfo errorInfo, out string message)
        {
            if (errorInfo is null)
            {
                message = string.Empty;

                return false;
            }

            message = GetStringFromByteArray(errorInfo.errorMessage);

            return !string.IsNullOrWhiteSpace(message);
        }

        private static string GetStringFromByteArray(byte[] data)
        {
            int count = 0;

            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (data[i] != 0)
                {
                    count = i;
                    break;
                }
            }

            if (count == 0)
            {
                return string.Empty;
            }
            else
            {
                return Encoding.ASCII.GetString(data, 0, count);
            }
        }

        private static void ValidateGmicImageList(SafeGmicImageList list)
        {
            if (list is null)
            {
                ExceptionUtil.ThrowArgumentNullException(nameof(list));
            }

            if (list.IsClosed || list.IsInvalid)
            {
                ExceptionUtil.ThrowArgumentException($"The { nameof(SafeGmicImageList) } handle is closed or invalid.");
            }
        }
    }
}
