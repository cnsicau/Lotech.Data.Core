using System;
using System.Data.Common;
using System.Linq;
using Lotech.Data.Descriptors;
using Lotech.Data.Utils;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 存储过程参数封装
    /// </summary>
    /// <typeparam name="TParameter"></typeparam>
    class ProcedureParameter<TParameter> where TParameter : class
    {
        static readonly Action<IDatabase, DbCommand, TParameter>[] binders = AttributeDescriptorFactory.Create<TParameter>().Members
            .Select(member =>
            {
                var get = MemberAccessor<TParameter, object>.GetGetter(member.Member);
                return new Action<IDatabase, DbCommand, TParameter>((db, command, parameter) =>
                    db.AddInParameter(command, db.BuildParameterName(member.Name), member.DbType, get(parameter)));
            }).ToArray();


        private readonly IDatabase db;
        private readonly TParameter parameter;

        public ProcedureParameter(IDatabase db, TParameter parameter)
        {
            this.db = db;
            this.parameter = parameter;
        }

        /// <summary>
        /// 绑定值到命令中
        /// </summary>
        /// <param name="command"></param>
        public void BindCommandParameters(DbCommand command)
        {
            foreach (var binder in binders)
            {
                binder(db, command, parameter);
            }
        }
    }
}
