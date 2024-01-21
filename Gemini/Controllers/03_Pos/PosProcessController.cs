using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Gemini.Controllers.Bussiness;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Gemini.Models;
using Constants = Gemini.Controllers.Bussiness.Constants;
using Gemini.Models._03_Pos;
using RestSharp;
using System.Web;
using SINNOVA.Core;

namespace Gemini.Controllers._03_Pos
{
    [CustomAuthorizeAttribute]
    public class PosProcessController : GeminiController
    {
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        /// <summary>
        /// List view grid
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GetSettingUser();
            return View();
        }

        public ActionResult Read([DataSourceRequest] DataSourceRequest request)
        {
            var user = GetSettingUser();

            List<PosProcessModel> posProcessModel = (from pp in DataGemini.PosProcesses
                                                     join pprod in DataGemini.PosProduces on pp.GuidProduce equals pprod.Guid
                                                     where pp.ManufacturingBy == user.Guid
                                                           || pp.TransportBy == user.Guid
                                                           || pp.DistributeBy == user.Guid
                                                           || (user.IsAccreditation && pp.DistributeStatus == PosProcess_DistributeStatus.Done)
                                                           || user.IsAdmin
                                                     select new PosProcessModel
                                                     {
                                                         Guid = pp.Guid,
                                                         ManufacturingBy = pp.ManufacturingBy,
                                                         ManufacturingStartAt = pp.ManufacturingStartAt,
                                                         ManufacturingEndAt = pp.ManufacturingEndAt,
                                                         TransportBy = pp.TransportBy,
                                                         TransportStartAt = pp.TransportStartAt,
                                                         TransportEndAt = pp.TransportEndAt,
                                                         DistributeBy = pp.DistributeBy,
                                                         DistributeStartAt = pp.DistributeStartAt,
                                                         DistributeEndAt = pp.DistributeEndAt,
                                                         Signature = pp.Signature,
                                                         SignAt = pp.SignAt,
                                                         ManufacturingStatus = pp.ManufacturingStatus,
                                                         TransportStatus = pp.TransportStatus,
                                                         DistributeStatus = pp.DistributeStatus,
                                                         Status = pp.Status,
                                                         BigchainDBTransactionId = pp.BigchainDBTransactionId,
                                                         CreatedAt = pp.CreatedAt,
                                                         CreatedBy = pp.CreatedBy,
                                                         UpdatedAt = pp.UpdatedAt,
                                                         UpdatedBy = pp.UpdatedBy,

                                                         GuidProduce = pp.GuidProduce,
                                                         ProduceCode = pprod.Code,
                                                         ProduceName = pprod.Name,
                                                         ProduceGuidBatch = pprod.GuidBatch,
                                                         ProduceGuidCategory = pprod.GuidCategory
                                                     }).OrderByDescending(s => s.CreatedAt).ToList();

            posProcessModel.ForEach(x =>
            {
                x.StatusName = x.Status != null ? PosProcess_Status.dicDesc[x.Status.Value] : String.Empty;
            });

            DataSourceResult result = posProcessModel.ToDataSourceResult(request);
            return Json(result);
        }

        public ActionResult Create()
        {
            try
            {
                var user = GetSettingUser();
                ViewBag.IsManufacturing = user.IsManufacturing;
                ViewBag.IsTransport = user.IsTransport;
                ViewBag.IsDistribute = user.IsDistribute;

                var posProcess = new PosProcess();
                var viewModel = new PosProcessModel(posProcess) { IsUpdate = 0 };
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
                var user = GetSettingUser();
                ViewBag.IsManufacturing = user.IsManufacturing;
                ViewBag.IsTransport = user.IsTransport;
                ViewBag.IsDistribute = user.IsDistribute;

                var posProcess = new PosProcess();
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);

                var viewModel = new PosProcessModel(posProcess) { IsUpdate = 1 };
                viewModel.ProduceName = DataGemini.PosProduces.FirstOrDefault(x => x.Guid == viewModel.GuidProduce)?.Name;

                if (viewModel.ManufacturingStartAt != null)
                    viewModel.ManufacturingStartAtString = viewModel.ManufacturingStartAt.ToString();

                if (viewModel.ManufacturingEndAt != null)
                    viewModel.ManufacturingEndAtString = viewModel.ManufacturingEndAt.ToString();

                if (viewModel.TransportStartAt != null)
                    viewModel.TransportStartAtString = viewModel.TransportStartAt.ToString();

                if (viewModel.TransportEndAt != null)
                    viewModel.TransportEndAtString = viewModel.TransportEndAt.ToString();

                if (viewModel.DistributeStartAt != null)
                    viewModel.DistributeStartAtString = viewModel.DistributeStartAt.ToString();

                if (viewModel.DistributeEndAt != null)
                    viewModel.DistributeEndAtString = viewModel.DistributeEndAt.ToString();

                if (viewModel.ManufacturingStatus != null)
                    viewModel.ManufacturingStatusName = PosProcess_ManufacturingStatus.dicDesc[viewModel.ManufacturingStatus.Value];

                if (viewModel.TransportStatus != null)
                    viewModel.TransportStatusName = PosProcess_TransportStatus.dicDesc[viewModel.TransportStatus.Value];

                if (viewModel.DistributeStatus != null)
                    viewModel.DistributeStatusName = PosProcess_DistributeStatus.dicDesc[viewModel.DistributeStatus.Value];

                if (viewModel.TransportBy != null || viewModel.DistributeBy != null)
                {
                    var sUser = DataGemini.SUsers.Where(x => (viewModel.TransportBy != null && viewModel.TransportBy != Guid.Empty && x.Guid == viewModel.TransportBy)
                                                             || (viewModel.DistributeBy != null && viewModel.DistributeBy != Guid.Empty && x.Guid == viewModel.DistributeBy));

                    viewModel.TransportByName = sUser.FirstOrDefault(x => x.Guid == viewModel.TransportBy)?.Username;
                    viewModel.DistributeByName = sUser.FirstOrDefault(x => x.Guid == viewModel.DistributeBy)?.Username;
                }

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
                var posProcess = new PosProcess();
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);
                var lstErrMsg = ValidateStatus_Delete(posProcess);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    DataGemini.PosProcesses.Remove(posProcess);
                    if (SaveData("PosProcess") && posProcess != null)
                    {
                        DataReturn.ActiveCode = posProcess.Guid.ToString();
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    }
                    else
                    {
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.BadRequest);
                        DataReturn.MessagError = Constants.CannotDelete + " Date : " + DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> ValidateStatus_Delete(PosProcess posProcess)
        {
            List<string> lstErrMsg = new List<string>();

            if (posProcess.TransportStatus != null || posProcess.DistributeStatus != null)
            {
                lstErrMsg.Add("Quy trình đang thực hiện, không thể xóa!");
            }

            if (posProcess.Status >= PosProcess_Status.Tested)
            {
                lstErrMsg.Add("Quy trình đã qua kiểm định, không thể xóa!");
            }

            return lstErrMsg;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Update(PosProcessModel viewModel)
        {
            var posProcess = new PosProcess();
            try
            {
                var lstErrMsg = Validate_Update(viewModel);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    var user = GetSettingUser();
                    var prod = DataGemini.PosProduces.FirstOrDefault(x => x.Guid == viewModel.GuidProduce);
                    var trans = DataGemini.SUsers.FirstOrDefault(x => x.Guid == viewModel.TransportBy);
                    var dis = DataGemini.SUsers.FirstOrDefault(x => x.Guid == viewModel.DistributeBy);
                    viewModel.UpdatedBy = viewModel.CreatedBy = user.Username;
                    if (viewModel.IsUpdate == 0)
                    {
                        if (user.IsManufacturing)
                        {
                            viewModel.ManufacturingBy = user.Guid;

                            if (viewModel.ManufacturingStatus != PosProcess_ManufacturingStatus.Done)
                            {
                                viewModel.TransportBy = null;
                                viewModel.DistributeBy = null;
                            }

                            #region History
                            viewModel.History += String.Format(@"[{1}] Nhà sản xuất - {0} tạo mới:", user.Username, DateTime.Now);

                            if (viewModel.GuidProduce != null)
                                viewModel.History += String.Format(@"<br>- Sản phẩm: {0}", prod?.Name);

                            if (viewModel.ManufacturingStartAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian bắt đầu sản xuất: {0}", viewModel.ManufacturingStartAt.ToString());

                            if (viewModel.ManufacturingEndAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian kết thúc sản xuất: {0}", viewModel.ManufacturingEndAt.ToString());

                            if (viewModel.ManufacturingStatus != null)
                                viewModel.History += String.Format(@"<br>- Trạng thái sản xuất: {0}", PosProcess_ManufacturingStatus.dicDesc[viewModel.ManufacturingStatus.GetValueOrDefault(PosProcess_ManufacturingStatus.Inprogress)]);

                            if (viewModel.TransportBy != null)
                                viewModel.History += String.Format(@"<br>- Nhà vận chuyển: {0}", trans?.Username);

                            if (viewModel.DistributeBy != null)
                                viewModel.History += String.Format(@"<br>- Nhà phân phối: {0}", dis?.Username);
                            #endregion
                        }

                        viewModel.Setvalue(posProcess);
                        DataGemini.PosProcesses.Add(posProcess);
                    }
                    else
                    {
                        posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == viewModel.Guid);
                        viewModel.Signature = posProcess.Signature;
                        viewModel.SignAt = posProcess.SignAt;

                        #region History
                        viewModel.History = posProcess.History;
                        if (user.IsManufacturing)
                        {
                            viewModel.History += String.Format(@"<br><br>[{1}] Nhà sản xuất - {0} cập nhật:", user.Username, DateTime.Now);

                            if (viewModel.GuidProduce != null)
                                viewModel.History += String.Format(@"<br>- Sản phẩm: {0}", prod?.Name);

                            if (viewModel.ManufacturingStartAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian bắt đầu sản xuất: {0}", viewModel.ManufacturingStartAt.ToString());

                            if (viewModel.ManufacturingEndAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian kết thúc sản xuất: {0}", viewModel.ManufacturingEndAt.ToString());

                            if (viewModel.ManufacturingStatus != null)
                                viewModel.History += String.Format(@"<br>- Trạng thái sản xuất: {0}", PosProcess_ManufacturingStatus.dicDesc[viewModel.ManufacturingStatus.GetValueOrDefault(PosProcess_ManufacturingStatus.Inprogress)]);

                            if (viewModel.TransportBy != null)
                                viewModel.History += String.Format(@"<br>- Nhà vận chuyển: {0}", trans?.Username);

                            if (viewModel.DistributeBy != null)
                                viewModel.History += String.Format(@"<br>- Nhà phân phối: {0}", dis?.Username);
                        }

                        if (user.IsTransport)
                        {
                            viewModel.History += String.Format(@"<br><br>[{1}] Nhà vận chuyển - {0} cập nhật:", user.Username, DateTime.Now);

                            if (viewModel.TransportStartAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian bắt đầu vận chuyển: {0}", viewModel.TransportStartAt.ToString());

                            if (viewModel.TransportEndAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian kết thúc vận chuyển: {0}", viewModel.TransportEndAt.ToString());

                            if (viewModel.TransportStatus != null)
                                viewModel.History += String.Format(@"<br>- Trạng thái vận chuyển: {0}", PosProcess_TransportStatus.dicDesc[viewModel.TransportStatus.GetValueOrDefault(PosProcess_TransportStatus.Inprogress)]);
                        }

                        if (user.IsDistribute)
                        {
                            viewModel.History += String.Format(@"<br><br>[{1}] Nhà phân phối - {0} cập nhật:", user.Username, DateTime.Now);

                            if (viewModel.DistributeStartAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian bắt đầu phân phối: {0}", viewModel.DistributeStartAt.ToString());

                            if (viewModel.DistributeEndAt != null)
                                viewModel.History += String.Format(@"<br>- Thời gian kết thúc phân phối: {0}", viewModel.DistributeEndAt.ToString());

                            if (viewModel.DistributeStatus != null)
                                viewModel.History += String.Format(@"<br>- Trạng thái phân phối: {0}", PosProcess_DistributeStatus.dicDesc[viewModel.DistributeStatus.GetValueOrDefault(PosProcess_DistributeStatus.Inprogress)]);
                        }
                        #endregion

                        viewModel.Status = viewModel.DistributeStatus == PosProcess_DistributeStatus.Done ? PosProcess_Status.Untested : (int?)null;

                        viewModel.Setvalue(posProcess);
                    }
                    if (SaveData("PosProcess") && posProcess != null)
                    {
                        DataReturn.ActiveCode = posProcess.Guid.ToString();
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                    }
                    else
                    {
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.BadRequest);
                        DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                if (viewModel.IsUpdate == 0)
                {
                    DataGemini.PosProcesses.Remove(posProcess);
                }
                HandleError(ex);
            }
            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> Validate_Update(PosProcessModel viewModel)
        {
            List<string> lstErrMsg = new List<string>();

            if (viewModel.Status >= PosProcess_Status.Tested)
            {
                lstErrMsg.Add("Quy trình đã qua bước kiểm định, không thể sửa!");
            }

            var lstProcess = DataGemini.PosProcesses.Where(c => c.GuidProduce == viewModel.GuidProduce && c.Guid != viewModel.Guid && c.Status != PosProcess_Status.Reject && c.Status != PosProcess_Status.Changed);

            if (lstProcess.Count() > 0)
            {
                lstErrMsg.Add("Sản phẩm đã tồn tại quy trình!");
            }

            return lstErrMsg;
        }

        public ActionResult Approve(Guid guid)
        {
            var posProcess = new PosProcess();
            try
            {
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);
                var lstErrMsg = Validate_Approval(posProcess);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    var rsaHelper = new RSAHelper();

                    string privateKeyXML = string.Empty;
                    string publicKeyXML = string.Empty;
                    rsaHelper.GenerateKeys(guid.ToString().ToLower(), out privateKeyXML, out publicKeyXML);

                    var hashMessage = rsaHelper.ConvertStringToHash(posProcess.History);
                    var signature = rsaHelper.SignData(hashMessage, privateKeyXML);

                    posProcess.PublicKey = publicKeyXML;
                    posProcess.Signature = signature;
                    posProcess.UpdatedBy = GetUserInSession();
                    posProcess.SignAt = posProcess.UpdatedAt = DateTime.Now;
                    posProcess.Status = PosProcess_Status.Tested;

                    if (SaveData("PosProcess") && posProcess != null)
                    {
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
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Reject(Guid guid)
        {
            var posProcess = new PosProcess();
            try
            {
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);
                var lstErrMsg = Validate_Approval(posProcess);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    posProcess.UpdatedBy = GetUserInSession();
                    posProcess.UpdatedAt = DateTime.Now;
                    posProcess.Status = PosProcess_Status.Reject;

                    if (SaveData("PosProcess") && posProcess != null)
                    {
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
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> Validate_Approval(PosProcess posProcess)
        {
            List<string> lstErrMsg = new List<string>();

            if (posProcess.Status >= PosProcess_Status.Tested)
            {
                lstErrMsg.Add("Quy trình đã qua bước kiểm định, không thể sửa!");
            }

            return lstErrMsg;
        }

        public ActionResult ValidateBefore_SaveBigchainTransactionId(Guid guid)
        {
            var posProcess = new PosProcess();
            try
            {
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);

                var lstErrMsg = ValidateBefore_SaveBigchainTransactionId(posProcess);

                if (lstErrMsg.Count > 0)
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = String.Join("<br/>", lstErrMsg);
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        private List<string> ValidateBefore_SaveBigchainTransactionId(PosProcess posProcess)
        {
            List<string> lstErrMsg = new List<string>();

            if (posProcess.Status == PosProcess_Status.Untested)
            {
                lstErrMsg.Add("Quy trình chưa được kiểm định!");
            }
            else if (posProcess.Status == PosProcess_Status.Reject)
            {
                lstErrMsg.Add("Quy trình kiểm định thất bại!");
            }
            else if (posProcess.Status == PosProcess_Status.UploadedBigchainDB || posProcess.Status == PosProcess_Status.Changed)
            {
                lstErrMsg.Add("Quy trình đã được upload lên BigchainDB!");
            }

            return lstErrMsg;
        }

        public ActionResult SaveBigchainTransactionId(string id, Guid guid)
        {
            var posProcess = new PosProcess();
            try
            {
                posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);

                posProcess.Status = PosProcess_Status.UploadedBigchainDB;
                posProcess.BigchainDBTransactionId = id;
                posProcess.UpdatedBy = GetUserInSession();
                posProcess.UpdatedAt = DateTime.Now;

                if (SaveData("PosProcess") && posProcess != null)
                {
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
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public ActionResult ReAuthenBigchainDB(Guid guid, string transId)
        {
            var posProcess = new PosProcess();
            var posProcessBigchainDB = new PosProcess();
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                var client = new RestClient($"https://test.ipdb.io/api/v1/transactions/{transId}");
                client.Timeout = -1;

                var request = new RestRequest(Method.GET);
                var response = client.Execute<ResponseBigchainDB>(request);
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var posProcessBigchainDBObj = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseBigchainDB>(response.Content);
                    if (posProcessBigchainDBObj != null && posProcessBigchainDBObj.asset != null && posProcessBigchainDBObj.asset.data != null && !string.IsNullOrWhiteSpace(posProcessBigchainDBObj.asset.data.process))
                    {
                        posProcessBigchainDB = Newtonsoft.Json.JsonConvert.DeserializeObject<PosProcess>(posProcessBigchainDBObj.asset.data.process);

                        posProcess = DataGemini.PosProcesses.FirstOrDefault(c => c.Guid == guid);

                        var lstErrMsg = Validate_ReAuthenBigchainDB(posProcess, posProcessBigchainDB);

                        if (lstErrMsg.Count > 0)
                        {
                            posProcess.Status = PosProcess_Status.Changed;
                            posProcess.UpdatedAt = DateTime.Now;

                            if (SaveData("PosProcess") && posProcess != null)
                            {
                                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                                DataReturn.MessagError = String.Join("<br/><br/>", lstErrMsg);
                            }
                            else
                            {
                                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                                DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                            }
                        }
                        else
                        {
                            posProcess.Status = PosProcess_Status.UploadedBigchainDB;
                            posProcess.UpdatedAt = DateTime.Now;

                            if (SaveData("PosProcess") && posProcess != null)
                            {
                                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.OK);
                            }
                            else
                            {
                                DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                                DataReturn.MessagError = Constants.CannotUpdate + " Date : " + DateTime.Now;
                            }
                        }
                    }
                    else
                    {
                        DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                        DataReturn.MessagError = "Có lỗi xảy ra khi xác thực !";

                        Log.Error("ReAuthenBigchainDB: " + response.Content);
                    }
                }
                else
                {
                    DataReturn.StatusCode = Convert.ToInt16(HttpStatusCode.Conflict);
                    DataReturn.MessagError = "Có lỗi xảy ra khi xác thực !";

                    Log.Error("ReAuthenBigchainDB: " + Newtonsoft.Json.JsonConvert.SerializeObject(response));
                }
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return Json(DataReturn, JsonRequestBehavior.AllowGet);
        }

        public List<string> Validate_ReAuthenBigchainDB(PosProcess posProcess, PosProcess posProcessBigchainDB)
        {
            var offset = DateTimeOffset.Now.Offset;

            List<string> lstErrMsg = new List<string>();

            if (posProcess.Guid != posProcessBigchainDB.Guid)
                lstErrMsg.Add("● Bản ghi đã bị thay đổi");

            if (posProcess.ManufacturingBy != posProcessBigchainDB.ManufacturingBy)
                lstErrMsg.Add("● Quy trình sản xuất - Nhà sản xuất đã bị thay đổi");

            if (posProcess.ManufacturingStartAt != null && posProcessBigchainDB.ManufacturingStartAt != null
                && DateTime.Compare(posProcessBigchainDB.ManufacturingStartAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.ManufacturingStartAt.Value) != 0)
                lstErrMsg.Add("● Quy trình sản xuất - Ngày bắt đầu đã bị thay đổi");

            if (posProcess.ManufacturingEndAt != null && posProcessBigchainDB.ManufacturingEndAt != null
                && DateTime.Compare(posProcessBigchainDB.ManufacturingEndAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.ManufacturingEndAt.Value) != 0)
                lstErrMsg.Add("● Quy trình sản xuất - Ngày hoàn thành đã bị thay đổi");

            if (posProcess.TransportBy != posProcessBigchainDB.TransportBy)
                lstErrMsg.Add("● Quy trình vận chuyển - Nhà vận chuyển đã bị thay đổi");

            if (posProcess.TransportStartAt != null && posProcessBigchainDB.TransportStartAt != null
                && DateTime.Compare(posProcessBigchainDB.TransportStartAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.TransportStartAt.Value) != 0)
                lstErrMsg.Add("● Quy trình vận chuyển - Ngày bắt đầu đã bị thay đổi");

            if (posProcess.TransportEndAt != null && posProcessBigchainDB.TransportEndAt != null
                && DateTime.Compare(posProcessBigchainDB.TransportEndAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.TransportEndAt.Value) != 0)
                lstErrMsg.Add("● Quy trình vận chuyển - Ngày hoàn thành đã bị thay đổi");

            if (posProcess.DistributeBy != posProcessBigchainDB.DistributeBy)
                lstErrMsg.Add("● Quy trình phân phối - Nhà phân phối đã bị thay đổi");

            if (posProcess.DistributeStartAt != null && posProcessBigchainDB.DistributeStartAt != null
                && DateTime.Compare(posProcessBigchainDB.DistributeStartAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.DistributeStartAt.Value) != 0)
                lstErrMsg.Add("● Quy trình phân phối - Ngày bắt đầu đã bị thay đổi");

            if (posProcess.DistributeEndAt != null && posProcessBigchainDB.DistributeEndAt != null
                && DateTime.Compare(posProcessBigchainDB.DistributeEndAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.DistributeEndAt.Value) != 0)
                lstErrMsg.Add("● Quy trình phân phối - Ngày hoàn thành đã bị thay đổi");

            if (posProcess.SignAt != null && posProcessBigchainDB.SignAt != null
                && DateTime.Compare(posProcessBigchainDB.SignAt.Value.AddHours(offset.Hours).AddMinutes(offset.Minutes).AddSeconds(offset.Seconds), posProcess.SignAt.Value) != 0)
                lstErrMsg.Add("● Quy trình kiểm định - Ngày ký đã bị thay đổi");

            if (posProcess.ManufacturingStatus != posProcessBigchainDB.ManufacturingStatus)
                lstErrMsg.Add("● Quy trình sản xuất - Trạng thái đã bị thay đổi");

            if (posProcess.TransportStatus != posProcessBigchainDB.TransportStatus)
                lstErrMsg.Add("● Quy trình vận chuyển - Trạng thái đã bị thay đổi");

            if (posProcess.DistributeStatus != posProcessBigchainDB.DistributeStatus)
                lstErrMsg.Add("● Quy trình phân phối - Trạng thái đã bị thay đổi");

            // Nhật ký
            var rsaHelper = new RSAHelper();

            var hashMessage = rsaHelper.ConvertStringToHash(posProcess.History);
            var isVerified = rsaHelper.VerifyData(hashMessage, posProcessBigchainDB.Signature, posProcess.PublicKey);
            if (!isVerified)
                lstErrMsg.Add("● Quy trình - Nhật ký đã bị thay đổi");

            // Sản phẩm
            var lstPosProduce = DataGemini.PosProduces.Where(x => x.Guid == posProcess.GuidProduce || x.Guid == posProcessBigchainDB.GuidProduce).ToList();
            var posProduce = lstPosProduce.FirstOrDefault(x => x.Guid == posProcess.GuidProduce);
            var posProduceBigchainDB = lstPosProduce.FirstOrDefault(x => x.Guid == posProcessBigchainDB.GuidProduce);

            if (posProduce.Guid != posProduceBigchainDB.Guid)
                lstErrMsg.Add("● Sản phẩm đã bị thay đổi");

            if (!posProduce.Code.Equals(HttpUtility.HtmlDecode(posProduceBigchainDB.Code), StringComparison.OrdinalIgnoreCase))
                lstErrMsg.Add("● Sản phẩm - Mã sản phẩm đã bị thay đổi");

            if (!posProduce.Name.Equals(HttpUtility.HtmlDecode(posProduceBigchainDB.Name), StringComparison.OrdinalIgnoreCase))
                lstErrMsg.Add("● Sản phẩm - Tên sản phẩm đã bị thay đổi");

            if (posProduce.GuidBatch != posProduceBigchainDB.GuidBatch)
                lstErrMsg.Add("● Sản phẩm - Lô hàng đã bị thay đổi");

            if (posProduce.GuidCategory != posProduceBigchainDB.GuidCategory)
                lstErrMsg.Add("● Sản phẩm - Danh mục hàng hóa đã bị thay đổi");

            return lstErrMsg;
        }
    }
}