
namespace Shared.Models
{
    public class MediaFile
    {
        public string Id { get; set; }
        public string ContentDisposition { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
        public long Length { get; set; }
        public string Path { get; set; }
    }
}