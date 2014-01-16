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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CAMS_ETL
{
    /// <summary>
    /// Interaction logic for frmCreateDb.xaml
    /// </summary>
    public partial class frmCreateDb : Window
    {
        public frmCreateDb()
        {
            InitializeComponent();
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (txtDbName.Text != string.Empty && inputfiles.inputFilenames != null)
            {
                try
                {
                    string xmlFormatstring = new ConvertExcelToXml().StoreInDB(inputfiles.inputFilenames, txtDbName.Text);
                    if (xmlFormatstring == "") this.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message);
                    return;
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
