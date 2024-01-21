using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Gemini.Controllers.Bussiness;
using Gemini.Models;
using Gemini.Models._01_Hethong;
using Newtonsoft.Json.Linq;
using SINNOVA.Core;
using SINNOVA.Core.Security;

namespace Gemini.Controllers._01_Hethong
{
    public class AdminController : GeminiController
    {
        #region Index
        public ActionResult Index()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var currentPortal = "admin";
            if (route != null)
            {
                currentPortal = route.GetRequiredString("Portal");
            }
            try
            {
                var id = (FormsIdentity)User.Identity;
                FormsAuthenticationTicket authTicket = id.Ticket;
                var currentUsername = authTicket.Name;

                //Block IP or List IP
                if (GetIP() == "192.168.1.2")
                {
                    return RedirectToAction("BlockError", "Admin");
                }

                if (string.IsNullOrEmpty(currentUsername))
                {
                    return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
                }

                ViewBag.Title = "Quản trị hệ thống Version 1.0.0.0 By Mr.Duong";

                return View();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
            }
        }
        #endregion

        #region Login Get
        [HttpGet]
        public ActionResult Login()
        {
            User user = new User();
            user.SecurityCode = (DateTime.Today.Day + DateTime.Today.Month).ToString();
            return View(user);
        }
        #endregion

        #region Register Get
        [HttpGet]
        public ActionResult Register()
        {
            UserRegister userRegister = new UserRegister();

            return View(userRegister);
        }
        #endregion

        #region Logout
        public ActionResult Logout()
        {
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            if (route != null)
            {
                var currentPortal = route.GetRequiredString("Portal");
                var currentMenu = route.GetRequiredString("Menu");
                ViewData["Portal"] = currentPortal;
                ViewData["Menu"] = currentMenu;
            }
            //
            Session.RemoveAll();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Admin");
        }
        #endregion

        #region Login Post
        [HttpPost]
        public ActionResult Login(User user, FormCollection formCollection)
        {
            Log.Debug("AdminController-Login: Đăng nhập");
            var route = RouteTable.Routes.GetRouteData(HttpContext);
            var currentPortal = "admin";
            if (route != null)
            {
                currentPortal = route.GetRequiredString("Portal");
                var currentMenu = route.GetRequiredString("Menu");
                ViewData["Portal"] = currentPortal;
                ViewData["Menu"] = currentMenu;
            }
            try
            {
                if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.SecurityCode))
                {
                    return RedirectToAction("Login", "Admin", new { Portal = currentPortal });
                }
                if (Convert.ToInt16(user.SecurityCode) == Convert.ToInt16(DateTime.Now.Month + DateTime.Now.Day))
                {
                    var expirationDate = 1;
                    //Duy tri dang  nhap
                    if (user.RememberMe)
                    {
                        expirationDate = 30;
                    }
                    GeminiEntities DataGemini = new GeminiEntities();
                    var passWordSha = Encrypt(user.Password.Trim());

                    var erpNsd = DataGemini.SUsers.FirstOrDefault(s => s.Username.ToUpper().Equals(user.Username.ToUpper().Trim()) && s.Password.ToUpper().Equals(passWordSha.ToUpper()));
                    if (erpNsd != null)
                    {
                        if (erpNsd.Active)
                        {
                            var lstMenuLinkUrl = string.Join("|", GetAllMenuInRole(erpNsd.GuidRole).Select(x => x.LinkUrl));

                            var authTicket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddDays(expirationDate), user.RememberMe, vString.GetValueTostring(user.KendoTheme) + ";" + vString.GetValueTostring(user.KendoLanguage) + ";" + vString.GetValueTostring(lstMenuLinkUrl));
                            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(authTicket));
                            if (authTicket.IsPersistent)
                            {
                                cookie.Expires = authTicket.Expiration;
                            }
                            cookie.HttpOnly = true;
                            HttpContext.Response.Cookies.Add(cookie);

                            Log.Debug("AdminController-Login: Đăng nhập thành công");
                            return RedirectToAction("Index", "Admin", new { Portal = currentPortal });
                        }
                        else
                        {
                            Log.Debug("AdminController-Login: Tài khoản chưa kích hoạt");
                            ViewBag.ErrorAcc = "Tài khoản chưa kích hoạt. Bấm <a onclick=\"return SendEmailActive();\">vào đây</a> để nhận Email kích hoạt tài khoản.";
                            return View(user);
                        }
                    }

                    Log.Debug("AdminController-Login: Đăng nhập thất bại");
                    ViewBag.ErrorAcc = "Sai tên đăng nhập hoặc mật khẩu. Vui lòng kiểm tra lại.";
                    return View(user);
                }
                ViewBag.ErrorAcc = "Sai mã bảo mật. Vui lòng kiểm tra lại.";
                return View(user);
            }
            catch
            {
                Log.Debug("AdminController-Login: Đăng nhập thất bại");
                ViewBag.ErrorAcc = "Đăng nhập thất bại. Vui lòng thử lại sau ít phút.";
                return View(user);
            }

        }
        #endregion

        #region Register Post

        public ActionResult Register(UserRegister userRegister, FormCollection registerCollection)
        {
            var _obj = new SUser();
            try
            {
                if (!userRegister.Password.Equals(userRegister.PasswordAgain))
                {
                    ViewBag.ErrorAcc = "Mật khẩu không trùng nhau";
                    return View(userRegister);
                }
                else
                {
                    //var response = Request["g-recaptcha-response"];
                    //string secretKey = "6LdMLlkUAAAAAHawi_ylz7A3eUa_ioaGB9DWoq9m";
                    //var client = new WebClient();
                    //var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
                    //var obj = JObject.Parse(result);
                    //var status = (bool)obj.SelectToken("success");
                    var status = true;
                    if (status)
                    {
                        var user = DataGemini.SUsers.FirstOrDefault(s => s.Username.ToUpper().Equals(userRegister.Username.ToUpper()) || s.Email.ToUpper().Equals(userRegister.Email.ToUpper()) || s.Mobile.ToUpper().Equals(userRegister.Mobile.ToUpper()));
                        var roleUser = DataGemini.SRoles.FirstOrDefault(s => s.Name.ToUpper().Equals("MANUFACTURING"));
                        if (user == null)
                        {
                            _obj.Guid = Guid.NewGuid();
                            _obj.GuidRole = roleUser.Guid;
                            _obj.Position = roleUser.Name;
                            _obj.CreatedBy = roleUser.Name;
                            _obj.UpdatedBy = roleUser.Name;
                            _obj.UpdatedAt = DateTime.Now;
                            _obj.Active = false;
                            _obj.RecordsInPage = 100;
                            _obj.FullName = userRegister.FullName;
                            _obj.Username = userRegister.Username;
                            _obj.Password = Encrypt(userRegister.Password);
                            _obj.Mobile = userRegister.Mobile;
                            _obj.Email = userRegister.Email;
                            _obj.StartDate = DateTime.UtcNow;
                            _obj.CreatedAt = DateTime.UtcNow;
                            _obj.EndDate = DateTime.UtcNow.AddDays(30);
                            DataGemini.SUsers.Add(_obj);
                            if (SaveData("SUsers"))
                            {
                                Log.Debug("AdminController-Register: Đăng ký thành công User: " + _obj.Username);
                                SendEmails(CrmEmailTemplate_Code.ActiveAccount, _obj);
                                return RedirectToAction("Login", "Admin");
                            }

                            return RedirectToAction("Register", "Admin");
                        }
                        else
                        {
                            ViewBag.ErrorAcc = "Tài khoản đã tồn tại hãy kiểm tra lại email/SĐT và tên đăng nhập!";
                            Log.Debug("AdminController-Register: Đăng ký thất bại");
                            return View(userRegister);
                        }
                    }
                    else
                    {
                        Log.Debug("AdminController-Register: Response null");
                        return View(userRegister);
                    }
                }
            }
            catch (Exception ex)
            {
                DataGemini.SUsers.Remove(_obj);
                HandleError(ex);
                return Json(DataReturn, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region GetLocalIpAddress
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        #endregion

        #region Block Ip
        public ActionResult BlockError(Exception error)
        {
            Response.StatusCode = 505;
            ViewBag.Message = GetIP();
            return null;
        }

        private string GetIP()
        {
            return HttpContextUtility.getIpAdress();
        }
        #endregion

        public ActionResult SendEmailActive(string username)
        {
            var _obj = DataGemini.SUsers.FirstOrDefault(s => s.Username.ToUpper().Equals(username.ToUpper()));

            if (_obj == null)
            {
                DataReturn.MessagError = "Tài khoản không tồn tại.";
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
            }
            else
            {
                SendEmails(CrmEmailTemplate_Code.ActiveAccount, _obj);
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ActiveAccount(string token)
        {
            var plainText = Decrypt(token);

            var lstData = plainText.Split('|');

            if (lstData.Length == 3)
            {
                string username = lstData[0];
                string email = lstData[1];
                string expiryDateString = lstData[2];

                DateTime expiryDate = DateTime.Parse(expiryDateString);

                if (DateTime.Now < expiryDate)
                {
                    Log.Debug("[ActiveAccount] " + username + "|" + email);
                    var erpNsd = DataGemini.SUsers.FirstOrDefault(s => s.Username.ToUpper().Equals(username.ToUpper()) && s.Email.ToUpper().Equals(email.ToUpper()));
                    if (erpNsd != null)
                    {
                        if (erpNsd.Active == false)
                        {
                            erpNsd.Active = true;
                            try
                            {
                                DataGemini.SaveChanges();
                                Log.Debug("Active account " + username + "|" + email + "Thành công");

                                return RedirectToAction("Login", "Admin");
                            }
                            catch
                            {
                                return Content("Kích hoạt tài khoản thất bại. Thử lại sau vài phút.");
                            }
                        }
                    }
                    else
                    {
                        return Content("Tài khoản kích hoạt không tồn tại.");
                    }
                }
                else
                {
                    return Content("Thời gian kích hoạt tài khoản đã hết. Vui lòng đăng nhập để gửi lại mã kích hoạt.");
                }
            }

            return Content("Kích hoạt tài khoản thất bại. Thử lại sau vài phút.");
        }

        public ActionResult NotAuthorized()
        {
            return Redirect("/Error/ErrorList");
        }
    }
}