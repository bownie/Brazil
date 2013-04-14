using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// A C++ template for Friendlier
    /// </summary>
    public class CppTemplate : Template
    {
        public CppTemplate(string name, string path)
            : base(TemplateType.Cpp, name, path)
        {
            setDescription("For C++ development with Qt libs and g++ under Windows");
            addFeature(new TextEditFeature());
        }
    }
}
