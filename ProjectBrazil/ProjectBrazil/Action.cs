using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Modifier from the Keyboard
    /// </summary>
    public enum KeyboardModifier
    {
        None        = 0x0,
        Alt         = 0x1,
        Shift       = 0x2,
        Control     = 0x4,
        Windows     = 0x8
    }

    /// <summary>
    /// What is the status of the Key or Button
    /// </summary>
    public enum KeyButtonState
    {
        Pressed,
        Released,
        Held
    }

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
        public KeyAction(Keys key, bool down = true)
        {
            m_key = key;
            if (down)
            {
                m_state = KeyButtonState.Pressed;
            }

            m_name = "Key Action";
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="kA"></param>
        public KeyAction(KeyAction kA) : base(kA)
        {
            m_key = kA.m_key;
            m_state = kA.m_state;
            m_name = "Key Action";
        }

        /// <summary>
        /// KeyAction constructor with modifier
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifier"></param>
        /// <param name="down"></param>
        public KeyAction(Keys key, KeyboardModifier modifier, bool down = true)
        {
            m_key = key;
            m_modifier = modifier;
            m_name = "Key Action";

            if (down)
            {
                m_state = KeyButtonState.Pressed;
            }
        }

        /// <summary>
        /// Operator equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(KeyAction a, KeyAction b)
        {
            return (a.m_key == b.m_key && a.m_modifier == b.m_modifier &&
                    a.m_state == b.m_state && a.m_name == b.m_name);
        }

        /// <summary>
        /// Operator not equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(KeyAction a, KeyAction b)
        {
            return (a.m_key != b.m_key || a.m_modifier != b.m_modifier ||
                    a.m_state != b.m_state || a.m_name != b.m_name);
        }

        // Needed to avoid warning
        //
        public override int GetHashCode()
        {
            return -1;
        }

        // Needed to avoid warning
        //
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Key
        /// </summary>
        public Keys m_key { get; set; }

        /// <summary>
        /// Key down or up event?
        /// </summary>
//        public bool m_down { get; set; }
        public KeyButtonState m_state = KeyButtonState.Released;

        /// <summary>
        /// Is Shift set?
        /// </summary>
        /// <returns></returns>
        public bool withShift()
        {
            return ((m_modifier & KeyboardModifier.Shift) > 0);
        }

        /// <summary>
        /// Is Alt set?
        /// </summary>
        /// <returns></returns>
        public bool withAlt()
        {
            return ((m_modifier & KeyboardModifier.Alt) > 0);
        }

        /// <summary>
        /// Is Control set?
        /// </summary>
        /// <returns></returns>
        public bool withControl()
        {
            return ((m_modifier & KeyboardModifier.Control) > 0);
        }

        /// <summary>
        /// Test this KeyboardAction against a key and key state
        /// </summary>
        /// <param name="key"></param>
        /// <param name="modifier"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool test(Keys key, KeyboardModifier modifier, KeyButtonState state = KeyButtonState.Pressed)
        {
            return (m_key == key && m_modifier == modifier && m_state == state);

        }

        /// <summary>
        /// Shift down?
        /// </summary>
        public KeyboardModifier m_modifier = KeyboardModifier.None;
    }


    /// <summary>
    /// Project Brazil mouse actions
    /// </summary>
    public enum Mouse
    {
        None,
        LeftButtonPress,
        LeftButtonHeld,
        LeftButtonRelease,
        MiddleButtonPress,
        MiddleButtonHeld,
        MiddleButtonRelease,
        RightButtonPress,
        RightButtonHeld,
        RightButtonRelease,
        ScrollUp,
        ScrollDown
    }

    /// <summary>
    /// MouseAction
    /// </summary>
    public class MouseAction : Action
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MouseAction()
        {
            m_mouse = Mouse.None;
        }

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

        /// <summary>
        /// Mouse state
        /// </summary>
        public Mouse m_mouse { get; set; }

        /// <summary>
        /// Mouse position
        /// </summary>
        public BrazilVector2 m_position = BrazilVector2.Zero;

        /// <summary>
        /// Scrollwheel value
        /// </summary>
        public int m_scrollWheel = 0;
    }
}
