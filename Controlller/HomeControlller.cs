using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStore.Models;
using TechStore.Data;
using X.PagedList;
public class HomeController : Controller
{
    public IActionResult ThumbNail()
    {
        return View();
    }
}
