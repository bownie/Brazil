﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{
    public class XygloFlyingBlock : XygloXnaDrawableShape
    {
        /// <summary>
        /// Use this constructor usually when we don't want to draw it yet - for a temp drawable which is going
        /// to be start/end point based rather than a static or semi-static shape.
        /// </summary>
        /// <param name="color"></param>
        /// <param name="?"></param>
        public XygloFlyingBlock(Color colour, BasicEffect effect)
        {
            m_colour = colour;
            m_effect = effect;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Positional constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="size"></param>
        /// <param name="position"></param>
        public XygloFlyingBlock(Color colour, BasicEffect effect, Vector3 position, Vector3 size)
        {
            // Store the effect
            m_effect = effect;
            m_colour = colour;

            m_blockSize = size;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /*
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
        }*/

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

            Matrix worldMatrix = Matrix.CreateRotationZ((float)m_rotation) * m_orientation;

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
            if (m_vertexBuffer == null)
                m_vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), m_vertices.Count(), BufferUsage.None);

            // Always reset the data
            //
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
                m_indices[19] = 4;
                m_indices[20] = 6;

                m_indices[21] = 1;
                m_indices[22] = 0;
                m_indices[23] = 4;

                // Top
                //
                m_indices[24] = 4;
                m_indices[25] = 2;
                m_indices[26] = 0;

                m_indices[27] = 4;
                m_indices[28] = 5;
                m_indices[29] = 2;

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


        /// <summary>
        /// Draw this FlyingBlock by setting and swriting the 
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device, FillMode fillMode = FillMode.Solid)
        {
            device.Indices = m_indexBuffer;
            device.SetVertexBuffer(m_vertexBuffer);

            if (m_alphaBlendingTest) device.BlendState = BlendState.AlphaBlend;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.FillMode = fillMode;
            device.RasterizerState = rasterizerState;

            foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                //device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_vertices.Length, 0, m_indices.Length / 3);
            }
        }

        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture)
        {
        }

        /// <summary>
        /// Polygons in this item - see this:
        /// 
        /// http://xboxforums.create.msdn.com/forums/p/23549/126997.aspx
        /// </summary>
        /// <returns></returns>
        public override int getPolygonCount()
        {
            return m_indices.Count() / 3;
        }

        /*
        /// <summary>
        /// Return the VertexBuffer
        /// </summary>
        /// <returns></returns>
        public VertexBuffer getVertexBuffer()
        {
            return m_vertexBuffer;
        }*/

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

        public Vector3 getSize() { return m_blockSize; }

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

    }
}
