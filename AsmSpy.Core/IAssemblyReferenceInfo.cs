using System.Collections.Generic;
using System.Reflection;

namespace AsmSpy.Core
{
    public interface IAssemblyReferenceInfo
    {
        Assembly ReflectionOnlyAssembly { get; set; }
        AssemblySource AssemblySource { get; set; }
        AssemblyName AssemblyName { get; }
        ICollection<IAssemblyReferenceInfo> ReferencedBy { get; }
        ICollection<IAssemblyReferenceInfo> References { get; }
        void AddReference(IAssemblyReferenceInfo info);
        void AddReferencedBy(IAssemblyReferenceInfo info);
    }
}