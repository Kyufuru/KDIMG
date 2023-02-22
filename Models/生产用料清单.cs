using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("生产用料清单")]
    class 生产用料清单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 工程ID { get; set; }
        public int 产品ID { get; set; }
        public int 组织ID { get; set; }
        public int BOMID { get; set; }
        public int 生产订单ID { get; set; }
        public int 子项物料ID { get; set; }
        public int 子项单位ID { get; set; }
        public int 生产订单行号 { get; set; }
        public int 项次 { get; set; }

        public string 单据编号 { get; set; }
        public string 供应商批号 { get; set; }
        public string 生产订单编号 { get; set; }
        public string 物料申请单单号 { get; set; }
        public string 产品名称 { get; set; }
        public string 研发备注 { get; set; }
        public string 直发工地 { get; set; }

        public double 分子 { get; set; }
        public double 分母 { get; set; }
        public double 预采购 { get; set; }
        public double 预委外 { get; set; }
        public double 预库存 { get; set; }
        public double 数量 { get; set; }
        public double 应发数量 { get; set; }
        public double 已领数量 { get; set; }
        public double 未领数量 { get; set; }
        public double 已申请数量 { get; set; }
        public double 补料选单数量 { get; set; }
        public double 退料选单数量 { get; set; }
        public double 已下委外单数量 { get; set; }
        public double 已批号调整数量 { get; set; }
        public double 批号库存数量 { get; set; }
        public double 累计入库数量 { get; set; }

        public DateTime? 创建日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }

        public char? 单据状态 { get; set; }
        public char? 申请关闭 { get; set; }
        public char? 委外关闭 { get; set; }
        public char? 批号调整关闭 { get; set; }
    }
}
