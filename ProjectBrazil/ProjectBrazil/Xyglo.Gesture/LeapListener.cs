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

        public override void OnFrame(Controller arg0)
        {
            Frame frame = arg0.Frame();

            //if (frame.Hands.Empty)
                //return;

            //Logger.logMsg("Got some fingers - count = " + frame.Fingers.Count);

            Leap.GestureList gestures = frame.Gestures();

            foreach (Leap.Gesture g in gestures)
            {
                switch (g.Type)
                {
                    case Leap.Gesture.GestureType.TYPESWIPE:   
                        SwipeGesture swipe = new SwipeGesture(g);
                        OnSwipe(new SwipeEventArgs(swipe));
                        break;

                    case Leap.Gesture.GestureType.TYPESCREENTAP:


                    case Leap.Gesture.GestureType.TYPECIRCLE:
                    case Leap.Gesture.GestureType.TYPEKEYTAP:

                    case Leap.Gesture.GestureType.TYPEINVALID:
                        //Logger.logMsg(


                    default:
                        Logger.logMsg("Got other type");
                        break;
                }
            }
        }


        /// <summary>
        /// Convenience method for using the event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSwipe(SwipeEventArgs e)
        {
            if (SwipeEvent != null) SwipeEvent(this, e);
        }

        /// <summary>
        /// This listener can emit a SwipeEvent
        /// </summary>
        public event SwipeEventHandler SwipeEvent;

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
