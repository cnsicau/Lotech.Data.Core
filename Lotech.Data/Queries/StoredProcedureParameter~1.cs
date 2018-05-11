using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 存储过程参数
    /// </summary>
    /// <typeparam name="TParameter">参数类型</typeparam>
    class StoredProcedureParameter<TParameter> where TParameter : class
    {
        /// <summary>
        /// 编译时动态字段名定位器
        /// </summary>
        class CompiledFieldLocator
        {
            static readonly string fieldPrefix, fieldSubfix;
            static CompiledFieldLocator()
            {
                var dynamicObject = new { PropertyNameForDetectDynamicField = Guid.Empty };
                var field = dynamicObject.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).First();
                const string propertyName = nameof(dynamicObject.PropertyNameForDetectDynamicField);
                var index = field.Name.IndexOf(propertyName);

                fieldPrefix = field.Name.Substring(0, index);
                fieldSubfix = field.Name.Substring(index + propertyName.Length);
            }
            /// <summary>
            /// 通过编译生成的属性名获取其字段属性
            /// </summary>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            static public FieldInfo Locate<TObject>(string propertyName)
            {
                return typeof(TObject).GetField(fieldPrefix + propertyName + fieldSubfix
                    , BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        static readonly Action<StoredProcedureParameter<TParameter>, IDbCommand, TParameter> _bind;
        static readonly Dictionary<string, Action<TParameter, object>> _reverseMapping = new Dictionary<string, Action<TParameter, object>>(StringComparer.CurrentCultureIgnoreCase);

        static StoredProcedureParameter()
        {
            Action<StoredProcedureParameter<TParameter>, IDbCommand, string, object> addParameter = AddParameter;
            var properties = typeof(TParameter).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var thisArg = Expression.Parameter(typeof(StoredProcedureParameter<TParameter>), "this");
            var commandArg = Expression.Parameter(typeof(IDbCommand), "command");
            var parameterArg = Expression.Parameter(typeof(TParameter), "parameter");

            var blocks = new List<Expression>();
            foreach (var property in properties)
            {
                // AddParameter(this, command, Prop.Name, parameter.Prop);
                blocks.Add(
                    Expression.Call(addParameter.Method, thisArg, commandArg, Expression.Constant(property.Name)
                        , Expression.Convert(Expression.MakeMemberAccess(parameterArg, property), typeof(object)))
                );
                var field = CompiledFieldLocator.Locate<TParameter>(property.Name);
                _reverseMapping.Add(property.Name, (_, value) => field.SetValue(_, value));
            }

            var expression = Expression.Lambda<Action<StoredProcedureParameter<TParameter>, IDbCommand, TParameter>>(
                Expression.Block(blocks)
                , thisArg, commandArg, parameterArg
            );
            _bind = expression.Compile();
        }

        private readonly TParameter _parameter;
        private readonly Action<string> _log;
        private readonly Func<object, object> _valueConverter;
        private IDbCommand _command;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="log"></param>
        /// <param name="valueConverter"></param>
        internal StoredProcedureParameter(TParameter parameter, Action<string> log, Func<object, object> valueConverter)
        {
            _parameter = parameter;
            _log = log;
            _valueConverter = valueConverter;
        }

        static void AddParameter(StoredProcedureParameter<TParameter> sp, IDbCommand command, string parameterName, object value)
        {
            sp._log(string.Format("  :{0}\t=\t{1}", parameterName, value));

            var parameter = command.CreateParameter();
            parameter.ParameterName = parameterName;
            parameter.Value = value ?? DBNull.Value;
            parameter.Direction = ParameterDirection.InputOutput;

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// 将值应用到参数
        /// </summary>
        /// <param name="command"></param>
        public void Binding(IDbCommand command)
        {
            _command = command;
            if (_parameter != null)  // 参数非默认值时绑定
                _bind(this, command, _parameter);
        }

        /// <summary>
        /// 将参数值反向绑定到参数对象中
        /// </summary>
        public void ReverseBinding()
        {
            foreach (DbParameter parameter in _command.Parameters)
            {
                Action<TParameter, object> reverse;
                if (_reverseMapping.TryGetValue(parameter.ParameterName, out reverse))
                    reverse(_parameter, parameter.Value);
            }
        }
    }
}
