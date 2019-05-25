namespace SportsDatabase.Tables
{
    public class User
    {
        public long ID { get; set; }
        public string Username { get; set; }

        public string Password { get; set; }

        public bool Remember { get; set; }

    }
}
