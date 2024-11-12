using Microsoft.AspNetCore.Mvc;
using UserManagement.Services;

namespace UserManagement.Controllers;

public class CaptchaController: Controller
{
    public ActionResult GetCaptchaImage()
    {
        var (imageData, answer) = CaptchaService.GenerateCaptcha();
        HttpContext.Session.SetString("CaptchaAnswer", answer);
        return File(imageData, "image/png");
    }
    
}