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
    /// Thanks to:
    /// 
    /// http://thver.blogspot.nl/2012/07/how-to-create-sphere-programmatically.html
    /// 
    /// </summary>
    public class XygloSphere : XygloXnaDrawableShape
    {
        //VertexPositionColor[] vertices; //later, I will provide another example with VertexPositionNormalTexture
        //VertexBuffer vbuffer;
        //short[] indices; //my laptop can only afford Reach, no HiDef :(
        //IndexBuffer ibuffer;
        //BasicEffect effect;
        //GraphicsDevice graphicd;

        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = false;

        /// <summary>
        /// Radius of sphere
        /// </summary>
        float m_radius;

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


        /*
        protected Vector3 m_blockSize = new Vector3(10, 10, 10);
        /// <summary>
        /// Build the shape and populate the Vertex and Index buffers
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            if (m_vertices == null)
            {
                m_vertices = new VertexPositionColorTexture[8];
                m_vertices[0].TextureCoordinate = new Vector2(0, 0);
                m_vertices[1].TextureCoordinate = new Vector2(0, 1);
                m_vertices[2].TextureCoordinate = new Vector2(1, 0);
                m_vertices[3].TextureCoordinate = new Vector2(1, 1);
                m_vertices[4].TextureCoordinate = new Vector2(1, 1);
                m_vertices[5].TextureCoordinate = new Vector2(0, 1);
                m_vertices[6].TextureCoordinate = new Vector2(1, 0);
                m_vertices[7].TextureCoordinate = new Vector2(0, 0);
            }

            Matrix worldMatrix = Matrix.CreateRotationX((float)m_rotation);

            // front left top
            //
            //m_vertices[0] = new VertexPositionColorTexture(m_position, m_colour, new Vector2(0, 0));
            m_vertices[0].Position = m_position + Vector3.Transform(new Vector3(-m_blockSize.X / 2, m_blockSize.Y / 2, m_blockSize.Z / 2), worldMatrix);
            m_vertices[0].Color = m_colour;

            // front left bottom
            //m_vertices[1] = new VertexPositionColorTexture(m_position + new Vector3(0, -m_blockSize.Y, 0), m_colour, new Vector2(0, 1));
            m_vertices[1].Position = m_position + Vector3.Transform(new Vector3(-m_blockSize.X / 2, -m_blockSize.Y / 2, m_blockSize.Z / 2), worldMatrix);
            m_vertices[1].Color = m_colour;

            // front right top
            //m_vertices[2] = new VertexPositionColorTexture(m_position + new Vector3(m_blockSize.X, 0, 0), m_colour, new Vector2(1, 0));
            m_vertices[2].Position = m_position + Vector3.Transform(new Vector3(m_blockSize.X / 2, m_blockSize.Y / 2, m_blockSize.Z / 2), worldMatrix);
            m_vertices[2].Color = m_colour;

            // front right bottom
            //m_vertice[3] = new VertexPositionColorTexture(m_position + new Vector3(m_blockSize.X, -m_blockSize.Y, 0), m_colour, new Vector2(1, 1));
            m_vertices[3].Position = m_position + Vector3.Transform(new Vector3(m_blockSize.X / 2, -m_blockSize.Y / 2, m_blockSize.Z / 2), worldMatrix);
            m_vertices[3].Color = m_colour;

            // back left top reversing the UV ordering for the back
            //m_vertices[4] = new VertexPositionColorTexture(m_position + new Vector3(0, 0, -m_blockSize.Z), m_colour, new Vector2(1, 1));
            m_vertices[4].Position = m_position + Vector3.Transform(new Vector3(-m_blockSize.X / 2, m_blockSize.Y / 2, -m_blockSize.Z / 2), worldMatrix);
            m_vertices[4].Color = m_colour;

            // back right top
            //m_vertices[5] = new VertexPositionColorTexture(m_position + new Vector3(m_blockSize.X, 0, -m_blockSize.Z), m_colour, new Vector2(0, 1));
            m_vertices[5].Position = m_position + Vector3.Transform(new Vector3(m_blockSize.X / 2, m_blockSize.Y / 2, -m_blockSize.Z / 2), worldMatrix);
            m_vertices[5].Color = m_colour;

            // back left bottom
            //m_vertices[6] = new VertexPositionColorTexture(m_position + new Vector3(0, -m_blockSize.Y, -m_blockSize.Z), m_colour, new Vector2(1, 0));
            m_vertices[6].Position = m_position + Vector3.Transform(new Vector3(-m_blockSize.X / 2, -m_blockSize.Y / 2, -m_blockSize.Z / 2), worldMatrix);
            m_vertices[6].Color = m_colour;

            // back right bottom
            //m_vertices[7] = new VertexPositionColorTexture(m_position + new Vector3(m_blockSize.X, -m_blockSize.Y, -m_blockSize.Z), m_colour, new Vector2(0, 0));
            m_vertices[7].Position = m_position + Vector3.Transform(new Vector3(m_blockSize.X / 2, -m_blockSize.Y / 2, -m_blockSize.Z / 2), worldMatrix);
            m_vertices[7].Color = m_colour;

            // Now we need to describe 32 vertices
            //
            m_vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), m_vertices.Count(), BufferUsage.WriteOnly);
            m_vertexBuffer.SetData(m_vertices);

            // Total number of indices - these don't change so only set them up once.
            // We're using 16 bit indices here (short) so that we don't hit any hardware limits
            // and this is of course plenty for us.
            //
            if (m_indices == null)
            {
                m_indices = new short[36];  // 2 triangles * 6 size * 3 points of a triangle

                // Front side
                //
                m_indices[0] = 0;
                m_indices[1] = 2;
                m_indices[2] = 3;

                m_indices[3] = 0;
                m_indices[4] = 3;
                m_indices[5] = 1;

                // Right side
                //
                m_indices[6] = 2;
                m_indices[7] = 7;
                m_indices[8] = 3;

                m_indices[9] = 2;
                m_indices[10] = 5;
                m_indices[11] = 7;

                // Back side
                //
                m_indices[12] = 5;
                m_indices[13] = 4;
                m_indices[14] = 6;

                m_indices[15] = 5;
                m_indices[16] = 6;
                m_indices[17] = 7;

                // Left side
                //
                m_indices[18] = 1;
                m_indices[19] = 6;
                m_indices[20] = 4;

                m_indices[21] = 4;
                m_indices[22] = 2;
                m_indices[23] = 1;

                // Top
                //
                m_indices[24] = 4;
                m_indices[25] = 2;
                m_indices[26] = 0;

                m_indices[27] = 2;
                m_indices[28] = 4;
                m_indices[29] = 5;

                // Bottom - might need to reverse these?
                //
                m_indices[30] = 1;
                m_indices[31] = 3;
                m_indices[32] = 6;

                m_indices[33] = 3;
                m_indices[34] = 7;
                m_indices[35] = 6;

                // Set the index buffer
                //
                m_indexBuffer = new IndexBuffer(device, typeof(short), m_indices.Length, BufferUsage.WriteOnly);
                m_indexBuffer.SetData(m_indices);
            }
        }
        */

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
        protected int nvertices;

        /// <summary>
        /// Total number of indicies in sphere
        /// </summary>
        protected int nindices;

        /// <summary>
        /// Build buffers - make this rerunnable
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            // Set total number of vertices
            //
            nvertices = m_vertsInCircle * m_circsInSphere; 

            // Total number of indices
            //
            nindices = m_vertsInCircle * m_circsInSphere * 6;

            if (m_vertices == null)
            {
                m_vertices = new VertexPositionColorTexture[nvertices];
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
                m_indices = new short[nindices];

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

                m_indexBuffer = new IndexBuffer(device, typeof(short), nindices, BufferUsage.WriteOnly);
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
