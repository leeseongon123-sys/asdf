using System;
using System.Collections.Generic;
using SculptGame.Building;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SculptGame.Player
{
    /// <summary>
    /// 핫바 슬롯 하나의 데이터
    /// </summary>
    [System.Serializable]
    public class HotbarSlot
    {
        public BuildableObjectData data;
        public int count;

        public bool IsEmpty => data == null || count <= 0;

        public void Clear()
        {
            data = null;
            count = 0;
        }
    }

    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory Instance { get; private set; }

        public const int SlotCount = 1;

        public float pickupRange = 3.0f;
        public LayerMask resourceLayerMask;

        // 고정 1슬롯 배열
        private HotbarSlot[] slots = new HotbarSlot[SlotCount];
        private int selectedSlotIndex = 0;

        public int SelectedSlotIndex => selectedSlotIndex;
        public HotbarSlot[] Slots => slots;

        /// <summary>현재 선택된 슬롯의 아이템 데이터 (없으면 null)</summary>
        public BuildableObjectData SelectedItem =>
            (slots[selectedSlotIndex] != null && !slots[selectedSlotIndex].IsEmpty)
                ? slots[selectedSlotIndex].data
                : null;

        public event Action OnInventoryChanged;
        public event Action<ResourceObject> OnResourceHovered;

        private ResourceObject currentHoveredResource;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // 슬롯 초기화
            for (int i = 0; i < SlotCount; i++)
                slots[i] = new HotbarSlot();
        }

        private void Update()
        {
            CheckForPickupableResource();

            // E키 줍기
            if (currentHoveredResource != null && IsEPressed())
            {
                PickupResource(currentHoveredResource);
            }

            // 숫자키 1 으로 슬롯 선택
            HandleSlotSelectionInput();
        }

        private void HandleSlotSelectionInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current == null) return;
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
#else
            if (Input.GetKeyDown(KeyCode.Alpha1)) SelectSlot(0);
#endif
        }

        /// <summary>지정 슬롯 인덱스를 선택</summary>
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= SlotCount) return;
            selectedSlotIndex = index;
            OnInventoryChanged?.Invoke();
        }

        /// <summary>아이템을 빈 슬롯에 추가. 가득 찼으면 false 반환</summary>
        public bool AddItem(BuildableObjectData data, int count = 1)
        {
            if (data == null) return false;

            // 빈 슬롯 탐색
            for (int i = 0; i < SlotCount; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].data = data;
                    slots[i].count = count;

                    // 선택 슬롯이 비어 있었으면 이쪽으로 포커스
                    if (slots[selectedSlotIndex].IsEmpty)
                        selectedSlotIndex = i;

                    OnInventoryChanged?.Invoke();
                    return true;
                }
            }

            // 슬롯이 꽉 참
            Debug.LogWarning("[Inventory] 핫바가 가득 찼습니다! (최대 1칸)");
            return false;
        }

        /// <summary>선택된 슬롯에서 아이템 1개 소비</summary>
        public bool ConsumeItem(BuildableObjectData data)
        {
            // 선택된 슬롯 우선 소비
            if (!slots[selectedSlotIndex].IsEmpty && slots[selectedSlotIndex].data == data)
            {
                return ConsumeFromSlot(selectedSlotIndex);
            }

            // 다른 슬롯 탐색
            for (int i = 0; i < SlotCount; i++)
            {
                if (!slots[i].IsEmpty && slots[i].data == data)
                {
                    return ConsumeFromSlot(i);
                }
            }
            return false;
        }

        private bool ConsumeFromSlot(int index)
        {
            if (slots[index].IsEmpty) return false;
            slots[index].count--;
            if (slots[index].count <= 0)
            {
                slots[index].Clear();
                // 선택 슬롯이 비었으면 다른 슬롯으로 이동
                if (selectedSlotIndex == index)
                    SelectNextAvailableSlot();
            }
            OnInventoryChanged?.Invoke();
            return true;
        }

        private void SelectNextAvailableSlot()
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    selectedSlotIndex = i;
                    return;
                }
            }
            // 모두 비어 있으면 0번 유지
            selectedSlotIndex = 0;
        }

        /// <summary>특정 슬롯 아이템 수 반환</summary>
        public int GetSlotCount(int index)
        {
            if (index < 0 || index >= SlotCount || slots[index].IsEmpty) return 0;
            return slots[index].count;
        }

        /// <summary>특정 BuildableObjectData 총 개수 반환 (호환성용)</summary>
        public int GetItemCount(BuildableObjectData data)
        {
            int total = 0;
            for (int i = 0; i < SlotCount; i++)
            {
                if (!slots[i].IsEmpty && slots[i].data == data)
                    total += slots[i].count;
            }
            return total;
        }

        /// <summary>인벤토리가 꽉 찼는지 여부</summary>
        public bool IsFull()
        {
            for (int i = 0; i < SlotCount; i++)
                if (slots[i].IsEmpty) return false;
            return true;
        }

        public void SelectItem(BuildableObjectData data)
        {
            for (int i = 0; i < SlotCount; i++)
            {
                if (!slots[i].IsEmpty && slots[i].data == data)
                {
                    SelectSlot(i);
                    return;
                }
            }
        }

        // ─────────────────────── Pickup / Hover ───────────────────────

        private void CheckForPickupableResource()
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            Ray ray = mainCam.ScreenPointToRay(GetMousePosition());
            RaycastHit hit;

            ResourceObject foundResource = null;
            if (Physics.Raycast(ray, out hit, 20f))
            {
                ResourceObject res = hit.collider.GetComponentInParent<ResourceObject>();
                if (res != null)
                {
                    float dist = Vector3.Distance(transform.position, res.transform.position);
                    if (dist <= pickupRange)
                        foundResource = res;
                }
            }

            if (currentHoveredResource != foundResource)
            {
                if (currentHoveredResource != null) currentHoveredResource.SetHovered(false);
                currentHoveredResource = foundResource;
                if (currentHoveredResource != null) currentHoveredResource.SetHovered(true);
                OnResourceHovered?.Invoke(currentHoveredResource);
            }
        }

        public void PickupResource(ResourceObject res)
        {
            if (res == null || res.objectData == null) return;

            if (IsFull())
            {
                Debug.LogWarning("[Inventory] 핫바가 가득 찼습니다. 먼저 배치하거나 분해하세요!");
                return;
            }

            bool added = AddItem(res.objectData, 1);
            if (!added) return;

            if (currentHoveredResource == res)
            {
                currentHoveredResource.SetHovered(false);
                currentHoveredResource = null;
                OnResourceHovered?.Invoke(null);
            }

            Destroy(res.gameObject);
        }

        public ResourceObject GetHoveredResource() => currentHoveredResource;

        // ─────────────────────── Input Helpers ───────────────────────

        private Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Vector3.zero;
#else
            return Input.mousePosition;
#endif
        }

        private bool IsEPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.E);
#endif
        }
    }
}
