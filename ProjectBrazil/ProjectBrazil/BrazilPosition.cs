using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Screen relative positioning of items
    /// </summary>
    public enum BrazilPosition
    {
        None,
        TopLeft,
        TopMiddle,
        TopRight,
        TopThirdMiddle,
        MiddleLeft,
        Middle,
        MiddleRight,
        BottomThirdMiddle,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }

    /// <summary>
    /// Relative positioning of items
    /// </summary>
    public enum BrazilRelativePosition
    {
        None,
        BeneathCentred,
        BeneathLeft,
        BeneathRight,
        AboveCentred,
        AboveLeft,
        AboveRight,
        LeftButt,
        RightButt
    }


    public enum BrazilAlignment
    {
        Left,
        Middle,
        Right,
        Top,
        Bottom
    }
}
