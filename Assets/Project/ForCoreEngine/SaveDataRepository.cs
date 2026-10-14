using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CoreEngine.GameData
{
    public class SaveDataRepository<TRecord, TDataToSave>
        where TRecord : class, IRecord, new()
        where TDataToSave : class, IDataToSave, new()
    {
        /// <summary>
        /// 저장될 데이터의 구조체
        /// </summary>
        [Serializable]
        public struct SaveData : IEquatable<SaveData>
        {
            public ulong RecordId;
            public TDataToSave DataToSave;

            // 참조 주소(ReferenceEquals) 기반 동등성 비교
            public bool Equals(SaveData other)
            {
                return RecordId == other.RecordId &&
                       ReferenceEquals(DataToSave, other.DataToSave);
            }

            public override bool Equals(object obj)
            {
                return obj is SaveData other && Equals(other);
            }

            // TRuntimeData의 Equals/GetHashCode 오버라이딩 여부와 관계없이 
            // 순수 힙 메모리 주소 고유 해시값을 추출
            public override int GetHashCode()
            {
                int runtimeDataHash = DataToSave != null
                    ? RuntimeHelpers.GetHashCode(DataToSave)
                    : 0;

                return HashCode.Combine(RecordId, runtimeDataHash);
            }
        }

        [Newtonsoft.Json.JsonProperty]
        private HashSet<SaveData> SaveDataSet = new();


        public bool Add(BaseDataInstance<TRecord, TDataToSave> dataInstance)
        {
            if (dataInstance == null ||
                dataInstance.Record == null ||
                dataInstance.DataToSave == null)
            {
                Debug.LogWarning("Attempted to add a null data instance or runtime data.");
                return false;
            }

            var newInstance = new SaveData
            {
                RecordId = dataInstance.Record.ID,
                DataToSave = dataInstance.DataToSave
            };

            return SaveDataSet.Add(newInstance);
        }
        public bool Remove(BaseDataInstance<TRecord, TDataToSave> dataInstance)
        {
            if (dataInstance == null ||
                dataInstance.Record == null ||
                dataInstance.DataToSave == null)
            {
                Debug.LogWarning("Attempted to remove a null data instance or runtime data.");
                return false;
            }

            var instanceToRemove = new SaveData
            {
                RecordId = dataInstance.Record.ID,
                DataToSave = dataInstance.DataToSave
            };

            return SaveDataSet.Remove(instanceToRemove);
        }

        public void Save()
        {
            //TODO: Json으로 데이터 저장
        }

        public List<BaseDataInstance<TRecord, TDataToSave>> Load<TTable>(TableHandler<TTable, TRecord> tableHandler)
            where TTable : BaseTable<TRecord>
        {
            if (tableHandler == null) return new();

            if (!LoadInternal()) return new();

            if (SaveDataSet.Count == 0)
            {
                Debug.LogWarning("No save instances found to load.");
                return new();
            }

            // 미리 공간 확보
            List<BaseDataInstance<TRecord, TDataToSave>> result = new(SaveDataSet.Count);

            foreach (var instance in SaveDataSet)
            {
                if (tableHandler.TryGetRecord(instance.RecordId, out TRecord record))
                {
                    result.Add(new BaseDataInstance<TRecord, TDataToSave>(record, instance.DataToSave));
                }
                else
                {
                    Debug.LogWarning($"Record with ID {instance.RecordId} not found in table.");
                }
            }
            return result;
        }

        private bool LoadInternal()
        {
            // TODO: Json 읽기 로직

            HashSet<SaveData> loaded = null; // 임시 더미

            if (loaded is not null)
            {
                SaveDataSet = loaded;
                return true;
            }

            // 파일이 없거나, 파싱에 실패해서 loaded가 null인 경우
            return false;
        }
    }
}
