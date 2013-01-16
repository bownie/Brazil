using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Provide support for smoothly changing eye position
    /// </summary>
    public class EyeHandler
    {
        public EyeHandler(XygloContext context, XygloKeyboardHandler keyboardHandler)
        {
            m_context = context;
            m_keyboardHandler = keyboardHandler;
        }

        /// <summary>
        /// Move the eye to a new position - store the original one so we can tell how far along the path we've moved
        /// </summary>
        /// <param name="newPosition"></param>
        public void flyToPosition(Vector3 newPosition)
        {
            // If we're already changing eye position then change the new eye position only
            //
            if (m_changingEyePosition)
            {
                // Set new destination
                //
                m_newEyePosition = newPosition;

                // Modify the vectors we need to aim to the new target
                //
                m_vFly = (m_newEyePosition - m_eye) / m_flySteps;

                // Also set up the target modification vector (where the eye is looking)
                //
                Vector3 tempTarget = m_newEyePosition;
                tempTarget.Z = 0.0f;
                m_vFlyTarget = (tempTarget - m_target) / m_flySteps;
            }
            else
            {
                // If we're currently stationary then we start moving like this
                //
                m_originalEyePosition = m_eye;
                m_newEyePosition = newPosition;
                m_changingPositionLastGameTime = TimeSpan.Zero;
                m_changingEyePosition = true;
            }
        }

        /// <summary>
        /// Transform current eye position to an intended eye position over time.  We use the orignal eye position to enable us
        /// to accelerate and deccelerate.  We have to modify both the eye position and the target that the eye is looking at
        /// by the same amount to keep our orientation constant.
        /// </summary>
        /// <param name="delta"></param>
        public void changeEyePosition(GameTime gameTime)
        {
            if (!m_changingEyePosition) return;

            // Start of 
            if (m_changingPositionLastGameTime == TimeSpan.Zero)
            {
                m_vFly = (m_newEyePosition - m_eye) / m_flySteps;

                // Also set up the target modification vector (where the eye is looking)
                //
                Vector3 tempTarget = m_newEyePosition;
                tempTarget.Z = 0.0f;
                m_vFlyTarget = (tempTarget - m_target) / m_flySteps;

                // Want to enforce a minimum vector size here
                //
                while (m_vFlyTarget.Length() != 0 && m_vFlyTarget.Length() < 10.0f)
                {
                    m_vFlyTarget *= 2;
                }

                m_changingPositionLastGameTime = gameTime.TotalGameTime;
            }

            // At this point we have the m_vFly and m_vFlyTarget vectors loaded
            // with a fraction of the distance from source to target.  To begin
            // with we need to start slowly and accelerate smoothly.  We use an
            // acceleration (acc) which is based on the distance from source to 
            // target.  This is used as a multiplier on the movement vector to
            // provide the acceleration within bounds set by Max and Min.
            //
            float acc = 1.0f;
            float percTrack = (m_eye - m_originalEyePosition).Length() / (m_newEyePosition - m_originalEyePosition).Length();

            bool enableAcceleration = false;

            if (enableAcceleration)
            {
                // Need a notion of distance for the next movement
                //
                if (m_eye != m_originalEyePosition)
                {
                    if (percTrack < 0.5)
                    {
                        acc = percTrack;
                    }
                    else
                    {
                        acc = 1.0f - percTrack;
                    }

                    // Set absolute limits on acceleration
                    //
                    acc = Math.Max(acc, 0.12f);
                    acc = Math.Min(acc, 1.0f);
                }
            }

            // Perform movement of the eye by the movement vector and acceleration
            //
            if (gameTime.TotalGameTime - m_changingPositionLastGameTime > m_movementPause)
            {
                m_eye += m_vFly * acc;

                // modify target by the other vector (this is to keep our eye level constan
                //
                m_target.X += m_vFlyTarget.X * acc;
                m_target.Y += m_vFlyTarget.Y * acc;

                m_changingPositionLastGameTime = gameTime.TotalGameTime;
            }

            // Font scaling
            //
            m_keyboardHandler.doFontScaling(acc);

            // Test arrival of the eye at destination position
            //
            m_testArrived.Center = m_newEyePosition;
            m_testArrived.Radius = 5.0f;
            m_testArrived.Contains(ref m_eye, out m_testResult);

            if (m_testResult == ContainmentType.Contains)
            {
                m_eye = m_newEyePosition;
                m_target.X = m_newEyePosition.X;
                m_target.Y = m_newEyePosition.Y;
                m_changingEyePosition = false;
                m_keyboardHandler.setCurrentFontScale(1.0f);
            }
            /*
        else
        {
            float distanceToTarget = (m_newEyePosition - m_eye).Length();
            float distanceToTargetNext = (m_newEyePosition - m_eye + (m_vFly * acc)).Length();
            // Check for overshoots
            //
            //if (((m_newEyePosition - m_eye + (m_vFly * acc)).Length() > (m_newEyePosition - m_eye).Length()))
            if (distanceToTargetNext > distanceToTarget)
            {
                Logger.logMsg("OVERSHOT");
            }
        }*/
        }

        public void setEyePosition(Vector3 eye) { m_eye = eye; }
        public Vector3 getEyePosition() { return m_eye; }

        public void setTargetPosition(Vector3 position) { m_target = position; }
        public Vector3 getTargetPosition() { return m_target; }

        public bool isChangingPosition() { return m_changingEyePosition; }


        /// <summary>
        /// XygloContext
        /// </summary>
        protected XygloContext m_context;

        /// <summary>
        /// Keyboard handler
        /// </summary>
        protected XygloKeyboardHandler m_keyboardHandler;

        /// <summary>
        /// Eye/Camera location
        /// </summary>
        protected Vector3 m_eye = new Vector3(0f, 0f, 500f);  // 275 is good

        /// <summary>
        /// Camera target
        /// </summary>
        protected Vector3 m_target;

        /// <summary>
        /// The new destination for our Eye position
        /// </summary>
        protected Vector3 m_newEyePosition;

        /// <summary>
        /// Original eye position - we know where we came from
        /// </summary>
        protected Vector3 m_originalEyePosition;

        /// <summary>
        /// Eye acceleration vector
        /// </summary>
        protected Vector3 m_eyeAcc = Vector3.Zero;

        /// <summary>
        /// Eye velocity vector
        /// </summary>
        protected Vector3 m_eyeVely = Vector3.Zero;

        /// <summary>
        /// Are we changing eye position?
        /// </summary>
        protected bool m_changingEyePosition = false;

        /// <summary>
        /// Used when changing the eye position - movement timer
        /// </summary>
        protected TimeSpan m_changingPositionLastGameTime;

        /// <summary>
        /// Frame rate of animation when moving between eye positions
        /// </summary>
        protected TimeSpan m_movementPause = new TimeSpan(0, 0, 0, 0, 10);

        /// <summary>
        /// This is the vector we're flying in - used to increment position each frame when
        /// moving between eye positions.
        /// </summary>
        protected Vector3 m_vFly;

        /// <summary>
        /// If our target position is not centred below our eye then we also have a vector here we need to
        /// modify.
        /// </summary>
        protected Vector3 m_vFlyTarget;

        /// <summary>
        /// How many steps between eye start and eye end fly position
        /// </summary>
        protected int m_flySteps = 10;

        /// <summary>
        /// Testing whether arrived in bounding sphere
        /// </summary>
        protected BoundingSphere m_testArrived = new BoundingSphere();

        /// <summary>
        /// Test result
        /// </summary>
        protected ContainmentType m_testResult;
    }
}
