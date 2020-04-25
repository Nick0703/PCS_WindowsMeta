using System;
using System.Data.SQLite;
using System.IO;

namespace PCS_WindowsMeta
{
    class Program
    {
        static void Main(string[] args)
        {
            // Set the title
            Console.Title = "PlexCloudServers DB Update Utility";

            // Messages
            string newCustomPath = "\nEnter your custom Metadata path (e.g. D:\\Plex Media Server\\): ";

            // Plex/DB paths
            string pmsInstall = @"%localappdata%\Plex Media Server\"; ;
            string pmsDd = @"Plug-in Support\Databases\com.plexapp.plugins.library.db";
            string usrMountPath;
            pmsInstall = Environment.ExpandEnvironmentVariables(pmsInstall);
            pmsDd = Environment.ExpandEnvironmentVariables(pmsDd);

            // Display the Main message
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("PCS Metadata DB Update Utility (Wind0ws Version)");
            makeLines();
            Console.ForegroundColor = ConsoleColor.White;

            //bool correctAns;
            string dbPath;
            // Ask for the type of installation
            do
            {
                Console.WriteLine("Please specify where your Plex Metadata is located:\n");
                Console.WriteLine("1. Default (%localappdata%)");
                Console.WriteLine("2. Custom path install\n");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("Enter 1 or 2: ");
                Console.ForegroundColor = ConsoleColor.White;
                string usrOpt = Console.ReadLine();

                // Check which option
                var charc = usrOpt?.ToLower();
                if (charc == "1") // Default metadata path
                {
                    dbPath = Path.Combine(pmsInstall, pmsDd);
                    break;
                }
                else if (charc == "2") // Custom metadata path
                {
                    Console.Write(newCustomPath);
                    string customPath = Console.ReadLine();

                    // Confirm with the user if the path is correct
                    while (!confirmation(customPath))
                    {
                        // Ask for the user's mount path
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(newCustomPath);

                        // Get the input from the user
                        customPath = Console.ReadLine();
                    }

                    // Make sure that custom path ends with '\'
                    if (!customPath.EndsWith(@"\"))
                    {
                        customPath += @"\";
                    }

                    // Set it as environment path
                    customPath = Environment.ExpandEnvironmentVariables(customPath);

                    // Check if that directory exists
                    if (Directory.Exists(customPath))
                    {
                        dbPath = Path.Combine(customPath, pmsDd);
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nPath doesn't exists!\n");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
                else // Wrong answer
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nIncorrect option!\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    //correctAns = false;
                }
            } while (true);

            makeLines();

            // Ask for the user's mount path
            do
            {
                usrMountPath = getMountPath();

                // Get the confirmation from the user if the path is correct
                while (!confirmation(usrMountPath))
                {
                    // Ask for the user's mount path
                    usrMountPath = getMountPath();
                }

                // Make sure that the user's path ends with '\'
                if (!usrMountPath.EndsWith(@"\"))
                {
                    usrMountPath += @"\";
                }

                // Check if that mount path exists
                string usrMount = Environment.ExpandEnvironmentVariables(usrMountPath);
                var pcsMount = Path.Combine(usrMount, "Movies");
                if (Directory.Exists(pcsMount))
                {
                    break;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nThe mount path does not exist!\n");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            } while (true);

            // Create the connection
            makeLines();
            SQLiteConnection sqlite_conn = CreateConnection(dbPath);

            // Do the changes in the DB
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Updating the database, this may take a few seconds...");
            UpdateData(sqlite_conn, usrMountPath);

            Console.WriteLine("\nFinished updating the database!\n");

            // Exit when user presses Enter key
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Press Enter to exit.");
            Console.ResetColor();
            Console.ReadKey(true);
        }

        static void makeLines()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n***********************************************************************************\n");
            Console.ForegroundColor = ConsoleColor.White;
        }

        // Get the mount path
        static string getMountPath()
        {
            string newPathMsg = "Enter the path of your mount location such as: ";
            string newPathEx = @"(E.g. Z:\, Z:\Media\ or Z:\Shared drives\PlexCloudServers\Media\)";
            string newPathInput = "\nYour path: ";

            Console.WriteLine(newPathMsg + "\n" + newPathEx);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(newPathInput);
            Console.ForegroundColor = ConsoleColor.White;

            // Get the input from the user
            string usrMountPath = Console.ReadLine();

            return usrMountPath;
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
        static SQLiteConnection CreateConnection(string dbPath)
        {
            string connString = string.Format("Data Source={0}", dbPath);

            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection(connString);

            try
            {
                sqlite_conn.Open();
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nUnable to open the database\n");
                Console.ResetColor();
            }

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
