using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("采购员表")]
    class 采购员表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public string 名称 { get; set; }
    }
}
