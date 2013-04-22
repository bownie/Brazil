using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Xyglo.Brazil;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Xyglo.Friendlier
{
    /// <summary>
    /// Exports a BrazilApp to a series of external scripts that can be built into an app.
    /// </summary>
    public abstract class ScriptExporter //: IBrazilExporter
    {
        public ScriptExporter(BrazilApp app, string exportDir, string templateDir)
        {
            m_app = app;
            m_exportDirectory = exportDir;
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
        /// Make a directory
        /// </summary>
        /// <param name="directory"></param>
        protected void makeDir(string directory)
        {
            Directory.CreateDirectory(directory);
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
            File.WriteAllText(filePath, @text);
        }

        protected void appendText(string filePath, string text)
        {
            File.AppendAllText(filePath, @text);
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
        /// Copy a file with overwrite
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destinationDirectory"></param>
        protected void copyFile(string filePath, string destinationDirectory, bool overwrite = true)
        {
            string destFile = destinationDirectory + @"\" + System.IO.Path.GetFileName(filePath);
            File.Copy(filePath, destFile, overwrite);
        }

        /// <summary>
        /// Copy directory
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        protected void copyDir(string sourcePath, string destPath, bool overwrite = true)
        {
            if (!Directory.Exists(destPath))
            {
    	        Directory.CreateDirectory(destPath);
            }

            foreach (string file in Directory.GetFiles(sourcePath))
            {
    	        string dest = Path.Combine(destPath, Path.GetFileName(file));
                File.Copy(file, dest, overwrite);
            }

            foreach (string folder in Directory.GetDirectories(sourcePath))
            {
    	        string dest = Path.Combine(destPath, Path.GetFileName(folder));
    	        copyDir(folder, dest, overwrite);
            }
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
        /// Run the export
        /// </summary>
        public abstract void export();

        /// <summary>
        /// Clean the export directory
        /// </summary>
        public void clean()
        {
            Directory.Delete(m_exportDirectory, true);
        }

        /// <summary>
        /// Modify a string to a double escaped one
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        protected string escapeBackslashes(string filePath)
        {
            string pattern = @"([^\\])\\([^\\])";
            Regex rgx = new Regex(pattern);
            string rS = rgx.Replace(filePath, @"$1\\$2");
            return rS;
        }

        /// <summary>
        /// Do some postprocessing of our text output file
        /// </summary>
        /// <param name="exportFile"></param>
        protected void postProcessEscapes(string exportFile)
        {
            string tmpFile = exportFile + ".post.txt";
            // Delete the file if it exists. 
            if (File.Exists(tmpFile))
                File.Delete(tmpFile);

            // Create the file. 
            StreamWriter outfile = new StreamWriter(tmpFile);

            using (StreamReader sr = File.OpenText(exportFile))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    outfile.WriteLine(escapeBackslashes(s));
                }
            }
        }


        /// <summary>
        /// App handle
        /// </summary>
        protected readonly BrazilApp m_app;

        /// <summary>
        /// Directory to export into
        /// </summary>
        protected string m_exportDirectory;

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
