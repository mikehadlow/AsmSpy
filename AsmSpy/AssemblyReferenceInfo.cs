using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace AsmSpy
{
    public class AssemblyReferenceInfo
    {
        #region Fields

        HashSet<AssemblyReferenceInfo> _References = new HashSet<AssemblyReferenceInfo>();
        HashSet<AssemblyReferenceInfo> _ReferencedBy = new HashSet<AssemblyReferenceInfo>();

        #endregion

        #region Properties

        public AssemblyName AssemblyName { get; private set; }
        public AssemblyReferenceInfo[] ReferencedBy { get { return _ReferencedBy.ToArray(); } }
        public AssemblyReferenceInfo[] References { get { return _References.ToArray(); } }

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
            if (!_References.Contains(info))
            {
                _References.Add(info);
            }
        }

        public void AddReferencedBy(AssemblyReferenceInfo info)
        {
            if (!_ReferencedBy.Contains(info))
            {
                _ReferencedBy.Add(info);
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
