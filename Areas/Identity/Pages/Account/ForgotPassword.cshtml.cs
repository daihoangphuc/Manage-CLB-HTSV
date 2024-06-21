// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Manage_CLB_HTSV.Data;
using Manage_CLB_HTSV.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using NuGet.Packaging.Signing;

namespace Manage_CLB_HTSV.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;
        public ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, IWebHostEnvironment webHostEnvironment, ApplicationDbContext applicationDbContext)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _context = applicationDbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please
                // visit https://go.microsoft.com/fwlink/?LinkID=532713
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ResetPassword",
                    pageHandler: null,
                    values: new { area = "Identity", code },
                    protocol: Request.Scheme);
/*
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Reset Password",
                    $"Hãy click vào đây để đặt lại mật khẩu <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Đặt lại mật khẩu</a> ");

*/


                // Đường dẫn đến file HTML template
                string templatePath = Path.Combine(_webHostEnvironment.WebRootPath, "emailhtml", "forgot_password_template.html");

                // Đọc nội dung của file template
                string htmlTemplate = System.IO.File.ReadAllText(templatePath);


                var name = Input.Email.Split('@')[0];
                string thoigian = TimeZoneHelper.GetVietNamTime(DateTime.UtcNow).ToString("HH:mm:ss 'Ngày' dd/MM/yyyy");
                string link = $"{ HtmlEncoder.Default.Encode(callbackUrl) }";

                string htmlMessage = htmlTemplate.Replace("{{tennguoidung}}", name).Replace("{{thoigian}}", thoigian).Replace("{{link}}", link);

                try
                {
                    await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Khôi phục mật khẩu",
                    htmlMessage);

                    return RedirectToPage("./ForgotPasswordConfirmation");
                }
                catch (Exception ex)
                {
                   

                }




                
            }

            return Page();
        }
    }
}
