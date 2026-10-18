using System;
using System.Collections.Generic;
using CoreEngine.Actor;
using CoreEngine.Pool;
using CoreEngine.GameData;
using Farm.GameData;
using UnityEngine;

namespace Farm.Character
{
    [Serializable]
    public class CharacterDataFeature : BaseActorFeature, IActorFeature, ISpawnable, IDisposable
    {
        [SerializeField] private CharacterInventory inventory = new();
        // 추후 신규 구조로 컨버팅하며 주석 해제
        // [SerializeField] private CharacterQuestBook questBook = new();
        // [SerializeField] private CharacterCropDataSheet cropDataSheet = new();
        // [SerializeField] private CharacterEggEncyclopedia eggEncyclopedia = new();

        private readonly List<IDataHandler> _handlers = new();
        private readonly Dictionary<Type, IDataHandler> _handlerDict = new();

        public CharacterInventory Inventory => inventory;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            RegisterHandler(inventory);
            // RegisterHandler(questBook);
            // RegisterHandler(cropDataSheet);
            // RegisterHandler(eggEncyclopedia);

            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].Initialize(Host);
            }
        }

        private void RegisterHandler<T>(T handler) where T : class, IDataHandler
        {
            if (handler == null) return;
            _handlers.Add(handler);
            _handlerDict[typeof(T)] = handler;
        }

        public bool TryGetHandler<T>(out T handler) where T : class, IDataHandler
        {
            if (_handlerDict.TryGetValue(typeof(T), out var raw))
            {
                handler = raw as T;
                return handler != null;
            }
            handler = null;
            return false;
        }

        void ISpawnable.OnSpawn()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].OnSpawn();
            }
        }

        void ISpawnable.OnDespawn()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].OnDespawn();
            }
        }

        public void SaveAll()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].SaveData();
            }
        }

        public void LoadAll()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].LoadData();
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _handlers.Count; i++)
            {
                _handlers[i].Dispose();
            }
        }
    }
}