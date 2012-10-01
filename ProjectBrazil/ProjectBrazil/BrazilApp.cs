using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A BrazilApp has to inherit and implement this class.  This is the proper - outside world view
    /// of our application.  At this class we create and hold various other components which we
    /// need to pass into the implementation of this world.  We use the ViewSpace as the actual
    /// interface into the graphics and user interface interaction layer.
    /// 
    /// Note that this namespace is uncontaminated by Xyglo.Brazil.Xna as it should stay that way.
    /// 
    /// </summary>
    public abstract class BrazilApp
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilApp()
        {
            m_viewSpace.initialise(m_actionMap, m_componentList, m_world);
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
        public abstract void initialise();

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
        /// Connect all the "normal"editor keys to a target - if the target is
        /// not specified then we assume the default target is the focus object.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="target"></param>
        public void connectEditorKeys(State state, Target target = Target.Default)
        {
            // Alphas
            //
            Keys[] alphaKeys = { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
                            Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T, Keys.U,
                            Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z };
            foreach (Keys key in alphaKeys)
            {
                connectKey(state, key, target);
            }

            // Numbers
            //
            Keys[] numKeys = { Keys.D0, Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            foreach (Keys key in numKeys)
            {
                connectKey(state, key, target);
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
            Keys[] otherKeys = { Keys.OemComma, Keys.OemPeriod, Keys.OemQuotes, Keys.OemCloseBrackets, Keys.OemOpenBrackets, Keys.OemPipe, Keys.OemMinus, Keys.OemPlus, Keys.OemQuestion, Keys.Back, Keys.Delete, Keys.Decimal, Keys.OemBackslash, Keys.LeftWindows, Keys.RightWindows, Keys.LeftControl, Keys.RightControl, Keys.RightShift, Keys.LeftShift, Keys.LeftShift, Keys.RightAlt, Keys.LeftAlt /*, Keys.Right, Keys.Left, Keys.Up, Keys.Down, Keys.PageUp, Keys.PageDown */ };
            foreach (Keys key in otherKeys)
            {
                connectKey(state, key, target);
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
        public void connectArrowKeys(State state, Target target = Target.Default)
        {
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
        /// List of Components that our app holds - these could be Drawable components
        /// </summary>
        protected List<Component> m_componentList = new List<Component>();
    }
}
