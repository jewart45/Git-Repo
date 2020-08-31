using System;

namespace BetHistoryImport.Classes
{
    public struct FighterFinalOddsExport
    {
        public DateTime EventDate;

        public string EventName;

        public string Name;

        public double Odds;

        public string Winner;

        public int Gender;
        public float Height;
        public float Weight;
        public float GrapplingAccuracy;
        public float StrikingAccuracy;
        public float Reach;
        public float LegReach;
        public float SubAvg;
        public float TakedownAvg;
        public float SigStrikesAbsorb;
        public float SigStrikesLand;
        public float SigStrikesDefend;


        public bool SelectionBias;
    }
}