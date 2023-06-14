using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEditor;

using UnityEngine;

using Whirllaxy.Utility;

namespace Whirllaxy.Editor
{
    public static class SerializedObjectExtension
    {
        public static IEnumerable<SerializedProperty> GetChildren(this SerializedProperty property)
        {
            property = property.Copy();
            var nextElement = property.Copy();
            bool hasNextElement = nextElement.NextVisible(false);
            if (!hasNextElement)
            {
                nextElement = null;
            }

            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if ((SerializedProperty.EqualContents(property, nextElement))) yield break;
                yield return property;
            }
        }

        ///---------------------------------------------------------------
        public static System.Type GetManagedReferenceType(this SerializedProperty property)
        {
            var sfull = property.managedReferenceFullTypename;
            if (string.IsNullOrEmpty(sfull)) return null;

            var arr = sfull.Split(' ');
            if (arr.Length != 2) return null;

            return System.Type.GetType(string.Format("{0}, {1}", arr[1], arr[0]));
        }

        ///---------------------------------------------------------------
        public static System.Type GetTargetType(this SerializedObject _obj)
        {
            if (_obj == null) return null;

            if (_obj.isEditingMultipleObjects)
            {
                var c = _obj.targetObjects[0];
                return c.GetType();
            }
            else
            {
                return _obj.targetObject.GetType();
            }
        }

        ///---------------------------------------------------------------
        public static System.Type GetTargetType(this SerializedProperty prop)
        {
            if (prop == null) return null;

            FieldInfo field;
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return TypeUtil.FindType(prop.type) ?? typeof(object);
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? typeof(int) : typeof(long);
                case SerializedPropertyType.Boolean:
                    return typeof(bool);
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? typeof(double) : typeof(float);
                case SerializedPropertyType.String:
                    return typeof(string);
                case SerializedPropertyType.Color:
                    {
                        field = GetFieldOfProperty(prop);
                        return field != null ? field.FieldType : typeof(Color);
                    }
                case SerializedPropertyType.ObjectReference:
                    {
                        field = GetFieldOfProperty(prop);
                        return field != null ? field.FieldType : typeof(UnityEngine.Object);
                    }
                case SerializedPropertyType.LayerMask:
                    return typeof(LayerMask);
                case SerializedPropertyType.Enum:
                    {
                        field = GetFieldOfProperty(prop);
                        return field != null ? field.FieldType : typeof(System.Enum);
                    }
                case SerializedPropertyType.Vector2:
                    return typeof(Vector2);
                case SerializedPropertyType.Vector3:
                    return typeof(Vector3);
                case SerializedPropertyType.Vector4:
                    return typeof(Vector4);
                case SerializedPropertyType.Rect:
                    return typeof(Rect);
                case SerializedPropertyType.ArraySize:
                    return typeof(int);
                case SerializedPropertyType.Character:
                    return typeof(char);
                case SerializedPropertyType.AnimationCurve:
                    return typeof(AnimationCurve);
                case SerializedPropertyType.Bounds:
                    return typeof(Bounds);
                case SerializedPropertyType.Gradient:
                    return typeof(Gradient);
                case SerializedPropertyType.Quaternion:
                    return typeof(Quaternion);
                case SerializedPropertyType.ExposedReference:
                    {
                        field = GetFieldOfProperty(prop);
                        return field != null ? field.FieldType : typeof(UnityEngine.Object);
                    }
                case SerializedPropertyType.FixedBufferSize:
                    return typeof(int);
                case SerializedPropertyType.Vector2Int:
                    return typeof(Vector2Int);
                case SerializedPropertyType.Vector3Int:
                    return typeof(Vector3Int);
                case SerializedPropertyType.RectInt:
                    return typeof(RectInt);
                case SerializedPropertyType.BoundsInt:
                    return typeof(BoundsInt);
                default:
                    {
                        field = GetFieldOfProperty(prop);
                        return field != null ? field.FieldType : typeof(object);
                    }
            }
        }

        ///---------------------------------------------------------------
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        ///---------------------------------------------------------------
        public static void SetTargetObjectOfProperty(this SerializedProperty _prop, object value)
        {
            var path = _prop.propertyPath.Replace(".Array.data[", "[");
            object obj = _prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }

            if (object.ReferenceEquals(obj, null)) return;

            try
            {
                var element = elements.Last();

                if (element.Contains("["))
                {
                    var tp = obj.GetType();
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    var field = tp.GetField(elementName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    var arr = field.GetValue(obj) as System.Collections.IList;
                    arr[index] = value;
                }
                else
                {
                    var tp = obj.GetType();
                    var field = tp.GetField(element, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(obj, value);
                    }
                }

            }
            catch
            {
                return;
            }
        }

        ///---------------------------------------------------------------
        /// <summary>
        /// Gets the object that the property is a member of
        /// </summary>
        /// <param name="_prop"></param>
        /// <returns></returns>
        public static object GetTargetObject(this SerializedProperty _prop)
        {
            if (_prop == null) return null;

            var path = _prop.propertyPath.Replace(".Array.data[", "[");
            object obj = _prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements.Take(elements.Length - 1))
            {
                if (element.Contains("["))
                {
                    var elementName = element[..element.IndexOf("[")];
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        ///---------------------------------------------------------------
        public static void SetEnumValue<T>(this SerializedProperty prop, T value) where T : struct
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            //var tp = typeof(T);
            //if(tp.IsEnum)
            //{
            //    prop.enumValueIndex = prop.enumNames.IndexOf(System.Enum.GetName(tp, value));
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConvertUtil.ToInt(value);
        }

        ///---------------------------------------------------------------
        public static void SetEnumValue(this SerializedProperty prop, System.Enum value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //int i = prop.enumNames.IndexOf(System.Enum.GetName(value.GetType(), value));
            //if (i < 0) i = 0;
            //prop.enumValueIndex = i;
            prop.intValue = ConvertUtil.ToInt(value);
        }

        ///---------------------------------------------------------------
        public static void SetEnumValue(this SerializedProperty prop, object value)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (prop.propertyType != SerializedPropertyType.Enum) throw new System.ArgumentException("SerializedProperty is not an enum type.", "prop");

            if (value == null)
            {
                prop.enumValueIndex = 0;
                return;
            }

            //var tp = value.GetType();
            //if (tp.IsEnum)
            //{
            //    int i = prop.enumNames.IndexOf(System.Enum.GetName(tp, value));
            //    if (i < 0) i = 0;
            //    prop.enumValueIndex = i;
            //}
            //else
            //{
            //    int i = ConvertUtil.ToInt(value);
            //    if (i < 0 || i >= prop.enumNames.Length) i = 0;
            //    prop.enumValueIndex = i;
            //}
            prop.intValue = ConvertUtil.ToInt(value);
        }

        ///---------------------------------------------------------------
        public static T GetEnumValue<T>(this SerializedProperty prop) where T : struct, System.IConvertible
        {
            if (prop == null) throw new System.ArgumentNullException("prop");

            try
            {
                //var name = prop.enumNames[prop.enumValueIndex];
                //return ConvertUtil.ToEnum<T>(name);
                return ConvertUtil.ToEnum<T>(prop.intValue);
            }
            catch
            {
                return default(T);
            }
        }

        public static System.Enum GetEnumValue(this SerializedProperty prop, System.Type tp)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!tp.IsEnum) throw new System.ArgumentException("Type must be an enumerated type.");

            try
            {
                //var name = prop.enumNames[prop.enumValueIndex];
                //return System.Enum.Parse(tp, name) as System.Enum;
                return ConvertUtil.ToEnumOfType(tp, prop.intValue);
            }
            catch
            {
                return System.Enum.GetValues(tp).Cast<System.Enum>().First();
            }
        }

        ///---------------------------------------------------------------
        public static void SetPropertyValue(this SerializedProperty _prop, object _value)
        {
            if (_prop == null) throw new System.ArgumentNullException(nameof(_prop));

            switch (_prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    _prop.intValue = ConvertUtil.ToInt(_value);
                    break;
                case SerializedPropertyType.Boolean:
                    _prop.boolValue = ConvertUtil.ToBool(_value);
                    break;
                case SerializedPropertyType.Float:
                    _prop.floatValue = ConvertUtil.ToSingle(_value);
                    break;
                case SerializedPropertyType.String:
                    _prop.stringValue = ConvertUtil.ToString(_value);
                    break;
                case SerializedPropertyType.Color:
                    _prop.colorValue = ConvertUtil.ToColor(_value);
                    break;
                case SerializedPropertyType.ObjectReference:
                    _prop.objectReferenceValue = _value as UnityEngine.Object;
                    break;
                case SerializedPropertyType.LayerMask:
                    _prop.intValue = (_value is LayerMask mask) ? mask.value : ConvertUtil.ToInt(_value);
                    break;
                case SerializedPropertyType.Enum:
                    _prop.SetEnumValue(_value);
                    break;
                case SerializedPropertyType.Vector2:
                    _prop.vector2Value = ConvertUtil.ToVector2(_value);
                    break;
                case SerializedPropertyType.Vector3:
                    _prop.vector3Value = ConvertUtil.ToVector3(_value);
                    break;
                case SerializedPropertyType.Vector4:
                    _prop.vector4Value = ConvertUtil.ToVector4(_value);
                    break;
                case SerializedPropertyType.Rect:
                    _prop.rectValue = (Rect)_value;
                    break;
                case SerializedPropertyType.ArraySize:
                    _prop.arraySize = ConvertUtil.ToInt(_value);
                    break;
                case SerializedPropertyType.Character:
                    _prop.intValue = ConvertUtil.ToInt(_value);
                    break;
                case SerializedPropertyType.AnimationCurve:
                    _prop.animationCurveValue = _value as AnimationCurve;
                    break;
                case SerializedPropertyType.Bounds:
                    _prop.boundsValue = (Bounds)_value;
                    break;
                case SerializedPropertyType.Gradient:
                    throw new System.InvalidOperationException("Can not handle Gradient types.");
            }
        }

        ///---------------------------------------------------------------
        public static object GetPropertyValue(this SerializedProperty _prop)
        {
            if (_prop == null) throw new ArgumentNullException(nameof(_prop));

            return _prop.propertyType switch
            {
                SerializedPropertyType.Integer         => _prop.intValue,
                SerializedPropertyType.Boolean         => _prop.boolValue,
                SerializedPropertyType.Float           => _prop.floatValue,
                SerializedPropertyType.String          => _prop.stringValue,
                SerializedPropertyType.Color           => _prop.colorValue,
                SerializedPropertyType.ObjectReference => _prop.objectReferenceValue,
                SerializedPropertyType.LayerMask       => (LayerMask)_prop.intValue,
                SerializedPropertyType.Enum            => _prop.enumValueIndex,
                SerializedPropertyType.Vector2         => _prop.vector2Value,
                SerializedPropertyType.Vector3         => _prop.vector3Value,
                SerializedPropertyType.Vector4         => _prop.vector4Value,
                SerializedPropertyType.Rect            => _prop.rectValue,
                SerializedPropertyType.ArraySize       => _prop.arraySize,
                SerializedPropertyType.Character       => (char)_prop.intValue,
                SerializedPropertyType.AnimationCurve  => _prop.animationCurveValue,
                SerializedPropertyType.Bounds          => _prop.boundsValue,
                SerializedPropertyType.Gradient        => throw new InvalidOperationException("Can not handle Gradient types."),
                _ => null,
            };
        }

        ///---------------------------------------------------------------
        public static T GetPropertyValue<T>(this SerializedProperty _prop)
        {
            var obj = GetPropertyValue(_prop);
            if (obj is T tObj) return tObj;

            var tp = typeof(T);
            try
            {
                return (T)Convert.ChangeType(obj, tp);
            }
            catch(Exception)
            {
                return default;
            }
        }

        ///---------------------------------------------------------------
        public static SerializedPropertyType GetPropertyType(System.Type tp)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            
            if(tp.IsEnum) return SerializedPropertyType.Enum;

            var code = System.Type.GetTypeCode(tp);
            switch(code)
            {
                case System.TypeCode.SByte:
                case System.TypeCode.Byte:
                case System.TypeCode.Int16:
                case System.TypeCode.UInt16:
                case System.TypeCode.Int32:
                case System.TypeCode.UInt32:
                case System.TypeCode.Int64:
                case System.TypeCode.UInt64:
                    return SerializedPropertyType.Integer;
                case System.TypeCode.Boolean:
                    return SerializedPropertyType.Boolean;
                case System.TypeCode.Single:
                case System.TypeCode.Double:
                    return SerializedPropertyType.Float;
                case System.TypeCode.String:
                    return SerializedPropertyType.String;
                case System.TypeCode.Char:
                    return SerializedPropertyType.Character;
                default:
                    {
                        if (TypeUtil.IsType(tp, typeof(Color)))
                            return SerializedPropertyType.Color;
                        else if (TypeUtil.IsType(tp, typeof(UnityEngine.Object)))
                            return SerializedPropertyType.ObjectReference;
                        else if (TypeUtil.IsType(tp, typeof(LayerMask)))
                            return SerializedPropertyType.LayerMask;
                        else if (TypeUtil.IsType(tp, typeof(Vector2)))
                            return SerializedPropertyType.Vector2;
                        else if (TypeUtil.IsType(tp, typeof(Vector3)))
                            return SerializedPropertyType.Vector3;
                        else if (TypeUtil.IsType(tp, typeof(Vector4)))
                            return SerializedPropertyType.Vector4;
                        else if (TypeUtil.IsType(tp, typeof(Quaternion)))
                            return SerializedPropertyType.Quaternion;
                        else if (TypeUtil.IsType(tp, typeof(Rect)))
                            return SerializedPropertyType.Rect;
                        else if (TypeUtil.IsType(tp, typeof(AnimationCurve)))
                            return SerializedPropertyType.AnimationCurve;
                        else if (TypeUtil.IsType(tp, typeof(Bounds)))
                            return SerializedPropertyType.Bounds;
                        else if (TypeUtil.IsType(tp, typeof(Gradient)))
                            return SerializedPropertyType.Gradient;
                    }
                    return SerializedPropertyType.Generic;

            }
        }

        ///---------------------------------------------------------------
        public static System.TypeCode GetPropertyTypeCode(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return prop.type == "long" ? System.TypeCode.Int64 : System.TypeCode.Int32;
                case SerializedPropertyType.Boolean:
                    return System.TypeCode.Boolean;
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? System.TypeCode.Double : System.TypeCode.Single;
                case SerializedPropertyType.String:
                    return System.TypeCode.String;
                case SerializedPropertyType.LayerMask:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.Enum:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.ArraySize:
                    return System.TypeCode.Int32;
                case SerializedPropertyType.Character:
                    return System.TypeCode.Char;
                default:
                    return System.TypeCode.Object;
            }
        }

        ///---------------------------------------------------------------
        public static double GetNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    return (double)prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue ? 1d : 0d;
                case SerializedPropertyType.Float:
                    return prop.type == "double" ? prop.doubleValue : (double)prop.floatValue;
                case SerializedPropertyType.ArraySize:
                    return (double)prop.arraySize;
                case SerializedPropertyType.Character:
                    return (double)prop.intValue;
                default:
                    return 0d;
            }
        }

        ///---------------------------------------------------------------
        public static void SetNumericValue(this SerializedProperty prop, double value)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                    prop.intValue = (int)value;
                    break;
                case SerializedPropertyType.Boolean:
                    prop.boolValue = (System.Math.Abs(value) > MathUtil.DBL_EPSILON);
                    break;
                case SerializedPropertyType.Float:
                    if (prop.type == "double")
                        prop.doubleValue = value;
                    else
                        prop.floatValue = (float)value;
                    break;
                case SerializedPropertyType.ArraySize:
                    prop.arraySize = (int)value;
                    break;
                case SerializedPropertyType.Character:
                    prop.intValue = (int)value;
                    break;
            }
        }

        ///---------------------------------------------------------------
        public static bool IsNumericValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Integer:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.Float:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                    return true;
                default:
                    return false;
            }
        }

        ///---------------------------------------------------------------
        public static bool IsVectorValue(this SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Vector2:
                case SerializedPropertyType.Vector3:
                case SerializedPropertyType.Vector4:
                case SerializedPropertyType.Vector2Int:
                case SerializedPropertyType.Vector3Int:
                    return true;
                default:
                    return false;
            }
        }

        ///---------------------------------------------------------------
        public static IEnumerable<SerializedProperty> EnumerateArray(this SerializedProperty prop)
        {
            if (!prop.isArray) yield break;

            for (int i = 0; i < prop.arraySize; i++)
            {
                yield return prop.GetArrayElementAtIndex(i);
            }
        }

        ///---------------------------------------------------------------
        public static T[] GetAsArray<T>(this SerializedProperty _prop)
        {
            if (_prop == null) throw new ArgumentNullException(nameof(_prop));
            if (!_prop.isArray) throw new ArgumentException("SerializedProperty does not represent an Array.", nameof(_prop));

            var arr = new T[_prop.arraySize];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = GetPropertyValue<T>(_prop.GetArrayElementAtIndex(i));
            }
            return arr;
        }

        ///---------------------------------------------------------------
        public static void SetAsArray<T>(this SerializedProperty prop, T[] arr)
        {
            if (prop == null) throw new System.ArgumentNullException("prop");
            if (!prop.isArray) throw new System.ArgumentException("SerializedProperty does not represent an Array.", "prop");

            int sz = arr != null ? arr.Length : 0;
            prop.arraySize = sz;
            for (int i = 0; i < sz; i++)
            {
                prop.GetArrayElementAtIndex(i).SetPropertyValue(arr[i]);
            }
        }

        ///---------------------------------------------------------------
        public static int GetChildPropertyCount(SerializedProperty property, bool includeGrandChildren = false)
        {
            var pstart = property.Copy();
            var pend = property.GetEndProperty();
            int cnt = 0;

            pstart.Next(true);
            while (!SerializedProperty.EqualContents(pstart, pend))
            {
                cnt++;
                pstart.Next(includeGrandChildren);
            }

            return cnt;
        }

        ///---------------------------------------------------------------
        public static FieldInfo GetFieldOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var tp = GetTargetType(prop.serializedObject);
            if (tp == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            var elements = path.Split('.');
            System.Reflection.FieldInfo field = null;
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));

                    field = tp.GetMember(elementName, MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault() as FieldInfo;
                    if (field == null) return null;
                    tp = field.FieldType;
                }
                else
                {
                    field = tp.GetMember(element, MemberTypes.Field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault() as FieldInfo;
                    if (field == null) return null;
                    tp = field.FieldType;
                }
            }
            return field;
        }

        ///=========================================
        /// ██╗███╗   ██╗███╗   ██╗███████╗██████╗ 
        /// ██║████╗  ██║████╗  ██║██╔════╝██╔══██╗
        /// ██║██╔██╗ ██║██╔██╗ ██║█████╗  ██████╔╝
        /// ██║██║╚██╗██║██║╚██╗██║██╔══╝  ██╔══██╗
        /// ██║██║ ╚████║██║ ╚████║███████╗██║  ██║
        /// ╚═╝╚═╝  ╚═══╝╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝
        ///=========================================
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        ///---------------------------------------------------------------
        private static object GetValue_Imp(object source, string name, int index)
        {
            if (GetValue_Imp(source, name) is not System.Collections.IEnumerable enumerable) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }
    }
}