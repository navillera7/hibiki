using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
namespace MySurveyApp.Pages;
using MySurveyApp.Data;
using MySurveyApp.Services;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    // 화면에서 에러 메시지를 표시하기 위한 프로퍼티 (에러 해결 핵심!)
    public string? ErrorMessage { get; set; }

    public List<CampaignProgress> OngoingPolls { get; set; } = new();
    public async Task OnGetAsync()
    {
        // 1. 활성화된 캠페인 목록을 토큰 정보와 함께 가져옵니다.
        OngoingPolls = await _db.Campaigns
            .Where(c => c.IsActive)
            .Select(c => new CampaignProgress
            {
                Title = c.Title,
                // 투표율 계산: (사용된 토큰 수 / 전체 토큰 수) * 100
                Percentage = c.Tokens.Count > 0 
                    ? (int)((double)c.Tokens.Count(t => t.IsUsed) / c.Tokens.Count * 100) 
                    : 0
            })
            .ToListAsync();
    }
    public class CampaignProgress
    {
        public string Title { get; set; } = "";
        public int Percentage { get; set; }
    }

    public void OnGet()
    {
        // 처음 페이지 로드 시에는 에러 메시지 없음
    }

    public async Task<IActionResult> OnPostAsync(string voterToken)
    {
        if (string.IsNullOrEmpty(voterToken))
        {
            ErrorMessage = "코드를 입력해주세요.";
            return Page();
        }

        // DB에서 해당 코드가 유효한지(존재하며 사용 전인지) 확인
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

        // 코드가 유효하면 투표 페이지로 이동 (토큰 값을 들고 감)
        return RedirectToPage("Vote", new { token = voterToken });
    }
}