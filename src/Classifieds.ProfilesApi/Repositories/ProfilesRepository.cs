using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.Models;
using Shared.Enums;
using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Nest;

namespace ProfilesApi.Repositories
{

    public class ProfilesRepository
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IElasticClient _esClient;

        public ProfilesRepository(IAmazonS3 s3Client, IElasticClient esClient)
        {
            _s3Client = s3Client;
            _esClient = esClient;
        }

        public async Task<Shared.Models.Profile> Create(Shared.Models.Profile profile)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var keyName = $"profiles/{profile.Id}.json.gz";
            var bucketName = "classifieds-ui-dev";
            // var userId = await _authApi.GetUserId();
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    using var writer = new StreamWriter(zip);
                    writer.Write(JsonConvert.SerializeObject(profile, new JsonSerializerSettings
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

            await _esClient.IndexDocumentAsync(profile);

            return profile;
        }

        public async Task<IEnumerable<Shared.Models.Profile>> Get(string userId, string parentId)
        {
            var searchResponse = await _esClient.SearchAsync<Shared.Models.Profile>(s => s
                .Query(q =>
                    +q.Term(t => t.ParentId.Suffix("keyword"), parentId) &&
                    (
                        +q.Term(t => t.EntityPermissions.readUserIds.Suffix("keyword"), userId) ||
                        +q.Term(t => t.EntityPermissions.writeUserIds.Suffix("keyword"), userId) ||
                        +q.Term(t => t.EntityPermissions.deleteUserIds.Suffix("keyword"), userId)
                    )
                )
            );
            return searchResponse.Documents;
        }

        public async Task<IEnumerable<ProfileNavItem>> GetNavItems(string userId)
        {
            var searchResponse = await _esClient.SearchAsync<Shared.Models.Profile>(s => s
                .Query(q =>
                    (
                        +q.Term(t => t.EntityPermissions.readUserIds.Suffix("keyword"), userId) ||
                        +q.Term(t => t.EntityPermissions.writeUserIds.Suffix("keyword"), userId) ||
                        +q.Term(t => t.EntityPermissions.deleteUserIds.Suffix("keyword"), userId)
                    )
                )
            );
            var rootIds = new List<string>();
            foreach (var doc in searchResponse.Documents)
            {
                if (doc.ParentId == null && !rootIds.Contains(doc.Id))
                {
                    rootIds.Add(doc.Id);
                }
            }
            foreach (var doc in searchResponse.Documents)
            {
                if(doc.ParentId != null && !rootIds.Contains(doc.ParentId))
                {
                    rootIds.Add(doc.ParentId);
                }
            }
            var searchResponse2 = await _esClient.SearchAsync<Shared.Models.Profile>(s => s
                .Query(q =>
                    +q.Terms(t => t
                        .Field(f => f.Id.Suffix("keyword"))
                        .Terms(rootIds)
                    ) ||
                    +q.Terms(t => t
                        .Field(f => f.ParentId.Suffix("keyword"))
                        .Terms(rootIds)
                    )
                )
            );
            var items = new List<ProfileNavItem>();
            foreach(var doc in searchResponse2.Documents) {
                items.Add(new ProfileNavItem { Id = doc.Id, ParentId = doc.Id, Title = doc.Title });
            }
            return items;
        }

    }
}
