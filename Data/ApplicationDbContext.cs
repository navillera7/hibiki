using Microsoft.EntityFrameworkCore;

namespace MySurveyApp.Data; // <--- App이 붙은 이름 확인
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DB 테이블들
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<VoterToken> VoterTokens { get; set; }
    public DbSet<AnonymousResponse> AnonymousResponses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 토큰 코드는 중복되지 않도록 유니크 설정
        modelBuilder.Entity<VoterToken>()
            .HasIndex(t => t.Code)
            .IsUnique();
            
        // 익명 응답 테이블에는 유저 정보를 아예 담지 않도록 설계됨 (보안 핵심)
    }
}