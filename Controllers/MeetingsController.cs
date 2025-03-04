using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MeetingPlatform.Models;
using MeetingPlatform.Data;
using MeetingPlatform.DTO;

[Route("api/meetings")]
[ApiController]
public class MeetingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MeetingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1️⃣ Create a new meeting
    [HttpPost]
    public async Task<IActionResult> CreateMeeting([FromForm] MeetingDTO MeetingDTO)
    {
        Meeting meeting = new Meeting();
        meeting.Title = MeetingDTO.Title;
        

        meeting.MeetingId = Guid.NewGuid().ToString(); // Generate unique Meeting ID
        _context.Meetings.Add(meeting);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeetingById), new { id = meeting.Id }, meeting);
    }

    // 2️⃣ Get all meetings
    [HttpGet]
    public async Task<IActionResult> GetAllMeetings()
    {
        var meetings = await _context.Meetings.Include(c => c.Participants).ToListAsync();
        return Ok(meetings);
    }

    // 3️⃣ Get a specific meeting by ID
    [HttpGet("{meetingId}")]
    public async Task<IActionResult> GetMeetingById(string meetingId)
    {
        var meeting =  _context.Meetings.Where(c=>c.MeetingId == meetingId).Include(c => c.Participants).ToList();

        if (meeting == null)
        {
            return NotFound("Meeting not found.");
        }

        return Ok(meeting);
    }

    // 4️⃣ Join a meeting using a Meeting ID
    private string GetUserIdFromJwt()
    {
        var userId = User.Identities.Select(c => c.Claims).ToArray()[0].ToArray()[0].Value;
        return userId;
    }


    [HttpPost("join/{meetingId}")]
    public async Task<IActionResult> JoinMeeting(string meetingId)
    {
        var userId = GetUserIdFromJwt();
        var meeting = await _context.Meetings.Include(m => m.Participants)
                                             .FirstOrDefaultAsync(m => m.MeetingId == meetingId);

        if (meeting == null)
        {
            return NotFound("Meeting not found.");
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (!meeting.Participants.Contains(user))
        {
            meeting.Participants.Add(user); 
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = "Joined meeting successfully!", meeting });
    }
}
