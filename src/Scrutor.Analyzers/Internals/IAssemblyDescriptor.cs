using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Scrutor.Analyzers.Internals
{
    interface IAssemblyDescriptor
    {
    }

    struct AssemblyDescriptor : IAssemblyDescriptor
    {
        public IAssemblySymbol AssemblySymbol { get; }

        public AssemblyDescriptor(IAssemblySymbol assemblySymbol)
        {
            AssemblySymbol = assemblySymbol;
        }
        public override string ToString() => "All";
    }

    struct AllAssemblyDescriptor : IAssemblyDescriptor
    {
        public override string ToString() => "All";
    }
    interface ICompiledAssemblyDescriptor : IAssemblyDescriptor
    {
        INamedTypeSymbol TypeFromAssembly { get; }
    }
    struct CompiledAssemblyDescriptor : ICompiledAssemblyDescriptor
    {
        public INamedTypeSymbol TypeFromAssembly { get; }
        public CompiledAssemblyDescriptor(INamedTypeSymbol typeFromAssembly) => TypeFromAssembly = typeFromAssembly;
        public override string ToString() => Helpers.GetFullMetadataName(TypeFromAssembly);
    }
    struct CompiledAssemblyDependenciesDescriptor : ICompiledAssemblyDescriptor
    {
        public INamedTypeSymbol TypeFromAssembly { get; }
        public CompiledAssemblyDependenciesDescriptor(INamedTypeSymbol typeFromAssembly) => TypeFromAssembly = typeFromAssembly;
        public override string ToString() => Helpers.GetFullMetadataName(TypeFromAssembly);
    }
}