
//using Microsoft.SqlServer.Management.Common;
//using Microsoft.SqlServer.Management.Smo;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CAMS_ETL.Helpers
//{
//    public class DBHelper
//    {
//        public DataSet ds { get; set; }

//        public void CreateDataBase(string dbName)
//        {
//            if (ds.Tables.Count == 0) return;
//            try
//            {
//                Microsoft.SqlServer.Management.Smo.Server server =
//                    new Microsoft.SqlServer.Management.Smo.Server(new ServerConnection()
//                    {
//                        ConnectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString,                       

//                    });
//                Database db = new Database(server, "master");
//                db.Name = dbName;
//                //db.Drop();

//                AddDBTables(db);
//                CreateDatabse_and_tables(db);
//                //CreateForeignKeysForTables(db);
               
//                db.AutoClose = true;
//                //BulkInsertData();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception(ex.Message);

//            }

//        }

//        private bool IsDatabaseExist(Database db)
//        {
//            string sqlconnString = ConfigurationManager.ConnectionStrings[1].ConnectionString;
//            string cmdText = string.Format("Select dbid from master.dbo.sysdatabases where name = '{0}'", db.Name);

//            bool bRet = false;
//            using (SqlConnection sqlConn = new SqlConnection(sqlconnString))
//            {
//                sqlConn.Open();
//                using (SqlCommand sqlCmd = new SqlCommand(cmdText, sqlConn))
//                {
//                    object temp = sqlCmd.ExecuteScalar();
//                    if (temp != null)
//                    {
//                        int dbid = (short)temp;
//                        sqlConn.Close();
//                        bRet = dbid > 0;
//                    }
//                }
//            }
//            return bRet;
//        }  

//        public void CreateDatabse_and_tables(Database db)
//        {
//            if (!IsDatabaseExist(db))
//            {
//                db.Create();
//            }
//            else
//            {
//                throw new Exception("Database exists");
//            }

//            foreach (Microsoft.SqlServer.Management.Smo.Table table in db.Tables)
//            {
//                try
//                {

//                    table.Create();
//                }
//                catch
//                {
//                    throw;
//                }
//            }

//        }
//        void AddDBTables(Database db)
//        {
//            foreach (DataTable tbl in ds.Tables)
//            {
//                Microsoft.SqlServer.Management.Smo.Table table =
//                    new Microsoft.SqlServer.Management.Smo.Table(db, tbl.TableName);
//                //Adding DB columns to database table
//                //using DataSet table columns
//                AddDBColumns(table, tbl);
//                db.Tables.Add(table);
//            }
//        }

//        void AddDBColumns(Microsoft.SqlServer.Management.Smo.Table table, DataTable datatable)
//        {
//            foreach (DataColumn col in datatable.Columns)
//            {

//                Column tblcol = new Column();
//                tblcol.Name = col.ColumnName;
//                //tblcol.Create();
//                table.Columns.Add(tblcol);
//                if (col.DataType.Name == "String")
//                {
//                    tblcol.DataType = DataType.NVarChar(col.MaxLength);

//                }
//                if (col.DataType.Name == "Int32" || col.DataType.Name == "Int64")
//                {
//                    tblcol.DataType = DataType.Int;
//                }
//                tblcol.Default = col.DefaultValue.ToString();

//                tblcol.Nullable = col.AllowDBNull;
//                if (col.AutoIncrement)
//                {
//                    tblcol.Nullable = false;
//                    Index idx = new Index(table, "pk_" + col.ColumnName);
//                    idx.IndexedColumns.Add(new IndexedColumn(idx, tblcol.Name));
//                    idx.IsClustered = true;
//                    idx.IsUnique = true;
//                    idx.IndexKeyType = IndexKeyType.DriPrimaryKey;
//                    table.Indexes.Add(idx);

//                    tblcol.Identity = col.AutoIncrement;
//                    tblcol.IdentityIncrement = col.AutoIncrementStep;
//                    tblcol.IdentitySeed = col.AutoIncrementSeed;
//                }

//            }


//        }

//        //Once Database is Created with table objects.
//        //we can populate Database  tables (just now created step2) using ADO.NET Bulk Insert

//        //Step 3) Bulk Insert Datatable into SqlServer

//        public void BulkInsertData(string dbname)
//        {
//            try
//            {                
//                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings[1].ConnectionString);
//                sb.InitialCatalog = dbname;

//                SqlBulkCopy blkCopy = new SqlBulkCopy(sb.ConnectionString, SqlBulkCopyOptions.KeepIdentity);
//                foreach (DataTable table in ds.Tables)
//                {
//                    blkCopy.DestinationTableName = table.TableName;
//                    blkCopy.WriteToServer(table);
//                }
//                blkCopy.Close();
//            }
//            catch
//            {
//                throw;
//            }
//        }
//    }
    
//}
