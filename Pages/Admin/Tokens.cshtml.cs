using Microsoft.AspNetCore.Mvc; // IActionResult 사용을 위해 추가
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;

namespace MySurveyApp.Pages.Admin;

public class AdminTokensModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public AdminTokensModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public string CampaignTitle { get; set; } = "";
    public List<VoterToken> Tokens { get; set; } = new();

    // 1. 반환 타입을 Task에서 Task<IActionResult>로 변경합니다. 
    public async Task<IActionResult> OnGetAsync(int id)
    {
        // 2. 관리자 권한 확인 로직을 최상단에 추가합니다.
        if (HttpContext.Session.GetString("IsAdmin") != "true")
        {
            return RedirectToPage("Login");
        }
        
        var campaign = await _db.Campaigns
            .Include(c => c.Tokens)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign != null)
        {
            CampaignTitle = campaign.Title; 
            Tokens = campaign.Tokens;
            return Page(); // 권한이 있고 캠페인을 찾았다면 페이지를 보여줍니다.
        }

        return RedirectToPage("Index"); // 캠페인이 없으면 목록으로 되돌립니다.
    }
}