using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Abstract base class for template
    /// </summary>
    public abstract class Template
    {
        public Template(TemplateType type, string name, string path)
        {
            m_name = name;
            m_path = path;
            m_type = type;
        }

        public string getName() { return m_name; }
        public string getDescription() { return m_description; }
        public void setDescription(string description) { m_description = description; }
        public string getPath() { return m_path; }

        /// <summary>
        /// Add some config to our template
        /// </summary>
        /// <param name="item"></param>
        /// <param name="defaultValue"></param>
        protected void addConfiguration(string item, string defaultValue)
        {
            m_configuration.Add(new Configuration(item, defaultValue));
        }

        /// <summary>
        /// Add a feature to the list
        /// </summary>
        /// <param name="feature"></param>
        protected void addFeature(Feature feature)
        {
            m_features.Add(feature);
        }

        /// <summary>
        /// Path to the template
        /// </summary>
        protected string m_path;

        /// <summary>
        /// Name of the template
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Description
        /// </summary>
        protected string m_description = "";

        /// <summary>
        /// Default configuration list for the project
        /// </summary>
        protected List<Configuration> m_configuration = new List<Configuration>();

        /// <summary>
        /// List of features this class implements
        /// </summary>
        protected List<Feature> m_features = new List<Feature>();

        /// <summary>
        /// Template type
        /// </summary>
        protected TemplateType m_type;
    }
}
