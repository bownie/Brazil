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
        /// Construct by a mouse action
        /// </summary>
        /// <param name="state"></param>
        /// <param name="mouse"></param>
        public StateAction(State state, Mouse mouse)
        {
            m_state = state;
            addAction(new MouseAction(mouse));
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

        /// <summary>
        /// Can we match all actions with this StateAction?   We pass in a list
        /// of KeyActions and convert our internal ActionList into KeyActions and
        /// compare them.  There is probably a cleaner way of doing this but this is
        /// at least clear.
        /// 
        /// At this point we ignore any modifier keys as they should already have modified
        /// the Keys that are passed in.
        /// 
        /// </summary>
        /// <param name="actions"></param>
        /// <returns></returns>
        public bool matchKeyActions(List<KeyAction> keyActions)
        {
            foreach (KeyAction keyAction in keyActions)
            {
                // skip the modifier keys
                //
                if (keyAction.m_key == Keys.LeftAlt ||
                    keyAction.m_key == Keys.RightAlt ||
                    keyAction.m_key == Keys.LeftControl ||
                    keyAction.m_key == Keys.RightControl ||
                    keyAction.m_key == Keys.LeftShift ||
                    keyAction.m_key == Keys.RightShift ||
                    keyAction.m_key == Keys.LeftWindows ||
                    keyAction.m_key == Keys.RightWindows)
                    continue;

                bool match = true;

                foreach (Action action in m_actionList)
                {
                    if (action.GetType() == typeof(KeyAction))
                    {
                        KeyAction testKeyAction = (KeyAction)action;

                        if (testKeyAction != keyAction)
                        {
                            match = false;
                            break;
                        }
                    }
                }

                // If we're still matching here then we can return true
                //
                if (match)
                {
                    return true;
                }
            }

            // Default is not matched
            return false;
        }

        /// <summary>
        /// Match all the actions of a single keyAction
        /// </summary>
        /// <param name="keyAction"></param>
        /// <returns></returns>
        public bool matchKeyAction(KeyAction keyAction)
        {
            bool match = true;
            foreach (Action action in m_actionList)
            {
                if (action.GetType() == typeof(KeyAction))
                {
                    KeyAction testKeyAction = (KeyAction)action;

                    if (testKeyAction != keyAction)
                    {
                        match = false;
                        break;
                    }
                }
            }

            return match;
        }

        /// <summary>
        /// Match all the actions on a single mouseAction
        /// </summary>
        /// <param name="mouseAction"></param>
        /// <returns></returns>
        public bool matchMouseAction(MouseAction mouseAction)
        {
            bool match = true;
            foreach (Action action in m_actionList)
            {
                if (action.GetType() == typeof(MouseAction))
                {
                    MouseAction testKeyAction = (MouseAction)action;

                    if (testKeyAction != mouseAction)
                    {
                        match = false;
                        break;
                    }
                }
            }

            return match;

        }

        /*
        public int CompareTo(Object obj)
        {
            StateAction sA = (StateAction)(obj);

            return(sA.m_state == m_state && sA.m_actionList.Union(m_actionList)
        }

        public bool compareActionsLists(List<Action> listA, List<Action> listB)
        {
            List<Action> tempListA = listA.;
            List<Action> tempListB = listB.Sort();


        }*/

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
