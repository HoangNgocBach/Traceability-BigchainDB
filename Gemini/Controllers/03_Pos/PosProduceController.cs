using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gemini.Controllers.Bussiness;
using Gemini.Models;
using Gemini.Models._02_Cms.U;
using Gemini.Models._03_Pos;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Constants = Gemini.Controllers.Bussiness.Constants;
using System.Net.Mail;
using Gemini.Models._01_Hethong;
using System.Windows.Forms;

namespace Gemini.Controllers._03_Pos
{
    [CustomAuthorizeAttribute]
    public class PosProduceController : GeminiController
    {
        #region Main
        [Authorize]
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
            var user = GetSettingUser();

            List<PosProduceModel> posProduceModel = (from pp in DataGemini.PosProduces
                                                     join pc in DataGemini.PosCategories on pp.GuidCategory equals pc.Guid
                                                     where pp.CreatedBy == user.Username || user.IsAdmin
                                                     select new PosProduceModel
                                                     {
                                                         Guid = pp.Guid,
                                                         Code = pp.Code,
                                                         Name = pp.Name,
                                                         NameCategory = pc.Name,
                                                         Active = pp.Active,
                                                         Note = pp.Note,
                                                         CreatedAt = pp.CreatedAt,
                                                         CreatedBy = pp.CreatedBy,
                                                         UpdatedAt = pp.UpdatedAt,
                                                         UpdatedBy = pp.UpdatedBy,
                                                         Description = pp.Description,
                                                         Price = pp.Price,
                                                         Unit = pp.Unit,
                                                         GuidCategory = pp.GuidCategory,
                                                         GuidBatch = pp.GuidBatch,
                                                         ListGallery = (from fr in DataGemini.FProduceGalleries
                                                                        join im in DataGemini.UGalleries on fr.GuidGallery equals im.Guid
                                                                        where fr.GuidProduce == pp.Guid
                                                                        select new UGalleryModel
                                                                        {
                                                                            Image = im.Image
                                                                        }).ToList()
                                                     }).OrderByDescending(s => s.CreatedAt).ToList();

            foreach (var item in posProduceModel)
            {
                var tmpLinkImg = item.ListGallery.ToList();
                if (tmpLinkImg.Count == 0)
                {
                    item.LinkImg0 = "/Content/Custom/empty-album.png";
                    item.LinkImg1 = "/Content/Custom/empty-album.png";
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                if (tmpLinkImg.Count == 1)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = "/Content/Custom/empty-album.png";
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                else if (tmpLinkImg.Count == 2)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = tmpLinkImg[1].Image;
                    item.LinkImg2 = "/Content/Custom/empty-album.png";
                }
                else if (tmpLinkImg.Count > 2)
                {
                    item.LinkImg0 = tmpLinkImg[0].Image;
                    item.LinkImg1 = tmpLinkImg[1].Image;
                    item.LinkImg2 = tmpLinkImg[2].Image;
                }
            }

            DataSourceResult result = posProduceModel.ToDataSourceResult(request);
            return Json(result);
        }

        public ActionResult Create()
        {
            var user = GetSettingUser();
            ViewBag.IsAdmin = user.IsAdmin;

            var posProduce = new PosProduce();
            try
            {
                var tmp = DataGemini.SReplaceCodes.ToList();
                posProduce = new PosProduce();
                var viewModel = new PosProduceModel(posProduce) { IsUpdate = 0, Active = true, ReplaceCode = tmp };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }

        }

        [AllowAnonymous]
        public ActionResult Display(Guid guid)
        {
            var posProduce = new PosProduce();
            try
            {
                var tmp = DataGemini.SReplaceCodes.ToList();
                posProduce = DataGemini.PosProduces.FirstOrDefault(c => c.Guid == guid);
                var viewModel = new PosProduceModel(posProduce) { ReplaceCode = tmp };

                viewModel.PosCategory = DataGemini.PosCategories.FirstOrDefault(x => x.Guid == viewModel.GuidCategory);
                viewModel.PosBatch = DataGemini.PosBatches.FirstOrDefault(x => x.Guid == viewModel.GuidBatch);
                viewModel.PosProcess = DataGemini.PosProcesses.FirstOrDefault(x => x.GuidProduce == viewModel.Guid);

                if (viewModel.PosProcess != null)
                {
                    var lstUser = DataGemini.SUsers.Where(x => x.Guid == viewModel.PosProcess.ManufacturingBy
                                                                || x.Guid == viewModel.PosProcess.TransportBy
                                                                || x.Guid == viewModel.PosProcess.DistributeBy).ToList();

                    viewModel.ManufacturingBy = lstUser.FirstOrDefault(x => x.Guid == viewModel.PosProcess.ManufacturingBy);
                    viewModel.TransportBy = lstUser.FirstOrDefault(x => x.Guid == viewModel.PosProcess.TransportBy);
                    viewModel.DistributeBy = lstUser.FirstOrDefault(x => x.Guid == viewModel.PosProcess.DistributeBy);

                    viewModel.PosProcessStatusName = PosProcess_Status.dicDesc[viewModel.PosProcess.Status.GetValueOrDefault(PosProcess_Status.Untested)];
                }

                viewModel.ListGallery = (from fr in DataGemini.FProduceGalleries
                                         join im in DataGemini.UGalleries on fr.GuidGallery equals im.Guid
                                         where fr.GuidProduce == guid
                                         select new UGalleryModel
                                         {
                                             Image = im.Image
                                         }).ToList();

                ViewBag.Title = "Quản trị hệ thống";
                var currentUsername = GetUserInSession();
                ViewBag.CurrentUsername = !string.IsNullOrWhiteSpace(currentUsername) ? currentUsername : "Login";

                return PartialView("Display", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Edit(Guid guid)
        {
            var user = GetSettingUser();
            ViewBag.IsAdmin = user.IsAdmin;

            var posProduce = new PosProduce();
            try
            {
                var tmp = DataGemini.SReplaceCodes.ToList();
                posProduce = DataGemini.PosProduces.FirstOrDefault(c => c.Guid == guid);
                var viewModel = new PosProduceModel(posProduce) { IsUpdate = 1, ReplaceCode = tmp };
                return PartialView("Edit", viewModel);
            }
            catch (Exception)
            {
                return Redirect("/Error/ErrorList");
            }
        }

        public ActionResult Copy(Guid guid)
        {
            var posProduce = new PosProduce();
            var clone = new PosProduce();
            try
            {
                posProduce = DataGemini.PosProduces.FirstOrDefault(c => c.Guid == guid);
                #region Copy

                DataGemini.PosProduces.Add(clone);
                //Copy values from source to clone
                var sourceValues = DataGemini.Entry(posProduce).CurrentValues;
                DataGemini.Entry(clone).CurrentValues.SetValues(sourceValues);
                //Change values of the copied entity
                clone.Code = clone.Code + " - Copy";
                clone.Guid = Guid.NewGuid();
                clone.CreatedAt = clone.UpdatedAt = DateTime.Now;
                clone.UpdatedBy = clone.CreatedBy = GetUserInSession();
                if (SaveData("PosProduce"))
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
                DataGemini.PosProduces.Remove(clone);
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(Guid guid)
        {
            var posProduce = new PosProduce();
            try
            {
                posProduce = DataGemini.PosProduces.FirstOrDefault(c => c.Guid == guid);
                DataGemini.PosProduces.Remove(posProduce);
                if (SaveData("PosProduce") && posProduce != null)
                {
                    DataReturn.ActiveCode = posProduce.Guid.ToString();
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
        public ActionResult Update(PosProduceModel viewModel)
        {
            var posProduce = new PosProduce();
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
                        viewModel.Setvalue(posProduce);
                        DataGemini.PosProduces.Add(posProduce);
                    }
                    else if (viewModel.IsUpdate == 1)
                    {
                        posProduce = DataGemini.PosProduces.FirstOrDefault(c => c.Guid == viewModel.Guid);
                        viewModel.Setvalue(posProduce);
                    }
                    posProduce.SeoFriendUrl = posProduce.Guid.ToString();

                    if (SaveData("PosProduce") && posProduce != null)
                    {
                        DataReturn.ActiveCode = posProduce.Guid.ToString();
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
                    DataGemini.PosProduces.Remove(posProduce);
                }
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> ValidateDuplicate(PosProduceModel viewModel)
        {
            List<string> lstErrMsg = new List<string>();

            var lstProduce = DataGemini.PosProduces.Where(c => c.Code.Equals(viewModel.Code, StringComparison.OrdinalIgnoreCase) && c.Guid != viewModel.Guid);

            if (lstProduce.Count() > 0)
            {
                lstErrMsg.Add("Mã sản phẩm đã tồn tại!");
            }

            return lstErrMsg;
        }
        #endregion

        public ActionResult DeleteFProduceGallery(string guidProduce, string guidGallery)
        {
            try
            {
                var listGuidGallery = guidGallery.Split(';').ToList();
                var listFRemove = DataGemini.FProduceGalleries.Where(x => listGuidGallery.Contains(x.GuidGallery.ToString())).ToList();
                DataGemini.FProduceGalleries.RemoveRange(listFRemove);

                var listGRemove = DataGemini.UGalleries.Where(x => listGuidGallery.Contains(x.Guid.ToString())).ToList();
                DataGemini.UGalleries.RemoveRange(listGRemove);

                DataGemini.SaveChanges();

                DataReturn.ActiveCode = guidProduce;
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveFProduceGallery(string guidProduce, string guidGallery)
        {
            try
            {
                var listRemove = DataGemini.FProduceGalleries.Where(x => x.GuidProduce == new Guid(guidProduce)).ToList();
                DataGemini.FProduceGalleries.RemoveRange(listRemove);
                var listGuidGallery = guidGallery.Split(';');
                foreach (var item in listGuidGallery)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        FProduceGallery fProduceGallery = new FProduceGallery();
                        fProduceGallery.Guid = Guid.NewGuid();
                        fProduceGallery.CreatedAt = DateTime.Now;
                        fProduceGallery.CreatedBy = GetUserInSession();
                        fProduceGallery.GuidGallery = new Guid(item);
                        fProduceGallery.GuidProduce = new Guid(guidProduce);
                        DataGemini.FProduceGalleries.Add(fProduceGallery);

                    }
                }
                DataGemini.SaveChanges();

                DataReturn.ActiveCode = guidProduce;
                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Save(HttpPostedFileBase File_path1, string guidProduce)
        {
            var msg = "";
            var nameFile = Path.GetFileName(File_path1.FileName);
            guidProduce = guidProduce ?? String.Empty;
            if (File_path1 != null)
            {
                string tmp = Server.MapPath("~/Content/UserFiles/Images/Product/" + guidProduce + "/");
                if (System.IO.File.Exists(Path.Combine(tmp, nameFile)))
                {
                    System.IO.File.Delete(Path.Combine(tmp, nameFile));
                }
                var physicalPath = Path.Combine(Server.MapPath("~/Content/UserFiles/Images/Product/" + guidProduce + "/"), nameFile);

                VerifyDir(physicalPath);
                WriteFileFromStream(File_path1.InputStream, physicalPath);
                msg = nameFile;
                SaveGallery(nameFile, guidProduce);
            }
            return Json(new { status = "" + msg + "" }, "text/plain");
        }

        public static void VerifyDir(string path)
        {
            try
            {
                var list = path.Split(new string[] { "\\" }, StringSplitOptions.None);
                var directory = path.Replace("\\" + list[list.Count() - 1], "");
                DirectoryInfo dir = new DirectoryInfo(directory);
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch { }
        }

        public static void WriteFileFromStream(Stream stream, string toFile)
        {
            using (FileStream fileToSave = new FileStream(toFile, FileMode.Create))
            {
                stream.CopyTo(fileToSave);
            }
        }

        public void SaveGallery(string fileName1, string guidProduce)
        {
            try
            {
                Guid guidGroup = Guid.Empty;
                var group = DataGemini.UGroups.FirstOrDefault(s => s.Name == "PosProduce" && s.Type == "GALLERY_GROUP");
                if (group == null)
                {
                    UGroup posGroup = new UGroup();
                    guidGroup = posGroup.Guid = Guid.NewGuid();
                    posGroup.Name = "PosProduce";
                    posGroup.Active = true;
                    posGroup.CreatedAt = posGroup.UpdatedAt = DateTime.Now;
                    posGroup.CreatedBy = posGroup.UpdatedBy = GetUserInSession();
                    posGroup.Type = "GALLERY_GROUP";
                    DataGemini.UGroups.Add(posGroup);
                    DataGemini.SaveChanges();
                }
                else
                {
                    guidGroup = group.Guid;
                }
                UGallery uGallery = new UGallery();
                uGallery.Name = fileName1;
                uGallery.Guid = Guid.NewGuid();
                uGallery.GuidGroup = guidGroup;
                uGallery.Active = true;
                uGallery.CreatedAt = uGallery.UpdatedAt = DateTime.Now;
                uGallery.CreatedBy = uGallery.UpdatedBy = GetUserInSession();
                uGallery.Image = "/Content/UserFiles/Images/Product/" + guidProduce + "/" + fileName1;
                DataGemini.UGalleries.Add(uGallery);

                FProduceGallery fProduceGallery = new FProduceGallery();
                fProduceGallery.Guid = Guid.NewGuid();
                fProduceGallery.CreatedAt = DateTime.Now;
                fProduceGallery.CreatedBy = GetUserInSession();
                fProduceGallery.GuidGallery = uGallery.Guid;
                fProduceGallery.GuidProduce = new Guid(guidProduce);
                DataGemini.FProduceGalleries.Add(fProduceGallery);

                DataGemini.SaveChanges();
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }
    }

    public class MemoryPostedFile : HttpPostedFileBase
    {
        private readonly byte[] fileBytes;

        public MemoryPostedFile(byte[] fileBytes, string fileName)
        {
            this.fileBytes = fileBytes;
            this.FileName = fileName;
            this.InputStream = new MemoryStream(fileBytes);
        }

        public override int ContentLength => fileBytes.Length;

        public override string FileName { get; }

        public override Stream InputStream { get; }
    }
}