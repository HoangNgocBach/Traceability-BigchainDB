using System;
using System.Linq;
using Gemini.Models;
using System.Web.Mvc;
using System.Web.Routing;

namespace Gemini
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            try
            {
                routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

                #region Add portal admin in router
                var db = new GeminiEntities();

                routes.MapRoute(
                    name: "admin",
                    url: "admin" + "/{controller}/{action}/{id}/{Menu}",
                    defaults: new { Portal = "admin", controller = "Admin", action = "Index", id = UrlParameter.Optional, Menu = "start" }
                );
                #endregion

                #region Web
                routes.MapRoute(
                    name: "SanPhamDanhMuc",
                    url: "danh-muc/{guidCategory}",
                    defaults: new { controller = "HomeCommon", action = "ProduceList", guidCategory = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "SanPhamChiTiet",
                    url: "san-pham/{guidProduce}",
                    defaults: new { controller = "HomeCommon", action = "ProduceDetail", guidProduce = UrlParameter.Optional }
                );

                routes.MapRoute(
                    name: "VeChungToi",
                    url: "ve-chung-toi",
                    defaults: new { controller = "HomeCommon", action = "AboutUs" }
                );

                var sMenus = db.SMenus.Where(p => p.Type == "WEB").ToList();

                foreach (SMenu itemMenu in sMenus)
                {
                    bool isAdd = true;
                    string routeUrl = (itemMenu.RouterUrl);
                    string name = (itemMenu.Guid.ToString());
                    string url = (itemMenu.LinkUrl);
                    if (!string.IsNullOrEmpty(routeUrl))
                    {
                        //Kiem tra trung
                        foreach (Route route in routes)
                        {
                            if ((url == route.Url))
                            {
                                isAdd = false;
                            }
                        }
                        //Add route neu khong trung
                        if (isAdd)
                        {
                            var routerDic = new RouteValueDictionary();
                            string[] arr = routeUrl.Split(';');
                            foreach (string item in arr)
                            {
                                if (!string.IsNullOrEmpty(item))
                                {
                                    string[] arr1 = item.Split('=');
                                    if (arr1.Length > 1)
                                    {
                                        routerDic.Add(arr1[0], arr1[1]);
                                    }
                                }

                            }

                            routes.MapRoute(
                                name: name,
                                url: url,
                                defaults: routerDic
                            );
                        }
                    }
                }
                #endregion

                #region Router defaults
                //==================================================================//
                // Router cho trang chu va cac PartialView
                routes.MapRoute(
                    name: "Default",
                    url: "{controller}/{action}/{id}/{id1}",
                    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional, id1 = UrlParameter.Optional }
                );
                #endregion
            }
            catch (Exception ex)
            {

            }
        }
    }
}