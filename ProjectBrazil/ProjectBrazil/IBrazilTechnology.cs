using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xyglo.Brazil.Xna;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Interface to the technology layer - these are the methods that the layer must implement
    /// </summary>
    public interface IBrazilTechnology
    {
        /// <summary>
        /// Force a world push to the implementation layer
        /// </summary>
        void pushWorld();

        /// <summary>
        /// Matches the XNA run call - start our app
        /// </summary>
        void Run();

        /// <summary>
        /// Set up the project that might have been loaded externally
        /// </summary>
        /// <param name="project"></param>
        void setProject(Project project);

        /// <summary>
        /// Enable an interface to our message system
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        void setTemporaryMessage(string message, double time);
    }
}
