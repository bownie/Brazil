using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// This class can be subclassed to provide an easy interface to event emitting
    /// </summary>
    public class XygloEventEmitter
    {

        public event PositionChangeEventHandler ChangePositionEvent;
        public event TemporaryMessageEventHandler TemporaryMessageEvent;
        public event XygloViewChangeEventHandler XygloViewChangeEvent;
        public event EyeChangeEventHandler EyeChangeEvent;
        public event NewBufferViewEventHandler NewBufferViewEvent;
        public event CleanExitEventHandler CleanExitEvent;
        public event CommandEventHandler CommandEvent;
        public event OpenProjectEventHandler OpenProjectEvent;
        public event NewProjectEventHandler NewProjectEvent;

        /// <summary>
        /// Convenience method for using the event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnChangePosition(PositionEventArgs e)
        {
            if (ChangePositionEvent != null) ChangePositionEvent(this, e);
        }

        /// <summary>
        /// Convenience method for emitting temporary message change
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnTemporaryMessage(TextEventArgs e)
        {
            if (TemporaryMessageEvent != null) TemporaryMessageEvent(this, e);
        }

        /// <summary>
        /// Convenience function for emitting BufferViewChange event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnViewChange(XygloViewEventArgs e)
        {
            if (XygloViewChangeEvent != null) XygloViewChangeEvent(this, e);
        }

        /// <summary>
        /// Eye change event
        /// </summary>
        /// <param name="eye"></param>
        /// <param name="target"></param>
        protected virtual void OnEyeChangeEvent(PositionEventArgs eye, PositionEventArgs target)
        {
            if (EyeChangeEvent != null) EyeChangeEvent(this, eye, target);
        }

        /// <summary>
        /// New BufferView
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnNewBufferViewEvent(NewViewEventArgs e)
        {
            if (NewBufferViewEvent != null) NewBufferViewEvent(this, e);
        }

        /// <summary>
        ///  Clean exit event.  This calls checkExit() at the XygloXNA level.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCleanExitEvent(CleanExitEventArgs e)
        {
            if (CleanExitEvent != null) CleanExitEvent(this, e);
        }

        /// <summary>
        /// A Command event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnCommandEvent(CommandEventArgs e)
        {
            if (CommandEvent != null) CommandEvent(this, e);
        }

        /// <summary>
        /// New Project event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOpenProjectEvent(OpenProjectEventArgs e)
        {
            if (OpenProjectEvent != null) OpenProjectEvent(this, e);
        }

        protected virtual void OnNewProjectEvent(NewProjectEventArgs e)
        {
            if (NewProjectEvent != null) NewProjectEvent(this, e);
        }
    }
}
