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

        public List<Target> getTargetsForKeys(State state, List<KeyAction> keys)
        {
            Dictionary<StateAction, Target> dict = getActionsForState(state);
            List<Target> rL = new List<Target>();

            foreach (StateAction sA in dict.Keys)
            {
                //if (sA.getActions().Count == keys.Count && Enumerable.

                //sA.getActions();
            //}

            //var DifferencesList = keys.Where(x => !sA.getActions().Any(x1 => x1.m_name == x.m_name))
            //.Union(ListB.Where(x => !ListA.Any(x1 => x1.id == x.id)));
            }

            return rL;
        }
        /// <summary>
        /// The ActionMap is basically a Dictionary for each State and Action which
        /// points to a Target - or not.
        /// </summary>
        protected Dictionary<StateAction, Target> m_actionDictionary = new Dictionary<StateAction, Target>();

    }
}
