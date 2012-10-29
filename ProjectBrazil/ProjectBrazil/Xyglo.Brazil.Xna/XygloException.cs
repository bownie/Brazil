using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil.Xna
{
    public class XygloException : Exception
    {
        public XygloException(string part, string description)
        {
            m_part = part;
            m_description = description;
        }

        /// <summary>
        /// What part of the Xyglo framework are we in?
        /// </summary>
        public string m_part;

        /// <summary>
        /// Describe this exception
        /// </summary>
        public string m_description;
    }
}
