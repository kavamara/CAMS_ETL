using System;
using System.Collections.Generic;
using System.IO;
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

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for frmSelectTemplate.xaml
    /// </summary>
    public partial class frmSelectTemplate : Window
    {
        public static EventHandler OkClicked;
        public static EventHandler CancelClicked;

        public frmSelectTemplate(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            LoadControlData();
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


        public void LoadControlData()
        {
            int i= 0;
            ViewModel vm = new ViewModel();
            vm.Items = new Dictionary<string, object>();
             string path = Properties.Settings.Default.ConfigSavePath;
             if (path != string.Empty)
             {
                 if (IsConfigFound(ref path))
                 {
                     foreach (string f in Directory.GetFiles(path))
                     {
                         vm.Items.Add(System.IO.Path.GetFileNameWithoutExtension(f), i);
                     }
                 }
             }
             ST.ItemsSource = vm.Items;
             ST.SelectedItems = new Dictionary<string, object>();
             ST.SetDefaultAll();
            // ST.SelectedItems.Add("All", 0);
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (CancelClicked != null)
            {
                this.Hide();
                CancelClicked(null, null);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (OkClicked != null)
            {
                OkClicked(ST.SelectedItems, null);
            }
        }
    }
}
