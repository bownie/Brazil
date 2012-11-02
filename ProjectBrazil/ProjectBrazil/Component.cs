using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public enum Behaviour
    {
        None,
        Goody, // a prize, not a person
        Baddy  // a person, not a price
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

        /// <summary>
        /// Is this class affected by gravity?
        /// </summary>
        /// <returns></returns>
        public bool isAffectedByGravity()
        {
            return m_gravityAffected;
        }

        /// <summary>
        /// Return the mass of this object
        /// </summary>
        /// <returns></returns>
        public double getMass()
        {
            return m_mass;
        }

        /// <summary>
        /// Get the hardness of this object
        /// </summary>
        /// <returns></returns>
        public double getHardness()
        {
            return m_hardness;
        }

        /// <summary>
        /// Set the hardness
        /// </summary>
        /// <param name="hardness"></param>
        public void setHardness(double hardness)
        {
            m_hardness = hardness;
        }

        /// <summary>
        /// Has substance if it has some hardness
        /// </summary>
        /// <returns></returns>
        public bool isCorporeal()
        {
            return (m_hardness > 0);
        }

        /// <summary>
        /// Get the Component behaviour
        /// </summary>
        /// <returns></returns>
        public Behaviour getBehaviour()
        {
            return m_behaviour;
        }

        /// Get the name
        /// </summary>
        /// <returns></returns>
        public string getName()
        {
            return m_name;
        }

        /// <summary>
        /// Set the name
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name)
        {
            m_name = name;
        }

        /// <summary>
        /// Has this Component been destroyed on the drawing side?  If so don't recreate it.
        /// </summary>
        /// <returns></returns>
        public bool isDestroyed()
        {
            return m_isDestroyed;
        }

        /// <summary>
        /// Set this Component to destroyed so it's not recreated drawing side
        /// </summary>
        /// <param name="isDestroyed"></param>
        public void setDestroyed(bool isDestroyed)
        {
            m_isDestroyed = isDestroyed;
        }

        /// <summary>
        /// Affected by gravity?
        /// </summary>
        protected bool m_gravityAffected = false;

        /// <summary>
        /// Mass of this object
        /// </summary>
        protected double m_mass = 0;

        /// <summary>
        /// Hardness of this object
        /// </summary>
        protected double m_hardness = 0;
        
        /// <summary>
        /// Default behaviour is none - but could be a goody or a baddy
        /// </summary>
        protected Behaviour m_behaviour = Behaviour.None;

        /// <summary>
        /// List of sub-components
        /// </summary>
        List<Component> m_components = new List<Component>();

        /// <summary>
        /// A Component can be made available in zero or many States - a HashSet will ensure we
        /// don't get duplicates in this list.
        /// </summary>
        HashSet<State> m_stateList = new HashSet<State>();

        /// <summary>
        /// Name for this component just in case we want to track it easily for some insane reason
        /// </summary>
        protected string m_name;

        /// <summary>
        /// Is this Component destroyed in GameLand?  If so don't regenerate it
        /// </summary>
        protected bool m_isDestroyed = false;
    }
}
