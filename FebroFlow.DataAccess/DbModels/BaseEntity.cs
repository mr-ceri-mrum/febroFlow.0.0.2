using System.ComponentModel.DataAnnotations;

namespace FebroFlow.DataAccess.DbModels;

public class BaseEntity : IEntity<Guid>
{
    [Key]
    public Guid Id { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime DataCreate { get; set; } = DateTime.Now;
}
