using System.Collections.Generic;
using SculptGame.Player;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SculptGame.Building
{
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        [Header("Building Settings")]
        public LayerMask buildableLayerMask;
        public float maxBuildDistance = 25f;
        public bool useGridSnap = true;
        public float gridSize = 0.5f;

        [Header("Object Limits")]
        public int maxObjectsPerRound = 50;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Dictionary<GameObject, BuildableObjectData> objectDataMap = new Dictionary<GameObject, BuildableObjectData>();

        [Header("Current Selection")]
        public BuildableObjectData selectedObjectData;

        [Header("Ghost Preview Materials")]
        public Material validGhostMaterial;
        public Material invalidGhostMaterial;

        private GameObject ghostObject;
        private float currentRotationY = 0f;
        private Camera mainCam;
        private PlacableObject currentHoveredObject;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            mainCam = Camera.main;
            CreateGhostMaterialsIfNeeded();
        }

        private void Update()
        {
            if (mainCam == null) mainCam = Camera.main;
            if (mainCam == null) return;

            // Sync selection with Inventory
            if (PlayerInventory.Instance != null)
            {
                selectedObjectData = PlayerInventory.Instance.SelectedItem;
            }

            // Handle Object Rotation (Q / E keys or Mouse Scroll)
            HandleRotationInput();

            Ray ray = mainCam.ScreenPointToRay(GetMousePosition());
            RaycastHit hit;
            bool hitSomething = Physics.Raycast(ray, out hit, maxBuildDistance, buildableLayerMask);

            // Handle Ghost Preview & Placement
            bool hasInventoryItem = selectedObjectData != null && PlayerInventory.Instance != null && PlayerInventory.Instance.GetItemCount(selectedObjectData) > 0;
            if (hasInventoryItem)
            {
                UpdateGhostPreview(hitSomething, hit);

                // Place Object on Left Click
                if (IsLeftClickPressed() && hitSomething)
                {
                    TryPlaceObject(hit);
                }
            }
            else
            {
                DestroyGhost();
            }

            // Handle Hover & Delete/Pickup Object (Right Click / R Key)
            HandleHoverAndDelete(hitSomething, hit);
        }

        public void SelectObjectToBuild(BuildableObjectData objectData)
        {
            selectedObjectData = objectData;
            DestroyGhost();
        }

        private void UpdateGhostPreview(bool hitSomething, RaycastHit hit)
        {
            if (!hitSomething)
            {
                if (ghostObject != null) ghostObject.SetActive(false);
                return;
            }

            Vector3 spawnPosition = hit.point;
            if (useGridSnap)
            {
                spawnPosition = SnapToGrid(spawnPosition);
            }

            spawnPosition += hit.normal * 0.01f;

            if (ghostObject == null)
            {
                CreateGhostObject();
            }

            if (ghostObject != null)
            {
                ghostObject.SetActive(true);
                ghostObject.transform.position = spawnPosition;
                ghostObject.transform.rotation = Quaternion.Euler(0f, currentRotationY, 0f);

                bool inBounds = BuildingCanvas.Instance == null || BuildingCanvas.Instance.IsPositionInCanvas(spawnPosition);
                bool underLimit = spawnedObjects.Count < maxObjectsPerRound;
                SetGhostValid(inBounds && underLimit);
            }
        }

        private void TryPlaceObject(RaycastHit hit)
        {
            if (selectedObjectData == null) return;
            if (PlayerInventory.Instance != null && PlayerInventory.Instance.GetItemCount(selectedObjectData) <= 0)
            {
                Debug.LogWarning("소지한 재료 오브젝트가 없습니다! 맵에서 재료를 먼저 주워오세요.");
                return;
            }

            if (spawnedObjects.Count >= maxObjectsPerRound)
            {
                Debug.LogWarning("Maximum object limit reached for this round!");
                return;
            }

            Vector3 spawnPos = hit.point;
            if (useGridSnap) spawnPos = SnapToGrid(spawnPos);

            if (BuildingCanvas.Instance != null && !BuildingCanvas.Instance.IsPositionInCanvas(spawnPos))
            {
                Debug.LogWarning("Cannot place object outside the Building Canvas!");
                return;
            }

            // Consume 1 item from inventory
            if (PlayerInventory.Instance != null)
            {
                if (!PlayerInventory.Instance.ConsumeItem(selectedObjectData)) return;
            }

            Quaternion spawnRot = Quaternion.Euler(0f, currentRotationY, 0f);

            GameObject newObj = null;
            if (selectedObjectData.prefab != null)
            {
                newObj = Instantiate(selectedObjectData.prefab, spawnPos, spawnRot);
            }
            else
            {
                newObj = GameObject.CreatePrimitive(selectedObjectData.primitiveShape);
                newObj.transform.position = spawnPos;
                newObj.transform.rotation = spawnRot;
                newObj.transform.localScale = selectedObjectData.defaultScale;

                Renderer rend = newObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = selectedObjectData.defaultColor;
                }
            }

            PlacableObject placable = newObj.GetComponent<PlacableObject>();
            if (placable == null) placable = newObj.AddComponent<PlacableObject>();
            placable.objectId = selectedObjectData.objectId;
            placable.objectName = selectedObjectData.displayName;

            newObj.layer = gameObject.layer;

            spawnedObjects.Add(newObj);
            objectDataMap[newObj] = selectedObjectData;
        }

        private void HandleHoverAndDelete(bool hitSomething, RaycastHit hit)
        {
            PlacableObject hitObject = null;
            if (hitSomething && hit.collider != null)
            {
                hitObject = hit.collider.GetComponentInParent<PlacableObject>();
            }

            if (currentHoveredObject != hitObject)
            {
                if (currentHoveredObject != null)
                {
                    currentHoveredObject.SetHighlight(false, Color.white);
                }
                currentHoveredObject = hitObject;
                if (currentHoveredObject != null)
                {
                    currentHoveredObject.SetHighlight(true, Color.red);
                }
            }

            // Right click or R key to disassemble object back into inventory
            if (currentHoveredObject != null && (IsRightClickPressed() || IsRKeyPressed()))
            {
                GameObject targetObj = currentHoveredObject.gameObject;
                if (objectDataMap.TryGetValue(targetObj, out BuildableObjectData data))
                {
                    if (PlayerInventory.Instance != null)
                    {
                        PlayerInventory.Instance.AddItem(data, 1);
                    }
                    objectDataMap.Remove(targetObj);
                }

                spawnedObjects.Remove(targetObj);
                Destroy(targetObj);
                currentHoveredObject = null;
            }
        }

        public void ClearAllObjects()
        {
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null) Destroy(obj);
            }
            spawnedObjects.Clear();
            objectDataMap.Clear();
            DestroyGhost();
        }

        public int GetPlacedObjectCount()
        {
            return spawnedObjects.Count;
        }

        private Vector3 SnapToGrid(Vector3 pos)
        {
            return new Vector3(
                Mathf.Round(pos.x / gridSize) * gridSize,
                Mathf.Round(pos.y / gridSize) * gridSize,
                Mathf.Round(pos.z / gridSize) * gridSize
            );
        }

        private void HandleRotationInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                if (Keyboard.current.qKey.wasPressedThisFrame) currentRotationY -= 45f;
                if (Keyboard.current.eKey.wasPressedThisFrame) currentRotationY += 45f;
            }
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll > 0.1f) currentRotationY += 45f;
                else if (scroll < -0.1f) currentRotationY -= 45f;
            }
#else
            if (Input.GetKeyDown(KeyCode.Q)) currentRotationY -= 45f;
            if (Input.GetKeyDown(KeyCode.E)) currentRotationY += 45f;
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0.05f) currentRotationY += 45f;
            else if (scroll < -0.05f) currentRotationY -= 45f;
#endif
        }

        private void CreateGhostObject()
        {
            if (selectedObjectData == null) return;

            if (selectedObjectData.prefab != null)
            {
                ghostObject = Instantiate(selectedObjectData.prefab);
            }
            else
            {
                ghostObject = GameObject.CreatePrimitive(selectedObjectData.primitiveShape);
                ghostObject.transform.localScale = selectedObjectData.defaultScale;
            }

            Collider[] colliders = ghostObject.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) Destroy(col);

            SetGhostValid(true);
        }

        private void SetGhostValid(bool isValid)
        {
            if (ghostObject == null) return;

            Material targetMat = isValid ? validGhostMaterial : invalidGhostMaterial;
            Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
            foreach (var rend in renderers)
            {
                if (targetMat != null) rend.material = targetMat;
            }
        }

        private void DestroyGhost()
        {
            if (ghostObject != null)
            {
                Destroy(ghostObject);
                ghostObject = null;
            }
        }

        private void CreateGhostMaterialsIfNeeded()
        {
            if (validGhostMaterial == null)
            {
                validGhostMaterial = new Material(Shader.Find("Sprites/Default"));
                validGhostMaterial.color = new Color(0.2f, 1f, 0.3f, 0.5f);
            }
            if (invalidGhostMaterial == null)
            {
                invalidGhostMaterial = new Material(Shader.Find("Sprites/Default"));
                invalidGhostMaterial.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            }
        }

        private Vector3 GetMousePosition()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.position.ReadValue();
            return Vector3.zero;
#else
            return Input.mousePosition;
#endif
        }

        private bool IsLeftClickPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        private bool IsRightClickPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            return Input.GetMouseButtonDown(1);
#endif
        }

        private bool IsRKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.R);
#endif
        }
    }
}
