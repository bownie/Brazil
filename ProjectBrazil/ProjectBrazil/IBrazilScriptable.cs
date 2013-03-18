using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectBrazil
{
    /// <summary>
    /// Required if a component is to be scriptedised
    /// </summary>
    public interface IBrazilScriptable
    {
        string getScript();
    }
}
