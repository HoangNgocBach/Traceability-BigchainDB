using System.Web;
using System.Web.Mvc;

namespace Gemini.Controllers._01_Hethong
{
    public class CacheController : Controller
    {
        //
        // GET: /Cache/

        public ActionResult Index()
        {
            HttpRuntime.Cache.Remove("Menu");
            HttpRuntime.Cache.Remove("Dichvu");
            HttpRuntime.Cache.Remove("HopTac");
            HttpRuntime.Cache.Remove("HotProduce");
            HttpRuntime.Cache.Remove("CategoryRoot");
            HttpRuntime.Cache.Remove("Slider");
            HttpRuntime.Cache.Remove("TopProduce");
            return View();
        }

    }
}
