using System;
using System.Collections;


namespace CoreEngine.GameData
{
    public interface IDataToSave
    {
    }

    public class BaseDataInstance<TRecord, TDataToSave>
        where TRecord : class, IRecord // 반드시 생성시 주입
        where TDataToSave : class, IDataToSave, new()
    {
        public TRecord Record { get; private set; }
        public TDataToSave DataToSave { get; private set; }


        public BaseDataInstance(TRecord record, TDataToSave runtimeData)
        {
            Record = record;
            DataToSave = runtimeData;
        }
        public BaseDataInstance(TRecord record)
        {
            Record = record;
            DataToSave = new TDataToSave();
        }
        public void SetRuntimeData(TDataToSave runtimeData)
        {
            DataToSave = runtimeData;
        }
    }
}