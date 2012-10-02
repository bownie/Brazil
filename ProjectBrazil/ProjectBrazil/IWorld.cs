using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Interfaces that our World must implement
    /// </summary>
    interface IWorld
    {
        /// <summary>
        /// Return a list of features for this world (items that we can assign actions to)
        /// </summary>
        /// <returns></returns>
        //List<string> getFeatures();

        /// <summary>
        /// We want this to set our entry point for the application
        /// </summary>
        /// <param name="initialState"></param>
        void setInitialState(State initialState);

        /// <summary>
        /// Return the list of states that the world can be in
        /// </summary>
        /// <returns></returns>
        List<State> getStates();

        /// <summary>
        /// A list of Targets when performing Actions on State transitions - Targets
        /// are usually a form of Feature
        /// </summary>
        /// <returns></returns>
        List<Target> getTargets();

        /// <summary>
        /// List of the available Input Features
        /// </summary>
        /// <returns></returns>
        //List<string> getInputFeatures();

    }
}
