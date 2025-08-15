using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FacebookWebhookDemo.Controllers
{
    // Simple Lead model
    public class Lead
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Source { get; set; } = "Facebook";
        public string Status { get; set; } = "New";
    }

    // In-memory storage for demo
    public static class LeadStorage
    {
        private static readonly List<Lead> _leads = new();
        private static readonly object _lock = new();

        public static void AddLead(Lead lead)
        {
            lock (_lock)
            {
                _leads.Add(lead);
            }
            Console.WriteLine($"💡 New Lead Added: {lead.Name} ({lead.Email})");
        }

        public static List<Lead> GetAllLeads()
        {
            lock (_lock)
            {
                return _leads.OrderByDescending(l => l.Timestamp).ToList();
            }
        }

        public static int GetLeadCount()
        {
            lock (_lock)
            {
                return _leads.Count;
            }
        }

        public static void ClearAllLeads()
        {
            lock (_lock)
            {
                _leads.Clear();
            }
            Console.WriteLine("🗑️ All leads cleared!");
        }
    }

    // Simple request model for demo triggers
    public class DemoTriggerRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

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

        // Facebook webhook verification (GET request)
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

        // Receive webhook data (POST request)
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

        // Helper method to extract values from JSON
        private string GetValueFromJson(JsonElement json, string property, string defaultValue)
        {
            try
            {
                if (json.TryGetProperty(property, out var element))
                {
                    return element.GetString() ?? defaultValue;
                }
            }
            catch { }
            return defaultValue;
        }

        // Get all leads (for dashboard)
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

        // Manual trigger for demo (this is the cool part!)
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

        // Simulate Facebook webhook data
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

            var json = JsonSerializer.Serialize(facebookPayload);

            // Simulate the webhook call to ourselves
            var lead = new Lead
            {
                Name = $"Facebook Lead {Random.Shared.Next(100, 999)}",
                Email = $"fb-lead{Random.Shared.Next(100, 999)}@example.com",
                Phone = $"+1-555-{Random.Shared.Next(1000, 9999)}",
                Source = "Facebook Simulation"
            };

            LeadStorage.AddLead(lead);

            _logger.LogInformation($"📘 Facebook webhook simulated: {json}");

            return Ok(new
            {
                status = "success",
                message = "Facebook webhook simulated!",
                payload = facebookPayload,
                lead = lead
            });
        }

        // Clear all leads (for demo reset) - FIXED VERSION
        [HttpDelete("clear")]
        public IActionResult ClearLeads()
        {
            var currentCount = LeadStorage.GetLeadCount();
            LeadStorage.ClearAllLeads();

            _logger.LogInformation($"🗑️ Cleared {currentCount} leads for demo reset");

            return Ok(new { status = "success", message = $"Cleared {currentCount} leads" });
        }
    }

    // Dashboard Controller
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

    // Home Controller
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Dashboard");
        }
    }
}