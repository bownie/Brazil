﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Declaration and storage of our Brazil context
    /// </summary>
    public class BrazilContext
    {
        /// <summary>
        /// The state of our application - what we're doing at the moment
        /// </summary>
        public State m_state;

        /// <summary>
        /// Confirmation state - expecting Y/N
        /// </summary>
        public ConfirmState m_confirmState = ConfirmState.Test("None");

        /// <summary>
        /// BrazilWorld holds some Worldly information for us
        /// </summary>
        public BrazilWorld m_world = null;

        /// <summary>
        /// Get the list of States from the BrazilApp
        /// </summary>
        public List<State> m_states = null;

        /// <summary>
        /// Get the list of Targets from the BrazilApp
        /// </summary>
        public List<Target> m_targets = null;

        /// <summary>
        /// ActionMap is a Project Brazil reference gets passed from the constructor
        /// </summary>
        public ActionMap m_actionMap = null;
    }
}