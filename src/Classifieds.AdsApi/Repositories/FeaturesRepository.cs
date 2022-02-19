using System.Collections.Generic;
using System.Linq;
using Nest;
using Shared.Models;
using AdsApi.Helpers;

namespace AdsApi.Repositories
{
    public class FeaturesRepository
    {

        private readonly IElasticClient _esClient;
        private readonly AdAttributesHelper _adAttributesHelper;

        public FeaturesRepository(IElasticClient esClient, AdAttributesHelper adAttributesHelper)
        {
            _esClient = esClient;
            _adAttributesHelper = adAttributesHelper;
        }

        public List<AdFeature> Get(AdType adType, string searchString, string adSearchString, List<double> location, List<string> features, IDictionary<string, IDictionary<int, string>> attributes)
        {
            var searchResponse = _esClient.Search<AdIndex>(s => s
                .Size(0)
               .Query(q =>
               {
                   if (searchString != null || location != null || features != null)
                   {
                       q.Bool(b =>
                       {
                           if (location != null)
                           {
                               b.Filter(f =>
                               {
                                   var andQuery = _adAttributesHelper.applyAttributes(adType.Attributes, f, attributes);
                                   andQuery &= f.Term(t => t
                                       .Field(f => f.AdType)
                                       .Value(adType.Id)
                                   );
                                   andQuery &= f.GeoDistance(g => g
                                       .Field(a => a.Location)
                                       .DistanceType(GeoDistanceType.Arc)
                                       .Location(location[1], location[0])
                                       .Distance("10m")
                                       .ValidationMethod(GeoValidationMethod.IgnoreMalformed)
                                   );
                                   return andQuery;
                               });
                           }
                           else
                           {
                               b.Filter(f =>
                               {
                                   QueryContainer andQuery = _adAttributesHelper.applyAttributes(adType.Attributes, f, attributes);
                                   andQuery &= f.Term(t => t
                                       .Field(f => f.AdType)
                                       .Value(adType.Id)
                                   );
                                   return andQuery;
                               });
                           }

                           if (adSearchString != null || features != null)
                           {
                               b.Must(mu =>
                               {
                                   QueryContainer andQuery = null;
                                   if (adSearchString != null)
                                   {
                                       andQuery &= mu.Match(m => m
                                           .Field(f => f.Title)
                                           .Query(adSearchString)
                                       );
                                   }
                                   if (features != null && features.Count > 0)
                                   {
                                       foreach (var feature in features)
                                       {
                                           andQuery &= mu.Nested(n => n
                                               .Path(p => p.Features)
                                               .Query(nq => nq
                                                   .Bool(b => b
                                                       .Must(m => m
                                                           .Match(mn => mn
                                                               .Field(f => f.Features.First().HumanName)
                                                               .Query(feature)
                                                           )
                                                       )
                                                   )
                                               )
                                           );
                                       }
                                   }
                                   return andQuery;
                               });
                           }
                           return b;
                       });
                   } else
                   {
                       q.Bool(b => b
                            .Filter(f => f
                                .Term(t => t
                                    .Field(f => f.AdType)
                                    .Value(adType)
                                )
                            )
                       );
                   }
                   return q;
               })
                .Aggregations(a => a
                    .Nested("features", n => n
                        .Path(p => p.Features)
                        .Aggregations(a =>
                        {
                            if(searchString != null)
                            {
                                a.Filter("features_filtered", f => f
                                    .Filter(f => f
                                        .Bool(b => b
                                            .Should(s => {
                                                QueryContainer query = null;
                                                if (features != null && features.Count > 0)
                                                {
                                                    foreach (var feature in features)
                                                    {
                                                        query |= s.Term(t => t
                                                            .Field(f => f.Features.First().HumanName.Suffix("keyword"))
                                                            .Value(feature)
                                                        );
                                                    }
                                                }
                                                query |= s.Match(m => m
                                                  .Field(f => f.Features.First().HumanName)
                                                  .Query(searchString)
                                                );
                                                return query;
                                            })
                                        )
                                    )
                                    .Aggregations(a => a
                                        .Terms("feature_names", t => t
                                            .Field(f => f.Features.First().HumanName.Suffix("keyword"))
                                        )
                                    )
                                );
                            } else
                            {
                                a.Terms("feature_names", t => t
                                    .Field(f => f.Features.First().HumanName.Suffix("keyword"))
                                );
                            }
                            return a;
                        })
                    )
                )
            );
            IReadOnlyCollection<KeyedBucket<string>> buckets;
            if(searchString != null)
            {
                buckets = searchResponse.Aggregations.Nested("features").Filter("features_filtered").Terms("feature_names").Buckets;
            } else
            {
                buckets = searchResponse.Aggregations.Nested("features").Terms("feature_names").Buckets;
            }
            var adFeatures = new List<AdFeature>();
            foreach(var bucket in buckets)
            {
                var feature = new AdFeature() { Id = bucket.Key, HumanName = bucket.Key };
                adFeatures.Add(feature);
            }
            return adFeatures;
        }

    }
}
