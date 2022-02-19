using MediaApi.InputModels;
using Shared.Models;
using MediaApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;

namespace MediaApi.Controllers
{
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FilesRepository _filesRepository;

        public FilesController(FilesRepository filesRepository)
        {
            _filesRepository = filesRepository;
        }

        /*[HttpGet("file/{id:required:guid}", Name = "GetFile")]
        public async Task<ActionResult<MediaFile>> Get(string id)
        {
            var mediaFile = await _filesRepository.Get(id);

            if (mediaFile == null)
            {
                return NotFound();
            }

            return mediaFile;
        }*/

        [HttpPost("file")]
        public async Task<ActionResult<MediaFile>> Create([FromForm]CreateFileInputModel file)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;
            var mediaFile = await _filesRepository.Create(token.Subject, file);
            return mediaFile;
            // return CreatedAtRoute("GetFile", new { id = mediaFile.Id.ToString() }, mediaFile);
        }

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            //_logger.LogDebug("Log this");
            return "This is a test";
        }
    }
}
