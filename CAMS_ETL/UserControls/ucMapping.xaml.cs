using CAMS_ETL.HelperClasses;
using CSOpenXmlExcelToXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for ucMapping.xaml
    /// </summary>
    public partial class ucMapping : UserControl
    {
        public List<GridItem> excludedList { get; set; }
        public List<GridItem> replaceValues { get; set; }
        public Dictionary<string, object> columnNames { get; set; }
        public Dictionary<string, object> selectedItems { get; set; }
      //  public List<GridItem> ReplaceItems;
        frmGrid f;

        public ucMapping()
        {
            InitializeComponent();                       
            frmGrid.OkReplaceClicked += new EventHandler(ReplaceOKClicked);
        }

        public void LoadControlData(string TemplateColName, List<Mapping> mapping, string selectedTemplate)
        {
            lblTemplateColumn.Content = TemplateColName;
            BindInputColumns();

            if (mapping != null)
            {
                Mapping m = mapping.Where(p => p.templatePropName == TemplateColName).FirstOrDefault();
                if (m != null)
                {
                    if (selectedTemplate == m.ConfigFileName)
                    {
                        Dictionary<string, object> tempvalue = new Dictionary<string, object>();
                        int i = 0;
                        foreach (string s in m.inputColumns)
                        {
                            tempvalue.Add(s, i);
                            i++;
                        }
                        MC.SelectedItems = tempvalue;
                        txtSeparator.Text = m.format;
                        chkUnique.IsChecked = m.isUnique;
                        replaceValues = m.replaceValues;
                        if (replaceValues != null && replaceValues.Count() > 0)
                        {
                            btnReplace.Background = Brushes.LightBlue;
                            btnReplace.Foreground = Brushes.White;
                            btnReplace.Width = 25;
                            Animate();
                        }
                    }
                }
            }
        }



        void Animate()
        {

            DoubleAnimation myDoubleAnimation = new DoubleAnimation();
            if (btnReplace.Width <= 25)
            {
                // Animate the Button's Width.
                myDoubleAnimation.From = 25;
                myDoubleAnimation.To = 75;
                myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
                btnReplace.BeginAnimation(Button.WidthProperty, myDoubleAnimation);
            }
            else
            {
                btnReplace.BeginAnimation(Button.WidthProperty, null);
                btnReplace.Width = 75;
            }
        }


        public void BindInputColumns()
        {
            if (columnNames != null)
            {
                ViewModel vm = new ViewModel();
                vm.Items = new Dictionary<string, object>();

                MC.ItemsSource = columnNames;                
            }
        }
    
        private void btnReplace_Click(object sender, object e)
        {
            if (f == null)
            {
                f = new frmGrid(mode.replace, replaceValues);
                f.ShowDialog();
            }
            else
            {
                f.Show();
            }
            
        }
       
        private void ReplaceOKClicked(object sender, object e)
        {
            if (f != null)
            {
                replaceValues = f.GridItems;
            }
        }

    }
}
