using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Antlr.Runtime.Misc;
using Gemini.Resources;

namespace Gemini.Models._03_Pos
{
    public class PosProcessModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public Guid Guid { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public Guid GuidProduce { get; set; }

        public Guid? ManufacturingBy { get; set; }

        public DateTime? ManufacturingStartAt { get; set; }

        public DateTime? ManufacturingEndAt { get; set; }

        public Guid? TransportBy { get; set; }

        public DateTime? TransportStartAt { get; set; }

        public DateTime? TransportEndAt { get; set; }

        public Guid? DistributeBy { get; set; }

        public DateTime? DistributeStartAt { get; set; }

        public DateTime? DistributeEndAt { get; set; }

        public string History { get; set; }

        public string Signature { get; set; }

        public string PublicKey { get; set; }

        public DateTime? SignAt { get; set; }

        public int? ManufacturingStatus { get; set; }

        public int? TransportStatus { get; set; }

        public int? DistributeStatus { get; set; }

        public int? Status { get; set; }

        public string BigchainDBTransactionId { get; set; }

        [Editable(false)]
        public DateTime? CreatedAt { get; set; }

        [Editable(false)]
        [StringLength(25, ErrorMessageResourceName = "ErrorMaxLength25", ErrorMessageResourceType = typeof(Resource))]
        public String CreatedBy { get; set; }

        [Editable(false)]
        public DateTime? UpdatedAt { get; set; }

        [Editable(false)]
        [StringLength(25, ErrorMessageResourceName = "ErrorMaxLength25", ErrorMessageResourceType = typeof(Resource))]
        public String UpdatedBy { get; set; }
        #endregion

        public String StatusName { get; set; }

        public String StatusUploadBigchainDB { get; set; }

        #region PosProduce

        public String ProduceCode { get; set; }

        public String ProduceName { get; set; }

        public Guid? ProduceGuidBatch { get; set; }

        public Guid? ProduceGuidCategory { get; set; }

        #endregion

        public String ManufacturingStatusName { get; set; }

        public String TransportStatusName { get; set; }

        public String DistributeStatusName { get; set; }

        public String TransportByName { get; set; }

        public String DistributeByName { get; set; }

        public String ManufacturingStartAtString { get; set; }

        public String ManufacturingEndAtString { get; set; }

        public String TransportStartAtString { get; set; }

        public String TransportEndAtString { get; set; }

        public String DistributeStartAtString { get; set; }

        public String DistributeEndAtString { get; set; }

        #region Constructor
        public PosProcessModel()
        {
        }

        public PosProcessModel(PosProcess posProcess)
        {
            Guid = posProcess.Guid;
            GuidProduce = posProcess.GuidProduce;
            ManufacturingBy = posProcess.ManufacturingBy;
            ManufacturingStartAt = posProcess.ManufacturingStartAt;
            ManufacturingEndAt = posProcess.ManufacturingEndAt;
            TransportBy = posProcess.TransportBy;
            TransportStartAt = posProcess.TransportStartAt;
            TransportEndAt = posProcess.TransportEndAt;
            DistributeBy = posProcess.DistributeBy;
            DistributeStartAt = posProcess.DistributeStartAt;
            DistributeEndAt = posProcess.DistributeEndAt;
            PublicKey = posProcess.PublicKey;
            Signature = posProcess.Signature;
            SignAt = posProcess.SignAt;
            ManufacturingStatus = posProcess.ManufacturingStatus;
            TransportStatus = posProcess.TransportStatus;
            DistributeStatus = posProcess.DistributeStatus;
            Status = posProcess.Status;
            History = posProcess.History;
            BigchainDBTransactionId = posProcess.BigchainDBTransactionId;
            CreatedAt = posProcess.CreatedAt;
            CreatedBy = posProcess.CreatedBy;
            UpdatedAt = posProcess.UpdatedAt;
            UpdatedBy = posProcess.UpdatedBy;
        }
        #endregion

        #region Function
        public void Setvalue(PosProcess posProcess)
        {
            if (IsUpdate == 0)
            {
                posProcess.Guid = Guid.NewGuid();
                posProcess.CreatedBy = CreatedBy;
                posProcess.CreatedAt = DateTime.Now;
            }
            posProcess.GuidProduce = GuidProduce;
            posProcess.BigchainDBTransactionId = BigchainDBTransactionId;
            posProcess.History = History;
            posProcess.ManufacturingBy = ManufacturingBy;
            posProcess.ManufacturingStartAt = ManufacturingStartAt;
            posProcess.ManufacturingEndAt = ManufacturingEndAt;
            posProcess.TransportBy = TransportBy;
            posProcess.TransportStartAt = TransportStartAt;
            posProcess.TransportEndAt = TransportEndAt;
            posProcess.DistributeBy = DistributeBy;
            posProcess.DistributeStartAt = DistributeStartAt;
            posProcess.DistributeEndAt = DistributeEndAt;
            posProcess.PublicKey = PublicKey;
            posProcess.Signature = Signature;
            posProcess.SignAt = SignAt;
            posProcess.ManufacturingStatus = ManufacturingStatus;
            posProcess.TransportStatus = TransportStatus;
            posProcess.DistributeStatus = DistributeStatus;
            posProcess.Status = Status;
            posProcess.UpdatedAt = DateTime.Now;
            posProcess.UpdatedBy = UpdatedBy;
        }
        #endregion
    }

    #region BigchainDB

    public class Asset
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public string process { get; set; }
    }

    public class ResponseBigchainDB
    {
        public Asset asset { get; set; }
    }

    #endregion
}