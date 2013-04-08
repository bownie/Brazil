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
        protected bool m_alphaBlendingTest = true;

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
        /// Cursor offset from position - uses font size to position the menu
        /// </summary>
        protected Vector2 m_cursorOffset = Vector2.Zero;

        /// <summary>
        /// Size of the font we're using
        /// </summary>
        XygloView.ViewSize m_fontSize = XygloView.ViewSize.Medium;

        /// <summary>
        /// Menu constructor
        /// </summary>
        /// <param name="colour"></param>
        /// <param name="effect"></param>
        /// <param name="position"></param>
        public XygloMenu(FontManager fontManager, SpriteBatch spriteBatch, Color colour, BasicEffect effect, Vector3 position, Vector2 cursorOffset, XygloView.ViewSize viewFontSize)
        {
            m_fontSize = viewFontSize;
            m_fontManager = fontManager;
            m_spriteBatch = spriteBatch;

            // Convert cursor offset to screen offset
            //
            m_cursorOffset.X = cursorOffset.X * m_fontManager.getViewFont(m_fontSize).MeasureString("X").X;
            m_cursorOffset.Y = cursorOffset.Y * m_fontManager.getViewFont(m_fontSize).LineSpacing;

            // Store the effect
            //
            m_effect = effect;
            m_colour = colour;
            m_position = position;

            if (m_alphaBlendingTest) m_colour.A = 10;
        }

        /// <summary>
        /// Set the cursor offset
        /// </summary>
        /// <param name="screenPosition"></param>
        public void setOffset(ScreenPosition screenPosition)
        {
            m_cursorOffset.X = screenPosition.X * m_fontManager.getViewFont(m_fontSize).MeasureString("X").X;
            m_cursorOffset.Y = screenPosition.Y * m_fontManager.getViewFont(m_fontSize).LineSpacing;
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
        /// Build the shape and populate the Vertex and Index buffers.  Needs to be called after any change
        /// to position.
        /// </summary>
        /// <param name="device"></param>
        public override void buildBuffers(GraphicsDevice device)
        {
            // Set total number of vertices - two circle circumferences plus 2 centre points
            //
            m_numVertices = 4;

            // Total number of indices
            //
            m_numIndices = 6;

            if (m_vertices == null)
            {
                m_vertices = new VertexPositionColorTexture[m_numVertices];
                m_vertices[0].TextureCoordinate = new Vector2(0, 0);
                m_vertices[1].TextureCoordinate = new Vector2(0, 1);
                m_vertices[2].TextureCoordinate = new Vector2(1, 0);
                m_vertices[3].TextureCoordinate = new Vector2(1, 1);
            }

            if (m_vertexBuffer == null)
            {
                m_vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), m_vertices.Count(), BufferUsage.WriteOnly);
            }

            createVertices(device);
            createIndices(device);
        }

        /// <summary>
        /// Create the vertices
        /// </summary>
        protected void createVertices(GraphicsDevice device)
        {
            int longest = 0;
            foreach (string item in m_options)
            {
                if (item.Length > longest)
                    longest = item.Length;
            }

            float width = ( longest + 2 ) * m_fontManager.getViewFont(m_fontSize).MeasureString("X").X;
            float height = ( m_options.Count + 1 ) * m_fontManager.getViewFont(m_fontSize).LineSpacing;

            // Do this twice for the two coins
            //
            //m_vertices[0].Position = Vector3.Transform(new Vector3(m_position.X, m_position.Y, 0), Matrix.Invert(m_effect.World));
            m_vertices[0].Position = new Vector3(m_position.X + m_cursorOffset.X, m_position.Y + m_cursorOffset.Y, 0);
            m_vertices[0].Color = Color.DarkBlue;

            //m_vertices[1].Position = Vector3.Transform(new Vector3(m_position.X + width, m_position.Y, 0), Matrix.Invert(m_effect.World));
            m_vertices[1].Position = new Vector3(m_position.X + m_cursorOffset.X + width, m_position.Y + m_cursorOffset.Y, 0);
            m_vertices[1].Color = Color.DarkBlue;

            //m_vertices[2].Position = Vector3.Transform(new Vector3(m_position.X + width, m_position.Y + height, 0), Matrix.Invert(m_effect.World));
            m_vertices[2].Position = new Vector3(m_position.X + m_cursorOffset.X + width, m_position.Y + m_cursorOffset.Y + height, 0);
            m_vertices[2].Color = Color.DarkBlue;

            //m_vertices[3].Position = Vector3.Transform(new Vector3(m_position.X, m_position.Y + height, 0), Matrix.Invert(m_effect.World));
            m_vertices[3].Position = new Vector3(m_position.X + m_cursorOffset.X, m_position.Y + m_cursorOffset.Y + height, 0);
            m_vertices[3].Color = Color.DarkBlue;

            // Push the vertex buffer
            //
            m_vertexBuffer = new VertexBuffer(device, typeof(VertexPositionColorTexture), m_vertices.Count(), BufferUsage.None);
            m_vertexBuffer.SetData(m_vertices);
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

            // Set the index buffer
            //
            m_indexBuffer = new IndexBuffer(device, typeof(short), m_indices.Length, BufferUsage.WriteOnly);
            m_indexBuffer.SetData(m_indices);
        }

        /// <summary>
        /// Draw this XygloCoin
        /// </summary>
        /// <param name="device"></param>
        public override void draw(GraphicsDevice device, FillMode fillMode = FillMode.Solid)
        {
            // See here for information on restoring the renderstate:
            //
            // http://blogs.msdn.com/b/shawnhar/archive/2006/11/13/spritebatch-and-renderstates.aspx
            //
            device.Indices = m_indexBuffer;
            device.SetVertexBuffer(m_vertexBuffer);

            if (m_alphaBlendingTest) device.BlendState = BlendState.AlphaBlend;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.FillMode = fillMode;
            device.RasterizerState = rasterizerState;

            // Disable the texture
            //
            m_effect.TextureEnabled = false;
            foreach (EffectPass pass in m_effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                // IndexedPrimitives
                //
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_vertices.Length, 0, m_indices.Length / 3);

                //device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, m_vertices, 0, m_vertices.Length, m_indices, 0, m_indices.Length / 3);
                // Attempt
                //device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList, m_vertices, 0, m_vertices.Length);
            }


            // Reenable the texture
            //
            m_effect.TextureEnabled = true;

            float textScale = 1.0f;

            m_spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, RasterizerState.CullNone, m_effect);

            int i = 0;
            foreach(string item in m_options)
            {
                //string remainder = line.Substring(xPos, line.Length - xPos);

                m_spriteBatch.DrawString(
                    m_fontManager.getViewFont(m_fontSize),
                    item,
                    new Vector2(m_position.X + m_cursorOffset.X, m_position.Y + m_cursorOffset.Y + m_fontManager.getViewFont(m_fontSize).LineSpacing * i++),
                    m_colour,
                    0,
                    Vector2.Zero,
                    m_fontManager.getTextScale() * (float)textScale,
                    0,
                    0);
            }

            m_spriteBatch.End();
        }

        public override void drawPreview(GraphicsDevice device, BoundingBox fullBoundingBox, BoundingBox previewBoundingBox, Texture2D texture)
        {
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
        
        /// <summary>
        /// The BoundingSphere for this coin
        /// </summary>
        /// <returns></returns>
        //public BoundingSphere getBoundingSphere()
        //{
            //return new BoundingSphere(m_position, m_radius);
        //}

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

    }
}
