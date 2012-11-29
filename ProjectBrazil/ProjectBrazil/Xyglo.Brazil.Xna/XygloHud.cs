using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Head up display
    /// </summary>
    public class XygloHud : XygloXnaDrawableShape
    {
        public XygloHud()
        {
        }

        /// Override the getBoundingBox call
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            throw new NotImplementedException();
        }

        public override void draw(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public override void buildBuffers(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox)
        {
        }
    }
}
