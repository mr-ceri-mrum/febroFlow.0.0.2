using System.ComponentModel.DataAnnotations.Schema;

namespace FebroFlow.DataAccess.DbModels;

public class Flow : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public Guid CreatorId { get; set; }
    public string SysteamPromt { get; set; }
    public Guid VectorId { get; set; }
    public string? Tags { get; set; }
}
