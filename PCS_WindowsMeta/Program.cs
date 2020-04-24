using System;
using System.Data.SQLite;
using System.IO;

namespace PCS_WindowsMeta
{
    class Program
    {
        static string pmsInstall = @"%localappdata%\Plex Media Server\";
        static string pmsDd = @"Plug-in Support\Databases\com.plexapp.plugins.library.db";

        static void Main(string[] args)
        {
            // Messages
            string newPathMsg = "Enter the path of your mount location such as: ";
            string newPathEx = @"(E.g. Z:\, Z:\Media\ or Z:\Shared drives\PlexCloudServers\Media\)";
            string newPathInput = "\nYour path: ";

            // Plex/DB paths
            pmsInstall = Environment.ExpandEnvironmentVariables(pmsInstall);
            pmsDd = Environment.ExpandEnvironmentVariables(pmsDd);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PCS Metadata DB Update Utility (Wind0ws Version)\n");

            // Ask for the user's mount path
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(newPathMsg);
            Console.WriteLine(newPathEx);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(newPathInput);
            Console.ForegroundColor = ConsoleColor.White;

            // Get the input from the user
            string usrPath = Console.ReadLine();

            // Get the confirmation from the user if the path is correct
            while (!confirmation(usrPath))
            {
                // Ask for the user's mount path
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\n" + newPathMsg);
                Console.WriteLine(newPathEx);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(newPathInput);
                Console.ForegroundColor = ConsoleColor.White;

                // Get the input from the user
                usrPath = Console.ReadLine();
            }

            // Make sure that the user's path ends with '\'
            if (!usrPath.EndsWith(@"\"))
            {
                usrPath += @"\";
            }

            // Create the connection
            SQLiteConnection sqlite_conn = CreateConnection();

            // Do the changes in the DB
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUpdating the database, this may take a few seconds...");
            UpdateData(sqlite_conn, usrPath);

            Console.WriteLine("\nFinished updating the database!\n");
            Console.ResetColor();
        }

        // User Path Confirmation
        static bool confirmation(string path)
        {
            Console.ForegroundColor = ConsoleColor.White;
            string conf = string.Format("\nYou entered: '{0}', is this correct? Yes (Y) or No (N): ", path);
            Console.Write(conf);

            string check = Console.ReadLine();
            var charc = check?.ToLower();

            if (charc == "y")
            {
                return true;
            }
            else if (charc == "n")
            {
                return false;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input.");
                Console.ResetColor();
                return confirmation(path);
            }
        }

        // Database Connection
        static SQLiteConnection CreateConnection()
        {
            string dbPath = Path.Combine(pmsInstall, pmsDd);
            string connString = string.Format("Data Source={0}", dbPath);

            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection(connString);

            try
            {
                sqlite_conn.Open();
            }
            catch (Exception) { }

            return sqlite_conn;
        }

        // Update the paths in the Database
        static void UpdateData(SQLiteConnection conn, string new_path)
        {
            SQLiteCommand command = conn.CreateCommand();
            string root_path = @"P:\Shared drives\PlexCloudServers\Media\";
            string root_path_like = @"%P:\Shared drives\PlexCloudServers\Media\%";

            string url_path = @"file://P:\Shared drives\PlexCloudServers\Media\";
            string url_path_like = @"%file://P:\Shared drives\PlexCloudServers\Media\%";

            command.CommandText = "UPDATE section_locations SET root_path = replace(root_path, @root_path, @new_path) WHERE root_path LIKE @root_path_like";
            command.Parameters.Add(new SQLiteParameter("@root_path", root_path));
            command.Parameters.Add(new SQLiteParameter("@new_path", new_path));
            command.Parameters.Add(new SQLiteParameter("@root_path_like", root_path_like));
            command.ExecuteNonQuery();

            command.CommandText = "UPDATE media_streams SET url = replace(url, @url_path, @new_path) WHERE url LIKE @url_path_like";
            command.Parameters.Add(new SQLiteParameter("@url_path", url_path));
            command.Parameters.Add(new SQLiteParameter("@new_path", new_path));
            command.Parameters.Add(new SQLiteParameter("@url_path_like", url_path_like));
            command.ExecuteNonQuery();

            command.CommandText = "UPDATE media_parts SET file = replace(file, @root_path, @new_path) WHERE file LIKE @root_path_like";
            command.Parameters.Add(new SQLiteParameter("@root_path", root_path));
            command.Parameters.Add(new SQLiteParameter("@new_path", new_path));
            command.Parameters.Add(new SQLiteParameter("@root_path_like", root_path_like));
            command.ExecuteNonQuery();

        }
    }
}
