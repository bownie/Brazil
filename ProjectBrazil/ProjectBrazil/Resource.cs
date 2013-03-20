using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The type of the Resource
    /// </summary>
    public enum ResourceType
    {
        Image,
        Video,
        Audio,
        Midi
    }

    /// <summary>
    /// A Resource is a file type that will be included as an Asset in the app package.
    /// 
    /// We can assign a resource to a State or to a Component and they may be re-used.
    /// 
    /// </summary>
    public class Resource
    {
        public Resource(ResourceType type, string name, string filePath)
        {
            m_type = type;
            m_name = name;
            m_filePath = filePath;
        }

        public string getName() { return m_name; }
        public string getFilePath() { return m_filePath; }
        public ResourceType getType() { return m_type; }

        /// <summary>
        /// Name for this resource which should be unique in a Resource collection
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Path to this resource
        /// </summary>
        protected string m_filePath;

        /// <summary>
        /// Type of this resource
        /// </summary>
        protected ResourceType m_type;
    }
}
