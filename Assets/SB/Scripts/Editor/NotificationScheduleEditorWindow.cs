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
        private NotificationTriggerType _previewTrigger = NotificationTriggerType.WorryCardSaved;

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
            EditorGUILayout.HelpBox("문구, 트리거, 예약 시간, 조건을 바꾸면 런타임 알림 서비스가 그대로 읽어서 알림을 예약합니다. ML-Agent 판단 없이 이 규칙만 사용합니다.", MessageType.Info);

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

            DrawPreview();

            EditorGUILayout.Space(8f);
            _serializedCatalog.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            SerializedProperty rules = _serializedCatalog.FindProperty("rules");
            DrawRules(rules);
            EditorGUILayout.EndScrollView();

            if (_serializedCatalog.ApplyModifiedProperties())
                EditorUtility.SetDirty(_catalog);
        }

        private void DrawPreview()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("조건 미리보기", EditorStyles.boldLabel);
            _previewTrigger = (NotificationTriggerType)EditorGUILayout.EnumPopup("트리거", _previewTrigger);

            int enabledCount = 0;
            foreach (NotificationRule rule in _catalog.FindRules(_previewTrigger, NotificationRuleContext.Empty))
            {
                EditorGUILayout.LabelField($"예약 가능: {rule.DisplayName}", EditorStyles.miniLabel);
                enabledCount++;
            }

            if (enabledCount <= 0)
                EditorGUILayout.HelpBox("현재 빈 앱 상태 기준으로 예약될 규칙이 없어요. 카드 수/회고 수 조건이 있는 규칙은 실제 런타임 상태에서 평가됩니다.", MessageType.None);
        }

        private void DrawRules(SerializedProperty rules)
        {
            EditorGUILayout.LabelField("알림 규칙", EditorStyles.boldLabel);
            for (int i = 0; i < rules.arraySize; i++)
            {
                SerializedProperty rule = rules.GetArrayElementAtIndex(i);
                SerializedProperty displayName = rule.FindPropertyRelative("displayName");
                string title = string.IsNullOrWhiteSpace(displayName.stringValue)
                    ? $"Rule {i + 1}"
                    : displayName.stringValue;

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    rule.isExpanded = EditorGUILayout.Foldout(rule.isExpanded, title, true);
                    if (!rule.isExpanded)
                        continue;

                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("enabled"), new GUIContent("사용"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("id"), new GUIContent("ID"));
                    EditorGUILayout.PropertyField(displayName, new GUIContent("이름"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("triggerType"), new GUIContent("트리거"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("routeType"), new GUIContent("이동 화면"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("title"), new GUIContent("제목"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("body"), new GUIContent("본문"));

                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField("예약 시간", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("delayDays"), new GUIContent("며칠 뒤"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("delayHours"), new GUIContent("몇 시간 뒤"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("delayMinutes"), new GUIContent("몇 분 뒤"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("useFixedClockTime"), new GUIContent("고정 시각 사용"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("fixedHour"), new GUIContent("고정 시"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("fixedMinute"), new GUIContent("고정 분"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("repeat"), new GUIContent("반복"));
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("repeatIntervalDays"), new GUIContent("반복 간격 일"));

                    DrawConditions(rule.FindPropertyRelative("conditions"));

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("회고 대기 조건 추가"))
                            AddCondition(rule, NotificationConditionType.UnreviewedCardCountAtLeast, 1);

                        if (GUILayout.Button("오늘 고민 수 조건 추가"))
                            AddCondition(rule, NotificationConditionType.TodayWorryCountAtLeast, 2);

                        if (GUILayout.Button("좋았던 결과 비율 조건 추가"))
                            AddCondition(rule, NotificationConditionType.DidNotHappenPercentAtLeast, 50);

                        if (GUILayout.Button("최근 감정 조건 추가"))
                            AddCondition(rule, NotificationConditionType.LatestEmotionIs, 0);
                    }

                    if (GUILayout.Button("이 규칙 삭제"))
                    {
                        rules.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
        }

        private static void DrawConditions(SerializedProperty conditions)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("발송 조건", EditorStyles.boldLabel);

            if (conditions.arraySize <= 0)
                EditorGUILayout.HelpBox("조건이 없으면 트리거가 발생할 때 항상 예약됩니다.", MessageType.None);

            for (int i = 0; i < conditions.arraySize; i++)
            {
                SerializedProperty condition = conditions.GetArrayElementAtIndex(i);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(condition.FindPropertyRelative("enabled"), GUIContent.none, GUILayout.Width(20f));
                        EditorGUILayout.PropertyField(condition.FindPropertyRelative("conditionType"), GUIContent.none);

                        if (GUILayout.Button("삭제", GUILayout.Width(48f)))
                        {
                            conditions.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }

                    NotificationConditionType type = (NotificationConditionType)condition.FindPropertyRelative("conditionType").enumValueIndex;
                    if (type == NotificationConditionType.LatestEmotionIs)
                        EditorGUILayout.PropertyField(condition.FindPropertyRelative("emotionValue"), new GUIContent("감정"));
                    else if (type != NotificationConditionType.Always)
                        EditorGUILayout.PropertyField(condition.FindPropertyRelative("intValue"), new GUIContent("값"));
                }
            }
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

        private static void AddCondition(SerializedProperty rule, NotificationConditionType type, int value)
        {
            SerializedProperty conditions = rule.FindPropertyRelative("conditions");
            int index = conditions.arraySize;
            conditions.InsertArrayElementAtIndex(index);

            SerializedProperty condition = conditions.GetArrayElementAtIndex(index);
            condition.FindPropertyRelative("enabled").boolValue = true;
            condition.FindPropertyRelative("conditionType").enumValueIndex = (int)type;
            condition.FindPropertyRelative("intValue").intValue = value;
            condition.FindPropertyRelative("emotionValue").enumValueIndex = (int)WorryEmotionState.StillDistressed;
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
            SerializedProperty conditions = property.FindPropertyRelative("conditions");
            conditions.arraySize = rule.Conditions.Length;
            for (int i = 0; i < rule.Conditions.Length; i++)
            {
                NotificationRuleCondition condition = rule.Conditions[i];
                SerializedProperty target = conditions.GetArrayElementAtIndex(i);
                Set(target, "enabled", condition.Enabled);
                Set(target, "conditionType", (int)condition.ConditionType);
                Set(target, "intValue", condition.IntValue);
                Set(target, "emotionValue", (int)condition.EmotionValue);
            }
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
                0,
                false,
                1,
                new[]
                {
                    NotificationRuleCondition.Create(NotificationConditionType.UnreviewedCardCountAtLeast, 1)
                });
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
