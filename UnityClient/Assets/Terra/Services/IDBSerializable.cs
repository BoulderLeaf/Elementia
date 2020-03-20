using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Terra.SerializedData.Entities;

namespace Terra.Services
{
    public enum DBDataType
    {
        TEXT,
        NUMERIC,
        INTEGER,
        REAL,
        BLOB
    }
    
    public struct IDBColumn
    {
        public string ColumnName;
        public DBDataType DataType;

        public override string ToString()
        {
            return ColumnName;
        }
    }
    
    public interface IDBSerializable
    {
    }

    public interface IDBSchema
    {
        string Table { get; }
        IDBColumn[] Columns { get; }
        int PrimaryKeyColumnIndex { get; }
    }

    public interface IDBSerializer<TSerializable> : IDBSchema
        where TSerializable:IDBSerializable
    {
        TSerializable Instantiate();
        void ParseStringResult(ref TSerializable serializable, int columnIndex, string value);
        void ParseIntegerResult(ref TSerializable serializable, int columnIndex, int value);
        void ParseNumericResult(ref TSerializable serializable, int columnIndex, float value);
        string GetValue(TSerializable serializable, int columnIndex);
    }
}