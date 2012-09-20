using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Vector2 is based on XNA's Vector2
    /// </summary>
    public class BrazilVector2
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        BrazilVector2()
        {
            X = 0.0f;
            Y = 0.0f;
        }

        /// <summary>
        /// Single constructor
        /// </summary>
        /// <param name="single"></param>
        BrazilVector2(float single)
        {
            X = single;
            Y = single;
        }

        /// <summary>
        /// Double constructor
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        BrazilVector2(float x1, float y1)
        {
            X = x1;
            Y = y1;
        }

        //static Vector2()
        //{
        //}

        public float X { get; set; }
        public float Y { get; set; }

        public static BrazilVector2 Zero { get { return ZeroVector2; } }

        static public BrazilVector2 ZeroVector2 = new BrazilVector2(0, 0);
    }
}
