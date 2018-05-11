using Lotech.Data.Descriptors;
using Lotech.Data.Utils;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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
        /// 映射值
        /// </summary>
        /// <param name="entity">目标实体</param>
        /// <param name="source">数据源</param>
        /// <param name="column">列序号</param>
        /// <param name="convert">转换方法</param>
        delegate void MapReaderValueDelegate(TEntity entity, IResultSource source, int column, ValueConverter.ConvertDelegate convert);

        /// <summary>
        /// 映射描述子
        /// </summary>
        class MapDescriptor
        {
            /// <summary>
            /// 成员名称
            /// </summary>
            public string MemberName { get; set; }
            /// <summary>
            /// 成员值类型名称，如 decimal?
            /// </summary>
            public string MemberValueType { get; set; }
            /// <summary>
            /// 成员类型 Property 、 Field
            /// </summary>
            public MemberTypes MemberType { get; set; }
            /// <summary>
            /// 映射处理
            /// </summary>
            public MapReaderValueDelegate Map { get; set; }
            /// <summary>
            /// 值类型
            /// </summary>
            public Type ValueType { get; set; }
        }
        #region Static Members

        /// <summary>
        /// 生成动态属性映射代理方法
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        static MapReaderValueDelegate CreateMapDelegate(MemberInfo member)
        {
            var entityParameter = Expression.Parameter(typeof(TEntity), "entity");
            var sourceParameter = Expression.Parameter(typeof(IResultSource), "source");
            var columnParameter = Expression.Parameter(typeof(int), "column");
            var convertParameter = Expression.Parameter(typeof(ValueConverter.ConvertDelegate), "convert");
            Func<IResultSource, int, ValueConverter.ConvertDelegate, object> readValue = ReadValue;

            var memberValueType = member.MemberType == MemberTypes.Property
                                    ? ((PropertyInfo)member).PropertyType
                                    : ((FieldInfo)member).FieldType;

            // entity.MEMBER = (MemberValueType)ReadValue(memberType, realType, source, columnIndex);
            return Expression.Lambda<MapReaderValueDelegate>(
                    Expression.Assign(
                        Expression.MakeMemberAccess(entityParameter, member),
                        Expression.Convert(
                            Expression.Call(readValue.Method,
                                sourceParameter,
                                columnParameter,
                                convertParameter
                            ),
                            memberValueType)
                    )
                , entityParameter, sourceParameter, columnParameter, convertParameter).Compile();
        }

        /// <summary>
        /// 若是Nullable，返回其实际类型，否则返回当前类型
        /// </summary>
        /// <param name="valueType"></param>
        /// <returns></returns>
        static Type GetUnderlyingOrCurrentType(Type valueType)
        {
            return Nullable.GetUnderlyingType(valueType) ?? valueType;
        }

        /// <summary>
        /// 读取DataReader指定列的值
        /// </summary>
        /// <param name="source">源</param>
        /// <param name="column">列序号</param>
        /// <param name="convert">值转换器</param>
        /// <returns></returns>
        static object ReadValue(IResultSource source, int column, ValueConverter.ConvertDelegate convert)
        {
            return convert(source[column]);
        }


        #endregion

        #region Fields & Constructor
        IReadOnlyDictionary<string, MapDescriptor> memberDescriptors;
        /// <summary>
        /// 
        /// </summary>
        public EntityResultMapper()
            : this(AttributeDescriptorFactory.Create<TEntity>()) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityDescriptor"></param>
        public EntityResultMapper(EntityDescriptor entityDescriptor)
        {
            if (entityDescriptor == null) throw new ArgumentNullException(nameof(entityDescriptor));

            var entityDescriptors = new Dictionary<string, MapDescriptor>(StringComparer.CurrentCultureIgnoreCase);

            foreach (var member in entityDescriptor.Members)
            {
                MapDescriptor descriptor = new MapDescriptor();
                descriptor.MemberName = member.Name;
                descriptor.MemberType = member.Member.MemberType;
                descriptor.MemberValueType = member.Type.ToString();
                descriptor.ValueType = member.Type;
                descriptor.Map = CreateMapDelegate(member.Member);

                entityDescriptors[member.Name] = descriptor;
            }
            this.memberDescriptors = entityDescriptors;
        }
        #endregion

        #region Instance Members
        private bool initialized = false;

        private KeyValuePair<int, MapDescriptor>[] mappers;
        private ValueConverter.ConvertDelegate[] converts;

        private IResultSource source;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="source"></param>
        public void TearUp(IResultSource source)
        {
            this.source = source;
        }

        void Initialize()
        {
            var mappers = new List<KeyValuePair<int, MapDescriptor>>();
            converts = new ValueConverter.ConvertDelegate[source.Columns.Count];
            // 分析需要映射列集合（实体中、Reader中共有的列）
            for (int i = source.Columns.Count - 1; i >= 0; i--)
            {
                var column = source.Columns[i];
                MapDescriptor mapper;
                if (memberDescriptors.TryGetValue(column, out mapper))
                {
                    mappers.Add(new KeyValuePair<int, MapDescriptor>(i, mapper));
                    converts[i] = ValueConverter.GetConvert(source.GetColumnType(i), mapper.ValueType);
                }
            }
            this.mappers = mappers.ToArray();
            initialized = true;
        }

        /// <summary>
        /// 完成
        /// </summary>
        public void TearDown() { }

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
            }else if (!initialized)
            {
                Initialize();
            }

            result = New();

            foreach (var descriptor in mappers)
            {
                var columnIndex = descriptor.Key;
                var description = descriptor.Value;
                try
                {
                    description.Map(result, source, columnIndex, converts[columnIndex]);
                }
                catch (Exception e)
                {
                    throw new InvalidCastException($"{description.MemberName} 列映射失败，值“{source[columnIndex]}”对于类型 {description.MemberValueType} 无效", e);
                }
            }
            return true;
        }
        #endregion
    }
}
