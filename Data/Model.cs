using System.ComponentModel.DataAnnotations;

namespace MySurveyApp.Data; // <--- App이 붙은 이름 확인

// 1. 전체 설문/선거 캠페인
public class Campaign {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<Question> Questions { get; set; } = new();
    public List<VoterToken> Tokens { get; set; } = new();
}

// 2. 개별 질문 또는 후보자
public class Question {
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = "Survey"; // Survey 또는 Election
    public int MaxPoints { get; set; } = 1; // 1인당 투표 가능 수
    public List<QuestionOption> Options { get; set; } = new();
}

public class QuestionOption {
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Name { get; set; } = string.Empty; // 후보자 이름/선택지
    public string? Description { get; set; } // 공약/설명
}
// 3. 사용자 접속용 익명 토큰
public class VoterToken
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    
    // 이 줄을 추가하세요! (Navigation Property)
    // 이것이 있어야 .Include(t => t.Campaign) 코드가 작동합니다.
    public virtual Campaign Campaign { get; set; } = default!; 

    [Required]
    public string Code { get; set; } = string.Empty;
    public bool IsUsed { get; set; } = false;
}

// 4. 투표 결과 (익명 보장을 위해 Token과 연결하지 않음!)
public class AnonymousResponse
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string AnswerValue { get; set; } = string.Empty; // "찬성", "A후보 2표" 등
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

