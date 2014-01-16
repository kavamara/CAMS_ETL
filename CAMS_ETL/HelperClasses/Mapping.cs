using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CAMS_ETL
{ 
    [XmlRoot(Namespace = "", ElementName = "MappingGroup", DataType = "Mapping", IsNullable = true)]
    [KnownType(typeof(string))]
    public class Mapping
    {
       
        [System.Xml.Serialization.XmlElementAttribute("inputColumns")]
        public List<string> inputColumns { get; set; }

      
        [System.Xml.Serialization.XmlElementAttribute("isUnique")]       
        public bool isUnique { get; set; }

    
        [System.Xml.Serialization.XmlElementAttribute("format")]      
        public string format { get; set; }

    
        [System.Xml.Serialization.XmlElementAttribute("templatePropName")]       
        public string templatePropName { get; set; }

  
        [System.Xml.Serialization.XmlElementAttribute("excludeValues")]     
        public List<GridItem> excludeValues { get; set; }

   
        [System.Xml.Serialization.XmlElementAttribute("replaceValues")]
        public List<GridItem> replaceValues { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("inputFilenames")]
        public List<string> inputFilenames { get; set; }

    
        [System.Xml.Serialization.XmlElementAttribute("templateFilenames")]
        public List<string> templateFilenames { get; set; }        

  
        [System.Xml.Serialization.XmlElementAttribute("OutputFolderPath")]      
        public string OutputFolderPath { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("TemplateFolder")]
        public string TemplateFolder { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("ConfigFileName")]
        public string ConfigFileName { get; set; }
    }
}
