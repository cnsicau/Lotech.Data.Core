using System;
using System.Collections;
using System.Data.Common;

namespace Lotech.Data
{
    /// <summary>
    /// 组合数据读取，用于绑定其他需要释放的对象对象，利于在DataReader关闭时同步释放组合对象
    /// </summary>
    public class CompositedDataReader : DbDataReader, IEnumerable
    {
        #region Fields & Constructors

        private IDisposable[] compositedDisposingObjects;
        private DbDataReader reader;

        internal CompositedDataReader(DbDataReader reader, params IDisposable[] compositedDisposingObjects)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            this.reader = reader;
            this.compositedDisposingObjects = compositedDisposingObjects;
        }
        #endregion

        #region DbDataReader Members

        /// <summary>
        /// 
        /// </summary>
        public override void Close()
        {
            reader.Close();
            if (compositedDisposingObjects != null && compositedDisposingObjects.Length > 0)
            {
                Array.ForEach(compositedDisposingObjects, o => { if (o != null) o.Dispose(); });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int Depth
        {
            get { return reader.Depth; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int FieldCount
        {
            get { return reader.FieldCount; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool GetBoolean(int ordinal)
        {
            return reader.GetBoolean(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override byte GetByte(int ordinal)
        {
            return reader.GetByte(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return reader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 
        /// </summary>
        public override char GetChar(int ordinal)
        {
            return reader.GetChar(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return reader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GetDataTypeName(int ordinal)
        {
            return reader.GetDataTypeName(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override DateTime GetDateTime(int ordinal)
        {
            return reader.GetDateTime(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override decimal GetDecimal(int ordinal)
        {
            return reader.GetDecimal(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override double GetDouble(int ordinal)
        {
            return reader.GetDouble(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override IEnumerator GetEnumerator()
        {
            return ((IEnumerable)reader).GetEnumerator();
        }

        /// <summary>
        /// 
        /// </summary>
        public override Type GetFieldType(int ordinal)
        {
            return reader.GetFieldType(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override float GetFloat(int ordinal)
        {
            return reader.GetFloat(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override Guid GetGuid(int ordinal)
        {
            return reader.GetGuid(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override short GetInt16(int ordinal)
        {
            return reader.GetInt16(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetInt32(int ordinal)
        {
            return reader.GetInt32(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override long GetInt64(int ordinal)
        {
            return reader.GetInt64(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GetName(int ordinal)
        {
            return reader.GetName(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetOrdinal(string name)
        {
            return reader.GetOrdinal(name);
        }

        /// <summary>
        /// 
        /// </summary>
        public override System.Data.DataTable GetSchemaTable()
        {
            return reader.GetSchemaTable();
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GetString(int ordinal)
        {
            return reader.GetString(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override object GetValue(int ordinal)
        {
            return reader.GetValue(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override int GetValues(object[] values)
        {
            return reader.GetValues(values);
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool HasRows
        {
            get { return reader.HasRows; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsClosed
        {
            get { return reader.IsClosed; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsDBNull(int ordinal)
        {
            return reader.IsDBNull(ordinal);
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool NextResult()
        {
            return reader.NextResult();
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool Read()
        {
            return reader.Read();
        }

        /// <summary>
        /// 
        /// </summary>
        public override int RecordsAffected
        {
            get { return reader.RecordsAffected; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override object this[string name]
        {
            get { return reader[name]; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public override object this[int ordinal]
        {
            get { return reader[ordinal]; }
        }
        #endregion
    }
}
