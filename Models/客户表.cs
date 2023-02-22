using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("客户表")]
    class 客户表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public int 组织ID { get; set; }
        public string 名称 { get; set; }
    }
}
