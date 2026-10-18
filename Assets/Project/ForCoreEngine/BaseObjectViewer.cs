using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CoreEngine.GameData;

namespace Farm.UI
{
    public abstract class BaseObjectViewer<TViewTarget> : MonoBehaviour
        where TViewTarget : class
    {
        public TViewTarget ConnectObject { get; private set; }
        public bool HasTarget => ConnectObject != null;

        public void Connect(TViewTarget target)
        {
            if (ReferenceEquals(ConnectObject, target))
            {
                UpdateView();
                return;
            }

            if (ConnectObject != null)
            {
                OnDisconnected(ConnectObject);
                ConnectObject = null;
            }

            if (target == null)
            {
                ClearView();
                return;
            }

            ConnectObject = target;
            OnConnected(ConnectObject);
            UpdateView();
        }

        public void Disconnect() => Connect(null);

        protected virtual void OnConnected(TViewTarget target) { }
        protected virtual void OnDisconnected(TViewTarget target) { }

        public abstract void UpdateView();
        public abstract void ClearView();
    }

    public abstract class BaseDataInstanceViewer<TInstance, TRecord, TRuntimeData> : BaseObjectViewer<TInstance>
        where TInstance : BaseDataInstance<TRecord, TRuntimeData>
        where TRecord : class, IRecord
        where TRuntimeData : class, IDataToSave, new()
    {
        [SerializeField] protected TextMeshProUGUI nameText;
        [SerializeField] protected TextMeshProUGUI descriptionText;
        [SerializeField] protected Image iconImage;

        public override void UpdateView()
        {
            if (!HasTarget)
            {
                ClearView();
                return;
            }

            UpdateViewFromRecord(ConnectObject.Record);
            UpdateViewFromRuntimeData(ConnectObject.RuntimeData);
        }

        protected abstract void UpdateViewFromRecord(TRecord record);
        protected abstract void UpdateViewFromRuntimeData(TRuntimeData runtimeData);

        public override void ClearView()
        {
            if (nameText) nameText.text = string.Empty;
            if (descriptionText) descriptionText.text = string.Empty;
            if (iconImage)
            {
                iconImage.sprite = null;
                iconImage.color = Color.clear;
            }
        }
    }
}