using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Vector2 is based on XNA's Vector2
    /// </summary>
    public class BrazilVector3
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilVector3()
        {
            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;
        }

        /// <summary>
        /// Single constructor
        /// </summary>
        /// <param name="single"></param>
        public BrazilVector3(float single)
        {
            X = single;
            Y = single;
            Z = single;
        }

        /// <summary>
        /// Double constructor
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public BrazilVector3(float x1, float y1, float z1)
        {
            X = x1;
            Y = y1;
            Z = z1;
        }

        /// <summary>
        /// Accessors
        /// </summary>
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static BrazilVector3 Zero { get { return ZeroVector3; } }

        static public BrazilVector3 ZeroVector3 = new BrazilVector3(0, 0, 0);
    }
}
