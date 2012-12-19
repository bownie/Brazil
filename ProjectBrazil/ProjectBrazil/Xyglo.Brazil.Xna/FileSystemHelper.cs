using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Help out with FileSystem methods
    /// </summary>
    public class FileSystemHelper
    {
        /// <summary>
        /// Get an executable path from a given string
        /// </summary>
        /// <param name="exe"></param>
        /// <returns></returns>
        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            { 
                if (Path.GetDirectoryName(exe) == String.Empty) 
                { 
                    foreach (string test in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(';')) 
                    { 
                        string path = test.Trim(); 
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path); 
                    } 
                } 
                throw new FileNotFoundException(new FileNotFoundException().Message, exe); 
            } 
            return Path.GetFullPath(exe);
        }
    }
}
