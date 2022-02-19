using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using TaxonomyApi.Repositories;
using System;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;

namespace TaxonomyApi.Controllers
{
    [ApiController]
    public class VocabulariesController : ControllerBase
    {
        private readonly VocabulariesRepository _vocabulariesRepository;
        private readonly ILogger<VocabulariesController> _logger;

        public VocabulariesController(VocabulariesRepository vocabulariesRepository, ILogger<VocabulariesController> logger)
        {
            _vocabulariesRepository = vocabulariesRepository;
            _logger = logger;
        }

        /*[HttpGet("vocabularylistitems")]
        public async Task<List<Vocabulary>> Get()
        {
            var userId = await _authApi.GetUserId();
            _logger.LogDebug("User Id: {userId}", userId);
            var vocabs = await _vocabulariesRepository.Get(true, userId);
            return vocabs;
        }*/

        /*[HttpGet("vocabulary/{id:guid:required}", Name = "GetVocabulary")]
        public async Task<ActionResult<Vocabulary>> Get(string id)
        {
            var vocab = await _vocabulariesRepository.Get(id);

            if (vocab == null)
            {
                return NotFound();
            }

            return vocab;
        }*/

        [HttpPost("vocabulary")]
        public async Task<Vocabulary> Create(Vocabulary vocabulary)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadToken(HttpContext.Request.Headers["Authorization"].ToString().Substring(7)) as JwtSecurityToken;

            vocabulary.Id = Guid.NewGuid().ToString();
            vocabulary.UserId = token.Subject;
            return await _vocabulariesRepository.Create(vocabulary);
        }

        /*[HttpPut("vocabulary/{id:guid:required}")]
        public async Task<ActionResult<Vocabulary>> Update(Vocabulary vocabulary)
        {
            var currentVocab = await _vocabulariesRepository.Get(vocabulary.Id);
            if(currentVocab == null)
            {
                return NotFound();
            } else if(currentVocab.UserId != vocabulary.UserId) {
                return Unauthorized();
            }
            return await _vocabulariesRepository.Update(vocabulary);
        }*/

        [HttpGet("test")]
        public ActionResult<string> Test()
        {
            //_logger.LogDebug("Log this");
            return "This is a test";
        }

    }
}
