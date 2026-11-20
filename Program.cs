using Microsoft.AspNetCore.Identity;

var hasher = new PasswordHasher<object>();

Console.Write("Enter test password: ");
var password = Console.ReadLine();

if (string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Password cannot be empty.");
    return;
}

var hash = hasher.HashPassword(new object(), password);

Console.WriteLine("\nGenerated Password Hash:");
Console.WriteLine(hash);