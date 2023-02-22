using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("监控表")]
    public class 监控表
    {
        public int Id { get; set; }
        public string 类别 { get; set; }
        public string 名称 { get; set; }
        public string 源表名称 { get; set; }
        public string 明细表名称 { get; set; }
        public string 头ID名称 { get; set; }
        public string 工程ID名称 { get; set; }
        public DateTime 最后修改时间 { get; set; }
        public DateTime 更新时间 { get; set; }
    }   
}
