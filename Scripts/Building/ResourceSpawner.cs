using System.Collections.Generic;
using UnityEngine;

namespace SculptGame.Building
{
    public class ResourceSpawner : MonoBehaviour
    {
        public static ResourceSpawner Instance { get; private set; }

        public List<BuildableObjectData> availableObjectTypes = new List<BuildableObjectData>();
        public int initialSpawnCount = 60;
        public Vector2 spawnAreaSize = new Vector2(220f, 220f);
        public Vector2 innerExclusionSize = new Vector2(40f, 40f); // Do not spawn inside Building Canvas

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            EnsureDefaultObjectTypes();
            SpawnInitialResources();
        }

        private void EnsureDefaultObjectTypes()
        {
            if (availableObjectTypes == null || availableObjectTypes.Count == 0)
            {
                availableObjectTypes = new List<BuildableObjectData>
                {
                    CreateData("cube", "큐브 (Cube)", PrimitiveType.Cube, new Color(0.9f, 0.3f, 0.3f)),
                    CreateData("sphere", "구 (Sphere)", PrimitiveType.Sphere, new Color(0.3f, 0.6f, 0.9f)),
                    CreateData("cylinder", "원통 (Cylinder)", PrimitiveType.Cylinder, new Color(0.95f, 0.8f, 0.2f)),
                    CreateData("capsule", "캡슐 (Capsule)", PrimitiveType.Capsule, new Color(0.3f, 0.85f, 0.4f))
                };
            }
        }

        public void SpawnInitialResources()
        {
            if (availableObjectTypes == null || availableObjectTypes.Count == 0) return;

            for (int i = 0; i < initialSpawnCount; i++)
            {
                SpawnSingleRandomResource();
            }
        }

        public void SpawnSingleRandomResource()
        {
            if (availableObjectTypes == null || availableObjectTypes.Count == 0) return;

            BuildableObjectData randomData = availableObjectTypes[Random.Range(0, availableObjectTypes.Count)];
            Vector3 spawnPos = GetRandomOuterPosition();

            GameObject resObj = null;
            if (randomData.prefab != null)
            {
                resObj = Instantiate(randomData.prefab, spawnPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
            }
            else
            {
                resObj = GameObject.CreatePrimitive(randomData.primitiveShape);
                resObj.transform.position = spawnPos;
                resObj.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                resObj.transform.localScale = randomData.defaultScale;

                Renderer rend = resObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = randomData.defaultColor;
                }
            }

            resObj.name = $"Resource_{randomData.displayName}";
            ResourceObject resComp = resObj.GetComponent<ResourceObject>();
            if (resComp == null) resComp = resObj.AddComponent<ResourceObject>();
            resComp.objectData = randomData;
        }

        private Vector3 GetRandomOuterPosition()
        {
            int attempts = 0;
            while (attempts < 100)
            {
                float x = Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f);
                float z = Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f);

                if (Mathf.Abs(x) > innerExclusionSize.x * 0.5f || Mathf.Abs(z) > innerExclusionSize.y * 0.5f)
                {
                    return new Vector3(x, 0.5f, z);
                }
                attempts++;
            }
            return new Vector3(15f, 0.5f, 15f);
        }

        private BuildableObjectData CreateData(string id, string name, PrimitiveType shape, Color color)
        {
            BuildableObjectData data = ScriptableObject.CreateInstance<BuildableObjectData>();
            data.objectId = id;
            data.displayName = name;
            data.primitiveShape = shape;
            data.defaultColor = color;
            data.defaultScale = Vector3.one;
            return data;
        }
    }
}
