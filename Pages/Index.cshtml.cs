using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;
using MySurveyApp.Services;

namespace MySurveyApp.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string? ErrorMessage { get; set; }
    public List<CampaignProgress> OngoingPolls { get; set; } = new();

    // ✅ 해결: OnGet()을 삭제하고 OnGetAsync 하나만 유지합니다.
    public async Task OnGetAsync()
    {
        OngoingPolls = await _db.Campaigns
            .Where(c => c.IsActive)
            .Select(c => new CampaignProgress
            {
                Title = c.Title,
                // 분모가 0이 되는 것을 방지하여 런타임 에러를 차단합니다.
                Percentage = c.Tokens.Any() 
                    ? (int)((double)c.Tokens.Count(t => t.IsUsed) / c.Tokens.Count * 100) 
                    : 0
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(string voterToken)
    {
        if (string.IsNullOrEmpty(voterToken))
        {
            ErrorMessage = "코드를 입력해주세요.";
            return Page();
        }

        var token = await _db.VoterTokens
            .FirstOrDefaultAsync(t => t.Code == voterToken);

        if (token == null)
        {
            ErrorMessage = "존재하지 않는 코드입니다.";
            return Page();
        }

        if (token.IsUsed)
        {
            ErrorMessage = "이미 투표에 사용된 코드입니다.";
            return Page();
        }

        return RedirectToPage("Vote", new { token = voterToken });
    }

    public class CampaignProgress
    {
        public string Title { get; set; } = "";
        public int Percentage { get; set; }
    }
}