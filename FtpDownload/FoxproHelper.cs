using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace FtpDownload
{
    public class FoxproHelper
    {
        public DataTable GetYourData()
        {
            var data = new DataTable();
            var connectionString = ConfigurationManager.ConnectionStrings["vfpConnectionString"].ConnectionString;
            var handle = new OleDbConnection(connectionString);

            // Open the connection, and if open successfully, you can try to query it
            handle.Open();

            if (handle.State == ConnectionState.Open)
            {
                var mySQL = "select * from Reprint"; // dbf table name

                var myQuery = new OleDbCommand(mySQL, handle);
                var da = new OleDbDataAdapter(myQuery);

                da.Fill(data);

                handle.Close();
            }

            return data;
        }

        public bool InsertFtpRecord(FileDetail fileDetail)
        {
            var lcsql =
                "insert into MasterFtp(FileName, CreateTime, Folder, Records, DlTime) values ( ?, ?,?,?,?)";


            var connectionString = ConfigurationManager.ConnectionStrings["vfpConnectionString"].ConnectionString;
            using (var connection = new OleDbConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SET NULL OFF";

                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = lcsql;
                    command.Parameters.AddWithValue("FileName", fileDetail.FileName);
                    command.Parameters.AddWithValue("CreateTime", fileDetail.FileDate);
                    command.Parameters.AddWithValue("Folder", fileDetail.Folder);
                    command.Parameters.AddWithValue("Records", fileDetail.Records);
                    command.Parameters.AddWithValue("DlTime", fileDetail.DownloadTime);

                    //connection.Open();
                    var retval = command.ExecuteNonQuery();
                    var success = (retval == 1);
                    return success;
                }
            }


        }

        public bool InsertSqlFtpRecord(FileDetail detail)
        {
            var connString = ConfigurationManager.ConnectionStrings["DatabaseContext"].ConnectionString;
            var sql =
                "insert into MasterFtp(FileName, CreateTime, Folder, Records, DlTime) values ( @fileName, @createTime,@folder,@Records,@dlTime)";
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = connString;
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                var p = new SqlParameter
                {
                    ParameterName = "@fileName",
                    DbType = DbType.String,
                    Value = detail.FileName
                };
                cmd.Parameters.Add(p);

                p = new SqlParameter
                {
                    ParameterName = "@createTime",
                    DbType = DbType.DateTime,
                    Value = detail.FileDate
                };
                cmd.Parameters.Add(p);


                p = new SqlParameter
                {
                    ParameterName = "@folder",
                    DbType = DbType.String,
                    Value = detail.Folder
                };
                cmd.Parameters.Add(p);


                p = new SqlParameter
                {
                    ParameterName = "@records",
                    DbType = DbType.Int32,
                    Value = detail.Records
                };
                cmd.Parameters.Add(p);


                p = new SqlParameter
                {
                    ParameterName = "@dlTime",
                    DbType = DbType.DateTime,
                    Value = detail.DownloadTime
                };
                cmd.Parameters.Add(p);

                var retval = cmd.ExecuteNonQuery();
                var success = (retval == 1);
                return success;
            }
        }
    }
}