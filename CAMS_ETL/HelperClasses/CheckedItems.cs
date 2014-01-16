using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAMS_ETL.HelperClasses
{
    public class CheckedItems
    {
        public string columnName { get; set; }
        public bool IsChecked { get; set; }

        public CheckedItems(string colName)
        {
            columnName = colName;
        }
    }
}
