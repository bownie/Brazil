using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xyglo.Brazil.Xna
{

    /// <summary>
    /// Xyglo graphics and input state context
    /// </summary>
    public class XygloContext
    {
        /// <summary>
        /// The state of our application - what we're doing at the moment
        /// </summary>
        public State m_state;

        /// <summary>
        /// Basic effect has with textures for fonts
        /// </summary>
        public BasicEffect m_basicEffect;

        /// <summary>
        /// Line effect has no textures
        /// </summary>
        public BasicEffect m_lineEffect;

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
        /// Is shift down?
        /// </summary>
        public bool m_shiftDown;

        /// <summary>
        /// Is control down?
        /// </summary>
        public bool m_ctrlDown;

        /// <summary>
        /// Is alt down?
        /// </summary>
        public bool m_altDown;

        /// <summary>
        /// Is Windows key down?
        /// </summary>
        public bool m_windowsDown;

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
        /// BrazilWorld holds some Worldly information for us
        /// </summary>
        public BrazilWorld m_world = null;

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
        /// Get the list of States from the BrazilApp
        /// </summary>
        public List<State> m_states = null;

        /// <summary>
        /// Get the list of Targets from the BrazilApp
        /// </summary>
        public List<Target> m_targets = null;

        /// <summary>
        /// A helper class for drawing things
        /// </summary>
        public DrawingHelper m_drawingHelper;
    }
}
