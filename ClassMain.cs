using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Dapper;
using Dapper.Contrib.Extensions;
using JSysLibrary;
using JWNorm;
using 金蝶中间层镜像.Models;

namespace 金蝶中间层镜像
{
    public partial class ClassMain : JWNormHostVS
    {
        /// <summary>
        /// 窗口句柄
        /// </summary>
        IntPtr pForm = IntPtr.Zero;
        /// <summary>
        /// 日志缓存
        /// </summary>
        public static StrLogHcq buffer = null;
        /// <summary>
        /// 线程
        /// </summary>
        public static Thread mainThr, manThr;

        /// <summary>
        /// 开关
        /// </summary>
        public static bool isManual, isStop;
        private bool isDaily;
        /// <summary>
        /// 日期格式
        /// </summary>
        public static string DFMT = "yyyy/MM/dd HH:mm:ss.fff";

        /// <summary>
        /// 金蝶连接字符串
        /// </summary>
        public static readonly string CONNSTR_KD = "data source=192.168.0.201;initial catalog=AIS20201230201927;persist security info=True;user id=jdmiddb;password=Jumper@0203;MultipleActiveResultSets=True;MultipleActiveResultSets=true;";
        /// <summary>
        /// 统计类报表连接字符串
        /// </summary>
        public static readonly string CONNSTR_STAT = "data source=192.168.0.204;initial catalog=统计类报表;persist security info=True;user id=sa;password=Jumper#669385;MultipleActiveResultSets=True;MultipleActiveResultSets=true;";
        /// <summary>
        /// 镜像连接字符串
        /// </summary>
        public static readonly string CONNSTR_IMG = "data source=192.168.0.204;initial catalog=金蝶数据镜像;persist security info=True;user id=sa;password=Jumper#669385;MultipleActiveResultSets=True;MultipleActiveResultSets=true;";
        /// <summary>
        /// 日志连接字符串
        /// </summary>
        public static readonly string CONNSTR_LOG = "data source=192.168.0.204;initial catalog=中间层日志;persist security info=True;user id=sa;password=Jumper#669385;MultipleActiveResultSets=True;MultipleActiveResultSets=true;";
        /// <summary>
        /// SQL
        /// </summary>
        public static Dictionary<string, string> SQL;

        static ClassMain()
        {
            isStop = true;
            isManual = false;
            buffer = new StrLogHcq(1024*10);
            SQL = new Dictionary<string, string>
            {
                ["统计类报表"] = "[192.168.0.204].统计类报表.dbo.",
                ["金蝶数据镜像"] = "[192.168.0.204].金蝶数据镜像.dbo.",
                ["金蝶"] = "[192.168.0.201].AIS20201230201927.dbo.",
                ["基础资料日期"] = "IIF(FMODIFYDATE > FCREATEDATE,FMODIFYDATE,FCREATEDATE)",
                ["单据日期"] = "IIF(FMODIFYDATE > ISNULL(FAPPROVEDATE,0),FMODIFYDATE,FAPPROVEDATE)",
                ["SEL明细工程ID"] = "LEFT JOIN (SELECT FID,{0} 工程ID FROM {1}) A1 ON A1.FID = A.ID",
                ["采购延期报表"] = @"
                SELECT
                    订单头ID,订单分录ID,入库头ID,入库分录ID,
                    DATEDIFF(DAY,计划到货日期,IIF(最终未入库数量 > 0,GETDATE(),入库日期)) 延期天数,
                    采购数量,延期数量,实际入库数量,
                    1 - (最终未入库数量 / 采购数量) 完成率,
                    CASE 
                        WHEN 入库日期 IS NULL 
                        THEN IIF(计划到货日期 > GETDATE(),0,1)
                        ELSE 1 - (延期数量 / 采购数量)
                    END 准时率,
                    IIF(ISNULL(绝对延期天数,0) <= 0,0,绝对延期天数) 绝对延期天数,
                    IIF(ISNULL(绝对延期天数,0) <= 0,0,绝对延期天数) / 采购数量 平均延期天数,
                    最后修改时间
                FROM (
                    SELECT 
                        MAX(订单头ID) 订单头ID,订单分录ID,
                        MAX(入库头ID) 入库头ID,入库分录ID,
                        MAX(计划到货日期) 计划到货日期,
                        IIF(SUM(IIF(入库日期 IS NULL,1,0)) = 0,MAX(入库日期),NULL) 入库日期,
                        MAX(采购数量) 采购数量,
                        MAX(实际入库数量) 实际入库数量,
                        MAX(最终未入库数量) 最终未入库数量,
                        SUM(IIF(相对延期天数 > 0,实际入库数量,0)) + IIF(MAX(最终未入库数量) > 0 AND MAX(计划到货日期) < GETDATE(),MAX(最终未入库数量),0) 延期数量,
                        SUM(IIF(相对延期天数 > 0,实际入库数量 * 相对延期天数,0)) + (MAX(最终未入库数量)) * DATEDIFF(DAY,MAX(计划到货日期),GETDATE()) 绝对延期天数,
                        MAX(最后修改时间) 最后修改时间
                    FROM (
                        SELECT 
                            订单头ID,订单分录ID,入库头ID,入库分录ID,计划到货日期,入库日期,
                            采购数量,ISNULL(实际入库数量,0) 实际入库数量,
                            采购数量 - ISNULL(累计入库数量,0) 最终未入库数量,
                            DATEDIFF(DAY,计划到货日期,ISNULL(入库日期,GETDATE())) 相对延期天数,
                            IIF(订单修改时间 > ISNULL(入库修改时间,0),订单修改时间,入库修改时间) 最后修改时间
                        FROM (
                            SELECT DISTINCT
                                头ID 订单头ID,分录ID 订单分录ID,物料ID,
                                计划到货日期,采购数量,最后修改时间 订单修改时间
                            FROM 采购订单
                            WHERE 头ID IN({0}) AND 作废状态 = 'A' AND 采购数量 >= 1 AND 单据状态 != 'Z'
                        ) A1
                        LEFT JOIN (
                            SELECT
                                头ID 入库头ID,分录ID 入库分录ID,源单头ID,源单分录ID,
                                创建日期 入库日期,
                                (实收数量 - 退料数量) 实际入库数量,
                                最后修改时间 入库修改时间
                            FROM 采购入库单
                            WHERE 作废状态 = 'A' AND 单据状态 != 'Z'
                        ) A2 ON A2.源单分录ID = A1.订单分录ID
                        LEFT JOIN (
                            SELECT 
                                源单分录ID,
                                SUM(实收数量 - 退料数量) 累计入库数量
                            FROM 采购入库单
                            WHERE 作废状态 = 'A' AND 单据状态 != 'Z'
                            GROUP BY 源单分录ID
                        ) A3 ON A3.源单分录ID = A1.订单分录ID
                    ) GRP
                    GROUP BY 订单分录ID,入库分录ID
                ) B1",
                ["员工表"] = @"
                SELECT
                    ID,组织ID,员工ID,
                    员工编码,员工任岗编码,名称,
                    创建日期,最后修改时间,单据状态
                FROM (
                    SELECT
                        FSTAFFID ID,
                        FUSEORGID 组织ID,
                        FEMPINFOID 员工ID,
                        FNUMBER 员工编码,
                        FSTAFFNUMBER 员工任岗编码,
                        FCREATEDATE 创建日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态
                    FROM T_BD_STAFF
                    WHERE FSTAFFID IN({0})
                ) BS
                LEFT JOIN (
                    SELECT FSTAFFID,FNAME 名称
                    FROM T_BD_STAFF_L
                ) HEIL ON HEIL.FSTAFFID = BS.ID",
                ["客户表"] = @"
                SELECT 
                    ID,组织ID,名称
                FROM (
                    SELECT
                        FCUSTID ID,
                        FUSEORGID 组织ID
                    FROM T_BD_CUSTOMER
                    WHERE FCUSTID IN({0})
                ) C
                LEFT JOIN (
                    SELECT
                        FCUSTID,
                        FNAME 名称
                    FROM T_BD_CUSTOMER_L
                ) CL ON CL.FCUSTID = C.ID",
                ["采购员表"] = @"
                SELECT 
                    FID ID,
                    FNAME 名称
                FROM V_BD_BUYER_L
                WHERE FID IN({0})",
                ["仓位表"] = @"
                SELECT
                    FID ID,
                    FNUMBER 仓位
                FROM T_BAS_FLEXVALUESENTRY
                WHERE FID IN({0})",
                ["仓库表"] = @"
                SELECT 
                    FSTOCKID ID,
                    FNAME 名称
                FROM T_BD_STOCK_L
                WHERE FSTOCKID IN({0})",
                ["仓管员表"] = @"
                SELECT 
                    FID ID,
                    FNAME 名称
                FROM V_BD_WAREHOUSEWORKERS_L
                WHERE FID IN({0})",
                ["单据类型表"] = @"
                SELECT
                    FBILLTYPEID ID,
                    FNAME 名称
                FROM T_BAS_BILLTYPE_L
                WHERE FBILLTYPEID IN({0})",
                ["供应商表"] = @"
                SELECT
                    ID,组织ID,编码,名称
                FROM (
                    SELECT
                        FSUPPLIERID ID,
                        FNUMBER 编码,
                        FUSEORGID 组织ID
                    FROM T_BD_SUPPLIER
                    WHERE FSUPPLIERID IN({0})
                ) S
                LEFT JOIN (
                    SELECT
                        FSUPPLIERID,
                        FNAME 名称
                    FROM T_BD_SUPPLIER_L
                ) SL ON S.ID = SL.FSUPPLIERID",
                ["工程表"] = @"
                SELECT
                    ID,组织ID,客户ID,编码,名称,
                    创建日期,工程状态,部门属性
                FROM (
                    SELECT 
                        FDEPTID ID,
                        FDEPTPROPERTY 部门属性ID,
                        FUSEORGID 组织ID,
                        F_JZ_BASE 客户ID,
                        FNUMBER 编码,
                        FCREATEDATE 创建日期,
                        CASE
                            WHEN FBILLSTATUS = 1 THEN '在建'
                            WHEN FBILLSTATUS = 2 THEN '售后期内'
                            WHEN FBILLSTATUS = 3 THEN '售后期外'
                            ELSE '未定义'
                        END 工程状态
                    FROM T_BD_DEPARTMENT
                    WHERE FDEPTID IN({0})
                ) D
                LEFT JOIN (
                    SELECT
                        FDEPTID,
                        FNAME 名称
                    FROM T_BD_DEPARTMENT_L
                ) DL ON D.ID = DL.FDEPTID
                JOIN (
                    SELECT 
                        FENTRYID,
                        FDATAVALUE 部门属性
                    FROM T_BAS_ASSISTANTDATAENTRY_L
                    WHERE FDATAVALUE = '基本生产部门'
                ) ADL ON D.部门属性ID = ADL.FENTRYID",
                ["物料表"] = @"
                SELECT
                    ID,组织ID,基本单位ID,库存单位ID,辅助单位ID,
                    编码,新助记码,图号,物料名称,规格型号,存货类别,净重,采购周期
                FROM (
                    SELECT 
                        FMATERIALID ID,
                        FNUMBER 编码,
                        FUSEORGID 组织ID,
                        FMNEMONICCODE 新助记码,
                        F_JP_REMARK1 图号
                    FROM T_BD_MATERIAL
                    WHERE FMATERIALID IN({0})
                ) M
                LEFT JOIN (
                    SELECT
                        FMATERIALID,
                        FNAME 物料名称,
                        FSPECIFICATION 规格型号
                    FROM T_BD_MATERIAL_L
                ) ML ON M.ID = ML.FMATERIALID
                LEFT JOIN (
                    SELECT
                        FMATERIALID,
                        FCATEGORYID 存货类别ID,
                        FBASEUNITID 基本单位ID,
                        FNETWEIGHT 净重
                    FROM T_BD_MATERIALBASE
                ) MB ON M.ID = MB.FMATERIALID
                LEFT JOIN (
                    SELECT
                        FMATERIALID,
                        FSTOREUNITID 库存单位ID,
                        FAUXUNITID 辅助单位ID
                    FROM T_BD_MATERIALSTOCK
                ) MS ON M.ID = MS.FMATERIALID
                LEFT JOIN (
                    SELECT
                        FCATEGORYID,
                        FNAME 存货类别
                    FROM T_BD_MATERIALCATEGORY_L
                ) CL ON MB.存货类别ID = CL.FCATEGORYID
                LEFT JOIN (
                    SELECT
                        FMATERIALID,
                        FFIXLEADTIME 采购周期
                    FROM T_BD_MATERIALPLAN
                ) MP ON M.ID = MP.FMATERIALID",
                ["单位表"] = @"
                SELECT
                    ID,
                    名称,
                    最后修改时间
                FROM (
                    SELECT
                        FUNITID ID,
                        FMODIFYDATE 最后修改时间
                    FROM 
                        T_BD_UNIT
                    WHERE FUNITID IN({0})
                ) U
                LEFT JOIN (
                    SELECT
                        FUNITID,
                        FNAME 名称
                    FROM
                        T_BD_UNIT_L
                ) UL ON UL.FUNITID = U.ID",
                ["批号表"] = @"
                SELECT
                    FLOTID ID,
                    FNUMBER 批号
                FROM T_BD_LOTMASTER
                WHERE FLOTID IN({0})",
                ["即时库存单"] = @"
                SELECT
                    FID 头ID,
                    FMATERIALID 物料ID,
                    FSTOCKORGID 组织ID,
                    FSTOCKID 仓库ID,
                    FUPDATETIME 最后更新日期,
                    FBASEQTY 库存数量
                FROM T_STK_INVENTORY
                WHERE FID IN({0})",
                ["其他入库单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_STK_MISCELLANEOUS
                    WHERE FID IN({0})
                ) SMC
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID,
                        FQTY 数量
                    FROM T_STK_MISCELLANEOUSENTRY
                ) SMCE ON SMC.头ID = SMCE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_STK_MISCELLANEOUSENTRY_LK
                ) SMCEL ON SMCE.分录ID = SMCEL.FENTRYID",
                ["其他出库单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,
                    单据编号,库存方向,数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FSTOCKDIRECT 库存方向,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_STK_MISDELIVERY
                    WHERE FID IN({0})
                ) SMD
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID,
                        FQTY 数量
                    FROM T_STK_MISDELIVERYENTRY
                ) SMDE ON SMD.头ID = SMDE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_STK_MISDELIVERYENTRY_LK
                ) SMDEL ON SMDE.分录ID = SMDEL.FENTRYID",
                ["批号调整单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    物料ID,组织ID,单据编号,工程号,转换数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态,转换类型
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_STK_LOTADJUST
                    WHERE FID IN({0})
                ) SMC
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        F_JP_PRONAME 工程号,
                        FQTY 转换数量,
                        FCONVERTTYPE 转换类型
                    FROM T_STK_LOTADJUSTENTRY
                ) SMCE ON SMC.头ID = SMCE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_STK_LOTADJUSTENTRY_LK
                ) SMCEL ON SMCE.分录ID = SMCEL.FENTRYID",
                ["采购申请单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,父项物料ID,物料ID,单位ID,组织ID,项次,
                    单据编号,产品名称,申请数量,批准数量,
                    允许采购数量,订单关联数量,剩余未出库数量,
                    创建日期,申请日期,到货日期,审核日期,
                    图纸下发日期,建议采购日期,最后修改时间,
                    单据状态,关闭状态,作废状态,行业务终止,行业务关闭
                FROM (
                    SELECT
                        FID 头ID,
                        FAPPLICATIONORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPLICATIONDATE 申请日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCLOSESTATUS 关闭状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PUR_REQUISITION
                    WHERE FID IN({0})
                ) PR
                LEFT JOIN (
                    SELECT 
                        FID,
                        FENTRYID 分录ID,
                        FREQUIREDEPTID 工程ID,
                        F_JPUM_PANRTMAT 父项物料ID,
                        FMATERIALID 物料ID,
                        FUNITID 单位ID,
                        FSEQ 项次,
                        FJPUMPRODUCT 产品名称,
                        FREQQTY 申请数量,
                        FAPPROVEQTY 批准数量,
                        F_JP_QTY 允许采购数量,
                        F_JZ_QTY1 剩余未出库数量,
                        F_JPUM_TZXFHXM 图纸下发日期,
                        FSUGGESTPURDATE 建议采购日期,
                        FARRIVALDATE 到货日期,
                        FMRPCLOSESTATUS 行业务关闭,
                        FMRPTERMINATESTATUS 行业务终止
                    FROM T_PUR_REQENTRY 
                ) PRE ON PR.头ID = PRE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FORDERJOINQTY 订单关联数量
                    FROM T_PUR_REQENTRY_R
                ) PRER ON PRE.分录ID = PRER.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PUR_REQENTRY_LK
                ) PREL ON PRE.分录ID = PREL.FENTRYID",
                ["采购订单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,工程ID,部门ID,
                    物料ID,组织ID,单位ID,批号ID,供应商ID,采购员ID,项次,
                    单据类型ID,单据编号,工单号,
                    产品名称,实际重量,采购数量,已入库数量,未入库数量,累计入库数量,
                    创建日期,采购日期,审核日期,计划到货日期,最后修改时间,
                    单据状态,关闭状态,作废状态,行业务终止,行业务关闭
                FROM (
                    SELECT 
                        FID 头ID,
                        FPURCHASEORGID 组织ID,
                        FSUPPLIERID 供应商ID,
                        FJPUMDEPARTMENT 部门ID,
                        FPURCHASERID 采购员ID,
                        FBILLTYPEID 单据类型ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FDATE 采购日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCLOSESTATUS 关闭状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PUR_POORDER
                    WHERE FID IN ({0})
                ) PO
                LEFT JOIN (
                    SELECT
                        FID ID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        FUNITID 单位ID,
                        FLOT 批号ID,
                        FSEQ 项次,
                        F_JPUM_MESHXM 工单号,
                        FJPUMPRODUCT 产品名称,
                        FQTY 采购数量,
                        F_JP_QTY3 实际重量,
                        FMRPTERMINATESTATUS 行业务终止,
                        FMRPCLOSESTATUS 行业务关闭
                    FROM T_PUR_POORDERENTRY
                ) POE ON PO.头ID = POE.ID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FREQUIREDEPTID 工程ID,
                        FDELIVERYLASTDATE 计划到货日期
                    FROM T_PUR_POORDERENTRY_D
                ) POED ON POE.分录ID = POED.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FBASEAPJOINQTY 已入库数量,
                        FBASECHECKCUTPAYQTY 未入库数量,
                        FSTOCKINQTY 累计入库数量
                    FROM T_PUR_POORDERENTRY_R
                ) POER ON POE.分录ID = POER.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PUR_POORDERENTRY_LK
                ) POEL ON POE.分录ID = POEL.FENTRYID",
                ["采购入库单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,工程ID,物料ID,组织ID,
                    单位ID,批号ID,供应商ID,仓管员ID,仓库ID,仓位ID,项次,
                    产品名称,单据编号,送货单号,检验单号,
                    到货日期,创建日期,审核日期,入库日期,最后修改时间,
                    应收数量,实收数量,退料数量,报检数量,检验合格数量,
                    单据状态,作废状态,检验状态,是否勾选检验
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FSUPPLIERID 供应商ID,
                        FSTOCKERID 仓管员ID,
                        FBILLNO 单据编号,
                        FDELIVERYBILL 送货单号,
                        F_JPUM_GYSDHHXM 到货日期,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FDATE 入库日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_STK_INSTOCK
                    WHERE FID IN({0})
                ) SI
                LEFT JOIN (
                    SELECT
                        FID ID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID,
                        FUNITID 单位ID,
                        FLOT 批号ID,
                        FSTOCKID 仓库ID,
                        FSTOCKLOCID 仓位ID,
                        FSEQ 项次,
                        F_JPUM_JYDHHXM 检验单号,
                        FJPUMPRODUCT 产品名称,
                        FMUSTQTY 应收数量,
                        FREALQTY 实收数量,
                        FRETURNJOINQTY 退料数量,
                        F_JPUM_BJSLHXM 报检数量,
                        F_JPUM_JYSLHXM 检验合格数量,
                        F_JPUM_CHECKHXM 是否勾选检验,
                        FBILLJYZTHXM 检验状态
                    FROM T_STK_INSTOCKENTRY
                ) SIE ON SI.头ID = SIE.ID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_STK_INSTOCKENTRY_LK
                ) SIEL ON SIE.分录ID = SIEL.FENTRYID",
                ["采购退料单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,工程ID,物料ID,组织ID,
                    单据编号,创建日期,审核日期,最后修改时间,实退数量,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PUR_MRB
                    WHERE FID IN({0})
                ) PMR
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        FJPUMPROJECTNO 工程ID,
                        FRMREALQTY 实退数量
                    FROM T_PUR_MRBENTRY
                ) PMRE ON PMR.头ID = PMRE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PUR_MRBENTRY_LK
                ) PMRELK ON PMRE.分录ID = PMRELK.FENTRYID",
                ["生产用料清单"] = @"
                SELECT
                    头ID,分录ID,工程ID,产品ID,组织ID,BOMID,
                    生产订单ID,子项物料ID,子项单位ID,生产订单行号,项次,
                    单据编号,供应商批号,生产订单编号,物料申请单单号,

                    产品名称,研发备注,直发工地,
                    分子,分母,预采购,预委外,预库存,
                    数量,应发数量,已领数量,未领数量,
                    已申请数量,补料选单数量,退料选单数量,
                    已下委外单数量,已批号调整数量,
                    批号库存数量,累计入库数量,

                    创建日期,审核日期,最后修改时间,
                    单据状态,申请关闭,委外关闭,批号调整关闭
                FROM (
                    SELECT
                        FID 头ID,
                        FPRDORGID 组织ID,
                        FWORKSHOPID 工程ID,
                        FMATERIALID 产品ID,
                        FBOMID BOMID,
                        FMOID 生产订单ID,
                        FBILLNO 单据编号,
                        FMOBILLNO 生产订单编号,
                        FQTY 数量,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态
                    FROM T_PRD_PPBOM
                    WHERE FID IN({0})
                ) PB
                LEFT JOIN (
                    SELECT
                        FID ID,
                        FENTRYID 分录ID,
                        FMATERIALID 子项物料ID,
                        FUNITID 子项单位ID,
                        FMOENTRYSEQ 生产订单行号,
                        FSEQ 项次,
                        F_JPUM_WLSQBILLNO 物料申请单单号,
                        F_JPUM_GYSLOTHXM 供应商批号,
                        F_JPUM_COMBOHXM 直发工地,
                        FNUMERATOR 分子,
                        FDENOMINATOR 分母,
                        F_JP_PURQTY 预采购,
                        F_JP_SUBQTY 预委外,
                        F_JP_STOCKQTY 预库存,
                        FMUSTQTY 应发数量,
                        F_PAEZ_QTY 已申请数量,
                        F_JP_QTY 已下委外单数量,
                        F_JZ_QTY 已批号调整数量,
                        F_JPUM_CGRKHXM 累计入库数量,
                        F_JPUM_PHNUMHXM 批号库存数量,
                        FBILLSTATUS 申请关闭,
                        FBILLSTATUS2 委外关闭,
                        FBILLSTATUS4 批号调整关闭
                    FROM T_PRD_PPBOMENTRY
                ) PBE ON PB.头ID = PBE.ID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FMEMO 研发备注
                    FROM T_PRD_PPBOMENTRY_L
                ) PBEL ON PBE.分录ID = PBEL.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FPICKEDQTY 已领数量,
                        FNOPICKEDQTY 未领数量,
                        FSELREPICKEDQTY 补料选单数量,
                        FSELPRCDRETURNQTY 退料选单数量
                    FROM T_PRD_PPBOMENTRY_Q
                ) PBEQ ON PBE.分录ID = PBEQ.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FMATERIALID,
                        FNAME 产品名称
                    FROM T_BD_MATERIAL_L
                ) ML ON PB.产品ID = ML.FMATERIALID",
                ["生产订单"] = @"
                    SELECT
                        头ID,分录ID,源单头ID,源单分录ID,
                        工程ID,物料ID,单位ID,组织ID,项次,
                        单据编号,数量,创建日期,下达日期,审核日期,最后修改时间,
                        单据状态,作废状态,挂起状态,业务状态
                    FROM (
                        SELECT
                            FID 头ID,
                            FPRDORGID 组织ID,
                            FBILLNO 单据编号,
                            FCREATEDATE 创建日期,
                            FAPPROVEDATE 审核日期,
                            FMODIFYDATE 最后修改时间,
                            FDOCUMENTSTATUS 单据状态,
                            FCANCELSTATUS 作废状态
                        FROM T_PRD_MO
                        WHERE FID IN({0})
                    ) PMO
                    LEFT JOIN (
                        SELECT 
                            FID,
                            FENTRYID 分录ID,
                            FWORKSHOPID 工程ID,
                            FMATERIALID 物料ID,
                            FUNITID 单位ID,
                            FSEQ 项次,
                            FQTY 数量,
                            FISSUSPEND 挂起状态
                        FROM T_PRD_MOENTRY
                    ) PMOE ON PMO.头ID = PMOE.FID
                    LEFT JOIN (
                        SELECT
                            FENTRYID,
                            FSTATUS 业务状态,
                            FCONVEYDATE 下达日期
                        FROM T_PRD_MOENTRY_A
                    ) PMOEA ON PMOE.分录ID = PMOEA.FENTRYID
                    LEFT JOIN (
                        SELECT 
                            FENTRYID,
                            FSBILLID 源单头ID,
                            FSID 源单分录ID
                        FROM T_PRD_MOENTRY_LK
                    ) PMOEL ON PMOE.分录ID = PMOEL.FENTRYID",
                ["生产领料单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实发数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PRD_PICKMTRL
                    WHERE FID IN({0})
                ) PPP
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FWORKSHOPID 工程ID,
                        FMATERIALID 物料ID,
                        FACTUALQTY 实发数量
                    FROM T_PRD_PICKMTRLDATA
                ) PPPE ON PPP.头ID = PPPE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PRD_PICKMTRLDATA_LK
                ) PPPEL ON PPPE.分录ID = PPPEL.FENTRYID",
                ["生产退料单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实退数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PRD_RETURNMTRL
                    WHERE FID IN({0})
                ) PPR
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FWORKSHOPID 工程ID,
                        FMATERIALID 物料ID,
                        FQTY 实退数量
                    FROM T_PRD_RETURNMTRLENTRY
                ) PPRE ON PPR.头ID = PPRE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PRD_RETURNMTRLENTRY_LK
                ) PPREL ON PPRE.分录ID = PPREL.FENTRYID",
                ["生产补料单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实发数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_PRD_FEEDMTRL
                    WHERE FID IN({0})
                ) PPF
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FWORKSHOPID 工程ID,
                        FMATERIALID 物料ID
                    FROM T_PRD_FEEDMTRLDATA
                ) PPFE ON PPF.头ID = PPFE.FID
                LEFT JOIN (
                    SELECT
                        FENTRYID,
                        FACTUALQTY 实发数量
                    FROM T_PRD_FEEDMTRLDATA_Q
                ) PPFEQ ON PPFE.分录ID = PPFEQ.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_PRD_FEEDMTRLDATA_LK
                ) PPFEL ON PPFE.分录ID = PPFEL.FENTRYID",
                ["委外用料清单"] = @"
                SELECT
                    头ID,分录ID,工程ID,产品ID,组织ID,BOMID,
                    子项物料ID,子项单位ID,委外订单行号,项次,
                    单据编号,委外订单编号,产品名称,备注,
                    分子,分母,预采购,预委外,预库存,
                    数量,应发数量,已领数量,已申请数量,原始携带量,已批号调整数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,子项类型,申请关闭,批号调整关闭
                FROM (
                    SELECT
                        FID 头ID,
                        FMATERIALID 产品ID,
                        FSUBORGID 组织ID,
                        FBILLNO 单据编号,
                        FSUBBILLNO 委外订单编号,
                        FQTY 数量,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态
                    FROM T_SUB_PPBOM
                    WHERE FID IN({0})
                ) SB
                LEFT JOIN (
                    SELECT 
                        FID,
                        FDESCRIPTION 备注
                    FROM T_SUB_PPBOM_L
                ) SBL ON SB.头ID = SBL.FID
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 子项物料ID,
                        FUNITID 子项单位ID,
                        FBOMID BOMID,
                        FSUBREQENTRYSEQ 委外订单行号,
                        FSEQ 项次,
                        FNUMERATOR 分子,
                        FDENOMINATOR 分母,
                        F_JP_PURQTY 预采购,
                        F_JP_SUBQTY 预委外,
                        F_JP_STOCKQTY 预库存,
                        FMUSTQTY 应发数量,
                        F_JZ_QTY 已申请数量,
                        FBASEMUSTQTY 原始携带量,
                        F_JZ_QTY1 已批号调整数量,
                        FMATERIALTYPE 子项类型,
                        FBILLSTATUS 申请关闭,
                        FBILLSTATUS1 批号调整关闭
                    FROM T_SUB_PPBOMENTRY
                ) SBE ON SB.头ID = SBE.FID 
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FPICKEDQTY 已领数量
                    FROM T_SUB_PPBOMENTRY_Q
                ) SBEQ ON SBE.分录ID = SBEQ.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FMATERIALID,
                        FNAME 产品名称
                    FROM T_BD_MATERIAL_L
                ) ML ON SB.产品ID = ML.FMATERIALID",
                ["委外订单"] = @"
                    SELECT
                        头ID,分录ID,源单头ID,源单分录ID,
                        工程ID,物料ID,单位ID,组织ID,项次,
                        单据编号,数量,创建日期,下达日期,审核日期,最后修改时间,
                        单据状态,作废状态,挂起状态,业务状态
                    FROM (
                        SELECT
                            FID 头ID,
                            FSUBORGID 组织ID,
                            FBILLNO 单据编号,
                            FCREATEDATE 创建日期,
                            FAPPROVEDATE 审核日期,
                            FMODIFYDATE 最后修改时间,
                            FDOCUMENTSTATUS 单据状态,
                            FCANCELSTATUS 作废状态
                        FROM T_SUB_REQORDER
                        WHERE FID IN({0})
                    ) PSO
                    LEFT JOIN (
                        SELECT 
                            FID,
                            FENTRYID 分录ID,
                            FJPUMPROJECTNO 工程ID,
                            FMATERIALID 物料ID,
                            FUNITID 单位ID,
                            FSEQ 项次,
                            FQTY 数量,
                            FCONVEYDATE 下达日期,
                            FSTATUS 业务状态,
                            FISSUSPEND 挂起状态
                        FROM T_SUB_REQORDERENTRY
                    ) PSOE ON PSO.头ID = PSOE.FID
                    LEFT JOIN (
                        SELECT 
                            FENTRYID,
                            FSBILLID 源单头ID,
                            FSID 源单分录ID
                        FROM T_PRD_MOENTRY_LK
                    ) PSOEL ON PSOE.分录ID = PSOEL.FENTRYID",
                ["委外领料单"] = @"
                SELECT
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实发数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SUB_PICKMTRL
                    WHERE FID IN({0})
                ) SBP
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID,
                        FACTUALQTY 实发数量
                    FROM T_SUB_PICKMTRLDATA
                ) SBPE ON SBP.头ID = SBPE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_SUB_PICKMTRLDATA_LK
                ) SBPEL ON SBPE.分录ID = SBPEL.FENTRYID",
                ["委外退料单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实退数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SUB_RETURNMTRL
                    WHERE FID IN({0})
                ) SBR
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID,
                        FQTY 实退数量
                    FROM T_SUB_RETURNMTRLENTRY
                ) SBRE ON SBR.头ID = SBRE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_SUB_RETURNMTRL_LK
                ) SBREL ON SBRE.分录ID = SBREL.FENTRYID",
                ["委外补料单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实发数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSTOCKORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SUB_FEEDMTRL
                    WHERE FID IN({0})
                ) SBF
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FJPUMPROJECTNO 工程ID,
                        FMATERIALID 物料ID
                    FROM T_SUB_FEEDMTRLENTRY
                ) SBFE ON SBF.头ID = SBFE.FID
                LEFT JOIN (
                    SELECT
                        FENTRYID,
                        FACTUALQTY 实发数量
                    FROM T_SUB_FEEDMTRLENTRY_Q
                ) SBFEQ ON SBFE.分录ID = SBFEQ.FENTRYID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_SUB_FEEDMTRLENTRY_LK
                ) SBFEL ON SBFE.分录ID = SBFEL.FENTRYID",
                ["销售订单"] = @"
                SELECT 
                    头ID,分录ID,工程ID,物料ID,组织ID,
                    单据编号,销售数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSALEDEPTID 工程ID,
                        FSALEORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SAL_ORDER
                    WHERE FID IN({0})
                ) SO
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        FQTY 销售数量
                    FROM T_SAL_ORDERENTRY
                ) SOE ON SO.头ID = SOE.FID",
                ["销售出库单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实发数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSALEDEPTID 工程ID,
                        FSALEORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SAL_OUTSTOCK
                    WHERE FID IN({0})
                ) SSO
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        FREALQTY 实发数量
                    FROM T_SAL_OUTSTOCKENTRY
                ) SSOE ON SSO.头ID = SSOE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_SAL_OUTSTOCKENTRY_LK
                ) SSOEL ON SSOE.分录ID = SSOEL.FENTRYID",
                ["销售退货单"] = @"
                SELECT 
                    头ID,分录ID,源单头ID,源单分录ID,
                    工程ID,物料ID,组织ID,单据编号,实退数量,
                    创建日期,审核日期,最后修改时间,
                    单据状态,作废状态
                FROM (
                    SELECT
                        FID 头ID,
                        FSALEDEPTID 工程ID,
                        FSALEORGID 组织ID,
                        FBILLNO 单据编号,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态,
                        FCANCELSTATUS 作废状态
                    FROM T_SAL_RETURNSTOCK
                    WHERE FID IN({0})
                ) SR
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        FMATERIALID 物料ID,
                        FREALQTY 实退数量
                    FROM T_SAL_RETURNSTOCKENTRY
                ) SRE ON SR.头ID = SRE.FID
                LEFT JOIN (
                    SELECT 
                        FENTRYID,
                        FSBILLID 源单头ID,
                        FSID 源单分录ID
                    FROM T_SAL_RETURNSTOCKENTRY_LK
                ) SREL ON SRE.分录ID = SREL.FENTRYID",
                ["变更函"] = @"
                SELECT 
                    头ID,分录ID,物料ID,单位ID,组织ID,工程ID,
                    申请部门ID,申请人ID,
                    单据编号,主题,变更类型,变更内容,
                    变更原因,变更原因补充,责任部门,变更数量,
                    创建日期,审核日期,最后修改时间,单据状态
                FROM (
                    SELECT
                        FID 头ID,
                        FORGID 组织ID,
                        FJPUMPROJECTNO 工程ID,
                        FDEPTID 申请部门ID,
                        FAPPLIERID 申请人ID,
                        F_JP_BASE13 变更类型ID,
                        F_JP_BASE14 变更原因ID,
                        FBILLNO 单据编号,
                        F_JP_REMARKS1 主题,
                        F_JP_REMARKS 变更内容,
                        FREMARKS 变更原因补充,
                        F_JP_CCBGBM 责任部门,
                        FCREATEDATE 创建日期,
                        FAPPROVEDATE 审核日期,
                        FMODIFYDATE 最后修改时间,
                        FDOCUMENTSTATUS 单据状态
                    FROM JP_t_Cust_Entry100008
                    WHERE FID IN({0})
                ) SC
                LEFT JOIN (
                    SELECT
                        FID,
                        FENTRYID 分录ID,
                        F_JP_BASE6 物料ID,
                        F_JP_UNITID 单位ID,
                        F_JP_QTY 变更数量
                    FROM JP_t_Cust_Entry1000136
                ) SCE ON SC.头ID = SCE.FID
                LEFT JOIN (
                    SELECT FID,FNAME 变更类型
                    FROM JZ_t_Cust_Entry100028_L
                ) SCTL ON SCTL.FID = SC.变更类型ID
                LEFT JOIN(
                    SELECT FID,FNAME 变更原因
                    FROM JZ_t_Cust_Entry100029_L
                ) SCRL ON SCRL.FID = SC.变更原因ID"
            };
        }
        public byte[] JLoad(byte[] JMainData)
        {
            isStop = false;

            using (var db = new SqlConnection(CONNSTR_IMG))
                FormMain.NameList = db.Query<string>("SELECT 名称 FROM 监控表").ToArray();

            manThr = new Thread(Main)
            {
                IsBackground = true,
                Name = "手动"
            };
            mainThr = new Thread(Main)
            {
                IsBackground = true,
                Name = "自动"
            };
            mainThr.Start();
            return JWNormFun.ReturnLoad(
                JMainData, "e9d9c948-b832-42a7-b4e7-b43624a0dcc2"
            );
        }

        public byte[] JMeasure()
        {
            if (isManual)
            {
                isManual = false;
                manThr.Start();
            }
            return JWNormFun.ReturnMeasure();
        }

        /// <summary>
        /// 写日志，如果含有换行符，则不输出时间
        /// </summary>
        /// <param name="log">内容</param>
        /// <returns>当前对象，以进行连锁调用</returns>
        public ClassMain Log(object log)
        {
            if (log != null)
                buffer.AddLog(
                    $"{log}".EndsWith("\r\n") ?
                    $"{log}" : $"({Thread.CurrentThread.Name}) [{DateTime.Now:yy/MM/dd HH:mm:ss}] {log}"
                );
            return this;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="dbSrc">源数据库</param>
        /// <param name="dbTo">目标数据库</param>
        /// <param name="ids">更新字段</param>
        /// <returns>影响行数</returns>
        public long GetData<T>(监控表 x, SqlConnection dbFrom, SqlConnection dbTo, string ids)
        {
            string col = (x.类别 == "单据") ? "头ID" : ((x.名称 == "采购延期报表") ? "订单头ID" : "ID");
            try
            {
                var rs = dbFrom.Query<T>(string.Format(SQL[x.名称], ids));
                Log($"成功，共 {rs.Count()} 行\r\n").Log($"镜像中...");
                dbTo.Execute($"DELETE FROM {x.名称} WHERE {col} IN({ids})");
                return dbTo.Insert(rs);
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        /// <summary>
        /// 线程函数，设置定时器，并根据定时器和线程类型调用对应方法
        /// </summary>
        public void Main()
        {
            string name = Thread.CurrentThread.Name;
            DateTime Timer_2Hour = DateTime.Now.AddHours(2);
            DateTime Timer_7 = DateTime.Today.AddHours(7);
            DateTime Timer_12 = DateTime.Today.AddHours(12);
            DateTime now;

            Log($"线程启动").Log("\r\n");
            while (!isStop)
            {
                if (name == "手动")
                {
                    isDaily = false;
                    Update();
                    break;
                }
                else if (manThr.ThreadState == ThreadState.Running) manThr.Join();

                now = DateTime.Now;
                if (now.CompareTo(Timer_2Hour) >= 0)
                {
                    isDaily = false;
                    Timer_2Hour = Timer_2Hour.AddHours(2);
                    Update("单据,报表");
                }
                if (now.CompareTo(Timer_7) >= 0 && now.Hour == 7)
                {
                    isDaily = true;
                    Timer_7 = Timer_7.AddDays(1);
                    Update("基础资料,报表");
                }
                if (now.CompareTo(Timer_12) >= 0 && now.Hour == 12)
                {
                    isDaily = true;
                    Timer_12 = Timer_12.AddDays(1);
                    Update("基础资料");
                }
            }
            Log("线程结束").Log("\r\n");
        }

        /// <summary>
        /// 读监控表，按最后修改时间（创建/审核与最后修改日期取较大值）更新镜像表，写入日志表
        /// </summary>
        /// <param name="type">更新类型：基础资料，单据，报表</param>
        private void Update(string type = null)
        {
            List<镜像日志> logs = new List<镜像日志>();
            IEnumerable<dynamic> rs = null;
            string sql,ids,lastDate,lastTime,
                curType = "",
                curThrName = Thread.CurrentThread.Name;
            bool isNull,isEntry;
            long count, projId = 0;

            try
            {
                Log("连接 [中间层] ...");
                using (var dbImg = new SqlConnection(CONNSTR_IMG))
                {
                    Log("成功\r\n").Log("查询 [监控表] ...");
                    sql = "SELECT * FROM 监控表 WHERE 1=1";
                    if (curThrName == "手动")
                    {
                        if (FormMain.BillType != "全部") sql += $" AND 名称 = '{FormMain.BillType}'";
                        else if (!string.IsNullOrEmpty(FormMain.BillNo)) sql += $" AND 类别 = '单据'";
                        if (!string.IsNullOrEmpty(FormMain.ProjNo)) sql += $" OR 名称 = '工程表'";
                    }
                    else sql += $" AND 类别 IN('{type.Replace(",", "','")}')";        // 按更新类型筛选监控表

                    var img = dbImg.Query<监控表>(sql);
                    using (var dbStat = new SqlConnection(CONNSTR_STAT))
                    {
                        Log($"成功\r\n").Log("连接 [金蝶数据库] ...");
                        using (var dbKd = new SqlConnection(CONNSTR_KD))
                        {
                            Log("成功\r\n").Log("\r\n");
                            if (!string.IsNullOrEmpty(FormMain.ProjNo))             // 如果填写了工程号，则按工程号查找工程ID
                            {
                                sql = $"SELECT FDEPTID FROM T_BD_DEPARTMENT_L WHERE FNAME = '{FormMain.ProjNo}'";
                                projId = dbKd.QueryFirst<int>(sql);
                            }
                            img.AsList().ForEach(x =>                               // 遍历监控表
                            {
                                count = 0;
                                curType = x.名称;
                                Log($"[{curType}]").Log("\r\n");
                                Log($"同步历史数据...");                              // 查找金蝶表不存在，而镜像表存在的内容，并删除
                                ids = (x.名称 == "采购延期报表") ? "订单头ID" : (x.类别 == "单据") ? "头ID" : "ID";
                                sql = $@"
                                DELETE A1 FROM {((x.类别 == "报表") ? SQL["统计类报表"] : "")}{curType} A1 
                                WHERE NOT EXISTS (
                                    SELECT 1
                                    FROM {((x.类别 == "报表") ? "" : SQL["金蝶"])}{x.明细表名称} A2 
                                    WHERE A1.{ids} = A2.{x.头ID名称}
                                    {((x.类别 == "基础资料" || curType == "即时库存单") ? "--": "")} AND A1.{
                                        ids.Replace("头","分录")} = A2.{((x.类别 == "报表") ? "分录ID" : "FENTRYID")}
                                )";
                                count = dbImg.Execute(sql);

                                Log($"成功，共 {count} 行\r\n").Log($"查询头ID...");    // 根据监控表的最后修改时间，在金蝶查找在该时间之后的（单据头）ID
                                isNull = FormMain.BillNo == null && FormMain.ProjNo == null;
                                isEntry = x.工程ID名称 == "FJPUMPROJECTNO" && (x.源表名称.StartsWith("T_SUB") || x.源表名称.StartsWith("T_STK"));
                                lastTime = (x.名称 == "即时库存单" || x.类别 == "报表") ? "FUPDATETIME" : SQL[$"{x.类别}日期"];
                                lastDate = x.最后修改时间.ToString(DFMT);

                                if (curType == "采购延期报表")
                                {
                                    if (isDaily)
                                    {
                                        dbImg.Execute($"DELETE FROM {SQL["统计类报表"]}采购延期报表");
                                        sql = $@"INSERT INTO 采购延期报表 {SQL[curType].Replace("头ID IN({0}) AND ", "")
                                            .Replace("采购订单",$"{SQL["金蝶数据镜像"]}采购订单").Replace("采购入库单",$"{SQL["金蝶数据镜像"]}采购入库单")}";
                                        count = dbStat.Execute(sql);
                                    }
                                    else
                                    {
                                        sql = $@"
                                            SELECT * FROM (
                                                SELECT DISTINCT 
                                                    头ID ID,
                                                    CASE WHEN A2.最后修改时间 IS NULL THEN A1.最后修改时间
                                                        ELSE IIF(A1.最后修改时间 > A2.最后修改时间,A2.最后修改时间,A1.最后修改时间) 
                                                    END 最后修改时间
                                                FROM (
                                                    SELECT 头ID,分录ID,最后修改时间
                                                    FROM 采购订单
                                                    WHERE 作废状态 = 'A' AND 采购数量 >= 1 AND 单据状态 != 'Z'
                                                    {(string.IsNullOrEmpty(FormMain.BillNo) ? "--" : "")} AND 单据编号 = '{FormMain.BillNo}'
                                                    {((projId == 0) ? "--" : "")} AND 工程ID = {projId}
                                                ) A1
                                                LEFT JOIN (
                                                    SELECT 源单头ID,源单分录ID,最后修改时间
                                                    FROM 采购入库单
                                                    WHERE 作废状态 = 'A' AND 单据状态 != 'Z'
                                                ) A2 ON A2.源单头ID = A1.头ID AND A2.源单分录ID = A1.分录ID
                                            ) B
                                            WHERE 最后修改时间 > '{lastDate}'";
                                        rs = dbImg.Query(sql);
                                    }
                                }
                                else if (curThrName == "自动" || x.工程ID名称 == null || isNull)
                                {
                                    sql = $@"
                                    SELECT * FROM (
                                        SELECT 
                                            {x.头ID名称} ID, 
                                            {lastTime} 最后修改时间
                                        FROM {x.源表名称}
                                    ) A WHERE 最后修改时间  > '{lastDate}'";
                                    rs = dbKd.Query(sql);
                                }
                                else
                                {
                                    sql = $@"
                                        SELECT * FROM (
                                            SELECT 
                                                {x.头ID名称} ID,
                                                {((x.名称 == "工程表" || string.IsNullOrEmpty(FormMain.BillNo)) ? "--" : "")}FBILLNO 单据编号,
                                                {((isEntry || string.IsNullOrEmpty(FormMain.ProjNo)) ? "--" : "")}{x.工程ID名称} 工程ID,
                                                {lastTime} 最后修改时间
                                            FROM {x.源表名称}
                                            {((x.名称 == "工程表" || string.IsNullOrEmpty(FormMain.BillNo)) ? "" :
                                            $"WHERE FBILLNO = '{FormMain.BillNo}'")}
                                        ) A 
                                        {(isEntry ? string.Format(SQL["SEL明细工程ID"],x.工程ID名称,x.明细表名称) : "")}
                                        WHERE {((projId == 0)? $"最后修改时间 > '{lastDate}'" : $"工程ID = {projId}")}";
                                    rs = dbKd.Query(sql);
                                }
                                
                                if (rs != null && rs.Count() > 0)
                                {
                                    ids = string.Join(",",
                                        (curType == "单据类型表" || x.名称 == "即时库存单") ?
                                        rs.Select(r => $"'{r.ID}'") : rs.Select(r => r.ID)
                                    );

                                    Log($"成功，共 {rs.Count()} 行\r\n").Log($"查询新数据...");
                                    switch (curType)
                                    {
                                        case "员工表":
                                            count = GetData<员工表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "客户表":
                                            count = GetData<客户表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购员表":
                                            count = GetData<采购员表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "仓位表":
                                            count = GetData<仓位表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "仓库表":
                                            count = GetData<仓库表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "仓管员表":
                                            count = GetData<仓管员表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "单据类型表":
                                            count = GetData<单据类型表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "供应商表":
                                            count = GetData<供应商表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "工程表":
                                            count = GetData<工程表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "批号表":
                                            count = GetData<批号表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "物料表":
                                            count = GetData<物料表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "单位表":
                                            count = GetData<单位表>(x, dbKd, dbImg, ids);
                                            break;
                                        case "即时库存单":
                                            count = GetData<即时库存单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "其他入库单":
                                            count = GetData<其他入库单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "其他出库单":
                                            count = GetData<其他出库单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "批号调整单":
                                            count = GetData<批号调整单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购申请单":
                                            count = GetData<采购申请单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购订单":
                                            count = GetData<采购订单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购入库单":
                                            count = GetData<采购入库单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购退料单":
                                            count = GetData<采购退料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "生产用料清单":
                                            count = GetData<生产用料清单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "生产订单":
                                            count = GetData<生产订单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "生产领料单":
                                            count = GetData<生产领料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "生产退料单":
                                            count = GetData<生产退料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "生产补料单":
                                            count = GetData<生产补料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "委外用料清单":
                                            count = GetData<委外用料清单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "委外订单":
                                            count = GetData<委外订单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "委外领料单":
                                            count = GetData<委外领料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "委外退料单":
                                            count = GetData<委外退料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "委外补料单":
                                            count = GetData<委外补料单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "销售订单":
                                            count = GetData<销售订单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "销售出库单":
                                            count = GetData<销售出库单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "销售退货单":
                                            count = GetData<销售退货单>(x, dbKd, dbImg, ids);
                                            break;
                                        case "变更函":
                                            count = GetData<变更函>(x, dbKd, dbImg, ids);
                                            break;
                                        case "采购延期报表":
                                            count = GetData<采购延期报表>(x, dbImg, dbStat, ids);
                                            break;
                                        default:
                                            throw new Exception("未定义类型");
                                    }
                                    x.最后修改时间 = rs.Max(r => r.最后修改时间);
                                }
                                logs.Add(new 镜像日志
                                {
                                    时间 = DateTime.Now,
                                    来源 = curType,
                                    行数 = count
                                });
                                x.更新时间 = DateTime.Now;
                                Log($"成功，共 {count} 行\r\n").Log("写入 [监控表]...");
                                dbImg.Update(x);
                                Log("成功\r\n").Log("\r\n");
                                
                                rs = null;
                            });
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Log("\r\n").Log($"错误，{err.Message}").Log("\r\n");
                logs.Add(new 镜像日志
                {
                    时间 = DateTime.Now,
                    来源 = curType,
                    行数 = 0,
                    备注 = $"{err.Message}"
                });
                return;
            }
            Log("正在记录日志...");
            using (var log = new SqlConnection(CONNSTR_LOG)) log.Insert(logs);
            Log("成功\r\n");
        }

        public byte[] JShowMainForm(byte[] ddata)
        {
            string err = "";
            if (WinApi.IsWindow(pForm) != 1)
                pForm = JWNormFun.ShowForm(typeof(FormMain), null, ref err);
            else pForm = JWNormFun.ShowForm(pForm, ref err);

            return null;
        }
        public byte[] JAppRetrun(byte[] reData) { return null; }
        public byte[] JDeBug(byte[] dedata) { return null; }
        public byte[] JClose() { return JWNormFun.Closex(null); }
        public byte[] JMessage(byte[] JMsgBs) { return null; }
    }
}
