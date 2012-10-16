using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// A BrazilApp application Confirmation State
    /// </summary>
    public class ConfirmState
    {
        public ConfirmState()
        {
            m_name = "None";
        }

        public ConfirmState(string value)
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
        /// Set state
        /// </summary>
        /// <param name="checkState"></param>
        public void set(string checkState)
        {
            m_name = checkState;
        }

        /// <summary>
        /// Static 'virtual' constructor for testing value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        static public ConfirmState Test(string value)
        {
            return new ConfirmState(value);
        }

        /// <summary>
        /// The name of our Target
        /// </summary>
        protected string m_name { get; set; }
    }
}
