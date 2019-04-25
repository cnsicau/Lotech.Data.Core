using Lotech.Data.Descriptors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 实体结果映射
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityResultMapper<TEntity> : ResultMapper<TEntity> where TEntity : class
    {
        class MapContext
        {
            public MapContext(int fieldIndex, string memberName, Type memberType)
            {
                FieldIndex = fieldIndex;
                MemberName = memberName;
                MemberType = memberType;
            }

            public int FieldIndex { get; }

            public string MemberName { get; }

            public Type MemberType { get; }
        }

        delegate TEntity MapDelegate(IDataRecord record, out object val, out MapContext context);

        static Tuple<string[], IDescriptorProvider, MapDelegate>[] maps = new Tuple<string[], IDescriptorProvider, MapDelegate>[2];
        static volatile int bound;

        static bool Equals(IDataReader reader, IDescriptorProvider provider, Tuple<string[], IDescriptorProvider, MapDelegate> map)
        {
            if (provider == map.Item2
                && reader.FieldCount == map.Item1.Length
                && reader.GetName(0) == map.Item1[0])
            {
                for (int i = reader.FieldCount - 1; i > 0; i--)
                {
                    if (reader.GetName(i) != map.Item1[i]) return false;
                }
                return true;
            }
            return false;
        }

        static MapDelegate GetOrCreateMapDelegate(IDataReader reader, IDescriptorProvider provider)
        {
            for (int i = 0; i < bound; i++)
            {
                if (Equals(reader, provider, maps[i])) return maps[i].Item3;
            }
            lock (maps)
            {
                for (int i = 0; i < bound; i++)
                {
                    if (Equals(reader, provider, maps[i])) return maps[i].Item3;
                }
                if (bound == maps.Length)
                {
                    var newContainer = new Tuple<string[], IDescriptorProvider, MapDelegate>[bound + 2];
                    Array.Copy(maps, newContainer, bound);
                    maps = newContainer;
                }

                try
                {
                    return (maps[bound] = CreateMapDelegate(reader, provider)).Item3;
                }
                finally
                {
                    bound++;
                }
            }
        }

        private static Tuple<string[], IDescriptorProvider, MapDelegate> CreateMapDelegate(IDataReader reader, IDescriptorProvider provider)
        {
            var fields = new string[reader.FieldCount];
            var members = provider.GetEntityDescriptor<TEntity>(Operation.None).Members;

            var record = Expression.Parameter(typeof(IDataRecord), "record");
            var value = Expression.Parameter(typeof(object).MakeByRefType(), "value");
            var context = Expression.Parameter(typeof(MapContext).MakeByRefType(), "context");

            var blocks = new List<Expression>();
            var ret = Expression.Variable(typeof(TEntity), "ret");
            blocks.Add(Expression.Assign(ret, Expression.New(typeof(TEntity))));

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = reader.GetName(i);
                foreach (var member in members)
                {
                    if (member.Name.Equals(fields[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        var valueType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;

                        var mapContext = new MapContext(i, member.Name, member.Type);
                        var fieldExpression = Expression.Constant(i);
                        blocks.Add(Expression.Assign(value, Expression.Call(record,
                                typeof(IDataRecord).GetMethod(nameof(IDataRecord.GetValue)), fieldExpression)
                            ));
                        blocks.Add(Expression.Assign(context, Expression.Constant(mapContext)));

                        var to = typeof(Convert).GetMethod("To" + valueType.Name, new[] { typeof(object) });

                        var isDBNullExpression = Expression.Call(
                                    record, typeof(IDataRecord).GetMethod(nameof(IDataRecord.IsDBNull)), fieldExpression);
                        var valueExpression = to != null ? (Expression)Expression.Call(to, value)
                            : Expression.ConvertChecked(Expression.Call(
                                        typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) }),
                                        value,
                                        Expression.Constant(valueType)), valueType);
                        blocks.Add(Expression.IfThen(Expression.Not(isDBNullExpression),
                            Expression.Assign(
                                Expression.MakeMemberAccess(ret, member.Member),
                                    valueType == member.Type ? valueExpression
                                        : Expression.ConvertChecked(valueExpression, member.Type)
                            )));
                        break;
                    }
                }
            }
            var label = Expression.Label(typeof(TEntity));

            blocks.Add(Expression.Return(label, ret));
            blocks.Add(Expression.Label(label, ret));

            var map = Expression.Lambda<MapDelegate>(Expression.Block(typeof(TEntity), new[] { ret }, blocks), record, value, context).Compile();
            return Tuple.Create(fields, provider, map);
        }

        MapDelegate map;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public override void TearUp(IDataReader reader)
        {
            base.TearUp(reader);
            map = GetOrCreateMapDelegate(reader, Database.DescriptorProvider);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool MapNext(out TEntity result)
        {
            if (!reader.Read())
            {
                result = null;
                return false;
            }

            MapContext context = null;
            object value = null;
            try
            {
                result = map(reader, out value, out context);
                return true;
            }
            catch (Exception exception)
            {
                throw new InvalidCastException($"列 {context.MemberName} 映射失败，值“{value}”({value?.GetType() })无法转换为 {context.MemberType}", exception);
            }
        }
    }
}
