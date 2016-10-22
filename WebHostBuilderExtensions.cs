using System;
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
            // Access default resolver
            Type compilationLibraryType = typeof(CompilationLibrary);
            PropertyInfo defaultResolverProperty = compilationLibraryType.GetProperty("DefaultResolver", BindingFlags.NonPublic | BindingFlags.Static);

            CompositeCompilationAssemblyResolver defaultResolver = defaultResolverProperty.GetValue(null) as CompositeCompilationAssemblyResolver;

            // Access resolvers array
            Type compositeCompilationAssemblyResolverType = typeof(CompositeCompilationAssemblyResolver);
            FieldInfo resolversField = compositeCompilationAssemblyResolverType.GetField("_resolvers", BindingFlags.NonPublic | BindingFlags.Instance);

            // Replace .NET resolver with a Mono one
            ICompilationAssemblyResolver[] resolvers = resolversField.GetValue(defaultResolver) as ICompilationAssemblyResolver[];
            for (int i = 0; i < resolvers.Length; i++)
            {
                ReferenceAssemblyPathResolver referenceAssemblyPathResolver = resolvers[i] as ReferenceAssemblyPathResolver;

                if (referenceAssemblyPathResolver != null)
                    resolvers[i] = new MonoCompilationAssemblyResolver(referenceAssemblyPathResolver);
            }

            return hostBuilder;
        }
    }
}