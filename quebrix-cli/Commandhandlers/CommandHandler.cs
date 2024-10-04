using Newtonsoft.Json.Linq;
using Russel_CLI.Helpers;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;

public static class CommandHandler
{
    private static string ReadPassword()
    {
        string password = string.Empty;
        ConsoleKey key;

        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && password.Length > 0)
            {
                password = password[..^1];
                Console.Write("\b \b");
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                password += keyInfo.KeyChar;
                "*".WriteInfoInLine();
            }
        }
        while (key != ConsoleKey.Enter);

        return password;
    }
    public static async Task HandleCommand(ApiClient client, string connectionString)
    {
        string mainUserName = string.Empty;
        string mainPassword = string.Empty;
        bool isLoggedIn = false;

        while (true)
        {
            if (!isLoggedIn)
            {
                "user_name:".WriteResponseInLine();
                var userName = Console.ReadLine();
                "password:".WriteResponseInLine();
                var password = ReadPassword();

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    "You must provide a user_name and password.".WriteError();
                    continue;
                }

                var result = await client.Authenticate(userName, password);
                if (result)
                {
                    mainUserName = userName;
                    mainPassword = password;
                    isLoggedIn = true;
                    "".WriteResponse();
                    "Logged in successfully.".WriteGreen();
                }
                else
                {
                    "Authentication failed.".WriteError();
                    continue;
                }
            }

            // Process commands once logged in
            PrintPrompt(connectionString);
            var input = Console.ReadLine().Trim();
            var parts = input.Split(new[] { ' ' }, 5);
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0])
            {
                case "ACL" when parts.Length == 1:
                    {
                        "invalid command".WriteError();
                        "ACL <options> [userName ...]".WriteTip();
                        break;
                    }
                case "ACL" when parts.Length == 3 && parts[1] == "DELUSER":
                    {
                        await client.DeleteUser(parts[2], mainUserName, mainPassword);
                        break;
                    }
                case "ACL" when parts.Length == 2:
                    {
                        if (parts[1] == "LOAD")
                            await client.LoadUserFromFile(mainUserName, mainPassword);
                        else if (parts[1] == "WHO_AM_I")
                            await client.WhoAmI(mainUserName, mainPassword);
                        else if (parts[1] == "LIST")
                            await client.ListOfUsers(mainUserName, mainPassword);
                        else
                            PrintHelp();
                        break;
                    }
                case "ACL" when parts.Length == 5 && parts[1] == "SETUSER":
                    {
                        var user = parts[2];
                        var password = parts[3];
                        var role = parts[4];
                        await client.AddUser(user, password, role, mainUserName, mainPassword);
                        break;
                    }
                case "ping":
                    {
                        await client.CheckConnection();
                        break;
                    }
                case "SET" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        "value:".WriteResponse();
                        var value = Console.ReadLine();
                        if (string.IsNullOrEmpty(value))
                        {
                            "error value can not be bull".WriteError();
                            break;
                        }
                        "ttl:".WriteResponse();
                        var ttl = Console.ReadLine();

                        if (string.IsNullOrEmpty(ttl))
                        {
                            await client.Set(cluster, key, value, mainUserName, mainPassword);
                        }
                        else
                        {
                            var expireTime = Convert.ToInt64(ttl);
                            await client.Set(cluster, key, value, mainUserName, mainPassword, expireTime);
                        }
                        break;
                    }
                case "INCR" when parts.Length == 4 || parts.Length == 3:
                    {
                        if (parts.Length == 4)
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            var value = parts[3];
                            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int result))
                                await client.INCR(cluster, key, result, mainUserName, mainPassword);
                            else
                            {
                                "invalid enumerator. input must be number".WriteError();
                                "Tip => INCR [cluster] [key] [number] => INCR dev incr 2".WriteTip();
                            }

                        }
                        else
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            await client.INCR(cluster, key, null, mainUserName, mainPassword);
                        }

                        break;
                    }
                case "COPY" when parts.Length == 3:
                    {
                        var srcCluster = parts[1];
                        var destCluster = parts[2];
                        await client.CopyCluster(srcCluster, destCluster, mainUserName, mainPassword);
                        break;
                    }
                case "MOVE" when parts.Length == 3:
                    {
                        var srcCluster = parts[1];
                        var destCluster = parts[2];
                        await client.MoveCluster(srcCluster, destCluster, mainUserName, mainPassword);
                        break;
                    }
                case "DECR" when parts.Length == 4 || parts.Length == 3:
                    {
                        if (parts.Length == 4)
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            var value = parts[3];
                            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int result))
                                await client.DECR(cluster, key, result, mainUserName, mainPassword);
                            else
                            {
                                "invalid enumerator. input must be number".WriteError();
                                "Tip => DECR [cluster] [key] [number] => DECR dev incr 2".WriteTip();
                            }

                        }
                        else
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            await client.DECR(cluster, key, null, mainUserName, mainPassword);
                        }
                        break;
                    }
                case "SET_CLUSTER" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.SetCluster(cluster, mainUserName, mainPassword);
                        break;
                    }
                case "keys*" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        var keys = await client.GetKeysOfCluster(cluster, mainUserName, mainPassword);
                        $"Keys in cluster [{cluster}]: {string.Join(", ", keys)}".WriteResponse();
                        break;
                    }
                case "TYPE" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.TypeOfKey(cluster, key, mainUserName, mainPassword);
                        break;
                    }
                case "EXISTS" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.Exists(cluster, key, mainUserName, mainPassword);
                        break;
                    }
                case "COUNT" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.KeysCount(cluster, mainUserName, mainPassword);
                        break;
                    }
                case "EXPIRE" when parts.Length == 4:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        var ttl = parts[3];
                        if (!string.IsNullOrEmpty(ttl) && int.TryParse(ttl, out int result))
                            await client.Expire(cluster, key, result, mainUserName, mainPassword);
                        else
                        {
                            "invalid enumerator. input must be number".WriteError();
                            "Tip => EXPIRE [cluster] [key] [number] => EXPIRE dev test 2000".WriteTip();
                        }
                        break;
                    }
                case "GET" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        var value = await client.Get(cluster, key, mainUserName, mainPassword);
                        if (value == null)
                            $"Key not found.".WriteError();
                        else
                            $"{value}".WriteResponse();
                        break;
                    }
                case "DELETE" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.Delete(cluster, key, mainUserName, mainPassword);
                        break;
                    }
                case "CLEAR_CLUSTER" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.ClearCluster(cluster, mainUserName, mainPassword);
                        break;
                    }
                case "cluster*":
                    {
                        var clusters = await client.GetAllClusters(mainUserName, mainPassword);
                        $"Clusters are: {string.Join(", ", clusters)}".WriteResponse();
                        break;
                    }
                case "-v":
                    {
                        $"Quebrix cli VERSION: {Assembly.GetExecutingAssembly().GetName().Version}".WriteResponse();
                        break;
                    }
                case "help":
                    {
                        PrintHelp();
                        break;
                    }
                case "LOGOUT":
                    {
                        isLoggedIn = false;
                        mainUserName = string.Empty;
                        mainPassword = string.Empty;
                        "Logged out successfully.".WriteGreen();
                        break;
                    }
                case "cls":
                    {
                        Console.Clear();
                        PrintQuebrix();
                        break;
                    }
                default:
                    {
                        "Invalid command. Use 'help' to see available commands.".WriteInfo();
                        break;
                    }
            }
        }

    }

    private static void PrintPrompt(string connectionString)
    {
        Console.Write($"{connectionString}> ");
    }
    private static void PrintQuebrix()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("" +
            "");

        Console.WriteLine("                 |=====================================================================================|");
        Console.WriteLine("                 ||   ////////    //      //   ||///////   ||//////     ||/////////   ||  \\\\    //    ||");
        Console.WriteLine("                 ||  ||      ||   //      //   ||          ||     ||    ||       //        \\\\  //     ||");
        Console.WriteLine("                 ||  ||      ||   //      //   ||          ||     //    ||      //    ||    \\\\//      ||");
        Console.WriteLine("                 ||  ||      ||   //      //   ||///////   ||/////      ||///////     ||     \\\\\\      ||");
        Console.WriteLine("                 ||  ||      ||   //      //   ||          ||     ||    ||      //    ||     //\\\\     ||");
        Console.WriteLine("                 ||  ||      ///  //      //   ||          ||     //    ||      //    ||    //  \\\\    ||");
        Console.WriteLine("                 ||   ///////      ////////    ||///////   ////////     ||      //    ||   //    \\\\   ||");
        Console.WriteLine("                 |=====================================================================================|");
        Console.WriteLine("");
        Console.WriteLine($"                  VERSION {Assembly.GetExecutingAssembly().GetName().Version}" +
            "");
        Console.ResetColor();
    }
    private static void PrintHelp()
    {
        "SET Commands ================>".WriteDarkCyan();
        "SET [clusterName] [key] - Set value for key in cluster".WriteInfo();
        "TIP => ttl can be null in set but value can not be null or empty".WriteTip();
        "COUNT [clusterName] - for get key count of cluster".WriteInfo();
        "EXISTS [clusterName] [key] - check key exists in cluster or not".WriteInfo();
        "EXPIRE [clusterName] [key] [ttl] - for set expiretion of key".WriteInfo();
        "TYPE [clusterName] [key] - for get type of value".WriteInfo();
        "INCR [clusterName] [key] [enumerable] - for increase command".WriteInfo();
        "TIP => value can be null in INCR".WriteTip();
        "DECR [clusterName] [key] [enumerable] - for decrease command".WriteInfo();
        "TIP => value can be null in DECR".WriteTip();
        "LOGOUT for logout in quebrix".WriteInfo();
        "COPY [src clusterName] [dest clusterName] move values of cluster to destination cluster".WriteInfo();
        "Tip => copy command will override all values in destination if it exists".WriteTip();
        "MOVE [src clusterName] [dest clusterName] move values of cluster to destination cluster then delete src".WriteInfo();
        "Tip => if you used this command src will deleted and it wont be returnable".WriteTip();
        "SET_CLUSTER [clusterName] - Set a new cluster".WriteInfo();
        "GET [clusterName] [key] - Get value for key in cluster".WriteInfo();
        "DELETE [clusterName] [key] - Delete key from cluster".WriteInfo();
        "CLEAR_CLUSTER [clusterName] - Clear all keys in cluster".WriteInfo();
        "cluster* - Get list of all clusters".WriteInfo();
        "keys* [clusterName] - Get all keys in cluster".WriteInfo();
        "ACL Commands ======================>".WriteDarkCyan();
        "ACL WHO_AM_I - for show current user role".WriteInfo();
        "ACL LIST for load all valid users".WriteInfo();
        "ACL LOAD for load all users from cred file in server".WriteInfo();
        "ACL DELUSER [userName] for delete user from server ".WriteInfo();
        "ACL SETUSER [userName] [password] [role]".WriteInfo();
        "TIP => by this command user also delete from file".WriteTip();
        "-v - Show version".WriteInfo();
        "help - Show this help message".WriteInfo();
    }

}
