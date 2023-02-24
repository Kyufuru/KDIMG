using Dapper.Contrib.Extensions;
using System;

namespace 金蝶中间层镜像.Models
{
    [Table("采购入库单")]
    class 采购入库单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 源单头ID { get; set; }
        public int 源单分录ID { get; set; }
        public int 工程ID { get; set; }
        public int 物料ID { get; set; }
        public int 组织ID { get; set; }
        public int 单位ID { get; set; }
        public int 批号ID { get; set; }
        public int 供应商ID { get; set; }
        public int 仓管员ID { get; set; }
        public int 仓库ID { get; set; }
        public int 仓位ID { get; set; }
        public int 项次 { get; set; }

        public string 产品名称 { get; set; }
        public string 单据编号 { get; set; }
        public string 送货单号 { get; set; }
        public string 检验单号 { get; set; }

        public DateTime? 到货日期 { get; set; }
        public DateTime? 创建日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 入库日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }

        public double 应收数量 { get; set; }
        public double 实收数量 { get; set; }
        public double 退料数量 { get; set; }
        public double 报检数量 { get; set; }
        public double 检验合格数量 { get; set; }

        public char 单据状态 { get; set; }
        public char 作废状态 { get; set; }
        public char 检验状态 { get; set; }
        public char 是否勾选检验 { get; set; }
    }
}
