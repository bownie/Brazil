using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Something that Friendlier offers as a paid extra
    /// </summary>
    public class Feature
    {
        public Feature(string name, string description)
        {
            m_name = name;
            m_description = description;
            m_validFrom = System.DateTime.Now;
            m_validUntil = System.DateTime.Now;
        }

        /// <summary>
        /// Feature name
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Feature description
        /// </summary>
        protected string m_description;

        /// <summary>
        /// Validity date
        /// </summary>
        DateTime m_validFrom;

        /// <summary>
        /// Validity date
        /// </summary>
        DateTime m_validUntil;
    }


    /// <summary>
    /// It's a text editor
    /// </summary>
    public class TextEditFeature : Feature
    {
        public TextEditFeature()
            : base("TextEdit", "Enabled text editing in Friendlier")
        {
        }
    }

    public class AppDevelopmentFeature : Feature
    {
        public AppDevelopmentFeature()
            : base("GameDevelop", "Enables development of apps in Friendlier")
        {
        }
    }

    public class AndroidDeploymentFeature : Feature
    {
        public AndroidDeploymentFeature()
            : base("AndroidDeploy", "Enables deployment of apps to android")
        {
        }
    }
}
