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
    public class BufferViewEventArgs : System.EventArgs
    {
        public BufferViewEventArgs(BufferView bv)
        {
            m_bufferView = bv;
            m_setActiveBufferOnly = true;
        }

        public BufferViewEventArgs(BufferView bv, ScreenPosition sp, bool extendHighlight = false)
        {
            m_bufferView = bv;
            m_screenPosition = sp;
            m_extendHighlight = extendHighlight;
        }

        public BufferView getBufferView() { return m_bufferView; }
        public ScreenPosition getScreenPosition() { return m_screenPosition; }
        public bool isExtendingHighlight() { return m_extendHighlight; }
        public bool setActiveOnly() { return m_setActiveBufferOnly; }

        protected BufferView m_bufferView;
        protected ScreenPosition m_screenPosition;
        protected bool m_extendHighlight = false;
        protected bool m_setActiveBufferOnly = false;
    }

    /// <summary>
    /// Returned from XygloMouse
    /// </summary>
    public class NewBufferViewEventArgs : System.EventArgs
    {
        public NewBufferViewEventArgs(string filename, ScreenPosition sp)
        {
            m_filename = filename;
            m_screenPosition = sp;
        }

        public string getFileName() { return m_filename; }
        public ScreenPosition getScreenPosition() { return m_screenPosition; }

        protected string m_filename;
        protected ScreenPosition m_screenPosition;
    }


    // Declare some delegates
    //
    public delegate void PositionChangeEventHandler(object sender, PositionEventArgs e);
    public delegate void TemporaryMessageEventHandler(object sender, TextEventArgs e);
    public delegate void BufferViewChangeEventHandler(object sender, BufferViewEventArgs e);
    public delegate void EyeChangeEventHandler(object sender, PositionEventArgs eye, PositionEventArgs target);
    public delegate void NewBufferViewEventHandler(object sender, NewBufferViewEventArgs e);


}
