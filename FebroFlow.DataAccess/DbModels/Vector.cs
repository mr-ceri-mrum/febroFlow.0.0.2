using System.ComponentModel.DataAnnotations;

namespace FebroFlow.DataAccess.DbModels;

public class Vector : BaseEntity
{
    public string IndexName { get; set; }
    public Guid CreatorId { get; set; }
    public Guid OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}