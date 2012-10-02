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
        /// Get a Target
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public Target getTarget(string value)
        {
            return new Target(value);
        }

        /// <summary>
        /// Override here to ensure that a List.CompareTo works with this class
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            return (((Target)obj).m_name == this.m_name);
        }

        /// <summary>
        /// Needed mainly to avoid warning
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return m_name.Length * m_name.Length;
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