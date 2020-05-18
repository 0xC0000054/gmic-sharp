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

namespace GmicSharp.Interop
{
    internal enum GmicStatus
    {
        Ok = 0,
        InvalidParameter,
        OutOfMemory,
        UnknownImageFormat,
        GmicError,
        GmicResourcePathInitFailed,
        GmicUnsupportedChannelCount,
        ImageListIndexOutOfRange,
        UnknownError
    }
}
