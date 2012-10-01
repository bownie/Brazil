using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// An abstract class for designing a State - States are extended by applications (those that derive BrazilApp) and
    /// are stored at that level.
    /// </summary>
    public class BrazilState
    {
        protected string m_stateName;
    }
}
