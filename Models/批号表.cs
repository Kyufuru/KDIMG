using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("批号表")]
    class 批号表
    {
        [ExplicitKey]
        public string ID { get; set; }
        public string 批号 { get; set; }
    }
}
