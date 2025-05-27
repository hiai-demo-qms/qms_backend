using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ChatbotController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
