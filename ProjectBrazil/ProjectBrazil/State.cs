using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The State of a BrazilApp application
    /// </summary>
    public class State
    {
        public State()
        {
            m_name = "None";
        }

        public State(string value)
        {
            m_name = value;
        }

        static public State Test(string value)
        {
            return new State(value);
        }

        /// <summary>
        /// The name of our Target
        /// </summary>
        public string m_name { get; set; }
    }
}
