using CAMS_ETL.HelperClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
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

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for InputFiles.xaml
    /// </summary>
    public partial class InputFiles : UserControl
    {
        public static EventHandler NextClicked;

        public InputFiles()
        {
            InitializeComponent();
            if (ConfigurationManager.AppSettings["ShowCreateDB"].ToString() == "false")
            {
                btnDatabase.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        public void LoadControlData(List<Mapping> mappingData)
        {
            btnNext.Visibility = System.Windows.Visibility.Hidden;
            if (mappingData != null)
            {
                if (mappingData.FirstOrDefault() != null)
                {
                    if (mappingData.FirstOrDefault().inputFilenames.FirstOrDefault() != null)
                    {
                        txtInputFile.Text = mappingData.FirstOrDefault().inputFilenames.FirstOrDefault();

                        inputfiles.inputFilenames.Add(mappingData.FirstOrDefault().inputFilenames.FirstOrDefault());
                        btnNext.Visibility = System.Windows.Visibility.Visible;
                    }
                }
            }            
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xlsx";
            dlg.Filter = "Excel 2010|*.xlsx";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                txtInputFile.Text = dlg.FileName;
                btnNext.Visibility = System.Windows.Visibility.Visible;
            }
        }       

        private void btnDatabase_Click(object sender, RoutedEventArgs e)
        {
            frmCreateDb db = new frmCreateDb();
            db.ShowDialog();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (inputfiles.inputFilenames.Where(p => p.Contains(txtInputFile.Text)).Count() == 0)
            {
                inputfiles.inputFilenames.Clear();
                inputfiles.inputFilenames.Add(txtInputFile.Text);
            }      
            NextClicked(sender, e);
        }
    }
}
