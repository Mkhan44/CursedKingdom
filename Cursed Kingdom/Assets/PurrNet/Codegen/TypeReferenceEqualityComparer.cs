using System.Collections.Generic;
using Mono.Cecil;

namespace PurrNet.Codegen
{
    public class TypeReferenceEqualityComparer : IEqualityComparer<TypeReference>
    {
        private TypeReferenceEqualityComparer()
        {
        }

        public static IEqualityComparer<TypeReference> Default { get; } = new TypeReferenceEqualityComparer();

        public bool Equals(TypeReference x, TypeReference y)
        {
            return GetKey(x) == GetKey(y);
        }

        public int GetHashCode(TypeReference obj)
        {
            return GetKey(obj)?.GetHashCode() ?? 0;
        }

        private static string GetKey(TypeReference obj)
        {
            if (obj == null)
                return null;

            return $"{obj.GetType().FullName}|{GetAssemblyName(obj.Scope)}|{obj.FullName}";
        }

        private static string GetAssemblyName(IMetadataScope scope)
        {
            if (scope == null)
                return null;

            if (scope is ModuleDefinition md)
            {
                return md.Assembly.FullName;
            }

            return scope.ToString();
        }
    }
}
