namespace FebroFlow.Data.Dtos.Flow;


    public class CreateFlowRequest
    {
        public Guid VectorId { get; set; }
        public string SysteamPromt { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Guid CreatorId { get; set; }
        public string? Tags { get; set; }
    }
