using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// An abstract base class that we can use to hold the things we need to 
    /// draw an XNA item.
    /// </summary>
    public abstract class XygloXnaDrawableShape : XygloXnaDrawable
    {
        /// <summary>
        /// Draw this component
        /// </summary>
        //public abstract void draw(GraphicsDevice device);

        /// <summary>
        /// Build the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        //public abstract void buildBuffers(GraphicsDevice device);

        /// <summary>
        /// Store locally our effect
        /// </summary>
        protected BasicEffect m_effect;

        /// <summary>
        /// The index buffer
        /// </summary>
        protected IndexBuffer m_indexBuffer;

        /// <summary>
        /// The vertex buffer
        /// </summary>
        protected VertexBuffer m_vertexBuffer;

        /// <summary>
        /// All the vertices of the block
        /// </summary>
        protected VertexPositionColorTexture[] m_vertices = null;

        /// <summary>
        /// Indexes of all the vertices
        /// </summary>
        protected short[] m_indices = null;
    }
}
