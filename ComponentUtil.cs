#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public static class ComponentUtil
{
    static SerializedObject source;

    [MenuItem("CONTEXT/Component/Copy Serialized")]
    public static void CopySerializedFromBase(MenuCommand _command)
    { 
        source = new SerializedObject(_command.context); 
    }

    ///---------------------------------------------------------------
    [MenuItem("CONTEXT/Component/Paste Serialized")]
    public static void PasteSerializedFromBase(MenuCommand _command)
    {
        SerializedObject dest = new SerializedObject(_command.context);
        SerializedProperty prop_iterator = source.GetIterator();
        //jump into serialized object, this will skip script type so that we dont override the destination component's type
        if (prop_iterator.NextVisible(true))
        {
            while (prop_iterator.NextVisible(true)) //itterate through all serializedProperties
            {
                //try obtaining the property in destination component
                SerializedProperty prop_element = dest.FindProperty(prop_iterator.name);

                //validate that the properties are present in both components, and that they're the same type
                if (prop_element != null && prop_element.propertyType == prop_iterator.propertyType)
                {
                    //copy value from source to destination component
                    dest.CopyFromSerializedProperty(prop_iterator);
                }
            }
        }
        dest.ApplyModifiedProperties();
    }

    ///---------------------------------------------------------------
    [MenuItem("CONTEXT/Component/Convrt to TMPro_Text", false)]
    public static void ConverToTMProText(MenuCommand _command)
    {
        GameObject obj = (_command.context as UnityEngine.UI.Text).gameObject;
        UnityEngine.UI.Text textComp = _command.context as UnityEngine.UI.Text;
        // 抽取需要的数据
        var info = new
        {
            Enable    = textComp.enabled,
            FontStyle = textComp.fontStyle switch
            {
                FontStyle.Normal        => FontStyles.Normal,
                FontStyle.Bold          => FontStyles.Bold,
                FontStyle.Italic        => FontStyles.Italic,
                FontStyle.BoldAndItalic => FontStyles.Bold | FontStyles.Italic,
                _                       => FontStyles.Normal
            },
            Text           = textComp.text,
            Color          = textComp.color,
            FontSize       = textComp.fontSize,
            BestFitMaxSize = textComp.resizeTextMaxSize,
            BestFitMinSize = textComp.resizeTextMinSize,
            LineSpacing    = textComp.lineSpacing,
            Alignment      = textComp.alignment switch
            {
                TextAnchor.UpperLeft    => TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter  => TextAlignmentOptions.Top,
                TextAnchor.UpperRight   => TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft   => TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Center,
                TextAnchor.MiddleRight  => TextAlignmentOptions.Right,
                TextAnchor.LowerLeft    => TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter  => TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight   => TextAlignmentOptions.BottomRight,
                _                       => TextAlignmentOptions.TopLeft
            },
            EnableRichText    = textComp.supportRichText,
            EnableAutoSizing  = textComp.resizeTextForBestFit,
            WrappingEnabled   = textComp.horizontalOverflow is HorizontalWrapMode.Wrap,
            TextOverflowModes = textComp.verticalOverflow is VerticalWrapMode.Truncate ? TextOverflowModes.Truncate : TextOverflowModes.Overflow,
            RaycastTarget     = textComp.raycastTarget
        };
        Object.DestroyImmediate(_command.context);

        // 灌数据
        var tmp                    = ObjectFactory.AddComponent<TextMeshProUGUI>(obj);
            tmp.text               = info.Text;
            tmp.color              = info.Color;
            tmp.fontSize           = info.FontSize;
            tmp.fontSizeMax        = info.BestFitMaxSize;
            tmp.fontSizeMin        = info.BestFitMinSize;
            tmp.lineSpacing        = info.LineSpacing;
            tmp.alignment          = info.Alignment;
            tmp.richText           = info.EnableRichText;
            tmp.enableAutoSizing   = info.EnableAutoSizing;
            tmp.enableWordWrapping = info.WrappingEnabled;
            tmp.overflowMode       = info.TextOverflowModes;
            tmp.raycastTarget      = info.RaycastTarget;
    }

    [MenuItem("CONTEXT/Component/Convrt to TMPro_Text", true)]
    public static bool ValidateConverToTMProText(MenuCommand _command) => _command.context is UnityEngine.UI.Text;
}
#endif