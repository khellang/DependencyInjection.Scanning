﻿using Microsoft.CodeAnalysis;

namespace Scrutor.Analyzers
{
    internal static class Diagnostics
    {
        private const string Category = "Scrutor";

        public static DiagnosticDescriptor MustBeAnExpression { get; } = new DiagnosticDescriptor(
            "SCTR0001",
            "Must be a expression",
            "Methods that will be analyzed statically must be an expression, blocks and variables are not allowed",
            Category,
            DiagnosticSeverity.Error,
            true
        );

        public static DiagnosticDescriptor MustBeTypeOf { get; } = new DiagnosticDescriptor(
            "SCTR0002",
            "Must use typeof",
            "Method must be called with typeof, variables are not allowed",
            Category,
            DiagnosticSeverity.Error,
            true
        );

        public static DiagnosticDescriptor TypeNotResolved { get; } = new DiagnosticDescriptor(
            "SCTR0003",
            "Type could not be resolved",
            "The indicated type could not be resolved",
            Category,
            DiagnosticSeverity.Warning,
            true
        );

        public static DiagnosticDescriptor NamespaceMustBeAString { get; } = new DiagnosticDescriptor(
            "SCTR0003",
            "Namespace must be a string",
            "The given namespace must be a constant string",
            Category,
            DiagnosticSeverity.Warning,
            true
        );
    }
}