using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The State of a BrazilApp application
    /// </summary>
    public class State : IComparable //, IComparer<State>
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
        /// CompareTo implementation
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        int IComparable.CompareTo(object x)
        {
            State s1 = (State)x;
            return (s1.m_name.CompareTo(this.m_name));
        }

        /// <summary>
        /// Override here to ensure that a List.CompareTo works with this class
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            return (((State)obj).m_name == this.m_name);
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
        /// The name of our Target
        /// </summary>
        public string m_name { get; set; }
    }
}
