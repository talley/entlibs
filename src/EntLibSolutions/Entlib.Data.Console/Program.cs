using Entlib.Data.Console.Entities;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Microsoft.Practices.EnterpriseLibrary.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Config = System.Configuration.ConfigurationManager;
namespace Entlib.Data.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //EnterpriseDataDemos();
            string commandText =
           "WAITFOR DELAY '00:00:03';" +
           "SELECT * FROM Customers";

            RunCommandAsynchronously(commandText, GetConnection());

            System.Console.WriteLine("Press ENTER to continue.");
            System.Console.ReadLine();
        }

        private static void EnterpriseDataDemos()
        {
            DatabaseProviderFactory factory = new DatabaseProviderFactory();
            //Database database = factory.Create("Default");
            SqlDatabase db = new SqlDatabase(GetConnection());

            var query = "SELECT * FROM Customers as c ORDER BY c.CompanyName";
            var dataset = db.ExecuteDataSet(CommandType.Text, query);

            List<Customers> customers = new List<Customers>();
            // IAsyncResult x1 = db.BeginExecuteReader(CommandType.Text, query, CustomerAsyncResults2, customers);

            IDataReader reader = db.ExecuteReader(CommandType.Text, query);
            while (reader.Read())
            {
                System.Console.WriteLine(string.Concat(reader["CompanyName"], ":", reader["Country"]));
            }
            //IDataReader x2 = db.EndExecuteReader(CustomerAsyncResults);
        }


        private static void RunCommandAsynchronously(
        string commandText, string connectionString)
        {
            // Given command text and connection string, asynchronously execute
            // the specified command against the connection. For this example,
            // the code displays an indicator as it is working, verifying the
            // asynchronous behavior.
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    SqlCommand command = new SqlCommand(commandText, connection);

                    connection.Open();
                    IAsyncResult result = command.BeginExecuteReader();

                    // Although it is not necessary, the following code
                    // displays a counter in the console window, indicating that
                    // the main thread is not blocked while awaiting the command
                    // results.
                    int count = 0;
                    while (!result.IsCompleted)
                    {
                        count += 1;
                        System.Console.WriteLine("Waiting ({0})", count);
                        // Wait for 1/10 second, so the counter
                        // does not consume all available resources
                        // on the main thread.
                        System.Threading.Thread.Sleep(100);
                    }

                    using (SqlDataReader reader = command.EndExecuteReader(result))
                    {
                        DisplayResults(reader);
                    }
                }
                catch (SqlException ex)
                {
                    System.Console.WriteLine("Error ({0}): {1}", ex.Number, ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    System.Console.WriteLine("Error: {0}", ex.Message);
                }
                catch (Exception ex)
                {
                    // You might want to pass these errors
                    // back out to the caller.
                    System.Console.WriteLine("Error: {0}", ex.Message);
                }
            }
        }



        private static void DisplayResults(SqlDataReader reader)
        {
            // Display the data within the reader.
            while (reader.Read())
            {
                // Display all the columns.
                for (int i = 0; i < reader.FieldCount; i++)
                    System.Console.Write("{0} ", reader.GetValue(i));
                System.Console.WriteLine();
            }
        }
        private static void CustomerAsyncResults2(IAsyncResult ar)
        {
            System.Console.WriteLine(ar.IsCompleted);
        }

        static string GetConnection()
        {
            return Config.ConnectionStrings["Default"].ConnectionString;
        }


        public static IAsyncResult CustomerAsyncResults { get; set; }
    }
}
