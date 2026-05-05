Imports BCrypt.Net

Module GenerateHashes
    Sub Main()
        Dim passwords = {"Admin@1234", "Encoder@1234", "Viewer@1234"}
        For Each p In passwords
            Console.WriteLine($"{p} => {BCrypt.Net.BCrypt.HashPassword(p, 11)}")
        Next
    End Sub
End Module
