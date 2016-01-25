﻿namespace System
{
    /// <summary>
    /// 表示依赖注入采用单例模式的特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class SingletonMappingAttribute : Attribute
    {
        internal static readonly Type Type = typeof(SingletonMappingAttribute);
    }

    /// <summary>
    /// 表示依赖注入构造函数的解析排序。
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = true)]
    public class OrderMappingAttribute : Attribute
    {
        /// <summary>
        /// 设置或获取一个值，表示构造函数解析排序。数值越小优先解析。
        /// </summary>
        public int Order { get; set; }
    }
}
