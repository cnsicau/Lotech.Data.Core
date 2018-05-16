using System;
using System.Data;

namespace Lotech.Data.Descriptors
{
    class MemberTuple<TEntity> where TEntity : class
    {
        internal MemberTuple(string name, DbType dbType, string parameterName
            , Func<TEntity, object> getter = null
            , Action<TEntity, object> setter = null)
        {
            Name = name;
            DbType = dbType;
            ParameterName = parameterName;
            Getter = getter;
            Setter = setter;
        }

        internal MemberTuple(string name, string parameterName)
        {
            Name = name;
            ParameterName = parameterName;
        }

        internal string Name { get; }

        internal string ParameterName { get; }

        internal DbType DbType { get; }

        internal Func<TEntity, object> Getter { get; }

        internal Action<TEntity, object> Setter { get; }
    }
}