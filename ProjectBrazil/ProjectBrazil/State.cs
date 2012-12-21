using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A State in a BrazilApp application
    /// </summary>
    [DataContract(Name = "State", Namespace = "http://www.xyglo.com")]
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

        /// <summary>
        /// Check to see if this state is called that name
        /// </summary>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public bool equals(string checkState)
        {
            return (m_name == checkState);
        }

        /// <summary>
        /// Is this State not called that?
        /// </summary>
        /// <param name="checkState"></param>
        /// <returns></returns>
        public bool notEquals(string checkState)
        {
            return (m_name != checkState);
        }

        /// <summary>
        /// Generate a test state
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
        [DataMember]
        public string m_name { get; set; }
    }
}
