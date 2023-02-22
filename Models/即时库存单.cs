using System;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("即时库存单")]
    class 即时库存单
    {
        public int Id { get; set; }
        public string 头ID { get; set; }
        public int 物料ID { get; set; }
        public int 组织ID { get; set; }
        public int 仓库ID { get; set; }
        public DateTime? 最后更新日期 { get; set; }
        public double 库存数量 { get; set; }
    }
}
