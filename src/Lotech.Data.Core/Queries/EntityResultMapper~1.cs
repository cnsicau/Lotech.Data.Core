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
    class EntityResultMapper<TEntity> : IResultMapper<TEntity> where TEntity : class
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

        static bool Equals(IDataRecord record, IDescriptorProvider provider, Tuple<string[], IDescriptorProvider, MapDelegate> map)
        {
            if (provider == map.Item2
                && record.FieldCount == map.Item1.Length
                && record.GetName(0) == map.Item1[0])
            {
                for (int i = record.FieldCount - 1; i > 0; i--)
                {
                    if (record.GetName(i) != map.Item1[i]) return false;
                }
                return true;
            }
            return false;
        }

        static MapDelegate GetOrCreateMapDelegate(IDataRecord record, IDescriptorProvider provider)
        {
            for (int i = 0; i < bound; i++)
            {
                if (Equals(record, provider, maps[i])) return maps[i].Item3;
            }
            lock (maps)
            {
                for (int i = 0; i < bound; i++)
                {
                    if (Equals(record, provider, maps[i])) return maps[i].Item3;
                }
                if (bound == maps.Length)
                {
                    var newContainer = new Tuple<string[], IDescriptorProvider, MapDelegate>[bound + 2];
                    Array.Copy(maps, newContainer, bound);
                    maps = newContainer;
                }

                try
                {
                    return (maps[bound] = CreateMapDelegate(record, provider)).Item3;
                }
                finally
                {
                    bound++;
                }
            }
        }

        private static Tuple<string[], IDescriptorProvider, MapDelegate> CreateMapDelegate(IDataRecord dataRecord, IDescriptorProvider provider)
        {
            var fields = new string[dataRecord.FieldCount];
            var members = provider.GetEntityDescriptor<TEntity>(Operation.None).Members;

            var record = Expression.Parameter(typeof(IDataRecord), "record");
            var value = Expression.Parameter(typeof(object).MakeByRefType(), "value");
            var context = Expression.Parameter(typeof(MapContext).MakeByRefType(), "context");

            var blocks = new List<Expression>();
            var ret = Expression.Variable(typeof(TEntity), "ret");
            blocks.Add(Expression.Assign(ret, Expression.New(typeof(TEntity))));

            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = dataRecord.GetName(i);
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
                        var func = typeof(Utils.Convert<>).MakeGenericType(member.Type)
                            .GetMethod(nameof(Utils.Convert<bool>.CreateFromExpression));
                        blocks.Add(Expression.Assign(
                                Expression.MakeMemberAccess(ret, member.Member),
                                (Expression)func.Invoke(null, new object[] { value })
                            ));
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

        object IResultMapper<TEntity>.TearUp(IDatabase database, IDataRecord record)
        {
            return GetOrCreateMapDelegate(record, database.DescriptorProvider);
        }

        TEntity IResultMapper<TEntity>.Map(IDataRecord record, object tearState)
        {
            MapContext context = null;
            object value = null;
            try
            {
                return ((MapDelegate)tearState)(record, out value, out context);
            }
            catch (Exception exception)
            {
                throw new InvalidCastException($"列 {context.MemberName} 映射失败，值“{value}”({value?.GetType() })无法转换为 {context.MemberType}", exception);
            }
        }

        void IResultMapper<TEntity>.TearDown(object tearState) { }
    }
}
