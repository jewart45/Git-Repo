namespace CommonClasses
{
    public class MarketplaceRunner
    {
        public string Name { get; set; }
        public string SelectionID { get; set; }
        public string Odds { get; set; }

        public MarketplaceRunner(string name, string selectionId = null, string odds = null)
        {
            Name = name;
            SelectionID = selectionId;
            Odds = odds;
        }
    }
}