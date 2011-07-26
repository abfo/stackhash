using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using StackHashBusinessObjects;

namespace WinQualAtomFeed
{
    public class AtomProduct
    {
        private StackHashProduct m_StackHashProduct;

        public AtomProduct() { ;}  // Needed to serialize;

        public AtomProduct(StackHashProduct product)
        {
            m_StackHashProduct = product;
        }

        public StackHashProduct Product
        {
            get { return m_StackHashProduct; }
            set { m_StackHashProduct = value; }
        }
    }

    public class AtomProductCollection : Collection<AtomProduct>
    {
        private DateTime m_DateFeedUpdated;

        public AtomProductCollection() { }

        public DateTime DateFeedUpdated
        {
            get { return m_DateFeedUpdated; }
            set { m_DateFeedUpdated = value; }
        }
    }

}

