using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TTCN.Models;
using System.Linq;

namespace TTCN.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLDVContext _context;

        public AccountController(QLDVContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string matKhau)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(matKhau))
            {
                return Json(new { success = false, message = "Vui lòng nhập email và mật khẩu." });
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.MatKhau == matKhau);

            if (user == null)
            {
                return Json(new { success = false, message = "Email hoặc mật khẩu không chính xác." });
            }

            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserHoTen", user.HoTen);
            HttpContext.Session.SetString("UserVaiTro", user.VaiTro);
            HttpContext.Session.SetString("SessionStartTime", DateTime.UtcNow.ToString("o"));

            if (user.VaiTro == "Admin")
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Users") });
            }
            else
            {
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}