using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Vector2 is based on XNA's Vector2
    /// </summary>
    public class BrazilMatrix3
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public BrazilMatrix3()
        {
            m1.X = 0.0f;
            m1.Y = 0.0f;
            m1.Z = 0.0f;

            m2.X = 0.0f;
            m2.Y = 0.0f;
            m2.Z = 0.0f;

            m3.X = 0.0f;
            m3.Y = 0.0f;
            m3.Z = 0.0f;
        }

        /// <summary>
        /// Single constructor
        /// </summary>
        /// <param name="single"></param>
        public BrazilMatrix3(float single)
        {
            m1.X = single;
            m1.Y = single;
            m1.Z = single;

            m2.X = single;
            m2.Y = single;
            m2.Z = single;

            m3.X = single;
            m3.Y = single;
            m3.Z = single;
        }

        /// <summary>
        /// Double constructor
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public BrazilMatrix3(BrazilVector3 v1, BrazilVector3 v2, BrazilVector3 v3)
        {
            m1 = v1;
            m2 = v2;
            m3 = v3;
        }

        public BrazilVector3 m1;
        public BrazilVector3 m2;
        public BrazilVector3 m3;

        /// <summary>
        /// Zero Matrix3
        /// </summary>
        static public BrazilMatrix3 Zero { get { return ZeroMatrix3; } }

        /// <summary>
        /// Identity Matrix3
        /// </summary>
        static public BrazilMatrix3 Identity { get { return IdentityMatrix3; } }

        /// <summary>
        /// Static constructor for zero matrix
        /// </summary>
        static private BrazilMatrix3 ZeroMatrix3 = new BrazilMatrix3(new BrazilVector3(0, 0, 0), new BrazilVector3(0, 0, 0), new BrazilVector3(0, 0, 0));

        /// <summary>
        /// Static constructor for identity matrix
        /// </summary>
        static private BrazilMatrix3 IdentityMatrix3 = new BrazilMatrix3(new BrazilVector3(0, 0, 0), new BrazilVector3(0, 0, 0), new BrazilVector3(0, 0, 0));
    }
}
