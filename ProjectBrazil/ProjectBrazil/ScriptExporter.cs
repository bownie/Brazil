using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Exports a BrazilApp to an external script format
    /// </summary>
    public class ScriptExporter : IBrazilExporter
    {
        public ScriptExporter(IBrazilApp app)
        {
            m_app = app;
        }

        public IBrazilApp getApp
        {
            get { return this.m_app; }
        }

        public void export(string fileName)
        {
            foreach (Component component in m_app.getComponents())
            {

            }
        }

        protected readonly IBrazilApp m_app;
    }
}
