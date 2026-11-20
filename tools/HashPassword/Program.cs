using System;

namespace HashPassword;

class Program
{
    static void Main(string[] args)
    {
        var password = Console.ReadLine();
        if (string.IsNullOrEmpty(password))
        {
            Console.Error.WriteLine("Error: No password provided on standard input.");
            Environment.Exit(1);
        }

        // Generate BCrypt hash using work factor 11 (matching VoyageHub AuthService)
        var hash = BCrypt.Net.BCrypt.HashPassword(password.TrimEnd('\r', '\n'), 11);
        Console.WriteLine(hash);
    }
}
