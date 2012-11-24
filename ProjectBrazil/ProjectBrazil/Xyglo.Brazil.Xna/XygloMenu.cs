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
 /*
  * // Set total number of vertices - two circle circumferences plus 2 centre points
            //
            m_numVertices = m_vertsInCircle * m_circlesInCoin + 2;

            // Total number of indices - will require more
            //
            m_numIndices = m_vertsInCircle * m_circlesInCoin * 6;

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
  */
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        protected void createVertices()
        {
            /*
            Vector3 rad = new Vector3((float)Math.Abs(m_radius), 0, 0);
            int maxVertex = 0;
            float coinWidth = m_radius / 8;
            Vector3 point;

            // Rotate about the Y axis
            //
            Matrix worldMatrix = Matrix.CreateRotationY((float)m_rotation);

            // Do this twice for the two coins
            //
            for (int x = 0; x < m_circlesInCoin; x++)
            {
                // There is no difx rotation for these
                //
                float difx = 0.0f;

                for (int y = 0; y < m_vertsInCircle; y++)
                {
                    float dify = 360.0f / (float)m_vertsInCircle;

                    Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); // rotate vertex around z
                    Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); // rotate circle around y

                    // transformation including rotation with world matrix
                    //
                    point = Vector3.Transform(Vector3.Transform(rad, zrot), yrot);

                    // Adjust the translation by a factor of the radius for the two coins
                    //
                    if (x == 0)
                    {
                        point.Z += coinWidth;
                    }
                    else // x == 1
                    {
                        point.Z -= coinWidth;
                    }

                    point = m_position + Vector3.Transform(point, worldMatrix);

                    //m_vertices[x + y * m_vertsInCircle] = new VertexPositionColorTexture(point, m_colour, new Vector2(0, 0));
                    m_vertices[y + x * m_vertsInCircle].Position = point;
                    m_vertices[y + x * m_vertsInCircle].Color = m_colour;
                    m_vertices[y + x * m_vertsInCircle].TextureCoordinate = Vector2.Zero;
                    maxVertex = y + x * m_vertsInCircle;
                }
            }

            // DO THIS:
            // http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2/Textures.php


            // Define the centre points
            point.X = 0;
            point.Y = 0;
            point.Z = coinWidth;
            point = m_position + Vector3.Transform(point, worldMatrix);
            m_vertices[maxVertex + 1].Position = point;
            m_vertices[maxVertex + 1].Color = m_colour;
            m_vertices[maxVertex + 1].TextureCoordinate = Vector2.Zero;

            point.X = 0;
            point.Y = 0;
            point.Z = -coinWidth;
            point = m_position + Vector3.Transform(point, worldMatrix); 
            m_vertices[maxVertex + 2].Position = point;
            m_vertices[maxVertex + 2].Color = m_colour;
            m_vertices[maxVertex + 2].TextureCoordinate = Vector2.Zero;
             */
        }

        /// <summary>
        /// Creation of indices is a one off operation
        /// </summary>
        void createIndices(GraphicsDevice device)
        {
            /*
            // Generate indices if not already
            //
            if (m_indices != null) return;
            
            int centreFront = m_circlesInCoin * m_vertsInCircle;
            int centreBack = centreFront + 1;
            m_indices = new short[m_numIndices];

            // Index counter
            int i = 0;

            for (int y = 0; y < m_vertsInCircle; y++)
            {
                // Front circle
                //
                m_indices[i++] = (short)centreFront;
                m_indices[i++] = (short)(y);
                m_indices[i++] = (short)((y + 1) % m_vertsInCircle);

                // Back circle
                //
                m_indices[i++] = (short)centreBack;
                m_indices[i++] = (short)(2 * m_vertsInCircle - y - 1);
                m_indices[i++] = (short)((2 * m_vertsInCircle - y - 2 < m_vertsInCircle) ? 3 * m_vertsInCircle - y - 2 : 2 * m_vertsInCircle - y - 2);

                // Connect edges with two triangles
                //
                m_indices[i++] = (short)(y);
                m_indices[i++] = (short)(m_vertsInCircle + y);
                m_indices[i++] = (short)(m_vertsInCircle + (m_vertsInCircle + y + 1) % (m_vertsInCircle));

                m_indices[i++] = (short)(y);
                m_indices[i++] = (short)(short)(m_vertsInCircle + (m_vertsInCircle + y + 1) % (m_vertsInCircle));
                m_indices[i++] = (short)((y + 1) % m_vertsInCircle);
            }

            m_indexBuffer = new IndexBuffer(device, typeof(short), m_numIndices, BufferUsage.WriteOnly);
            m_indexBuffer.SetData(m_indices);
            */
        }

        /// <summary>
        /// Draw this XygloCoin
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device)
        {
            /*
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
            */

            float textScale = 1.0f;

            m_spriteBatch.Begin();

            foreach(string item in m_options)
            {
                //string remainder = line.Substring(xPos, line.Length - xPos);

                m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(XygloView.ViewSize.Medium),
                    item,
                    new Vector2(0, 0),
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
