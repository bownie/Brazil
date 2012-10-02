using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A nice Text Banner
    /// </summary>
    public class BannerText : Component3D
    {
        /// <summary>
        /// FlyingBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public BannerText(BrazilColour colour, BrazilVector3 position, double size, string text)
        {
            m_colour = colour;
            m_position = position;
            m_size = size;
            m_text = text;
        }

        /// <summary>
        /// Get text
        /// </summary>
        /// <returns></returns>
        public string getText()
        {
            return m_text;
        }

        /// <summary>
        /// Get the Size
        /// </summary>
        /// <returns></returns>
        public double getSize()
        {
            return m_size;
        }

        /// <summary>
        /// Size of the Text
        /// </summary>
        /// <param name="size"></param>
        public void setSize(double size)
        {
            m_size = size;
        }

        /// <summary>
        /// The font size
        /// </summary>
        protected double m_size;

        /// <summary>
        /// The text
        /// </summary>
        protected string m_text;
    }
}
