using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ApiDoc.ApiDocEnum
{
    /// <summary>
    /// 字段状态-必填或非必填
    /// </summary>
    public enum FiledStatus
    {
        必填 = 1,
        非必填 = 0
    }

    /// <summary>
    /// 对象属性的类型
    /// </summary>
    public enum MethodType
    {
        /// <summary>
        /// 【引用类型】类/实体
        /// </summary>
        Class = 1,
        /// <summary>
        /// 【引用类型】集合对象（不包含字典类）
        /// </summary>
        List = 2,
        /// <summary>
        /// 【值类型】值类型-string,int,double,decimal等
        /// </summary>
        Val = 3,
        /// <summary>
        /// 【引用类型/集合】值类型的集合（string,int,double,decimal等）
        /// </summary>
        ValList = 4,
    }

    /// <summary>
    /// 每个描述对应的类型
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 方法
        /// </summary>
        M,
        /// <summary>
        /// 属性
        /// </summary>
        P,
        /// <summary>
        /// 字段
        /// </summary>
        F,
        /// <summary>
        /// 类/实体
        /// </summary>
        T,
    }


    public enum MemberInfoName
    {
        Class,
        String,
        Int,
        Int16,
        Int32,
        Int64,
        Float,
        Double,
        Decimal,
        Bool,
        DateTime,
        Object, 
        List,
    }
}