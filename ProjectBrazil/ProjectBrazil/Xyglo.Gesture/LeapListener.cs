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

            //arg0.EnableGesture(Leap.Gesture.GestureType.TYPECIRCLE);
            //arg0.EnableGesture(Leap.Gesture.GestureType.TYPEKEYTAP);
            arg0.EnableGesture(Leap.Gesture.GestureType.TYPESCREENTAP);
            arg0.EnableGesture(Leap.Gesture.GestureType.TYPESWIPE);
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
                OnScreenPosition(new ScreenPositionEventArgs(hitPoint, latestFrame.Pointables[0].Id));
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

        private long m_currentTime;
        private long m_previousTime;
        private long m_timeChange;

        protected void testFingers(Controller cntrlr)
        {
                      // Get the current frame.
            Frame currentFrame = cntrlr.Frame();

            m_currentTime = currentFrame.Timestamp;
            m_timeChange = m_currentTime - m_previousTime;

            if (m_timeChange > 1000)
            {
                if (!currentFrame.Hands.Empty)
                {
                    // Get the first finger in the list of fingers
                    //Finger finger = currentFrame.Fingers[0];


                    foreach (Finger finger in currentFrame.Fingers)
                    {
                        // Get the closest screen intercepting a ray projecting from the finger
                        Screen screen = cntrlr.CalibratedScreens.ClosestScreenHit(finger);

                        if (screen != null && screen.IsValid)
                        {
                            // Get the velocity of the finger tip
                            var tipVelocity = (int)finger.TipVelocity.Magnitude;


                            // Use tipVelocity to reduce jitters when attempting to hold
                            // the cursor steady
                            if (tipVelocity > 25)
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
                                    ScreenList screens = cntrlr.CalibratedScreens;
                                    Vector hitPoint = pointableScreenPos(finger, screens);
                                    OnScreenPosition(new ScreenPositionEventArgs(hitPoint, finger.Id));

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

        /// <summary>
        /// This listener can emit a SwipeEvent
        /// </summary>
        public event SwipeEventHandler SwipeEvent;
        public event ScreenTapEventHandler ScreenTapEvent;
        public event ScreenPositionEventHandler ScreenPositionEvent;

        #region ORIGINAL_CPP
        /*
#include <iostream>
#include "Leap.h"
using namespace Leap;

class SampleListener : public Listener {
  public:
    virtual void onInit(const Controller&);
    virtual void onConnect(const Controller&);
    virtual void onDisconnect(const Controller&);
    virtual void onExit(const Controller&);
    virtual void onFrame(const Controller&);
};

void SampleListener::onInit(const Controller& controller) {
  std::cout << "Initialized" << std::endl;
}

void SampleListener::onConnect(const Controller& controller) {
  std::cout << "Connected" << std::endl;
  controller.enableGesture(Gesture::TYPE_CIRCLE);
  controller.enableGesture(Gesture::TYPE_KEY_TAP);
  controller.enableGesture(Gesture::TYPE_SCREEN_TAP);
  controller.enableGesture(Gesture::TYPE_SWIPE);
}

void SampleListener::onDisconnect(const Controller& controller) {
  std::cout << "Disconnected" << std::endl;
}

void SampleListener::onExit(const Controller& controller) {
  std::cout << "Exited" << std::endl;
}

void SampleListener::onFrame(const Controller& controller) {
  // Get the most recent frame and report some basic information
  const Frame frame = controller.frame();
  std::cout << "Frame id: " << frame.id()
            << ", timestamp: " << frame.timestamp()
            << ", hands: " << frame.hands().count()
            << ", fingers: " << frame.fingers().count()
            << ", tools: " << frame.tools().count()
            << ", gestures: " << frame.gestures().count() << std::endl;

  if (!frame.hands().empty()) {
    // Get the first hand
    const Hand hand = frame.hands()[0];

    // Check if the hand has any fingers
    const FingerList fingers = hand.fingers();
    if (!fingers.empty()) {
      // Calculate the hand's average finger tip position
      Vector avgPos;
      for (int i = 0; i < fingers.count(); ++i) {
        avgPos += fingers[i].tipPosition();
      }
      avgPos /= (float)fingers.count();
      std::cout << "Hand has " << fingers.count()
                << " fingers, average finger tip position" << avgPos << std::endl;
    }

    // Get the hand's sphere radius and palm position
    std::cout << "Hand sphere radius: " << hand.sphereRadius()
              << " mm, palm position: " << hand.palmPosition() << std::endl;

    // Get the hand's normal vector and direction
    const Vector normal = hand.palmNormal();
    const Vector direction = hand.direction();

    // Calculate the hand's pitch, roll, and yaw angles
    std::cout << "Hand pitch: " << direction.pitch() * RAD_TO_DEG << " degrees, "
              << "roll: " << normal.roll() * RAD_TO_DEG << " degrees, "
              << "yaw: " << direction.yaw() * RAD_TO_DEG << " degrees" << std::endl;
  }

  // Get gestures
  const GestureList gestures = frame.gestures();
  for (int g = 0; g < gestures.count(); ++g) {
    Gesture gesture = gestures[g];

    switch (gesture.type()) {
      case Gesture::TYPE_CIRCLE:
      {
        CircleGesture circle = gesture;
        std::string clockwiseness;

        if (circle.pointable().direction().angleTo(circle.normal()) <= PI/4) {
          clockwiseness = "clockwise";
        } else {
          clockwiseness = "counterclockwise";
        }

        // Calculate angle swept since last frame
        float sweptAngle = 0;
        if (circle.state() != Gesture::STATE_START) {
          CircleGesture previousUpdate = CircleGesture(controller.frame(1).gesture(circle.id()));
          sweptAngle = (circle.progress() - previousUpdate.progress()) * 2 * PI;
        }
        std::cout << "Circle id: " << gesture.id()
                  << ", state: " << gesture.state()
                  << ", progress: " << circle.progress()
                  << ", radius: " << circle.radius()
                  << ", angle " << sweptAngle * RAD_TO_DEG
                  <<  ", " << clockwiseness << std::endl;
        break;
      }
      case Gesture::TYPE_SWIPE:
      {
        SwipeGesture swipe = gesture;
        std::cout << "Swipe id: " << gesture.id()
          << ", state: " << gesture.state()
          << ", direction: " << swipe.direction()
          << ", speed: " << swipe.speed() << std::endl;
        break;
      }
      case Gesture::TYPE_KEY_TAP:
      {
        KeyTapGesture tap = gesture;
        std::cout << "Key Tap id: " << gesture.id()
          << ", state: " << gesture.state()
          << ", position: " << tap.position()
          << ", direction: " << tap.direction()<< std::endl;
        break;
      }
      case Gesture::TYPE_SCREEN_TAP:
      {
        ScreenTapGesture screentap = gesture;
        std::cout << "Screen Tap id: " << gesture.id()
        << ", state: " << gesture.state()
        << ", position: " << screentap.position()
        << ", direction: " << screentap.direction()<< std::endl;
        break;
      }
      default:
        std::cout << "Unknown gesture type." << std::endl;
        break;
    }
  }

  if (!frame.hands().empty() || !gestures.empty()) {
    std::cout << std::endl;
  }
}

int main() {
  // Create a sample listener and controller
  SampleListener listener;
  Controller controller;

  // Have the sample listener receive events from the controller
  controller.addListener(listener);

  // Keep this process running until Enter is pressed
  std::cout << "Press Enter to quit..." << std::endl;
  std::cin.get();

  // Remove the sample listener when done
  controller.removeListener(listener);

  return 0;
}
        */
        #endregion

    }
}
