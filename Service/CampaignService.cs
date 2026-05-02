using Microsoft.EntityFrameworkCore;
namespace MySurveyApp.Services;
using System.Security.Cryptography;

using MySurveyApp.Data; // <--- 여기도 수정

public class CampaignService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<CampaignService> _logger;

    public CampaignService(ApplicationDbContext db, ILogger<CampaignService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 1. 새 설문/선거 캠페인을 생성하고 고유 토큰들을 발행합니다.
    /// </summary>
    public async Task<Campaign> CreateCampaignAsync(string title, string? description, int tokenCount, List<Question> questions)
    {
        var campaign = new Campaign 
        {
            Title = title,
            Description = description, 
            TokenCount = tokenCount,
            IsActive = true,           
            Questions = questions
        };

        // participantCount 대신 tokenCount 변수를 사용합니다.
        for (int i = 0; i < tokenCount; i++)
        {
            campaign.Tokens.Add(new VoterToken
            {
                Code = GenerateSecureCode(10), // 10자리 보안 코드
                IsUsed = false
            });
        }

        _db.Campaigns.Add(campaign);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation($"새 캠페인 '{title}' 생성 완료 (ID: {campaign.Id}, 토큰: {tokenCount}개)");
        return campaign;
    }

public async Task CreateAdvancedCampaignAsync(string title, int participantCount, List<Question> questions)
{
    var campaign = new Campaign
    {
        Title = title,
        Questions = questions,
        IsActive = true
    };

    // 참여자 수만큼 토큰 생성
    for (int i = 0; i < participantCount; i++)
    {
        campaign.Tokens.Add(new VoterToken { Code = GenerateSecureCode(10) });
    }

    _db.Campaigns.Add(campaign);
    await _db.SaveChangesAsync();
}

    /// <summary>
    /// 2. 입력된 토큰이 유효한지 확인하고 해당 캠페인 정보를 가져옵니다.
    /// </summary>
    public async Task<VoterToken?> GetValidTokenAsync(string code)
    {
        return await _db.VoterTokens
            .Include(t => t.CampaignId) // 어떤 캠페인인지 연결
            .FirstOrDefaultAsync(t => t.Code == code && !t.IsUsed);
    }

    /// <summary>
    /// 3. 핵심: 투표 제출 (익명성 보장 및 중복 방지 트랜잭션)
    /// </summary>
    // CampaignService.cs 내의 SubmitVotesAsync를 이 코드로 교체하세요.
public async Task<bool> SubmitVotesAsync(string tokenCode, Dictionary<int, Dictionary<string, int>> userVotes)
{
    using var transaction = await _db.Database.BeginTransactionAsync();

    try
    {
        var token = await _db.VoterTokens
            .FirstOrDefaultAsync(t => t.Code == tokenCode && !t.IsUsed);

        if (token == null) return false;

        // 질문별(qId)로 순회
        foreach (var qId in userVotes.Keys)
        {
            // 후보자별(optName) 표수(voteCount) 확인
            var options = userVotes[qId];
            foreach (var opt in options)
            {
                string optName = opt.Key;
                int voteCount = opt.Value;

                // 누적 투표 처리: 한 후보에게 준 표수만큼 레코드 생성
                for (int i = 0; i < voteCount; i++)
                {
                    _db.AnonymousResponses.Add(new AnonymousResponse
                    {
                        QuestionId = qId,
                        AnswerValue = optName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        token.IsUsed = true;
        await _db.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        _logger.LogError(ex, "투표 제출 중 오류 발생");
        return false;
    }
}


    /// <summary>
    /// 4. 결과 집계 (누적 투표 및 설문 결과 합산)
    /// </summary>
    public async Task<List<ResultSummary>> GetResultsAsync(int campaignId)
    {
        var questions = await _db.Questions
            .Where(q => q.CampaignId == campaignId)
            .ToListAsync();

        var results = new List<ResultSummary>();

        foreach (var q in questions)
        {
            var responses = await _db.AnonymousResponses
                .Where(r => r.QuestionId == q.Id)
                .ToListAsync();

            results.Add(new ResultSummary
            {
                QuestionText = q.Text,
                // 응답 값들을 그룹화하여 카운트 (예: "찬성": 15명, "반대": 5명)
                VoteCounts = responses.GroupBy(r => r.AnswerValue)
                                     .ToDictionary(g => g.Key, g => g.Count())
            });
        }

        return results;
    }
    // CampaignService.cs 내부에 추가
public async Task ToggleActiveAsync(int id) {
    var campaign = await _db.Campaigns.FindAsync(id);
    if (campaign != null) {
        campaign.IsActive = !campaign.IsActive; // 켜져있으면 끄고, 꺼져있으면 켬
        await _db.SaveChangesAsync();
    }
}

public async Task DeleteCampaignAsync(int id) {
    var campaign = await _db.Campaigns.FindAsync(id);
    if (campaign != null) {
        _db.Campaigns.Remove(campaign);
        await _db.SaveChangesAsync();
    }
}

    // 보안 코드 생성 (숫자 0, 1과 알파벳 O, I 등 헷갈리는 문자 제외)
    private string GenerateSecureCode(int length)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; //수정 예정 
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[RandomNumberGenerator.GetInt32(s.Length)]).ToArray());
    }
}



// 결과 출력을 위한 DTO 클래스
public class ResultSummary
{
    public string QuestionText { get; set; } = string.Empty;
    public Dictionary<string, int> VoteCounts { get; set; } = new();
}

