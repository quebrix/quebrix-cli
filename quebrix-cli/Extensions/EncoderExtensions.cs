using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Russel_CLI.Extensions;

public static class EncoderExtensions
{
    /// <summary>
    /// decode base 64 to string
    /// </summary>
    /// <param name="base64String"></param>
    /// <returns></returns>
    public static string DecodeBase64ToString(this string base64String)
    {
        byte[] decodedBytes = Convert.FromBase64String(base64String);

        string decodedString = Encoding.UTF8.GetString(decodedBytes);

        return decodedString;
    }

    public static string DecodeByteToString(this byte[] bytes)
    {
        string decodedString = Encoding.UTF8.GetString(bytes);
        string value = BitConverter.ToString(bytes);
        return HexStringToString(value);
    }

    private static string HexStringToString(string hexString)
    {
        string[] hexValues = hexString.Split('-');

        byte[] bytes = new byte[hexValues.Length];
        for (int i = 0; i < hexValues.Length; i++)
        {
            bytes[i] = byte.Parse(hexValues[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return Encoding.UTF8.GetString(bytes);
    }
}


