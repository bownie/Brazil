using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// An Action ties a UserInput to an interface element.  Let's try an example:
    /// 
    /// TextEditing:Keys.A .. Keys.Z|Shift // default action is consume on highlighted action
    /// TextEditing:Alt|N -> NewBufferView
    /// TextEditing:LeftButtonDrag -> WorldDrag 
    /// TextEditing:LeftButtonClick -> ZapToPosition
    /// TextEditing:Alt|C -> Configuration // activate config mode
    /// 
    /// The Action is an abstract base class to allow us to share actions across input
    /// devices.
    /// 
    /// </summary>
    public abstract class Action
    {
        public string m_name { get; set; }

        public Action()
        {
            m_name = "Default Action";
        }

        public Action(Action a)
        {
            m_name = a.m_name;
        }
    }

    /// <summary>
    /// KeyAction
    /// </summary>
    public class KeyAction : Action
    {
        /// <summary>
        /// Key constructor
        /// </summary>
        /// <param name="key"></param>
        public KeyAction(Keys key) { m_key = key; }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="kA"></param>
        public KeyAction(KeyAction kA) : base(kA)
        {
            m_key = kA.m_key;
            m_down = kA.m_down;
        }


        /// <summary>
        /// Key
        /// </summary>
        public Keys m_key { get; set; }

        /// <summary>
        /// Key down or up event?
        /// </summary>
        public bool m_down { get; set; }
    }

    /// <summary>
    /// MouseAction
    /// </summary>
    public class MouseAction : Action
    {
        /// <summary>
        /// Mouse constructor
        /// </summary>
        /// <param name="mouse"></param>
        public MouseAction(Mouse mouse)
        {
            m_mouse = mouse;
        }

        public MouseAction(MouseAction mA)
        {
            m_mouse = mA.m_mouse;
            m_name = m_name;
        }

        public Mouse m_mouse { get; set; }
    }

}
