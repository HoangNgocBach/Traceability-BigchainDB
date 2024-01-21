using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gemini.Controllers.Bussiness;
using Gemini.Models;
using Gemini.Models._02_Cms.U;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Constants = Gemini.Controllers.Bussiness.Constants;

namespace Gemini.Controllers._02_Cms.U
{
    [CustomAuthorizeAttribute]
    public class UGalleryController : GeminiController
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
            List<UGalleryModel> uGalleryModels = (from ug in DataGemini.UGalleries
                                                  join groups in DataGemini.UGroups on ug.GuidGroup equals groups.Guid into ps
                                                  from p in ps.DefaultIfEmpty()
                                                  select new UGalleryModel
                                                  {
                                                      Guid = ug.Guid,
                                                      Name = ug.Name,
                                                      NameGroup = p.Name,
                                                      Description = ug.Description,
                                                      GuidGroup = ug.GuidGroup,
                                                      Link = ug.Link,
                                                      Image = ug.Image,
                                                      Active = ug.Active,
                                                      Note = ug.Note,
                                                      CreatedAt = ug.CreatedAt,
                                                      CreatedBy = ug.CreatedBy,
                                                      UpdatedAt = ug.UpdatedAt,
                                                      UpdatedBy = ug.UpdatedBy,
                                                  }).OrderBy(s => s.Name).ToList();
            DataSourceResult result = uGalleryModels.ToDataSourceResult(request);
            return Json(result);
        }

        public ActionResult Create()
        {
            var uGallery = new UGallery();
            try
            {
                var viewModel = new UGalleryModel(uGallery) { IsUpdate = 0, Active = true };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Edit(Guid guid)
        {
            var uGallery = new UGallery();
            try
            {
                uGallery = DataGemini.UGalleries.FirstOrDefault(c => c.Guid == guid);
                var viewModel = new UGalleryModel(uGallery) { IsUpdate = 1 };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Delete(Guid guid)
        {
            var uGallery = new UGallery();
            try
            {
                uGallery = DataGemini.UGalleries.FirstOrDefault(c => c.Guid == guid);

                #region Delete Physical

                var url = uGallery.Image.Replace(ConstantsImage.Slash, "/");
                if (!string.IsNullOrEmpty(url))
                {
                    var folder = Path.GetDirectoryName(url);
                    var fileNameImage = Path.GetFileName(url);
                    fileNameImage = fileNameImage.Replace(ConstantsImage.Space, " ");
                    FileInfo fileImage = new FileInfo(Server.MapPath(folder) + "\\" + fileNameImage);
                    if (fileImage.Exists)
                    {
                        fileImage.Delete();
                    }

                    var fullPath = Path.Combine(Server.MapPath(folder));

                    string filesToDelete = @"*" + uGallery.Guid + "*" + ConstantsImage.FormatJpgImage;
                    try
                    {
                        string[] fileListImage = Directory.GetFiles(fullPath, filesToDelete);
                        foreach (var item in fileListImage)
                        {
                            FileInfo fileImages = new FileInfo(item);
                            if (fileImages.Exists)
                            {
                                fileImages.Delete();
                            }
                        }
                    }
                    catch
                    {

                    }
                }

                #endregion

                #region Delete FProduceGallery

                var fProduceGallery = DataGemini.FProduceGalleries.Where(x => x.GuidGallery == guid).ToList();
                if (fProduceGallery.Any())
                {
                    DataGemini.FProduceGalleries.RemoveRange(fProduceGallery);
                }

                #endregion

                DataGemini.UGalleries.Remove(uGallery);
                if (SaveData("U_GALLERY") && uGallery != null)
                {
                    DataReturn.ActiveCode = uGallery.Guid.ToString();
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
        public ActionResult Update(UGalleryModel viewModel)
        {
            var uGallery = new UGallery();
            try
            {
                viewModel.UpdatedBy = viewModel.CreatedBy = GetUserInSession();
                if (viewModel.IsUpdate == 0)
                {
                    viewModel.Setvalue(uGallery);
                    DataGemini.UGalleries.Add(uGallery);
                }
                else
                {
                    uGallery = DataGemini.UGalleries.FirstOrDefault(c => c.Guid == viewModel.Guid);
                    viewModel.Setvalue(uGallery);

                }
                if (SaveData("UGallery") && uGallery != null)
                {
                    DataReturn.ActiveCode = uGallery.Guid.ToString();
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                }
                DataGemini.SaveChanges();
                if (viewModel.IsUpdate == 0)
                {
                    string listSizeImg = DataGemini.UGroups.Where(x => x.Guid == uGallery.GuidGroup).Select(x => x.Note).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(listSizeImg))
                    {
                        var imgSizes = listSizeImg.Split(';');
                        foreach (var item in imgSizes)
                        {
                            if (!string.IsNullOrEmpty(item))
                            {
                                #region ScaleAndSave

                                var widthString = item.Split('x')[0];
                                var heightString = item.Split('x')[1];
                                int widthOfImage = Convert.ToInt32(widthString);
                                int heightOfImage = Convert.ToInt32(heightString);

                                var url = viewModel.UrlAnh.Replace(ConstantsImage.Slash, "/");
                                var folder = Path.GetDirectoryName(url);
                                var fileName = Path.GetFileName(url);
                                fileName = fileName.Replace(ConstantsImage.Space, " ");
                                var fileName2 = uGallery.Guid + "-" + widthString + "x" + heightString + ConstantsImage.FormatJpgImage;
                                var imageScaled = ScaleImage(Path.Combine(Server.MapPath(folder), fileName), widthOfImage, heightOfImage);
                                var physicalPath = Path.Combine(Server.MapPath(folder), fileName2);
                                physicalPath = physicalPath.Replace("Images\\san-pham", "Thumbnail");
                                imageScaled.Save(physicalPath);

                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    DataGemini.UGalleries.Remove(uGallery);
                    DataGemini.SaveChanges();
                }
                HandleError(ex);
            }
            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Copy(Guid guid)
        {
            var uGallery = new UGallery();
            var clone = new UGallery();
            try
            {
                uGallery = DataGemini.UGalleries.FirstOrDefault(c => c.Guid == guid);
                #region Copy
                DataGemini.UGalleries.Add(clone);
                //Copy values from source to clone
                var sourceValues = DataGemini.Entry(uGallery).CurrentValues;
                DataGemini.Entry(clone).CurrentValues.SetValues(sourceValues);
                //Change values of the copied entity
                clone.Name = clone.Name + " - Copy";
                clone.Guid = Guid.NewGuid();
                clone.UpdatedAt = clone.CreatedAt = DateTime.Now;
                clone.UpdatedBy = clone.CreatedBy = GetUserInSession();
                if (SaveData("U_NGALLERY"))
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
                DataGemini.UGalleries.Remove(clone);
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [AllowAnonymous]
        public ActionResult ReadProduce([DataSourceRequest] DataSourceRequest request, string guid)
        {
            List<UGalleryModel> uGalleryModel = (from ug in DataGemini.UGalleries
                                                 join fpg in DataGemini.FProduceGalleries on ug.Guid equals fpg.GuidGallery
                                                 join pp in DataGemini.PosProduces on fpg.GuidProduce equals pp.Guid
                                                 join groups in DataGemini.UGroups on ug.GuidGroup equals groups.Guid
                                                 where pp.Guid.ToString() == guid && groups.Name == "PosProduce"
                                                 select new UGalleryModel
                                                 {
                                                     Guid = ug.Guid,
                                                     Name = ug.Name,
                                                     NameGroup = groups.Name,
                                                     Description = ug.Description,
                                                     GuidGroup = ug.GuidGroup,
                                                     Link = ug.Link,
                                                     Image = ug.Image,
                                                     Active = ug.Active,
                                                     Note = ug.Note,
                                                     IsProduce = true,
                                                     CreatedAt = ug.CreatedAt,
                                                     CreatedBy = ug.CreatedBy,
                                                     UpdatedAt = ug.UpdatedAt,
                                                     UpdatedBy = ug.UpdatedBy,
                                                 }).OrderByDescending(s => s.CreatedAt).ToList();

            DataSourceResult result = uGalleryModel.ToDataSourceResult(request);
            return Json(result);
        }
    }
}