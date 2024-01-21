using System.Web.Mvc;
using Gemini.Controllers.Bussiness;

namespace Gemini.Controllers._20_Web.Home
{
    public class HomeController : GeminiController
    {
        public ActionResult Index()
        {
            var currentUsername = GetUserInSession();
            ViewBag.CurrentUsername = !string.IsNullOrWhiteSpace(currentUsername) ? currentUsername : "Đăng nhập";

            return View("Index");
        }
    }
}