using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Attributes
{
    public class AttributeGroup
    {

        public string AttributeName { get; set; }                            
        public int AttributeID { get; set; }
        public int ProductsPerAttribute { get; set; }
        public List<AttributeValue> AttributeValueList { get; set; }
        public string  Lineage { get; set; }
    }

    public class AttributeValue
    {
        public string AttributeValueName { get; set; }                          
        public int NumberPerAttribute { get; set; }
        //public List<int> ProductIDs { get; set; }

        //make some list to store per attribute value a list of product id's
        //so that if we press a link like 20000 Hz (6)
        //we get 6 products from the assortment content view  
    }
}
