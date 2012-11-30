namespace Xyglo.Brazil.Xna
{
    /// <summary>
    /// Enumeration of temporary types
    /// </summary>
    public enum BrazilTemporaryType
    {
        CopyText
    }
    /// <summary>
    /// A BrazilTemporary drawable type - we use this as a Key into a Dictionary that points to a
    /// temporary XygloDrawableComponent object
    /// </summary>
    public class BrazilTemporary
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BrazilTemporary(BrazilTemporaryType type, int index = 0)
        {
            m_type = type;
            m_index = index;
        }

        /// <summary>
        /// Get the type
        /// </summary>
        /// <returns></returns>
        public BrazilTemporaryType getType()
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
        public static bool operator ==(BrazilTemporary a, BrazilTemporary b)
        {
            return (a.getType() == b.getType() && a.getIndex() == b.getIndex() && a.getDropDead() == b.getDropDead());
        }

        /// <summary>
        /// Not equals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(BrazilTemporary a, BrazilTemporary b)
        {
            return (a.getType() != b.getType() || a.getIndex() != b.getIndex() || a.getDropDead() != b.getDropDead());
        }

        /*
        // Needed to avoid warning
        //
        public override int GetHashCode()
        {
            return m_index;
        }*/

        /// <summary>
        /// Type that this temporary refers to
        /// </summary>
        protected BrazilTemporaryType m_type;

        /// <summary>
        /// Index of this temporary - used as an identifier only in the case of ordering requirements
        /// </summary>
        protected int m_index = 0;

        /// <summary>
        /// Dropdead time for this and associated object if there is no other scope limitation
        /// </summary>
        double m_dropdead = 0;
    }
}
