using System;
using System.Web;
using Gemini.Resources;
using System.ComponentModel.DataAnnotations;

namespace Gemini.Models._05_Website
{
    public class WUsAboutModel
    {
        public int IsUpdate { get; set; }

        #region Properties
        [ScaffoldColumn(false)]
        public Guid Guid { get; set; }

        [StringLength(255, ErrorMessageResourceName = "ErrorMaxLength255", ErrorMessageResourceType = typeof(Resource))]
        [Required(ErrorMessageResourceType = typeof(Resources.Resource), ErrorMessageResourceName = "RequiredFill")]
        public String Name { get; set; }

        public bool Active { get; set; }

        public String Description { get; set; }

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

        #region Constructor
        public WUsAboutModel()
        {
        }

        public WUsAboutModel(WUsAbout wUsAbout)
        {
            Guid = wUsAbout.Guid;
            Name = wUsAbout.Name;
            Active = wUsAbout.Active;
            CreatedAt = wUsAbout.CreatedAt;
            CreatedBy = wUsAbout.CreatedBy;
            UpdatedAt = wUsAbout.UpdatedAt;
            UpdatedBy = wUsAbout.UpdatedBy;
            Description = HttpUtility.HtmlDecode(wUsAbout.Description);
        }
        #endregion

        #region Function
        public void Setvalue(WUsAbout wUsAbout)
        {
            if (IsUpdate == 0)
            {
                wUsAbout.Guid = Guid.NewGuid();
                wUsAbout.CreatedBy = CreatedBy;
                wUsAbout.CreatedAt = DateTime.Now;
            }
            wUsAbout.Name = Name;
            wUsAbout.Active = Active;
            wUsAbout.UpdatedAt = DateTime.Now;
            wUsAbout.UpdatedBy = UpdatedBy;
            wUsAbout.Description = Description;
        }
        #endregion
    }
}