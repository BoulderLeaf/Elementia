using Terra.Services;

namespace Terra.SerializedData.Entities
{
    public abstract class AbstractEntityComponent<TData> : IEntityComponent where TData : IDBSerializable
    {
        private TData _Data;
        public TData Data
        {
            get { return _Data; }
            set
            {
                bool didChange = !value.Equals(_Data);
                _Data = value;
            
                if(didChange)
                    OnChange();
            }
        }

        public abstract EntityComponent Type { get; }
        protected abstract IDBSerializer<TData> Serializer { get; }
        protected abstract TerraDBService.IDBWhereClause<TData> WhereClause { get; }
        protected TerraDBService DB { get; private set; }

        public AbstractEntityComponent(TerraDBService DB, TData Data)
        {
            this.DB = DB;
            _Data = Data;
        }
        
        protected void OnChange()
        {
            DB.Write(Data, Serializer, WhereClause);
        }
    }
}