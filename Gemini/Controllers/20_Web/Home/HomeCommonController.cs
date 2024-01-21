using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gemini.Controllers.Bussiness;
using Gemini.Models._02_Cms.U;
using Gemini.Models._03_Pos;
using Gemini.Models._05_Website;
using Gemini.Models._20_Web;

namespace Gemini.Controllers._20_Web.Home
{
    public class HomeCommonController : GeminiController
    {
        [ChildActionOnly]
        public ActionResult Header()
        {
            var username = GetUserInSession();
            ViewBag.CurrentUsername = string.IsNullOrWhiteSpace(username) ? "Đăng nhập" : username;

            return PartialView();
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            var username = GetUserInSession();
            ViewBag.CurrentUsername = string.IsNullOrWhiteSpace(username) ? "Đăng nhập" : username;

            return PartialView();
        }

        #region Produce List

        public ActionResult ProduceList(string guidCategory, string page, string sortBy)
        {
            try
            {
                int recordMax = 12;
                page = page ?? "page-1";
                string[] arrPage = page.Split('-');
                int numberPage = Convert.ToInt16(arrPage[1]) * recordMax;
                int numberPageActive = Convert.ToInt16(arrPage[1]);

                var models = new ProduceListModel();

                models.PosCategory = new PosCategoryModel();
                models.ListPosCategory = new List<PosCategoryModel>();
                models.ListPosProduceByCategory = new List<PosProduceModel>();

                models.ListPosCategory = (from cat in DataGemini.PosCategories
                                          where cat.Active
                                          select new PosCategoryModel
                                          {
                                              Guid = cat.Guid,
                                              Name = cat.Name,
                                              OrderBy = cat.OrderBy
                                          }).OrderBy(s => s.OrderBy).ToList();

                if (!string.IsNullOrEmpty(guidCategory))
                {
                    models.PosCategory = (from cat in DataGemini.PosCategories
                                          where cat.Active && cat.Guid.ToString().ToLower().Trim() == guidCategory
                                          select new PosCategoryModel
                                          {
                                              Guid = cat.Guid,
                                              Name = cat.Name
                                          }).FirstOrDefault();
                }

                IQueryable<PosProduceModel> listPosProduceByCategory;
                if (models.PosCategory != null && models.PosCategory.Guid != Guid.Empty)
                {
                    listPosProduceByCategory = from pp in DataGemini.PosProduces
                                               join pc in DataGemini.PosCategories on pp.GuidCategory equals pc.Guid
                                               where pp.Active && pc.Active && pp.GuidCategory == models.PosCategory.Guid
                                               select new PosProduceModel
                                               {
                                                   Guid = pp.Guid,
                                                   Name = pp.Name,
                                                   NameCategory = pc.Name,
                                                   Price = pp.Price,
                                                   Unit = pp.Unit,
                                                   ListGallery = (from fr in DataGemini.FProduceGalleries
                                                                  join im in DataGemini.UGalleries on fr.GuidGallery equals im.Guid
                                                                  where fr.GuidProduce == pp.Guid
                                                                  select new UGalleryModel
                                                                  {
                                                                      Image = im.Image,
                                                                      CreatedAt = im.CreatedAt
                                                                  }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                   CreatedAt = pp.CreatedAt
                                               };
                }
                else
                {
                    listPosProduceByCategory = from pp in DataGemini.PosProduces
                                               join pc in DataGemini.PosCategories on pp.GuidCategory equals pc.Guid
                                               where pp.Active && pc.Active
                                               select new PosProduceModel
                                               {
                                                   Guid = pp.Guid,
                                                   Name = pp.Name,
                                                   NameCategory = pc.Name,
                                                   Price = pp.Price,
                                                   Unit = pp.Unit,
                                                   ListGallery = (from fr in DataGemini.FProduceGalleries
                                                                  join im in DataGemini.UGalleries on fr.GuidGallery equals im.Guid
                                                                  where fr.GuidProduce == pp.Guid
                                                                  select new UGalleryModel
                                                                  {
                                                                      Image = im.Image,
                                                                      CreatedAt = im.CreatedAt
                                                                  }).OrderBy(x => x.CreatedAt).Take(1).ToList(),
                                                   CreatedAt = pp.CreatedAt
                                               };
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy)
                    {
                        case "newest":
                            listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                            break;
                        case "oldest":
                            listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.CreatedAt);
                            break;
                        case "a-z":
                            listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.Name);
                            break;
                        case "z-a":
                            listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.Name);
                            break;
                        case "priceH-L":
                            listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.Price);
                            break;
                        case "priceL-H":
                            listPosProduceByCategory = listPosProduceByCategory.OrderBy(x => x.Price);
                            break;
                        default:
                            listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                            break;
                    }
                }
                else
                {
                    listPosProduceByCategory = listPosProduceByCategory.OrderByDescending(x => x.CreatedAt);
                }

                //Sent data to view caculate
                ViewData["perpage"] = recordMax;
                ViewData["total"] = listPosProduceByCategory.Count();
                ViewData["pageActive"] = numberPageActive;

                //Check page start
                if (Convert.ToInt16(arrPage[1]) == 1)
                {
                    numberPage = 0;
                }
                else
                {
                    numberPage = numberPage - recordMax;
                }

                models.ListPosProduceByCategory = listPosProduceByCategory.Skip(numberPage).Take(recordMax).ToList();

                foreach (var item in models.ListPosProduceByCategory)
                {
                    var tmpLinkImg = item.ListGallery;
                    if (tmpLinkImg.Count == 0)
                    {
                        item.LinkImg0 = "/Content/Custom/empty-album.png";
                    }
                    else
                    {
                        item.LinkImg0 = tmpLinkImg[0].Image;
                    }
                }

                return View(models);
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

        #endregion

        #region Produce Detail

        public ActionResult ProduceDetail(string guidProduce)
        {
            try
            {
                if (!string.IsNullOrEmpty(guidProduce))
                {
                    var models = new ProduceDetailModel();

                    var posProduce = (from pp in DataGemini.PosProduces
                                      join pc in DataGemini.PosCategories on pp.GuidCategory equals pc.Guid
                                      where pp.Active && pp.Guid.ToString().ToLower().Trim() == guidProduce
                                      select new PosProduceModel
                                      {
                                          Guid = pp.Guid,
                                          Code = pp.Code,
                                          Name = pp.Name,
                                          ListGallery = (from fr in DataGemini.FProduceGalleries
                                                         join im in DataGemini.UGalleries on fr.GuidGallery equals im.Guid
                                                         where fr.GuidProduce == pp.Guid
                                                         select new UGalleryModel
                                                         {
                                                             Image = im.Image
                                                         }).ToList(),
                                          Price = pp.Price,
                                          Unit = pp.Unit,
                                          Note = pp.Note,
                                          Description = pp.Description,
                                          GuidCategory = pp.GuidCategory,
                                          GuidBatch = pp.GuidBatch,
                                          CreatedBy = pp.CreatedBy,
                                          NameCategory = pc.Name
                                      }).FirstOrDefault();

                    posProduce.PosCategory = DataGemini.PosCategories.FirstOrDefault(x => x.Guid == posProduce.GuidCategory);
                    posProduce.PosBatch = DataGemini.PosBatches.FirstOrDefault(x => x.Guid == posProduce.GuidBatch);
                    posProduce.PosProcess = DataGemini.PosProcesses.FirstOrDefault(x => x.GuidProduce == posProduce.Guid);

                    if (posProduce.PosProcess != null)
                    {
                        posProduce.PosProcessStatusName = PosProcess_Status.dicDesc[posProduce.PosProcess.Status.GetValueOrDefault(PosProcess_Status.Untested)];
                    }

                    models.PosProduce = posProduce;
                    if (models.PosProduce != null)
                    {
                        return View(models);
                    }
                    else
                    {
                        return Redirect("/Error/ErrorList");
                    }
                }

                return Redirect("/Error/ErrorList");
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

        #endregion

        #region About Us

        public ActionResult AboutUs()
        {
            try
            {
                var models = new AboutUsModel();

                models.AboutUs = (from ua in DataGemini.WUsAbouts
                                  where ua.Active
                                  select new WUsAboutModel
                                  {
                                      Guid = ua.Guid,
                                      Name = ua.Name,
                                      Description = ua.Description
                                  }).FirstOrDefault();

                if (models.AboutUs != null)
                {
                    return View(models);
                }
                else
                {
                    return Redirect("/Error/ErrorList");
                }
            }
            catch
            {
                return Redirect("/Error/ErrorList");
            }
        }

        #endregion
    }
}