#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using SculptGame.AI;
using SculptGame.Building;
using SculptGame.Game;
using SculptGame.Player;
using SculptGame.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace SculptGame.EditorTools
{
    public class GameSetupUtility : EditorWindow
    {
        [MenuItem("Tools/Sculpt Game/Auto Setup Scene & Managers")]
        public static void SetupCompleteScene()
        {
            // 1. Create Base Managers Parent GameObject
            GameObject managersObj = GameObject.Find("--- MANAGERS ---");
            if (managersObj == null) managersObj = new GameObject("--- MANAGERS ---");

            GameManager gm = managersObj.GetComponent<GameManager>();
            if (gm == null) gm = managersObj.AddComponent<GameManager>();

            RoundManager rm = managersObj.GetComponent<RoundManager>();
            if (rm == null) rm = managersObj.AddComponent<RoundManager>();

            AIVisionEvaluator eval = managersObj.GetComponent<AIVisionEvaluator>();
            if (eval == null) eval = managersObj.AddComponent<AIVisionEvaluator>();

            BuildingSystem buildSys = managersObj.GetComponent<BuildingSystem>();
            if (buildSys == null) buildSys = managersObj.AddComponent<BuildingSystem>();

            // 2. Setup Environment, Lighting & Ground Canvas
            GameObject envObj = GameObject.Find("--- ENVIRONMENT ---");
            if (envObj == null) envObj = new GameObject("--- ENVIRONMENT ---");

            // Setup Bright Sun Light
            Light sunLight = Object.FindFirstObjectByType<Light>();
            GameObject lightObj;
            if (sunLight == null)
            {
                lightObj = new GameObject("Sun Light");
                lightObj.transform.SetParent(envObj.transform);
                sunLight = lightObj.AddComponent<Light>();
                sunLight.type = LightType.Directional;
            }
            else
            {
                lightObj = sunLight.gameObject;
            }

            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            sunLight.color = new Color(1.0f, 0.97f, 0.92f);
            sunLight.intensity = 1.4f;
            sunLight.shadows = LightShadows.Soft;

            // Setup Ground Floor
            GameObject floorObj = GameObject.Find("BuildingCanvasFloor");
            if (floorObj == null)
            {
                floorObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floorObj.name = "BuildingCanvasFloor";
                floorObj.transform.SetParent(envObj.transform);
                floorObj.transform.position = Vector3.zero;

                Renderer rend = floorObj.GetComponent<Renderer>();
                if (rend != null)
                {
                    Material mat = new Material(Shader.Find("Universal Render Pipelines/Lit"));
                    if (mat == null || mat.shader == null) mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.88f, 0.92f, 0.96f);
                    rend.material = mat;
                }
            }

            // Always force-update floor scale (covers full 220m spawn area)
            floorObj.transform.localScale = new Vector3(25f, 1f, 25f); // 250x250

            BuildingCanvas canvasComp = floorObj.GetComponent<BuildingCanvas>();
            if (canvasComp == null) canvasComp = floorObj.AddComponent<BuildingCanvas>();
            canvasComp.canvasSize = new Vector3(30f, 10f, 30f);

            buildSys.buildableLayerMask = ~0; // All layers

            // 3. Setup Player & Top-Down Camera & Inventory
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj == null)
            {
                playerObj = new GameObject("Player");
                playerObj.transform.position = Vector3.zero;

                // Create Visual Mesh Child
                GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                visualObj.name = "Visual";
                visualObj.transform.SetParent(playerObj.transform);
                visualObj.transform.localPosition = new Vector3(0f, 1.0f, 0f);
                visualObj.transform.localRotation = Quaternion.identity;

                // Remove collider from visual child to avoid collision conflict
                Collider visCol = visualObj.GetComponent<Collider>();
                if (visCol != null) Object.DestroyImmediate(visCol);

                // Add visual material
                Renderer pRend = visualObj.GetComponent<Renderer>();
                if (pRend != null)
                {
                    Material pMat = new Material(Shader.Find("Universal Render Pipelines/Lit"));
                    if (pMat == null || pMat.shader == null) pMat = new Material(Shader.Find("Standard"));
                    pMat.color = new Color(0.2f, 0.6f, 0.95f); // Bright blue player character
                    pRend.material = pMat;
                }

                CharacterController cc = playerObj.AddComponent<CharacterController>();
                cc.height = 2.0f;
                cc.radius = 0.4f;
                cc.center = new Vector3(0, 1.0f, 0);

                playerObj.AddComponent<PlayerController>();
            }

            PlayerInventory inv = playerObj.GetComponent<PlayerInventory>();
            if (inv == null) inv = playerObj.AddComponent<PlayerInventory>();

            ResourceSpawner spawner = managersObj.GetComponent<ResourceSpawner>();
            if (spawner == null) spawner = managersObj.AddComponent<ResourceSpawner>();

            if (spawner.availableObjectTypes == null || spawner.availableObjectTypes.Count == 0)
            {
                spawner.availableObjectTypes = new List<BuildableObjectData>
                {
                    CreateObjectData("cube", "큐브 (Cube)", PrimitiveType.Cube, new Color(0.85f, 0.85f, 0.85f)),
                    CreateObjectData("sphere", "구 (Sphere)", PrimitiveType.Sphere, new Color(0.95f, 0.4f, 0.4f)),
                    CreateObjectData("cylinder", "원통 (Cylinder)", PrimitiveType.Cylinder, new Color(0.35f, 0.75f, 0.95f)),
                    CreateObjectData("capsule", "캡슐 (Capsule)", PrimitiveType.Capsule, new Color(0.95f, 0.85f, 0.3f))
                };
            }

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }

            // Unparent camera for Top-Down follow
            mainCam.transform.SetParent(null);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color(0.80f, 0.88f, 0.96f);

            CameraController camCtrl = mainCam.GetComponent<CameraController>();
            if (camCtrl == null) camCtrl = mainCam.gameObject.AddComponent<CameraController>();
            camCtrl.target = playerObj.transform;
            camCtrl.followOffset = new Vector3(0f, 14f, -10f);
            camCtrl.cameraAngle = new Vector3(50f, 0f, 0f);
            mainCam.transform.position = playerObj.transform.position + camCtrl.followOffset;
            mainCam.transform.rotation = Quaternion.Euler(camCtrl.cameraAngle);

            if (eval != null) eval.evalCamera = mainCam;

            // 4. Setup EventSystem with InputSystemUIInputModule
            EventSystem existingEs = Object.FindFirstObjectByType<EventSystem>();
            GameObject esObj;
            if (existingEs == null)
            {
                esObj = new GameObject("EventSystem");
                existingEs = esObj.AddComponent<EventSystem>();
            }
            else
            {
                esObj = existingEs.gameObject;
            }

            // Remove legacy StandaloneInputModule if present
            StandaloneInputModule oldModule = esObj.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                DestroyImmediate(oldModule);
            }

#if ENABLE_INPUT_SYSTEM
            if (esObj.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                esObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
#else
            if (esObj.GetComponent<StandaloneInputModule>() == null)
            {
                esObj.AddComponent<StandaloneInputModule>();
            }
#endif

            // 5. Setup UI Canvas Hierarchy
            SetupUICanvas(managersObj);

            // Save and Mark Scene Dirty
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            Debug.Log("✅ [Sculpt Game] Auto Setup Complete! UI Canvas, EventSystem, Managers, Ground Canvas Floor, and 3D Player Controller fully connected.");
            EditorUtility.DisplayDialog("Setup Complete", "100% 자동 세팅 완료!\n\nUI 캔버스, 매니저, 3D 플레이어, 캔버스 바닥이 완벽하게 자동 연결되었습니다.\n인벤토리 슬롯: 1칸", "확인");
        }

        private static void SetupUICanvas(GameObject managersObj)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            GameObject canvasObj;
            if (canvas == null)
            {
                canvasObj = new GameObject("GameUI_Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }
            else
            {
                canvasObj = canvas.gameObject;
            }

            UIManager uiMgr = managersObj.GetComponent<UIManager>();
            if (uiMgr == null) uiMgr = managersObj.AddComponent<UIManager>();

            // --- Lobby Panel ---
            GameObject lobbyPanel = FindOrCreateChild(canvasObj, "LobbyPanel");
            SetRectFull(lobbyPanel);
            Image lobbyBg = GetOrAdd<Image>(lobbyPanel);
            lobbyBg.color = new Color(0.1f, 0.12f, 0.18f, 0.95f);

            GameObject titleTextObj = FindOrCreateChild(lobbyPanel, "TitleText");
            TextMeshProUGUI titleTxt = GetOrAdd<TextMeshProUGUI>(titleTextObj);
            titleTxt.text = "AI 협동 조형 게임";
            titleTxt.fontSize = 54;
            titleTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(titleTxt);
            SetRectAnchor(titleTextObj, new Vector2(0.5f, 0.75f), new Vector2(400, 100));

            GameObject startBtnObj = FindOrCreateChild(lobbyPanel, "StartButton");
            SetRectAnchor(startBtnObj, new Vector2(0.5f, 0.45f), new Vector2(280, 70));
            Image startBtnImg = GetOrAdd<Image>(startBtnObj);
            startBtnImg.color = new Color(0.2f, 0.7f, 0.3f);
            Button startBtn = GetOrAdd<Button>(startBtnObj);

            GameObject startBtnTextObj = FindOrCreateChild(startBtnObj, "Text");
            TextMeshProUGUI startBtnTxt = GetOrAdd<TextMeshProUGUI>(startBtnTextObj);
            startBtnTxt.text = "게임 시작 ▶";
            startBtnTxt.fontSize = 32;
            startBtnTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(startBtnTxt);
            SetRectFull(startBtnTextObj);

            LobbyUI lobbyComp = GetOrAdd<LobbyUI>(lobbyPanel);
            lobbyComp.startGameButton = startBtn;

            // --- Topic Announce Panel ---
            GameObject topicPanel = FindOrCreateChild(canvasObj, "TopicAnnouncePanel");
            SetRectFull(topicPanel);
            Image topicBg = GetOrAdd<Image>(topicPanel);
            topicBg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

            GameObject topicTitleObj = FindOrCreateChild(topicPanel, "TopicTitleText");
            TextMeshProUGUI topicTitleTxt = GetOrAdd<TextMeshProUGUI>(topicTitleObj);
            topicTitleTxt.text = "주제: 자동차";
            topicTitleTxt.fontSize = 60;
            topicTitleTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(topicTitleTxt);
            SetRectAnchor(topicTitleObj, new Vector2(0.5f, 0.6f), new Vector2(800, 120));

            GameObject topicDescObj = FindOrCreateChild(topicPanel, "TopicDescText");
            TextMeshProUGUI topicDescTxt = GetOrAdd<TextMeshProUGUI>(topicDescObj);
            topicDescTxt.text = "제한 시간 동안 캔버스에 주제어를 조형하세요!";
            topicDescTxt.fontSize = 32;
            topicDescTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(topicDescTxt);
            SetRectAnchor(topicDescObj, new Vector2(0.5f, 0.45f), new Vector2(800, 100));

            uiMgr.topicAnnounceTitleText = topicTitleTxt;
            uiMgr.topicAnnounceDescText = topicDescTxt;

            // --- Building HUD Panel ---
            GameObject hudPanel = FindOrCreateChild(canvasObj, "BuildingHUDPanel");
            SetRectFull(hudPanel);

            // ═══════════════════════════════════════════════
            // TOP BAR — 타이머 + 주제어 (중앙 상단)
            // ═══════════════════════════════════════════════
            GameObject topBarObj = FindOrCreateChild(hudPanel, "TopBar");
            SetRectAnchor(topBarObj, new Vector2(0.5f, 0.93f), new Vector2(400, 120));
            Image topBarBg = GetOrAdd<Image>(topBarObj);
            topBarBg.color = new Color(0.1f, 0.1f, 0.15f, 0.7f);  // 회색 반투명 패널

            // 타이머 (크고 중앙)
            GameObject hudTimerObj = FindOrCreateChild(topBarObj, "TimerText");
            RectTransform timerRt = GetOrAdd<RectTransform>(hudTimerObj);
            timerRt.anchorMin = new Vector2(0.5f, 0.5f);
            timerRt.anchorMax = new Vector2(0.5f, 0.5f);
            timerRt.sizeDelta = new Vector2(300, 60);
            timerRt.anchoredPosition = new Vector2(0f, 30f);
            TextMeshProUGUI hudTimerTxt = GetOrAdd<TextMeshProUGUI>(hudTimerObj);
            hudTimerTxt.text = "02:00";
            hudTimerTxt.fontSize = 52;
            hudTimerTxt.fontStyle = FontStyles.Bold;
            hudTimerTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(hudTimerTxt);

            // 주제어 (타이머 아래)
            GameObject hudTopicObj = FindOrCreateChild(topBarObj, "TopicText");
            RectTransform topicRt = GetOrAdd<RectTransform>(hudTopicObj);
            topicRt.anchorMin = new Vector2(0.5f, 0.5f);
            topicRt.anchorMax = new Vector2(0.5f, 0.5f);
            topicRt.sizeDelta = new Vector2(300, 40);
            topicRt.anchoredPosition = new Vector2(0f, -20f);
            TextMeshProUGUI hudTopicTxt = GetOrAdd<TextMeshProUGUI>(hudTopicObj);
            hudTopicTxt.text = "자동차";
            hudTopicTxt.fontSize = 28;
            hudTopicTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(hudTopicTxt);

            // Early Submit Button (우측 상단)
            GameObject submitBtnObj = FindOrCreateChild(hudPanel, "SubmitButton");
            SetRectAnchor(submitBtnObj, new Vector2(0.9f, 0.93f), new Vector2(160, 60));
            Image submitBtnImg = GetOrAdd<Image>(submitBtnObj);
            submitBtnImg.color = new Color(0.9f, 0.5f, 0.1f);
            Button submitBtn = GetOrAdd<Button>(submitBtnObj);

            GameObject submitTxtObj = FindOrCreateChild(submitBtnObj, "Text");
            TextMeshProUGUI submitTxt = GetOrAdd<TextMeshProUGUI>(submitTxtObj);
            submitTxt.text = "AI 채점 요청";
            submitTxt.fontSize = 20;
            submitTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(submitTxt);
            SetRectFull(submitTxtObj);

            HUDUI hudComp = GetOrAdd<HUDUI>(hudPanel);
            hudComp.topicText = hudTopicTxt;
            hudComp.timerText = hudTimerTxt;
            hudComp.targetScoreText = null;  // 제거
            hudComp.submitEarlyButton = submitBtn;

            // ═══════════════════════════════════════════════
            // HOTBAR — 하단 중앙 1슬롯 핫바 (약간 작게)
            // ═══════════════════════════════════════════════
            GameObject hotbarRoot = FindOrCreateChild(hudPanel, "HotbarRoot");
            
            // 기존 슬롯 정리
            for (int i = hotbarRoot.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = hotbarRoot.transform.GetChild(i);
                if (child.name.StartsWith("Slot_"))
                {
                    DestroyImmediate(child.gameObject);
                }
            }
            
            SetRectAnchorBottom(hotbarRoot, new Vector2(0.5f, 0f), new Vector2(160f, 100f), new Vector2(0f, 20f));

            // 반투명 배경 패널
            Image hotbarBg = GetOrAdd<Image>(hotbarRoot);
            hotbarBg.color = new Color(0.04f, 0.05f, 0.12f, 0.88f);

            HotbarUI hotbarUI = GetOrAdd<HotbarUI>(hotbarRoot);
            hotbarUI.slotRoots   = new GameObject[PlayerInventory.SlotCount];
            hotbarUI.slotBgImages   = new Image[PlayerInventory.SlotCount];
            hotbarUI.slotIconImages = new Image[PlayerInventory.SlotCount];
            hotbarUI.slotNameTexts  = new TextMeshProUGUI[PlayerInventory.SlotCount];
            hotbarUI.slotCountTexts = new TextMeshProUGUI[PlayerInventory.SlotCount];
            hotbarUI.slotOutlines   = new Image[PlayerInventory.SlotCount];

            float slotW = 136f;
            float slotH = 86f;
            float centerX = 0f;

            for (int si = 0; si < PlayerInventory.SlotCount; si++)
            {
                string slotName = $"Slot_{si + 1}";
                GameObject slotObj = FindOrCreateChild(hotbarRoot, slotName);
                RectTransform slotRt = GetOrAdd<RectTransform>(slotObj);
                slotRt.anchorMin = new Vector2(0.5f, 0.5f);
                slotRt.anchorMax = new Vector2(0.5f, 0.5f);
                slotRt.sizeDelta = new Vector2(slotW, slotH);
                slotRt.anchoredPosition = new Vector2(centerX, 0f);

                // 외곽선
                Image outlineImg = GetOrAdd<Image>(slotObj);
                outlineImg.color = new Color(1f, 1f, 1f, 0.15f);
                hotbarUI.slotOutlines[si] = outlineImg;

                // 배경
                GameObject bgObj = FindOrCreateChild(slotObj, "BG");
                RectTransform bgRt = GetOrAdd<RectTransform>(bgObj);
                bgRt.anchorMin = Vector2.zero;
                bgRt.anchorMax = Vector2.one;
                bgRt.offsetMin = new Vector2(2f, 2f);
                bgRt.offsetMax = new Vector2(-2f, -2f);
                Image bgImg = GetOrAdd<Image>(bgObj);
                bgImg.color = new Color(0.05f, 0.06f, 0.12f, 0.88f);
                hotbarUI.slotBgImages[si] = bgImg;

                // 아이콘
                GameObject iconObj = FindOrCreateChild(slotObj, "Icon");
                RectTransform iconRt = GetOrAdd<RectTransform>(iconObj);
                iconRt.anchorMin = new Vector2(0f, 0.5f);
                iconRt.anchorMax = new Vector2(0f, 0.5f);
                iconRt.sizeDelta = new Vector2(56f, 56f);
                iconRt.anchoredPosition = new Vector2(34f, 0f);
                Image iconImg = GetOrAdd<Image>(iconObj);
                iconImg.color = new Color(0.35f, 0.35f, 0.35f, 0.35f);
                hotbarUI.slotIconImages[si] = iconImg;

                // 슬롯 번호 + 이름 텍스트
                GameObject nameObj = FindOrCreateChild(slotObj, "NameText");
                RectTransform nameRt = GetOrAdd<RectTransform>(nameObj);
                nameRt.anchorMin = new Vector2(0f, 0.5f);
                nameRt.anchorMax = new Vector2(1f, 0.5f);
                nameRt.offsetMin = new Vector2(68f, 6f);
                nameRt.offsetMax = new Vector2(-6f, 30f);
                TextMeshProUGUI nameTxt = GetOrAdd<TextMeshProUGUI>(nameObj);
                nameTxt.text      = $"[{si + 1}]";
                nameTxt.fontSize  = 14f;
                nameTxt.alignment = TextAlignmentOptions.Left;
                nameTxt.color     = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                ApplyGalmuriFont(nameTxt);
                hotbarUI.slotNameTexts[si] = nameTxt;

                // 수량 텍스트
                GameObject countObj = FindOrCreateChild(slotObj, "CountText");
                RectTransform countRt = GetOrAdd<RectTransform>(countObj);
                countRt.anchorMin = new Vector2(0f, 0f);
                countRt.anchorMax = new Vector2(1f, 0f);
                countRt.offsetMin = new Vector2(68f, 6f);
                countRt.offsetMax = new Vector2(-6f, 24f);
                TextMeshProUGUI countTxt = GetOrAdd<TextMeshProUGUI>(countObj);
                countTxt.text      = "";
                countTxt.fontSize  = 20f;
                countTxt.fontStyle = FontStyles.Bold;
                countTxt.alignment = TextAlignmentOptions.Left;
                countTxt.color     = Color.white;
                ApplyGalmuriFont(countTxt);
                hotbarUI.slotCountTexts[si] = countTxt;

                hotbarUI.slotRoots[si] = slotObj;
            }

            // 줍기 힌트 텍스트 (핫바 위)
            GameObject hintObj = FindOrCreateChild(hotbarRoot, "PickupHint");
            RectTransform hintRt = GetOrAdd<RectTransform>(hintObj);
            hintRt.anchorMin = new Vector2(0f, 1f);
            hintRt.anchorMax = new Vector2(1f, 1f);
            hintRt.offsetMin = new Vector2(0f, 4f);
            hintRt.offsetMax = new Vector2(0f, 28f);
            TextMeshProUGUI hintTxt = GetOrAdd<TextMeshProUGUI>(hintObj);
            hintTxt.text      = "";
            hintTxt.fontSize  = 16f;
            hintTxt.alignment = TextAlignmentOptions.Center;
            hintTxt.color     = new Color(0.9f, 1f, 0.5f);
            ApplyGalmuriFont(hintTxt);
            hotbarUI.pickupHintText = hintTxt;

            // --- Scoring Loading Panel ---
            GameObject scoringPanel = FindOrCreateChild(canvasObj, "ScoringLoadingPanel");
            SetRectFull(scoringPanel);
            Image scoringBg = GetOrAdd<Image>(scoringPanel);
            scoringBg.color = new Color(0.05f, 0.05f, 0.1f, 0.85f);

            GameObject scoringTextObj = FindOrCreateChild(scoringPanel, "ScoringText");
            TextMeshProUGUI scoringTxt = GetOrAdd<TextMeshProUGUI>(scoringTextObj);
            scoringTxt.text = "AI 심사위원이 작품을 채점하는 중입니다...";
            scoringTxt.fontSize = 36;
            scoringTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(scoringTxt);
            SetRectAnchor(scoringTextObj, new Vector2(0.5f, 0.5f), new Vector2(800, 100));

            // --- AI Result Modal Panel ---
            GameObject resultPanel = FindOrCreateChild(canvasObj, "ResultModalPanel");
            SetRectFull(resultPanel);
            Image resultBg = GetOrAdd<Image>(resultPanel);
            resultBg.color = new Color(0.05f, 0.05f, 0.08f, 0.95f);

            GameObject resultCardObj = FindOrCreateChild(resultPanel, "ResultCard");
            SetRectAnchor(resultCardObj, new Vector2(0.5f, 0.5f), new Vector2(700, 600));
            Image cardBg = GetOrAdd<Image>(resultCardObj);
            cardBg.color = new Color(0.15f, 0.18f, 0.25f);

            GameObject rawImgObj = FindOrCreateChild(resultCardObj, "CapturedPreview");
            SetRectAnchor(rawImgObj, new Vector2(0.5f, 0.7f), new Vector2(320, 240));
            RawImage rawImg = GetOrAdd<RawImage>(rawImgObj);

            GameObject scoreTxtObj = FindOrCreateChild(resultCardObj, "ScoreText");
            TextMeshProUGUI resultScoreTxt = GetOrAdd<TextMeshProUGUI>(scoreTxtObj);
            resultScoreTxt.text = "85점";
            resultScoreTxt.fontSize = 50;
            resultScoreTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(resultScoreTxt);
            SetRectAnchor(scoreTxtObj, new Vector2(0.5f, 0.42f), new Vector2(300, 60));

            GameObject statusTxtObj = FindOrCreateChild(resultCardObj, "StatusText");
            TextMeshProUGUI resultStatusTxt = GetOrAdd<TextMeshProUGUI>(statusTxtObj);
            resultStatusTxt.text = "통과 (PASS)";
            resultStatusTxt.fontSize = 32;
            resultStatusTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(resultStatusTxt);
            SetRectAnchor(statusTxtObj, new Vector2(0.5f, 0.33f), new Vector2(300, 50));

            GameObject commentTxtObj = FindOrCreateChild(resultCardObj, "CommentText");
            TextMeshProUGUI resultCommentTxt = GetOrAdd<TextMeshProUGUI>(commentTxtObj);
            resultCommentTxt.text = "AI 심사평: \"차체와 바퀴 표현이 우수합니다.\"";
            resultCommentTxt.fontSize = 22;
            resultCommentTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(resultCommentTxt);
            SetRectAnchor(commentTxtObj, new Vector2(0.5f, 0.2f), new Vector2(600, 80));

            GameObject nextBtnObj = FindOrCreateChild(resultCardObj, "NextRoundButton");
            SetRectAnchor(nextBtnObj, new Vector2(0.5f, 0.08f), new Vector2(240, 50));
            Image nextBtnImg = GetOrAdd<Image>(nextBtnObj);
            nextBtnImg.color = new Color(0.2f, 0.6f, 0.9f);
            Button nextBtn = GetOrAdd<Button>(nextBtnObj);

            GameObject nextBtnTxtObj = FindOrCreateChild(nextBtnObj, "Text");
            TextMeshProUGUI nextBtnTxt = GetOrAdd<TextMeshProUGUI>(nextBtnTxtObj);
            nextBtnTxt.text = "다음 라운드 진입 →";
            nextBtnTxt.fontSize = 22;
            nextBtnTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(nextBtnTxt);
            SetRectFull(nextBtnTxtObj);

            AIResultUI resultComp = GetOrAdd<AIResultUI>(resultPanel);
            resultComp.capturedImageDisplay = rawImg;
            resultComp.scoreText = resultScoreTxt;
            resultComp.resultStatusText = resultStatusTxt;
            resultComp.commentText = resultCommentTxt;
            resultComp.nextRoundButton = nextBtn;

            // --- Game Over Panel ---
            GameObject gameOverPanel = FindOrCreateChild(canvasObj, "GameOverPanel");
            SetRectFull(gameOverPanel);
            Image gameOverBg = GetOrAdd<Image>(gameOverPanel);
            gameOverBg.color = new Color(0f, 0f, 0f, 0.9f);

            GameObject gameOverTextObj = FindOrCreateChild(gameOverPanel, "GameOverText");
            TextMeshProUGUI gameOverTxt = GetOrAdd<TextMeshProUGUI>(gameOverTextObj);
            gameOverTxt.text = "모든 라운드가 종료되었습니다!";
            gameOverTxt.fontSize = 48;
            gameOverTxt.alignment = TextAlignmentOptions.Center;
            ApplyGalmuriFont(gameOverTxt);
            SetRectAnchor(gameOverTextObj, new Vector2(0.5f, 0.5f), new Vector2(800, 100));

            // Link Panels to UIManager
            uiMgr.lobbyPanel = lobbyPanel;
            uiMgr.topicAnnouncePanel = topicPanel;
            uiMgr.buildingHUDPanel = hudPanel;
            uiMgr.scoringLoadingPanel = scoringPanel;
            uiMgr.resultModalPanel = resultPanel;
            uiMgr.gameOverPanel = gameOverPanel;
        }

        private static TMP_FontAsset _galmuriFont;

        private static TMP_FontAsset GetGalmuriFont()
        {
            if (_galmuriFont != null) return _galmuriFont;
            _galmuriFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Galmuri11 SDF.asset");
            if (_galmuriFont == null)
                Debug.LogWarning("[Sculpt Game] Galmuri11 SDF.asset not found at Assets/Galmuri11 SDF.asset — text will use TMP default font.");
            return _galmuriFont;
        }

        private static void ApplyGalmuriFont(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            TMP_FontAsset font = GetGalmuriFont();
            if (font != null) tmp.font = font;
        }

        private static GameObject FindOrCreateChild(GameObject parent, string name)
        {
            Transform t = parent.transform.Find(name);
            if (t != null) return t.gameObject;
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null) comp = go.AddComponent<T>();
            return comp;
        }

        private static void SetRectFull(GameObject go)
        {
            RectTransform rt = GetOrAdd<RectTransform>(go);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void SetRectAnchor(GameObject go, Vector2 anchor, Vector2 size)
        {
            RectTransform rt = GetOrAdd<RectTransform>(go);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
        }
       
        private static void SetRectAnchorBottom(GameObject go,Vector2 anchor,Vector2 size,Vector2 offset)
        {
            RectTransform rt = GetOrAdd<RectTransform>(go);
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
        }

        private static BuildableObjectData CreateObjectData(string id, string name, PrimitiveType shape, Color color)
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
#endif
