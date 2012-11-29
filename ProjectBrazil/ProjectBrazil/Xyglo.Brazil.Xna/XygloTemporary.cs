namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Enumeration of temporary types
    /// </summary>
    public enum XygloTemporaryType
    {
        CopyText
    }
    /// <summary>
    /// A XygloTemporary drawable type - we use this as a Key into a Dictionary that points to a
    /// temporary XygloDrawableComponent object
    /// </summary>
    public class XygloTemporary
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public XygloTemporary(XygloTemporaryType type, int index = 0)
        {
            m_type = type;
            m_index = index;
        }

        /// <summary>
        /// Get the type
        /// </summary>
        /// <returns></returns>
        public XygloTemporaryType getType()
        {
            return m_type;
        }

        /// <summary>
        /// Get the index
        /// </summary>
        /// <returns></returns>
        public int getIndex()
        {
            return m_index;
        }

        /// <summary>
        /// Set the index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void setIndex(int index)
        {
            m_index = index;
        }

        /// <summary>
        /// Get the dropdead time
        /// </summary>
        /// <returns></returns>
        public double getDropDead()
        {
            return m_dropdead;
        }

        /// <summary>
        /// Set the dropdead time
        /// </summary>
        /// <param name="dropDead"></param>
        public void setDropDead(double dropDead)
        {
            m_dropdead = dropDead;
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(XygloTemporary a, XygloTemporary b)
        {
            return (a.getType() == b.getType() && a.getIndex() == b.getIndex() && a.getDropDead() == b.getDropDead());
        }

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(XygloTemporary a, XygloTemporary b)
        {
            return (a.getType() != b.getType() || a.getIndex() != b.getIndex() || a.getDropDead() != b.getDropDead());
        }

        /// <summary>
        /// Type that this temporary refers to
        /// </summary>
        protected XygloTemporaryType m_type;

        /// <summary>
        /// Index of this temporary - used as an identifier only in the case of ordering requirements
        /// </summary>
        protected int m_index;

        /// <summary>
        /// Dropdead time for this and associated object if there is no other scope limitation
        /// </summary>
        double m_dropdead;
    }
}
