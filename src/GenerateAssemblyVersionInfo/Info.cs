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

namespace GenerateAssemblyVersionInfo
{
    internal static class Info
    {
        // The file version constants that are used when generating the GmicSharp
        // AssemblyVersionInfo file.
        // There are only three values because GmicSharp uses Semantic Versioning
        // https://semver.org/spec/v2.0.0.html.

        public const int MajorVersion = 0;
        public const int MinorVersion = 6;
        public const int PatchVersion = 0;
    }
}
