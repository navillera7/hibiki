using Microsoft.EntityFrameworkCore;
using MySurveyApp.Data;
using MySurveyApp.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 수정된 부분 시작 ---
// 환경에 따라 DB 저장 경로를 다르게 설정합니다.
var dbPath = builder.Environment.IsDevelopment() 
    ? "Data Source=SurveyApp.db"             // 로컬(내 컴퓨터)에서는 기존처럼 현재 폴더에 저장
    : "Data Source=/app/data/SurveyApp.db";  // Railway(운영)에서는 영구 보존 볼륨에 저장

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(dbPath));
// --- 수정된 부분 끝 ---
    builder.Services.AddScoped<CampaignService>();
// Add services to the container.

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30분간 활동 없으면 로그아웃
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages();

var app = builder.Build();
app.UseSession(); // app.UseRouting() 다음에 위치하는 것이 좋습니다.

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();
// Program.cs 하단 (app.Run() 직전)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // DB가 확실히 생성되었는지 확인
    db.Database.EnsureCreated();

    // 데이터가 하나도 없을 때만 테스트 데이터 생성
    if (!db.Campaigns.Any())
    {
        var testCampaign = new Campaign
        {
            Title = "첫 번째 익명 투표 테스트",
            IsActive = true
        };

        // 질문 추가
        testCampaign.Questions.Add(new Question { Text = "오늘 점심 메뉴는 만족스러웠나요?", Type = "Survey" });
        testCampaign.Questions.Add(new Question { Text = "차기 회장 후보 (최대 2표 가능)", Type = "Election", MaxPoints = 2 });

        // 테스트용 토큰 추가
        testCampaign.Tokens.Add(new VoterToken { Code = "1234", IsUsed = false });

        db.Campaigns.Add(testCampaign);
        db.SaveChanges();
    }
}
app.Run();
