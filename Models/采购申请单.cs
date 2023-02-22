using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("采购申请单")]
    class 采购申请单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 源单头ID { get; set; }
        public int 源单分录ID { get; set; }
        public int 工程ID { get; set; }
        public int 父项物料ID { get; set; }
        public int 物料ID { get; set; }
        public int 单位ID { get; set; }
        public int 组织ID { get; set; }
        public int 项次 { get; set; }

        public string 单据编号 { get; set; }
        public string 产品名称 { get; set; }
        public float 申请数量 { get; set; }
        public float 批准数量 { get; set; }
        public float 允许采购数量 { get; set; }
        public float 订单关联数量 { get; set; }
        public float 剩余未出库数量 { get; set; }

        public DateTime? 创建日期 { get; set; }
        public DateTime? 申请日期 { get; set; }
        public DateTime? 到货日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 图纸下发日期 { get; set; }
        public DateTime? 建议采购日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }

        public char 单据状态 { get; set; }
        public char 关闭状态 { get; set; }
        public char 作废状态 { get; set; }
        public char 行业务终止 { get; set; }
        public char 行业务关闭 { get; set; }
    }
}
