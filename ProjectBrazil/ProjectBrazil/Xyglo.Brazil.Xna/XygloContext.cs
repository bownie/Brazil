using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{

    /// <summary>
    /// Xyglo generic context
    /// </summary>
    public class XygloContext
    {
        /// <summary>
        /// BloomComponent
        /// </summary>
        public BloomComponent m_bloom;

        // Current bloom settings index
        //
        public int m_bloomSettingsIndex = 0;

        /// <summary>
        /// Basic effect has with textures for fonts
        /// </summary>
        public BasicEffect m_basicEffect;

        /// <summary>
        /// Line effect has no textures
        /// </summary>
        public BasicEffect m_lineEffect;

        /// <summary>
        /// Physics Effect for test purposes
        /// </summary>
        public BasicEffect m_physicsEffect;

        /// <summary>
        /// XNA graphics device context
        /// </summary>
        public GraphicsDeviceManager m_graphics;

        /// <summary>
        /// File system view
        /// </summary>
        public FileSystemView m_fileSystemView;

        /// <summary>
        /// Font manager
        /// </summary>
        public FontManager m_fontManager;

        /// <summary>
        /// Component list from XygloXNA
        /// </summary>
        public List<Component> m_componentList;

        /// <summary>
        /// Frustrum
        /// </summary>
        public BoundingFrustum m_frustrum;

        /// <summary>
        /// XygloXNA view matrix
        /// </summary>
        public Matrix m_viewMatrix;

        /// <summary>
        /// XygloXNA projection matrix
        /// </summary>
        public Matrix m_projection;

        /// <summary>
        /// The XygloProject
        /// </summary>
        public Project m_project;

        /// <summary>
        /// A drawable, keyed component dictionary
        /// </summary>
        public Dictionary<Component, XygloXnaDrawable> m_drawableComponents = new Dictionary<Component, XygloXnaDrawable>();

        /// <summary>
        /// A list of temporary Drawables - everything on here must have a time to live set for them
        /// or a scope defined to get rid of them from this list.
        /// </summary>
        public Dictionary<BrazilTemporary, XygloXnaDrawable> m_temporaryDrawables = new Dictionary<BrazilTemporary, XygloXnaDrawable>();

        /// <summary>
        /// Source BufferView when doing a drag of text etc
        /// </summary>
        public BufferView m_sourceBufferView = null;

        /// <summary>
        /// One SpriteBatch
        /// </summary>
        public SpriteBatch m_spriteBatch;

        /// <summary>
        /// Current Z position - we call it m_zoomLevel
        /// </summary>
        public float m_zoomLevel = 500.0f;

        /// <summary>
        /// Step for zooming
        /// </summary>
        public float m_zoomStep = 50.0f;

        /// <summary>
        /// Set field of view of the camera in radians
        /// </summary>
        public float m_fov = MathHelper.PiOver4;

        /// <summary>
        /// A helper class for drawing things
        /// </summary>
        public DrawingHelper m_drawingHelper;

        /// <summary>
        /// Keep a copy of GameTime somewhere central
        /// </summary>
        public GameTime m_gameTime;

        /// <summary>
        /// Another SpriteBatch for the overlay
        /// </summary>
        public SpriteBatch m_overlaySpriteBatch;

        /// <summary>
        /// A third SpriteBatch for panners/differs etc utilising alpha
        /// </summary>
        public SpriteBatch m_pannerSpriteBatch;

        /// <summary>
        /// A flat texture we use for drawing coloured blobs like highlighting and cursors
        /// </summary>
        public Texture2D m_flatTexture;

        /// <summary>
        /// A rendertarget for the text scroller
        /// </summary>
        public RenderTarget2D m_textScroller;

        /// <summary>
        /// A texture we can render a text string to and scroll
        /// </summary>
        public Texture2D m_textScrollTexture;

        /// <summary>
        /// Splash screen texture
        /// </summary>
        public Texture2D m_splashScreen;
    }
}
