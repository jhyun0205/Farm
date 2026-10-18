using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CoreEngine.Helpers;

namespace CoreEngine.GameData
{
    public class SaveDataRepository<TInstance, TRecord, TDataToSave>
        where TInstance : BaseDataInstance<TRecord, TDataToSave>
        where TRecord : class, IRecord
        where TDataToSave : class, IDataToSave, new()
    {
        [Serializable]
        public struct SaveData : IEquatable<SaveData>
        {
            public ulong RecordId;
            public TDataToSave DataToSave;

            public bool Equals(SaveData other)
            {
                return RecordId == other.RecordId &&
                       ReferenceEquals(DataToSave, other.DataToSave);
            }

            public override bool Equals(object obj) => obj is SaveData other && Equals(other);

            public override int GetHashCode()
            {
                int runtimeDataHash = DataToSave != null ? RuntimeHelpers.GetHashCode(DataToSave) : 0;
                return HashCode.Combine(RecordId, runtimeDataHash);
            }
        }

        [Newtonsoft.Json.JsonProperty]
        private HashSet<SaveData> SaveDataSet = new();

        public bool Add(TInstance dataInstance)
        {
            if (dataInstance == null || dataInstance.Record == null || dataInstance.RuntimeData == null)
            {
                LogHelper.LogWarning("Attempted to add a null data instance or runtime data.");
                return false;
            }

            var newInstance = new SaveData
            {
                RecordId = dataInstance.Record.ID,
                DataToSave = dataInstance.RuntimeData
            };

            return SaveDataSet.Add(newInstance);
        }

        public bool Remove(TInstance dataInstance)
        {
            if (dataInstance == null || dataInstance.Record == null || dataInstance.RuntimeData == null)
            {
                LogHelper.LogWarning("Attempted to remove a null data instance or runtime data.");
                return false;
            }

            var instanceToRemove = new SaveData
            {
                RecordId = dataInstance.Record.ID,
                DataToSave = dataInstance.RuntimeData
            };

            return SaveDataSet.Remove(instanceToRemove);
        }

        public void Clear() => SaveDataSet.Clear();

        public void Save()
        {
            // TODO: GZipStream + Newtonsoft.Json 직렬화 저장 로직
        }

        public List<TInstance> Load<TTable>(
            TableHandler<TTable, TRecord> tableHandler,
            Func<TRecord, TDataToSave, TInstance> instanceFactory)
            where TTable : BaseTable<TRecord>
        {
            if (tableHandler == null || instanceFactory == null) return new();
            if (!LoadInternal()) return new();

            if (SaveDataSet.Count == 0) return new();

            List<TInstance> result = new(SaveDataSet.Count);

            foreach (var saved in SaveDataSet)
            {
                if (tableHandler.TryGetRecord(saved.RecordId, out TRecord record))
                {
                    result.Add(instanceFactory(record, saved.DataToSave));
                }
                else
                {
                    LogHelper.LogWarning($"Record with ID {saved.RecordId} not found in table.");
                }
            }
            return result;
        }

        private bool LoadInternal()
        {
            HashSet<SaveData> loaded = null; // TODO: Json 역직렬화
            if (loaded is not null)
            {
                SaveDataSet = loaded;
                return true;
            }
            return false;
        }
    }
}