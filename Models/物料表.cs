using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("物料表")]
    class 物料表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public int 组织ID { get; set; }
        public int 基本单位ID { get; set; }
        public int 库存单位ID { get; set; }
        public int 辅助单位ID { get; set; }
        public string 编码 { get; set; }
        public string 新助记码 { get; set; }
        public string 图号 { get; set; }
        public string 物料名称 { get; set; }
        public string 规格型号 { get; set; }
        public string 存货类别 { get; set; }
        public double 净重 { get; set; }
        public int 采购周期 { get; set; }
    }
}
