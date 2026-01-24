using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BCrypt.Net;

namespace MySurveyApp.Pages.Admin;

public class LoginModel : PageModel {
    // 1. 설정 파일을 읽기 위한 필드를 선언합니다.
    private readonly IConfiguration _config;

    // 2. 생성자를 통해 IConfiguration을 주입(Injection)받습니다.
    public LoginModel(IConfiguration config) {
        _config = config;
    }

    [BindProperty] public string AdminPassword { get; set; } = "";
    public string ErrorMessage { get; set; } = "";

    public void OnGet() {
        // 로그인 페이지 접속 시 세션을 초기화하거나 필요한 작업을 수행할 수 있습니다.
    }

    public IActionResult OnPost() {
        // 3. 이제 _config를 사용하여 환경 변수나 설정 파일의 값을 읽을 수 있습니다.
        string storedHash = Environment.GetEnvironmentVariable("ADMIN_PWD_HASH") 
                            ?? _config["AdminSettings:PasswordHash"] 
                            ?? "";

        if (!string.IsNullOrEmpty(storedHash) && BCrypt.Net.BCrypt.Verify(AdminPassword, storedHash)) {
            HttpContext.Session.SetString("IsAdmin", "true");
            return RedirectToPage("Index");
        }

        ErrorMessage = "비밀번호가 올바르지 않습니다.";
        return Page();
    }
}