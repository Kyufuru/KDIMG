using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("单据类型表")]
    class 单据类型表
    {
        [ExplicitKey]
        public string ID { get; set; }
        public string 名称 { get; set; }
    }
}
