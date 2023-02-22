using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("镜像日志")]
    class 镜像日志
    {
        public int Id { get; set; }
        public DateTime? 时间 { get; set; }
        public string 来源 { get; set; }
        public long 行数 { get; set; }
        public string 备注 { get; set; }
    }
}
