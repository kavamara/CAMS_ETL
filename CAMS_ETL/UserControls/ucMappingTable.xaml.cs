using CAMS_ETL.HelperClasses;
using CSOpenXmlExcelToXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using System.Xml.Serialization;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for ucMappingTable.xaml
    /// </summary>
    public partial class ucMappingTable : UserControl
    {
        public XDocument OrigialDoc { get; set; }
        public List<Template> templates { get; set; }
        public Dictionary<string, object> InputColumns { get; set; }
        public List<List<Mapping>>AllMappingData { get; set; }

        public ucMappingTable()
        {
            InitializeComponent();
        }
        
        public void LoadControlData(List<Mapping> mapping)
        {
            InputColumns = GetColumnNamesFromInput();
            AllMappingData = new List<List<Mapping>>();
            AllMappingData.Add(mapping);
        }

        private void cboTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (templates != null)
            {
                BindDataToUC();
            }
        }

        private void cboTemplate_Loaded(object sender, EventArgs e)
        {
            templates = new List<HelperClasses.Template>();
            int i = 0;
            if (inputfiles.templateFilenames != null)
            {
                foreach (string file in inputfiles.templateFilenames)
                {
                    i++;
                    Template t = new Template();
                    t.TemplateId = i;
                    t.TemplateName = System.IO.Path.GetFileNameWithoutExtension(file);
                    DataSet dsCols = XMLConvertHelper.GetColumnNamesFromExcel(file,0);
                    
                    if (dsCols != null)
                    {
                        foreach (DataColumn col in dsCols.Tables[0].Columns)
                        {
                            if (col != null)
                            {
                                t.Columns.Add(col.ColumnName);
                            }
                        }
                    }
                    templates.Add(t);
                }
              
                cboTemplate.ItemsSource = templates;
                cboTemplate.DisplayMemberPath = "TemplateName";                
                cboTemplate.SelectedIndex = 0;
               

            }
        }        

        private Dictionary<string, object> GetColumnNamesFromInput()
        {
            if (InputColumns == null)
            {
                InputColumns = new Dictionary<string, object>();
                int id = 0;
                DataSet dsCols = XMLConvertHelper.GetColumnNamesFromExcel(inputfiles.inputFilenames.FirstOrDefault(),0);
                if (dsCols != null && dsCols.Tables.Count > 0)
                {
                    foreach (DataColumn dc in dsCols.Tables[0].Columns)
                    {
                        id++;
                        InputColumns.Add(dc.ColumnName, id);
                    }
                }
                
            }            
            return InputColumns;
        }

        private void BindDataToUC()
        {
            gControls.Children.Clear();
            double height = 0;
            int i = 0;
            List<Mapping> mappingData = new List<Mapping>();
            Template selTemplate = (Template)cboTemplate.SelectedItem;
            if (selTemplate != null)
            {
                if (selTemplate.Columns != null)
                {                   
                    foreach (string colName in selTemplate.Columns)
                    {
                        ucMapping m = new ucMapping();

                        mappingData = (XMLConvertHelper.GetMapping(selTemplate.TemplateName));
                        
                        m.columnNames = InputColumns;
                        m.LoadControlData(colName, mappingData, selTemplate.TemplateName);
                        gControls.RowDefinitions.Add(new RowDefinition());
                        m.SetValue(Grid.RowProperty, i);
                        gControls.Children.Add(m);
                        i++;
                        height += m.Height;
                    }                                  
                    gControls.RowDefinitions.Add(new RowDefinition());
                    btnSave.SetValue(Grid.RowProperty, i);

                    gControls.Children.Add(btnSave);

                    mainStack.Height = height + 90;                   
                }
            }
        }       

        private Mapping CreateMappingData(ucMapping map)
        {
            Mapping mapping = new Mapping();
            if (map != null)
            {
                mapping.inputColumns = new List<string>();
                if (map.MC.SelectedItems != null)
                {
                    foreach (KeyValuePair<string, object> s in map.MC.SelectedItems)
                    {
                        mapping.inputColumns.Add(s.Key);
                    }
                }
                mapping.isUnique = (bool)map.chkUnique.IsChecked;
                mapping.format = !string.IsNullOrEmpty(map.txtSeparator.Text) ? map.txtSeparator.Text : "  ";
                mapping.templatePropName = map.lblTemplateColumn.Content.ToString();
                mapping.excludeValues = map.excludedList;
                mapping.replaceValues = map.replaceValues;
                mapping.inputFilenames = inputfiles.inputFilenames;
                mapping.OutputFolderPath = inputfiles.OutputFolderPath;
                mapping.templateFilenames = inputfiles.templateFilenames;
                mapping.TemplateFolder = inputfiles.TemplateFolder;
                mapping.ConfigFileName = this.cboTemplate.Text;
            }

            return mapping;
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            List<Mapping> mappingData = new List<Mapping>();

            foreach (UIElement rd in gControls.Children)
            {
                if (rd.GetType() == typeof(ucMapping))
                {
                    ucMapping m = (ucMapping)rd;
                    if (m != null)
                    {
                        mappingData.Add(CreateMappingData(m));
                    }
                }
            }
            XDocument doc = XMLConvertHelper.SerializeToXmlDoc(mappingData);
            
            Template selTemplate = (Template)cboTemplate.SelectedItem;
            FileSaveHelper.SaveXml(inputfiles.OutputFolderPath, doc, selTemplate.TemplateName);     
            MessageBox.Show(string.Format("Configuration for '{0}' saved.", selTemplate.TemplateName));
        }
       
    }
}
