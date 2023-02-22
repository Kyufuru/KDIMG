using System;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("员工表")]
    class 员工表
    {
        [ExplicitKey]
        public int ID { get; set; }
        public int 组织ID { get; set; }
        public int 员工ID { get; set; }
        public string 员工编码 { get; set; }
        public string 员工任岗编码 { get; set; }
        public string 名称 { get; set; }
        public DateTime 创建日期 { get; set; }
        public DateTime 最后修改时间 { get; set; }
        public char 单据状态 { get; set; }
    }
}
