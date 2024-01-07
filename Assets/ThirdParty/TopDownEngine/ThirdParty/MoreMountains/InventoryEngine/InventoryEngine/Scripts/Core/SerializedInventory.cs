using System;

namespace MoreMountains.InventoryEngine
{
    [Serializable]
    /// <summary>
    /// 파일에서 인벤토리를 저장/로드하는 데 도움이 되는 직렬화된 클래스입니다.
    /// </summary>
    public class SerializedInventory
    {
        public int NumberOfRows;
        public int NumberOfColumns;
        public string InventoryName = "Inventory";
        public MoreMountains.InventoryEngine.Inventory.InventoryTypes InventoryType;
        public bool DrawContentInInspector = false;
        public string[] ContentType;
        public int[] ContentQuantity;
    }
}