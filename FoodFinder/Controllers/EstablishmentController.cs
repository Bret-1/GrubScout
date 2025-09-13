using Microsoft.AspNetCore.Mvc;
using FoodFinder.Models;

namespace FoodFinder.Controllers
{
    public class EstablishmentController : Controller
    {
        public IActionResult Index()
        {
            var establishment = new Establishment();
            return View(establishment);
        }

    }
}
