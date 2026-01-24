using Microsoft.AspNetCore.Mvc; // IActionResult 사용을 위해 필요합니다.
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;

namespace MySurveyApp.Pages.Admin;

public class AdminResultsModel : PageModel {
    private readonly ApplicationDbContext _db;
    public AdminResultsModel(ApplicationDbContext db) => _db = db;

    public string Title { get; set; } = "";
    public List<QResult> Results { get; set; } = new();

    // 1. 반환 타입을 Task에서 Task<IActionResult>로 변경합니다.
    public async Task<IActionResult> OnGetAsync(int id) {
        // 2. 관리자 권한 확인 로직을 맨 윗줄에 추가합니다.
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }

        var campaign = await _db.Campaigns
            .Include(c => c.Questions)
            .FirstOrDefaultAsync(c => c.Id == id);
            
        if (campaign == null) return RedirectToPage("Index"); // 캠페인이 없으면 목록으로 보냅니다.
        
        Title = campaign.Title;

        foreach (var q in campaign.Questions) {
            var votes = await _db.AnonymousResponses
                .Where(r => r.QuestionId == q.Id)
                .GroupBy(r => r.AnswerValue)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            Results.Add(new QResult { 
                QuestionText = q.Text, 
                Type = q.Type, 
                VoteCounts = votes, 
                TotalVotes = votes.Values.Sum() 
            });
        }

        return Page(); // 권한이 확인되면 정상적으로 페이지를 보여줍니다.
    }

    public class QResult {
        public string QuestionText { get; set; } = "";
        public string Type { get; set; } = "Survey";
        public Dictionary<string, int> VoteCounts { get; set; } = new();
        public int TotalVotes { get; set; }
    }
}