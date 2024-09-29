using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Cell.Core.Execution.CodeCompletion
{
    public static class TypeSymbolConverter
    {
        public static Type GetTypeFromSymbol(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));

            // Handle special cases
            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                return GetNamedType(namedTypeSymbol);
            }
            else if (typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
            {
                return GetArrayType(arrayTypeSymbol);
            }
            else if (typeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                throw new InvalidOperationException($"Cannot convert type parameter: {typeParameterSymbol.Name}");
            }

            // Fallback for other types
            return Type.GetType(typeSymbol.ToDisplayString()) ?? throw new InvalidOperationException("Unable to convert type symbol.");
        }

        private static Type GetNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            var typeArguments = namedTypeSymbol.TypeArguments.Select(GetTypeFromSymbol).ToArray();
            var typeName = namedTypeSymbol.ToDisplayString();
            typeName = TypeSymbolNameToReflectionTypeName(typeName);
            var type = Type.GetType(typeName) ?? throw new InvalidOperationException("Unable to find named type.");

            return type.IsGenericType ? type.MakeGenericType(typeArguments) : type;
        }

        private static string TypeSymbolNameToReflectionTypeName(string typeName)
        {
            if (typeName.Contains('<'))
            {
                var split = typeName.Split('<');
                var genericTypeName = split[0];
                var genericTypeArguments = split[1].TrimEnd('>').Split(',');
                var genericTypeArgumentsCount = genericTypeArguments.Length;
                return $"{genericTypeName}`{genericTypeArgumentsCount}";
            }
            return typeName;
        }

        private static Type GetArrayType(IArrayTypeSymbol arrayTypeSymbol)
        {
            var elementType = GetTypeFromSymbol(arrayTypeSymbol.ElementType);
            return arrayTypeSymbol.Rank switch
            {
                1 => elementType.MakeArrayType(),
                _ => throw new NotSupportedException("Only single-dimensional arrays are supported.")
            };
        }

        private static string GetFullTypeName(INamedTypeSymbol namedTypeSymbol)
        {
            var typeName = namedTypeSymbol.ToDisplayString();
            var assemblyName = namedTypeSymbol.ContainingAssembly.Name;

            return $"{typeName}, {assemblyName}";
        }
    }
}
