using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    public class XygloFlyingBlock : XygloXnaDrawable
    {
        /// <summary>
        /// Positional constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public XygloFlyingBlock(Color colour, BasicEffect effect, Vector3 size, Vector3 position)
        {
            // Store the effect
            m_effect = effect;
            m_colour = colour;

            m_blockSize = size;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Position and size
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        /// <param name="size"></param>
        public XygloFlyingBlock(Color colour, BasicEffect effect, BrazilVector3 position, BrazilVector3 size)
        {
            // Store the effect
            m_effect = effect;
            m_colour = colour;

            m_blockSize.X = size.X;
            m_blockSize.Y = size.Y;
            m_blockSize.Z = size.Z;

            m_position.X = position.X;
            m_position.Y = position.Y;
            m_position.Z = position.Z;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

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

                // Bottom
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
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_vertices.Length, 0, m_indices.Length / 3);

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
        /// Size of this block
        /// </summary>
        public Vector3 m_blockSize;

        /// <summary>
        /// All the vertices of the block
        /// </summary>
        protected VertexPositionColorTexture[] m_vertices = null;

        /// <summary>
        /// Indexes of all the vertices
        /// </summary>
        protected short[] m_indices = null;

        /// <summary>
        /// The index buffer
        /// </summary>
        protected IndexBuffer m_indexBuffer;

        /// <summary>
        /// The vertex buffer
        /// </summary>
        protected VertexBuffer m_vertexBuffer;

        /// <summary>
        /// Our texture
        /// </summary>
        public Texture2D m_shapeTexture;

        /// <summary>
        /// Alpha Blend text
        /// </summary>
        protected bool m_alphaBlendingTest = false;

    }
}
