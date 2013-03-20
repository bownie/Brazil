using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Exports a BrazilApp to a series of external scripts that can be built into an app.
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

        /// <summary>
        /// Export the app - various stages to this:
        /// 
        /// - generate AndroidManifest.xml depending on States defined (States equate to Activities)
        ///  [the xml needs to point to resources which need to be loaded]
        /// - generate java that hooks up the Activities
        /// - generates any AngelScript for Urho3D if there is a 3D implementation
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        public void export(string fileName)
        {
            
        }

        /// <summary>
        /// Generate the Android manifest to the given file path
        /// </summary>
        /// <param name="fileName"></param>
        protected void generateAndroidManifest(string fileName)
        {
            foreach (State state in m_app.getStates())
            {

            }
        }

        /// <summary>
        /// Generate java files for activity to a given directory
        /// </summary>
        /// <param name="directory"></param>
        protected void generateActivityJava(string directory)
        {

        }

        /// <summary>
        /// Generate any Angel scripts as required by components
        /// </summary>
        /// <param name="directory"></param>
        protected void generateAngelScript(string directory)
        {
            // If there are components spread across more than one state then we can't do this export
            // so first check the state of all the components.
            //
            foreach (Component component in m_app.getComponents())
            {
                
            }
        }


        protected string getAndroidManifestHeader()
        {
            string thing = @"<?xml version=""1.0"" encoding=""utf-8""?>\
<manifest xmlns:android=""http://schemas.android.com/apk/res/android""";
  /*  
            <?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.googlecode.urho3d"
    android:versionCode="1"
    android:versionName="1.0">
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-feature android:glEsVersion="0x00020000" />
    <uses-sdk android:targetSdkVersion="8" android:minSdkVersion="8" />
    <application android:label="@string/app_name" android:icon="@drawable/icon">
        <activity android:name="SDLActivity"
                  android:label="@string/app_name"
                  android:theme="@android:style/Theme.NoTitleBar.Fullscreen"
                  android:configChanges="keyboardHidden|orientation"
                  android:screenOrientation="landscape">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>

*/
            return thing;
        }
        /// <summary>
        /// App handle
        /// </summary>
        protected readonly IBrazilApp m_app;
    }
}
