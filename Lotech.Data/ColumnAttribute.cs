﻿using System;
using System.Data;

namespace Lotech.Data
{
    /// <summary>
    /// 列注释
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        public ColumnAttribute() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbType"></param>
        public ColumnAttribute(DbType dbType) { DbType = dbType; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ColumnAttribute(string name) : this(name, false) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbType"></param>
        public ColumnAttribute(string name, DbType dbType) : this(name, false, dbType) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKey"></param>
        public ColumnAttribute(string name, bool primaryKey)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            PrimaryKey = primaryKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="primaryKey"></param>
        /// <param name="dbType"></param>
        public ColumnAttribute(string name, bool primaryKey, DbType dbType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));

            PrimaryKey = primaryKey;
            DbType = dbType;
        }

        /// <summary>
        /// 列名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 是否主键
        /// </summary>
        public bool PrimaryKey { get; set; }

        /// <summary>
        /// ADO.NET DB类型
        /// </summary>
        public DbType? DbType { get; set; }

        /// <summary>
        /// 是否由库生成，如：自增长、计算列
        /// </summary>
        public bool DbGenerated { get; set; }

        /// <summary>
        /// 该成员忽略参与数据的任何操作，包含但不限于SELECT 、 DELETE、 INSERT、 UPDATE 
        /// </summary>
        public bool Ignore { get; set; }
    }
}
