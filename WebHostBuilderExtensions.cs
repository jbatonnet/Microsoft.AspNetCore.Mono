using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyModel.Resolution;

namespace Microsoft.AspNetCore.Mono
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseMonoCompatibility(this IWebHostBuilder hostBuilder)
        {
            // Just return if we are not using Mono
            if (Type.GetType("Mono.Runtime") == null)
                return hostBuilder;

            // Register to assembly resolve event
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            // Access default resolver
            var compilationLibraryType = typeof(CompilationLibrary);
            var defaultResolverProperty = compilationLibraryType.GetProperty(
                "DefaultResolver",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            var defaultResolver = defaultResolverProperty.GetValue(null) as CompositeCompilationAssemblyResolver;

            // Access resolvers array
            var compositeCompilationAssemblyResolverType = typeof(CompositeCompilationAssemblyResolver);
            var resolversField = compositeCompilationAssemblyResolverType.GetField(
                "_resolvers",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            var resolvers = resolversField.GetValue(defaultResolver) as ICompilationAssemblyResolver[];

            // Replace .NET resolver with a Mono one
            for (var i = 0; i < resolvers.Length; i++)
            {
                var referenceAssemblyPathResolver = resolvers[i] as ReferenceAssemblyPathResolver;

                if (referenceAssemblyPathResolver != null)
                    resolvers[i] = new MonoCompilationAssemblyResolver(referenceAssemblyPathResolver);
            }

            return hostBuilder;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            string hintPath = null;
            if (args.RequestingAssembly?.IsDynamic == false)
            {
                var requestingAssemblyPath = args.RequestingAssembly.Location;

                if (!string.IsNullOrEmpty(requestingAssemblyPath))
                    hintPath = Path.GetDirectoryName(requestingAssemblyPath);
            }

            var assemblyPath = MonoAssemblyResolver.FindAssembly(assemblyName.Name, hintPath);
            return assemblyPath != null ? Assembly.LoadFile(assemblyPath) : null;
        }
    }
}