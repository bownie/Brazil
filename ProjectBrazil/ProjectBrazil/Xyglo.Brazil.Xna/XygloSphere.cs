using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Thanks to the following link for the bones of this class:
    /// 
    /// http://thver.blogspot.nl/2012/07/how-to-create-sphere-programmatically.html
    /// 
    /// We have rebuilt the maths in there into our XygloXnaDrawableShape paradigm allowing it
    /// to be built and drawn whenever we call it from the generic code.
    /// 
    /// </summary>
    public class XygloSphere : XygloXnaDrawableShape
    {
        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = false;

        /// <summary>
        /// Radius of sphere
        /// </summary>
        float m_radius;

        /// <summary>
        /// Vertices in a circle
        /// </summary>
        protected int m_vertsInCircle = 50;

        /// <summary>
        /// Circles in a sphere
        /// </summary>
        protected int m_circsInSphere = 50;

        /// <summary>
        /// Total number of vertices in sphere
        /// </summary>
        protected int m_numVertices;

        /// <summary>
        /// Total number of indicies in sphere
        /// </summary>
        protected int m_numIndices;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        /// <param name="radius"></param>
        public XygloSphere(Color colour, BasicEffect effect, BrazilVector3 position, float radius)
        {
            m_colour = colour;
            m_effect = effect;
            m_position.X = position.X;
            m_position.Y = position.Y;
            m_position.Z = position.Z;
            m_radius = radius;
        }


        /// <summary>
        /// Override the getBoundingBox call
        /// </summary>
        /// <returns></returns>
        public override BoundingBox getBoundingBox()
        {
            Vector3 radius = new Vector3(m_radius, m_radius, m_radius);
            return new BoundingBox(m_position - radius, m_position + radius);
        }

        /// <summary>
        /// Build buffers - make this rerunnable but we only want to build this once
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            // Set total number of vertices
            //
            m_numVertices = m_vertsInCircle * m_circsInSphere; 

            // Total number of indices
            //
            m_numIndices = m_vertsInCircle * m_circsInSphere * 6;

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
            Vector3 rad = new Vector3((float)Math.Abs(m_radius), 0, 0);

            for (int x = 0; x < m_circsInSphere; x++)
            {
                float difx = 360.0f / (float)m_circsInSphere;

                for (int y = 0; y < m_vertsInCircle; y++)
                {
                    float dify = 360.0f / (float)m_vertsInCircle;

                    Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); // rotate vertex around z
                    Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); // rotate circle around y
                    Vector3 point = m_position + Vector3.Transform(Vector3.Transform(rad, zrot), yrot);// transformation

                    //m_vertices[x + y * m_vertsInCircle] = new VertexPositionColorTexture(point, m_colour, new Vector2(0, 0));
                    m_vertices[x + y * m_vertsInCircle].Position = point;
                    m_vertices[x + y * m_vertsInCircle].Color = m_colour;
                    m_vertices[x + y * m_vertsInCircle].TextureCoordinate = Vector2.Zero;
                }
            }
        }

        /// <summary>
        /// Creation of indices is a one off operation
        /// </summary>
        void createIndices(GraphicsDevice device)
        {
            if (m_indices == null)
            {
                m_indices = new short[m_numIndices];

                int i = 0;
                for (int x = 0; x < m_circsInSphere; x++)
                {
                    for (int y = 0; y < m_vertsInCircle; y++)
                    {
                        int s1 = x == (m_circsInSphere - 1) ? 0 : x + 1;
                        int s2 = y == (m_vertsInCircle - 1) ? 0 : y + 1;
                        short upperLeft = (short)(x * m_circsInSphere + y);
                        short upperRight = (short)(s1 * m_circsInSphere + y);
                        short lowerLeft = (short)(x * m_circsInSphere + s2);
                        short lowerRight = (short)(s1 * m_circsInSphere + s2);
                        m_indices[i++] = upperLeft;
                        m_indices[i++] = upperRight;
                        m_indices[i++] = lowerLeft;
                        m_indices[i++] = lowerLeft;
                        m_indices[i++] = upperRight;
                        m_indices[i++] = lowerRight;
                    }
                }

                m_indexBuffer = new IndexBuffer(device, typeof(short), m_numIndices, BufferUsage.WriteOnly);
                m_indexBuffer.SetData(m_indices);
            }
        }

        /// <summary>
        /// Draw this FlyingBlock by setting and swriting the 
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
            }
        }

        /*
        public void Draw(Camera cam) // the camera class contains the View and Projection Matrices
        {
            effect.View = cam.View;
            effect.Projection = cam.Projection;
            graphicd.RasterizerState = new RasterizerState() { FillMode = FillMode.WireFrame }; // Wireframe as in the picture
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicd.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, nvertices, indices, 0, indices.Length / 3);
            }
        }
         * */
    }
}
