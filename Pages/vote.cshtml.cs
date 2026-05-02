using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;
using MySurveyApp.Services;

namespace MySurveyApp.Pages;

public class VoteModel : PageModel
{
    private readonly CampaignService _service;
    private readonly ApplicationDbContext _db;

    public VoteModel(CampaignService service, ApplicationDbContext db)
    {
        _service = service;
        _db = db;
    }

    // 뒤에 = default!; 를 붙여 경고를 제거합니다.
    [BindProperty] public string Token { get; set; } = default!;
    public string CampaignTitle { get; set; } = default!;
    public string? CampaignDescription { get; set; }
    public List<Question> Questions { get; set; } = default!;

public async Task<IActionResult> OnGetAsync(string token)
{
    
    // Include를 통해 질문의 선택지(Options)까지 싹 다 긁어옵니다.
    var validToken = await _db.VoterTokens
        .Include(t => t.Campaign)
            .ThenInclude(c => c.Questions)
                .ThenInclude(q => q.Options) 
        .FirstOrDefaultAsync(t => t.Code == token && !t.IsUsed);

    if (validToken == null) return RedirectToPage("Index");

    Token = token;
    CampaignTitle = validToken.Campaign.Title;
    CampaignDescription = validToken.Campaign.Description;
    Questions = validToken.Campaign.Questions;

    return Page();
}

[BindProperty]
public Dictionary<int, Dictionary<string, int>> VoteData { get; set; } = new();

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();

    // 1. 토큰 유효성 및 사용 여부 재확인
    var tokenRecord = await _db.VoterTokens
        .FirstOrDefaultAsync(t => t.Code == Token && !t.IsUsed);
    
    if (tokenRecord == null) return RedirectToPage("Index");

    // 2. 투표 데이터 저장
    foreach (var qId in VoteData.Keys)
    {
        var options = VoteData[qId];
        foreach (var optName in options.Keys)
        {
            int voteCount = options[optName];
            
            // 한 후보에게 던진 표수만큼 반복해서 저장 (결과 집계용)
            for (int i = 0; i < voteCount; i++)
            {
                _db.AnonymousResponses.Add(new AnonymousResponse
                {
                    QuestionId = qId,
                    AnswerValue = optName
                });
            }
        }
    }

    // 3. 토큰 사용 처리
    tokenRecord.IsUsed = true;
    await _db.SaveChangesAsync();

    return RedirectToPage("Success");
}
}