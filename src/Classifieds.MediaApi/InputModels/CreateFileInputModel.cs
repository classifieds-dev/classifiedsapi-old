using Microsoft.AspNetCore.Http;

namespace MediaApi.InputModels
{
    public class CreateFileInputModel
    {
        public IFormFile File { get; set; }
    }
}
