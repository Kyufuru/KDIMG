using System;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("委外订单")]
    class 委外订单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 源单头ID { get; set; }
        public int 源单分录ID { get; set; }
        public int 工程ID { get; set; }
        public int 物料ID { get; set; }
        public int 单位ID { get; set; }
        public int 项次 { get; set; }

        public string 单据编号 { get; set; }
        public double 数量 { get; set; }

        public DateTime? 创建日期 { get; set; }
        public DateTime? 下达日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }

        public char 单据状态 { get; set; }
        public char 作废状态 { get; set; }
        public char 挂起状态 { get; set; }
        public char 业务状态 { get; set; }
    }
}
