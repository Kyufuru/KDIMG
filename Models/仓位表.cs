using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("仓位表")]
    class 仓位表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public string 仓位 { get; set; }
    }
}
