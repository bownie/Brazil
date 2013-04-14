using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Template for a 3D game
    /// </summary>
    public class Template3D : Template
    {
        public Template3D(string name, string path):base(TemplateType.ThreeD, name, path)
        {
            setDescription("Provides template for 3D game and experience development.");

            addFeature(new AppDevelopmentFeature());
            addFeature(new TextEditFeature());
            addFeature(new AndroidDeploymentFeature());

            // etc.
            //
            //addConfiguration()
        }
    }
}
