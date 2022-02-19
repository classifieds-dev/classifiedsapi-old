using System;
using Nest;
using Shared.Models;

namespace IndexProfiles
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new ConnectionSettings(new Uri("https://i12sa6lx3y:v75zs8pgyd@classifieds-4537380016.us-east-1.bonsaisearch.net:443"))
            // var settings = new ConnectionSettings(new Uri("https://vpc-classifieds-dev-yd75o3lrbrn24sbfn4dal3a35m.us-east-1.es.amazonaws.com");
            //var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("classified_profiles");
            var client = new ElasticClient(settings);
            client.Indices.Delete("classified_profiles");
            client.Indices.Create("classified_profiles", c => c
                .Index<Shared.Models.Profile>()
                .Map<Shared.Models.Profile>(m => m
                    .AutoMap()
                    .Properties(p => p
                       .Nested<PhoneNumber>(n => n
                            .Name(nn => nn.PhoneNumbers)
                       )
                       .Nested<Location>(n => n
                            .Name(nn => nn.Locations)
                            .Properties(np => np
                                .Nested<PhoneNumber>(n2 => n2
                                    .Name(nn => nn.PhoneNumbers)
                                )
                            )
                       )
                    )
                )
            );
            Console.WriteLine("Complete");
        }
    }
}
