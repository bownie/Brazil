using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xyglo.Brazil;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Exports a BrazilApp to a series of external scripts that can be built into an app.
    /// </summary>
    public abstract class ScriptExporter //: IBrazilExporter
    {
        public ScriptExporter(BrazilApp app, string projectDir, string templateDir)
        {
            m_app = app;
            m_projectDirectory = projectDir;
            m_templateDirectory = templateDir;
        }

        /// <summary>
        /// Check a directory exists
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        protected bool checkDir(string directory)
        {
            return (Directory.Exists(directory));
        }

        /// <summary>
        /// Test to see if a file path is writeable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected void testWriteable(string filePath)
        {
            File.WriteAllText(filePath, "Friendlier is testing writeability of this file.");
            File.Delete(filePath);
        }

        /// <summary>
        /// Write out a whole formatted file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        protected void writeAll(string filePath, string text)
        {
            File.WriteAllText(filePath, text);
        }

        protected void appendText(string filePath, string text)
        {
            File.AppendAllText(filePath, text);
        }

        /// <summary>
        /// Append all to a file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="text"></param>
        protected void appendFile(string outputFilePath, string appendFilePath)
        {
            File.AppendAllText(outputFilePath, File.ReadAllText(appendFilePath).Replace("\r\n", "\n"));
        }

        /// <summary>
        /// Use this to get the current method name - useful for creating methods of the same name
        /// as that they export.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string getCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return sf.GetMethod().Name;
        }

        /// <summary>
        /// Get a tab prefix string for a given level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        protected string getTabPrefix(int level)
        {
            string rS = "";
            for (int i = 0; i < level; i++)
                rS += m_tabSpace;

            return rS;
        }

        /// <summary>
        /// This must be defined in subclasses
        /// </summary>
        public abstract void export();

        /// <summary>
        /// App handle
        /// </summary>
        protected readonly BrazilApp m_app;

        /// <summary>
        /// Directory to build into
        /// </summary>
        protected string m_projectDirectory;

        /// <summary>
        /// Template directory - sometimes we need to fetch files from the template when exporting
        /// </summary>
        protected string m_templateDirectory;

        /// <summary>
        /// Tab space size
        /// </summary>
        protected string m_tabSpace = "  ";

        /// <summary>
        /// Line ending
        /// </summary>
        protected string m_LE = "\r\n";
    }
}
