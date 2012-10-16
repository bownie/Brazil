using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Some convenience functions for converting between XNA and Brazil worlds
    /// </summary>
    static public class XygloConvert
    {
        /// <summary>
        /// Get an XNA Vector2 for a BrazilVector2
        /// </summary>
        /// <param name="bV"></param>
        /// <returns></returns>
        static public Vector2 getVector3(BrazilVector2 bV)
        {
            return (bV == null ? Vector2.Zero : new Vector2(bV.X, bV.Y));
        }

        /// <summary>
        /// Get an XNA Vector3 for a BrazilVector3
        /// </summary>
        /// <param name="bV"></param>
        /// <returns></returns>
        static public Vector3 getVector3(BrazilVector3 bV)
        {
            return (bV == null ? Vector3.Zero : new Vector3(bV.X, bV.Y, bV.Z));
        }

        /// <summary>
        /// Get a XNA BoundingBox
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        static public BoundingBox getBoundingBox(BrazilBoundingBox bb)
        {
            return new BoundingBox(XygloConvert.getVector3(bb.getMinimum()), XygloConvert.getVector3(bb.getMaximum()));
        }

        /// <summary>
        /// Get a XNA Color for a BrazilColour
        /// </summary>
        /// <param name="brazilColour"></param>
        /// <returns></returns>
        static public Microsoft.Xna.Framework.Color getColour(BrazilColour brazilColour)
        {
            switch (brazilColour)
            {
                case BrazilColour.Black:
                    return Color.Black;

                case BrazilColour.Blue:
                    return Color.Blue;

                case BrazilColour.Brown:
                    return Color.Brown;

                case BrazilColour.Green:
                    return Color.Green;

                case BrazilColour.Orange:
                    return Color.Orange;

                case BrazilColour.Pink:
                    return Color.Pink;

                case BrazilColour.Red:
                    return Color.Red;

                case BrazilColour.White:
                    return Color.White;

                case BrazilColour.Yellow:
                    return Color.Yellow;

                default:
                    return Color.White;
            }
        }

        /// <summary>
        /// Convert key mappings from rax (XNA) to framework (Brazil) - we also need to add modifier keys to actions
        /// when checking against a StateAction list.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        static public List<Xyglo.Brazil.Keys> keyMappings(Microsoft.Xna.Framework.Input.Keys[] keys, bool ignoreModifiers = false)
        {
            List<Xyglo.Brazil.Keys> rL = new List<Xyglo.Brazil.Keys>();
            Xyglo.Brazil.Keys newKey = Xyglo.Brazil.Keys.None;

            foreach (Microsoft.Xna.Framework.Input.Keys key in keys)
            {
                switch (key)
                {
                    case Microsoft.Xna.Framework.Input.Keys.A:
                        newKey = Keys.A;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.B:
                        newKey = Keys.B;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.C:
                        newKey = Keys.C;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D:
                        newKey = Keys.D;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.E:
                        newKey = Keys.E;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F:
                        newKey = Keys.F;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.G:
                        newKey = Keys.G;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.H:
                        newKey = Keys.H;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.I:
                        newKey = Keys.I;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.J:
                        newKey = Keys.J;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.K:
                        newKey = Keys.K;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.L:
                        newKey = Keys.L;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.M:
                        newKey = Keys.N;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.O:
                        newKey = Keys.O;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.P:
                        newKey = Keys.P;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Q:
                        newKey = Keys.Q;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.R:
                        newKey = Keys.R;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.S:
                        newKey = Keys.S;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.T:
                        newKey = Keys.T;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.U:
                        newKey = Keys.U;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.V:
                        newKey = Keys.V;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.W:
                        newKey = Keys.W;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.X:
                        newKey = Keys.X;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Y:
                        newKey = Keys.Y;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Z:
                        newKey = Keys.Z;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D0:
                        newKey = Keys.D0;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D1:
                        newKey = Keys.D1;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D2:
                        newKey = Keys.D2;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D3:
                        newKey = Keys.D3;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D4:
                        newKey = Keys.D4;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D5:
                        newKey = Keys.D5;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D6:
                        newKey = Keys.D6;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D7:
                        newKey = Keys.D7;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D8:
                        newKey = Keys.D8;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.D9:
                        newKey = Keys.D9;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Home:
                        newKey = Keys.Home;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.End:
                        newKey = Keys.End;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Up:
                        newKey = Keys.Up;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Down:
                        newKey = Keys.Down;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Left:
                        newKey = Keys.Left;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Right:
                        newKey = Keys.Right;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Escape:
                        newKey = Keys.Escape;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.LeftAlt:
                        if (!ignoreModifiers) newKey = Keys.LeftAlt;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.RightAlt:
                        if (!ignoreModifiers) newKey = Keys.RightAlt;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.LeftControl:
                        if (!ignoreModifiers) newKey = Keys.LeftControl;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.RightControl:
                        if (!ignoreModifiers) newKey = Keys.RightControl;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.LeftShift:
                        if (!ignoreModifiers) newKey = Keys.LeftShift;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.RightShift:
                        if (!ignoreModifiers) newKey = Keys.RightShift;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.LeftWindows:
                        if (!ignoreModifiers) newKey = Keys.LeftWindows;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.RightWindows:
                        if (!ignoreModifiers) newKey = Keys.RightWindows;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Space:
                        if (!ignoreModifiers) newKey = Keys.Space;
                        break;

                    default:
                        newKey = Keys.None;
                        break;
                }

                if (newKey != Keys.None)
                {
                    rL.Add(newKey);
                }
            }
            return rL;
        }

        /// <summary>
        /// Convert the mouse mappings from XNA to framework - it appears that buttons show as released
        /// continuously so we want to use this process to change these mappings to events.  The return
        /// list here will give information on whether the button has been held, pressed or released 
        /// since last Update().
        /// </summary>
        /// <returns></returns>
        static public List<Mouse> convertMouseMappings(Microsoft.Xna.Framework.Input.MouseState state, Microsoft.Xna.Framework.Input.MouseState lastState)
        {
            List<Mouse> rL = new List<Mouse>();

            // Check the left button
            //
            if (state.LeftButton != lastState.LeftButton)
            {
                if (state.LeftButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.LeftButtonPress);
                }
                else if (state.LeftButton == ButtonState.Released)
                {
                    rL.Add(Mouse.LeftButtonRelease);
                }
            }
            else  // possibly it's being held
            {
                if (state.LeftButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.LeftButtonHeld);
                }
            }


            if (state.MiddleButton != lastState.MiddleButton)
            {
                if (state.MiddleButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.MiddleButtonPress);
                }
                else if (state.MiddleButton == ButtonState.Released)
                {
                    rL.Add(Mouse.MiddleButtonRelease);
                }
            }
            else
            {
                if (state.MiddleButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.MiddleButtonHeld);
                }
            }

            if (state.RightButton != lastState.RightButton)
            {
                if (state.RightButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.RightButtonPress);
                }
                else if (state.RightButton == ButtonState.Released)
                {
                    rL.Add(Mouse.RightButtonRelease);
                }
            }
            else
            {
                if (state.RightButton == ButtonState.Pressed)
                {
                    rL.Add(Mouse.RightButtonHeld);
                }
            }

            return rL;
        }
    }
}
