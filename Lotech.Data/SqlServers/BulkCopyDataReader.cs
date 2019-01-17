#if NET_4
using System;
using System.Collections.Generic;
using System.Data;
using Lotech.Data.Descriptors;

namespace Lotech.Data.SqlServers
{
    class BulkCopyDataReader<TEntity> : IDataReader where TEntity : class
    {
        private object[] values;
        private readonly IEnumerator<TEntity> entities;
        private readonly MemberTuple<TEntity>[] members;

        public BulkCopyDataReader(IEnumerable<TEntity> entities, MemberTuple<TEntity>[] members)
        {
            this.entities = entities.GetEnumerator();
            this.members = members;
            values = new object[members.Length];
        }

        public object this[string name] { get { throw new NotSupportedException(); } }

        public object this[int i] { get { throw new NotSupportedException(); } }

        public int Depth { get { throw new NotSupportedException(); } }

        public int FieldCount { get { return members.Length; } }

        public bool IsClosed { get { return values == null; } }

        public int RecordsAffected { get { throw new NotSupportedException(); } }

        public void Close() { entities.Dispose(); }

        public void Dispose() { entities.Dispose(); }

        public bool GetBoolean(int i) { throw new NotImplementedException(); }

        public byte GetByte(int i) { throw new NotImplementedException(); }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }

        public char GetChar(int i) { throw new NotImplementedException(); }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) { throw new NotImplementedException(); }

        public IDataReader GetData(int i) { throw new NotImplementedException(); }

        public string GetDataTypeName(int i) { throw new NotImplementedException(); }

        public DateTime GetDateTime(int i) { throw new NotImplementedException(); }

        public decimal GetDecimal(int i) { throw new NotImplementedException(); }

        public double GetDouble(int i) { throw new NotImplementedException(); }

        public Type GetFieldType(int i) { throw new NotImplementedException(); }

        public float GetFloat(int i) { throw new NotImplementedException(); }

        public Guid GetGuid(int i) { throw new NotImplementedException(); }

        public short GetInt16(int i) { throw new NotImplementedException(); }

        public int GetInt32(int i) { throw new NotImplementedException(); }

        public long GetInt64(int i) { throw new NotImplementedException(); }

        public string GetName(int i) { throw new NotImplementedException(); }

        public int GetOrdinal(string name) { throw new NotImplementedException(); }

        public DataTable GetSchemaTable() { throw new NotImplementedException(); }

        public string GetString(int i) { throw new NotImplementedException(); }

        public object GetValue(int i) { return values[i]; }

        public int GetValues(object[] values) { throw new NotImplementedException(); }

        public bool IsDBNull(int i) { throw new NotImplementedException(); }

        public bool NextResult() { throw new NotImplementedException(); }

        public bool Read()
        {
            if (entities.MoveNext())
            {
                var current = entities.Current;
                for (int i = members.Length - 1; i >= 0; i--)
                {
                    values[i] = members[i].Getter(current);
                }
                return true;
            }
            values = null;
            return false;
        }
    }
}
#endif