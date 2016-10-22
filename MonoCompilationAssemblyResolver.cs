using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Microsoft.AspNetCore.Mono
{
    public class MonoCompilationAssemblyResolver : ICompilationAssemblyResolver
    {
        public ICompilationAssemblyResolver Resolver { get; }

        public MonoCompilationAssemblyResolver(ICompilationAssemblyResolver resolver)
        {
            Resolver = resolver;
        }

        public bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies)
        {
            string[] searchDirectories = new string[]
            {
                Environment.CurrentDirectory,
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "/usr/lib/mono/4.5",
                "/usr/lib/mono/4.5/Facades",
            };

            foreach (string directory in searchDirectories)
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(directory, library.Name + ".dll"));
                if (fileInfo.Exists)
                {
                    assemblies.Add(fileInfo.FullName);
                    return true;
                }
            }

            return Resolver.TryResolveAssemblyPaths(library, assemblies);
        }
    }
}
