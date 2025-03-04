using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
namespace MeetingPlatform.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public string Title { get; set; }
        [AllowNull]
        public string? Description { get; set; }
        [AllowNull]
        public DateTime? StartTime { get; set; }
        [AllowNull]
        public DateTime? EndTime { get; set; }
        [AllowNull]
        public string? MeetingId { get; set; } // Unique ID for joining the meeting
        [AllowNull]
        public List<User>? Participants { get; set; } // List of users in the meeting
    }
}
