using System;
using System.Collections.Generic;
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
    /// Interaction logic for frmPlaceHoder.xaml
    /// </summary>
    public partial class frmPlaceHoder : Window
    {
        public frmPlaceHoder()
        {
            InitializeComponent();
            UserControls.ucComposite.CloseClicked +=  new EventHandler(CloseClicked);
            UserControls.ucComposite uc = new UserControls.ucComposite();
            phGrid.Children.Add(uc);
        }

        private void CloseClicked(object sender, object e)
        {
            this.Close();
        }

    }
}
