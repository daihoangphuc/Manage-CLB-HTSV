using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using website_CLB_HTSV.Models;

namespace website_CLB_HTSV.Controllers
{
    [Authorize(Roles = "Administrators")]   
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Hiển thị danh sách người dùng
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var users = from s in _userManager.Users select s;
            if (!string.IsNullOrEmpty(searchString))
            { 
                users = users.Where(x=>x.UserName.Contains(searchString));
            }
            int pageSize = 10; // Số lượng mục trên mỗi trang
            return View(await PaginatedList<IdentityUser>.CreateAsync(users.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // Xóa người dùng
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(nameof(Index));
        }

        // Thêm vai trò
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View();
        }

        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            ViewBag.UserId = id;
            ViewBag.UserName = user.UserName;
            ViewBag.Roles = new SelectList(roles, "Value", "Text");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(string id, string roleName)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Lấy danh sách vai trò hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);

            if (string.IsNullOrEmpty(roleName))
            {
                // Nếu người dùng không chọn vai trò mới, sử dụng vai trò cũ
                roleName = userRoles.FirstOrDefault();
            }

            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng đã có vai trò này chưa
            var userRoleExists = await _userManager.IsInRoleAsync(user, roleName);

            if (!userRoleExists)
            {
                // Thêm mối quan hệ người dùng - vai trò vào cơ sở dữ liệu
                await _userManager.AddToRoleAsync(user, roleName);

                // Loại bỏ vai trò cũ nếu cần
                foreach (var oldRole in userRoles)
                {
                    if (oldRole != roleName)
                    {
                        await _userManager.RemoveFromRoleAsync(user, oldRole);
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Người dùng đã có vai trò này, không cần thêm lại
            ModelState.AddModelError("", "User already has this role.");

            var roles = await _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                })
                .ToListAsync();

            ViewBag.UserId = id;
            ViewBag.UserName = user.UserName;
            ViewBag.Roles = new SelectList(roles, "Value", "Text");

            return View();
        }

    }
}
