using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Xyglo.Friendlier;


// Here are some event specialisations we use within the Xyglo.Xna code.
//
namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Position Event arguments
    /// </summary>
    public class PositionEventArgs : System.EventArgs
    {
        public PositionEventArgs(Vector3 newPosition)
        {
            m_position = newPosition;
        }

        public Vector3 getPosition()
        {
            return m_position;
        }

        protected Vector3 m_position;
    }


    /// <summary>
    /// Use this to set a temporary message
    /// </summary>
    public class TextEventArgs : System.EventArgs
    {
        public TextEventArgs(string text, double duration, GameTime gameTime)
        {
            m_text = text;
            m_duration = duration;
            m_gameTime = gameTime;
        }

        public string getText() { return m_text; }
        public double getDuration() { return m_duration; }
        public GameTime getGameTime() { return m_gameTime; }

        protected string m_text;

        protected double m_duration;

        protected GameTime m_gameTime;

    }

    /// <summary>
    /// Returned from XygloMouse - you can set the BufferView and ScreenPosition on the BufferView
    /// and also if the highlight is being extended or not.
    /// </summary>
    public class XygloViewEventArgs : System.EventArgs
    {
        public XygloViewEventArgs(XygloView view)
        {
            m_view = view;
            m_setActiveBufferOnly = true;
        }

        public XygloViewEventArgs(XygloView view, ScreenPosition sp, bool extendHighlight = false)
        {
            m_view = view;
            m_screenPosition = sp;
            m_extendHighlight = extendHighlight;
        }

        public XygloView getView() { return m_view; }
        public ScreenPosition getScreenPosition() { return m_screenPosition; }
        public bool isExtendingHighlight() { return m_extendHighlight; }
        public bool setActiveOnly() { return m_setActiveBufferOnly; }

        protected XygloView m_view;
        protected ScreenPosition m_screenPosition;
        protected bool m_extendHighlight = false;
        protected bool m_setActiveBufferOnly = false;
    }

    /// <summary>
    /// Mode for when we are adding a new XygloView
    /// </summary>
    public enum NewViewMode
    {
        ScreenPosition,    // Used to position an existing file
        Relative,          // Used when we're positioning a file that is to be opened
        NewBuffer,         // Used to relative position a new file
        Copy               // When copying a bufferview
    }

    /// <summary>
    /// Returned from XygloMouse
    /// </summary>
    public class NewViewEventArgs : System.EventArgs
    {
        public NewViewEventArgs(string filename, ScreenPosition sp, BufferView.ViewPosition position)
        {
            m_filename = filename;
            m_screenPosition = sp;
            m_viewPosition = position;
            m_mode = NewViewMode.ScreenPosition;
        }

        /// <summary>
        /// Second constructor for different mode when we are positioning a file that needs to be opened
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="position"></param>
        /// <param name="readOnly"></param>
        /// <param name="tailing"></param>
        public NewViewEventArgs(string filename, BufferView.ViewPosition position, bool readOnly, bool tailing)
        {
            m_filename = filename;
            m_viewPosition = position;
            m_readOnly = readOnly;
            m_isTailing = tailing;
            m_mode = NewViewMode.Relative;
        }

        public NewViewEventArgs(BufferView.ViewPosition position)
        {
            m_viewPosition = position;
            m_mode = NewViewMode.NewBuffer;
        }

        public NewViewEventArgs(FontManager fontManager, XygloView sourceView, BufferView.ViewPosition position, NewViewMode mode)
        {
            m_viewPosition = position;
            m_fontManager = fontManager;
            m_sourceView = sourceView;
            m_mode = mode;
        }

        public string getFileName() { return m_filename; }
        public ScreenPosition getScreenPosition() { return m_screenPosition; }
        public BufferView.ViewPosition getViewPosition() { return m_viewPosition; }
        public bool isReadOnly() { return m_readOnly; }
        public bool isTailing() { return m_isTailing; }
        public NewViewMode getMode() { return m_mode; }
        public FontManager getFontManager() { return m_fontManager; }
        public XygloView getSourceView() { return m_sourceView; }

        protected string m_filename;
        protected ScreenPosition m_screenPosition;
        protected BufferView.ViewPosition m_viewPosition;
        protected bool m_readOnly = false;
        protected bool m_isTailing = false;
        protected FontManager m_fontManager;
        protected XygloView m_sourceView;

        protected NewViewMode m_mode;
    }

    /// <summary>
    /// Take a gametime with an event
    /// </summary>
    public class CleanExitEventArgs : System.EventArgs
    {
        public CleanExitEventArgs(GameTime gameTime, bool forceExit = false)
        {
            m_gameTime = gameTime;
            m_forceExit = forceExit;
        }

        public GameTime getGameTime() { return m_gameTime; }
        public bool getForceExit() { return m_forceExit; }

        protected GameTime m_gameTime;
        protected bool m_forceExit = false;
    }

    /// <summary>
    /// A list of commands we can send to the engine
    /// </summary>
    public enum XygloCommand
    {
        Build,          // perform a build
        AlternateBuild, // perform alernate build
        XygloClient,    // activate a client
        XygloComponent, // activate a component
        UrhoExport      // export as Urho AngelScript
    }

    /// <summary>
    /// Callback event used to activate internal commands
    /// </summary>
    public class CommandEventArgs : System.EventArgs
    {
        public CommandEventArgs(GameTime gameTime, XygloCommand command, string arguments = "")
        {
            m_gameTime = gameTime;
            m_command = command;
            m_arguments = arguments;
        }

        public XygloCommand getCommand() { return m_command; }
        public string getArguments() { return m_arguments; }
        public GameTime getGameTime() { return m_gameTime; }

        protected XygloCommand m_command;
        protected string m_arguments;
        protected GameTime m_gameTime;
    }

    /// <summary>
    /// When loading a new project
    /// </summary>
    public class OpenProjectEventArgs : System.EventArgs
    {
        public OpenProjectEventArgs(string projectFile)
        {
            m_projectFile = projectFile;
        }

        public string getProjectFile()
        {
            return m_projectFile;
        }

        protected string m_projectFile;
    }

    /// <summary>
    /// All the details should be stored in the centrally located m_context.m_templateManager
    /// </summary>
    public class NewProjectEventArgs : System.EventArgs
    {
        public NewProjectEventArgs()
        {
        }
    }

    // Declare some delegates
    //
    public delegate void PositionChangeEventHandler(object sender, PositionEventArgs e);
    public delegate void TemporaryMessageEventHandler(object sender, TextEventArgs e);
    public delegate void XygloViewChangeEventHandler(object sender, XygloViewEventArgs e);
    public delegate void EyeChangeEventHandler(object sender, PositionEventArgs eye, PositionEventArgs target);
    public delegate void NewBufferViewEventHandler(object sender, NewViewEventArgs e);
    public delegate void CleanExitEventHandler(object sender, CleanExitEventArgs e);
    public delegate void CommandEventHandler(object sender, CommandEventArgs e);
    public delegate void OpenProjectEventHandler(object sender, OpenProjectEventArgs e);
    public delegate void NewProjectEventHandler(object sender, NewProjectEventArgs e);
}
