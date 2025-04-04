using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStore.Models;

namespace TẹchStore.Controllers
{
    public class ManageController : Controller
    {
        public IActionResult View()
        {
            return View();
        }
    }   
}