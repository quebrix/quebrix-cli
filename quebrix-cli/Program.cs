using Russel_CLI.Helpers;
using Russel_CLI.Settings;
using System.Text.Json;
using System.Reflection;
using System.Text;

public class Program
{
    public static async Task Main(string[] args)
    {

        try
        {
            Console.InputEncoding = Encoding.Unicode;
            Console.OutputEncoding = Encoding.Unicode;
            PrintQuebrix();
            var configJson = await File.ReadAllTextAsync("CLI_Config.json");
            var config = JsonSerializer.Deserialize<Config>(configJson);
            var port = config.ApiSettings.Port;
            var ip = config.ApiSettings.Ip;
            var client = new ApiClient($"http://{ip}:{port}");
            await CommandHandler.HandleCommand(client,$"{ip}:{port}");
        }
        catch (Exception ex)
        {
            ex.Message.WriteError();
        }
      
    }

    public static void PrintQuebrix()
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
}
