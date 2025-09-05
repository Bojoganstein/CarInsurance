namespace CarInsurance.Api.Models
{
    public class CarHistoryItem
    {
        public string Type { get; set; } = default!;   // Policy or Claim
        public DateOnly Date { get; set; }             // Key for sorting the history

        // Insurance Fields
        public long? PolicyId { get; set; }
        public string? Provider { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        // Claim Fields
        public long? ClaimId { get; set; }
        public DateOnly? ClaimDate { get; set; }
        public string? Description { get; set; }
        public decimal? Amount { get; set; }
    }
}
