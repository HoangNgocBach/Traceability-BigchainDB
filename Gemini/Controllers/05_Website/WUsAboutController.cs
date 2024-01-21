using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gemini.Controllers.Bussiness;
using Gemini.Models;
using Gemini.Models._05_Website;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Constants = Gemini.Controllers.Bussiness.Constants;

namespace Gemini.Controllers._05_Website
{
    [CustomAuthorizeAttribute]
    public class WUsAboutController : GeminiController
    {
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            GetSettingUser();
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            List<WUsAbout> wUsAbout = DataGemini.WUsAbouts.OrderByDescending(p => p.CreatedAt).ToList();
            var data = ConvertIEnumerate(wUsAbout);
            var result = data.OrderByDescending(x => x.CreatedAt).ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<WUsAboutModel> ConvertIEnumerate(IEnumerable<WUsAbout> source)
        {
            return source.Select(item => new WUsAboutModel(item)).ToList();
        }

        public ActionResult Create()
        {
            try
            {
                var wUsAbout = new WUsAbout();
                var viewModel = new WUsAboutModel(wUsAbout) { IsUpdate = 0, Active = false };
                return PartialView("Edit", viewModel);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Edit(Guid guid)
        {
            try
            {
                var wUsAbout = new WUsAbout();
                wUsAbout = DataGemini.WUsAbouts.FirstOrDefault(c => c.Guid == guid);
                var viewModel = new WUsAboutModel(wUsAbout) { IsUpdate = 1 };
                return PartialView("Edit", viewModel);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Delete(Guid guid)
        {
            try
            {
                var wUsAbout = new WUsAbout();
                wUsAbout = DataGemini.WUsAbouts.FirstOrDefault(c => c.Guid == guid);
                DataGemini.WUsAbouts.Remove(wUsAbout);
                if (SaveData("WUsAbout") && wUsAbout != null)
                {
                    DataReturn.ActiveCode = wUsAbout.Guid.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.BadRequest);
                    DataReturn.MessagError = Constants.CannotDelete + " Date : " + DateTime.Now;
                }

            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(WUsAboutModel viewModel)
        {
            var wUsAbout = new WUsAbout();
            try
            {
                var lstErrMsg = ValidateDuplicate(viewModel);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                    if (viewModel.IsUpdate == 0)
                    {
                        viewModel.Setvalue(wUsAbout);
                        DataGemini.WUsAbouts.Add(wUsAbout);
                    }
                    else
                    {
                        wUsAbout = DataGemini.WUsAbouts.FirstOrDefault(c => c.Guid == viewModel.Guid);
                        viewModel.Setvalue(wUsAbout);
                    }
                    if (SaveData("WUsAbout") && wUsAbout != null)
                    {
                        DataReturn.ActiveCode = wUsAbout.Guid.ToString();
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    }
                    else
                    {
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                        DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    DataGemini.WUsAbouts.Remove(wUsAbout);
                }
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> ValidateDuplicate(WUsAboutModel viewModel)
        {
            List<string> lstErrMsg = new List<string>();

            var lstProduce = DataGemini.WUsAbouts.Where(c => c.Active && c.Guid != viewModel.Guid);

            if (lstProduce.Count() > 0)
            {
                lstErrMsg.Add("Tồn tại bản ghi đang kích hoạt!");
            }

            return lstErrMsg;
        }

        public ActionResult Copy(Guid guid)
        {
            var sTypes = new WUsAbout();
            var clone = new WUsAbout();
            try
            {
                sTypes = DataGemini.WUsAbouts.FirstOrDefault(c => c.Guid == guid);
                #region Copy
                DataGemini.WUsAbouts.Add(clone);
                //Copy values from source to clone
                var sourceValues = DataGemini.Entry(sTypes).CurrentValues;
                DataGemini.Entry(clone).CurrentValues.SetValues(sourceValues);
                //Change values of the copied entity
                clone.Name = clone.Name + " - Copy";
                clone.Guid = Guid.NewGuid();
                clone.Active = false;
                clone.UpdatedAt = clone.CreatedAt = DateTime.Now;
                clone.UpdatedBy = clone.CreatedBy = GetUserInSession();
                if (SaveData("WUsAbout"))
                {
                    DataReturn.ActiveCode = clone.Guid.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = Constants.CannotCopy + " Date : " + DateTime.Now;
                }
                #endregion
            }
            catch (Exception ex)
            {
                DataGemini.WUsAbouts.Remove(clone);
                HandleError(ex);
            }


            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
    }
}