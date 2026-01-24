using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySurveyApp.Data;
using MySurveyApp.Services;

namespace MySurveyApp.Pages.Admin;

public class AdminCreateModel : PageModel {
    
    private readonly CampaignService _service;
    public AdminCreateModel(CampaignService service) => _service = service;

    [BindProperty] public string CampaignTitle { get; set; } = "";
    [BindProperty] public int TokenCount { get; set; } = 20;
    [BindProperty] public List<QuestionInput> Questions { get; set; } = new();

    // 1. 페이지 접속 시 관리자 권한 확인을 위해 OnGetAsync를 추가합니다.
    public IActionResult OnGet() {
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync() {
        // 2. 폼 제출 시에도 권한을 확인합니다.
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }

        if (!Questions.Any()) {
            ModelState.AddModelError("", "최소 하나 이상의 안건을 생성해야 합니다.");
            return Page();
        }

        var mappedQuestions = Questions.Select(q => new Question {
            Text = q.Text,
            Type = q.Type,
            MaxPoints = q.MaxPoints,
            Options = q.OptionsRaw?.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => {
                    var parts = line.Split(':');
                    return new QuestionOption { 
                        Name = parts[0].Trim(), 
                        Description = parts.Length > 1 ? parts[1].Trim() : null 
                    };
                }).ToList() ?? new List<QuestionOption>()
        }).ToList();

        await _service.CreateCampaignAsync(CampaignTitle, TokenCount, mappedQuestions);
        return RedirectToPage("Index");
    }

    public class QuestionInput {
        public string Text { get; set; } = "";
        public string Type { get; set; } = "Survey";
        public int MaxPoints { get; set; } = 1;
        public string OptionsRaw { get; set; } = "";
    }
}