using BCrypt.Net;
Console.WriteLine("Admin@1234   => " + BCrypt.Net.BCrypt.HashPassword("Admin@1234",   11));
Console.WriteLine("Encoder@1234 => " + BCrypt.Net.BCrypt.HashPassword("Encoder@1234", 11));
Console.WriteLine("Viewer@1234  => " + BCrypt.Net.BCrypt.HashPassword("Viewer@1234",  11));
