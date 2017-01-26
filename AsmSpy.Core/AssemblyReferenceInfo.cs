using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AsmSpy.Core
{
    public class AssemblyReferenceInfo : IAssemblyReferenceInfo
    {
        #region Fields

        private readonly HashSet<IAssemblyReferenceInfo> _references = new HashSet<IAssemblyReferenceInfo>();
        private readonly HashSet<IAssemblyReferenceInfo> _referencedBy = new HashSet<IAssemblyReferenceInfo>();

        #endregion

        #region Properties

        public virtual Assembly ReflectionOnlyAssembly { get; set; }
        public virtual AssemblySource AssemblySource { get; set; }
        public virtual AssemblyName AssemblyName { get; }
        public virtual ICollection<IAssemblyReferenceInfo> ReferencedBy => _referencedBy.ToArray();
        public virtual ICollection<IAssemblyReferenceInfo> References => _references.ToArray();

        #endregion

        #region Constructor

        public AssemblyReferenceInfo(AssemblyName assemblyName)
        {
            AssemblyName = assemblyName;
        }

        #endregion

        #region Reference Support

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

        #endregion

        #region HashCode Support

        public override int GetHashCode()
        {
            return AssemblyName.FullName.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var info = obj as IAssemblyReferenceInfo;
            if (info == null)
            {
                return false;
            }
            return info.AssemblyName.FullName == AssemblyName.FullName;
        }

        #endregion
    }
}
