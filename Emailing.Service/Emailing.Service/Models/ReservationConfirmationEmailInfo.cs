namespace Emailing.Service.Models
{
	public class ReservationConfirmationEmailInfo
	{
		public Guid UserId { get; set; }
		public string Saloon { get; set; }
		public string Service { get; set; }
		public string Worker { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}
