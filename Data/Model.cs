using System.ComponentModel.DataAnnotations;

namespace MySurveyApp.Data; // <--- App이 붙은 이름 확인

// 1. 전체 설문/선거 캠페인
// 1. 전체 설문/선거 캠페인
public class Campaign {
    public int Id { get; set; }
    public string Title { get; set; } = "";
    
    public string? Description { get; set; } 
    public int TokenCount { get; set; }
    
    // --- 이 두 줄이 반드시 있어야 합니다! ---
    public bool IsActive { get; set; } = true; 
    public List<VoterToken> Tokens { get; set; } = new(); 
    
    public List<Question> Questions { get; set; } = new();
}

// 2. 개별 질문 또는 후보자
public class Question {
    public int Id { get; set; }
    public string Text { get; set; } = "";
    
    // 이 부분을 추가해 주세요!
    public string? Description { get; set; } 
    
    public string Type { get; set; } = "Survey";
    public int MaxPoints { get; set; } = 1;
    
    public List<QuestionOption> Options { get; set; } = new();
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; } = null!;
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

