using Shared.Models;
using System.Collections.Generic;
using System.Linq;
using Nest;
using System.Threading.Tasks;
using Shared.Enums;
using AdsApi.Helpers;
using Amazon.S3;
using Amazon.S3.Transfer;
using Newtonsoft.Json;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json.Serialization;

namespace AdsApi.Repositories
{
    public class AdsRepository
    {
        private readonly IElasticClient _esClient;
        private readonly AdAttributesHelper _adAttributesHelper;
        private readonly IAmazonS3 _s3Client;

        public AdsRepository(IElasticClient esClient, AdAttributesHelper adAttributesHelper, IAmazonS3 s3Client)
        {
            _esClient = esClient;
            _adAttributesHelper = adAttributesHelper;
            _s3Client = s3Client;
        }

        public async Task<Ad> Create(Ad ad)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var keyName = $"ads/{ad.Id}.json.gz";
            var bucketName = "classifieds-ui-dev";
            // var userId = await _authApi.GetUserId();
            using (var ms = new MemoryStream()) {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    using var writer = new StreamWriter(zip);
                    writer.Write(JsonConvert.SerializeObject(ad, new JsonSerializerSettings {
                        ContractResolver = new DefaultContractResolver
                        {
                            NamingStrategy = new CamelCaseNamingStrategy()
                        }
                    }));
                    writer.Flush();
                    zip.Flush();
                }
                ms.Position = 0;
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = ms,
                    Key = keyName,
                    ContentType = "application/json",
                    BucketName = bucketName
                    //CannedACL = S3CannedACL.
                };
                uploadRequest.Headers.ContentEncoding = "gzip";
                // uploadRequest.Metadata.Add("UserId", userId);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
            return ad;
        }

        public /*async Task<*/Ad/*>*/ Update(Ad ad)
        {
            // @todo
            /*var filter = new BsonDocument("_id", ObjectId.Parse(ad.Id));
            await _ads.ReplaceOneAsync(filter, ad);*/
            return ad;
        }

        public List<Ad> Get(AdType adType, int page, string searchString, List<double> location, List<string> features, IDictionary<string, IDictionary<int, string>> attributes, int size = 25)
        {
            var from = (page - 1) * size;
            var searchResponse = _esClient.Search<AdIndex>(s => s
                .Query(q =>
                {
                    if (searchString != null || location != null || features != null)
                    {
                        q.Bool(b =>
                        {

                            if(location != null)
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
                            } else
                            {
                                b.Filter(f =>
                                {
                                    var andQuery = _adAttributesHelper.applyAttributes(adType.Attributes, f, attributes);
                                    andQuery &= f.Term(t => t
                                        .Field(f => f.AdType)
                                        .Value(adType.Id)
                                    );
                                    return andQuery;
                                });
                            }


                            if (searchString != null || features != null)
                            {
                                b.Must(mu =>
                                {
                                    QueryContainer andQuery = null;
                                    if (searchString != null)
                                    {
                                        andQuery &= mu.Match(m => m
                                            .Field(f => f.Title)
                                            .Query(searchString)
                                        );
                                    }
                                    if(features != null && features.Count > 0)
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
                    }
                    return q;
                })
                .From(from)
                .Size(size)
            );
            return LoadAds(searchResponse.Documents);
        }

        public List<Ad> LoadAds(IReadOnlyCollection<AdIndex> data)
        {
            var ads = new List<Ad>();
            foreach (var adIndex in data)
            {
                var ad = new Ad { Id = adIndex.Id, Title = adIndex.Title, Description = adIndex.Description, Images = new List<AdImage>(), Attributes = new List<AttributeValue>() };
                if(adIndex.Images != null)
                {
                    foreach (var image in adIndex.Images)
                    {
                        ad.Images.Add(new AdImage
                        {
                            Id = image.Id,
                            Path = image.Path,
                            Weight = image.Weight
                        });
                    }
                }
                if (adIndex.Attributes != null)
                {
                    foreach (var attr in adIndex.Attributes)
                    {
                        ad.Attributes.Add(new AttributeValue
                        {
                            Name = attr.Name,
                            DisplayName = attr.DisplayName,
                            Type = attr.Type,
                            Value = attr.Type == AttributeTypes.Number ? $"{attr.intValue}" : attr.stringValue, 
                            ComputedValue = attr.Type == AttributeTypes.Number ? $"{attr.intValue}" : attr.stringValue,
                            Test = 0,
                            Attributes = new List<AttributeValue>()
                        });
                    }
                }
                ads.Add(ad);
            }
            return ads;
        }
    }
}
