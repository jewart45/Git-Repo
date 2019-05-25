﻿namespace SportsDatabase.Tables
{
    public class Settings
    {
        public long ID { get; set; }
        public int LoggingFrequency_s { get; set; }
        public int ShortLoggingFrequency_s { get; set; }
        public int AutoRefreshInterval_s { get; set; }
    }
}