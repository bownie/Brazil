using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Interfaces for an App - we need these at the BrazilApp level and also at the XygloXNA level (or
    /// equivalent) if we are to implement Brazil within Brazil
    /// </summary>
    public interface IBrazilApp
    {
        /// <summary>
        /// Add a Component with a given State - by state name
        /// </summary>
        /// <param name="component"></param>
        void addComponent(string stateName, Component component);

        /// <summary>
        /// Add a Component with a given State
        /// </summary>
        /// <param name="component"></param>
        void addComponent(State state, Component component);

        /// <summary>
        /// Check a State exists
        /// </summary>
        /// <param name="state"></param>
        void checkState(State state);

        /// <summary>
        /// Get a state
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        State getState(string stateName);

        /// <summary>
        /// Get the current state of the app
        /// </summary>
        /// <returns></returns>
        State getState();

        /// <summary>
        /// Componetn list
        /// </summary>
        /// <returns></returns>
        List<Component> getComponents();
        
    }
}
