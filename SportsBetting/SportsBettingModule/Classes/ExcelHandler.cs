using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data.OleDb;
using System.Timers;

namespace SportsBettingModule.Classes
{
    class ExcelHandler
    {
        private OleDbConnection connection { get; set; }
        private OleDbCommand cmd { get; set; }
        public string errorMsg { get; set; }
        public string connectionString { get; set; }
        public bool isConnected { get; private set; }
        public List<string> LoggingColumns { get; set; }    //Accesible list of columns to log in datafile
        // Declare the delegate (if using non-generic pattern).
        public delegate void RaiseError(object sender, string error);

        // Declare the event.
        public event RaiseError ErrorOccured;





        public ExcelHandler(string filename)
        {

            connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filename + ";Extended Properties=Excel 12.0;Persist Security Info=True";

            //connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Dsn=Excel Files;dbq=C:\Users\jewar\source\repos\MMABetting\SoccerBettingModule\SoccerBettingModule\excel\mmaBets.xlsx;defaultdir=C:\Users\jewar\source\repos\MMABetting\SoccerBettingModule\SoccerBettingModule\excel;driverid=1046;maxbuffersize=2048;pagetimeout=5";
            //connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Driver ={ Driver do Microsoft Excel(*.xls)};Data Source=" + filename + ";Extended Properties=Excel 8.0;HDR=YES;";
            //connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0; Driver ={ Driver do Microsoft Excel(*.xls)}; dbq = C:\Users\jewar\source\repos\MMABetting\SoccerBettingModule\SoccerBettingModule\excel\mmaBets.xlsx; defaultdir = C:\Users\jewar\source\repos\MMABetting\SoccerBettingModule\SoccerBettingModule\excel; driverid = 790; fil = excel 8.0; filedsn = C:\Users\jewar\source\repos\MMABetting\SoccerBettingModule\SoccerBettingModule\excel\MMABetting.dsn; maxbuffersize = 2048; maxscanrows = 8; pagetimeout = 5; readonly= 0; safetransactions = 0; threads = 3; uid = admin; usercommitsync = Yes";
        }

        public bool ConnectToExcel()
        {
            Task<bool> connect = new Task<bool>(() =>
            {
                try
                {
                    connection = new OleDbConnection(connectionString);
                    cmd = new OleDbCommand();
                    cmd.Connection = connection;
                    cmd.Connection.Open();
                    cmd.Connection.Close();
                    cmd.CommandText = "";
                    if (LoggingColumns == null) LoggingColumns = new List<string>();
                    //cmd.CommandText = "Select * from [Bets]";
                    //var Reader = cmd.ExecuteReader();
                    isConnected = true;
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorOccured?.Invoke(this, ex.Message);
                    isConnected = false;
                    return false;
                }
            });

            connect.Start();
            return connect.Result;
        }

        public bool StartLogging(int minutes)
        {
            bool started = false;
            System.Timers.Timer timer = new System.Timers.Timer(minutes * 36000);
            timer.AutoReset = true;
            timer.Elapsed += LogData;


            timer.Enabled = true;

            return started;
        }

        private void LogData(object sender, ElapsedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        //public bool OpenConnection()
        //{
        //    try
        //    {
        //        cmd.Connection.Open();
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorOccured?.Invoke(this, ex.Message);
        //        return false;
        //    }


        //}

        public void CreateTable(string name, List<string> Columns)
        {
            //Workbook workbook = new Workbook();
            if (LoggingColumns == null) LoggingColumns = new List<string>();
            //OpenConnection();
            //cmd.CommandText = "Insert INTO [Bets$] (Test, Test2) Values ('2','B')";
            //cmd.ExecuteNonQuery();
            //CloseConnection();

            string columnlist = "";

            foreach (string col in Columns)
            {
                columnlist += "(" + col + " char(255)) , ";
            }

            //columnlist = columnlist.Substring(0,columnlist.Length - 3);


            string command = "Create Table Bettin (F1 char(255), F2 char(255))";

            ExecuteNonQuery(command);
            //cmd.CommandText = "ALTER TABLE [Bets$] ADD Testing VARCHAR(50)";

        }

        public bool AddColumn(string col)
        {
            string command = "ALTER TABLE [MySheet$] ADD COLUMN " + col + " CHAR(255)";

            return ExecuteNonQuery(command);
        }

        public void ShutdownConnection()
        {

            Task shutdown = new Task(() =>
            {


                if (connection != null)
                {
                    cmd.Connection.Close();

                    int i = 0;
                    while (connection.State == System.Data.ConnectionState.Open && i < 30)
                    {
                        System.Threading.Thread.Sleep(100);
                        i++;
                    }
                    //If still not closed
                    if (connection.State != System.Data.ConnectionState.Closed)
                    {
                        if (ErrorOccured != null)
                            ErrorOccured(this, "Could not close Connection to Excel");
                    }
                }

            });
            shutdown.Start();




        }

        private bool ExecuteNonQuery(string commandText)
        {
            if (cmd.Connection == null) ConnectToExcel();
            try
            {
                //Task<bool> connect = new Task<bool>(()=>{

                //    if (connection.State != System.Data.ConnectionState.Open)
                //        connection.Open();
                //    int i = 0;
                //    while(connection.State == System.Data.ConnectionState.Connecting)
                //    {
                //        System.Threading.Thread.Sleep(100);
                //    }
                //    if (connection.State == System.Data.ConnectionState.Open) {
                //        try
                //        {
                //            return true;
                //        }
                //        catch(Exception ex)
                //        {

                //            if (ErrorOccured != null)
                //                ErrorOccured(this, "Could not complete command: " + ex.Message);
                //            return false;

                //        }

                //    } 
                //    else return false;


                //});
                //connect.Start();
                //connect.ContinueWith(antecedant =>
                //{
                //    if (antecedant.Result)
                //    {
                //        //cmd.CommandText = commandText;
                //        //cmd.ExecuteNonQuery();
                //        //connection.Close();

                //    }
                //    else
                //    {
                //        //
                //    }

                //});


                try
                {
                    if (cmd.Connection.State != System.Data.ConnectionState.Open)
                        cmd.Connection.Open();


                    if (cmd.Connection.State == System.Data.ConnectionState.Open)
                    {
                        cmd.CommandText = commandText;
                        int test = cmd.ExecuteNonQuery();

                        cmd.Connection.Close();
                        connection = null;
                        return true;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    cmd.Connection.Close();
                    return false;
                }


            }
            catch (Exception ex)
            {
                if (ErrorOccured != null)
                    ErrorOccured(this, "Failed to Send Command to Excel, " + ex.Message);
                if (connection.State != System.Data.ConnectionState.Closed)
                    cmd.Connection.Close();
                return false;
            }



        }

        public void RaiseErrorEventHandler(Exception ex)
        {
            ErrorOccured?.Invoke(this, ex.Message);
        }

        //public bool CloseConnection()
        //{
        //    try
        //    {
        //        cmd.Connection.Close();
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        ErrorOccured?.Invoke(this, ex.Message);
        //        return false;
        //    }

        //}
    }
}
