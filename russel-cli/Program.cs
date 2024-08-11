using Russel_CLI.Helpers;
using Microsoft.Extensions.Configuration;
using Russel_CLI.Settings;
using System.Text.Json;
using RestSharp;

public class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            PrintRussel();
            var configJson = await File.ReadAllTextAsync("CLI_Config.json");
            var config = JsonSerializer.Deserialize<Config>(configJson);
            var port = config.ApiSettings.Port;
            var ip = config.ApiSettings.Ip;
            var client = new ApiClient($"http://{ip}:{port}");
            await CommandHandler.HandleCommand(client);
        }
        catch (Exception ex)
        {
            ex.Message.WriteError();
        }
      
    }

    public static void PrintRussel()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("" +
            "");

        Console.WriteLine("                      |============================================================================|");
        Console.WriteLine("                      ||  //////////  //     //   /////////   /////////    /////////    //        ||");
        Console.WriteLine("                      ||  //      //  //     //   //     /    //     /     //           //        ||");
        Console.WriteLine("                      ||  //     //   //     //   //          //           //           //        ||");
        Console.WriteLine("                      ||  ////////    //     //   /////////   /////////    /////////    //        ||");
        Console.WriteLine("                      ||  //     //   //     //          //          //    //           //        ||");
        Console.WriteLine("                      ||  //     //   //     //   /     //    /     //     //           //        ||");
        Console.WriteLine("                      ||  //     //   ////////   /////////   /////////     /////////    ///////// ||");
        Console.WriteLine("                      |============================================================================|");
        Console.WriteLine("                        VERSION 0.1.0" +
            "");
        Console.WriteLine("======================================= FOR CHECKING CONNECTION TYPE [ping] ============================================");

        Console.ResetColor();
    }
}
