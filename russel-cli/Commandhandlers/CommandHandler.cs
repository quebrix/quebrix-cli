using Russel_CLI.Helpers;

public static class CommandHandler
{
    public static async Task HandleCommand(ApiClient client)
    {
        while (true)
        {
            PrintPrompt();

            var input = Console.ReadLine().Trim();
            var parts = input.Split(new[] { ' ' }, 5);
            if (parts.Length == 0)
            {
                continue;
            }

            switch (parts[0])
            {
                case "ping":
                    {
                        await client.CheckConnection();
                        break;
                    }
                case "set" when parts.Length == 5 || parts.Length == 4:
                    {
                        if(parts.Length == 4)
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            var value = parts[3];
                            await client.Set(cluster, key, value);
                        }
                        else
                        {
                            var cluster = parts[1];
                            var key = parts[2];
                            var value = parts[3];
                            var expireTime = Convert.ToInt64(parts[4]);
                            await client.Set(cluster, key, value,expireTime);
                        }
                       
                        break;
                    }
                case "set_cluster" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.SetCluster(cluster); 
                        break;
                    }
                case "keys*" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        var keys = await client.GetKeysOfCluster(cluster);
                        $"Keys in cluster [{cluster}]: {string.Join(", ", keys)}".WriteResponse();
                        break;
                    }
                case "get" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        var value = await client.Get(cluster, key);
                        $"{value}".WriteResponse();
                        break;
                    }
                case "delete" when parts.Length == 3:
                    {
                        var cluster = parts[1];
                        var key = parts[2];
                        await client.Delete(cluster, key);
                        $"Deleted {key} from cluster [{cluster}]".WriteResponse();
                        break;
                    }
                case "clear_cluster" when parts.Length == 2:
                    {
                        var cluster = parts[1];
                        await client.ClearCluster(cluster);
                        $"Cleared cluster [{cluster}]".WriteResponse();
                        break;
                    }
                case "cluster*":
                    {
                        var clusters = await client.GetAllClusters();
                        $"Clusters are: {string.Join(", ", clusters)}".WriteResponse();
                        break;
                    }
                case "help":
                    {
                        PrintHelp();
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
        "set [cluster name] [key] [value] - Set value for key in cluster".WriteInfo();
        "set [cluster name] [key] [value] [ttl in millisecond] - Set value for key with time life in cluster".WriteInfo();
        "set_cluster [cluster name] - Set a new cluster".WriteInfo();
        "get [cluster name] [key] - Get value for key in cluster".WriteInfo();
        "delete [cluster name] [key] - Delete key from cluster".WriteInfo();
        "clear_cluster [cluster name] - Clear all keys in cluster".WriteInfo();
        "cluster* - Get list of all clusters".WriteInfo();
        "-v - Show version".WriteInfo();
        "help - Show this help message".WriteInfo();
    }

}
