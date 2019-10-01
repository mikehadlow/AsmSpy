using System;
using System.Collections;
using System.Reflection;

namespace AsmSpy.Core
{
    public static class AssemblyInformationProvider
    {
        private static readonly byte[] SystemPublicKeyToken = new byte[] { 183, 122, 92, 86, 25, 52, 224, 137 };

        // some system assemblies are signed with this public key token, but some Microsoft non-system assemblies are signed with it too
        ////private static readonly byte[] MicrosoftPublicKeyToken = new byte[] { 49, 191, 56, 86, 173, 54, 78, 53 };

        /// <summary>
        /// Check whether the given assembly name is a system assembly or not.
        /// </summary>
        /// <remarks>
        /// This ISN'T a security check, and shouldn't be used for security decisions.
        /// Rather this relies on a heuristic and may be used to filter our system assemblies from the dependency graph.
        /// </remarks>
        /// <param name="assemblyName">The assembly name to check.</param>
        /// <returns>true if the assembly name (probably) is a system assembly; otherwise, false.</returns>
        public static bool IsSystemAssembly(AssemblyName assemblyName)
        {
            assemblyName = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));

            // fairly reliable and easy to check
            if (StructuralComparisons.StructuralEqualityComparer.Equals(assemblyName.GetPublicKeyToken(), SystemPublicKeyToken))
            {
                return true;
            }

            return assemblyName.Name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
                assemblyName.Name.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase) ||
                assemblyName.Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
                assemblyName.Name.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase);
        }
    }
}
