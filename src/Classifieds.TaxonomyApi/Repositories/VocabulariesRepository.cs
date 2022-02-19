using Shared.Models;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TaxonomyApi.Repositories
{
    public class VocabulariesRepository
    {
        private readonly IAmazonS3 _s3Client;

        public VocabulariesRepository(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        /*public async Task<List<Vocabulary>> Get(bool globals, string userId)
        {
            var conditions = new List<ScanCondition> {
                new ScanCondition("UserId", ScanOperator.Equal, userId)
            };
            var vocabularies = await _dynamoContext.ScanAsync<Vocabulary>(conditions).GetRemainingAsync();
            return vocabularies;
        }*/

        /*public async Task<Vocabulary> Get(string id)
        {
            var vocabulary = await _dynamoContext.LoadAsync<Vocabulary>(id);
            return vocabulary;
        }*/

        public async Task<Vocabulary> Create(Vocabulary vocabulary)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var keyName = $"vocabularies/{vocabulary.Id}.json.gz";
            var bucketName = "classifieds-ui-dev";
            // var userId = await _authApi.GetUserId();
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    using var writer = new StreamWriter(zip);
                    writer.Write(JsonConvert.SerializeObject(vocabulary, new JsonSerializerSettings
                    {
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

            // await _esClient.IndexDocumentAsync(profile);

            return vocabulary;
        }

        /*public async Task<Vocabulary> Update(Vocabulary vocabulary)
        {
            await _dynamoContext.SaveAsync<Vocabulary>(vocabulary);
            return vocabulary;
        }*/
    }
}
