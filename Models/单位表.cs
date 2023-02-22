using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("单位表")]
    class 单位表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public string 名称 { get; set; }
    }
}
