using System;
using System.Data;

namespace Lotech.Data
{
    /// <summary>
    /// 组合数据读取，用于绑定其他需要释放的对象对象，利于在DataReader关闭时同步释放组合对象
    /// </summary>
    public class CompositedDataReader : IDataReader
    {
        private readonly IDataReader dataReader;
        private readonly IDisposable[] compositedDisposables;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="compositedDisposables"></param>
        public CompositedDataReader(IDataReader dataReader, params IDisposable[] compositedDisposables)
        {
            this.dataReader = dataReader;
            this.compositedDisposables = compositedDisposables;
        }

        void DisposeComposited()
        {
            for (int i = compositedDisposables.Length - 1; i >= 0; i--)
            {
                compositedDisposables[i].Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IDataReader Reader { get { return dataReader; } }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object this[int i] => dataReader[i];
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name] => dataReader[name];
        /// <summary>
        /// 
        /// </summary>
        public int Depth => dataReader.Depth;
        /// <summary>
        /// 
        /// </summary>
        public bool IsClosed => dataReader.IsClosed;
        /// <summary>
        /// 
        /// </summary>
        public int RecordsAffected => dataReader.RecordsAffected;
        /// <summary>
        /// 
        /// </summary>
        public int FieldCount => dataReader.FieldCount;
        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            dataReader.Close();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            dataReader.Dispose();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool GetBoolean(int i)
        {
            return dataReader.GetBoolean(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public byte GetByte(int i)
        {
            return dataReader.GetByte(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            return dataReader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public char GetChar(int i)
        {
            return dataReader.GetChar(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <param name="fieldoffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferoffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            return dataReader.GetChars(i, fieldoffset, buffer, bufferoffset, length);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public IDataReader GetData(int i)
        {
            return dataReader.GetData(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetDataTypeName(int i)
        {
            return dataReader.GetDataTypeName(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public DateTime GetDateTime(int i)
        {
            return dataReader.GetDateTime(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public decimal GetDecimal(int i)
        {
            return dataReader.GetDecimal(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public double GetDouble(int i)
        {
            return dataReader.GetDouble(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Type GetFieldType(int i)
        {
            return dataReader.GetFieldType(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public float GetFloat(int i)
        {
            return dataReader.GetFloat(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Guid GetGuid(int i)
        {
            return dataReader.GetGuid(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public short GetInt16(int i)
        {
            return dataReader.GetInt16(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public int GetInt32(int i)
        {
            return dataReader.GetInt32(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public long GetInt64(int i)
        {
            return dataReader.GetInt64(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetName(int i)
        {
            return dataReader.GetName(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetOrdinal(string name)
        {
            return dataReader.GetOrdinal(name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetSchemaTable()
        {
            return dataReader.GetSchemaTable();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public string GetString(int i)
        {
            return dataReader.GetString(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public object GetValue(int i)
        {
            return dataReader.GetValue(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public int GetValues(object[] values)
        {
            return dataReader.GetValues(values);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public bool IsDBNull(int i)
        {
            return dataReader.IsDBNull(i);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool NextResult()
        {
            return dataReader.NextResult();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            return dataReader.Read();
        }
    }
}
