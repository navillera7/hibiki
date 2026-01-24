using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;
using MySurveyApp.Services;
// using BCrypt.Net; // 1. 이 파일에서는 BCrypt를 직접 사용하지 않으므로 삭제해도 됩니다.
    
namespace MySurveyApp.Pages.Admin;

public class AdminIndexModel : PageModel {
    private readonly ApplicationDbContext _db;
    private readonly CampaignService _service;

    public AdminIndexModel(ApplicationDbContext db, CampaignService service) {
        _db = db;
        _service = service;
    }

    public List<Campaign> CampaignList { get; set; } = new();

    public async Task<IActionResult> OnGetAsync() {
        // 관리자 권한 확인
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }

        CampaignList = await _db.Campaigns
            .Include(c => c.Questions)
            .Include(c => c.Tokens)
            .ToListAsync();

        return Page();
    }
    
    public async Task<IActionResult> OnPostToggleActiveAsync(int id) {
        if (HttpContext.Session.GetString("IsAdmin") != "true") return RedirectToPage("Login");

        await _service.ToggleActiveAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id) {
        if (HttpContext.Session.GetString("IsAdmin") != "true") return RedirectToPage("Login");

        await _service.DeleteCampaignAsync(id);
        return RedirectToPage();
    }

    // 2. 로그아웃 기능을 추가하고 싶다면 아래 핸들러를 넣으세요.
    public IActionResult OnPostLogout() {
        HttpContext.Session.Remove("IsAdmin"); // 세션 삭제
        return RedirectToPage("/Index"); // 메인 투표 접속 페이지로 이동
    }
}