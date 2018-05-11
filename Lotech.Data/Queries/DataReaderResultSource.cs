using System;
using System.Collections.Generic;
using System.Data;

namespace Lotech.Data.Queries
{
    /// <summary>
    /// 数据读取器
    /// </summary>
    public class DataReaderResultSource : IResultSource
    {
        private readonly IDataReader reader;
        private readonly Lazy<IList<string>> columns;

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
            columns = new Lazy<IList<string>>(ParseColumns, false);
        }

        object IResultSource.this[int columnIndex]
        {
            get
            {
                var value =  reader[columnIndex];
                if (value == DBNull.Value)
                    return null;
                return value;
            }
        }

        IList<string> IResultSource.Columns
        {
            get
            {
                return columns.Value;
            }
        }

        Type IResultSource.GetColumnType(int columnIndex)
        {
            return reader.GetFieldType(columnIndex);
        }

        object[] IResultSource.GetRow()
        {
            var row = new object[columns.Value.Count];
            reader.GetValues(row);
            return row;
        }

        bool IResultSource.Next()
        {
            return !reader.IsClosed && reader.Read();
        }

        void IDisposable.Dispose() { reader.Dispose(); }

        IList<string> ParseColumns()
        {
            var columns = new string[reader.FieldCount];
            for (int i = columns.Length - 1; i >= 0; i--)
            {
                columns[i] = reader.GetName(i);
            }
            return columns;
        }
    }
}
