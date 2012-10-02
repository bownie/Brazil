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
