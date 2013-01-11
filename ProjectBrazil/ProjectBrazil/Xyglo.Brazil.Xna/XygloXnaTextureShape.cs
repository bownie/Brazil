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
    public abstract class XygloXnaTextureShape : XygloXnaDrawable
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
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected void initialisePrimitive(GraphicsDevice graphicsDevice)
        {
            // Create a vertex declaration, describing the format of our vertex data.

            // Create a vertex buffer, and copy our vertex data into it.
            m_vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), m_vertices.Count(), BufferUsage.None);
            m_vertexBuffer.SetData(m_vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            m_indexBuffer = new IndexBuffer(graphicsDevice, typeof(short), m_indices.Count(), BufferUsage.None);
            m_indexBuffer.SetData(m_indices.ToArray());

        }

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
        protected VertexPositionNormalTexture[] m_vertices = null;

        /// <summary>
        /// Indexes of all the vertices
        /// </summary>
        protected short[] m_indices = null;
    }
}
