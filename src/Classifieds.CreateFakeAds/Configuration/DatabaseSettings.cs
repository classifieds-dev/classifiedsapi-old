using System;
namespace CreateFakeAds.Configuration
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string AdsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string AdsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
