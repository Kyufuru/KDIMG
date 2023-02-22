using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("供应商表")]
    class 供应商表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public int 组织ID { get; set; }
        public string 编码 { get; set; }
        public string 名称 { get; set; }
    }
}
