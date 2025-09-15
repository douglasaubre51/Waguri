using Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    public class ErrorController(SessionService sessionService) : Controller
    {
        private readonly SessionService _sessionService = sessionService;

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Failure()
        {
            return View();
        }

        public IActionResult GoBack()
        {
            if (_sessionService.IsExpired(HttpContext))
            {
                Console.WriteLine($"session timed out!");
                return RedirectToActionPermanent("Failure", "Error");
            }
            var url = HttpContext.Session.GetString("projectUrl");

            return RedirectPermanent(url);
        }
    }
}
