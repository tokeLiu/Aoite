﻿//using System;
//using Aoite.Data;

//namespace System
//{
//    /// <summary>
//    /// 定义一个事务。
//    /// </summary>
//    public interface ITransaction : IDisposable
//    {
//        /// <summary>
//        /// 提交事务。
//        /// </summary>
//        void Commit();
//    }
//}

//namespace Aoite.CommandModel
//{
//    /// <summary>
//    /// 表示一个默认的事务。
//    /// </summary>
//    public sealed class DefaultTransaction : ITransaction
//    {
//        private System.Transactions.TransactionScope _t;

//        /// <summary>
//        /// 初始化一个 <see cref="DefaultTransaction"/> 类的新实例。
//        /// </summary>
//        public DefaultTransaction()
//        {
//            _t = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
//        }

//        void ITransaction.Commit() => _t.Complete();

//        void IDisposable.Dispose() => _t.Dispose();
//    }
//}
