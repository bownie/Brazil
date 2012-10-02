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
    /// is managed by the Core in terms of update and drawing.  A Component has
    /// to exist within a State of the application.  
    /// </summary>
    public abstract class Component
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Component()
        {
        }

        /// <summary>
        /// Construct with a State in place
        /// </summary>
        /// <param name="state"></param>
        public Component(State state)
        {
            m_stateList.Add(state);
        }

        /// <summary>
        /// Add a State that this Component is active in
        /// </summary>
        /// <param name="state"></param>
        public void addState(State state)
        {
            m_stateList.Add(state);
        }

        /// <summary>
        /// Get the States this Component is active in
        /// </summary>
        /// <returns></returns>
        public HashSet<State> getStates()
        {
            return m_stateList;
        }

        //public abstract void update();

        /// <summary>
        /// List of sub-components
        /// </summary>
        List<Component> m_components = new List<Component>();

        /// <summary>
        /// A Component can be made available in zero or many States - a HashSet will ensure we
        /// don't get duplicates in this list.
        /// </summary>
        HashSet<State> m_stateList = new HashSet<State>();
    }
}
