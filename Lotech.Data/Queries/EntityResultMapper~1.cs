using Lotech.Data.Descriptors;
using System;
using System.Collections;
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
                    conainers[bound] = container;
                    bound++;
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

            static Mapping[] CreateEntityMapContainer(MappingContainer container)
            {
                var members = container.DescriptorProvider.GetEntityDescriptor<TEntity>(Operation.None).Members;
                var mappings = new List<Mapping>();
                var reader = container.Reader;
                // 分析需要映射列集合（实体中、Reader中共有的列）
                for (int columnIndex = 0; columnIndex < container.Reader.FieldCount; columnIndex++)
                {
                    var column = reader.GetName(columnIndex);
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
            private IDataReader reader;
            private Mapping[] mappings;

            /// <summary>
            /// 
            /// </summary>
            public IDescriptorProvider DescriptorProvider { get { return descriptorProvider; } }
            /// <summary>
            /// 
            /// </summary>
            public IDataReader Reader { get { return reader; } }

            public Mapping[] Mappings { get { return mappings; } }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="descriptorProvider"></param>
            /// <param name="reader"></param>
            public MappingContainer(IDescriptorProvider descriptorProvider, IDataReader reader)
            {
                if (reader.FieldCount == 0) throw new NotSupportedException("columns is empty");
                this.descriptorProvider = descriptorProvider;
                this.reader = reader;
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
                        && reader.FieldCount == key.reader.FieldCount
                        && reader.GetName(0) == key.reader.GetName(0))
                {
                    for (int i = reader.FieldCount - 1; i > 0; i--)
                    {
                        if (reader.GetName(i) != key.reader.GetName(i)) return false;
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
                return descriptorProvider.GetHashCode() ^ reader.FieldCount
                    ^ reader.GetName(0).GetHashCode();
            }

            /// <summary>
            /// 脱离底层源(如关联的DbReader)，以便作为缓存key
            /// </summary>
            public void Strip(Mapping[] mappings)
            {
                this.mappings = mappings;
                reader = new SchemaReader(reader);
            }

            private class SchemaReader : IDataReader
            {
                private readonly string[] columns;

                int IDataReader.Depth => throw new NotImplementedException();

                bool IDataReader.IsClosed => throw new NotImplementedException();

                int IDataReader.RecordsAffected => throw new NotImplementedException();

                int IDataRecord.FieldCount => columns.Length;

                object IDataRecord.this[string name] => throw new NotImplementedException();

                object IDataRecord.this[int i] => throw new NotImplementedException();

                internal SchemaReader(IDataReader reader)
                {
                    columns = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns[i] = reader.GetName(i);
                    }
                }

                void IDataReader.Close() => throw new NotImplementedException();

                DataTable IDataReader.GetSchemaTable() => throw new NotImplementedException();

                bool IDataReader.NextResult() => throw new NotImplementedException();

                bool IDataReader.Read() => throw new NotImplementedException();

                bool IDataRecord.GetBoolean(int i) => throw new NotImplementedException();

                byte IDataRecord.GetByte(int i) => throw new NotImplementedException();

                long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
                {
                    throw new NotImplementedException();
                }

                char IDataRecord.GetChar(int i) => throw new NotImplementedException();

                long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
                {
                    throw new NotImplementedException();
                }

                IDataReader IDataRecord.GetData(int i) => throw new NotImplementedException();

                string IDataRecord.GetDataTypeName(int i) => throw new NotImplementedException();

                DateTime IDataRecord.GetDateTime(int i) => throw new NotImplementedException();

                decimal IDataRecord.GetDecimal(int i) => throw new NotImplementedException();

                double IDataRecord.GetDouble(int i) => throw new NotImplementedException();

                Type IDataRecord.GetFieldType(int i) => throw new NotImplementedException();

                float IDataRecord.GetFloat(int i) => throw new NotImplementedException();

                Guid IDataRecord.GetGuid(int i) => throw new NotImplementedException();

                short IDataRecord.GetInt16(int i) => throw new NotImplementedException();

                int IDataRecord.GetInt32(int i) => throw new NotImplementedException();

                long IDataRecord.GetInt64(int i) => throw new NotImplementedException();

                string IDataRecord.GetName(int i) => columns[i];

                int IDataRecord.GetOrdinal(string name) => throw new NotImplementedException();

                string IDataRecord.GetString(int i) => throw new NotImplementedException();

                object IDataRecord.GetValue(int i) => throw new NotImplementedException();

                int IDataRecord.GetValues(object[] values) => throw new NotImplementedException();

                bool IDataRecord.IsDBNull(int i) => throw new NotImplementedException();

                void IDisposable.Dispose() => throw new NotImplementedException();
            }
        }

        #region Fields & Constructor
        private Mapping[] mappings;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="reader"></param>
        public override void TearUp(IDataReader reader)
        {
            base.TearUp(reader);
            var container = new MappingContainer(Database.DescriptorProvider, reader);
            mappings = MappingFactory.Create(container);
        }

        /// <summary>
        /// 映射下一结果
        /// </summary>
        /// <returns></returns>
        public override bool MapNext(out TEntity result)
        {
            if (!Reader.Read())
            {
                result = default(TEntity);
                return false;
            }

            result = New();
            var index = mappings.Length;
            try
            {
                while(--index >=0)
                    mappings[index].Execute(result, Reader.GetValue(mappings[index].ColumnIndex));
                return true;
            }
            catch (Exception e)
            {
                throw new MapException(mappings[index], result, Reader.GetName(mappings[index].ColumnIndex), e);
            }
        }
        #endregion

        class MapException : InvalidCastException
        {
            public MapException(Mapping description, TEntity entity, object value, Exception exception)
                : base($"列 {description.MemberName} 映射失败，值“{value}”({value?.GetType() })无法转换为 {description.MemberValueType}", exception)
            {
                Value = value;
                Entity = entity;
            }

            public object Value { get; private set; }
            public TEntity Entity { get; private set; }
        }
    }
}
