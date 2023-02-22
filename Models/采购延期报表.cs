using System;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("采购延期报表")]
    class 采购延期报表
    {
        public int Id { get; set; }
        public int 订单头ID { get; set; }
        public int 订单分录ID { get; set; }
        public int 入库头ID { get; set; }
        public int 入库分录ID { get; set; }
        public int 延期天数 { get; set; }
        public double 采购数量 { get; set; }
        public double 延期数量 { get; set; }
        public double 实际入库数量 { get; set; }
        public float 完成率 { get; set; }
        public float 准时率 { get; set; }
        public double 绝对延期天数 { get; set; }
        public double 平均延期天数 { get; set; }
        public DateTime? 最后修改时间 { get; set; }
    }
}
