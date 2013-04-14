using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// A Configuration item
    /// </summary>
    [DataContract(Name = "Friendlier", Namespace = "http://www.xyglo.com")]
    public sealed class Configuration : IComparable
    {
        /// <summary>
        /// The name of this configuration item
        /// </summary>
        [DataMember]
        public string Name;

        /// <summary>
        /// The value of this configuration item
        /// </summary>
        [DataMember]
        public string Value;

        public Configuration(string name, string value)
        {
            Name = name;
            Value = value;
        }

        // Implement the generic CompareTo method with the Temperature 
        // class as the Type parameter. 
        //
        int IComparable.CompareTo(object obj)
        {
            Configuration config = (Configuration)(obj);

            // If other is not a valid object reference, this instance is greater.
            if (config == null) return 1;

            // The temperature comparison depends on the comparison of 
            // the underlying Double values. 
            return Name.CompareTo(config.Name);
        }
    }
}
