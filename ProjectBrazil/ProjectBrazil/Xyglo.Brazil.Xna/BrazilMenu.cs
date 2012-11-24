using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public enum BrazilMenuType
    {
        TopLeftCorner,
        ContextMenu
    };

    public class BrazilOption
    {
        public string m_description;
        public KeyAction m_shortcut;
    }

    /// <summary>
    /// A menu we can 
    /// </summary>
    public class BrazilMenu
    {
        public BrazilMenu()
        {
        }

        protected Dictionary<BrazilOption, Target> m_menuOptions = new Dictionary<BrazilOption, Target>();

        protected BrazilMenuType m_type;
    }
}
