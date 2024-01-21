using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gemini.Controllers.Bussiness;
using Gemini.Models;
using Gemini.Models._03_Pos;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Constants = Gemini.Controllers.Bussiness.Constants;

namespace Gemini.Controllers._03_Pos
{
    [CustomAuthorizeAttribute]
    public class PosCategoryController : GeminiController
    {
        #region Main

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
            List<PosCategory> posCategory = DataGemini.PosCategories.OrderBy(p => p.OrderBy).ToList();
            var data = ConvertIEnumerate(posCategory);
            var roots = BuildTree(data);
            foreach (var item in roots)
            {
                AppendChars(item);
            }

            var result = data.OrderBy(x => x.RootId).ThenBy(x => x.OrderBy).ToDataSourceResult(request);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            var posCategory = new PosCategory();
            try
            {
                var viewModel = new PosCategoryModel(posCategory) { IsUpdate = 0, Active = true };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }

        }

        public ActionResult Edit(Guid guid)
        {
            var posCategory = new PosCategory();
            try
            {
                PosCategoryModel viewModel;
                posCategory = DataGemini.PosCategories.FirstOrDefault(c => c.Guid == guid);
                if (posCategory.ParentGuid == null)
                {
                    viewModel = new PosCategoryModel(posCategory) { IsUpdate = 1 };
                }
                else
                {
                    var parent = DataGemini.PosCategories.FirstOrDefault(x => x.Guid == posCategory.ParentGuid);
                    viewModel = new PosCategoryModel(posCategory) { IsUpdate = 1 };
                }
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }


        }

        public ActionResult Delete(Guid guid)
        {
            var posCategory = new PosCategory();
            try
            {
                posCategory = DataGemini.PosCategories.FirstOrDefault(c => c.Guid == guid);

                #region Remove Produce and FProduceGallery

                var posProduce = DataGemini.PosProduces.Where(x => x.GuidCategory == guid).ToList();
                foreach (var item in posProduce)
                {
                    var fProduceGallery = DataGemini.FProduceGalleries.Where(x => x.GuidProduce == item.Guid).ToList();
                    DataGemini.FProduceGalleries.RemoveRange(fProduceGallery);
                }
                DataGemini.PosProduces.RemoveRange(posProduce);

                #endregion

                #region Remove Categories Child

                var childCategory = DataGemini.PosCategories.Where(c => c.ParentGuid == guid).ToList();
                if (childCategory.Any())
                {
                    DataGemini.PosCategories.RemoveRange(childCategory);
                }

                #endregion

                DataGemini.PosCategories.Remove(posCategory);
                if (SaveData("PosCategory") && posCategory != null)
                {
                    DataReturn.ActiveCode = posCategory.Guid.ToString();
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
        public ActionResult Update(PosCategoryModel viewModel)
        {
            var posCategory = new PosCategory();
            try
            {
                viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                if (viewModel.IsUpdate == 0)
                {
                    viewModel.Setvalue(posCategory);
                    DataGemini.PosCategories.Add(posCategory);
                }
                else
                {
                    posCategory = DataGemini.PosCategories.FirstOrDefault(c => c.Guid == viewModel.Guid);
                    viewModel.Setvalue(posCategory);
                }
                if (SaveData("PosCategory") && posCategory != null)
                {
                    DataReturn.ActiveCode = posCategory.Guid.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    DataGemini.PosCategories.Remove(posCategory);
                }
                HandleError(ex);
            }
            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<PosCategoryModel> ConvertIEnumerate(IEnumerable<PosCategory> source)
        {
            return source.Select(item => new PosCategoryModel(item)).ToList();
        }

        public ActionResult Copy(Guid guid)
        {
            var posCategory = new PosCategory();
            var clone = new PosCategory();
            try
            {
                posCategory = DataGemini.PosCategories.FirstOrDefault(c => c.Guid == guid);
                #region Copy
                DataGemini.PosCategories.Add(clone);
                //Copy values from source to clone
                var sourceValues = DataGemini.Entry(posCategory).CurrentValues;
                DataGemini.Entry(clone).CurrentValues.SetValues(sourceValues);
                //Change values of the copied entity
                clone.Name = clone.Name + " - Copy";
                clone.Guid = Guid.NewGuid();
                clone.UpdatedAt = clone.CreatedAt = DateTime.Now;
                clone.UpdatedBy = clone.CreatedBy = GetUserInSession();
                if (SaveData("PosCategory"))
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
                DataGemini.PosCategories.Remove(clone);
                HandleError(ex);
            }


            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        #endregion

        public static IList<PosCategoryModel> BuildTree(IEnumerable<PosCategoryModel> source)
        {
            IList<PosCategoryModel> roots = new BindingList<PosCategoryModel>();
            var groups = source.GroupBy(i => i.ParentGuid).ToList();
            var rootgroups = groups.FirstOrDefault(g => g.Key.HasValue == false);
            if (rootgroups != null)
            {
                roots = rootgroups.ToList();
                if (roots.Count > 0)
                {
                    var dict = groups.Where(g => g.Key.HasValue).ToDictionary(g => g.Key.Value, g => g.ToList());
                    int order = 0;
                    foreach (var t in roots)
                    {
                        order++;
                        t.RootId = order;
                        AddChildren(t, dict, ref order);
                    }
                }
            }

            return roots;
        }

        private static void AddChildren(PosCategoryModel node, IDictionary<Guid, List<PosCategoryModel>> source, ref int order)
        {
            if (source.ContainsKey(node.Guid))
            {
                node.Items = source[node.Guid];
                foreach (PosCategoryModel t in node.Items)
                {
                    order++;
                    t.RootId = order;
                    AddChildren(t, source, ref order);
                }
            }
            else
            {
                node.Items = new List<PosCategoryModel>();
            }
        }

        private static void AppendChars(PosCategoryModel posCategory, string append = "")
        {
            if (posCategory.Items != null && posCategory.Items.Any())
            {
                append += ">> ";
                foreach (var item in posCategory.Items)
                {
                    item.Name = string.Format("{0} {1}", append, item.Name);
                    AppendChars(item, append);
                }
            }
        }
    }
}