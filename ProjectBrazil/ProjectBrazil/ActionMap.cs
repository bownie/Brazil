using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// An ActionMap keeps track of the States, Actions and Targets for our application.
    /// These three concepts link together to define our user interaction.
    /// </summary>
    public class ActionMap
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ActionMap()
        {
        }

        /// <summary>
        /// Set a State and Action to a Target - returns false if already set
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool setAction(State state, Action action, Target target)
        {
            StateAction stateAction = new StateAction(state, action);

            if (m_actionDictionary.ContainsKey(stateAction))
            {
                return false;
            }

            // Ok add the target
            //
            m_actionDictionary.Add(stateAction, target);

            return true;
        }

        /// <summary>
        /// Set a StateActionList to a target - returns false if this is already set
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool setActionList(State state, List<Action> actions, Target target)
        {
            StateAction stateActions = new StateAction(state, actions);

            if (m_actionDictionary.ContainsKey(stateActions))
            {
                return false;
            }

            m_actionDictionary.Add(stateActions, target);

            return true;
        }

        /// <summary>
        /// Remove a State/Action from the action map
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool removeActionList(State state, Action action)
        {
            StateAction stateAction = new StateAction(state, action);

            return m_actionDictionary.Remove(stateAction);
        }

        /// <summary>
        /// Remove a State/Actionlist from the action map
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <returns></returns>
        public bool removeActionList(State state, List<Action> actions)
        {
            StateAction stateActions = new StateAction(state, actions);

            return m_actionDictionary.Remove(stateActions);
        }

        /// <summary>
        /// Filter the dictionary by state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Dictionary<StateAction, Target> getActionsForState(State state)
        {
            return m_actionDictionary.Where(item => item.Key.getState() == state).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Get a Target for a Key combination - only one Target is allowed for a combination.
        /// Note that this method ignores modifier keys as they should already be set in the 
        /// KeyAction itself.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public Target getTargetForKeys(State state, List<KeyAction> keys)
        {
            // Filter the StateActions by state
            //
            Dictionary<StateAction, Target> dict = getActionsForState(state);

            //  Check for key actions and return list
            //
            foreach (StateAction sA in dict.Keys)
            {
                // We only need to match keys once as we're looking up in a Dictionary that ensures
                // there's only one result.
                //
                if (sA.matchKeyActions(keys))
                {
                    return dict[sA];
                }
            }

            return Target.None;
        }

        /// <summary>
        /// The ActionMap is basically a Dictionary for each State and Action which
        /// points to a Target - or not.
        /// </summary>
        protected Dictionary<StateAction, Target> m_actionDictionary = new Dictionary<StateAction, Target>();

    }
}
