using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventListenerAPI.Models
{
	[Table("api_event_log", Schema = "apilog")]
    public class EventLog
    {
		[Key]
		public string uuid { get; set; }
		public string event_ver { get; set; }
		public string event_type { get; set; }
		public string event_action { get; set; }
		public string etc_str1 { get; set; }
		public string etc_str2 { get; set; }
		public string etc_str3 { get; set; }
		public int etc_num1 { get; set; }
		public int etc_num2 { get; set; }
		public int etc_num3 { get; set; }
		public string user_ip { get; set; }
		public DateTime upd_dt { get; set; }
    }

	public static class ConvertUtil
	{
		public static string ToStringUtc(this DateTime time)
        {
            return $"DateTime({time.Ticks}, DateTimeKind.Utc)";
        }
	}

}
