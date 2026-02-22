using System;
using AdRev.Domain.Models;

namespace AdRev.Core.Services
{
    public class AuditService
    {
        private readonly ResearcherProfileService _profileService;

        public AuditService(ResearcherProfileService profileService)
        {
            _profileService = profileService;
        }

        public void LogAction(ResearchProject project, string action, string entityType, string entityId, string details = "")
        {
            if (project == null) return;

            var profile = _profileService.GetProfile();
            var entry = new AuditLogEntry
            {
                UserName = profile.FullName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.Now
            };

            project.AuditLogs.Add(entry);
        }
    }
}
