using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Leap;
using Xyglo.Brazil.Xna;

namespace Xyglo.Gesture
{
    /// <summary>
    /// A class to wrap our Leap interface
    /// </summary>
    public class LeapListener : Leap.Listener
    {
        public override void OnConnect(Controller arg0)
        {
            Logger.logMsg("Leap connected");
            //base.OnConnect(arg0);

            arg0.EnableGesture(Leap.Gesture.GestureType.TYPECIRCLE);
            arg0.EnableGesture(Leap.Gesture.GestureType.TYPEKEYTAP);
            arg0.EnableGesture(Leap.Gesture.GestureType.TYPESCREENTAP);
            arg0.EnableGesture(Leap.Gesture.GestureType.TYPESWIPE);

            // Vanilla?
            //
            m_mtxFrameTransform = new Matrix();

            m_fFrameScale = 10.0f; // 0.0075f;
            m_mtxFrameTransform.origin = new Vector(0.0f, -2.0f, 0.5f);
            m_fPointableRadius = 1.0f; // 0.05f;

        }

        public override void OnInit(Controller arg0)
        {
            Logger.logMsg("Initialising Leap");
            base.OnInit(arg0);
        }

        public override void OnDisconnect(Controller arg0)
        {
            Logger.logMsg("Disconnecting Leap");
            base.OnDisconnect(arg0);
        }

        public override void OnExit(Controller arg0)
        {
            Logger.logMsg("Leap Exiting");
            base.OnExit(arg0);
        }


        protected const int handGestureFrameInterval = 2;

        /// <summary>
        /// Called every frame that the leap is working
        /// </summary>
        /// <param name="arg0"></param>
        public override void OnFrame(Controller controller)
        {
            testFingers(controller);
        }

        /// <summary>
        /// Return if the gesture was recognised
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        protected bool processGestures(Controller controller)
        {
            Frame latestFrame = controller.Frame();
            Frame refFrame = controller.Frame(handGestureFrameInterval);
            Leap.GestureList gestures = latestFrame.Gestures();
            bool gotGesture = false;

            foreach (Leap.Gesture g in gestures)
            {
                switch (g.Type)
                {
                    case Leap.Gesture.GestureType.TYPESWIPE:   
                        SwipeGesture swipe = new SwipeGesture(g);
                        OnSwipe(new SwipeEventArgs(swipe));
                        gotGesture = true;
                        break;

                    case Leap.Gesture.GestureType.TYPESCREENTAP:
                       ScreenTapGesture screenTap = new ScreenTapGesture(g);
                        OnScreenTap(new ScreenTapEventArgs(screenTap));
                        gotGesture = true;
                        break;

                    case Leap.Gesture.GestureType.TYPECIRCLE:
                        Leap.CircleGesture circleGesture = new Leap.CircleGesture(g);

                        bool clockWise = (circleGesture.Pointable.Direction.AngleTo(circleGesture.Normal) <= Math.PI / 4);

                        if (clockWise)
                        {
                            Logger.logMsg("Circle gesture clockwise");
                        }
                        else
                        {
                            Logger.logMsg("Circle gesture anticlockwise");
                        }

                        OnCircle(new CircleEventArgs(circleGesture.Center, circleGesture.DurationSeconds, clockWise));

                        gotGesture = true;
                        break;

                    case Leap.Gesture.GestureType.TYPEKEYTAP:
                        Logger.logMsg("Key tap gesture");
                        gotGesture = true;
                        break;

                    case Leap.Gesture.GestureType.TYPEINVALID:
                        Logger.logMsg("Invalid gesture");
                        break;

                    default:
                        Logger.logMsg("Got other type");
                        break;
                }
            }
            return gotGesture;
        }

        protected void originalFingers(Controller controller)
        {
            Frame latestFrame = controller.Frame();
            Frame refFrame = controller.Frame(handGestureFrameInterval);
            //Frame frame = arg0.Frame();

            //if (frame.Hands.Empty)
            //return;


            /*
            if (refFrame.IsValid && latestFrame.Hands.Count() == 1) {
                Hand hand = latestFrame.Hands[0];
                // double scaleFactor = hand.scaleFactor(refFrame);
                // cout << "Scale: " << scaleFactor << endl;
            }*/


            ScreenList screens = controller.CalibratedScreens;
            //Leap::GestureList gestures = latestFrame.gestures();

            // Process gestures here and if we have one then return
            //
            if (processGestures(controller))
                return;

            // For the moment leave out the gestures
            //

            /*
            Leap.GestureList gestures = latestFrame.Gestures();

            foreach (Leap.Gesture g in gestures)
            {
                switch (g.Type)
                {
                    case Leap.Gesture.GestureType.TYPESWIPE:   
                        SwipeGesture swipe = new SwipeGesture(g);
                        OnSwipe(new SwipeEventArgs(swipe));
                        break;

                    case Leap.Gesture.GestureType.TYPESCREENTAP:
                       ScreenTapGesture screenTap = new ScreenTapGesture(g);
                        OnScreenTap(new ScreenTapEventArgs(screenTap));
                        break;

                    case Leap.Gesture.GestureType.TYPECIRCLE:
                        break;

                    case Leap.Gesture.GestureType.TYPEKEYTAP:
                        break;

                    case Leap.Gesture.GestureType.TYPEINVALID:
                        //Logger.logMsg(
                        break;

                    default:
                        Logger.logMsg("Got other type");
                        break;
                }

            }*/

            if (latestFrame.Pointables.Count == 1)
            {
                //Logger.logMsg("Got one pointable");

                Vector hitPoint = pointableScreenPos(latestFrame.Pointables[0], screens);
                Vector vStartPos = m_mtxFrameTransform.TransformPoint(latestFrame.Pointables[0].TipPosition * m_fFrameScale);
                Vector vEndPos = m_mtxFrameTransform.TransformDirection(latestFrame.Pointables[0].Direction) * -0.25f;
                OnScreenPosition(new ScreenPositionEventArgs(hitPoint, vStartPos, vEndPos, latestFrame.Pointables[0].Hand.ToString(), latestFrame.Pointables[0].Id));
                return;

                /*
                foreach(Leap.Pointable pntbl in latestFrame.Pointables)
                {
                    if (!pntbl.IsValid)
                        continue;

                    Screen hitScreen = arg0.CalibratedScreens.ClosestScreenHit(pntbl);

                    if (hitScreen != null)
                    {
                        Vector positionHit = hitScreen.Intersect(pntbl, false);
                        //Logger.logMsg("Position X = " + positionHit.x + ", Y = " + positionHit.y + ", Z = " + positionHit.z);
                        OnScreenPosition(new ScreenPositionEventArgs(positionHit));
                    }
                }*/
            }
        }

        protected void testFingers(Controller controller)
        {
                      // Get the current frame.
            Frame currentFrame = controller.Frame();

            m_currentTime = currentFrame.Timestamp;
            m_timeChange = m_currentTime - m_previousTime;

            // Process gestures here and if we have one then return
            //
            if (processGestures(controller))
                return;

            // Every 500us I guess
            //
            if (m_timeChange > 1000)
            {
                if (!currentFrame.Hands.Empty)
                {
                    // Get the first finger in the list of fingers
                    //Finger finger = currentFrame.Fingers[0];


                    foreach (Finger finger in currentFrame.Fingers)
                    {
                        // Get the closest screen intercepting a ray projecting from the finger
                        Screen screen = controller.CalibratedScreens.ClosestScreenHit(finger);

                        if (screen != null && screen.IsValid)
                        {
                            // Get the velocity of the finger tip
                            var tipVelocity = (int)finger.TipVelocity.Magnitude;


                            // Use tipVelocity to reduce jitters when attempting to hold
                            // the cursor steady
                            if (tipVelocity > 10)
                            {
                                var xScreenIntersect = screen.Intersect(finger, true).x;
                                var yScreenIntersect = screen.Intersect(finger, true).y;

                                if (xScreenIntersect.ToString() != "NaN")
                                {
                                    var x = (int)(xScreenIntersect * screen.WidthPixels);
                                    var y = (int)(screen.HeightPixels - (yScreenIntersect * screen.HeightPixels));

                                    /*
                                    Console.WriteLine("Screen intersect X: " + xScreenIntersect.ToString());
                                    Console.WriteLine("Screen intersect Y: " + yScreenIntersect.ToString());
                                    Console.WriteLine("Width pixels: " + screen.WidthPixels.ToString());
                                    Console.WriteLine("Height pixels: " + screen.HeightPixels.ToString());

                                    Console.WriteLine("\n");

                                    Console.WriteLine("x: " + x.ToString());
                                    Console.WriteLine("y: " + y.ToString());

                                    Console.WriteLine("\n");

                                    Console.WriteLine("Tip velocity: " + tipVelocity.ToString());
                                    */

                                    // Move the cursor
                                    //MouseCursor.MoveCursor(x, y);
                                    ScreenList screens = controller.CalibratedScreens;
                                    Vector hitPoint = pointableScreenPos(finger, screens);

                                    Vector vStartPos = m_mtxFrameTransform.TransformPoint(finger.TipPosition * m_fFrameScale );
                                    Vector vEndPos = m_mtxFrameTransform.TransformDirection(finger.Direction ) * -0.25f;

                                    OnScreenPosition(new ScreenPositionEventArgs(hitPoint, vStartPos, vEndPos, finger.Hand.ToString(), finger.Id));

                                    //Console.WriteLine("\n" + new String('=', 40) + "\n");
                                }

                            }
                        }
                    }
                }

                m_previousTime = m_currentTime;
            }
        }

        /// <summary>
        /// Get the screen position pointed at
        /// </summary>
        /// <param name="pointable"></param>
        /// <param name="screens"></param>
        /// <returns></returns>
        protected Vector pointableScreenPos(Pointable pointable, ScreenList screens)
        {
            // need to have screen info
            if (screens.Empty)
                return new Vector();
    
            // get point location
            // get screen associated with gesture
            Screen screen = screens.ClosestScreenHit(pointable);
    
            Vector cursorLoc = screen.Intersect(pointable, true);
            if (!cursorLoc.IsValid()) {
                //Logger.logMsg("Failed to find intersection");
                return new Vector();
            }
    
            float screenX = cursorLoc.x * screen.WidthPixels;
            float screenY = (1.0f - (float)cursorLoc.y) * screen.HeightPixels;
            // cout << "Screen tapped at " << screenX << "," << screenY << endl;

            return new Vector(screenX, screenY, 0);
        }

        /// <summary>
        /// Convenience method for using the event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSwipe(SwipeEventArgs e)
        {
            if (SwipeEvent != null) SwipeEvent(this, e);
        }

        protected virtual void OnScreenTap(ScreenTapEventArgs e)
        {
            if (ScreenTapEvent != null) ScreenTapEvent(this, e);
        }

        protected virtual void OnScreenPosition(ScreenPositionEventArgs e)
        {
            if (ScreenPositionEvent != null) ScreenPositionEvent(this, e);
        }

        protected virtual void OnCircle(CircleEventArgs e)
        {
            if (CircleEvent != null) CircleEvent(this, e);
        }

        /// <summary>
        /// This listener can emit a SwipeEvent
        /// </summary>
        public event SwipeEventHandler SwipeEvent;
        public event ScreenTapEventHandler ScreenTapEvent;
        public event ScreenPositionEventHandler ScreenPositionEvent;
        public event CircleEventHandler CircleEvent;

        /// <summary>
        /// 
        /// </summary>
        protected Matrix m_mtxFrameTransform;


        protected float m_fFrameScale;
        protected float m_fPointableRadius;

        /// <summary>
        /// Store some timing information from leap
        /// </summary>
        protected long m_currentTime;

        /// <summary>
        /// Preview time
        /// </summary>
        protected long m_previousTime;

        /// <summary>
        /// Ticks
        /// </summary>
        protected long m_timeChange;
    }
}
