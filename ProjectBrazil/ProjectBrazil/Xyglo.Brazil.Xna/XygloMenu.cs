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
    /// A Xyglo Menu
    /// </summary>
    public class XygloMenu : XygloXnaDrawableShape
    {
        /// <summary>
        /// Total number of vertices in coin
        /// </summary>
        protected int m_numVertices;

        /// <summary>
        /// Total number of indicies in coin
        /// </summary>
        protected int m_numIndices;

        /// <summary>
        /// Size of this block
        /// </summary>
        public Vector3 m_blockSize;

        /// <summary>
        /// Our texture
        /// </summary>
        public Texture2D m_shapeTexture;

        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = false;

        /// <summary>
        /// Save our local SpriteBatch
        /// </summary>
        protected SpriteBatch m_spriteBatch;

        /// <summary>
        /// The overall FontManager
        /// </summary>
        protected FontManager m_fontManager;

        /// <summary>
        /// Options
        /// </summary>
        List<string> m_options = new List<string>();

        /// <summary>
        /// Menu constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        public XygloMenu(FontManager fontManager, SpriteBatch spriteBatch, Color colour, BasicEffect effect, Vector3 position)
        {
            m_fontManager = fontManager;
            m_spriteBatch = spriteBatch;

            // Store the effect
            //
            m_effect = effect;
            m_colour = colour;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Add a menu option
        /// </summary>
        /// <param name="item"></param>
        public void addOption(string option)
        {
            m_options.Add(option);
        }

        /// <summary>
        /// Build the shape and populate the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            // Set total number of vertices - two circle circumferences plus 2 centre points
            //
            m_numVertices = 4;

            // Total number of indices - will require more
            //
            m_numIndices = 6;

            if (m_vertices == null)
            {
                m_vertices = new VertexPositionColorTexture[m_numVertices];
            }

            if (m_vertexBuffer == null)
            {
                m_vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), m_vertices.Count(), BufferUsage.WriteOnly);
            }

            createVertices();
            createIndices(device);
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        protected void createVertices()
        {
            int longest = 0;
            foreach (string item in m_options)
            {
                if (item.Length > longest)
                    longest = item.Length;
            }

            float width = ( longest + 2 ) * m_fontManager.getViewFont(XygloView.ViewSize.Medium).MeasureString("X").X;
            float height = ( m_options.Count + 1 ) * m_fontManager.getViewFont(XygloView.ViewSize.Medium).LineSpacing;

            // Do this twice for the two coins
            //
            m_vertices[0].Position = new Vector3(m_position.X, m_position.Y, 0);
            m_vertices[0].Color = Color.Azure;
            m_vertices[0].TextureCoordinate = Vector2.Zero;

            m_vertices[1].Position = new Vector3(m_position.X + width, m_position.Y, 0);
            m_vertices[1].Color = Color.Azure;
            m_vertices[1].TextureCoordinate = Vector2.Zero;

            m_vertices[2].Position = new Vector3(m_position.X + width, m_position.Y + height, 0);
            m_vertices[2].Color = Color.Azure;
            m_vertices[2].TextureCoordinate = Vector2.Zero;

            m_vertices[3].Position = new Vector3(m_position.X, m_position.Y, 0);
            m_vertices[3].Color = Color.Azure;
            m_vertices[3].TextureCoordinate = Vector2.Zero;
        }

        /// <summary>
        /// Creation of indices is a one off operation
        /// </summary>
        void createIndices(GraphicsDevice device)
        {
            // Generate indices if not already
            //
            if (m_indices != null) return;
            
            m_indices = new short[m_numIndices];
            m_indices[0] = 0;
            m_indices[1] = 1;
            m_indices[2] = 2;
            m_indices[3] = 0;
            m_indices[4] = 2;
            m_indices[5] = 3;
        }

        /// <summary>
        /// Draw this XygloCoin
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
            device.Indices = m_indexBuffer;
            device.SetVertexBuffer(m_vertexBuffer);

            if (m_alphaBlendingTest) device.BlendState = BlendState.AlphaBlend;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;

            device.RasterizerState = rasterizerState;

            foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                // This is UserIndexedPrimitives - why?
                //
                //device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_vertices.Length, 0, m_indices.Length / 3);
                device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, m_vertices, 0, m_vertices.Length, m_indices, 0, m_indices.Length / 3);

                // Attempt
                //device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, m_vertices, 0, m_vertices.Length);
            }

            float textScale = 1.0f;

            m_spriteBatch.Begin();
            int i = 0;
            foreach(string item in m_options)
            {
                //string remainder = line.Substring(xPos, line.Length - xPos);

                m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(XygloView.ViewSize.Medium),
                    item,
                    new Vector2(m_position.X, m_position.Y + m_fontManager.getViewFont(XygloView.ViewSize.Medium).LineSpacing * i++),
                    m_colour,
                    0,
                    Vector2.Zero,
                    m_fontManager.getTextScale() * (float)textScale,
                    0,
                    0);
            }

            m_spriteBatch.End();
        }

        /// <summary>
        /// Return the VertexBuffer
        /// </summary>
        /// <returns></returns>
        public VertexBuffer getVertexBuffer()
        {
            return m_vertexBuffer;
        }

        /// <summary>
        /// Override the getBoundingBox call - examine vertex data and return a bounding box
        /// based on that.
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            Vector3 [] vertices = new Vector3[m_vertexBuffer.VertexCount];
            for(int i = 0; i < m_vertices.Length; i++)
            {
                vertices[i] = m_vertices[i].Position;
            }
            
            // This doesn't appear to work - hence the above
            //m_vertexBuffer.GetData<Vector3>(vertices);

            // Assuming that m_position is within our shape we always find min and max
            // based from this point.
            //
            return BoundingBox.CreateFromPoints(vertices); //(m_position - m_blockSize / 2, m_position + m_blockSize / 2);
        }
        
        /// <summary>
        /// The BoundingSphere for this coin
        /// </summary>
        /// <returns></returns>
        //public BoundingSphere getBoundingSphere()
        //{
            //return new BoundingSphere(m_position, m_radius);
        //}

    }
}
