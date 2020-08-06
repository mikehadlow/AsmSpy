using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AsmSpy.Core
{
    public class AssemblyReferenceInfo : IAssemblyReferenceInfo
    {
        private readonly HashSet<IAssemblyReferenceInfo> _references = new HashSet<IAssemblyReferenceInfo>();
        private readonly HashSet<IAssemblyReferenceInfo> _referencedBy = new HashSet<IAssemblyReferenceInfo>();

        public virtual Assembly ReflectionOnlyAssembly { get; set; }
        public virtual AssemblySource AssemblySource { get; set; }
        public virtual AssemblyName AssemblyName { get; }
        public virtual AssemblyName RedirectedAssemblyName { get; }
        public virtual ICollection<IAssemblyReferenceInfo> ReferencedBy => _referencedBy.ToArray();
        public virtual ICollection<IAssemblyReferenceInfo> References => _references.ToArray();
        public bool IsSystem => AssemblyInformationProvider.IsSystemAssembly(AssemblyName);
        public string FileName { get; }
        public bool ReferencedByRoot { get; set; } = false;
        public AssemblyReferenceInfo AlternativeFoundVersion { get; private set; }
        public bool HasAlternativeVersion => AlternativeFoundVersion != null;

        public AssemblyReferenceInfo(AssemblyName assemblyName, AssemblyName redirectedAssemblyName, string fileName = "")
        {
            AssemblyName = assemblyName 
                ?? throw new ArgumentNullException(nameof(assemblyName));
            RedirectedAssemblyName = redirectedAssemblyName 
                ?? throw new ArgumentNullException(nameof(redirectedAssemblyName));
            FileName = fileName 
                ?? throw new ArgumentNullException(nameof(fileName));
        }

        public virtual void AddReference(IAssemblyReferenceInfo info)
        {
            if (!_references.Contains(info))
            {
                _references.Add(info);
            }
        }

        public virtual void AddReferencedBy(IAssemblyReferenceInfo info)
        {
            if (!_referencedBy.Contains(info))
            {
                _referencedBy.Add(info);
            }
        }

        public void SetAlternativeFoundVersion(AssemblyReferenceInfo alternativeVersion)
        {
            if(alternativeVersion.AssemblyName.Name != AssemblyName.Name)
            {
                throw new InvalidOperationException(
                    $"Alternative version to {AssemblyName.Name}, must have the same Name, but is {alternativeVersion.AssemblyName.Name}");
            }
            if(ReflectionOnlyAssembly != null)
            {
                throw new InvalidOperationException(
                    $"AssemblyReferenceInfo for {AssemblyName.Name} has a ReflectionOnlyAssembly, so an alternative should not be set.");
            }
            if(AssemblySource != AssemblySource.NotFound)
            {
                throw new InvalidOperationException(
                    $"AssemblyReferenceInfo.AssemblySource for {AssemblyName.Name} is not 'NotFound', so an alternative should not be set.");
            }
            AlternativeFoundVersion = alternativeVersion;
        }

        public override int GetHashCode()
        {
            return AssemblyName.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is IAssemblyReferenceInfo info)
            {
                return info.AssemblyName.FullName == AssemblyName.FullName;
            }

            return false;
        }
    }
}
