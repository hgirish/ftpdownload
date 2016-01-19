using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;

namespace FtpDownload
{
    public class FoxproHelper
    {
        public DataTable GetYourData()
        {
            DataTable data = new DataTable();
            var connectionString = ConfigurationManager.ConnectionStrings["vfpConnectionString"].ConnectionString;
            OleDbConnection handle = new OleDbConnection(connectionString);

          // Open the connection, and if open successfully, you can try to query it
            handle.Open();

            if (handle.State == ConnectionState.Open)
            {
                string mySQL = "select * from Reprint";  // dbf table name

                OleDbCommand myQuery = new OleDbCommand(mySQL, handle);
                OleDbDataAdapter da = new OleDbDataAdapter(myQuery);

                da.Fill(data);

                handle.Close();
            }

            return data;
        }

        public void InsertFtpRecord(FileDetail fileDetail)
        {
            var lcsql =
                "insert into MasterFtp(FileName, CreateTime, Folder, Records, DlTime) values ( ?, ?,?,?,?)";


            string connectionString = ConfigurationManager.ConnectionStrings["vfpConnectionString"].ConnectionString;
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SET NULL OFF";

                    command.ExecuteNonQuery();
                }
                using (OleDbCommand command = connection.CreateCommand())
                {
                    command.CommandText = lcsql;
                    command.Parameters.AddWithValue("FileName", fileDetail.FileName);
                    command.Parameters.AddWithValue("CreateTime", fileDetail.FileDate);
                    command.Parameters.AddWithValue("Folder", fileDetail.Folder);
                    command.Parameters.AddWithValue("Records", fileDetail.Records);
                    command.Parameters.AddWithValue("DlTime", fileDetail.DownloadTime);
                 //   command.Parameters.AddWithValue("JobNum", string.Empty);
                  //  command.Parameters.AddWithValue("DbfName", string.Empty);
                   // command.Parameters.AddWithValue("NotifyDate", DateTime.MinValue);
                  //  command.Parameters.AddWithValue("PickupDate", DateTime.MinValue);
                   // command.Parameters.AddWithValue("UsShipDate", DBNull.Value);

                    //connection.Open();
                    command.ExecuteNonQuery();
                }
            }


        }
    }
}