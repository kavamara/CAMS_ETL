using CAMS_ETL.HelperClasses;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CAMS_ETL
{
    public enum folderMode
    {
        templates,
        outputPath
    }
    /// <summary>
    /// Interaction logic for ucTemplates.xaml
    /// </summary>
    public partial class ucTemplates : System.Windows.Controls.UserControl
    {
        public static EventHandler TemplatesNextClicked;
        public static EventHandler OutputNextClicked;
        public static folderMode mode;

        public ucTemplates(folderMode foldermode)
        {
            InitializeComponent();
            mode = foldermode;
            if (foldermode == folderMode.outputPath)
            {
                lblFile.Content = "Output Path:";
                dgInputFiles.Visibility = System.Windows.Visibility.Hidden;
               // btnNext.Margin = new Thickness(btnTemplates.Margin.Left, btnTemplates.Margin.Bottom + 50, btnTemplates.Margin.Right, btnTemplates.Margin.Bottom); 
            }
        }

        public void LoadControlData(List<Mapping> mappingData)
        {
            btnNext.Visibility = System.Windows.Visibility.Hidden;
            if (mappingData != null)
            {
                if (mappingData.FirstOrDefault() != null)
                {
                    txtInputFile.Text = mappingData.FirstOrDefault().OutputFolderPath;
                    if (mappingData.FirstOrDefault().templateFilenames.FirstOrDefault() != null)
                    {                        
                        if (mode == folderMode.templates)
                        {
                            txtInputFile.Text = mappingData.FirstOrDefault().TemplateFolder;
                            DisplayTemplates(mappingData.FirstOrDefault());
                        }
                    }
                }
            }
            if (txtInputFile.Text != string.Empty) btnNext.Visibility = System.Windows.Visibility.Visible;
        }
           

        private void btnTemplates_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            if (mode == folderMode.templates)
            {
                dlg.SelectedPath = string.Format("{0}Templates", AppDomain.CurrentDomain.BaseDirectory);
            }

            DialogResult result = dlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                string foldername = dlg.SelectedPath;
                txtInputFile.Text = foldername;

                DisplayTemplates(foldername);
            }            
        }

        public void DisplayTemplates(string folderName)
        {
            
            if (mode == folderMode.templates)
            {
                dgInputFiles.Items.Clear();
                foreach (string f in Directory.GetFiles(folderName))
                {                    
                    dgInputFiles.Items.Add(f);
                    btnNext.Visibility = System.Windows.Visibility.Visible;
                }
            }
            else
            {
                inputfiles.OutputFolderPath = folderName;
                btnNext.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public void DisplayTemplates(Mapping mapping)
        {
            if (mode == folderMode.templates)
            {                
                inputfiles.templateFilenames = mapping.templateFilenames;
                foreach (string item in mapping.templateFilenames)
                {
                    if (System.IO.Path.GetExtension(item) == ".xlsx")
                    {
                        dgInputFiles.Items.Add(item);
                    }
                }
            }
            else
            {
                inputfiles.OutputFolderPath = Properties.Settings.Default.ConfigSavePath; 
            }
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            if (txtInputFile.Text != string.Empty)
            {
                if (mode == folderMode.templates)
                {
                    inputfiles.templateFilenames.Clear();
                    foreach (string f in Directory.GetFiles(txtInputFile.Text))
                    {
                        if (System.IO.Path.GetExtension(f) == ".xlsx")
                        {
                            //dgInputFiles.Items.Add(f);
                            inputfiles.templateFilenames.Add(f);
                            inputfiles.TemplateFolder = txtInputFile.Text;
                        }
                    }
                }
                else
                {                    
                    inputfiles.OutputFolderPath = txtInputFile.Text;
                }
            }

            if (mode == folderMode.templates)
            {
                TemplatesNextClicked(sender, e);
            }
            else
            {
                OutputNextClicked(sender, e);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
