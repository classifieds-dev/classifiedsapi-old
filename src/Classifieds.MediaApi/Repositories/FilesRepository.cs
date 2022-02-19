using System;
using System.IO;
using System.Threading.Tasks;
using MediaApi.InputModels;
using Shared.Models;
using Amazon.S3;
using Amazon.S3.Transfer;
using HeyRed.Mime;

namespace MediaApi.Repositories
{
    public class FilesRepository
    {
        private readonly IAmazonS3 _s3Client;

        public FilesRepository(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<MediaFile> Create(string userId, CreateFileInputModel file)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);
            var id = Guid.NewGuid();
            var keyName = "media/" + id + "." + MimeTypesMap.GetExtension(file.File.ContentType);
            var bucketName = "classifieds-ui-dev";
            using (var newMemoryStream = new MemoryStream())
            {
                file.File.CopyTo(newMemoryStream);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = keyName,
                    ContentType = file.File.ContentType,
                    BucketName = bucketName
                    //CannedACL = S3CannedACL.
                };
                uploadRequest.Metadata.Add("UserId", userId);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }
            var mediaFile = new MediaFile()
            {
                Id = id.ToString(),
                ContentType = file.File.ContentType,
                ContentDisposition = file.File.ContentDisposition,
                Length = file.File.Length,
                FileName = file.File.FileName,
                Path = $"{keyName}"
            };
            return mediaFile;
        }
    }
}
