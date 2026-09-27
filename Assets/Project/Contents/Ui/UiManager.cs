using CoreEngine.UI;
using Farm.UI.Item;
using System;
using UnityEngine;

namespace Farm.UI
{
    public enum UiType
    {
        QuickSlot,
    }
    public class UiManager : BaseUiManager<UiType>
    {
        protected override Type GetConcreteUiType(UiType uiEnum)
        {
            return uiEnum switch
            {
                //UiType.QuickSlot => typeof(QuickSlot),
                _ => null
            };
        }
    }
}

