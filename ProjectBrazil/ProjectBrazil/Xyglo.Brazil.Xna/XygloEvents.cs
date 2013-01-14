using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;


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
    /// Returned from XygloMouse
    /// </summary>
    public class NewBufferViewEventArgs : System.EventArgs
    {
        public NewBufferViewEventArgs(string filename, ScreenPosition sp, BufferView.ViewPosition position)
        {
            m_filename = filename;
            m_screenPosition = sp;
            m_viewPosition = position;
        }

        public string getFileName() { return m_filename; }
        public ScreenPosition getScreenPosition() { return m_screenPosition; }
        public BufferView.ViewPosition getViewPosition() { return m_viewPosition; }

        protected string m_filename;
        protected ScreenPosition m_screenPosition;
        protected BufferView.ViewPosition m_viewPosition;
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
        XygloComponent  // activate a component
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


    // Declare some delegates
    //
    public delegate void PositionChangeEventHandler(object sender, PositionEventArgs e);
    public delegate void TemporaryMessageEventHandler(object sender, TextEventArgs e);
    public delegate void XygloViewChangeEventHandler(object sender, XygloViewEventArgs e);
    public delegate void EyeChangeEventHandler(object sender, PositionEventArgs eye, PositionEventArgs target);
    public delegate void NewBufferViewEventHandler(object sender, NewBufferViewEventArgs e);
    public delegate void CleanExitEventHandler(object sender, CleanExitEventArgs e);
    public delegate void CommandEventHandler(object sender, CommandEventArgs e);
}
