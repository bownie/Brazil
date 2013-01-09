using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna.Physics
{
    /*
    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }

*/
    public class QuadDrawer : DrawableGameComponent
    {
        private Texture2D texture;
        //private BasicEffect effect;

        private float size = 100.0f;

        private VertexPositionNormalTexture[] vertices;
        private short[] indices;

        protected XygloContext m_context;

        public QuadDrawer(Game game, XygloContext context, float size)
            : base(game)
        {
            this.size = size;
            m_context = context;
        }

        public override void Initialize()
        {
            BuildVertices();
            base.Initialize();
        }

        private void BuildVertices()
        {
            vertices = new VertexPositionNormalTexture[4];
            indices = new short[6];

            vertices[0].Position = Vector3.Forward + Vector3.Left;
            vertices[0].TextureCoordinate = new Vector2(0.0f, 1.0f);
            vertices[1].Position = Vector3.Backward + Vector3.Left;
            vertices[1].TextureCoordinate = new Vector2(0.0f, 0.0f);
            vertices[2].Position = Vector3.Forward + Vector3.Right;
            vertices[2].TextureCoordinate = new Vector2(1.0f, 1.0f);
            vertices[3].Position = Vector3.Backward + Vector3.Right;
            vertices[3].TextureCoordinate = new Vector2(1.0f, 0.0f);

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Normal = Vector3.Up;
                vertices[i].Position *= size;
                vertices[i].TextureCoordinate *= size;
            }

            indices[5] = 0; indices[4] = 1; indices[3] = 2;
            indices[2] = 2; indices[1] = 1; indices[0] = 3;
        }

        protected override void LoadContent()
        {


//            effect = new BasicEffect(this.GraphicsDevice);
            //effect.EnableDefaultLighting();
            //effect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);

            //effect.World = Matrix.CreateScale(1, -1, 1); // Matrix.Identity;
            //effect.TextureEnabled = true;

            //effect.Texture = texture;

            base.LoadContent();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override void Draw(GameTime gameTime)
        {
            Game demo = this.Game; // as JitterDemo;

            //GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            m_context.m_graphics.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;
            m_context.m_graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //effect.View = m_context.m_viewMatrix;
            //effect.Projection = m_context.m_projection;

            m_context.m_physicsEffect.View = m_context.m_viewMatrix;
            m_context.m_physicsEffect.Projection = m_context.m_projection;

            //foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            foreach (EffectPass pass in m_context.m_physicsEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawUserIndexedPrimitives
                    <VertexPositionNormalTexture>(PrimitiveType.TriangleList,
                    vertices, 0, 4, indices, 0, 2);
            }

            base.Draw(gameTime);
        }
    }
}
