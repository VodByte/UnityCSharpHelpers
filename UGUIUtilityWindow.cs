using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 对 uGUI 的 RectTransform 组件进行处理。可多选。
/// <para>
/// 使用方法：
/// 在 Hierarchy 中选择需要处理的 GameObject, 点击处理对应的按钮即可。
/// </para>
/// </summary>
public class UGUIUtilityWindow : EditorWindow
{
    private static UGUIUtilityWindow m_Window;

    [MenuItem("Tools/uGUIUtility")]
    private static void Open()
    {
        if (!m_Window) m_Window = CreateInstance<UGUIUtilityWindow>();
        m_Window.ShowUtility();
    }

    private void OnGUI()
    {
        using(new EditorGUILayout.VerticalScope())
        {
            if(GUILayout.Button("Anchor Around Object"))
            {
                AnchorAroundObject();
            }

            if (GUILayout.Button("Anchor Align Object"))
            {
                AnchorAlignObject();
            }
        }
    }

    //----------------------------------------------------------------------------------------------
    // DefineMethods
    //----------------------------------------------------------------------------------------------
    /// <summary> 把 Anchors 定位到 Rect 四角 </summary>
    /// <remarks> Pivot 需要是 (0.5, 0.5) </remarks>
    static void AnchorAroundObject()
    {
        var objs = Selection.gameObjects;
        foreach (GameObject o in objs)
        {
            if (o) Execute(o);
        }

        static void Execute(GameObject _o)
        {
            if (_o == null || _o.GetComponent<RectTransform>() == null) return;
            
            var r = _o.GetComponent<RectTransform>();
            Undo.RecordObject(r, "Set anchors around object");
            var p = _o.transform.parent.GetComponent<RectTransform>();

            var offsetMin = r.offsetMin;
            var offsetMax = r.offsetMax;
            var _anchorMin = r.anchorMin;
            var _anchorMax = r.anchorMax;

            var rect = p.rect;
            var parentWidth = rect.width;
            var parentHeight = rect.height;

            var anchorMin = new Vector2(_anchorMin.x + offsetMin.x / parentWidth,
                                        _anchorMin.y + offsetMin.y / parentHeight);
            var anchorMax = new Vector2(_anchorMax.x + offsetMax.x / parentWidth,
                                        _anchorMax.y + offsetMax.y / parentHeight);

            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;

            r.offsetMin = new Vector2(0, 0);
            r.offsetMax = new Vector2(0, 0);
            r.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    /// <summary> 把 Anchors 定位到 Rect 中心 </summary>
    /// <remarks> Pivot 需要是 (0.5, 0.5) </remarks>
    static void AnchorAlignObject()
    {
        var objs = Selection.gameObjects;
        foreach (GameObject o in objs)
        {
            if (o) Execute(o);
        }

        static void Execute(GameObject _o)
        {
            if (_o == null || _o.GetComponent<RectTransform>() == null) return;
            
            var r = _o.GetComponent<RectTransform>();
            Undo.RecordObject(r, "Set anchors around object");
            var p = _o.transform.parent.GetComponent<RectTransform>();

            var offsetMin = r.offsetMin;
            var offsetMax = r.offsetMax;
            var _anchorMin = r.anchorMin;
            var _anchorMax = r.anchorMax;

            var parentRect = p.rect;
            var parentWidth = parentRect.width;
            var parentHeight = parentRect.height;

            var tempRect = r.rect;
            var _halfWidth = tempRect.width / 2.0f;
            var _halfHeight = tempRect.height / 2.0f;

            var anchorMin = new Vector2(_anchorMin.x + (offsetMin.x + _halfWidth) / parentWidth,
                                        _anchorMin.y + (offsetMin.y + _halfHeight) / parentHeight);
            var anchorMax = new Vector2(_anchorMax.x + (offsetMax.x - _halfWidth) / parentWidth,
                                        _anchorMax.y + (offsetMax.y - _halfHeight) / parentHeight);
            r.anchorMin = anchorMin;
            r.anchorMax = anchorMax;

            r.offsetMin = new Vector2(-_halfWidth, -_halfHeight);
            r.offsetMax = new Vector2(_halfWidth, _halfHeight);
            r.pivot = new Vector2(0.5f, 0.5f);
        }
    }
}
