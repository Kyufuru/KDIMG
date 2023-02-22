using System;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("变更函")]
    class 变更函
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 物料ID { get; set; }
        public int 单位ID { get; set; }
        public int 组织ID { get; set; }
        public int 工程ID { get; set; }
        public int 申请部门ID { get; set; }
        public int 申请人ID { get; set; }
        public string 单据编号 { get; set; }
        public string 主题 { get; set; }
        public string 变更类型 { get; set; }
        public string 变更内容 { get; set; }
        public string 变更原因 { get; set; }
        public string 变更原因补充 { get; set; }
        public string 责任部门 { get; set; }
        public double 变更数量 { get; set; }
        public DateTime? 创建日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }
        public char 单据状态 { get; set; }
    }
}
