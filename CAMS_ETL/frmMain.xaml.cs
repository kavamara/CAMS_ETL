using CAMS_ETL.HelperClasses;
using CAMS_ETL.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for frmMain.xaml
    /// </summary>
    public partial class frmMain : Window
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        public Dictionary<string, object> selecteTemplateNames { get; set; }
        
        int runOnce = 0;

        public frmMain()
        {
            InitializeComponent();
            //frmSelectTemplate.OkClicked += new EventHandler(TemplatesOkClicked);
            //frmSelectTemplate.CancelClicked += new EventHandler(TemplatesCancelClicked);
            
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;

        }

        private void StartStopWait()
        {
            LoadingAdorner.IsAdornerVisible = !LoadingAdorner.IsAdornerVisible;
            this.btnGenerate.IsEnabled = !LoadingAdorner.IsAdornerVisible;
        }       

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (runOnce == 0)
            {
                runOnce++;
                CreateTemplatesFromConfig();                
            }
        }

        private void worker_RunWorkerCompleted(object sender,
                                               RunWorkerCompletedEventArgs e)
        {
            StartStopWait();
        }

        private bool IsConfigFound(ref string path)
        {
            bool isValid = false;
            if (!Directory.Exists(path) || Directory.GetFiles(path).Count() == 0)
            {
                MessageBox.Show
                        ("Configuration file(s) not found."
                        , "Config"
                        , MessageBoxButton.OK);

            }
            else
            {
                isValid = true;
            }
            path = Properties.Settings.Default.ConfigSavePath;
            return isValid;
        }

        private void CreateTemplatesFromConfig()
        {
            try
            {
                if (runOnce != 1) return;
                
        
                string path = Properties.Settings.Default.ConfigSavePath;
                if (path != string.Empty)
                {
                    if (IsConfigFound(ref path))
                    {
                        foreach (string f in Directory.GetFiles(path))
                        {
                            //if (selecteTemplateNames != null)
                            //{
                                //if (selecteTemplateNames.Select(p => p.Key.Contains(System.IO.Path.GetFileNameWithoutExtension(f))).Count() > 0)
                                //{
                                    XDocument configFile = XMLConvertHelper.GetConfigFile(f);

                                    // DataSet ds = FileSaveHelper.ConvertToDataSet(doc);
                                    string inputfilePath = XMLConvertHelper.GetElementFromConfig(configFile, "inputFilenames");
                                    XDocument originalDoc = XMLConvertHelper.ConvertExcelDataToXML(inputfilePath,0);
                                    List<Mapping> mappingData = XMLConvertHelper.DeSerializeDoc(configFile);
                                    string currentMapping = mappingData.Select(p => p.ConfigFileName).FirstOrDefault();
                                    string SavePath = mappingData.Select(p => p.OutputFolderPath).FirstOrDefault();
                                    DataSet ds = FilterResults(originalDoc, configFile, mappingData);
                                    RemoveDuplicates(ref ds, mappingData, currentMapping, SavePath);
                                    
                                    if (SavePath.LastIndexOf(@"\") != SavePath.Length - 1) SavePath = SavePath + @"\";
                                    
                                    FileSaveHelper.ExportDataSet(ds, string.Format("{0}{1}", SavePath, currentMapping + ".xlsx"));
                                //}
                            //}                                                      
                        }
                        MessageBox.Show("Templates have been successfully created.");  
                       // popTemplateSelect.Close();
                    }
                }
                else
                {
                    //throw new Exception("Config file path not found");
                    MessageBox.Show("Config file path not found");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error in generating templates : {0} ", ex.Message));
            }
            finally
            {
                
            }
        }

        //private void TemplatesOkClicked(object sender, object e)
        //{
        //    //if (sender != null)
        //    //{
        //    //    selecteTemplateNames = (Dictionary<string, object>)sender;

        //    //}
                       
        //    worker.RunWorkerAsync();
        //    worker_DoWork(null, null);  
        //}


        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {           
            StartStopWait();
            worker.RunWorkerAsync();
            worker_DoWork(null, null);
        }

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.ShowDialog();
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btnCondition_Click(object sender, RoutedEventArgs e)
        {
            frmPlaceHoder ph = new frmPlaceHoder();
            ph.ShowDialog();
        }

        private void btnConditionData_Click(object sender, RoutedEventArgs e)
        {
            ConditionData con = new ConditionData();
            con.ShowDialog();
        }       


        private DataSet FilterResults(XDocument orginalDoc , XDocument configFile, List<Mapping> mappingData)
        {
            DataSet ds = new DataSet();
            XDocument doc = new XDocument(new XElement("Dataset1"));

            if (doc != null)
            {              
                foreach (XElement xe in orginalDoc.Root.Elements())
                {
                    XElement xElem = new XElement("Table1");

                    foreach (Mapping m in mappingData)
                    {                       
                        List<string> values = new List<string>();
                        foreach (string col in m.inputColumns)
                        {
                            //replace values
                            if (xe.Element(XmlConvert.EncodeName(col)) != null)
                            {
                                string valueToAdd = xe.Element(XmlConvert.EncodeName(col)).Value;
                                if (valueToAdd != null) valueToAdd = valueToAdd.Trim();
                                        
                                if (m.replaceValues.Where(p => p.current.Contains(valueToAdd) 
                                                    || p.current.Contains("*") 
                                                    || (p.current.Contains(string.Empty) && valueToAdd == string.Empty && p.edited.Length > 0))
                                                    .Count() > 0)
                                {

                                    GridItem b = m.replaceValues.Where(p => p.current == valueToAdd).FirstOrDefault();
                                    if (b != null)
                                    {
                                        valueToAdd = b.edited;
                                    }
                                    else
                                    {
                                        b = m.replaceValues.Where(p => p.current == "*").FirstOrDefault();
                                        if (b != null)
                                        {
                                            valueToAdd = b.edited;
                                        }
                                        else
                                        {
                                            b = m.replaceValues.Where(p => string.IsNullOrEmpty(p.current) && b.edited.Length > 0).FirstOrDefault();
                                            if (b != null)
                                            {
                                                valueToAdd = b.edited;
                                            }
                                        }
                                    }
                                  
                                    values.Add(valueToAdd);
                                }
                                else
                                {
                                    values.Add(XmlConvert.DecodeName(valueToAdd));
                                }                                
                            }
                        }
                        string value = string.Join(m.format, values);
                       
                        XElement newElement = new XElement(XmlConvert.EncodeLocalName(m.templatePropName), value);                        
                        xElem.Add(newElement);                        
                    }
                    doc.Element("Dataset1").Add(xElem);
                    
                }                
                
                ds = XMLConvertHelper.ConvertToDataSet(doc);
           
            }
            return ds;
        }

        public void RemoveDuplicates(ref DataSet ds, List<Mapping> mappingData, string currentMapping, string SavePath)
        {
            //unique test
            List<string> uniqueColumns = new List<string>();
            List<DataColumn> dcs = new List<DataColumn>();
            foreach (Mapping m in mappingData)
            {
                if (m.isUnique)
                {
                    uniqueColumns.Add(m.templatePropName);
                    dcs.Add(ds.Tables[0].Columns[m.templatePropName]);
                }
            }
            if (uniqueColumns.Count() > 0)
            {
                //DataSet duplicatesSet = new DataSet();

                DataTable duplicates = DataSetHelper.RemoveDuplicates(ds.Tables[0], dcs.ToArray(), SavePath, currentMapping);
                
                ds.Tables.Clear();
                ds.Tables.Add(duplicates);
                
               // duplicatesSet.Tables.Add(duplicates);

              //  FileSaveHelper.ExportDataSet(duplicatesSet, string.Format("{0}\\{1}", SavePath, currentMapping + "_Duplicates.xlsx"));
            }

        }

        private static void ForceUIToUpdate()
        {
            DispatcherFrame frame = new DispatcherFrame();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate(object parameter)
            {
                frame.Continue = false;
                return null;
            }), null);

            Dispatcher.PushFrame(frame);
        }

        private void NewWindowThread<T, P>(Func<P, T> constructor, P param) where T : Window
        {
            Thread thread = new Thread(() =>
            {
                T w = constructor(param);
               w.Show();
                w.Closed += (sender, e) => w.Dispatcher.InvokeShutdown();
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }
    }
}
