using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using PandeaGames.Services;
using UnityEngine;

namespace Terra.Services
{
    public class TerraDBService : IService
    {
           
        private string dbPath
        {
            get => $@"URI=file:{Application.persistentDataPath}/Terra.db";
        }

        private HashSet<TerraDBRequest> _pendingChangeRequests { get; } = new HashSet<TerraDBRequest>();
        private HashSet<TerraDBSerializableRequest> _pendingReadRequests { get; } = new HashSet<TerraDBSerializableRequest>();
        private Dictionary<int, TerraDBRequest> _pendingWriteRequests { get; set; } = new Dictionary<int, TerraDBRequest>();

        private Dictionary<string, string> _serializerWriteCommandTextCache { get; } = new Dictionary<string, string>();
        
        public struct TerraDBSerializableRequest
        {
            public IDBSerializable Serializable;
        }

        public struct TerraDBParameterValue
        {
            public string Column;
            public string Value;
            public DBDataType DataType;
        }

        public struct TerraDBRequest
        {
            public string CommandText;
            public TerraDBParameterValue[] Values;
        }

        public void AddRequest(TerraDBRequest request)
        {
            _pendingChangeRequests.Add(request);
        }

        public void Setup(IDBSchema[] schemas)
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                foreach (IDBSchema schema in schemas)
                {
                    SQLiteCommand cmd = new SQLiteCommand(connection);

                    string[] tableDefs = GetColumnDefs(schema);

                    cmd.CommandText = $"CREATE TABLE IF NOT EXISTS {schema.Table} ({string.Join(", ", tableDefs)})";
                    cmd.ExecuteNonQuery();

                    for (int i = 0; i < schema.Columns.Length; i++)
                    {
                        try
                        {
                            string defaultValue = "0";
                            
                            
                            
                            cmd.CommandText =
                                $"ALTER TABLE {schema.Table} ADD COLUMN {schema.Columns[i]} {schema.Columns[i].DataType.ToString()} default {defaultValue}";
                            cmd.ExecuteNonQuery();
                        }
                        catch (SQLiteException ex)
                        {
                            //do nothing
                        }
                    }
                    
                }
            }
        }

        private string[] GetColumnDefs(IDBSchema schema)
        {
            string[] columnDefs = new string[schema.Columns.Length];
            for (int i = 0; i < schema.Columns.Length; i++)
            {
                IDBColumn column = schema.Columns[i];
                bool isPrimaryKey = i == schema.PrimaryKeyColumnIndex;

                string def = $"{column.ColumnName} {column.DataType.ToString()}";
                if (isPrimaryKey)
                {
                    def += " PRIMARY KEY";
                }

                columnDefs[i] = def;
            }

            return columnDefs;
        }

        public interface IDBWhereClause<TSerializable> where TSerializable:IDBSerializable
        {
            string Where(TSerializable serializable);
        }
        
        public class DBPrimaryKeyWhereClause<TSerializable, TSerializer> : IDBWhereClause<TSerializable> where TSerializable:IDBSerializable where TSerializer:IDBSerializer<TSerializable>
        {
            private TSerializer _serializer;
            public DBPrimaryKeyWhereClause(TSerializer serializer)
            {
                _serializer = serializer;
            }
            
            public virtual string Where(TSerializable serializable)
            {
                return
                    $"WHERE {_serializer.Columns[_serializer.PrimaryKeyColumnIndex]} = {_serializer.GetValue(serializable, _serializer.PrimaryKeyColumnIndex)}";
            }
        }
        
        public void Write<TSerializer, TSerializable>(TSerializable serializable, TSerializer serializer, IDBWhereClause<TSerializable> where)
        where TSerializable:IDBSerializable
        where TSerializer:IDBSerializer<TSerializable>
        {
            if (_pendingWriteRequests.ContainsKey(serializable.GetHashCode()))
            {
                _pendingWriteRequests.Remove(serializable.GetHashCode());
            }

            if (!_serializerWriteCommandTextCache.ContainsKey(serializer.Table))
            {
                string[] updateText = new string[serializer.Columns.Length];

                for (int i = 0; i < serializer.Columns.Length; i++)
                {
                    updateText[i] = $"{serializer.Columns[i]} = excluded.{serializer.Columns[i]}";
                }
                
                _serializerWriteCommandTextCache.Add(serializer.Table, 
                    $"INSERT INTO {serializer.Table}({string.Join(",", serializer.Columns)}) " +
                    $" Values(@{string.Join(",@", serializer.Columns)}) "+
                    $"ON CONFLICT({serializer.Columns[serializer.PrimaryKeyColumnIndex]}) DO UPDATE SET {string.Join(", ", updateText)}");
            }

            _serializerWriteCommandTextCache.TryGetValue(serializer.Table, out string commandText);

            TerraDBParameterValue[] values = new TerraDBParameterValue[serializer.Columns.Length];
            
            for (int i = 0; i < serializer.Columns.Length; i++)
            {
                values[i] = new TerraDBParameterValue()
                {
                    Column = serializer.Columns[i].ToString(),
                    Value = serializer.GetValue(serializable, i),
                    DataType = serializer.Columns[i].DataType
                };
            }
            
            TerraDBRequest request = new TerraDBRequest()
            {
                CommandText = commandText,
                Values = values
            };
            
            _pendingWriteRequests.Add(serializable.GetHashCode(), request);
        }
        
        public void Save()
        {
            Debug.Log($"[{nameof(TerraDBService)}] {nameof(Save)} with {_pendingWriteRequests.Count} changes.");
            Dictionary<int, TerraDBRequest> tmpCache = _pendingWriteRequests;
            _pendingWriteRequests = new Dictionary<int, TerraDBRequest>();
            Write(tmpCache);
        }

        private void Write(Dictionary<int, TerraDBRequest> writeRequests)
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                var cmd = new SQLiteCommand(connection);
                SQLiteTransaction transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                foreach (KeyValuePair<int, TerraDBRequest> kvp in writeRequests)
                {
                    try
                    {
                        TerraDBRequest request = kvp.Value;
                    
                        for (int i = 0; i < request.Values.Length; i++)
                        {
                            switch (request.Values[i].DataType)
                            {
                                case DBDataType.TEXT:
                                {
                                    cmd.Parameters.AddWithValue($"@{ request.Values[i].Column}", request.Values[i].Value);
                                    break;
                                }
                                case DBDataType.INTEGER:
                                {
                                    cmd.Parameters.AddWithValue($"@{ request.Values[i].Column}", int.Parse(request.Values[i].Value));
                                    break;
                                }
                                case DBDataType.NUMERIC:
                                {
                                    cmd.Parameters.AddWithValue($"@{ request.Values[i].Column}",float.Parse( request.Values[i].Value));
                                    break;
                                }
                            }
                        }

                        cmd.CommandText = request.CommandText;
                    
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        throw new Exception(kvp.Value.CommandText, innerException:e);
                    }
                   
                }
                
                transaction.Commit();
            }
        }

        public TSerializable[] Get<TSerializer, TSerializable>(TSerializer serializer)
            where TSerializable : IDBSerializable
            where TSerializer : IDBSerializer<TSerializable>
        {
            return Get<TSerializer, TSerializable>(serializer, string.Empty);
        }
        
        public TSerializable[] Get<TSerializer, TSerializable>(TSerializer serializer, string queryConstraint)
            where TSerializable : IDBSerializable
            where TSerializer : IDBSerializer<TSerializable>
        {
            return Get<TSerializer, TSerializable>(serializer, queryConstraint, $"SELECT * FROM {serializer.Table} {queryConstraint}");
        }

        public TSerializable[] Get<TSerializer, TSerializable>(TSerializer serializer, string queryConstraint, string CommandText)
            where TSerializable:IDBSerializable
            where TSerializer:IDBSerializer<TSerializable>
        {
            List<TSerializable> dataList = new List<TSerializable>();
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                var cmd = new SQLiteCommand(connection);
                cmd.CommandText = CommandText;
                SQLiteDataReader rdr;
                try
                {
                    rdr = cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    Debug.LogError(CommandText);
                    throw e;
                }

                while (rdr.Read())
                {
                    TSerializable data = serializer.Instantiate();
                    
                    for (int i = 0; i < serializer.Columns.Length; i++)
                    {
                        IDBColumn column = serializer.Columns[i];

                        if (rdr.IsDBNull(i))
                        {
                            continue;
                        }
                        
                        try
                        {
                            switch (column.DataType)
                            {
                                case DBDataType.TEXT:
                                {
                                    serializer.ParseStringResult(ref data, i, rdr.GetString(i));
                                    break;
                                }
                                case DBDataType.INTEGER:
                                {
                                    serializer.ParseIntegerResult(ref data, i, rdr.GetInt32(i));
                                    break;
                                }
                                case DBDataType.NUMERIC:
                                {
                                    serializer.ParseNumericResult(ref data, i, rdr.GetFloat(i));
                                    break;
                                }
                                default:
                                    throw new NotImplementedException();
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"[{typeof(TSerializer)}][{typeof(TSerializable)}] column [{column.ColumnName}] dataType [{column.DataType}] indexInReader [{i}] rdr.FieldCount[{rdr.FieldCount}]");
                            throw e;
                        }
                    }
                    dataList.Add(data);
                }
            }

            return dataList.ToArray();
        }

        public struct DBJoinParse
        {
            public IDBSchema Schema;
            public SQLiteDataReader Reader;
            public int ReadIndex;
        }
        
        public IEnumerable<DBJoinParse> Get<TSerializer, TSerializable>(TSerializer serializer, IDBSchema[] componentTables, string queryConstraint = "")
            where TSerializable:IDBSerializable
            where TSerializer:IDBSerializer<TSerializable>
        {
            List<TSerializable> dataList = new List<TSerializable>();
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();
                var cmd = new SQLiteCommand(connection);
                cmd.CommandText = $"SELECT * FROM {serializer.Table} {queryConstraint}";
                SQLiteDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int readIndex = 0;

                    for (int i = 0; i < componentTables.Length; i++)
                    {
                        yield return new DBJoinParse()
                        {
                            Schema = componentTables[i],
                            Reader = rdr,
                            ReadIndex = readIndex
                        };
                        readIndex += componentTables[i].Columns.Length;
                    }
                    
                    TSerializable data = serializer.Instantiate();
                    dataList.Add(data);
                    for (int i = 0; i < serializer.Columns.Length; i++)
                    {
                        IDBColumn column = serializer.Columns[i];

                        switch (column.DataType)
                        {
                            case DBDataType.TEXT:
                            {
                                serializer.ParseStringResult(ref data, i, rdr.GetString(i));
                                break;
                            }
                            case DBDataType.INTEGER:
                            {
                                serializer.ParseIntegerResult(ref data, i, rdr.GetInt32(i));
                                break;
                            }
                            case DBDataType.NUMERIC:
                            {
                                serializer.ParseNumericResult(ref data, i, rdr.GetFloat(i));
                                break;
                            }
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
            }
        }

        public void Initialize()
        {
            
        }
    }
}