using System;
using System.Collections;


namespace CoreEngine.GameData
{
    public interface IDataToSave
    {
    }

    public class BaseDataInstance<TRecord, TRuntimeData>
        where TRecord : class, IRecord // 반드시 생성시 주입
        where TRuntimeData : class, IDataToSave, new()
    {
        public TRecord Record { get; private set; }
        public TRuntimeData RuntimeData { get; private set; }


        public BaseDataInstance(TRecord record, TRuntimeData runtimeData)
        {
            Record = record;
            RuntimeData = runtimeData;
        }
        public BaseDataInstance(TRecord record)
        {
            Record = record;
            RuntimeData = new TRuntimeData();
        }
        public void SetRuntimeData(TRuntimeData runtimeData)
        {
            RuntimeData = runtimeData;
        }
    }
}