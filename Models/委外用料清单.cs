using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像
{
    [Table("委外用料清单")]
    class 委外用料清单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 工程ID { get; set; }
        public int 产品ID { get; set; }
        public int 组织ID { get; set; }
        public int BOMID { get; set; }
        public int 子项物料ID { get; set; }
        public int 子项单位ID { get; set; }
        public int 委外订单行号 { get; set; }
        public int 项次 { get; set; }

        public string 单据编号 { get; set; }
        public string 委外订单编号 { get; set; }
        public string 产品名称 { get; set; }
        public string 备注 { get; set; }

        public double 分子 { get; set; }
        public double 分母 { get; set; }
        public double 预采购 { get; set; }
        public double 预委外 { get; set; }
        public double 预库存 { get; set; }
        public double 数量 { get; set; }
        public double 应发数量 { get; set; }
        public double 已领数量 { get; set; }
        public double 已申请数量 { get; set; }
        public double 原始携带量 { get; set; }
        public double 已批号调整数量 { get; set; }

        public DateTime? 创建日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }

        public char? 单据状态 { get; set; }
        public char? 子项类型 { get; set; }
        public char? 申请关闭 { get; set; }
        public char? 批号调整关闭 { get; set; }
    }
}