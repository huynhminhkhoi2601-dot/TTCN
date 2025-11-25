using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TTCN.Models;

namespace TTCN.Controllers
{
    public class UsersController : AdminBaseController
    {
        private readonly QLDVContext _context;

        public UsersController(QLDVContext context)
        {
            this._context = context;
        }

        public IActionResult Index()
        {
            ViewBag.p = _context.Users.ToList();
            return View();
        }
    }
}