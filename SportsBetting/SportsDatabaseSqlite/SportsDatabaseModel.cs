namespace SportsDatabaseSqlite
{
    using SQLite.CodeFirst;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Tables;

    public partial class SportsDatabaseModel : DbContext
    {
        public SportsDatabaseModel()
            : base("name=SportsDatabaseModel")
        {
        }

        public DbSet<OddsInfo> oddsInfo { get; set; }

        public DbSet<FighterInfo> playerInfo { get; set; }

        public DbSet<ResultLog> results { get; set; }

        public DbSet<User> users { get; set; }

        public DbSet<Settings> settings { get; set; }

        public bool AddOdds(string fighterName, long odds, long percent, DateTime date)
        {
            oddsInfo.Add(new OddsInfo() { DateTaken = DateTime.Now, SelectionName = fighterName, OddsValue = odds, Percent = percent, EventDate = date });
            return SaveChanges() > 0;
        }

        public bool AddOdds(string fighterName, long odds, long percent, DateTime date, bool winner)
        {
            oddsInfo.Add(new OddsInfo() { DateTaken = DateTime.Now, SelectionName = fighterName, OddsValue = odds, Percent = percent, EventDate = date, Winner = winner });
            return SaveChanges() > 0;
        }

        public bool AddMultipleOdds(List<OddsInfo> fighterList)
        {
            foreach (OddsInfo f in fighterList)
            {
                oddsInfo.Add(f);
            }

            return SaveChanges() > 0;
        }

        public bool ChangeRemember(User user, bool remember)
        {
            User dbUser = users.Where(x => x.Remember).FirstOrDefault();
            if (dbUser != null && !remember)
            {
                dbUser.Remember = false;
            }
            else
            {
                dbUser = users.Where(x => x.Username == user.Username).FirstOrDefault();
                if (dbUser != null)
                {
                    dbUser.Remember = true;
                }
                else
                {
                    user.Remember = true;
                    users.Add(user);
                }
            }
            return SaveChanges() > 0;
        }

        public bool AddUser(string username, string password, bool remember)
        {
            users.Add(new User() { Username = username, Password = password, Remember = remember });
            return SaveChanges() > 0;
        }

        public bool DeleteUser(string username)
        {
            users.Remove(users.Where(x => x.Username == username).First());
            return SaveChanges() > 0;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //All tables listed here

            modelBuilder.Entity<OddsInfo>();
            modelBuilder.Entity<ResultLog>();
            modelBuilder.Entity<FighterInfo>();
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Settings>();
            

            SqliteCreateDatabaseIfNotExists<SportsDatabaseModel> sqlitConnectionInitializer = new SqliteCreateDatabaseIfNotExists<SportsDatabaseModel>(modelBuilder);
            Database.SetInitializer(sqlitConnectionInitializer);
        }
    }
}