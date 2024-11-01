using Microsoft.CodeAnalysis;

namespace Cell.Core.Execution.CodeCompletion
{
    /// <summary>
    /// Capable of converting a Roslyn <see cref="ITypeSymbol"/> to a .NET reflection <see cref="Type"/> object.
    /// </summary>
    public static class TypeSymbolConverter
    {
        /// <summary>
        /// Gets the <see cref="Type"/> object from the given <see cref="ITypeSymbol"/>.
        /// </summary>
        /// <param name="typeSymbol">The symbol to look for the Type for.</param>
        /// <returns>A type if it is able to be found. Might also return null.</returns>
        /// <exception cref="InvalidOperationException">Raised when the Type is not able to be found.</exception>
        public static Type? GetTypeFromSymbol(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is IErrorTypeSymbol)
            {
                return null;
            }
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
            return Type.GetType(typeSymbol.ToDisplayString()) ?? throw new InvalidOperationException("Unable to convert type symbol.");
        }

        private static Type? GetArrayType(IArrayTypeSymbol arrayTypeSymbol)
        {
            var elementType = GetTypeFromSymbol(arrayTypeSymbol.ElementType);
            if (elementType is null) return null;
            return arrayTypeSymbol.Rank switch
            {
                1 => elementType.MakeArrayType(),
                _ => throw new NotSupportedException("Only single-dimensional arrays are supported.")
            };
        }

        private static Type? GetNamedType(INamedTypeSymbol namedTypeSymbol)
        {
            var typeArguments = namedTypeSymbol.TypeArguments.Select(GetTypeFromSymbol).OfType<Type>().ToArray() ?? [];
            if (typeArguments is null) return null;
            var typeName = namedTypeSymbol.ToDisplayString();
            typeName = TypeSymbolNameToReflectionTypeName(typeName);
            if (typeName == "void") return null;
            var type = Type.GetType(typeName) ?? throw new InvalidOperationException($"Unable to find named type '{typeName}'.");

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
            if (typeName == "string") return "System.String";
            if (typeName == "bool") return "System.Boolean";
            if (typeName == "int") return "System.Int32";
            if (typeName == "object") return "System.Object";
            return typeName;
        }
    }
}
