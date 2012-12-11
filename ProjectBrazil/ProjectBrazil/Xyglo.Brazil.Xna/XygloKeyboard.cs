using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Xyglo.Brazil.Xna
{

    /// <summary>
    /// XygloKeyboard class wraps the XNA Keyboard handling routines and provides auto repeat
    /// and character capture with modifier keys.  No language/keyboard internationalisation
    /// support.
    /// </summary>
    public class XygloKeyboard
    {
        public XygloKeyboard(XygloContext context, double repeatHoldTime, double repeatInterval)
        {
            m_context = context;
            m_repeatHoldTime = repeatHoldTime;
            m_repeatInterval = repeatInterval;
        }

        /// <summary>
        /// Get all the KeyActions that are currently in progress - whether keys be newly down 
        /// or held down or released.  We can use this method to define repeat timings for individual
        /// keys as well.
        /// </summary>
        /// <returns></returns>
        public List<KeyAction> getAllKeyActions()
        {
            List<KeyAction> lKA = new List<KeyAction>();
            List<Keys> newKeys = XygloConvert.keyMappings(Keyboard.GetState().GetPressedKeys());
            List<Keys> lastKeys = XygloConvert.keyMappings(m_lastKeyboardState.GetPressedKeys());
            KeyboardModifier modifier = KeyboardModifier.None;

            // Check for modifiers - flag and remove
            //
            if (newKeys.Contains(Keys.LeftShift))
                modifier |= KeyboardModifier.Shift;

            if (newKeys.Contains(Keys.RightShift))
                modifier |= KeyboardModifier.Shift;

            if (newKeys.Contains(Keys.LeftControl))
                modifier |= KeyboardModifier.Control;

            if (newKeys.Contains(Keys.RightControl))
                modifier |= KeyboardModifier.Control;

            if (newKeys.Contains(Keys.LeftAlt))
                modifier |= KeyboardModifier.Alt;

            if (newKeys.Contains(Keys.RightAlt))
                modifier |= KeyboardModifier.Alt;
                
            if (newKeys.Contains(Keys.LeftWindows))
                modifier |= KeyboardModifier.Windows;

            if (newKeys.Contains(Keys.RightWindows))
                modifier |= KeyboardModifier.Windows;

            // At this point we can work out if we have any new keys pressed or any held
            // 
            foreach (Keys key in removeModifiers(newKeys))
            {
                bool pressed = true;

                foreach (Keys lastKey in XygloConvert.keyMappings(m_lastKeyboardState.GetPressedKeys()))
                {
                    if (lastKey == key) // was down last time so hasn't been pressed - is held
                    {
                        pressed = false;  // set flag
                        lastKeys.Remove(lastKey); // and remove from lastKeys
                        break;
                    }
                }

                KeyAction keyAction = new KeyAction(key, modifier);

                if (pressed)
                {
                    keyAction.m_state = KeyButtonState.Pressed;
                }
                else
                {
                    keyAction.m_state = KeyButtonState.Held;
                }

                lKA.Add(keyAction);
            }

            // Now any keys that are were and have been released.
            //
            foreach (Keys key in removeModifiers(lastKeys))
            {
                KeyAction keyAction = new KeyAction(key, modifier);
                keyAction.m_state = KeyButtonState.Released;
                lKA.Add(keyAction);
            }

            // Finally set the convenience flags for the modifiers already worked out
            //
            m_shiftDown = ((modifier & KeyboardModifier.Shift) == KeyboardModifier.Shift);
            m_ctrlDown = ((modifier & KeyboardModifier.Control) == KeyboardModifier.Control);
            m_altDown = ((modifier & KeyboardModifier.Alt) == KeyboardModifier.Alt);
            m_windowsDown = ((modifier & KeyboardModifier.Windows) == KeyboardModifier.Windows);

            // Store the last state
            //
            m_lastKeyboardState = Keyboard.GetState();

            return lKA;
        }

        /// <summary>
        /// Remove keyboard modifiers from a list of XNA Keys
        /// </summary>
        /// <param name="inList"></param>
        /// <returns></returns>
        protected List<Keys> removeModifiers(List<Keys> inList)
        {
            List<Keys> outList = new List<Keys>();
            List<Keys> modifierList = new List<Keys>();
            modifierList.Add(Keys.LeftShift);
            modifierList.Add(Keys.RightShift);
            modifierList.Add(Keys.LeftControl);
            modifierList.Add(Keys.RightControl);
            modifierList.Add(Keys.LeftAlt);
            modifierList.Add(Keys.RightAlt);
            modifierList.Add(Keys.LeftWindows);
            modifierList.Add(Keys.RightWindows);

            foreach (Keys key in inList)
            {
                if (!modifierList.Contains(key))
                {
                    outList.Add(key);
                }
            }

            return outList;
        }

        /// <summary>
        /// Check to see whether a key is available for repeat yet and ensure that the m_keyMap is
        /// updated with the latest status.  Returns true if we can do something with this key.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="keyAction"></param>
        /// <returns></returns>
        public bool checkKeyRepeat(GameTime gameTime, KeyAction keyAction)
        {
            // Skip all repeats except the ones we want
            //
            if (keyAction.m_state == KeyButtonState.Released)
            {
                // Release the value of the key pressed
                m_keyMap[keyAction.m_key] = null;
                m_keyMap.Remove(keyAction.m_key);
                return false;
            }

            // Create new key storage if it's not available yet - might want to
            // optimise this later.
            //
            if (!m_keyMap.ContainsKey(keyAction.m_key))
            {
                m_keyMap[keyAction.m_key] = new Pair<bool, double>();
            }

            // For the held state we have to check to see if it's ready to repeat this key yet
            //
            if (keyAction.m_state == KeyButtonState.Held)
            {
                if (m_keyMap[keyAction.m_key].First) // Within the first hold interval
                {
                    if ((gameTime.TotalGameTime.TotalSeconds - m_keyMap[keyAction.m_key].Second) < m_repeatHoldTime)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((gameTime.TotalGameTime.TotalSeconds - m_keyMap[keyAction.m_key].Second) < m_repeatInterval)
                    {
                        //Logger.logMsg("LS = " + gameTime.TotalGameTime.TotalSeconds + ", MS = " + m_keyMap[keyAction.m_key].Second);
                        return false;
                    }
                }
            }

            // For pressed and held store the last time this event was issued
            //
            m_keyMap[keyAction.m_key].First = (keyAction.m_state == KeyButtonState.Pressed);
            m_keyMap[keyAction.m_key].Second = gameTime.TotalGameTime.TotalSeconds;

            return true;
        }

        /// <summary>
        /// Are we pressing on a number key?
        /// </summary>
        /// <returns></returns>
        public string getNumberKey()
        {
            string key = "";

            foreach (Keys keyDown in Keyboard.GetState().GetPressedKeys())
            {
                switch (keyDown)
                {
                    case Keys.D0:
                        key = "0";
                        break;

                    case Keys.D1:
                        key = "1";
                        break;

                    case Keys.D2:
                        key = "2";
                        break;

                    case Keys.D3:
                        key = "3";
                        break;

                    case Keys.D4:
                        key = "4";
                        break;

                    case Keys.D5:
                        key = "5";
                        break;

                    case Keys.D6:
                        key = "6";
                        break;

                    case Keys.D7:
                        key = "7";
                        break;

                    case Keys.D8:
                        key = "8";
                        break;

                    case Keys.D9:
                        key = "9";
                        break;

                    default:
                        break;
                }
            }
            return key;
        }

        /// <summary>
        /// Set repeats
        /// </summary>
        /// <param name="repeatHoldTime"></param>
        /// <param name="repeatInterval"></param>
        public void setRepeats(double repeatHoldTime, double repeatInterval)
        {
            m_repeatHoldTime = repeatHoldTime;
            m_repeatInterval = repeatInterval;
        }

        /// <summary>
        /// Get a key from a KeyAction
        /// </summary>
        /// <param name="keyAction"></param>
        /// <param name="shiftDown"></param>
        /// <returns></returns>
        public string getKey(KeyAction keyAction)
        {
            string key = "";

            // Set this - we use this to protect having to change the switch
            // statement.  Again.
            //
            bool shiftDown = m_shiftDown;

            switch (keyAction.m_key)
            {
                case Keys.LeftShift:
                case Keys.RightShift:
                case Keys.LeftControl:
                case Keys.RightControl:
                case Keys.LeftAlt:
                case Keys.RightAlt:
                    break;

                case Keys.OemPipe:
                    if (shiftDown)
                        key += "|";
                    else
                        key += "\\";
                    break;

                case Keys.OemQuestion:
                    if (shiftDown)
                        key += "?";
                    else
                        key += "/";
                    break;

                case Keys.OemSemicolon:
                    if (shiftDown)
                        key += ":";
                    else
                        key += ";";
                    break;

                case Keys.OemQuotes:
                    if (shiftDown)
                        key += "\"";
                    else
                        key += "'";
                    break;

                case Keys.OemTilde:
                    if (shiftDown)
                        key += "@";
                    else
                        key += "'";
                    break;

                case Keys.OemOpenBrackets:
                    if (shiftDown)
                        key += "{";
                    else
                        key += "[";
                    break;

                case Keys.OemCloseBrackets:
                    if (shiftDown)
                        key += "}";
                    else
                        key += "]";
                    break;

                case Keys.D0:
                    if (shiftDown)
                        key += ")";
                    else
                        key += "0";
                    break;

                case Keys.D1:
                    if (shiftDown)
                        key += "!";
                    else
                        key += "1";
                    break;

                case Keys.D2:
                    if (shiftDown)
                        key += "@";
                    else
                        key += "2";
                    break;

                case Keys.D3:
                    if (shiftDown)
                        key += "#";
                    else
                        key += "3";
                    break;

                case Keys.D4:
                    if (shiftDown)
                        key += "$";
                    else
                        key += "4";
                    break;

                case Keys.D5:
                    if (shiftDown)
                        key += "%";
                    else
                        key += "5";
                    break;

                case Keys.D6:
                    if (shiftDown)
                        key += "^";
                    else
                        key += "6";
                    break;

                case Keys.D7:
                    if (shiftDown)
                        key += "&";
                    else
                        key += "7";
                    break;

                case Keys.D8:
                    if (shiftDown)
                        key += "*";
                    else
                        key += "8";
                    break;

                case Keys.D9:
                    if (shiftDown)
                        key += "(";
                    else
                        key += "9";
                    break;


                case Keys.Space:
                    key += " ";
                    break;

                case Keys.OemPlus:
                    if (shiftDown)
                        key += "+";
                    else
                        key += "=";
                    break;

                case Keys.OemMinus:
                    if (shiftDown)
                        key += "_";
                    else
                        key += "-";
                    break;

                case Keys.OemPeriod:
                    if (shiftDown)
                        key += ">";
                    else
                        key += ".";
                    break;

                case Keys.OemComma:
                    if (shiftDown)
                        key += "<";
                    else
                        key += ",";
                    break;

                case Keys.A:
                case Keys.B:
                case Keys.C:
                case Keys.D:
                case Keys.E:
                case Keys.F:
                case Keys.G:
                case Keys.H:
                case Keys.I:
                case Keys.J:
                case Keys.K:
                case Keys.L:
                case Keys.M:
                case Keys.N:
                case Keys.O:
                case Keys.P:
                case Keys.Q:
                case Keys.R:
                case Keys.S:
                case Keys.T: // see below for weirdness
                case Keys.U:
                case Keys.V:
                case Keys.W:
                case Keys.X:
                case Keys.Y:
                case Keys.Z:
                    if (shiftDown)
                        key += keyAction.m_key.ToString().ToUpper();
                    else
                        key += keyAction.m_key.ToString().ToLower();
                    break;

                    /*
                case Keys.T:
                    if (m_brazilContext.m_state.equals("FileOpen"))
                    {
                        // Open a file as read only and tail it
                        //
                        traverseDirectory(gameTime, true, true);
                    }
                    else
                    {
                        if (shiftDown)
                            key += keyAction.m_key.ToString().ToUpper();
                        else
                            key += keyAction.m_key.ToString().ToLower();
                    }
                    break;
                    */

                // Do nothing as default
                //
                default:
                    key += "";
                    break;
            }
            return key;
        }

        /// <summary>
        /// Last keyboard state so that we can compare with current
        /// </summary>
        protected KeyboardState m_lastKeyboardState;

        public bool isAltDown() { return m_altDown; }
        public bool isCtrlDown() { return m_ctrlDown; }
        public bool isShiftDown() { return m_shiftDown; }
        public bool isWindowsDown() { return m_windowsDown; }

        /// <summary>
        /// Time for key auto-repeat to start - defaults to zero
        /// </summary>
        protected double m_repeatHoldTime = 0; // seconds

        /// <summary>
        /// Time between autorepeats
        /// </summary>
        protected double m_repeatInterval = 0.05; // seconds

        /// <summary>
        /// Store a map of all keys that are being pressed or held - bool is true for the first
        /// pass through for the 'initial' hold.  Double is for the GameTime in TotalSeconds.
        /// </summary>
        protected Dictionary<Keys, Pair<bool, double>> m_keyMap = new Dictionary<Keys, Pair<bool, double>>();

        /// <summary>
        /// XygloContext
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// Is shift down?
        /// </summary>
        protected bool m_shiftDown;

        /// <summary>
        /// Is control down?
        /// </summary>
        protected bool m_ctrlDown;

        /// <summary>
        /// Is alt down?
        /// </summary>
        protected bool m_altDown;

        /// <summary>
        /// Is Windows key down?
        /// </summary>
        protected bool m_windowsDown;
    }
}
