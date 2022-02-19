using System;
using System.Collections.Generic;
using System.IO;
using IndexAds.Configuration;
using IndexAds.Repositories;
using Microsoft.Extensions.Configuration;
using Nest;
using Shared.Models;
using Shared.Enums;
using System.Threading.Tasks;

namespace IndexAds
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Indexing Started");
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var databaseSettings = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>();
            var settings = new ConnectionSettings(new Uri("https://i12sa6lx3y:v75zs8pgyd@classifieds-4537380016.us-east-1.bonsaisearch.net:443"))
            // var settings = new ConnectionSettings(new Uri("https://vpc-classifieds-dev-yd75o3lrbrn24sbfn4dal3a35m.us-east-1.es.amazonaws.com");
            //var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("classified_ads");
            var client = new ElasticClient(settings);
            client.Indices.Delete("classified_ads");
            client.Indices.Create("classified_ads", c => c
                .Index<AdIndex>()
                .Map<AdIndex>(m => m
                    .AutoMap()
                    .Properties(p => p
                       .GeoPoint(geo => geo
                            .Name(a => a.Location)
                       )
                       .Nested<AdFeatureIndex>(n => n
                            .Name(nn => nn.Features)
                       )
                       .Nested<AttributeIndex>(n => n
                            .Name(nn => nn.Attributes)
                       )
                       .Nested<AdImageIndex>(n => n
                            .Name(nn => nn.Images)
                       )
                    )
                )
            );
            var adsRepository = new AdsRepository(databaseSettings);
            foreach (var ad in await adsRepository.Get())
            { 

                var indexAd = new AdIndex { Id = ad.Id.ToString(), AdType = ad.AdType, Status = ad.Status, Title = ad.Title, Description = ad.Description, Location = ad.Location, Images = new List<AdImageIndex>(), Attributes = new List<AttributeIndex>(), Features = new List<AdFeatureIndex>() };
                Console.WriteLine(String.Format("Indexing {0}", ad.Id));

                if (ad.Attributes != null)
                {
                    foreach (var attribute in ad.Attributes)
                    {
                        indexAd.Attributes.AddRange(getAttributes(attribute));
                    }
                }
                if (ad.FeatureSets != null)
                {
                    foreach (var vocab in ad.FeatureSets)
                    {
                        foreach (var term in vocab.Terms)
                        {
                            indexAd.Features.AddRange(getFeatures(term));
                        }
                    }
                }
                if (ad.Images != null)
                {
                    foreach(var image in ad.Images)
                    {
                        indexAd.Images.Add(new AdImageIndex {
                            Id = image.Id,
                            Path = image.Path,
                            Weight = image.Weight
                        });
                    }
                }

                var res = client.IndexDocument(indexAd);
                Console.WriteLine(String.Format("Indexed {0}", res.Id));

            }
            Console.WriteLine("Indexing Competed");
        }

        static public List<AdFeatureIndex> getFeatures(Term term)
        {
            List<AdFeatureIndex> leafNodes = new List<AdFeatureIndex>();
            if (term.Children.Count == 0)
            {
                if(term.Selected)
                {
                    leafNodes.Add(new AdFeatureIndex { Id = term.Id.ToString(), HumanName = term.HumanName });
                }
            }
            else
            {
                foreach(var t in term.Children)
                {
                    leafNodes.AddRange(getFeatures(t));
                }
            }
            return leafNodes;
        }

        static public List<AttributeIndex> getAttributes(AttributeValue attributeValue)
        {
            List<AttributeIndex> leafNodes = new List<AttributeIndex>();
            if (attributeValue.Attributes == null || attributeValue.Attributes.Count == 0)
            {
                if (attributeValue.Type == AttributeTypes.Number)
                {
                    var computedValue = Int32.Parse(attributeValue.ComputedValue);
                    leafNodes.Add(new AttributeIndex { Name = attributeValue.Name, DisplayName = attributeValue.DisplayName, Type = attributeValue.Type, intValue = computedValue });
                }
                else
                {
                    leafNodes.Add(new AttributeIndex { Name = attributeValue.Name, DisplayName = attributeValue.DisplayName, Type = attributeValue.Type, stringValue = attributeValue.ComputedValue });
                }
            }
            else
            {
                foreach (var a in attributeValue.Attributes)
                {
                    leafNodes.AddRange(getAttributes(a));
                }
            }
            return leafNodes;
        }

    }
}
