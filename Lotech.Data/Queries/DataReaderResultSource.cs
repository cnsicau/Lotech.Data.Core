using System;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 数据读取器
    /// </summary>
    public class DataReaderResultSource : IResultSource
    {
        private readonly IDataReader reader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        public DataReaderResultSource(IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (reader.IsClosed)
                throw new InvalidOperationException("reader is closed");

            this.reader = reader;
        }

        int IResultSource.ColumnCount { get { return reader.FieldCount; } }

        string IResultSource.GetColumnName(int index) { return reader.GetName(index); }

        object IResultSource.GetColumnValue(int index)
        {
            if (reader.IsDBNull(index)) return null;
            return reader.GetValue(index);
        }

        Type IResultSource.GetColumnType(int columnIndex)
        {
            return reader.GetFieldType(columnIndex);
        }

        bool IResultSource.Next()
        {
            return !reader.IsClosed && reader.Read();
        }

        void IDisposable.Dispose() { reader.Dispose(); }
    }
}
