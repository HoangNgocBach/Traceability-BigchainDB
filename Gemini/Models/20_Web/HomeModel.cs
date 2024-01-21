using Gemini.Models._03_Pos;
using Gemini.Models._05_Website;
using System.Collections.Generic;

namespace Gemini.Models._20_Web
{
    public class ProduceListModel
    {
        public PosCategoryModel PosCategory { get; set; }

        public List<PosCategoryModel> ListPosCategory { get; set; }

        public List<PosProduceModel> ListPosProduceByCategory { get; set; }
    }

    public class ProduceDetailModel
    {
        public PosProduceModel PosProduce { get; set; }
    }

    public class AboutUsModel
    {
        public WUsAboutModel AboutUs { get; set; }
    }
}