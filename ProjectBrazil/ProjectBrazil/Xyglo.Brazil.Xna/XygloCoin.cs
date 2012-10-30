﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// A Xyglo Coin - two circles with something filling them up and a value written on it
    /// </summary>
    public class XygloCoin : XygloXnaDrawableShape
    {
        /// <summary>
        /// Radius of coin
        /// </summary>
        float m_radius;

        /// <summary>
        /// Vertices in a circle
        /// </summary>
        protected int m_vertsInCircle = 50;

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
        /// Number of circles in the coin
        /// </summary>
        protected int m_circlesInCoin = 2;

        /// <summary>
        /// Coin constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public XygloCoin(Color colour, BasicEffect effect, Vector3 position, float radius)
        {
            // Store the effect
            //
            m_effect = effect;
            m_colour = colour;

            m_radius = radius;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Build the shape and populate the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            // Set total number of vertices - two circle circumferences plus 2 centre points
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
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        protected void createVertices()
        {
            Vector3 rad = new Vector3((float)Math.Abs(m_radius), 0, 0);
            int maxVertex = 0;
            Vector3 point;
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
                    point = m_position + Vector3.Transform(Vector3.Transform(rad, zrot), yrot);// transformation

                    // Adjust the translation by a factor of the radius for the two coins
                    //
                    if (x == 0)
                    {
                        point.X += m_radius / 2.0f;
                    }
                    else // x == 1
                    {
                        point.X -= m_radius / 2.0f;
                    }

                    //m_vertices[x + y * m_vertsInCircle] = new VertexPositionColorTexture(point, m_colour, new Vector2(0, 0));
                    m_vertices[y + x * m_vertsInCircle].Position = point;
                    m_vertices[y + x * m_vertsInCircle].Color = m_colour;
                    m_vertices[y + x * m_vertsInCircle].TextureCoordinate = Vector2.Zero;
                    maxVertex = y + x * m_vertsInCircle;
                }
            }


            // Define the centre points
            point = m_position;
            point.X += m_radius / 2.0f;
            m_vertices[maxVertex + 1].Position = point;
            m_vertices[maxVertex + 1].Color = m_colour;
            m_vertices[maxVertex + 1].TextureCoordinate = Vector2.Zero;

            point = m_position;
            point.X -= m_radius / 2.0f;
            m_vertices[maxVertex + 2].Position = point;
            m_vertices[maxVertex + 2].Color = m_colour;
            m_vertices[maxVertex + 2].TextureCoordinate = Vector2.Zero;
        }

        /// <summary>
        /// Creation of indices is a one off operation
        /// </summary>
        void createIndices(GraphicsDevice device)
        {
            int centreFront = m_circlesInCoin * m_vertsInCircle;
            int centreBack = centreFront + 1;

            if (m_indices == null)
            {
                m_indices = new short[m_numIndices];
                
                int i = 0;
                //for (int x = 0; x < m_circlesInCoin; x++)
                //{
                    for (int y = 0; y < m_vertsInCircle; y++)
                    {
                        m_indices[i] = (short)centreFront;
                        //m_indices[m_vertsInCircle + i++] = (short)centreBack;
                        i++;
                        m_indices[i] = (short)((y + 1) % m_vertsInCircle);
                        //m_indices[m_vertsInCircle + i++] = (short)(2 * m_vertsInCircle - y);
                        i++;
                        
                        m_indices[i] = (short)(y);
                        //m_indices[m_vertsInCircle + i++] = (short)((2 * m_vertsInCircle - y - 1));
                        i++;
                    }

                //}
                /*
                for (int x = 0; x < m_circlesInCoin; x++)
                {
                    for (int y = 0; y < m_vertsInCircle; y++)
                    {
                        int s1 = x == (m_circlesInCoin - 1) ? 0 : x + 1;
                        int s2 = y == (m_vertsInCircle - 1) ? 0 : y + 1;
                        short upperLeft = (short)(x * m_circlesInCoin + y);
                        short upperRight = (short)(s1 * m_circlesInCoin + y);
                        short lowerLeft = (short)(x * m_circlesInCoin + s2);
                        short lowerRight = (short)(s1 * m_circlesInCoin + s2);
                        m_indices[i++] = upperLeft;
                        m_indices[i++] = upperRight;
                        m_indices[i++] = lowerLeft;
                        m_indices[i++] = lowerLeft;
                        m_indices[i++] = upperRight;
                        m_indices[i++] = lowerRight;

                        /////////
                        // Note - we also need to connect up the edges of the coin
                        /////////
                    }
                }*/

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
    }
}