using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public class Target
    {
        public Target()
        {
            m_name = "None";
        }

        public Target(string value)
        {
            m_name = value;
        }

        /// <summary>
        /// We use this construction to return no target
        /// </summary>
        public static readonly Target None = new Target("None");

        /// <summary>
        /// We yse this to return a default target
        /// </summary>
        public static readonly Target Default = new Target("Default");

        /// <summary>
        /// The name of our Target
        /// </summary>
        public string m_name { get; set; }
    }
}