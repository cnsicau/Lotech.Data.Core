using Lotech.Data.Descriptors;
using Lotech.Data.Utils;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lotech.Data.Queries
{
    class ProcedureQuery : Query, IProcedureQuery
    {
        static class ParameterBinder<TParameter> where TParameter : class
        {
            internal static readonly Action<List<KeyValuePair<string, object>>, TParameter> Func;
            static ParameterBinder()
            {
                var members = typeof(TParameter).GetMembers();

                var parameters = Expression.Parameter(typeof(List<KeyValuePair<string, object>>), "parameters");
                var parameter = Expression.Parameter(typeof(TParameter), "parameter");

                var add = typeof(List<KeyValuePair<string, object>>).GetMethod(nameof(List<KeyValuePair<string, object>>.Add)
                        , new Type[] { typeof(KeyValuePair<string, object>) });

                var addParameters = new List<Expression>();

                foreach (var member in members)
                {
                    Type memberValueType;
                    if (member.MemberType == MemberTypes.Field) { memberValueType = ((FieldInfo)member).FieldType; }
                    else if (member.MemberType == MemberTypes.Property && ((PropertyInfo)member).CanRead)
                    {
                        memberValueType = ((PropertyInfo)member).PropertyType;
                    }
                    else continue;

                    Expression value = Expression.MakeMemberAccess(parameter, member);
                    if (memberValueType.IsValueType) value = Expression.Convert(value, typeof(object));

                    addParameters.Add(
                        Expression.Call(parameters, add,
                            Expression.New(typeof(KeyValuePair<string, object>).GetConstructors()[0], Expression.Constant(member.Name), value))
                    );
                }

                Func = Expression.Lambda<Action<List<KeyValuePair<string, object>>, TParameter>>(
                        Expression.Block(addParameters), parameters, parameter).Compile();
            }
        }

        readonly List<KeyValuePair<string, object>> _parameters = new List<KeyValuePair<string, object>>();

        internal ProcedureQuery(IDatabase database) : base(database) { }

        internal ProcedureQuery(IDatabase database, string name) : base(database)
        {
            Name = name;
        }

        public string Name { get; set; }

        IProcedureQuery IProcedureQuery.AddParameter(string name, object value)
        {
            _parameters.Add(new KeyValuePair<string, object>(name, value));
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TParameter"></typeparam>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IProcedureQuery AddParameters<TParameter>(TParameter parameter) where TParameter : class
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            ParameterBinder<TParameter>.Func(_parameters, parameter);
            return this;
        }

        public override DbCommand CreateCommand()
        {
            var command = database.GetStoredProcedureCommand(Name);
            foreach (var p in _parameters)
            {
                var type = p.Value?.GetType() ?? typeof(string);
                database.AddInParameter(command, database.BuildParameterName(p.Key), database.ParseDbType(type), p.Value);
            }
            return command;
        }
    }
}
