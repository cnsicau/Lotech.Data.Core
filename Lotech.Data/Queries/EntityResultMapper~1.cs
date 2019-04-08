using Lotech.Data.Descriptors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 实体结果映射
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityResultMapper<TEntity> : IResultMapper<TEntity> where TEntity : class
    {
        static readonly Func<TEntity> New = Expression.Lambda<Func<TEntity>>(Expression.New(typeof(TEntity))).Compile();

        /// <summary>
        /// 映射描述子
        /// </summary>
        class Mapping
        {
            /// <summary>
            /// 数据列索引
            /// </summary>
            public int ColumnIndex { get; set; }
            /// <summary>
            /// 成员名称
            /// </summary>
            public string MemberName { get; set; }

            /// <summary>
            /// 成员值类型名称，如 decimal?
            /// </summary>
            public string MemberValueType { get; set; }

            /// <summary>
            /// 映射处理
            /// </summary>
            public Action<TEntity, object> Execute { get; set; }
        }

        static class MappingFactory
        {
            static MappingContainer[] conainers = new MappingContainer[2];
            static volatile int bound = 0;

            internal static Mapping[] Create(MappingContainer container)
            {
                for (int i = 0; i < bound; i++)
                {
                    var key = conainers[i];
                    if (key.Equals(container)) return key.Mappings;
                }
                lock (conainers)
                {
                    for (int i = 0; i < bound; i++)
                    {
                        var key = conainers[i];
                        if (key.Equals(container)) return key.Mappings;
                    }
                    if (bound == conainers.Length)
                    {
                        var newContainer = new MappingContainer[bound + 2];
                        Array.Copy(conainers, newContainer, bound);
                        conainers = newContainer;
                    }
                    var mappings = CreateEntityMapContainer(container);
                    container.Strip(mappings);
                    conainers[bound++] = container;
                    return mappings;
                }
            }

            /// <summary>
            /// 生成动态属性映射代理方法
            /// </summary>
            /// <param name="member"></param>
            /// <returns></returns>
            static Action<TEntity, object> CreateMemberMap(IMemberDescriptor member)
            {
                var valueType = Nullable.GetUnderlyingType(member.Type) ?? member.Type;
                var typeName = Type.GetTypeCode(valueType).ToString();

                var convert = typeof(Convert).GetMethod("To" + typeName, new Type[] { typeof(object) });

                var entity = Expression.Parameter(typeof(TEntity));
                var val = Expression.Parameter(typeof(object));
                return Expression.Lambda<Action<TEntity, object>>(
                        Expression.IfThen(Expression.ReferenceNotEqual(Expression.Constant(null), val),
                            Expression.Assign(
                                    Expression.MakeMemberAccess(
                                            entity, member.Member
                                        ),
                                    convert == null ? Expression.Convert(val, member.Type)
                                        : valueType == member.Type ? Expression.Call(convert, val)
                                        : (Expression)Expression.Convert(Expression.Call(convert, val), member.Type)
                                )
                            )
                        , entity, val
                    ).Compile();
            }

            static Mapping[] CreateEntityMapContainer(MappingContainer sourceKey)
            {
                var members = sourceKey.DescriptorProvider.GetEntityDescriptor<TEntity>(Operation.None).Members;
                var mappings = new List<Mapping>();
                var source = sourceKey.Source;
                // 分析需要映射列集合（实体中、Reader中共有的列）
                for (int columnIndex = 0; columnIndex < sourceKey.Source.ColumnCount; columnIndex++)
                {
                    var column = source.GetColumnName(columnIndex);
                    foreach (var member in members)
                    {
                        if (member.Name.Equals(column, StringComparison.InvariantCultureIgnoreCase))
                        {
                            mappings.Add(new Mapping
                            {
                                MemberName = member.Name,
                                MemberValueType = member.Type.ToString(),
                                Execute = CreateMemberMap(member),
                                ColumnIndex = columnIndex
                            });
                            break;
                        }
                    }
                }
                return mappings.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class MappingContainer
        {
            private readonly IDescriptorProvider descriptorProvider;
            private IResultSource source;
            private Mapping[] mappings;

            /// <summary>
            /// 
            /// </summary>
            public IDescriptorProvider DescriptorProvider { get { return descriptorProvider; } }
            /// <summary>
            /// 
            /// </summary>
            public IResultSource Source { get { return source; } }

            public Mapping[] Mappings { get { return mappings; } }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="descriptorProvider"></param>
            /// <param name="source"></param>
            public MappingContainer(IDescriptorProvider descriptorProvider, IResultSource source)
            {
                if (source.ColumnCount == 0) throw new NotSupportedException("columns is empty");
                this.descriptorProvider = descriptorProvider;
                this.source = source;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                var key = obj as MappingContainer;
                if (key != null && descriptorProvider == key.descriptorProvider
                        && source.ColumnCount == key.source.ColumnCount
                        && source.GetColumnName(0) == key.source.GetColumnName(0))
                {
                    for (int i = source.ColumnCount - 1; i > 0; i--)
                    {
                        if (source.GetColumnName(i) != key.source.GetColumnName(i)) return false;
                    }
                    return true;
                }
                return false;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public override int GetHashCode()
            {
                return descriptorProvider.GetHashCode() ^ source.ColumnCount
                    ^ source.GetColumnName(0).GetHashCode();
            }

            /// <summary>
            /// 脱离底层源(如关联的DbReader)，以便作为缓存key
            /// </summary>
            public void Strip(Mapping[] mappings)
            {
                this.mappings = mappings;
                source = new StripResultSource(source);
            }

            private class StripResultSource : IResultSource
            {
                private int columnCount;
                private string[] columns;
                private Type[] columnTypes;

                internal StripResultSource(IResultSource source)
                {
                    columnCount = source.ColumnCount;
                    columns = new string[columnCount];
                    columnTypes = new Type[columnCount];
                    for (int i = 0; i < columnCount; i++)
                    {
                        columns[i] = source.GetColumnName(i);
                        columnTypes[i] = source.GetColumnType(i);
                    }
                }

                public int ColumnCount => columnCount;

                public void Dispose() { }

                public string GetColumnName(int index) => columns[index];

                public Type GetColumnType(int columnIndex) => columnTypes[columnIndex];

                public object GetColumnValue(int index) { throw new NotSupportedException(); }

                public bool Next() { return false; }
            }
        }

        #region Fields & Constructor
        private IEnumerable<Mapping> mappings;
        private IDatabase database;
        private IResultSource source;

        /// <summary>
        /// 获取关联库
        /// </summary>
        public IDatabase Database
        {
            get { return database; }
            set { database = value; }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="source"></param>
        public void TearUp(IResultSource source)
        {
            this.source = source;
            var sourceKey = new MappingContainer(database.DescriptorProvider, source);
            mappings = MappingFactory.Create(sourceKey);
        }

        /// <summary>
        /// 映射下一结果
        /// </summary>
        /// <returns></returns>
        public bool MapNext(out TEntity result)
        {
            if (!source.Next())
            {
                result = default(TEntity);
                return false;
            }

            result = New();
            var enumerator = mappings.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                    enumerator.Current.Execute(result, source.GetColumnValue(enumerator.Current.ColumnIndex));
                return true;
            }
            catch (Exception e)
            {
                enumerator.Dispose();
                throw new MapFailedException(enumerator.Current, source.GetColumnValue(enumerator.Current.ColumnIndex), e);
            }
        }
        #endregion

        class MapFailedException : InvalidCastException
        {
            public MapFailedException(Mapping description, object value, Exception exception)
                : base($"{description.MemberName} 列映射失败，值“{value}”({value?.GetType() })对于类型 {description.MemberValueType} 无效", exception)
            {
                this.Value = value;
            }

            public object Value { get; private set; }
        }
    }
}
