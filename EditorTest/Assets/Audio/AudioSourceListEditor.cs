using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioSourceListEditor : EditorWindow
{
    [MenuItem("Editor/OriginalEditerPanels/AudioSourceListEditor", priority = 0)]
    public static void ShowWindow()
    {
        GetWindow<AudioSourceListEditor>("AudioSourceListEditor");
    }

    //フィールド上のオブジェクト用の変数
    public GameObject targetGameObject;
    private List<AudioSource> targetAudioSources = new List<AudioSource>();
    private SerializedObject targetSerialized;
    private SerializedProperty targetProperty;
    private int targetSelectedIndex = -1;

    //リストの中身編集用の変数
    private SerializedObject AudioValueObject;
    private SerializedProperty AudioValueProperty;
    private int ListSelectedIndex = -1;

    //クラス変数
    [SerializeField]
    private AudioSourceListClass BaseData;
    [SerializeField]
    private string BaseDataPath;

    //EditorWindowの管理用の変数
    private Vector2 ListScrollPosition;
    private Vector2 ValueScrollPosition;
    private Vector2 SourceScrollPosition;
    private bool Mode = false;

    //音声再生用の変数
    GameObject prebewGameObject;
    AudioSource prebewAudioSource;

    private void OnEnable()
    {
        var defaultData = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>("Assets/Audio/ScriptableObjects/AudioSourceListClass.asset");
        this.BaseData = defaultData.Clone();
        this.BaseDataPath = AssetDatabase.GetAssetPath(defaultData);
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {       
        if (state == PlayModeStateChange.EnteredPlayMode)
            OnEnterPlayMode();
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            OnExitPlayMode();
        }
    }

    // プレイモード開始時の処理を行う関数
    private void OnEnterPlayMode()
    {
        if (prebewGameObject != null)
            DestroyImmediate(prebewGameObject);
    }

    private void OnExitPlayMode()
    {
 
    }

    private void OnGUI()
    {
        //if (BaseData == null) BaseData = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>(BaseDataPath).Clone();
        using (new EditorGUILayout.VerticalScope())
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var beforeData = this.BaseData;
                BaseData = EditorGUILayout.ObjectField("Asset", BaseData, typeof(AudioSourceListClass), false) as AudioSourceListClass;
                if(beforeData != BaseData)
                {
                    AudioValueObject = null;
                    AudioValueProperty = null;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Audioを追加", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Audio");
                    BaseData.audioValue.Add(new AudioSourceListClass.AudioValue());
                }

                if (GUILayout.Button("元に戻す", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    this.BaseData = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>(this.BaseDataPath).Clone();
                    EditorGUIUtility.editingTextField = false;
                }

                if (GUILayout.Button("保存", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    var data = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>(this.BaseDataPath);
                    EditorUtility.CopySerialized(this.BaseData, data);
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssets();
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("モード切り替え", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(20f)))
                {
                    this.Mode = !Mode;
                }
            }
            using (new EditorGUILayout.HorizontalScope(GUILayout.MaxHeight(800f)))
            {
                using (var scroll_0 = new EditorGUILayout.ScrollViewScope(ListScrollPosition, GUILayout.MinWidth(290f)))
                {
                    if (BaseData == null)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Assetがありません。", GUILayout.MaxWidth(280f));
                        }
                    }
                    else
                    {
                        ShowIndex();

                        ListScrollPosition = scroll_0.scrollPosition;

                        for (int i = 0; i < this.BaseData.audioValue.Count; i++)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (NullCheck(AudioValueObject, AudioValueProperty) && this.ListSelectedIndex == i)
                                    GUI.color = Color.green;

                                EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(30f));
                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                if (BaseData.audioValue[i]._Audioclip != null)
                                    EditorGUILayout.LabelField(BaseData.audioValue[i]._Audioclip.ToString(), GUILayout.MaxWidth(150f));
                                else
                                    EditorGUILayout.LabelField("null", GUILayout.MaxWidth(150f));

                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                if (GUILayout.Button("編集", GUILayout.MaxWidth(60f)))
                                {
                                    Undo.RecordObject(this, "Select Audio");
                                    this.ListSelectedIndex = i;
                                    AudioValueObject = new SerializedObject(BaseData);
                                    AudioValueProperty = AudioValueObject.FindProperty("audioValue").GetArrayElementAtIndex(ListSelectedIndex);
                                }


                                GUI.color = Color.white;
                            }
                        }
                    }
                }
                using (new EditorGUILayout.VerticalScope())
                {
                    if (Mode)
                    {
                        targetGameObject = EditorGUILayout.ObjectField("GameObject", targetGameObject,
                            typeof(GameObject), true, GUILayout.MaxWidth(280f)) as GameObject;

                        if (targetGameObject != null)
                        {
                            targetAudioSources = new List<AudioSource>(targetGameObject.GetComponents<AudioSource>());

                            if (targetAudioSources.Count > 0)
                            {
                                ShowIndex();

                                using (var scroll_1 = new EditorGUILayout.ScrollViewScope(ValueScrollPosition, GUILayout.MaxWidth(280f)))
                                {
                                    ValueScrollPosition = scroll_1.scrollPosition;

                                    for (int i = 0; i < targetAudioSources.Count; i++)
                                    {
                                        if (NullCheck(targetSerialized, targetProperty) && this.targetSelectedIndex == i)
                                            GUI.color = Color.yellow;

                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(30f));
                                            EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                            if (targetAudioSources[i].clip != null)
                                                EditorGUILayout.LabelField(targetAudioSources[i].clip.ToString(), GUILayout.MaxWidth(150f));
                                            else
                                                EditorGUILayout.LabelField("null", GUILayout.MaxWidth(150f));

                                            EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                            if (GUILayout.Button("編集", GUILayout.MaxWidth(60f)))
                                            {
                                                Undo.RecordObject(this, "Select Audio");
                                                this.targetSelectedIndex = i;
                                                targetSerialized = new SerializedObject(targetAudioSources[i]);
                                                targetProperty = targetSerialized.GetIterator();
                                            }
                                        }
                                        GUI.color = Color.white;
                                    }
                                }
                            }
                            else
                                EditorGUILayout.LabelField("AudioSourceがありません。", GUILayout.MaxWidth(280f));
                        }
                        else
                            EditorGUILayout.LabelField("GameObjectがありません。", GUILayout.MaxWidth(280f));                 
                    }
                    else
                    {
                        if (NullCheck(AudioValueObject, AudioValueProperty))
                        {
                            using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(280f)))
                            {
                                if (GUILayout.Button("再生", GUILayout.MaxWidth(80f), GUILayout.MaxHeight(30f)))
                                {
                                    CreateAudioSystem();

                                    if (prebewAudioSource.clip != null)
                                        prebewAudioSource.Play();
                                    else
                                        Debug.LogWarning("AudioClipをアサインしてください。");
                                }

                                if (GUILayout.Button("停止", GUILayout.MaxWidth(80f), GUILayout.MaxHeight(30f)))
                                {
                                    if (prebewGameObject != null && prebewAudioSource != null)
                                    {
                                        if (prebewAudioSource.clip != null)
                                            prebewAudioSource.Stop();
                                        else
                                            Debug.LogWarning("AudioClipをアサインしてください。");
                                    }
                                }

                                GUI.color = Color.red;
                                if (GUILayout.Button("指定したAudioを削除", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(30f)))
                                {
                                    Undo.RecordObject(BaseData, "Remove Audio");
                                    BaseData.audioValue.RemoveAt(ListSelectedIndex);
                                    AudioValueObject = null; AudioValueProperty = null;
                                    this.ListSelectedIndex = -1;
                                }
                                GUI.color = Color.white;
                            }

                            if (NullCheck(AudioValueObject, AudioValueProperty))
                                using (var scroll_1 = new EditorGUILayout.ScrollViewScope(ValueScrollPosition, GUILayout.MinWidth(280f)))
                                {

                                    ValueScrollPosition = scroll_1.scrollPosition;

                                    AudioValueObject.Update();

                                    // 要素のプロパティを描画
                                    //EditorGUILayout.PropertyField(audioValueProperty, new GUIContent("audioValue"));

                                    var _Audioclip = AudioValueProperty.FindPropertyRelative("_Audioclip");
                                    EditorGUILayout.PropertyField(_Audioclip, new GUIContent("_Audioclip"));

                                    var _AudioMixerGroup = AudioValueProperty.FindPropertyRelative("_AudioMixerGroup");
                                    EditorGUILayout.PropertyField(_AudioMixerGroup, new GUIContent("_AudioMixerGroup"));

                                    var _Mute = AudioValueProperty.FindPropertyRelative("_Mute");
                                    EditorGUILayout.PropertyField(_Mute, new GUIContent("_Mute"));

                                    var _BypassEffects = AudioValueProperty.FindPropertyRelative("_BypassEffects");
                                    EditorGUILayout.PropertyField(_BypassEffects, new GUIContent("_BypassEffects"));

                                    var _BypassListenerEffects = AudioValueProperty.FindPropertyRelative("_BypassListenerEffects");
                                    EditorGUILayout.PropertyField(_BypassListenerEffects, new GUIContent("_BypassListenerEffects"));

                                    var _BypassRevevbZones = AudioValueProperty.FindPropertyRelative("_BypassRevevbZones");
                                    EditorGUILayout.PropertyField(_BypassRevevbZones, new GUIContent("_BypassRevevbZones"));

                                    var _PlayOnAwake = AudioValueProperty.FindPropertyRelative("_PlayOnAwake");
                                    EditorGUILayout.PropertyField(_PlayOnAwake, new GUIContent("_PlayOnAwake"));

                                    var _Loop = AudioValueProperty.FindPropertyRelative("_Loop");
                                    EditorGUILayout.PropertyField(_Loop, new GUIContent("_Loop"));

                                    var _Priority = AudioValueProperty.FindPropertyRelative("_Priority");
                                    EditorGUILayout.PropertyField(_Priority, new GUIContent("_Priority"));

                                    var _Volume = AudioValueProperty.FindPropertyRelative("_Volume");
                                    EditorGUILayout.PropertyField(_Volume, new GUIContent("_Volume"));

                                    var _Pitch = AudioValueProperty.FindPropertyRelative("_Pitch");
                                    EditorGUILayout.PropertyField(_Pitch, new GUIContent("_Pitch"));

                                    var _StereoPan = AudioValueProperty.FindPropertyRelative("_StereoPan");
                                    EditorGUILayout.PropertyField(_StereoPan, new GUIContent("_StereoPan"));

                                    var _SpatialBlend = AudioValueProperty.FindPropertyRelative("_SpatialBlend");
                                    EditorGUILayout.PropertyField(_SpatialBlend, new GUIContent("_SpatialBlend"));

                                    var _ReverbZoneMix = AudioValueProperty.FindPropertyRelative("_ReverbZoneMix");
                                    EditorGUILayout.PropertyField(_ReverbZoneMix, new GUIContent("_ReverbZoneMix"));

                                    var _DopplerLevel = AudioValueProperty.FindPropertyRelative("_DopplerLevel");
                                    EditorGUILayout.PropertyField(_DopplerLevel, new GUIContent("_DopplerLevel"));

                                    var _Spread = AudioValueProperty.FindPropertyRelative("_Spread");
                                    EditorGUILayout.PropertyField(_Spread, new GUIContent("_Spread"));

                                    var _VolumeRolloff = AudioValueProperty.FindPropertyRelative("_VolumeRolloff");
                                    EditorGUILayout.PropertyField(_VolumeRolloff, new GUIContent("_VolumeRolloff"));

                                    var _MinDistance = AudioValueProperty.FindPropertyRelative("_MinDistance");
                                    EditorGUILayout.PropertyField(_MinDistance, new GUIContent("_MinDistance"));

                                    var _MaxDistance = AudioValueProperty.FindPropertyRelative("_MaxDistance");
                                    EditorGUILayout.PropertyField(_MaxDistance, new GUIContent("_MaxDistance"));

                                    if (BaseData.audioValue[ListSelectedIndex]._MinDistance >= BaseData.audioValue[ListSelectedIndex]._MaxDistance)
                                        BaseData.audioValue[ListSelectedIndex]._MinDistance = BaseData.audioValue[ListSelectedIndex]._MaxDistance <= 0.01f ? 0 : BaseData.audioValue[ListSelectedIndex]._MaxDistance - 1f;

                                    if (BaseData.audioValue[ListSelectedIndex]._MaxDistance <= BaseData.audioValue[ListSelectedIndex]._MinDistance)
                                        BaseData.audioValue[ListSelectedIndex]._MaxDistance = BaseData.audioValue[ListSelectedIndex]._MinDistance + 1f;

                                    var _RolloffCustomCurve = AudioValueProperty.FindPropertyRelative("_RolloffCustomCurve");
                                    EditorGUILayout.PropertyField(_RolloffCustomCurve, new GUIContent("_RolloffCustomCurve"));


                                    // 変更を適用
                                    AudioValueObject.ApplyModifiedProperties();

                                    //BaseData._A[i].mute = EditorGUILayout.Toggle(BaseData._A[i].mute);
                                    //BaseData._A[i].outputAudioMixerGroup EditorGUI.ObjectField(BaseData._A[i].outputAudioMixerGroup, typeof(AudioMixerGroup));
                                    //BaseData._A[i].outputAudioMixerGroup = EditorGUILayout.ObjectField(BaseData._A[i].outputAudioMixerGroup, typeof(AudioMixerGroup));

                                }
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope(GUILayout.MinWidth(300f)))
                {
                    if (Mode)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(300f)))
                        {
                            if (NullCheck(AudioValueObject, AudioValueProperty))
                            {
                                EditorGUILayout.LabelField(ListSelectedIndex.ToString(), GUILayout.MaxWidth(30f));

                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                if(BaseData.audioValue.Count > ListSelectedIndex)
                                {
                                    if (BaseData.audioValue[ListSelectedIndex]._Audioclip != null)
                                        EditorGUILayout.LabelField(BaseData.audioValue[ListSelectedIndex]._Audioclip.ToString(), GUILayout.MaxWidth(260f));
                                    else
                                        EditorGUILayout.LabelField("null", GUILayout.MaxWidth(150f));
                                }     
                                else
                                {
                                    AudioValueObject = null; AudioValueProperty = null;
                                }
                            }
                            else
                                EditorGUILayout.LabelField("リスト側　未選択", GUILayout.MaxWidth(200f));
                        }

                        using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(300f)))
                        {
                            if (NullCheck(targetSerialized, targetProperty))
                            {
                                EditorGUILayout.LabelField(targetSelectedIndex.ToString(), GUILayout.MaxWidth(30f));

                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));

                                if (targetAudioSources.Count > targetSelectedIndex)
                                {
                                    if (targetAudioSources[targetSelectedIndex].clip != null)
                                        EditorGUILayout.LabelField(targetAudioSources[targetSelectedIndex].clip.ToString(), GUILayout.MaxWidth(260f));
                                    else
                                        EditorGUILayout.LabelField("null", GUILayout.MaxWidth(150f));
                                }
                                else
                                {
                                    targetSerialized = null; targetProperty = null;
                                }
                            }
                            else
                                EditorGUILayout.LabelField("オブジェクト側　未選択", GUILayout.MaxWidth(200f));
                        }

                        using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(300f)))
                        {
                            if (GUILayout.Button("Object=> List", GUILayout.MaxWidth(100f), GUILayout.MaxHeight(20f)))
                            {
                                if (NullCheck(targetSerialized, targetProperty) && NullCheck(AudioValueObject, AudioValueProperty))
                                {
                                    Undo.RecordObject(BaseData, "Copy Audio in List");
                                    BaseData.audioValue[ListSelectedIndex].SetAudioValue(targetAudioSources[targetSelectedIndex]);
                                }
                                else
                                    Debug.LogWarning("要素が選択されていません。");
                            }

                            if (GUILayout.Button("List=> Object", GUILayout.MaxWidth(100f), GUILayout.MaxHeight(20f)))
                            {
                                    if (NullCheck(targetSerialized, targetProperty) && NullCheck(AudioValueObject, AudioValueProperty))
                                    {
                                        Undo.RecordObject(BaseData, "Copy Audio in Object");
                                        BaseData.audioValue[ListSelectedIndex].GetAudioValue(targetAudioSources[targetSelectedIndex]);
                                    }
                                else
                                    Debug.LogWarning("要素が選択されていません。");
                            }

                            if (GUILayout.Button("Add List", GUILayout.MaxWidth(100f), GUILayout.MaxHeight(20f)))
                            {
                                if (NullCheck(targetSerialized, targetProperty))
                                {
                                    Undo.RecordObject(BaseData, "Add Audio");
                                    BaseData.audioValue.Add(new AudioSourceListClass.AudioValue(targetAudioSources[targetSelectedIndex]));
                                }
                                else
                                    Debug.LogWarning("要素が選択されていません。");  
                            }
                        }

                        if (targetSerialized != null && targetProperty != null)
                        {
                            //targetProperty.Next(true);
                            targetSerialized.Update();
                            targetProperty.Reset();

                            using (var scroll_2 = new EditorGUILayout.ScrollViewScope(SourceScrollPosition, GUILayout.MaxWidth(280f)))
                            {
                                SourceScrollPosition = scroll_2.scrollPosition;

                                if (targetProperty.NextVisible(true)) // 最初のプロパティに移動
                                    do
                                    {
                                        EditorGUILayout.PropertyField(targetProperty, true);
                                    }
                                    while (targetProperty.NextVisible(false));
                            }
                            //EditorGUILayout.PropertyField(targetProperty, new GUIContent("AudioSource"));
                            targetSerialized.ApplyModifiedProperties();
                        }
                    }
                }
            }
        }
    }

    private void ShowIndex()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("ID", GUILayout.MaxWidth(30f));
            EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
            EditorGUILayout.LabelField("Audio Name", GUILayout.MaxWidth(150f));
            EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
            EditorGUILayout.LabelField("編集ボタン", GUILayout.MaxWidth(60f));
        }
    }

    private bool NullCheck(SerializedObject serializedObject, SerializedProperty serializedProperty)
    {
        if (serializedObject == null || serializedProperty == null)
            return false;
        else
            return true;
    }
    private void CreateAudioSystem()
    {
        if (prebewGameObject != null)
            DestroyImmediate(prebewGameObject);
        prebewGameObject = new GameObject("AudioTest");
        prebewAudioSource = prebewGameObject.AddComponent<AudioSource>();
        BaseData.audioValue[ListSelectedIndex].GetAudioValue(prebewAudioSource);
    }
}

