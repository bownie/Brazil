using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A nice Text Banner
    /// </summary>
    public class BrazilBannerText : Component3D
    {
        /// <summary>
        /// FlyingBlock constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        public BrazilBannerText(BrazilColour colour, BrazilVector3 position, double size, string text)
        {
            m_colour = colour;
            m_position = position;
            m_size = size;
            m_text = text;
        }

        /// <summary>
        /// Position constructor - the positional details are worked out on the drawing side when
        /// we reconstruct this.
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        /// <param name="text"></param>
        public BrazilBannerText(BrazilColour colour, BrazilPosition brazilPosition, double size, string text)
        {
            m_colour = colour;
            m_size = size;
            m_text = text;
            m_brazilPosition = brazilPosition;
        }

        /// <summary>
        /// Relative constructor - against component, position and spacing
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="relPosition"></param>
        /// <param name="relComponent"></param>
        /// <param name="spacing"></param>
        /// <param name="size"></param>
        /// <param name="text"></param>
        public BrazilBannerText(BrazilColour colour, BrazilRelativePosition relPosition, Component3D relComponent, float spacing, double size, string text)
        {
            m_colour = colour;
            m_size = size;
            m_text = text;
            m_spacing = spacing;
            m_relPosition = relPosition;
            m_relComponent = relComponent;
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
        /// Get the BrazilPosition
        /// </summary>
        /// <returns></returns>
        public BrazilPosition getBrazilPosition()
        {
            return m_brazilPosition;
        }

        /// <summary>
        /// Get the BrazilRelativePosition which is used in conjunction with relative component
        /// to work out the position of this element
        /// </summary>
        /// <returns></returns>
        public BrazilRelativePosition getBrazilRelativePosition()
        {
            return m_relPosition;
        }

        /// <summary>
        /// The relative component
        /// </summary>
        /// <returns></returns>
        public Component3D getBrazilRelativeComponent()
        {
            return m_relComponent;
        }

        /// <summary>
        /// Spacing between relative components
        /// </summary>
        /// <returns></returns>
        public float getBrazilRelativeSpacing()
        {
            return m_spacing;
        }

        /// <summary>
        /// Brazil position on screen - has to be calculated at the drawing end to measure font 
        /// sizes and screen sizes so we store and pass this on.
        /// </summary>
        protected BrazilPosition m_brazilPosition = BrazilPosition.None;

        /// <summary>
        /// Relative component for relative position
        /// </summary>
        protected Component3D m_relComponent = null;

        /// <summary>
        /// Relative position
        /// </summary>
        protected BrazilRelativePosition m_relPosition = BrazilRelativePosition.None;

        /// <summary>
        /// Spacing between relative components
        /// </summary>
        protected float m_spacing = 0.0f;

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
