using System;
using System.Collections.Generic;
using CoreEngine.Actor;

namespace CoreEngine.GameData
{
    public interface IDataHandler : IDisposable
    {
        void Initialize(IActorHost host);
        void OnSpawn();
        void OnDespawn();
        void SaveData();
        void LoadData();
    }

    /// <summary>
    /// 과거 BaseDatabaseAccess를 대체하는 새로운 데이터 핸들러 베이스 클래스
    /// </summary>
    [Serializable]
    public abstract class BaseDataHandler<TTable, TRecord, TInstance, TDataToSave> : IDataHandler
        where TTable : BaseTable<TRecord>
        where TRecord : class, IRecord
        where TInstance : BaseDataInstance<TRecord, TDataToSave>
        where TDataToSave : class, IDataToSave, new()
    {
        protected IActorHost Host { get; private set; }
        protected readonly TableHandler<TTable, TRecord> tableHandler = new();
        protected readonly SaveDataRepository<TInstance, TRecord, TDataToSave> repository = new();

        public void Initialize(IActorHost host)
        {
            Host = host;
            OnInitialized();
        }

        protected virtual void OnInitialized() { }
        public virtual void OnSpawn() { }
        public virtual void OnDespawn() { }

        public virtual void SaveData() => repository.Save();
        public abstract void LoadData();

        public virtual void Dispose()
        {
            OnDespawn();
        }
    }
}