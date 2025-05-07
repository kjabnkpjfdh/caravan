namespace projectcaravan
{
    public class inlogdata
    {
     
            public int Id { get; set; }
            public string Username { get; set; }
            public string PasswordHash { get; set; } // Gebruik hashing!
            public string Role { get; set; } // "Admin", "User", ...
        

    }
}
