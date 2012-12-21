using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Xyglo.Brazil
{
    /// <summary>
    /// Abstract DrawableComponent in the Brazil space
    /// </summary>
    [DataContract(Namespace = "http://www.xyglo.com")]
    [KnownType(typeof(DrawableComponent))]
    public abstract class DrawableComponent : Component
    {
        // At the moment this abstract class doesn't add any value
    }
}