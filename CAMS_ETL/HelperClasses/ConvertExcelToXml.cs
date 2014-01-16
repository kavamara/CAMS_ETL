/****************************** Module Header ******************************\
* Module Name:  ConvertExcelToXml.cs
* Project:      CSOpenXmlExcelToXml
* Copyright(c)  Microsoft Corporation.
* 
* This class is used to convert excel data to XML format string using Open XMl
* Firstly, we use OpenXML API to get data from Excel to DataTable
* Then we Load the DataTable to Dataset.
* At Last,we call DataSet.GetXml() to get XML format string 
*
* This source is subject to the Microsoft Public License.
* See http://www.microsoft.com/en-us/openness/licenses.aspx.
* All other rights reserved.
* 
* THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
* EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
* WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/


using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.OleDb;
using System.Xml;
using System.Xml.Linq;
using CAMS_ETL.Helpers;

namespace CSOpenXmlExcelToXml
{
    public class ConvertExcelToXml
    {
        /// <summary>
        ///  Read Data from selected excel file into DataTable
        /// </summary>
        /// <param name="filename">Excel File Path</param>
        /// <returns></returns>
        private DataTable ReadExcelFile(string filename, bool renameTableName, int skip)
        {
            // Initialize an instance of DataTable
            DataTable dt = new DataTable();

            try
            {
                // Use SpreadSheetDocument class of Open XML SDK to open excel file
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
                {
                    // Get Workbook Part of Spread Sheet Document
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    // Get all sheets in spread sheet document 
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    // Get relationship Id
                    string relationshipId = sheetcollection.First().Id.Value;
                    if (renameTableName) dt.TableName = sheetcollection.First().Name.Value.Replace(" ", "");
                    // Get sheet1 Part of Spread Sheet Document
                    WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);

                    // Get Data in Excel file
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    IEnumerable<Row> rowcollection = sheetData.Descendants<Row>().Skip(skip);

                    if (rowcollection.Count() == 0)
                    {
                        return dt;
                    }

                    // Add columns
                    foreach (Cell cell in rowcollection.ElementAt(0))
                    {
                        dt.Columns.Add(GetValueOfCell(spreadsheetDocument, cell));
                    }

                    // Add rows into DataTable
                    foreach (Row row in rowcollection)
                    {
                        DataRow temprow = dt.NewRow();
                        int columnIndex = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            // Get Cell Column Index
                            int cellColumnIndex = GetColumnIndex(GetColumnName(cell.CellReference));

                            if (columnIndex < cellColumnIndex)
                            {
                                do
                                {
                                    if (temprow.ItemArray.ElementAtOrDefault(columnIndex) != null)
                                    {
                                        temprow[columnIndex] = string.Empty;                                        
                                    }
                                    columnIndex++;
                                }

                                while (columnIndex < cellColumnIndex);
                            }

                            if (temprow.ItemArray.ElementAtOrDefault(columnIndex) != null)
                            {
                                temprow[columnIndex] = GetValueOfCell(spreadsheetDocument, cell);
                            }
                            columnIndex++;
                        }

                        // Add the row to DataTable
                        // the rows include header row
                        dt.Rows.Add(temprow);
                    }
                }

                // Here remove header row
                dt.Rows.RemoveAt(0);
                return dt;
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        public DataTable ReadFirstRow(string filename, int skip)
        {
            // Initialize an instance of DataTable
            DataTable dt = new DataTable();

            try
            {
                // Use SpreadSheetDocument class of Open XML SDK to open excel file
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
                {
                    // Get Workbook Part of Spread Sheet Document
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    // Get all sheets in spread sheet document 
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    // Get relationship Id
                    string relationshipId = sheetcollection.First().Id.Value;

                    // Get sheet1 Part of Spread Sheet Document
                    WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);

                    // Get Data in Excel file
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                  //  IEnumerable<Row> rowcollection = sheetData.Descendants<Row>();
                    Row columns = sheetData.Descendants<Row>().Skip(skip).FirstOrDefault();
                    if (columns == null)
                    {
                        return dt;
                    }

                    // Add columns
                    foreach (Cell cell in columns)
                    {
                        dt.Columns.Add(GetValueOfCell(spreadsheetDocument, cell));
                    }
                }             
                return dt;
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        ///  Get Value of Cell 
        /// </summary>
        /// <param name="spreadsheetdocument">SpreadSheet Document Object</param>
        /// <param name="cell">Cell Object</param>
        /// <returns>The Value in Cell</returns>
        private static string GetValueOfCell(SpreadsheetDocument spreadsheetdocument, Cell cell)
        {
            // Get value in Cell
            SharedStringTablePart sharedString = spreadsheetdocument.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return string.Empty;
            }

            string cellValue = cell.CellValue.InnerText;

            // The condition that the Cell DataType is SharedString
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return sharedString.SharedStringTable.ChildElements[int.Parse(cellValue)].InnerText;
            }
            else
            {
                return cellValue;
            }
        }

        /// <summary>
        /// Get Column Name From given cell name
        /// </summary>
        /// <param name="cellReference">Cell Name(For example,A1)</param>
        /// <returns>Column Name(For example, A)</returns>
        private string GetColumnName(string cellReference)
        {
            if (cellReference != null)
            {
                // Create a regular expression to match the column name of cell
                Regex regex = new Regex("[A-Za-z]+");
                Match match = regex.Match(cellReference);
                return match.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Index of Column from given column name
        /// </summary>
        /// <param name="columnName">Column Name(For Example,A or AA)</param>
        /// <returns>Column Index</returns>
        private int GetColumnIndex(string columnName)
        {
            int columnIndex = 0;
            int factor = 1;

            // From right to left
            for (int position = columnName.Length - 1; position >= 0; position--)
            {
                // For letters
                if (Char.IsLetter(columnName[position]))
                {
                    columnIndex += factor * ((columnName[position] - 'A') + 1) - 1;
                    factor *= 26;
                }
            }

            return columnIndex;
        }

        private XDocument DtToXML(DataTable dt)
        {
            using (var stream = new MemoryStream())
            {
                dt.WriteXml(stream);
                stream.Position = 0;
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ConformanceLevel = ConformanceLevel.Fragment;
                XmlReader reader = XmlReader.Create(stream, settings);
                reader.MoveToContent();
                if (reader.IsEmptyElement) { reader.Read(); return null; }
                return XDocument.Load(reader);
            }
        }
        /// <summary>
        /// Convert DataTable to Xml format
        /// </summary>
        /// <param name="filename">Excel File Path</param>
        /// <returns>Xml format string</returns>
        public XDocument GetXML(string filename, int skip)
        {
            string f = Path.GetFileNameWithoutExtension(filename);
            using (DataSet ds = new DataSet())
            {
                ds.Tables.Add(this.ReadExcelFile(filename, false, skip));
                return DtToXML(ds.Tables[0]);
            }
        }

        public string GetColumnsXML(string filename, int skip)
        {
            //string f = Path.GetFileNameWithoutExtension(filename);

            using (DataSet ds = new DataSet())
            {
                //ds.Tables.Add(f);
                ds.Tables.Add(this.ReadFirstRow(filename,skip));
                return ds.GetXml();
            }
        }

        public DataSet GetColumnsDataSet(string filename, int skip)
        {
            //string f = Path.GetFileNameWithoutExtension(filename);

            using (DataSet ds = new DataSet())
            {
                //ds.Tables.Add(f);
                ds.Tables.Add(this.ReadFirstRow(filename,skip));
                return ds;
            }

        }
        public string StoreInDB(List<string> filenames, string dbName)
        {
            //try
            //{
            //    using (DataSet ds = new DataSet())
            //    {
            //        foreach (string f in filenames)
            //        {
            //            ds.Tables.Add(this.ReadExcelFile(f, true));
            //        }
            //        DBHelper db = new DBHelper();
            //        db.ds = ds;

            //        db.CreateDataBase(dbName);
            //        db.BulkInsertData(dbName);
            //    }
            //}
            //catch
            //{
            //    throw;
            //}
            return "";
        }

        private static DataTable makeDataTableFromSheetName(string filename, string sheetname)
        {
            string excelCon = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0;HDR=YES;IMEX=1\"", filename);
            DataTable dtImport = new DataTable();
            OleDbDataAdapter importCommand = new OleDbDataAdapter("select * from [" + sheetname + "]", excelCon);
            importCommand.Fill(dtImport);
            return dtImport;
        }
    }
}
