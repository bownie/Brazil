using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public enum BrazilAppMode
    {
        App,     // normal windowed or app mode running standalone
        Hosted,  // hosted within another app and showing interface to that app
        Shared   // shared mode enabling multiple connections/server mode
    };

    /// <summary>
    /// A BrazilApp has to inherit and implement this class.  This is the proper - outside world view
    /// of our application.  At this class we create and hold various other components which we
    /// need to pass into the implementation of this world.  We use the ViewSpace as the actual
    /// interface into the graphics and user interface interaction layer.
    /// 
    /// Note that this namespace is uncontaminated by Xyglo.Brazil.Xna as it should stay that way.
    /// 
    /// </summary>
    public abstract class BrazilApp : IWorld
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilApp(BrazilAppMode appMode = BrazilAppMode.App)
        {
            m_mode = appMode;
            m_viewSpace.initialise(m_actionMap, m_componentList, m_world, m_states, m_targets);
        }

        /// <summary>
        /// Run this app
        /// </summary>
        public void run()
        {
            m_viewSpace.run();
        }

        /// <summary>
        /// Initialise abstract
        /// </summary>
        public abstract void initialise(State state);

        /// <summary>
        /// Connect a key by named state and target
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="key"></param>
        /// <param name="targetName"></param>
        public void connectKey(string stateName, Keys key, string targetName = "")
        {
            Target target;
            if (targetName == "")
            {
                target = null;
            }
            else
            {
                target = getTarget(targetName);
            }

            // Get state
            //
            State state = getState(stateName);

            connectKey(state, key, target);
        }


        /// <summary>
        /// Connect a Key to a Target in a State
        /// </summary>
        /// <param name="state"></param>
        /// <param name="key"></param>
        /// <param name="target"></param>
        public void connectKey(State state, Keys key, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            m_actionMap.setAction(state, new KeyAction(key), target);
        }

        /// <summary>
        /// Connect a Key to a Target - ensure we specify a Button State for that key too.
        /// Connecting by string state and target.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="key"></param>
        /// <param name="buttonState"></param>
        /// <param name="targetName"></param>
        public void connectKey(string stateName, Keys key, KeyButtonState buttonState, string targetName = "")
        {
            Target target;
            if (targetName == "")
            {
                target = null;
            }
            else
            {
                target = getTarget(targetName);
            }

            // Get state
            //
            State state = getState(stateName);

            connectKey(state, key, buttonState, target);
        }

        /// <summary>
        /// Connect a Key to a Target - ensure we specify a Button State for that key too
        /// </summary>
        /// <param name="state"></param>
        /// <param name="key"></param>
        /// <param name="buttonState"></param>
        /// <param name="target"></param>
        public void connectKey(State state, Keys key, KeyButtonState buttonState, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            m_actionMap.setAction(state, new KeyAction(key, buttonState), target);
        }

        /// <summary>
        /// Connect up by strings - for convenience
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="action"></param>
        /// <param name="targetName"></param>
        public void connect(string stateName, Keys key, string targetName = "")
        {
            connect(stateName, new KeyAction(key), targetName);
        }

        /// <summary>
        /// Connect up by strings and Actions
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="action"></param>
        /// <param name="targetName"></param>
        public void connect(string stateName, Action action, string targetName = "")
        {
            Target target;
            if (targetName == "")
            {
                target = new Target(); // default
            }
            else
            {
                target = getTarget(targetName);
            }

            // Get state
            //
            State state = getState(stateName);

            // Check for valid State and Target - should always pass
            //
            checkState(state);
            checkTarget(target);

            m_actionMap.setAction(state, action, target);
        }

        /// <summary>
        /// Connect up States, generic Action and Targets
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, Action action, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            m_actionMap.setAction(state, action, target);
        }

        /// <summary>
        /// We use connect to define states, actions and targets for those actions
        /// </summary>
        /// <param name="state"></param>
        /// <param name="actions"></param>
        /// <param name="target"></param>
        public void connect(State state, List<Action> actions, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            // Roll through all the actions 
            foreach (Action action in actions)
            {
                m_actionMap.setAction(state, action, target);
            }
        }

        /// <summary>
        /// Connect EditorKeys by name
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="targetName"></param>
        public void connectEditorKeys(string stateName, string targetName = "")
        {
            Target target;
            if (targetName == "")
            {
                target = null;
            }
            else
            {
                target = getTarget(targetName);
            }

            // Get state
            //
            State state = getState(stateName);

            connectEditorKeys(state, target);
        }

        /// <summary>
        /// Connect all the "normal"editor keys to a target - if the target is
        /// not specified then we assume the default target is the focus object.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        public void connectEditorKeys(State state, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            // Alphas
            //
            Keys[] alphaKeys = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
                            Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U,
                            Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z };
            foreach (Keys key in alphaKeys)
            {
                // Connect key
                //
                connectKey(state, key, target);

                // Connect shifted key
                //
                connect(state, new KeyAction(key, KeyboardModifier.Shift), target);

                // Held for upper and lower case
                //
                connect(state, new KeyAction(key, KeyButtonState.Held), target);
                connect(state, new KeyAction(key, KeyButtonState.Held, KeyboardModifier.Shift), target);
            }

            // Numbers
            //
            Keys[] numKeys = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            foreach (Keys key in numKeys)
            {
                connectKey(state, key, target);

                // Connect shifted key
                //
                connect(state, new KeyAction(key, KeyboardModifier.Shift), target);

                // Held for upper and lower case
                //
                connect(state, new KeyAction(key, KeyButtonState.Held), target);
                connect(state, new KeyAction(key, KeyButtonState.Held, KeyboardModifier.Shift), target);
            }

            // Power keys - like Escape
            //
            Keys[] powerKeys = { Keys.Escape };
            foreach (Keys key in powerKeys)
            {
                connectKey(state, key, target);
            }

            // Other keys
            //
            Keys[] otherKeys = { Keys.Space, Keys.OemComma, Keys.OemPeriod, Keys.OemQuotes, Keys.OemCloseBrackets, Keys.OemOpenBrackets, Keys.OemPipe, Keys.OemMinus, Keys.OemPlus, Keys.OemQuestion, Keys.Back, Keys.Delete, Keys.Decimal, Keys.OemBackslash, Keys.LeftWindows, Keys.RightWindows, Keys.LeftControl, Keys.RightControl, Keys.RightShift, Keys.LeftShift, Keys.LeftShift, Keys.RightAlt, Keys.LeftAlt, Keys.Tab };
            foreach (Keys key in otherKeys)
            {
                connectKey(state, key, target);

                // Connect shifted key
                //
                connect(state, new KeyAction(key, KeyboardModifier.Shift), target);

                // Held for upper and lower case
                //
                connect(state, new KeyAction(key, KeyButtonState.Held), target);
                connect(state, new KeyAction(key, KeyButtonState.Held, KeyboardModifier.Shift), target);
            }

            // Connect the arrow and movement keys
            //
            connectArrowKeys(state, target);
        }

        /// <summary>
        /// Connect arrow, page up and down and select (Enter) keys to a target
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        public void connectArrowKeys(State state, Target target = null)
        {
            if (target == null) target = Target.Default;

            // Check for valid State and Target
            //
            checkState(state);
            checkTarget(target);

            Keys[] otherKeys = { Keys.Right, Keys.Left, Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown, Keys.Enter };
            foreach (Keys key in otherKeys)
            {
                connectKey(state, key, target);
            }
        }

        /// <summary>
        /// Set gravity in our World
        /// </summary>
        /// <param name="gravity"></param>
        public void setGravity(BrazilVector3 gravity)
        {
            m_world.setGravity(gravity);
        }

        /// <summary>
        /// Set the bounds of our world
        /// </summary>
        /// <param name="bb"></param>
        public void setWorldBounds(BrazilBoundingBox bb)
        {
            m_world.setBounds(bb);
        }

        /// <summary>
        /// Add a State to this App
        /// </summary>
        /// <param name="state"></param>
        public void addState(string state)
        {
            m_states.Add(new State(state));
        }

        /// <summary>
        /// Add a Target to this App
        /// </summary>
        /// <param name="target"></param>
        public void addTarget(string target)
        {
            m_targets.Add(new Target(target));
        }

        /// <summary>
        /// Add a ConfirmState to this App
        /// </summary>
        /// <param name="confirmState"></param>
        public void addConfirmState(string confirmState)
        {
            m_confirmStates.Add(new ConfirmState(confirmState));
        }

        /// <summary>
        /// Get a state
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public State getState(string stateName)
        {
            foreach(State state in m_states)
            {
                if (state.m_name == stateName)
                {
                    return state;
                }
            }

            throw new Exception("BrazilApp: state not defined " + stateName);
        }

        /// <summary>
        /// Get a target from our target list
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public Target getTarget(string targetName)
        {
            foreach (Target target in m_targets)
            {
                if (target.m_name == targetName)
                {
                    return target;
                }
            }

            throw new Exception("BrazilApp: target not defined " + targetName);
        }

        /// <summary>
        /// Add a Component with a given State - by state name
        /// </summary>
        /// <param name="component"></param>
        public void addComponent(string stateName, Component component)
        {
            State state = getState(stateName);
            addComponent(state, component);
        }

        /// <summary>
        /// Add a Component with a given State
        /// </summary>
        /// <param name="component"></param>
        public void addComponent(State state, Component component)
        {
            checkState(state);
            component.addState(state);
            m_componentList.Add(component);
        }

        /// <summary>
        /// Add a component to a StateAction combination
        /// </summary>
        /// <param name="stateAction"></param>
        /// <param name="component"></param>
        public void addComponent(StateAction stateAction, Component component)
        {
            checkState(stateAction.getState());
            component.addStateAction(stateAction);
            m_componentList.Add(component);
        }

        /// <summary>
        /// Get the list of States to satisfy our interface
        /// </summary>
        /// <returns></returns>
        public List<State> getStates()
        {
            return m_states;
        }

        /// <summary>
        /// Get the list of Targets to satisfy our IWorld interface
        /// </summary>
        /// <returns></returns>
        public List<Target> getTargets()
        {
            return m_targets;
        }

        /// <summary>
        /// Check a State exists
        /// </summary>
        /// <param name="state"></param>
        protected void checkState(State state)
        {
            if (!m_states.Contains(state))
            {
                throw new Exception("Unrecognized state " + state.m_name);
            }
        }

        /// <summary>
        /// Check a Target exists
        /// </summary>
        /// <param name="target"></param>
        protected void checkTarget(Target target)
        {
            if (!m_targets.Contains(target))
            {
                throw new Exception("Unrecognised target " + target.m_name);
            }
        }

        /// <summary>
        /// We can initialise our world using this method
        /// </summary>
        /// <param name="initialState"></param>
        public void setInitialState(State initialState)
        {
            // First check to see if this state is valid
            //
            if (!m_states.Contains(initialState))
            {
                throw new Exception("Unrecognized initial state " + initialState.m_name);
            }

            m_viewSpace.setState(initialState);
        }

        /// <summary>
        /// Push any world changes through to XNA
        /// </summary>
        public void pushWorldChanges()
        {
            m_viewSpace.pushWorld();
        }

        /// <summary>
        /// Get the application mode
        /// </summary>
        /// <returns></returns>
        public BrazilAppMode getMode()
        {
            return m_mode;
        }


        /// <summary>
        /// ViewSpace object - created at construction.   The Viewspace will define what we can
        /// see for this moment in our space.   It will be rebuilt on each 'level' according to how
        /// the application/game is structued.
        /// </summary>
        protected ViewSpace m_viewSpace = new ViewSpace();

        /// <summary>
        /// A BrazilWorld object that defines size, shape, colour gravity etc at a World level.  This
        /// can be changed but will most likely stay constant over rebuilds of the ViewSpace.
        /// </summary>
        protected BrazilWorld m_world = new BrazilWorld();

        /// <summary>
        /// Action map object - created at construction
        /// </summary>
        protected ActionMap m_actionMap = new ActionMap();

        /// <summary>
        /// List of States our application can be in
        /// </summary>
        protected List<State> m_states = new List<State>();

        /// <summary>
        /// List of Targets our states we can send events to 
        /// </summary>
        protected List<Target> m_targets = new List<Target>();

        /// <summary>
        /// List of ConfirmStates
        /// </summary>
        protected List<ConfirmState> m_confirmStates = new List<ConfirmState>();

        /// <summary>
        /// List of Components that our app holds - these could be Drawable components or anything.  We keep this 
        /// List private so we have to use accessors
        /// </summary>
        private List<Component> m_componentList = new List<Component>();

        /// <summary>
        /// What mode is this app running in?
        /// </summary>
        protected BrazilAppMode m_mode = BrazilAppMode.App;
    }
}
