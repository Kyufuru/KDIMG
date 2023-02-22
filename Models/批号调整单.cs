using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;

namespace 金蝶中间层镜像.Models
{
    [Table("批号调整单")]
    class 批号调整单
    {
        public int Id { get; set; }
        public int 头ID { get; set; }
        public int 分录ID { get; set; }
        public int 源单头ID { get; set; }
        public int 源单分录ID { get; set; }
        public int 物料ID { get; set; }
        public int 组织ID { get; set; }
        public string 单据编号 { get; set; }
        public string 工程号 { get; set; }
        public double 转换数量 { get; set; }
        public DateTime? 创建日期 { get; set; }
        public DateTime? 审核日期 { get; set; }
        public DateTime? 最后修改时间 { get; set; }
        public char 单据状态 { get; set; }
        public char 作废状态 { get; set; }
        public char? 转换类型 { get; set; }
    }
}
