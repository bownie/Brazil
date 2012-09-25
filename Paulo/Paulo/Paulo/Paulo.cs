using System;
using System.Collections.Generic;
using System.Linq;
using Xyglo.Brazil;

/*using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
*/

namespace Paulo
{
    /// <summary>
    /// This is the top level class for our Paulo game.  It inherits a BrazilApp object which
    /// inherits the XNA Game implementation.
    /// </summary>
    public class Paulo : BrazilApp
    {
        public Paulo()
        {
        }

        /// <summary>
        /// Allows the game to perform any initialisation of objects and positions before starting.
        /// </summary>
        public override void initialise()
        {
            FlyingBlock block1 = new FlyingBlock(BrazilColour.Blue, new BrazilVector3(-10, -100, 0), new BrazilVector3(100.0f, 100.0f, 10.0f));
            block1.setVelocity(new BrazilVector3(-1, 0, 0));

            // Push onto component list
            //
            m_componentList.Add(block1);

            FlyingBlock block2 = new FlyingBlock(BrazilColour.Brown, new BrazilVector3(0, 0, 0), new BrazilVector3(100, 20, 20));
            m_componentList.Add(block2);
        }


    }
}
