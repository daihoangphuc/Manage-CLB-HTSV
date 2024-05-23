using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services; // Import để sử dụng IEmailSender
using System;
using System.Threading.Tasks;

namespace website_CLB_HTSV.Controllers
{
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender; // Inject IEmailSender

        public IActionResult Index()
        {
            return View();
        }

        public AdminController(UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender; // Inject IEmailSender vào constructor
        }

        // Các action khác của controller

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User with this email does not exist.");
                return View();
            }

            var newPassword = GenerateRandomPassword(); // Tạo mật khẩu ngẫu nhiên mới
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                // Gửi email cho người dùng với mật khẩu mới
                await _emailSender.SendEmailAsync(email, "Reset Password", $"Your new password is: {newPassword}");

                return RedirectToAction("ResetPasswordConfirmation");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View();
            }
        }

        private string GenerateRandomPassword()
        {
            string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random randNum = new Random();
            char[] chars = new char[8];
            for (int i = 0; i < 8; i++)
            {
                chars[i] = allowedChars[(int)(randNum.NextDouble() * allowedChars.Length)];
            }
            return new string(chars);
        }
    }
}
