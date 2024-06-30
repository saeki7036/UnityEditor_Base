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

    //Windowがアクティブ状態に呼び出し
    private void OnEnable()
    {
        var defaultData = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>("Assets/Audio/ScriptableObjects/AudioSourceListClass.asset");
        this.BaseData = defaultData.Clone();
        this.BaseDataPath = AssetDatabase.GetAssetPath(defaultData);
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    //Windowが非アクティブ状態に呼び出し
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    //プレイモードの変更を検出
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
        //テスト用のオブジェクト削除
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
            //他ファイル指定
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
            //上部ツールボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                //Listに要素を追加
                if (GUILayout.Button("Audioを追加", GUILayout.MaxWidth(120f), GUILayout.MaxHeight(20f)))
                {
                    Undo.RecordObject(BaseData, "Add Audio");
                    BaseData.audioValue.Add(new AudioSourceListClass.AudioValue());
                }
                //編集前に戻す
                if (GUILayout.Button("元に戻す", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(20f)))
                {
                    this.BaseData = AssetDatabase.LoadAssetAtPath<AudioSourceListClass>(this.BaseDataPath).Clone();
                    EditorGUIUtility.editingTextField = false;
                }
                //編集を保存する
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
                //Listの中身を表示
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
                        TitleView();

                        ListScrollPosition = scroll_0.scrollPosition;

                        for (int i = 0; i < this.BaseData.audioValue.Count; i++)
                        {

                            if (NullCheck(AudioValueObject, AudioValueProperty) && this.ListSelectedIndex == i)
                                GUI.color = Color.green;

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                MemberView(i, BaseData.audioValue[i]._Audioclip);

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
                        //シーン上のオブジェクトを選択
                        targetGameObject = EditorGUILayout.ObjectField("GameObject", targetGameObject,
                            typeof(GameObject), true, GUILayout.MaxWidth(280f)) as GameObject;

                        if (targetGameObject != null)
                        {
                            //オブジェクトにあるAudioSourceをコンポーネントの数だけ取得
                            targetAudioSources = new List<AudioSource>(targetGameObject.GetComponents<AudioSource>());

                            //Listの中身を表示
                            if (targetAudioSources.Count > 0)
                            {
                                TitleView();
                                using (var scroll_1 = new EditorGUILayout.ScrollViewScope(ValueScrollPosition, GUILayout.MaxWidth(280f)))
                                {
                                    ValueScrollPosition = scroll_1.scrollPosition;

                                    for (int i = 0; i < targetAudioSources.Count; i++)
                                    {
                                        if (NullCheck(targetSerialized, targetProperty) && this.targetSelectedIndex == i)
                                            GUI.color = Color.yellow;
                                       
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            MemberView(i, targetAudioSources[i].clip);

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
                                //テスト再生用ボタン
                                if (GUILayout.Button("再生", GUILayout.MaxWidth(80f), GUILayout.MaxHeight(30f)))
                                {
                                    CreateAudioSystem();

                                    if (prebewAudioSource.clip != null)
                                        prebewAudioSource.Play();
                                    else
                                        Debug.LogWarning("AudioClipをアサインしてください。");
                                }
                                //テスト再生停止用ボタン
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
                                //削除用ボタン
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

                                    //プロパティの要素を描画
                                    PropertyView(AudioValueProperty);

                                    //上限と下限が逆転しないように補正
                                    if (BaseData.audioValue[ListSelectedIndex]._MinDistance >= BaseData.audioValue[ListSelectedIndex]._MaxDistance)
                                        BaseData.audioValue[ListSelectedIndex]._MinDistance = BaseData.audioValue[ListSelectedIndex]._MaxDistance <= 0.01f ? 0 : BaseData.audioValue[ListSelectedIndex]._MaxDistance - 1f;

                                    if (BaseData.audioValue[ListSelectedIndex]._MaxDistance <= BaseData.audioValue[ListSelectedIndex]._MinDistance)
                                        BaseData.audioValue[ListSelectedIndex]._MaxDistance = BaseData.audioValue[ListSelectedIndex]._MinDistance + 1f;

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
                        //List側の要素の選択を確認
                        using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(300f)))
                        {
                            if (NullCheck(AudioValueObject, AudioValueProperty))
                            {
                                EditorGUILayout.LabelField(ListSelectedIndex.ToString(), GUILayout.MaxWidth(30f));
                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
                                if(BaseData.audioValue.Count > ListSelectedIndex)
                                {
                                    ClipNameView(BaseData.audioValue[ListSelectedIndex]._Audioclip, 260f);
                                }     
                                else
                                {
                                    AudioValueObject = null; AudioValueProperty = null;
                                }
                            }
                            else
                                EditorGUILayout.LabelField("リスト側　未選択", GUILayout.MaxWidth(200f));
                        }
                        //オブジェクト側の要素の選択を確認
                        using (new EditorGUILayout.HorizontalScope(GUILayout.MinWidth(300f)))
                        {
                            if (NullCheck(targetSerialized, targetProperty))
                            {
                                EditorGUILayout.LabelField(targetSelectedIndex.ToString(), GUILayout.MaxWidth(30f));
                                EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
                                if (targetAudioSources.Count > targetSelectedIndex)
                                {
                                    ClipNameView(targetAudioSources[targetSelectedIndex].clip, 260f);
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
                            //オブジェクトのAudioSourceの設定をListに上書き
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
                            //ListのAudioSourceの設定をオブジェクトに上書き
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
                            //オブジェクトのAudioSourceの設定をListに追加
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

                        //オブジェクト側のAudioSourceの変数を順に描画
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

    //プロパティの描画
    private void PropertyView(SerializedProperty property)
    {
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

        var _RolloffCustomCurve = AudioValueProperty.FindPropertyRelative("_RolloffCustomCurve");
        EditorGUILayout.PropertyField(_RolloffCustomCurve, new GUIContent("_RolloffCustomCurve"));
    }

    //Listの番号とClip名を描画
    private void MemberView(int index, AudioClip clip)
    {
        EditorGUILayout.LabelField(index.ToString(), GUILayout.MaxWidth(30f));
        EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
        ClipNameView(clip,150f);
        EditorGUILayout.LabelField("|", GUILayout.MaxWidth(10f));
    }

    //AudioClipの名前を描画
    private void ClipNameView(AudioClip clip ,float size) 
    {
        if (clip != null)
            EditorGUILayout.LabelField(clip.ToString(), GUILayout.MaxWidth(size));
        else
            EditorGUILayout.LabelField("null", GUILayout.MaxWidth(size));
    }

    //Listの上部の要素の説明を描画
    private void TitleView()
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

    //シーン上にテスト再生用のオブジェクトを生成
    private void CreateAudioSystem()
    {
        if (prebewGameObject != null)
            DestroyImmediate(prebewGameObject);
        prebewGameObject = new GameObject("AudioTest");
        prebewAudioSource = prebewGameObject.AddComponent<AudioSource>();
        BaseData.audioValue[ListSelectedIndex].GetAudioValue(prebewAudioSource);
    }
}

