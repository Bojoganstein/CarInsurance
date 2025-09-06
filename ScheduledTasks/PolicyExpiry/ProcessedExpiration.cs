namespace CarInsurance.Api.ScheduledTasks.PolicyExpiry
{
    public class ProcessedExpiration
    {
        public long PolicyId { get; set; }
        public DateTime LoggedAtUtc { get; set; }
    }
}
