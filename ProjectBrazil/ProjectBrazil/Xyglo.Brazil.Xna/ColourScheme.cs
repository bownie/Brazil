using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Centralised place for defining an application colour scheme
    /// </summary>
    public class ColourScheme
    {

        /// <summary>
        /// List highlight
        /// </summary>
        /// <returns></returns>
        static public Color getHighlightColour() { return Color.LightGreen; }

        /// <summary>
        /// List item
        /// </summary>
        /// <returns></returns>
        static public Color getItemColour() {  return Color.DarkOrange; }

        /// <summary>
        /// Colour for flashing buffer views
        /// </summary>
        /// <returns></returns>
        static public Color getFlashColour() { return Color.White; }
    }
}
