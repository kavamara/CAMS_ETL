using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using CAMS_ETL.HelperClasses;

namespace CAMS_ETL.Helpers
{
    public class DataSetHelper
    {
        public static DataTable RemoveDuplicates(DataTable dt, DataColumn[] keyColumns, string SavePath, string currentMapping)
        {
            int rowNdx = 0;
            var res = new List<DataTable>();
            DataTable duplicatesTable = dt.Clone();

            var qFields = string.Join(", ", keyColumns.Select(x => "it[\"" + x.ColumnName + "\"] as " + x.ColumnName.Replace(" ","") + ""));
            
            var q = dt.AsEnumerable()
                                .AsQueryable()
                                .GroupBy("new(" + qFields + ")", "it")
                                .Select("new (it as Data)");
            
            var dtemp = dt.Clone();
            

            foreach (dynamic d in q)
            {  
               // dtemp.Rows.Add(d.Data.First().ItemArray());
                rowNdx = 0;
                foreach (var row in d.Data)
                {
                    if (rowNdx == 0)
                    {
                        dtemp.Rows.Add(row.ItemArray);
                    }
                    else
                    {
                        duplicatesTable.Rows.Add(row.ItemArray);
                    }
                    rowNdx++;
                }                
            }
            
            string newFileName = string.Format("{0}{1}", "test", rowNdx);
            FileSaveHelper.ExportDataTable(duplicatesTable, string.Format("{0}\\{1}", SavePath, currentMapping + "_Duplicates.xlsx"));
            return dtemp;
            //while (rowNdx < tbl.Rows.Count - 1)
            //{
            //    DataRow[] dups = FindDups(tbl, rowNdx, keyColumns);
               
            //    if (dups.Length > 0)
            //    {
            //        foreach (DataRow dup in dups)
            //        {
            //            duplicatesTable.ImportRow(dup);
            //            tbl.Rows.Remove(dup);                        
            //        }
            //        tbl.Rows.Add(dups.FirstOrDefault());
            //    }
            //    else
            //    {
            //        rowNdx++;
            //    }
            //}
           // return duplicatesTable;
        }



        private static DataRow[] FindDups(DataTable tbl, int sourceNdx, DataColumn[] keyColumns)
        {
            ArrayList retVal = new ArrayList();
            DataRow sourceRow = tbl.Rows[sourceNdx];

            for (int i = sourceNdx + 1; i < tbl.Rows.Count; i++)
            {
                DataRow targetRow = tbl.Rows[i];
                if (IsDup(sourceRow, targetRow, keyColumns))
                {
                    retVal.Add(targetRow);
                }
            }

            return (DataRow[])retVal.ToArray(typeof(DataRow));
        }

        private static bool IsDup(DataRow sourceRow, DataRow targetRow, DataColumn[] keyColumns)
        {
            bool retVal = true;

            foreach (DataColumn column in keyColumns)
            {
                retVal = retVal && sourceRow[column].Equals(targetRow[column]);
                if (!retVal) break;
            }

            return retVal;
        }
    }
}
