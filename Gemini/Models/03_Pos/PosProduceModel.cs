using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gemini.Models._02_Cms.U;
using Gemini.Resources;

namespace Gemini.Models._03_Pos
{
    public class PosProduceModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public Guid Guid { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public String Name { get; set; }

        public bool Active { get; set; }

        [StringLength(2000, ErrorMessageResourceName = "ErrorMaxLength2000", ErrorMessageResourceType = typeof(Resource))]
        public String Note { get; set; }

        public String Description { get; set; }

        public Decimal? Price { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        public String Unit { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public Guid GuidBatch { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public Guid GuidCategory { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        public String SeoTitle { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        public String SeoDescription { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        public String SeoFriendUrl { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public String Code { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        public String Status { get; set; }

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

        public List<UGalleryModel> ListGallery { get; set; }

        public String LinkImg0 { get; set; }

        public String LinkImg1 { get; set; }

        public String LinkImg2 { get; set; }

        public List<SReplaceCode> ReplaceCode { get; set; }

        public String NameCategory { get; set; }

        public String NameBatch { get; set; }

        public SUser ManufacturingBy { get; set; }

        public SUser TransportBy { get; set; }

        public SUser DistributeBy { get; set; }

        public PosCategory PosCategory { get; set; }

        public PosBatch PosBatch { get; set; }

        public PosProcess PosProcess { get; set; }

        public String PosProcessStatusName { get; set; }

        #region Constructor
        public PosProduceModel()
        {
        }

        public PosProduceModel(PosProduce posProduce)
        {
            Guid = posProduce.Guid;
            Name = posProduce.Name;
            Active = posProduce.Active;
            Note = HttpUtility.HtmlDecode(posProduce.Note);
            CreatedAt = posProduce.CreatedAt;
            CreatedBy = posProduce.CreatedBy;
            UpdatedAt = posProduce.UpdatedAt;
            UpdatedBy = posProduce.UpdatedBy;
            Description = HttpUtility.HtmlDecode(posProduce.Description);
            Price = posProduce.Price;
            Unit = posProduce.Unit;
            GuidBatch = posProduce.GuidBatch;
            GuidCategory = posProduce.GuidCategory;
            SeoTitle = posProduce.SeoTitle;
            SeoDescription = posProduce.SeoDescription;
            SeoFriendUrl = posProduce.SeoFriendUrl;
            Code = posProduce.Code;
            Status = posProduce.Status;
        }
        #endregion

        #region Function
        public void Setvalue(PosProduce posProduce)
        {
            if (IsUpdate == 0)
            {
                posProduce.Guid = Guid.NewGuid();
                posProduce.CreatedBy = CreatedBy;
                posProduce.CreatedAt = DateTime.Now;
            }
            posProduce.Name = Name;
            posProduce.Active = Active;
            posProduce.Note = Note;
            posProduce.UpdatedAt = DateTime.Now;
            posProduce.UpdatedBy = UpdatedBy;
            posProduce.Description = Description;
            posProduce.Price = Price;
            posProduce.Unit = Unit;
            posProduce.GuidBatch = GuidBatch;
            posProduce.GuidCategory = GuidCategory;
            posProduce.SeoTitle = SeoTitle;
            posProduce.SeoDescription = SeoDescription;
            posProduce.SeoFriendUrl = SeoFriendUrl;
            posProduce.Code = Code;
            posProduce.Status = Status;
        }
        #endregion
    }
}