using System;

namespace SportsDatabaseSqlite.Tables
{
    public partial class PlayerInfo
    {
        public long ID { get; set; }

        public string Name { get; set; }

        public bool SelectionBias { get; set; }

        public byte[] Image { get; set; }
        public string ImagePath { get; set; }

        public float StrikingAccuracy { get; set; }
        public float GrapplingAccuracy { get; set; }
        public float SigStrikeDef { get; set; }
        public float SubmissionAvg { get; set; }
        public float TakedownAvg { get; set; }
        public float SigStrikesAbs { get; set; }
        public float SigStrikesLand { get; set; }
        public float Reach { get; set; }
        public float LegReach { get; set; }
        public float Weight { get; set; }
        public float Height { get; set; }
        public int Gender { get; set; }
    }
}