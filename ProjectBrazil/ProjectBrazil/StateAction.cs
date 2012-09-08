using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A StateAction is a combination of a State with an Action (or Actions in a List<>)
    /// and is used as the key to our ActionMap dictionary.  
    /// 
    /// </summary>
    public class StateAction// : IComparable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state"></param>
        /// <param name="action"></param>
        public StateAction(State state, Action action)
        {
            m_state = state;
            addAction(action);
        }

        /// <summary>
        /// Insert a list of actions to this StateAction
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        public StateAction(State state, List<Action> actions)
        {
            m_state = state;
            m_actionList.AddRange(actions);
        }

        /// <summary>
        /// Add an Action to the list
        /// </summary>
        /// <param name="action"></param>
        public void addAction(Action action)
        {
            m_actionList.Add(action);
        }

        /// <summary>
        /// Get the ActionList
        /// </summary>
        /// <returns></returns>
        public List<Action> getActions()
        {
            return m_actionList;
        }

        /// <summary>
        /// Get the State that applies to this combination
        /// </summary>
        /// <returns></returns>
        public State getState()
        {
            return m_state;
        }

        //public int CompareTo(Object obj)
        //{
            //StateAction sa = (StateAction)(obj);

            //return(sa.m_state == m_state && m_action == m
        //}

        /// <summary>
        /// The State
        /// </summary>
        protected State m_state;

        /// <summary>
        /// A list of actions we can associate with this state - Actions can 
        /// be Keys or Mouse actions and can have edge conditions associated
        /// with them.
        /// </summary>
        protected List<Action> m_actionList = new List<Action>();
    }
}
