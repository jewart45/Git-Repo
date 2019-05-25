namespace MMADatabase
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Tables;
    using SQLite.CodeFirst;
    using System.Collections.Generic;

    public partial class MMADatabaseModel : DbContext
    {
        public MMADatabaseModel()
            : base("name=MMADatabaseModel1")
        {
        }

        public DbSet<OddsInfo> oddsInfo { get; set; }

        public DbSet<FighterInfo> fighterInfo {get; set;}

        public DbSet<User> users { get; set; }

        public DbSet<Settings> settings { get; set; }

        public bool AddOdds(string fighterName, long odds, long percent, DateTime date)
        {
            oddsInfo.Add(new OddsInfo() { DateTaken = DateTime.Now, Name = fighterName, OddsValue = odds, Percent = percent, FightDate = date });
            return SaveChanges() > 0; 
        }

        public bool AddMultipleOdds(List<OddsInfo> fighterList)
        {
            foreach(OddsInfo f in fighterList)
            {
                oddsInfo.Add(f);
            }
            
            return SaveChanges() > 0;
        }

        public bool ChangeRemember(User user, bool remember)
        {
            User dbUser = users.Where(x => x.Remember).FirstOrDefault();
            if (dbUser != null && !remember)
                dbUser.Remember = false;
            else
            {
                dbUser = users.Where(x => x.Username == user.Username).FirstOrDefault();
                if (dbUser != null)
                    dbUser.Remember = true;
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
            users.Remove(users.Where(x=> x.Username == username).First());
            return SaveChanges() > 0;

        }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //All tables listed here
            
            modelBuilder.Entity<OddsInfo>();
            modelBuilder.Entity<FighterInfo>();
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Settings>();

            var sqlitConnectionInitializer = new SqliteCreateDatabaseIfNotExists<MMADatabaseModel>(modelBuilder);
            Database.SetInitializer(sqlitConnectionInitializer);
        }
    }
}