using System;
using System.Data;

namespace Lotech.Data.Queries
{
    class MetaDataReader : IDataReader
    {
        private readonly string[] columns;

        public string[] Columns { get { return columns; } }

        #region Constructor

        internal MetaDataReader(IDataReader reader)
        {
            columns = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columns[i] = reader.GetName(i);
            }
        }
        #endregion

        #region IDataReader
        int IDataReader.Depth => throw new NotImplementedException();

        bool IDataReader.IsClosed => throw new NotImplementedException();

        int IDataReader.RecordsAffected => throw new NotImplementedException();

        int IDataRecord.FieldCount => columns.Length;

        object IDataRecord.this[string name] => throw new NotImplementedException();

        object IDataRecord.this[int i] => throw new NotImplementedException();

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
        #endregion
    }
}
