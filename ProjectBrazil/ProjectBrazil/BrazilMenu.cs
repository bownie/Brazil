using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    public enum BrazilMenuType
    {
        ContextMenu,
        TopLeftCorner
    };

    /// <summary>
    /// A Menu option
    /// </summary>
    public class BrazilMenuOption
    {
        public BrazilMenuOption(string name, KeyAction shortcut)
        {
            m_optionName = name;
            m_shortcut = shortcut;
        }

        /// <summary>
        /// Option name
        /// </summary>
        public string m_optionName;

        /// <summary>
        /// Keyboard Shortcut
        /// </summary>
        public KeyAction m_shortcut;
    }

    /// <summary>
    /// A menu we can connect up and place accordingly in our world - this is a type of Brazil Component
    /// that can be connected up to a Target (see the BrazilOption) and also be fired from a State and
    /// Action combination.
    /// </summary>
    public class BrazilMenu : Component
    {
        public BrazilMenu(BrazilMenuType type, Action action, string title)
        {
            m_type = type;
            m_title = title;
            m_action = action;

            // By default a menu is always destroyed
            //
            m_isDestroyed = true;
        }

        /// <summary>
        /// Add a menu option
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="shortcut"></param>
        /// <param name="target"></param>
        public void addMenuOption(string optionName, KeyAction shortcut, string target)
        {
            m_menuOptions.Add(new BrazilMenuOption(optionName, shortcut), target);
        }

        /// <summary>
        /// Get the menu options
        /// </summary>
        /// <returns></returns>
        public Dictionary<BrazilMenuOption, string> getMenuOptions()
        {
            return m_menuOptions;
        }


        /// <summary>
        /// Get the menu type
        /// </summary>
        /// <returns></returns>
        public BrazilMenuType getType()
        {
            return m_type;
        }

        /// <summary>
        /// Set the menu type
        /// </summary>
        /// <param name="type"></param>
        public void setType(BrazilMenuType type)
        {
            m_type = type;
        }

        /// <summary>
        /// Dictionary of Options/Target (string)
        /// </summary>
        protected Dictionary<BrazilMenuOption, string> m_menuOptions = new Dictionary<BrazilMenuOption, string>();

        /// <summary>
        /// Type of this menu
        /// </summary>
        protected BrazilMenuType m_type;

        /// <summary>
        /// Action that will activate this menu
        /// </summary>
        protected Action m_action;

        /// <summary>
        /// Title of this menu
        /// </summary>
        protected string m_title;
    }
}
