using System;
namespace log_analyzer.Models
{
	public class LogEntryModel
	{
		public DateTime? Timestamp { get; set; }
		public string Message { get; set; }
		public string Category { get; set; }
		public int Priority { get; set; }
		public string EventId { get; set; }
		public string Severity { get; set; }
		public string Title { get; set; }
	}
}
