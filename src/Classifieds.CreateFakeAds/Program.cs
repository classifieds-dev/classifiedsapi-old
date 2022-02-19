using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using CreateFakeAds.Configuration;
using Shared.Models;
using System.IO;
using MongoDB.Driver;
using Bogus;

namespace CreateFakeAds
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Creating ads started");
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var settings = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            var ads = database.GetCollection<Ad>(settings.AdsCollectionName);
            var testAds = new Faker<Ad>()
                .RuleFor(a => a.Title, f => f.Commerce.ProductName())
                .RuleFor(a => a.Description, f => f.Commerce.ProductMaterial());
            for (var i = 0; i < 100; i++)
            {
                var ad = testAds.Generate();
                ads.InsertOne(ad);
            }
            Console.WriteLine("Creating ads complete,");
        }
    }
}
