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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public XDocument InputDocument { get; set; }
        public List<Mapping> mappingData { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InputFiles.NextClicked += new EventHandler(InputNextClicked);
            ucTemplates.TemplatesNextClicked += new EventHandler(TemplatesNextClicked);
            ucTemplates.OutputNextClicked += new EventHandler(OutputNextClicked);           
            
            //load existing
            mappingData = XMLConvertHelper.GetFirstMapping();
            btnInput_Click(null, null);
        }

        private void setColour(Button c)
        {
            btnTemplates.Background = Brushes.White;
            btnInput.Background = Brushes.White;
            btnMap.Background = Brushes.White;
            btnOutput.Background = Brushes.White;
            btnExit.Background = Brushes.White;

            btnTemplates.Foreground = Brushes.Black;
            btnInput.Foreground = Brushes.Black;
            btnMap.Foreground = Brushes.Black;
            btnOutput.Foreground = Brushes.Black;
            btnExit.Foreground = Brushes.Black;

            if (c.Background == Brushes.Blue)
            {
                c.Background = Brushes.White;
                c.Foreground = Brushes.Black;
            }
            else
            {
                c.Background = Brushes.Blue;
                c.Foreground = Brushes.White;
            }
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setColour(btnInput);
                spMain.Children.Clear();
                InputFiles inFile = new InputFiles();
                inFile.LoadControlData(mappingData);
                spMain.Children.Add(inFile);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InputNextClicked(object sender, object e)
        {
            btnOutput_Click(null, null);
        }

        private void OutputNextClicked(object sender, object e)
        {            
            btnTemplates_Click(null, null);
        }    

        private void btnTemplates_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                spMain.Children.Clear();
                setColour(btnTemplates);
                ucTemplates templates = new ucTemplates(folderMode.templates);
                templates.LoadControlData(mappingData);
                spMain.Children.Add(templates);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TemplatesNextClicked(object sender, object e)
        {
            btnMap_Click(null, null);
        }

        private void StartStopWait()
        {
            LoadingAdorner.IsAdornerVisible = !LoadingAdorner.IsAdornerVisible;
           // this.IsEnabled = !LoadingAdorner.IsAdornerVisible;
        }

        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                setColour(btnMap);
                spMain.Children.Clear();
                StartStopWait();
                ucMappingTable mapping = new ucMappingTable();

                spMain.Children.Add(mapping);
                //spMain.Children.
                mapping.LoadControlData(mappingData);
                StartStopWait();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnOutput_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                spMain.Children.Clear();
                setColour(btnTemplates);
                ucTemplates templates = new ucTemplates(folderMode.outputPath);
                templates.LoadControlData(mappingData);
                spMain.Children.Add(templates);
                setColour(btnOutput);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e)
        {            
            this.Close();
        }
    }
}
