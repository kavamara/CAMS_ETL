using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Linq.Dynamic;
using CAMS_ETL.HelperClasses;
using System.IO;
using System.Configuration;
using System.Windows.Forms;
using CSOpenXmlExcelToXml;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for ConditionData.xaml
    /// </summary>
    public partial class ConditionData : Window
    {
        public Dictionary<string, string> selectableCols { get; set; }
        public List<string> fileList { get; set; }

        public ConditionData()
        {
            InitializeComponent();
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {           
            this.Close();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            if (txtConditionFile.Text == string.Empty || txtInputFile.Text == string.Empty || selectableCols.Count == 0)
            {
                System.Windows.MessageBox.Show("Please select files and columns to map");
            }
            else
            {
                DoProcess();
            }

        }

        private void DoProcess()
        {
            string filename = string.Empty;
            //read the inspection sheet and convert to xml
            foreach (string fileName in fileList)
            {
                XDocument conditionXML = XMLConvertHelper.ConvertExcelDataToXML(fileName, 1);
                XDocument inputXML = XMLConvertHelper.ConvertExcelDataToXML(txtInputFile.Text, 0);
                DataSet ds = FilterResults(conditionXML, inputXML, selectableCols, out filename);
                //conditionXML.Save("c:\\temp\\test.xml");
                if (ds != null || ds.Tables.Count > 0)
                {
                    ConvertExcelToXml cex = new ConvertExcelToXml();
                    cex.GenerateExcel(ds.Tables[0], fileName);
                    if (filename == string.Empty)
                    {
                        filename = string.Format("{0}_{1}.xlsx", System.IO.Path.GetFileNameWithoutExtension(txtInputFile.Text), DateTime.Now.ToFileTime().ToString());
                    }
                    filename = string.Format("{0}\\inspection\\{1}_{2}.xlsx", System.IO.Path.GetDirectoryName(txtInputFile.Text), System.IO.Path.GetFileNameWithoutExtension(txtInputFile.Text), DateTime.Now.ToFileTime().ToString());
                    FileSaveHelper.ExportDataSet(ds, filename);                    
                }
                else
                {
                    System.Windows.MessageBox.Show("No matching records found.");
                }
            }
            System.Windows.MessageBox.Show(string.Format("Operation completed. '{0}' created.", txtConditionFile.Text));
        }

        private DataSet FilterResults(XDocument orginalDoc, XDocument inputFile, Dictionary<string, string> mapCols, out string filename)
        {
            DataSet ds = new DataSet();
            filename = string.Empty;

            XDocument doc = new XDocument(new XElement("Dataset1"));
            List<bool> isMatching = null;

            if (doc != null)
            {
                foreach (XElement xe in orginalDoc.Root.Elements())
                {
                    XElement xElem = new XElement("Table1");

                    foreach (XElement xeIn in inputFile.Root.Elements())
                    {
                        if (filename == string.Empty)
                        {
                            if (xeIn.Element(XmlConvert.EncodeLocalName("Building Name")) != null)
                            {
                                filename = xeIn.Element(XmlConvert.EncodeLocalName("Building Name")).Value;
                            }
                        }

                        isMatching = new List<bool>();
                        foreach (KeyValuePair<string, string> kvp in mapCols)
                        {
                            if (xe.Element(XmlConvert.EncodeLocalName(kvp.Key)) == null || xeIn.Element(XmlConvert.EncodeLocalName(kvp.Value)) == null)
                            {
                                throw new Exception("Invalid files selected. Please inspection sheet contains correct columns");
                            }
                            if (xe.Element(XmlConvert.EncodeLocalName(kvp.Key)).Value ==
                                 xeIn.Element(XmlConvert.EncodeLocalName(kvp.Value)).Value)
                            {
                                isMatching.Add(true);
                            }
                            else
                            {
                                isMatching.Add(false);
                            }
                        }                 

                        if (isMatching.Where(p=> p == false).Count() == 0)
                        {
                            xElem.Add(xeIn);
                            doc.Element("Dataset1").Add(xeIn);
                        }                      
                    }                    
                }
              //  doc.Save("c:\\temp\\test.xml");
                
                ds = XMLConvertHelper.ConvertToDataSet(doc);

            }
            return ds;
        }

        private void btnConditionBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel 2010|*.xlsx";
            int skip = 0;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                //List<string> InspColumns = new List<string>();
                //string InspectionCols = ConfigurationManager.AppSettings["InspectionColumns"].ToString();
                //if (InspectionCols.Length > 0)
                //{
                //    InspColumns = InspectionCols.Split(',').ToList();
                //}
                if (txtHdrNumber.Text != string.Empty)
                {
                    int.TryParse(txtHdrNumber.Text, out skip);
                }
                txtConditionFile.Text = dlg.FileName;
                fileList = new List<string>();
                fileList.Add(dlg.FileName);

               
                if (fileList != null && fileList.Count > 0)
                {
                    string filename = fileList.FirstOrDefault();
                    if (filename != null)
                    {
                        List<string> InspColumns = GetColumnNamesFromInput(fileList.FirstOrDefault(), skip);
                        BindInspectionColumns(InspColumns);
                    }
                }
            }
        }
             
        private void btnInputBrowse_Click (object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel 2010|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                txtInputFile.Text = dlg.FileName;
                List<string> cols = GetColumnNamesFromInput(dlg.FileName,0);
                
                BindInputColumns(cols);
                //BindTypeColumns();
            }
        }

        private void BindInspectionColumns(List<string> columnNames)
        {
            if (columnNames != null)
            {
                ViewModel vm = new ViewModel();
                vm.Items = new Dictionary<string, object>();
                
                cboInspection.ItemsSource = columnNames;
            }
        }

        private void BindInputColumns(List<string> columnNames)
        {
            if (columnNames != null)
            {
                ViewModel vm = new ViewModel();
                vm.Items = new Dictionary<string, object>();

                cboInput.ItemsSource = columnNames;
            }
        }


        private List<string> GetColumnNamesFromInput(string filename, int skip)
        {
            List<string> InputColumns = new List<string>();

            DataSet dsCols = XMLConvertHelper.GetColumnNamesFromExcel(filename, skip);
            if (dsCols != null && dsCols.Tables.Count > 0)
            {
                foreach (DataColumn dc in dsCols.Tables[0].Columns)
                {                    
                    InputColumns.Add(dc.ColumnName);
                }
            }
            
            return InputColumns;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (cboInput.Text == string.Empty) return;
            if (cboInspection.Text == string.Empty) return;

            if (selectableCols == null)
            {
                selectableCols = new Dictionary<string, string>();
            }
           
            if (selectableCols.Where(p => p.Key == cboInput.Text).Count() == 0)
            {
                selectableCols.Add(cboInput.Text.Trim(), cboInspection.Text.Trim());
                dgMapping.Items.Add(new KeyValuePair<string, string>(cboInput.Text.Trim(), cboInspection.Text.Trim())); 
            }
        }

        private void txtHdrNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, e.Text.Length - 1))
            {
                e.Handled = true;
            }
        }

        private void btnFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            fileList = new List<string>();
            DialogResult result = dlg.ShowDialog();
            int skip = 0;

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string foldername = dlg.SelectedPath;
                txtConditionFile.Text = foldername;

                foreach (string f in Directory.GetFiles(foldername))
                {
                    fileList.Add(f);
                }

                if (txtHdrNumber.Text != string.Empty)
                {
                    int.TryParse(txtHdrNumber.Text, out skip);
                }

                if (fileList != null && fileList.Count > 0)
                {
                    string filename = fileList.FirstOrDefault();
                    if (filename != null)
                    {
                        List<string> InspColumns = GetColumnNamesFromInput(fileList.FirstOrDefault(), skip);
                        BindInspectionColumns(InspColumns);
                    }
                }
            }            
        }
    }
}
