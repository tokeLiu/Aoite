﻿using Aoite.Data;
using System;
using System.Collections.Specialized;

namespace Aoite.CommandModel
{
    /// <summary>
    /// 表示一个执行命令模型的上下文。
    /// </summary>
    public class Context : CommandModelContainerProviderBase, IContext
    {
        /// <summary>
        /// 获取正在执行的命令模型。
        /// </summary>
        public ICommand Command { get; }
        
        /// <summary>
        /// 获取执行命令模型的用户。该属性可能返回 null 值。
        /// </summary>
        public virtual dynamic User { get { return this.Container.GetUser(); } }

        private Lazy<HybridDictionary> _LazyData;
        /// <summary>
        /// 获取执行命令模型的其他参数，参数名称若为字符串则不区分大小写的序号字符串比较。
        /// </summary>
        public virtual HybridDictionary Data { get { return _LazyData.Value; } }

        private Lazy<IDbEngine> _LazyEngine;
        /// <summary>
        /// 获取上下文中的 <see cref="IDbEngine"/> 实例。
        /// <para>* 不应在执行器中开启事务。</para>
        /// </summary>
        public IDbEngine Engine { get { return _LazyEngine.Value; } }

        /// <summary>
        /// 初始化一个 <see cref="Context"/> 类的新实例。
        /// </summary>
        /// <param name="container">服务容器。</param>
        /// <param name="command">命令模型。</param>
        /// <param name="lazyData">延迟模式的命令模型的其他参数。</param>
        /// <param name="lazyEngine">延迟模式的上下文中的 <see cref="IDbEngine"/> 实例。</param>
        public Context(IIocContainer container, ICommand command, Lazy<HybridDictionary> lazyData, Lazy<IDbEngine> lazyEngine)
            : base(container)
        {
            if(command == null) throw new ArgumentNullException(nameof(command));
            if(lazyData == null) throw new ArgumentNullException(nameof(lazyData));
            if(lazyEngine == null) throw new ArgumentNullException(nameof(lazyEngine));

            this.Command = command;
            this._LazyData = lazyData;
            this._LazyEngine = lazyEngine;
        }

        /// <summary>
        /// 获取或设置键的值。
        /// </summary>
        /// <param name="key">键。</param>
        /// <returns>给定键的值。</returns>
        public object this[object key] { get { return Data[key]; } set { Data[key] = value; } }
    }
}
