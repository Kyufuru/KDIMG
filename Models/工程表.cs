using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("工程表")]
    class 工程表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public int 组织ID { get; set; }
        public int 客户ID { get; set; }
        public string 编码 { get; set; }
        public string 名称 { get; set; }
        public DateTime? 创建日期 { get; set; }
        public string 工程状态 { get; set; }
        public string 部门属性 { get; set; }
    }
}
