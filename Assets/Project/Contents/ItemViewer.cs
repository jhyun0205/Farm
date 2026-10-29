using Farm.GameData.Item;
using TMPro;
using UnityEngine;

namespace Farm.UI.Item
{
    public class ItemViewer : BaseDataInstanceViewer<ItemInstance, ItemRecord, ItemDataToSave>
    {
        [SerializeField] protected TextMeshProUGUI amountText;

        protected override void OnConnected(ItemInstance target)
        {
            // 슬롯에 들어있는 아이템의 수량/내구도가 변할 때마다 자동으로 뷰 갱신
            target.Event_OnRuntimeDataChanged += UpdateView;
        }

        protected override void OnDisconnected(ItemInstance target)
        {
            target.Event_OnRuntimeDataChanged -= UpdateView;
        }

        protected override void UpdateViewFromRecord(ItemRecord record)
        {
            if (nameText) nameText.text = record.Name;
            if (descriptionText) descriptionText.text = record.Description;
            if (iconImage)
            {
                iconImage.color = Color.white;
                // TODO: ItemRecord에 아이콘 AssetId가 추가되면 iconImage.sprite = record.Icon.Get(); 연결
            }
        }

        protected override void UpdateViewFromRuntimeData(ItemDataToSave runtimeData)
        {
            if (amountText)
            {
                amountText.text = runtimeData.amount > 1 ? runtimeData.amount.ToString() : string.Empty;
            }
        }

        public override void ClearView()
        {
            base.ClearView();
            if (amountText) amountText.text = string.Empty;
        }
    }
}