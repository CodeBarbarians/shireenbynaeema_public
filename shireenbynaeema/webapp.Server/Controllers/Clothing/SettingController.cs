namespace Server
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Application;

    using Domain;

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SettingController : ControllerBase
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        private readonly ISettingService settingService;

        public SettingController(ISettingService settingService) { this.settingService = settingService; }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var response = await settingService.GetAll();
            return Ok(response);
        }

        [HttpGet("{key}")]
        public async Task<ActionResult> GetByKey(string key)
        {
            var response = await settingService.GetByKey(key);
            return Ok(response);
        }

        [HttpPut]
        public async Task<ActionResult> Update(SettingDto request)
        {
            var response = await settingService.Update(request);
            return Ok(response);
        }

        [HttpPut("Bulk")]
        public async Task<ActionResult> BulkUpdate(List<SettingDto> request)
        {
            var response = await settingService.BulkUpdate(request);
            return Ok(response);
        }

        [HttpGet("Smtp")]
        public async Task<ActionResult> GetSmtpConfig()
        {
            var response = await settingService.GetByKey("SmtpConfig");
            return Ok(response);
        }

        [HttpPost("Smtp")]
        public async Task<ActionResult> SaveSmtpConfig([FromBody] SmtpConfig config)
        {
            var json = JsonSerializer.Serialize(config, JsonOptions);
            var response = await settingService.Update(new SettingDto { Key = "SmtpConfig", Value = json });
            return Ok(response);
        }

    [HttpGet("HeroSlides")]
    [AllowAnonymous]
    public async Task<ActionResult> GetHeroSlides()
    {
        var response = await settingService.GetByKey("HeroSlides");
        return Ok(response);
    }

        [HttpPost("HeroSlides")]
        public async Task<ActionResult> SaveHeroSlides([FromBody] List<HeroSlide> slides)
        {
            var json = JsonSerializer.Serialize(slides, JsonOptions);
            var response = await settingService.Update(new SettingDto { Key = "HeroSlides", Value = json });
            return Ok(response);
        }
    }
}
