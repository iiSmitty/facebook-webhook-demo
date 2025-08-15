using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using FacebookWebhookDemo.Models;
using FacebookWebhookDemo.Services;

namespace FacebookWebhookDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacebookWebhookController : ControllerBase
    {
        private readonly ILogger<FacebookWebhookController> _logger;
        private const string VERIFY_TOKEN = "demo_verify_token_123";

        public FacebookWebhookController(ILogger<FacebookWebhookController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Facebook webhook verification endpoint (GET request)
        /// </summary>
        [HttpGet]
        public IActionResult VerifyWebhook()
        {
            var mode = Request.Query["hub.mode"];
            var token = Request.Query["hub.verify_token"];
            var challenge = Request.Query["hub.challenge"];

            _logger.LogInformation($"Webhook verification attempt - Mode: {mode}, Token: {token}");

            if (mode == "subscribe" && token == VERIFY_TOKEN)
            {
                _logger.LogInformation("✅ Webhook verified successfully!");
                return Ok(challenge.ToString());
            }

            _logger.LogWarning("❌ Webhook verification failed");
            return BadRequest("Verification failed");
        }

        /// <summary>
        /// Receive webhook data from Facebook (POST request)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                _logger.LogInformation($"📨 Webhook received: {body}");

                // Parse the incoming data (simplified for demo)
                var webhookData = JsonSerializer.Deserialize<JsonElement>(body);

                // Create a lead from the webhook data
                var lead = new Lead
                {
                    Name = GetValueFromJson(webhookData, "name", "Demo User"),
                    Email = GetValueFromJson(webhookData, "email", "demo@example.com"),
                    Phone = GetValueFromJson(webhookData, "phone", "+1-555-0123"),
                    Source = "Facebook Webhook"
                };

                LeadStorage.AddLead(lead);

                _logger.LogInformation($"🎯 Lead processed successfully: {lead.Id}");

                return Ok(new { status = "success", leadId = lead.Id, message = "Lead received and processed!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 Error processing webhook");
                return StatusCode(500, new { status = "error", message = ex.Message });
            }
        }

        /// <summary>
        /// Get all leads for dashboard display
        /// </summary>
        [HttpGet("leads")]
        public IActionResult GetLeads()
        {
            var leads = LeadStorage.GetAllLeads();
            return Ok(new
            {
                totalCount = LeadStorage.GetLeadCount(),
                leads = leads,
                lastUpdated = DateTime.Now
            });
        }

        /// <summary>
        /// Manual trigger for demo lead generation
        /// </summary>
        [HttpPost("trigger-demo")]
        public IActionResult TriggerDemo([FromBody] DemoTriggerRequest? request = null)
        {
            var lead = new Lead
            {
                Name = request?.Name ?? $"Demo Lead {Random.Shared.Next(100, 999)}",
                Email = request?.Email ?? $"demo{Random.Shared.Next(100, 999)}@example.com",
                Phone = request?.Phone ?? $"+1-555-{Random.Shared.Next(1000, 9999)}",
                Source = "Manual Demo Trigger"
            };

            LeadStorage.AddLead(lead);

            _logger.LogInformation($"🚀 Demo lead triggered: {lead.Name}");

            return Ok(new
            {
                status = "success",
                message = "Demo lead created successfully!",
                lead = lead
            });
        }

        /// <summary>
        /// Simulate a Facebook webhook payload
        /// </summary>
        [HttpPost("simulate-facebook")]
        public IActionResult SimulateFacebook()
        {
            var facebookPayload = new
            {
                @object = "page",
                entry = new[]
                {
                    new
                    {
                        id = "page_123",
                        time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        changes = new[]
                        {
                            new
                            {
                                field = "leadgen",
                                value = new
                                {
                                    ad_id = "ad_456",
                                    form_id = "form_789",
                                    leadgen_id = $"lead_{Guid.NewGuid()}",
                                    created_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                                    page_id = "page_123"
                                }
                            }
                        }
                    }
                }
            };

            // Create a lead from the simulated Facebook data
            var lead = new Lead
            {
                Name = $"Facebook Lead {Random.Shared.Next(100, 999)}",
                Email = $"fb-lead{Random.Shared.Next(100, 999)}@example.com",
                Phone = $"+1-555-{Random.Shared.Next(1000, 9999)}",
                Source = "Facebook Simulation"
            };

            LeadStorage.AddLead(lead);

            _logger.LogInformation($"📘 Facebook webhook simulated");

            return Ok(new
            {
                status = "success",
                message = "Facebook webhook simulated!",
                payload = facebookPayload,
                lead = lead
            });
        }

        /// <summary>
        /// Clear all leads (for demo reset)
        /// </summary>
        [HttpDelete("clear")]
        public IActionResult ClearLeads()
        {
            var currentCount = LeadStorage.GetLeadCount();
            LeadStorage.ClearAllLeads();

            _logger.LogInformation($"🗑️ Cleared {currentCount} leads for demo reset");

            return Ok(new { status = "success", message = $"Cleared {currentCount} leads" });
        }

        /// <summary>
        /// Helper method to extract values from JSON with fallback
        /// </summary>
        private string GetValueFromJson(JsonElement json, string property, string defaultValue)
        {
            try
            {
                if (json.TryGetProperty(property, out var element))
                {
                    return element.GetString() ?? defaultValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error extracting {property} from JSON: {ex.Message}");
            }
            return defaultValue;
        }
    }

    /// <summary>
    /// Dashboard Controller for serving the main UI
    /// </summary>
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    /// <summary>
    /// Home Controller for redirecting to dashboard
    /// </summary>
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}