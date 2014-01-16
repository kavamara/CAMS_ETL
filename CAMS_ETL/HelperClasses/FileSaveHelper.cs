using DocumentFormat.OpenXml.Packaging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CAMS_ETL.HelperClasses
{
    public static class FileSaveHelper
    {
        //private bool IsConfigFound(ref string path)
        //{
        //    bool isValid = false;
        //    if (!Directory.Exists(path) || Directory.GetFiles(path).Count() == 0)
        //    {
        //        MessageBox.Show
        //                ("Configuration file(s) not found."
        //                , "Config"
        //                , MessageBoxButton.OK);

        //    }
        //    else
        //    {
        //        isValid = true;
        //    }
        //    path = Properties.Settings.Default.ConfigSavePath;
        //    return isValid;
        //}

        public static string CreateNewPath(string SavePath, string newFolder)
        {
            if (SavePath.LastIndexOf(@"\") != SavePath.Length - 1) SavePath = SavePath + @"\";
            if (newFolder != string.Empty) SavePath = string.Format(@"{0}\{1}\", SavePath, newFolder);
                        
            bool isExists = System.IO.Directory.Exists(SavePath);

            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(SavePath);
            }
            return SavePath;
        }

        public static string CreateNewPath(string path)
        {
            string createPath = System.IO.Path.GetDirectoryName(path);
            bool isExists = System.IO.Directory.Exists(createPath);

            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(createPath);
            }
            return path;

        } 
        public static void SaveXml(string savePath, XDocument doc, string filename)
        {
            if (savePath.LastIndexOf(@"\") != savePath.Length - 1) savePath = savePath + @"\";
            savePath = string.Format("{0}{1}\\", savePath, "Config");
            bool isExists = System.IO.Directory.Exists(savePath);


            if (!isExists)
            {
                System.IO.Directory.CreateDirectory(savePath);
            }
            doc.Save(string.Format("{0}{1}.xml", savePath, filename));

            SaveAppSetting("ConfigSavePath", savePath);
        }

        private static void SaveAppSetting(string setting, string value)
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default["ConfigSavePath"] = value;            
            Properties.Settings.Default.Save();
        }

        private static void ExportTable(System.Data.DataTable table, SpreadsheetDocument workbook)
        {
            var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
            var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
            sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

            DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
            string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

            uint sheetId = 1;
            if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
            {
                sheetId =
                    sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
            sheets.Append(sheet);

            DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

            List<String> columns = new List<string>();
            foreach (System.Data.DataColumn column in table.Columns)
            {
                columns.Add(column.ColumnName);

                DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                headerRow.AppendChild(cell);
            }


            sheetData.AppendChild(headerRow);

            foreach (System.Data.DataRow dsrow in table.Rows)
            {
                DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                foreach (String col in columns)
                {
                    DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                    cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                    cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                    newRow.AppendChild(cell);
                }

                sheetData.AppendChild(newRow);
            }
        }


        public static void ExportDataSet(DataSet ds, string destination)
        {
            CreateNewPath(destination);

            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                foreach (System.Data.DataTable table in ds.Tables)
                {
                    ExportTable(table, workbook);
                }
            }
        }

        public static void ExportDataTable(DataTable table, string destination)
        {
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();
               
                ExportTable(table, workbook);              
            }
        } 
    }
}
