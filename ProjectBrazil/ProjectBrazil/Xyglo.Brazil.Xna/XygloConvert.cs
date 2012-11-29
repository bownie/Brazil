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
        /// Round a Vector3 to a given precision
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        static public Vector3 roundVector(Vector3 vector, int precision = 2)
        {
            return new Vector3((float)Math.Round(vector.X, precision), (float)Math.Round(vector.Y, precision), (float)Math.Round(vector.Z, precision));
        }

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
                        newKey = Keys.M;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.N:
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

                        // Do we need ignoreModifiers or not??
                        //
                    case Microsoft.Xna.Framework.Input.Keys.Delete:
                        if (!ignoreModifiers) newKey = Keys.Delete;
                        break;

                        // Question and forward slash
                    case Microsoft.Xna.Framework.Input.Keys.OemQuestion:
                        newKey = Keys.OemQuestion;
                        break;
                        
                    case Microsoft.Xna.Framework.Input.Keys.OemBackslash:
                        newKey = Keys.OemBackslash;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Back:
                        newKey = Keys.Back;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Enter:
                        newKey = Keys.Enter;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F1:
                        newKey = Keys.F1;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F2:
                        newKey = Keys.F2;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F3:
                        newKey = Keys.F3;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F4:
                        newKey = Keys.F4;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F5:
                        newKey = Keys.F5;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F6:
                        newKey = Keys.F6;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F7:
                        newKey = Keys.F7;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F8:
                        newKey = Keys.F8;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F9:
                        newKey = Keys.F9;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F10:
                        newKey = Keys.F10;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F11:
                        newKey = Keys.F11;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.F12:
                        newKey = Keys.F12;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemPeriod:
                        newKey = Keys.OemPeriod;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemPipe:
                        newKey = Keys.OemPipe;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.PageUp:
                        newKey = Keys.PageUp;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.PageDown:
                        newKey = Keys.PageDown;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.Tab:
                        newKey = Keys.Tab;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemMinus:
                        newKey = Keys.OemMinus;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemPlus:
                        newKey = Keys.OemPlus;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemOpenBrackets:
                        newKey = Keys.OemOpenBrackets;
                        break;

                    case Microsoft.Xna.Framework.Input.Keys.OemCloseBrackets:
                        newKey = Keys.OemCloseBrackets;
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

            // Check for scrollwheel change
            //
            if (rL.Count == 0)
            {
                if (state.ScrollWheelValue > lastState.ScrollWheelValue)
                {
                    rL.Add(Mouse.ScrollUp);
                } else if (state.ScrollWheelValue < lastState.ScrollWheelValue)
                {
                    rL.Add(Mouse.ScrollDown);
                }
            }

            return rL;
        }
        

        /// <summary>
        /// Helper function to work out the position of a BrazilPosition at a given screen, character size
        /// and scaling.
        /// </summary>
        /// <param name="brazilPosition"></param>
        /// <param name="fontManager"></param>
        /// <param name="winWidth"></param>
        /// <param name="winHeight"></param>
        /// <param name="scale"></param>
        /// <param name="textLength"></param>
        /// <returns></returns>
        static public Vector3 getTextPosition(Component3D component, FontManager fontManager, int winWidth, int winHeight)
        {
            // Default to actual position supplied
            //
            Vector3 position = XygloConvert.getVector3(component.getPosition());

            // Now if we have a BrazilBannerText then we need to work out some values around it
            //
            if (component.GetType() == typeof(Xyglo.Brazil.BrazilBannerText))
            {
                BrazilBannerText bt = (Xyglo.Brazil.BrazilBannerText)component;
                int textLength = bt.getText().Length;
                float scale = (float)bt.getSize();

                float charWidth = scale * fontManager.getCharWidth(FontManager.FontType.Overlay);
                float lineSpacing = scale * fontManager.getLineSpacing(FontManager.FontType.Overlay);
                
                if (bt.getBrazilPosition() != BrazilPosition.None)
                {
                    switch (bt.getBrazilPosition())
                    {
                        case BrazilPosition.TopLeft:
                            position.X = 0;
                            position.Y = 0;
                            break;

                        case BrazilPosition.TopMiddle:
                            position.X = winWidth / 2 - charWidth * textLength / 2;
                            position.Y = 0;
                            break;

                        case BrazilPosition.TopRight:
                            position.X = winWidth - charWidth * textLength;
                            position.Y = 0;
                            break;

                        case BrazilPosition.TopThirdMiddle:
                            position.X = winWidth / 2 - charWidth * textLength / 2;
                            position.Y = winHeight / 3 - lineSpacing / 2;
                            break;

                        case BrazilPosition.MiddleLeft:
                            position.X = 0;
                            position.Y = winHeight / 2 - lineSpacing / 2;
                            break;

                        case BrazilPosition.Middle:
                            position.X = winWidth / 2 - charWidth * textLength / 2;
                            position.Y = winHeight / 2 - lineSpacing / 2;
                            break;

                        case BrazilPosition.MiddleRight:
                            position.X = winWidth - charWidth * textLength;
                            position.Y = winHeight / 2 - lineSpacing / 2;
                            break;

                        case BrazilPosition.BottomThirdMiddle:
                            position.X = winWidth / 2 - charWidth * textLength / 2;
                            position.Y = 2 * winHeight / 3 - lineSpacing / 2;
                            break;

                        case BrazilPosition.BottomLeft:
                            position.X = 0;
                            position.Y = winHeight - lineSpacing;
                            break;

                        case BrazilPosition.BottomMiddle:
                            position.X = winWidth / 2 - charWidth * textLength / 2;
                            position.Y = winHeight - lineSpacing;
                            break;

                        case BrazilPosition.BottomRight:
                            position.X = winWidth - charWidth * textLength;
                            position.Y = winHeight - lineSpacing;
                            break;

                        case BrazilPosition.None:
                        default:
                            // Do nothing
                            break;
                    }
                }
                else if (bt.getBrazilRelativeComponent() != null && bt.getBrazilRelativePosition() != BrazilRelativePosition.None)
                {
                    // Recursive call of this method
                    //
                    Vector3 relPosition = getTextPosition(bt.getBrazilRelativeComponent(), fontManager, winWidth, winHeight);

                    if (bt.getBrazilRelativeComponent().GetType() == typeof(Xyglo.Brazil.BrazilBannerText))
                    {
                        BrazilBannerText relBT = (BrazilBannerText)bt.getBrazilRelativeComponent();
                        int relTextLength = relBT.getText().Length;
                        float relScale = (float)relBT.getSize();
                        float relCharWidth = relScale * fontManager.getCharWidth(FontManager.FontType.Overlay);
                        float relLineSpacing = relScale * fontManager.getLineSpacing(FontManager.FontType.Overlay);

                        // Same Z position
                        //
                        position.Z = relPosition.Z;

                        switch (bt.getBrazilRelativePosition())
                        {
                            case BrazilRelativePosition.AboveLeft:
                                position.X = relPosition.X;
                                position.Y = relPosition.Y - lineSpacing;
                                break;

                            case BrazilRelativePosition.AboveCentred: // centred on the string from the relative component
                                position.X = (relPosition.X + relCharWidth * relTextLength / 2) - (charWidth * textLength / 2);
                                position.Y = relPosition.Y - lineSpacing;
                                break;

                            case BrazilRelativePosition.AboveRight:
                                position.X = relPosition.X + relCharWidth * relTextLength - charWidth * textLength;
                                position.Y = relPosition.Y - lineSpacing;
                                break;

                            case BrazilRelativePosition.LeftButt:
                                position.X = relPosition.X - charWidth * textLength - bt.getBrazilRelativeSpacing(); // include spacing
                                position.Y = relPosition.Y; // might need to adjust this
                                break;

                            case BrazilRelativePosition.RightButt:
                                position.X = relPosition.X + relCharWidth * relTextLength + bt.getBrazilRelativeSpacing(); // include spacing
                                position.Y = relPosition.Y; // again might need adjusting
                                break;

                            case BrazilRelativePosition.BeneathLeft:
                                position.X = relPosition.X;
                                position.Y = relPosition.Y + relLineSpacing;
                                break;

                            case BrazilRelativePosition.BeneathCentred:
                                position.X = (relPosition.X + relCharWidth * relTextLength / 2) - (charWidth * textLength / 2);
                                position.Y = relPosition.Y + relLineSpacing;
                                break;

                            case BrazilRelativePosition.BeneathRight:
                                position.X = relPosition.X + relCharWidth * relTextLength - charWidth * textLength;
                                position.Y = relPosition.Y + relLineSpacing;
                                break;

                            case BrazilRelativePosition.None:
                            default:
                                // Do nothing
                                break;
                        }
                    }
                }
            }
            
            return position;
        }
    }
}
