﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace Scrutor
{
    public class TypeSourceSelector : ITypeSourceSelector, ISelector
    {
        private List<ISelector> Selectors { get; } = new List<ISelector>();

        /// <inheritdoc />
        public IImplementationTypeSelector FromAssemblyOf<T>()
        {
            return InternalFromAssembliesOf(new[] { typeof(T).GetTypeInfo() });
        }

        public IImplementationTypeSelector FromCallingAssembly()
        {
            return FromAssemblies(Assembly.GetCallingAssembly());
        }

        public IImplementationTypeSelector FromExecutingAssembly()
        {
            return FromAssemblies(Assembly.GetExecutingAssembly());
        }

        public IImplementationTypeSelector FromEntryAssembly()
        {
            return FromAssemblies(Assembly.GetEntryAssembly()!);
        }

        public IImplementationTypeSelector FromApplicationDependencies()
        {
            return FromApplicationDependencies(_ => true);
        }

        public IImplementationTypeSelector FromApplicationDependencies(Func<Assembly, bool> predicate)
        {
            try
            {
                return FromDependencyContext(DependencyContext.Default, predicate);
            }
            catch
            {
                // Something went wrong when loading the DependencyContext, fall
                // back to loading all referenced assemblies of the entry assembly...
                return FromAssemblyDependencies(Assembly.GetEntryAssembly()!);
            }
        }

        public IImplementationTypeSelector FromDependencyContext(DependencyContext context)
        {
            return FromDependencyContext(context, _ => true);
        }

        public IImplementationTypeSelector FromDependencyContext(DependencyContext context, Func<Assembly, bool> predicate)
        {
            Preconditions.NotNull(context, nameof(context));
            Preconditions.NotNull(predicate, nameof(predicate));

            var assemblyNames = context.RuntimeLibraries
                .SelectMany(library => library.GetDefaultAssemblyNames(context));

            var assemblies = LoadAssemblies(assemblyNames);

            return InternalFromAssemblies(assemblies.Where(predicate));
        }

        public IImplementationTypeSelector FromAssemblyDependencies(Assembly assembly)
        {
            Preconditions.NotNull(assembly, nameof(assembly));

            var assemblies = new List<Assembly> { assembly };

            try
            {
                var dependencyNames = assembly.GetReferencedAssemblies();

                assemblies.AddRange(LoadAssemblies(dependencyNames));

                return InternalFromAssemblies(assemblies);
            }
            catch
            {
                return InternalFromAssemblies(assemblies);
            }
        }

        public IImplementationTypeSelector FromAssembliesOf(params Type[] types)
        {
            Preconditions.NotNull(types, nameof(types));

            return InternalFromAssembliesOf(types.Select(x => x.GetTypeInfo()));
        }

        public IImplementationTypeSelector FromAssembliesOf(IEnumerable<Type> types)
        {
            Preconditions.NotNull(types, nameof(types));

            return InternalFromAssembliesOf(types.Select(t => t.GetTypeInfo()));
        }

        public IImplementationTypeSelector FromAssemblies(params Assembly[] assemblies)
        {
            Preconditions.NotNull(assemblies, nameof(assemblies));

            return InternalFromAssemblies(assemblies);
        }

        public IImplementationTypeSelector FromAssemblies(IEnumerable<Assembly> assemblies)
        {
            Preconditions.NotNull(assemblies, nameof(assemblies));

            return InternalFromAssemblies(assemblies);
        }

        public IServiceTypeSelector AddTypes(params Type[] types)
        {
            Preconditions.NotNull(types, nameof(types));

            var selector = new ImplementationTypeSelector(this, types);

            Selectors.Add(selector);

            return selector.AddClasses();
        }

        public IServiceTypeSelector AddTypes(IEnumerable<Type> types)
        {
            Preconditions.NotNull(types, nameof(types));

            var selector = new ImplementationTypeSelector(this, types);

            Selectors.Add(selector);

            return selector.AddClasses();
        }

        public void Populate(IServiceCollection services, RegistrationStrategy registrationStrategy)
        {
            foreach (var selector in Selectors)
            {
                selector.Populate(services, registrationStrategy);
            }
        }

        private IImplementationTypeSelector InternalFromAssembliesOf(IEnumerable<TypeInfo> typeInfos)
        {
            return InternalFromAssemblies(typeInfos.Select(t => t.Assembly));
        }

        private IImplementationTypeSelector InternalFromAssemblies(IEnumerable<Assembly> assemblies)
        {
            return AddSelector(assemblies.SelectMany(asm => asm.DefinedTypes).Select(x => x.AsType()));
        }

        private static IEnumerable<Assembly> LoadAssemblies(IEnumerable<AssemblyName> assemblyNames)
        {
            var assemblies = new List<Assembly>();

            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    // Try to load the referenced assembly...
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch
                {
                    // Failed to load assembly. Skip it.
                }
            }

            return assemblies;
        }

        private IImplementationTypeSelector AddSelector(IEnumerable<Type> types)
        {
            var selector = new ImplementationTypeSelector(this, types);

            Selectors.Add(selector);

            return selector;
        }
    }
}
