using BiQ.AlertIntegrationDemo;
using Microsoft.Data.Sqlite;

namespace CustomerSystemDebug
{
    internal class Program
    {
        /// <summary>
        /// Simple command util to list all current customers in the mock customer system
        /// Intended for debug info. Is not part of the integration with BiQ.
        /// </summary>
        /// <param name="args">Not used</param>
        static void Main(string[] args)
        {
            Console.WriteLine("Db file at:" + ConfigValues.CustomerSystemConnectionString);
            Console.WriteLine("Args: list-all, count, delete-all");
            if (args.Length > 0 && args[0] == "list-all")
            {
                Console.WriteLine("All current customers:");
                using (var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT * from Customers";
                    using var reader = command.ExecuteReader();
                    while (true)
                    {
                        if (!reader.Read())
                            return;

                        var row = "";
                        var seperator = "";
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row += seperator + reader.GetName(i) + ": ";
                            if (reader.GetValue(i) == DBNull.Value)
                                row += "NULL";
                            else
                                row += "\"" + Convert.ToString(reader.GetValue(i)) + "\"";
                            seperator = ", ";
                        }
                        Console.WriteLine(row);
                        Console.WriteLine();
                    }
                }
            }
            if (args.Length > 0 && args[0] == "count")
            {
                using (var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"SELECT COUNT(*) from Customers";
                    var count = command.ExecuteScalar();
                    Console.WriteLine("Count of customers: " + count);
                }
            }
            if (args.Length > 0 && args[0] == "delete-all")
            {
                using (var connection = new SqliteConnection(ConfigValues.CustomerSystemConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = @"DELETE FROM Customers";
                    var count = command.ExecuteNonQuery();
                    Console.WriteLine("Deleted " + count + " customers");
                }
            }
        }
    }
}
