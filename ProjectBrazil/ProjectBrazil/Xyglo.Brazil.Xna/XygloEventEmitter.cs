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
        public event BufferViewChangeEventHandler BufferViewChangeEvent;
        public event EyeChangeEventHandler EyeChangeEvent;
        public event NewBufferViewEventHandler NewBufferViewEvent;

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
        protected virtual void OnBufferViewChange(BufferViewEventArgs e)
        {
            if (BufferViewChangeEvent != null) BufferViewChangeEvent(this, e);
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
        protected virtual void OnNewBufferViewEvent(NewBufferViewEventArgs e)
        {
            if (NewBufferViewEvent != null) NewBufferViewEvent(this, e);
        }

    }
}
