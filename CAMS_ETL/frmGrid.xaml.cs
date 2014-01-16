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
using System.Windows.Shapes;

namespace CAMS_ETL
{
    public class GridItem
    {
        public string current { get; set; }
        public string edited { get; set; }

        public GridItem()
        {
            current = string.Empty;
            edited = string.Empty;
        }
    }

    public enum mode
    {
        replace,
        exclude
    }
    /// <summary>
    /// Interaction logic for frmGrid.xaml
    /// </summary>
    public partial class frmGrid : Window
    {
        public static EventHandler OkReplaceClicked;
        public static EventHandler OkExculdeClicked;
        public List<GridItem> GridItems { get; set; }
        public static mode Inputmode { get; set; }

        public frmGrid(mode inputmode, List<GridItem> items)
        {
            InitializeComponent();
            Inputmode = inputmode;

            if (items != null)
            {
                GridItems = items;
            }
            else
            {
                GridItems = new List<GridItem>();

            }
            dgGrid.ItemsSource = GridItems;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (Inputmode == mode.replace)
            {
                OkReplaceClicked(sender, e);
            }
            else
            {
                OkExculdeClicked(sender, e);
            }
            this.Hide();
        }

       
    }
}
