using Russel_CLI.Helpers;
using System.Diagnostics;

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
    public static async Task HandleCommand(ApiClient client)
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
            PrintPrompt();
            var input = Console.ReadLine().Trim();
            var parts = input.Split(new[] { ' ' }, 5);
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0])
            {
                case "add_profile" when parts.Length == 4:
                    {
                        var user = parts[1];
                        var password = parts[2];
                        var role = parts[3];
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

    private static void PrintPrompt()
    {
        Console.Write("> ");
    }
    private static void PrintHelp()
    {
        "Commands:".WriteInfo();
        "add_profile [userName] [password] to set new profile".WriteInfo();
        "logout for logout in russel".WriteInfo();
        "set [cluster name] [key] - Set value for key in cluster".WriteInfo();
        "TIP => ttl can be null in set but value can not be null or empty".WriteTip();
        "set [cluster name] [key] [value] [ttl in millisecond] - Set value for key with time life in cluster".WriteInfo();
        "set_cluster [cluster name] - Set a new cluster".WriteInfo();
        "get [cluster name] [key] - Get value for key in cluster".WriteInfo();
        "delete [cluster name] [key] - Delete key from cluster".WriteInfo();
        "clear_cluster [cluster name] - Clear all keys in cluster".WriteInfo();
        "cluster* - Get list of all clusters".WriteInfo();
        "keys* [cluster name] - Get all keys in cluster".WriteInfo();
        "-v - Show version".WriteInfo();
        "help - Show this help message".WriteInfo();
    }

}
