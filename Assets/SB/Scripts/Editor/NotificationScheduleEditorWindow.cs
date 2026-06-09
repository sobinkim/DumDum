using SB.App.Application;
using SB.App.Domain;
using UnityEditor;
using UnityEngine;

namespace SB.App.Editor
{
    public sealed class NotificationScheduleEditorWindow : EditorWindow
    {
        private const string CatalogPath = "Assets/SB/ScriptableObjects/NotificationCatalog.asset";

        private NotificationCatalog _catalog;
        private SerializedObject _serializedCatalog;
        private Vector2 _scroll;

        [MenuItem("DumDum/Notification Rules")]
        public static void Open()
        {
            GetWindow<NotificationScheduleEditorWindow>("DumDum 알림 규칙");
        }

        [MenuItem("DumDum/Debug/Clear Daily Closure Lock")]
        public static void ClearDailyClosureLock()
        {
            new DailyClosureService().ClearLockForTesting();
            Debug.Log("[DumDum] 테스트용으로 하루 마무리 잠금을 해제했어요.");
        }

        private void OnEnable()
        {
            LoadOrCreateCatalog();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DumDum 알림 규칙", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("여기서 문구, 트리거, 예약 시간을 바꾸면 런타임 알림 서비스가 그대로 읽어서 스마트폰 알림을 예약합니다.", MessageType.Info);

            NotificationCatalog selectedCatalog = (NotificationCatalog)EditorGUILayout.ObjectField("Catalog", _catalog, typeof(NotificationCatalog), false);
            if (selectedCatalog != _catalog)
            {
                _catalog = selectedCatalog;
                _serializedCatalog = _catalog != null ? new SerializedObject(_catalog) : null;
            }

            if (_catalog == null)
            {
                if (GUILayout.Button("NotificationCatalog 생성"))
                    LoadOrCreateCatalog();

                return;
            }

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("기본 규칙으로 채우기"))
                    ReplaceWithDefaultRules();

                if (GUILayout.Button("고민 결과 확인 추가"))
                    AddRule(CreateWorryReviewRule());

                if (GUILayout.Button("하루 마무리 알림 추가"))
                    AddRule(CreateDailyClosureRule());

                if (GUILayout.Button("1분 테스트 추가"))
                    AddRule(CreateTestRule());
            }

            EditorGUILayout.Space(8f);
            _serializedCatalog.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            SerializedProperty rules = _serializedCatalog.FindProperty("rules");
            EditorGUILayout.PropertyField(rules, new GUIContent("알림 규칙"), true);
            EditorGUILayout.EndScrollView();

            if (_serializedCatalog.ApplyModifiedProperties())
                EditorUtility.SetDirty(_catalog);
        }

        private void LoadOrCreateCatalog()
        {
            _catalog = AssetDatabase.LoadAssetAtPath<NotificationCatalog>(CatalogPath);
            if (_catalog == null)
            {
                EnsureFolder("Assets/SB", "ScriptableObjects");
                _catalog = CreateInstance<NotificationCatalog>();
                _catalog.SetRuntimeRules(NotificationCatalog.CreateDefaultRules());
                AssetDatabase.CreateAsset(_catalog, CatalogPath);
                AssetDatabase.SaveAssets();
            }

            _serializedCatalog = new SerializedObject(_catalog);
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private void ReplaceWithDefaultRules()
        {
            Undo.RecordObject(_catalog, "Replace notification rules");
            _catalog.SetRuntimeRules(NotificationCatalog.CreateDefaultRules());
            EditorUtility.SetDirty(_catalog);
            _serializedCatalog = new SerializedObject(_catalog);
            AssetDatabase.SaveAssets();
        }

        private void AddRule(NotificationRule rule)
        {
            _serializedCatalog.Update();
            SerializedProperty rules = _serializedCatalog.FindProperty("rules");
            int index = rules.arraySize;
            rules.InsertArrayElementAtIndex(index);
            WriteRule(rules.GetArrayElementAtIndex(index), rule);
            _serializedCatalog.ApplyModifiedProperties();
            EditorUtility.SetDirty(_catalog);
            AssetDatabase.SaveAssets();
        }

        private static void WriteRule(SerializedProperty property, NotificationRule rule)
        {
            Set(property, "id", rule.Id);
            Set(property, "displayName", rule.DisplayName);
            Set(property, "enabled", rule.Enabled);
            Set(property, "triggerType", (int)rule.TriggerType);
            Set(property, "routeType", (int)rule.RouteType);
            Set(property, "title", rule.Title);
            Set(property, "body", rule.Body);
            Set(property, "delayDays", rule.DelayDays);
            Set(property, "delayHours", rule.DelayHours);
            Set(property, "delayMinutes", rule.DelayMinutes);
            Set(property, "useFixedClockTime", rule.UseFixedClockTime);
            Set(property, "fixedHour", rule.FixedHour);
            Set(property, "fixedMinute", rule.FixedMinute);
            Set(property, "repeat", rule.Repeat);
            Set(property, "repeatIntervalDays", rule.RepeatIntervalDays);
        }

        private static void Set(SerializedProperty root, string name, string value)
        {
            root.FindPropertyRelative(name).stringValue = value;
        }

        private static void Set(SerializedProperty root, string name, bool value)
        {
            root.FindPropertyRelative(name).boolValue = value;
        }

        private static void Set(SerializedProperty root, string name, int value)
        {
            root.FindPropertyRelative(name).intValue = value;
        }

        private static NotificationRule CreateWorryReviewRule()
        {
            return NotificationRule.Create(
                "worry-review-custom",
                "고민 결과 확인",
                NotificationTriggerType.WorryCardSaved,
                NotificationRouteType.OutcomeReview,
                "며칠 전 적은 걱정이 있어요",
                "실제로 어떻게 되었는지 확인해볼까요?",
                3,
                0,
                0,
                true,
                20,
                0);
        }

        private static NotificationRule CreateDailyClosureRule()
        {
            return NotificationRule.Create(
                "daily-closure-custom",
                "하루 마무리 다음날 알림",
                NotificationTriggerType.DailyClosureCompleted,
                NotificationRouteType.Whiteboard,
                "오늘 다시 정리할 수 있어요",
                "필요하면 가볍게 마음을 정리해볼까요?",
                1,
                0,
                0,
                true,
                12,
                0);
        }

        private static NotificationRule CreateTestRule()
        {
            return NotificationRule.Create(
                "manual-test-custom",
                "1분 뒤 테스트",
                NotificationTriggerType.ManualTest,
                NotificationRouteType.Whiteboard,
                "알림 테스트",
                "DumDum 알림이 정상적으로 예약됐어요.",
                0,
                0,
                1,
                false,
                0,
                0);
        }
    }
}
