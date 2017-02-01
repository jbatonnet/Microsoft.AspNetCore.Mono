using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.AspNetCore.Mono
{
    public class MonoAssemblyResolver
    {
        private static readonly string[] SearchDirectories;

        static MonoAssemblyResolver()
        {
            var mscorlibPath = Path.GetDirectoryName(typeof(int).Assembly.Location);
            var facadesPath = Path.Combine(mscorlibPath, "Facades");

            SearchDirectories = new string[]
            {
                Environment.CurrentDirectory,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                mscorlibPath,
                facadesPath,
            };
        }

        public static string FindAssembly(string assemblyName, string hintPath = null)
        {
            foreach (var directory in GetSearchDirectories(hintPath))
            {
                var fileInfo = new FileInfo(Path.Combine(directory, assemblyName + ".dll"));
                if (fileInfo.Exists)
                    return fileInfo.FullName;
            }

            return null;
        }

        private static IEnumerable<string> GetSearchDirectories(string hintPath = null)
        {
            if (!string.IsNullOrEmpty(hintPath))
                yield return hintPath;

            foreach (var directory in SearchDirectories)
                yield return directory;
        }
    }
}
