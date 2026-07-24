using SculptGame.Building;
using SculptGame.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SculptGame.UI
{
    /// <summary>
    /// 화면 하단 중앙에 표시되는 3슬롯 핫바 UI.
    /// 각 슬롯은 아이템 색상 아이콘 + 이름 + 수량을 표시하며,
    /// 선택된 슬롯은 흰색 외곽선 + 밝은 배경으로 하이라이트됩니다.
    /// </summary>
    public class HotbarUI : MonoBehaviour
    {
        [Header("Slot Root Objects (3개, 순서: 슬롯 0, 1, 2)")]
        public GameObject[] slotRoots = new GameObject[PlayerInventory.SlotCount];

        // 각 슬롯의 하위 컴포넌트 — Auto Setup이 채워줍니다
        [HideInInspector] public Image[] slotBgImages = new Image[PlayerInventory.SlotCount];
        [HideInInspector] public Image[] slotIconImages = new Image[PlayerInventory.SlotCount];
        [HideInInspector] public TextMeshProUGUI[] slotNameTexts = new TextMeshProUGUI[PlayerInventory.SlotCount];
        [HideInInspector] public TextMeshProUGUI[] slotCountTexts = new TextMeshProUGUI[PlayerInventory.SlotCount];
        [HideInInspector] public Image[] slotOutlines = new Image[PlayerInventory.SlotCount];

        // 줍기 안내 텍스트
        [Header("Pickup Hint")]
        public TextMeshProUGUI pickupHintText;

        // ─── 스타일 설정 ───
        private static readonly Color ColorSelected    = new Color(0.95f, 0.90f, 0.30f, 1f);   // 선택 외곽선 (노란빛)
        private static readonly Color ColorNormal      = new Color(1f, 1f, 1f, 0.15f);          // 비선택 외곽선
        private static readonly Color ColorBgSelected  = new Color(0.15f, 0.18f, 0.28f, 0.97f); // 선택 배경
        private static readonly Color ColorBgNormal    = new Color(0.05f, 0.06f, 0.12f, 0.88f); // 비선택 배경
        private static readonly Color ColorEmpty       = new Color(0.35f, 0.35f, 0.35f, 0.35f); // 빈 슬롯 아이콘
        private static readonly Color ColorFull        = new Color(1f, 0.35f, 0.35f, 0.9f);     // 인벤 가득 표시

        private void Start()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged += RefreshAll;
                PlayerInventory.Instance.OnResourceHovered  += HandleResourceHovered;
            }

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (PlayerInventory.Instance != null)
            {
                PlayerInventory.Instance.OnInventoryChanged -= RefreshAll;
                PlayerInventory.Instance.OnResourceHovered  -= HandleResourceHovered;
            }
        }

        /// <summary>전체 핫바 슬롯 갱신</summary>
        public void RefreshAll()
        {
            if (PlayerInventory.Instance == null) return;

            HotbarSlot[] slots    = PlayerInventory.Instance.Slots;
            int           selected = PlayerInventory.Instance.SelectedSlotIndex;

            for (int i = 0; i < PlayerInventory.SlotCount; i++)
            {
                RefreshSlot(i, slots[i], i == selected);
            }

            // 줍기 힌트 기본 초기화
            if (pickupHintText != null && PlayerInventory.Instance.GetHoveredResource() == null)
            {
                pickupHintText.text = "";
            }
        }

        private void RefreshSlot(int i, HotbarSlot slot, bool isSelected)
        {
            // ── 배경 ──
            if (slotBgImages != null && i < slotBgImages.Length && slotBgImages[i] != null)
                slotBgImages[i].color = isSelected ? ColorBgSelected : ColorBgNormal;

            // ── 외곽선 ──
            if (slotOutlines != null && i < slotOutlines.Length && slotOutlines[i] != null)
                slotOutlines[i].color = isSelected ? ColorSelected : ColorNormal;

            bool isEmpty = slot == null || slot.IsEmpty;

            // ── 아이콘 ──
            if (slotIconImages != null && i < slotIconImages.Length && slotIconImages[i] != null)
            {
                if (isEmpty)
                {
                    slotIconImages[i].sprite = null;
                    slotIconImages[i].color  = ColorEmpty;
                }
                else
                {
                    if (slot.data.icon != null)
                    {
                        slotIconImages[i].sprite = slot.data.icon;
                        slotIconImages[i].color  = Color.white;
                    }
                    else
                    {
                        // 아이콘 없으면 아이템 색상 사각형으로 대체
                        slotIconImages[i].sprite = null;
                        Color c = slot.data.defaultColor;
                        c.a = 0.90f;
                        slotIconImages[i].color = c;
                    }
                }
            }

            // ── 이름 ──
            if (slotNameTexts != null && i < slotNameTexts.Length && slotNameTexts[i] != null)
            {
                slotNameTexts[i].text  = isEmpty ? $"[{i + 1}]" : $"[{i + 1}] {slot.data.displayName}";
                slotNameTexts[i].color = isEmpty
                    ? new Color(0.5f, 0.5f, 0.5f, 0.6f)
                    : (isSelected ? new Color(1f, 0.95f, 0.5f) : Color.white);
            }

            // ── 수량 ──
            if (slotCountTexts != null && i < slotCountTexts.Length && slotCountTexts[i] != null)
            {
                slotCountTexts[i].text    = isEmpty ? "" : $"×{slot.count}";
                slotCountTexts[i].enabled = !isEmpty;
            }
        }

        private void HandleResourceHovered(ResourceObject res)
        {
            if (pickupHintText == null) return;

            if (res != null)
            {
                bool isFull = PlayerInventory.Instance != null && PlayerInventory.Instance.IsFull();
                if (isFull)
                {
                    pickupHintText.text  = $"[핫바 가득 참] 먼저 배치하거나 분해하세요!";
                    pickupHintText.color = ColorFull;
                }
                else
                {
                    pickupHintText.text  = $"[E] {res.objectData.displayName} 줍기";
                    pickupHintText.color = new Color(0.9f, 1f, 0.5f);
                }
            }
            else
            {
                pickupHintText.text = "";
            }
        }
    }
}
