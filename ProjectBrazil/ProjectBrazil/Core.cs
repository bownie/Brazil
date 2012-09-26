using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Xyglo.Brazil
{
    // Deprecated by BrazilApp
    //
    // DO NOT USE
    //

    /// <summary>
    /// A Core is inherited by the main engine of the implementing technology and
    /// interfaces the abstract ViewSpace world (our Xyglo interface) to the underlying
    /// Components.
    /// 
    /// The Components live within the core and are connected here.
    /// 
    /// </summary>
    public abstract class Core
    {
        /// <summary>
        /// Delete a component by position
        /// </summary>
        /// <param name="componentId"></param>
        public abstract void deleteComponent(int componentId);

        /// <summary>
        /// Add a Component and get an id back according to position
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public abstract int addComponent(Component component);

        /// <summary>
        /// Create a ComponentInstance in the runtime model
        /// </summary>
        /// <param name="componentId"></param>
        /// <returns></returns>
        public abstract int createInstance(Component component);


        /// <summary>
        /// Connect a Key to a Target in a State
        /// </summary>
        /// <param name="state"></param>
        /// <param name="key"></param>
        /// <param name="target"></param>
        public void connectKey(State state, Keys key, Target target = Target.Default)
        {
            m_actionMap.setAction(state, new KeyAction(key), target);
        }

        /// <summary>
        /// Connect up States, generic Action and Targets
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, Action action, Target target = Target.Default)
        {
            m_actionMap.setAction(state, action, target);
        }

        /// <summary>
        /// We use connect to define states, actions and targets for those actions
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, List<Action> actions, Target target = Target.Default)
        {
            // Roll through all the actions 
            foreach (Action action in actions)
            {
                m_actionMap.setAction(state, action, target);
            }
        }

        /// <summary>
        /// Connect all the normal editor keys to a target - if the target is
        /// not specified then we assume the default target is the focus object.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        public void connectEditorKeys(State state, Target target = Target.Default)
        {
            if (target == Target.Default)
            {
                // do something with the default target
            }

            //m_actionMap.setAction(state, 
        }

        /// <summary>
        /// ComponentInstances are the running instances of Components in our Core
        /// </summary>
        protected List<ComponentInstance> m_componentInstances = new List<ComponentInstance>();

        /// <summary>
        /// The Components in our Core
        /// </summary>
        protected List<Component> m_components = new List<Component>();

        /// <summary>
        /// Action map
        /// </summary>
        protected ActionMap m_actionMap = new ActionMap();
    }
}
