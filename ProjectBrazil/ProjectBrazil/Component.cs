using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

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
    [DataContract(Namespace = "http://www.xyglo.com", IsReference = true)]
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
        /// Add a StateAction combination that activates this Component
        /// </summary>
        /// <param name="stateAction"></param>
        public void addStateAction(StateAction stateAction)
        {
            m_stateActionList.Add(stateAction);
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
        /// Get the StateAction list
        /// </summary>
        /// <returns></returns>
        public HashSet<StateAction> getStateActions()
        {
            return m_stateActionList;
        }

        /// <summary>
        /// Is this class affected by gravity?
        /// </summary>
        /// <returns></returns>
        public bool isAffectedByGravity() { return m_gravityAffected; }
        public void setAffectedByGravity(bool gravity) { m_gravityAffected = gravity; }

        /// <summary>
        /// Can this Component be moved by other Components?
        /// </summary>
        /// <returns></returns>
        public bool isMoveable() { return m_moveable; }

        /// <summary>
        /// Set whether this Component can by moved by other Component interactions.
        /// </summary>
        /// <param name="moveable"></param>
        public void setMoveable(bool moveable) { m_moveable = moveable; } 

        /// <summary>
        /// Return the mass of this object
        /// </summary>
        /// <returns></returns>
        public float getMass() { return m_mass; }

        /// <summary>
        /// Get the hardness of this object
        /// </summary>
        /// <returns></returns>
        public float getHardness() { return m_hardness; }

        /// <summary>
        /// Set the hardness
        /// </summary>
        /// <param name="hardness"></param>
        public void setHardness(float hardness) { m_hardness = hardness; }

        /// <summary>
        /// Has substance if it has some hardness
        /// </summary>
        /// <returns></returns>
        public bool isCorporeal() { return (m_hardness > 0); }

        /// <summary>
        /// Get the Component behaviour
        /// </summary>
        /// <returns></returns>
        public Behaviour getBehaviour() { return m_behaviour; }

        /// Get the name
        /// </summary>
        /// <returns></returns>
        public string getName() { return m_name; }

        /// <summary>
        /// Set the name
        /// </summary>
        /// <param name="name"></param>
        public void setName(string name) { m_name = name; }

        /// <summary>
        /// Has this Component been destroyed on the drawing side?  If so don't recreate it.
        /// </summary>
        /// <returns></returns>
        public bool isDestroyed() { return m_isDestroyed; }

        /// <summary>
        /// Set this Component to destroyed so it's not recreated drawing side
        /// </summary>
        /// <param name="isDestroyed"></param>
        public void setDestroyed(bool isDestroyed) { m_isDestroyed = isDestroyed; }

        /// <summary>
        /// Is this component hiding?
        /// </summary>
        /// <returns></returns>
        public bool isHiding() { return m_isHiding; }

        /// <summary>
        /// Set this component to hiding/unhiding
        /// </summary>
        /// <param name="hiding"></param>
        public void setHiding(bool hiding) { m_isHiding = hiding; }

        /// <summary>
        /// Set an App container for this Component
        /// </summary>
        /// <param name="container"></param>
        public void setApp(BrazilApp app) { m_app = app; }

        /// <summary>
        /// Get the container for this Component
        /// </summary>
        public BrazilApp getApp() { return m_app; }

        /// <summary>
        /// Affected by gravity?
        /// </summary>
        [DataMember]
        protected bool m_gravityAffected = false;

        /// <summary>
        /// Mass of this object
        /// </summary>
        [DataMember]
        protected float m_mass = 0;

        /// <summary>
        /// Hardness of this object
        /// </summary>
        [DataMember]
        protected float m_hardness = 0;

        /// <summary>
        /// Can this Component be affected by interactions with other Components?
        /// </summary>
        [DataMember]
        protected bool m_moveable = true;
        
        /// <summary>
        /// Default behaviour is none - but could be a goody or a baddy
        /// </summary>
        [DataMember]
        protected Behaviour m_behaviour = Behaviour.None;

        /// <summary>
        /// List of sub-components
        /// </summary>
        [DataMember]
        List<Component> m_components = new List<Component>();

        /// <summary>
        /// A Component can be made available in zero or many States - a HashSet will ensure we
        /// don't get duplicates in this list.
        /// </summary>
        [DataMember]
        HashSet<State> m_stateList = new HashSet<State>();

        /// <summary>
        /// A Component can also be made available in a combination of State and Action - this is
        /// a finer grained context
        /// </summary>
        [DataMember]
        HashSet<StateAction> m_stateActionList = new HashSet<StateAction>();

        /// <summary>
        /// Name for this component just in case we want to track it easily for some insane reason
        /// </summary>
        [DataMember]
        protected string m_name;

        /// <summary>
        /// Is this Component destroyed in GameLand?  If so don't regenerate it
        /// </summary>
        [DataMember]
        protected bool m_isDestroyed = false;

        /// <summary>
        /// Is this Component hiding?
        /// </summary>
        [DataMember]
        protected bool m_isHiding = false;

        /// <summary>
        /// Any BrazilComponent can be within a BrazilApp
        /// </summary>
        [NonSerialized]
        protected BrazilApp m_app = null;
    }
}
