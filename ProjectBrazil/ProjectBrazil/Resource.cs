﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// The type of the Resource
    /// </summary>
    public enum ResourceType
    {
        Image,
        Video,
        Audio,
        Midi
    }

    /// <summary>
    /// Mode identifier for a Resource - how it is applied to a Component
    /// </summary>
    public enum ResourceMode
    {
        None   = 0x000000,  // Default
        Wrap   = 0x000001,  // video or image
        Centre = 0x000002,  // video or image
        Fill   = 0x000004,  // video or image
        Scroll = 0x000008,  // video or image
        Loop   = 0x00000F   // video
    }


    /// <summary>
    /// A Resource is a file type that will be included as an Asset in the app package.
    /// 
    /// We can assign a resource to a State or to a Component and they may be re-used.
    /// 
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class Resource
    {
        public Resource(ResourceType type, string name, string filePath, string description = "")
        {
            m_type = type;
            m_name = name;
            m_filePath = filePath;
            m_description = description;
        }

        public string getName() { return m_name; }
        public string getFilePath() { return m_filePath; }
        public ResourceType getType() { return m_type; }
        public string getDescription() { return m_description; }

        /// <summary>
        /// Name for this resource which should be unique in a Resource collection
        /// </summary>
        [DataMember]
        protected string m_name;

        /// <summary>
        /// Path to this resource
        /// </summary>
        [DataMember]
        protected string m_filePath;

        /// <summary>
        /// Type of this resource
        /// </summary>
        [DataMember]
        protected ResourceType m_type;

        /// <summary>
        /// Description
        /// </summary>
        [DataMember]
        protected string m_description;
    }

    /// <summary>
    /// The instance of a Resource in a Component
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    public class ResourceInstance
    {
        public ResourceInstance(Resource resource, ResourceMode mode = ResourceMode.None, string description = "")
        {
            m_resourceName = resource.getName();
            m_mode = mode;
            m_resource = resource;
        }

        /*
        private void OnCreated()
        {
            m_resource = new Resource(ResourceType.Image, "none");
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext c)
        {
            OnCreated();
        }
        */

        /// <summary>
        /// Get the resource reference
        /// </summary>
        /// <returns></returns>
        public Resource getResource() { return m_resource; }

        /// <summary>
        /// Set the resource reference
        /// </summary>
        /// <param name="resource"></param>
        public void setResource(Resource resource) { m_resource = resource; }

        /// <summary>
        /// Get resource mode
        /// </summary>
        /// <returns></returns>
        public ResourceMode getMode() { return m_mode; }

        /// <summary>
        /// Get the resource name
        /// </summary>
        /// <returns></returns>
        public string getResourceName() { return m_resourceName; }

        /// <summary>
        /// Resource pointer - don't serialise
        /// </summary>
        [NonSerialized]
        protected Resource m_resource;

        /// <summary>
        /// Unique name of resource
        /// </summary>
        [DataMember]
        protected string m_resourceName;

        /// <summary>
        /// Resource mode
        /// </summary>
        [DataMember]
        protected ResourceMode m_mode;
    }

}
