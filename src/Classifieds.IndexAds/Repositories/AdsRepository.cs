using System;
using IndexAds.Configuration;
using Shared.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;

namespace IndexAds.Repositories
{
    public class AdsRepository
    {
        // private readonly IMongoCollection<Ad> _ads;
        private readonly DynamoDBContext _dynamo;

        public AdsRepository(IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            // _ads = database.GetCollection<Ad>(settings.AdsCollectionName);

            var client2 = new AmazonDynamoDBClient();
            _dynamo = new DynamoDBContext(client2);
        }

        public async Task<List<Ad>> Get()
        {
            var conditions = new List<ScanCondition>();
            var ads = await _dynamo.ScanAsync<Ad>(conditions).GetRemainingAsync();
            return ads;
        }
    }
}
