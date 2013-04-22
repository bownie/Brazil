using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xyglo.Brazil;
using Xyglo.Brazil.Xna;
using Microsoft.Xna.Framework;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Screen widget base class
    /// </summary>
    public abstract class FriendlierWidget
    {
        public FriendlierWidget(XygloContext context, BrazilContext brazilContext, EyeHandler eyeHandler)
        {
            m_context = context;
            m_brazilContext = brazilContext;
            m_eyeHandler = eyeHandler;
        }

        /// <summary>
        /// Drawing abstract
        /// </summary>
        /// <param name="gametime"></param>
        public abstract void draw(GameTime gametime);

        /// <summary>
        /// Context
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// BrazilContext
        /// </summary>
        protected BrazilContext m_brazilContext;

        /// <summary>
        /// Eye handler reference
        /// </summary>
        protected EyeHandler m_eyeHandler;
    }
}
