using FacebookWebhookDemo.Models;

namespace FacebookWebhookDemo.Services
{
    /// <summary>
    /// In-memory storage service for leads (demo purposes only)
    /// In production, this would be replaced with a proper database service
    /// </summary>
    public static class LeadStorage
    {
        private static readonly List<Lead> _leads = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Add a new lead to storage
        /// </summary>
        /// <param name="lead">The lead to add</param>
        public static void AddLead(Lead lead)
        {
            ArgumentNullException.ThrowIfNull(lead);

            lock (_lock)
            {
                _leads.Add(lead);
            }

            Console.WriteLine($"💡 New Lead Added: {lead.Name} ({lead.Email})");
        }

        /// <summary>
        /// Get all leads ordered by timestamp (newest first)
        /// </summary>
        /// <returns>List of all leads</returns>
        public static List<Lead> GetAllLeads()
        {
            lock (_lock)
            {
                return _leads.OrderByDescending(l => l.Timestamp).ToList();
            }
        }

        /// <summary>
        /// Get the total count of leads
        /// </summary>
        /// <returns>Total number of leads</returns>
        public static int GetLeadCount()
        {
            lock (_lock)
            {
                return _leads.Count;
            }
        }

        /// <summary>
        /// Clear all leads from storage
        /// </summary>
        public static void ClearAllLeads()
        {
            lock (_lock)
            {
                _leads.Clear();
            }
            Console.WriteLine("🗑️ All leads cleared!");
        }

        /// <summary>
        /// Get leads by source
        /// </summary>
        /// <param name="source">The source to filter by</param>
        /// <returns>List of leads from the specified source</returns>
        public static List<Lead> GetLeadsBySource(string source)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(source);

            lock (_lock)
            {
                return _leads
                    .Where(l => l.Source.Equals(source, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// Get leads by status
        /// </summary>
        /// <param name="status">The status to filter by</param>
        /// <returns>List of leads with the specified status</returns>
        public static List<Lead> GetLeadsByStatus(string status)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(status);

            lock (_lock)
            {
                return _leads
                    .Where(l => l.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// Get a specific lead by ID
        /// </summary>
        /// <param name="id">The lead ID</param>
        /// <returns>The lead if found, null otherwise</returns>
        public static Lead? GetLeadById(string id)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);

            lock (_lock)
            {
                return _leads.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            }
        }

        /// <summary>
        /// Update lead status
        /// </summary>
        /// <param name="id">Lead ID</param>
        /// <param name="newStatus">New status</param>
        /// <returns>True if updated successfully</returns>
        public static bool UpdateLeadStatus(string id, string newStatus)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(id);
            ArgumentException.ThrowIfNullOrWhiteSpace(newStatus);

            lock (_lock)
            {
                var lead = _leads.FirstOrDefault(l => l.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                if (lead != null)
                {
                    lead.Status = newStatus;
                    Console.WriteLine($"📝 Lead {lead.Name} status updated to: {newStatus}");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get leads created within a specific time range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <returns>List of leads within the date range</returns>
        public static List<Lead> GetLeadsByDateRange(DateTime fromDate, DateTime toDate)
        {
            lock (_lock)
            {
                return _leads
                    .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                    .OrderByDescending(l => l.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// Get statistics about the leads
        /// </summary>
        /// <returns>Dictionary with various statistics</returns>
        public static Dictionary<string, object> GetStatistics()
        {
            lock (_lock)
            {
                var stats = new Dictionary<string, object>
                {
                    ["TotalLeads"] = _leads.Count,
                    ["LeadsBySource"] = _leads.GroupBy(l => l.Source).ToDictionary(g => g.Key, g => g.Count()),
                    ["LeadsByStatus"] = _leads.GroupBy(l => l.Status).ToDictionary(g => g.Key, g => g.Count()),
                    ["TodaysLeads"] = _leads.Count(l => l.Timestamp.Date == DateTime.Today),
                    ["LastLeadTime"] = _leads.OrderByDescending(l => l.Timestamp).FirstOrDefault()?.Timestamp
                };

                return stats;
            }
        }
    }
}