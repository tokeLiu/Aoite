﻿using System;

namespace Aoite.CommandModel
{
    /// <summary>
    /// 表示一个模拟命令模型的事件。
    /// </summary>
    /// <typeparam name="TCommand">命令模型的数据类型。</typeparam>
    public class MockEvent<TCommand> : IEvent<TCommand> where TCommand : ICommand
    {
        CommandExecutingHandler<TCommand> _executing;
        CommandExecutedHandler<TCommand> _executed;

        /// <summary>
        /// 初始化一个 <see cref="MockEvent{TCommand}"/> 类的新实例。
        /// </summary>
        /// <param name="executing">命令模型执行前发生的方法。可以为 null 值。</param>
        /// <param name="executed">命令模型执行后发生的方法。可以为 null 值。</param>
        public MockEvent(CommandExecutingHandler<TCommand> executing, CommandExecutedHandler<TCommand> executed)
        {
            this._executing = executing;
            this._executed = executed;
        }

        /// <summary>
        /// 命令模型执行前发生的方法。
        /// </summary>
        /// <param name="context">执行的上下文。</param>
        /// <param name="command">执行的命令模型。</param>
        public virtual bool RaiseExecuting(IContext context, TCommand command)
            => this._executing == null || this._executing(context, command);

        /// <summary>
        /// 命令模型执行后发生的方法。
        /// </summary>
        /// <param name="context">执行的上下文。</param>
        /// <param name="command">执行的命令模型。</param>
        /// <param name="exception">抛出的异常。</param>
        public virtual void RaiseExecuted(IContext context, TCommand command, Exception exception)
            => this._executed?.Invoke(context, command, exception);
    }
}
