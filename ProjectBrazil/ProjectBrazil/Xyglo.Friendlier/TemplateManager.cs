using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Friendlier
{
    public enum TemplateType
    {
        Cpp,
        Java,
        ThreeD
    } 

    /// <summary>
    /// Provides a centralised point for template management for Friendlier
    /// </summary>
    public class TemplateManager
    {
        public TemplateManager()
        {
            initialise();
        }

        protected void initialise()
        {
            m_templates.Add(new Template3D("3D Template", "templates/3D"));
            m_templates.Add(new CppTemplate("C++ Template", "template/cpp"));
        }

        /// <summary>
        /// Get a list of the available templates
        /// </summary>
        /// <returns></returns>
        public List<string> getTemplateList()
        {
            List<string> rL = new List<string>();

            foreach (Template template in m_templates)
            {
                string templateLabel = template.getName();
                if (template.getDescription() != "")
                    templateLabel += " - " + template.getDescription();

                rL.Add(templateLabel);
            }

            return rL;
        }

        /// <summary>
        /// Highlight
        /// </summary>
        /// <returns></returns>
        public int getHighlightIndex() { return m_highlighted; }
        public void setHighlightIndex(int index) { m_highlighted = index; }

        /// <summary>
        /// Used for incremnting index
        /// </summary>
        /// <returns></returns>
        public int getTotalTemplates() { return m_templates.Count(); }

        /// <summary>
        /// All of our templates
        /// </summary>
        List<Template> m_templates = new List<Template>();

        /// <summary>
        /// Highlight item in the list
        /// </summary>
        protected int m_highlighted = 0;
    }
}
