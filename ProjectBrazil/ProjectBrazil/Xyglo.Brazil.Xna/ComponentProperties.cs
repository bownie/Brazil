using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Property box for a component to allow viewing/editing
    /// </summary>
    public class ComponentProperties : Xyglo.Friendlier.FriendlierWidget
    {
        public ComponentProperties(XygloContext context, BrazilContext brazilContext, EyeHandler eyeHandler):
            base(context, brazilContext, eyeHandler)
        {
        }

        public void setSelectedComponent(Component component)
        {
            m_component = component;
        }

        public override void draw(GameTime gameTime)
        {
        }


        /// <summary>
        /// Select component
        /// </summary>
        protected Component m_component;

    }
}
