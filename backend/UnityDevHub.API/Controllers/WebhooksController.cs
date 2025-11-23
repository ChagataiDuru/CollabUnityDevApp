using Microsoft.AspNetCore.Mvc;
using UnityDevHub.API.Services;

namespace UnityDevHub.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    /// <summary>
    /// Controller for handling external webhooks, such as GitHub events.
    /// </summary>
    public class WebhooksController : ControllerBase
    {
        private readonly IVcsService _vcsService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IVcsService vcsService, ILogger<WebhooksController> logger)
        {
            _vcsService = vcsService;
            _logger = logger;
        }

        /// <summary>
        /// Handles incoming GitHub webhooks.
        /// </summary>
        /// <returns>OK if processed successfully.</returns>
        [HttpPost("github")]
        public async Task<IActionResult> GitHubWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var signature = Request.Headers["X-Hub-Signature-256"].ToString();

            _logger.LogInformation("Received GitHub webhook");

            try
            {
                await _vcsService.ProcessGitHubWebhookAsync(payload, signature);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GitHub webhook");
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
