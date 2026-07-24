using SculptGame.Building;
using SculptGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SculptGame.UI
{
    /// <summary>
    /// 핫바 아래에 위치하는 조작 안내 + 그리드 스냅 토글 패널.
    /// 슬롯 선택, 회전, 배치/분해 컨트롤 힌트를 표시합니다.
    /// </summary>
    public class ObjectPaletteUI : MonoBehaviour
    {
        [Header("Controls UI")]
        public Toggle gridSnapToggle;
        public Button clearAllButton;
        public TextMeshProUGUI controlHintText;

        private void Start()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged += RefreshHint;
                PlayerInventory.Instance.OnResourceHovered  += HandleResourceHovered;
            }

            if (gridSnapToggle != null)
            {
                gridSnapToggle.isOn = BuildingSystem.Instance != null && BuildingSystem.Instance.useGridSnap;
                gridSnapToggle.onValueChanged.AddListener(OnGridSnapToggled);
            }

            if (clearAllButton != null)
            {
                clearAllButton.onClick.AddListener(OnClearAllClicked);
            }

            RefreshHint();
        }

        public void RefreshHint()
        {
            if (controlHintText == null) return;

            BuildableObjectData selected = PlayerInventory.Instance?.SelectedItem;
            if (selected != null)
            {
                controlHintText.text = $"선택: <color=#FFEC6E>{selected.displayName}</color>  |  Q/E·휠: 회전  |  좌클릭: 배치  |  우클릭·R: 분해수거";
            }
            else
            {
                controlHintText.text = "맵을 탐색해 재료를 주워오세요  |  1·2·3: 슬롯 전환  |  우클릭·R: 분해수거";
            }
        }

        private void HandleResourceHovered(ResourceObject res)
        {
            // 아이템 호버 상태일 때 힌트는 HotbarUI가 담당, 여기서는 갱신만
            if (res == null) RefreshHint();
        }

        private void OnGridSnapToggled(bool value)
        {
            if (BuildingSystem.Instance != null)
                BuildingSystem.Instance.useGridSnap = value;
        }

        private void OnClearAllClicked()
        {
            if (BuildingSystem.Instance != null)
                BuildingSystem.Instance.ClearAllObjects();
        }

        private void OnDestroy()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged -= RefreshHint;
                PlayerInventory.Instance.OnResourceHovered  -= HandleResourceHovered;
            }
        }
    }
}
