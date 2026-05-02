using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySurveyApp.Data;
using MySurveyApp.Services;

namespace MySurveyApp.Pages.Admin;

public class AdminCreateModel : PageModel {
    
    private readonly CampaignService _service;
    public AdminCreateModel(CampaignService service) => _service = service;

    [BindProperty] public string CampaignTitle { get; set; } = "";
    [BindProperty] public string CampaignDescription { get; set; } = ""; // 추가: 전체 설문(캠페인) 설명
    [BindProperty] public int TokenCount { get; set; } = 20;
    [BindProperty] public List<QuestionInput> Questions { get; set; } = new();

    public IActionResult OnGet() {
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync() {
        if (HttpContext.Session.GetString("IsAdmin") != "true") {
            return RedirectToPage("Login");
        }

        if (!Questions.Any()) {
            ModelState.AddModelError("", "최소 하나 이상의 안건을 생성해야 합니다.");
            return Page();
        }

        var mappedQuestions = Questions.Select(q => new Question {
            Text = q.Text,
            Description = q.Description, // 추가: 개별 안건 설명 매핑
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

        // 참고: CampaignService의 CreateCampaignAsync 시그니처에 CampaignDescription이 추가되도록 서비스 코드를 수정해야 합니다.
        await _service.CreateCampaignAsync(CampaignTitle, CampaignDescription, TokenCount, mappedQuestions);
        return RedirectToPage("Index");
    }

    public class QuestionInput {
        public string Text { get; set; } = "";
        public string Description { get; set; } = ""; // 추가: 개별 안건 설명
        public string Type { get; set; } = "Survey";
        public int MaxPoints { get; set; } = 1;
        public string OptionsRaw { get; set; } = "";
    }
}