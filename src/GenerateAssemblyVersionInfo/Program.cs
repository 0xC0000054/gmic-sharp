////////////////////////////////////////////////////////////////////////
//
// This file is part of gmic-sharp, a .NET wrapper for G'MIC.
//
// Copyright (c) 2020, 2021, 2022 Nicholas Hayes
//
// This file is licensed under the MIT License.
// See LICENSE.txt for complete licensing and attribution information.
//
////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace GenerateAssemblyVersionInfo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: GenerateAssemblyInfo SolutionDir");
            }

            string solutionDir = args[0].Replace("\"", string.Empty);

            string assemblyVersionInfoPath = Path.Combine(solutionDir, "Properties", "AssemblyVersionInfo.cs");

            CreateAssemblyVersionInfo(assemblyVersionInfoPath);
        }

        private static void CreateAssemblyVersionInfo(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
            {
                sw.WriteLine("////////////////////////////////////////////////////////////////////////");
                sw.WriteLine("//");
                sw.WriteLine("// This file is part of gmic-sharp, a .NET wrapper for G'MIC.");
                sw.WriteLine("//");
                sw.WriteLine("// Copyright (c) {0} Nicholas Hayes", DateTime.UtcNow.Year.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("//");
                sw.WriteLine("// This file is licensed under the MIT License.");
                sw.WriteLine("// See LICENSE.txt for complete licensing and attribution information.");
                sw.WriteLine("//");
                sw.WriteLine("////////////////////////////////////////////////////////////////////////");
                sw.WriteLine();
                sw.WriteLine("// NOTE: This file is generated at build time. Modifications should be made");
                sw.WriteLine("// to the appropriate file in the GenerateAssemblyVersionInfo project.");
                sw.WriteLine();
                sw.WriteLine("using System;");
                sw.WriteLine("using System.Reflection;");
                sw.WriteLine();
                sw.WriteLine("[assembly: AssemblyVersion(\"{0}.{1}.{2}.0\")]",
                             Info.MajorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.MinorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.PatchVersion.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("[assembly: AssemblyFileVersion(\"{0}.{1}.{2}.0\")]",
                             Info.MajorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.MinorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.PatchVersion.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine();
                sw.WriteLine("namespace GmicSharp");
                sw.WriteLine("{");
                sw.WriteLine("    internal static class AssemblyVersionInfo");
                sw.WriteLine("    {");
                sw.WriteLine("        // This field allows the native library version check to work without");
                sw.WriteLine("        // depending on the AssemblyVersion attributes, which can change if this");
                sw.WriteLine("        // library is merged into another .NET binary.");
                sw.WriteLine("        internal static readonly Version LibraryVersion = new Version({0}, {1}, {2});",
                             Info.MajorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.MinorVersion.ToString(CultureInfo.InvariantCulture),
                             Info.PatchVersion.ToString(CultureInfo.InvariantCulture));
                sw.WriteLine("    }");
                sw.WriteLine("}");
                sw.WriteLine();
            }
        }
    }
}
