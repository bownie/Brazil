using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Not to be confused with a Project - which is actually the Friendlier level project file which
    /// manages which files are loaded and how a BrazilApp is shown.  The BrazilProject is used to manage
    /// the project information around a BrazilApp.  Therefore a BrazilProject provides metadata to describe
    /// what is in the app package we're using Friendlier to modify.
    /// </summary>
    public class BrazilProject
    {
        public BrazilProject(string fileName)
        {
            m_fileName = fileName;
        }

        public void serialise()
        {
            // Before serialisation we store how long we've been active for
            // and reset the last access time
            //
            DateTime snapshot = DateTime.Now;
            m_activeTime += snapshot - m_lastAccessTime;
            m_lastWriteTime = snapshot;

            if (m_fileName != null)
            {
                using (FileStream writer = new FileStream(m_fileName, FileMode.Create))
                {
                    System.Runtime.Serialization.DataContractSerializer x = new System.Runtime.Serialization.DataContractSerializer(this.GetType(), getKnownTypes());
                    x.WriteObject(writer, this);
                }
            }
        }

        /// <summary>
        /// Deserialise from the file
        /// </summary>
        static public BrazilProject deserialise(string fileName)
        {
            // Deserialize the data and read it from the instance.
            //
            BrazilProject deserializedProject = null;
 
            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            {
                // Have to create a ReaderQuota with a non-standard MaxDepth to be able to parse our model
                // 
                XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
                quotas.MaxDepth = 255;

                XmlDictionaryReader reader =
                    XmlDictionaryReader.CreateTextReader(fs, quotas);
                DataContractSerializer ser = new DataContractSerializer(typeof(BrazilProject), BrazilProject.getKnownTypes());

                deserializedProject = (BrazilProject)ser.ReadObject(reader, true);
            }

            return deserializedProject;
        }

        /// <summary>
        /// List of known types we want to export
        /// </summary>
        /// <returns></returns>
        static protected List<Type> getKnownTypes()
        {
            List<Type> knownTypes = new List<Type>();
            knownTypes.Add(typeof(BufferView));
            knownTypes.Add(typeof(BrazilView));
            knownTypes.Add(typeof(BrazilPaulo));
            knownTypes.Add(typeof(BrazilApp));
            knownTypes.Add(typeof(KeyAction));
            knownTypes.Add(typeof(MouseAction));

            knownTypes.Add(typeof(BrazilBannerText));
            knownTypes.Add(typeof(BrazilFinishBlock));
            knownTypes.Add(typeof(BrazilFlyingBlock));
            knownTypes.Add(typeof(BrazilGoody));
            knownTypes.Add(typeof(BrazilHud));
            knownTypes.Add(typeof(BrazilInterloper));
            knownTypes.Add(typeof(BrazilMenu));
            knownTypes.Add(typeof(Component));
            knownTypes.Add(typeof(Component3D));
            knownTypes.Add(typeof(DrawableComponent));

            return knownTypes;
        }

        /// <summary>
        /// App itself
        /// </summary>
        protected BrazilApp m_app;

        /// <summary>
        /// BrazilProject filename
        /// </summary>
        protected string m_fileName;

        /// <summary>
        /// How long has this project been active for (total in-app time)
        /// </summary>
        [DataMember]
        public TimeSpan m_activeTime
        { get; set; }

        /// <summary>
        /// When this project was initially created - when we persist this it should
        /// only ever take the initial value here.
        /// </summary>
        [DataMember]
        protected DateTime m_creationTime = DateTime.Now;

        /// <summary>
        /// When this project is reconstructed this value will be updated
        /// </summary>
        [DataMember]
        public DateTime m_lastAccessTime;

        /// <summary>
        /// Last time this project was persisted
        /// </summary>
        [DataMember]
        protected DateTime m_lastWriteTime;
    }
}
