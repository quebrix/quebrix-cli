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
    public static async Task HandleCommand(ApiClient client,string connectionString)
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
                case "set" when parts.Length == 3:
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
                case "MOVE" when parts.Length == 3:
                    {
                        var srcCluster = parts[1];
                        var destCluster = parts[2];
                        await client.MoveClusterValues(srcCluster,destCluster,mainUserName,mainPassword);
                        break;
                    }
                case "MOVEDEL" when parts.Length == 3:
                    {
                        var srcCluster = parts[1];
                        var destCluster = parts[2];
                        await client.MoveAndDeleteClusterValues(srcCluster, destCluster, mainUserName, mainPassword);
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
                case "set_cluster" when parts.Length == 2:
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
                case "get" when parts.Length == 3:
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
                case "delete" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.Delete(cluster, key, mainUserName, mainPassword);
                        break;
                    }
                case "clear_cluster" when parts.Length == 2:
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
                case "logout":
                    {
                        isLoggedIn = false;
                        mainUserName = string.Empty;
                        mainPassword = string.Empty;
                        "Logged out successfully.".WriteGreen();
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
    private static void PrintHelp()
    {
        "SET Commands ================>".WriteDarkCyan();
        "set [cluster name] [key] - Set value for key in cluster".WriteInfo();
        "INCR [clusterName] [key] [enumerable] - for increase command".WriteInfo();
        "TIP => value can be null in INCR".WriteTip();
        "DECR [clusterName] [key] [enumerable] - for decrease command".WriteInfo();
        "TIP => value can be null in DECR".WriteTip();
        "logout for logout in quebrix".WriteInfo();
        "MOVE [src clusterName] [dest clusterName] move values of cluster to destination cluster".WriteInfo();
        "Tip => move command will override all values in destination if it exists".WriteTip();
        "MOVEDEL [src clusterName] [dest clusterName] move values of cluster to destination cluster then delete src".WriteInfo();
        "Tip => if you used this command src will deleted and it wont be returnable".WriteTip();
        "TIP => ttl can be null in set but value can not be null or empty".WriteTip();
        "set [cluster name] [key] [value] [ttl in millisecond] - Set value for key with time life in cluster".WriteInfo();
        "set_cluster [cluster name] - Set a new cluster".WriteInfo();
        "get [cluster name] [key] - Get value for key in cluster".WriteInfo();
        "delete [cluster name] [key] - Delete key from cluster".WriteInfo();
        "clear_cluster [cluster name] - Clear all keys in cluster".WriteInfo();
        "cluster* - Get list of all clusters".WriteInfo();
        "keys* [cluster name] - Get all keys in cluster".WriteInfo();
        "ACL Commands ======================>".WriteDarkCyan();
        "ACL LIST for load all valid users".WriteInfo();
        "ACL LOAD for load all users from cred file in server".WriteInfo();
        "ACL DELUSER [userName] for delete user from server ".WriteInfo();
        "ACL SETUSER [userName] [password] [role]".WriteInfo();
        "TIP => by this command user also delete from file".WriteTip();
        "-v - Show version".WriteInfo();
        "help - Show this help message".WriteInfo();
    }

}
