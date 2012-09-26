using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public enum Behaviour
    {
    }

    /// <summary>
    /// A Component is something that can be represented in the ViewSpace and
    /// is managed by the Core in terms of update and drawing.
    /// </summary>
    public abstract class Component
    {
        public abstract void update();

        /// <summary>
        /// List of sub-components
        /// </summary>
        List<Component> m_components = new List<Component>();
    }
}
