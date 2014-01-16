using CAMS_ETL.HelperClasses;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Linq.Dynamic;
using CAMS_ETL.Helpers;

namespace CAMS_ETL.UserControls
{
    /// <summary>
    /// Interaction logic for ucComposite.xaml
    /// </summary>
    public partial class ucComposite : UserControl
    {
        public static EventHandler CloseClicked;
        public Dictionary<string, object> selectableCols { get; set; }

        public ucComposite()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CloseClicked != null)
            {
                CloseClicked(null, null);
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            StartStopWait();
            XDocument originalDoc = XMLConvertHelper.ConvertExcelDataToXML(txtInput.Text,0);
            DoProcess(originalDoc);


        }

        public List<XDocument> SplitFile(XDocument docket, int chunkSize)
        {
            var newDockets = new List<XDocument>();
            var d = new XDocument(docket);
            var orders = d.Root.Elements("Table1");
            XDocument newDocket = null;

            do
            {
                newDocket = new XDocument(new XElement("NewDataSet"));
                var chunk = orders.Take(chunkSize);
                newDocket.Root.Add(chunk);
                chunk.Remove();
                newDockets.Add(newDocket);
            } while (orders.Any());

            return newDockets;
        }

        private void StartStopWait()
        {
            LoadingAdorner.IsAdornerVisible = !LoadingAdorner.IsAdornerVisible;
            this.btnProcess.IsEnabled = !LoadingAdorner.IsAdornerVisible;
        }  

        private void DoProcess(XDocument doc)
        {            
            if (doc != null)
            {
                DataSet newDs = new DataSet();
                var res = new List<DataTable>();
                int i = 0;

                string SavePath = System.IO.Path.GetDirectoryName(txtInput.Text);

                SavePath = FileSaveHelper.CreateNewPath(SavePath, "Split");
                string currentMapping = System.IO.Path.GetFileNameWithoutExtension(txtInput.Text);

                bool isExists = System.IO.Directory.Exists(SavePath);

                if (!isExists)
                {
                    System.IO.Directory.CreateDirectory(SavePath);
                }

                if (cboCols.SelectedIndex == 0)
                {
                    //by columns
                    List<DataColumn> dcs = new List<DataColumn>();
                    List<string> cols = OC.SelectedItems.Select(p => p.Key).ToList();
                    var qFields = string.Join(", ", cols.Select(x => "it[\"" + x + "\"] as " + x.Replace(" ", "") + ""));

                    DataSet ds = XMLConvertHelper.ConvertToDataSet(doc);
                    DataTable dt =  ds.Tables[0];
                    var q = dt.AsEnumerable()
                                        .AsQueryable()
                                        .GroupBy("new(" + qFields + ")", "it")
                                        .Select("new (it as Data)");
                    foreach (dynamic d in q)
                    {
                        var dtemp = dt.Clone();
                        foreach (var row in d.Data)
                        {                            
                            dtemp.Rows.Add(row.ItemArray);                           
                        }
                        i++;
                        foreach (DataColumn dc in dtemp.Columns)
                        {
                            dcs.Add(dc);
                        }
                        
                        DataTable duplicates = DataSetHelper.RemoveDuplicates(dtemp, dcs.ToArray(), SavePath, currentMapping);
                        res.Add(dtemp);
                        string newFileName = string.Format("{0}{1}", currentMapping, i);

                        string id = (from DataRow dr in dtemp.Rows select (string)dr["Building Name"]).FirstOrDefault();
                        if (id != null)
                        {
                            newFileName = string.Format("{0}_{1}", newFileName, id);

                        }
                        FileSaveHelper.ExportDataTable(dtemp, string.Format("{0}{1}", SavePath, newFileName + ".xlsx"));
                    }
                }
                else
                {
                    //by row
                    int chunkNumber = 0;
                    if(!int.TryParse(txtNmb.Text, out chunkNumber))
                    {
                        chunkNumber = 1000;
                    }

                    List<XDocument> docs = SplitFile(doc, chunkNumber);                 
                   
                    foreach (XDocument d in docs)
                    {
                        newDs = XMLConvertHelper.ConvertToDataSet(d);
                        
                        i++;
                        string newFileName = string.Format("{0}{1}", currentMapping, i);
                        FileSaveHelper.ExportDataSet(newDs, string.Format("{0}{1}", SavePath, newFileName + ".xlsx"));
                    }
                }
                StartStopWait();
                CloseClicked(null, null);
            }
        }

        private void btnInputBrowse_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel 2010|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();
            if (result ==  true)
            {
                txtInput.Text = dlg.FileName;
                BindTypeColumns();                
            }
        }


        private DataSet GetColumnsFromFile(string filename)
        {
            return XMLConvertHelper.GetColumnNamesFromExcel(filename,0);
        }

        private Dictionary<string, object> GetColumnNamesFromInput(string filename)
        {           
            Dictionary<string, object> InputColumns = new Dictionary<string, object>();
            int id = 0;
            DataSet dsCols = XMLConvertHelper.GetColumnNamesFromExcel(filename,0);
            if (dsCols != null && dsCols.Tables.Count > 0)
            {
                foreach (DataColumn dc in dsCols.Tables[0].Columns)
                {
                    id++;
                    InputColumns.Add(dc.ColumnName, id);
                }
            }
            selectableCols = InputColumns;
            return InputColumns;
        }

        private void BindTypeColumns()
        {
            Dictionary<string, int> vm = new Dictionary<string, int>();
            
            vm.Add("Column Name", 1);
            vm.Add("Number of Rows", 2);
            
            cboCols.ItemsSource = vm;
            cboCols.DisplayMemberPath = "Key";
        }

        private void BindOutputColumns(Dictionary<string, object> columnNames)
        {
            if (columnNames != null)
            {
                ViewModel vm = new ViewModel();
                vm.Items = new Dictionary<string, object>();

                OC.ItemsSource = columnNames;
            }
        }

        private void cboCols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OC.Visibility = System.Windows.Visibility.Hidden;
            lblColumns.Visibility = System.Windows.Visibility.Hidden;
            txtNmb.Visibility = System.Windows.Visibility.Hidden;

            if (((KeyValuePair<string, int>)cboCols.SelectedValue).Value == 1)
            {
                OC.Visibility = System.Windows.Visibility.Visible;
                lblColumns.Visibility = System.Windows.Visibility.Visible;
                BindOutputColumns(GetColumnNamesFromInput(txtInput.Text));
            }
            else
            {
                lblColumns.Content = "No Rows :";
                txtNmb.Visibility = System.Windows.Visibility.Visible;
                lblColumns.Visibility = System.Windows.Visibility.Visible;
            }

        }
    }
}
