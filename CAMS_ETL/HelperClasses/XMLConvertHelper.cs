using CAMS_ETL.HelperClasses;
using CSOpenXmlExcelToXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CAMS_ETL
{
    public static class XMLConvertHelper
    {
        public static XDocument SerializeToXmlDoc<T>(this List<T> list)
        {
            XDocument xDoc = new XDocument();
            xDoc = Serialize(list);
            //using (var writer = xDoc.CreateWriter())
            //{
            //    DataContractSerializer dcs = new DataContractSerializer(typeof(List<T>));
            //    dcs.WriteObject(writer, list);
            //}
            return xDoc;
        }

        public static List<Mapping> DeSerializeDoc(XDocument doc)
        {           
            List<Mapping> mappingData = new List<Mapping>();

             mappingData = Deserialize<Mapping>(doc);

            return mappingData;
        }

        public static XDocument GetXDoc(string path)
        {
            XDocument doc = XDocument.Load(path);   
            return doc;
        }

        public static DataSet ConvertToDataSet(XDocument doc)
        {
            XmlReader reader = doc.CreateReader();
            DataSet ds = new DataSet();
            ds.ReadXml(reader);
            return ds;
        }

        public static XDocument ConvertExcelDataToXML(string excelfileName, int skip)
        {
            try
            {                
                if (excelfileName != string.Empty)
                {
                    XDocument xmlFormatstring = new ConvertExcelToXml().GetXML(excelfileName, skip);
                    if (xmlFormatstring == null)
                    {
                        throw new Exception("The content of Excel file is Empty");
                    }
                    return xmlFormatstring;
                }
                else
                {
                    throw new Exception("File not selected");
                }


            }
            catch (Exception ex)
            {
                throw new Exception("Error : " + ex.Message);
            }
        }

        public static DataSet GetColumnNamesFromExcel(string excelfileName, int skip)
        {
            try
            {
                if (excelfileName != string.Empty)
                {
                    DataSet columns = new ConvertExcelToXml().GetColumnsDataSet(excelfileName, skip);
                    if (columns == null)
                    {
                        throw new Exception(string.Format("Invalid file {0}", excelfileName));
                    }
                    return columns;
                }
                else
                {
                    throw new Exception(string.Format("Invalid file {0}", excelfileName));
                }


            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error getting columns from {0} : {1}", excelfileName, ex.Message));
            }
        }

        public static List<T> Deserialize<T>(XDocument doc)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));

            using (var reader = doc.Root.CreateReader())
            {
                return (List<T>)xmlSerializer.Deserialize(reader);
            }
        }

        public static XDocument Serialize<T>(List<T> list)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<T>));

            XDocument doc = new XDocument();
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, list);
            }

            return doc;
        }

        public static XDocument GetConfigFile(string path)
        {            
            if (!string.IsNullOrEmpty(path))
            {
                return XDocument.Load(path);
            }
            throw new Exception(string.Format("Config file not found in {0}", path));
        }

        public static string GetElementFromConfig(XDocument configFile, string localElementName)
        {
            if (configFile != null)
            {
                return configFile.Root.Descendants().Elements().Where(x => x.Name.LocalName == localElementName).Select(p => p.Value).FirstOrDefault();
            }
            return string.Empty;
        }

        public static List<Mapping> GetFirstMapping()
        {
            List<Mapping> mappingData = null;
            string path = Properties.Settings.Default.ConfigSavePath;
            if (path != string.Empty && System.IO.Directory.Exists(path))
            {
                if (Directory.GetFiles(path) != null && Directory.GetFiles(path).Count() > 0)
                {
                    mappingData = new List<Mapping>();
                    string fileName = Directory.GetFiles(path).FirstOrDefault();
                    if (fileName != null)
                    {
                        XDocument configFile = GetConfigFile(fileName);                    
                        mappingData = DeSerializeDoc(configFile);
                    }
                }               
            }
            return mappingData;
        }

        public static List<Mapping> GetMapping(string filename)
        {
            List<Mapping> mappingData = null;
            string path = Properties.Settings.Default.ConfigSavePath + "" + filename + ".xml";
            if (path != string.Empty && System.IO.File.Exists(path))
            {
                mappingData = new List<Mapping>();

                if (path != null)
                {
                    XDocument configFile = GetConfigFile(path);
                    mappingData = DeSerializeDoc(configFile);
                }              
            }
            return mappingData;
        }

    }
}
