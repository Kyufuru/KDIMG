using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("仓库表")]
    class 仓库表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public string 名称 { get; set; }
    }
}
