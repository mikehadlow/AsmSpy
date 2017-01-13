using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AsmSpy
{
    public class AssemblyReferenceInfo
    {
        #region Fields

        private readonly HashSet<AssemblyReferenceInfo> _references = new HashSet<AssemblyReferenceInfo>();
        private readonly HashSet<AssemblyReferenceInfo> _referencedBy = new HashSet<AssemblyReferenceInfo>();

        #endregion

        #region Properties

        public Assembly ReflectionOnlyAssembly { get; set; }
        public AssemblySource AssemblySource { get; set; }
        public AssemblyName AssemblyName { get; }
        public ICollection<AssemblyReferenceInfo> ReferencedBy => _referencedBy.ToArray();
        public ICollection<AssemblyReferenceInfo> References => _references.ToArray();

        #endregion

        #region Constructor

        public AssemblyReferenceInfo(AssemblyName assemblyName)
        {
            AssemblyName = assemblyName;
        }

        #endregion

        #region Reference Support

        public void AddReference(AssemblyReferenceInfo info)
        {
            if (!_references.Contains(info))
            {
                _references.Add(info);
            }
        }

        public void AddReferencedBy(AssemblyReferenceInfo info)
        {
            if (!_referencedBy.Contains(info))
            {
                _referencedBy.Add(info);
            }
        }

        #endregion

        #region HashCode Support

        public override int GetHashCode()
        {
            return AssemblyName.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var info = obj as AssemblyReferenceInfo;
            if (info == null)
            {
                return false;
            }
            return info.AssemblyName.FullName == AssemblyName.FullName;
        }

        #endregion
    }
}
