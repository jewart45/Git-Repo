using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class PlayerInfo
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public bool SelectionBias { get; set; }

        public byte[] Image { get; set; }

    }
}