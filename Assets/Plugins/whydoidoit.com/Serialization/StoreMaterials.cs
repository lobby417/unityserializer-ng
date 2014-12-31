// /* ------------------
//
//       (c) whydoidoit.com 2012
//           by Mike Talbot 
//     ------------------- */
// 
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Serialization;

[AddComponentMenu("Storage/Store Materials")]
[ExecuteInEditMode]
[DontStore]
public class StoreMaterials : MonoBehaviour {
    public List<MaterialProperty> MaterialProperties = new List<MaterialProperty>();
    static Index<string, List<MaterialProperty>> cache = new Index<string, List<MaterialProperty>>();


    [Serializable]
    public class MaterialProperty {
        public string name;
        public string description;
        public ShaderUtil.ShaderPropertyType type;
    }

    public class StoredValue {
        public MaterialProperty property;
        public object value;
    }


    static StoreMaterials() {
        DelegateSupport.RegisterFunctionType<Texture2D, int>();
        DelegateSupport.RegisterFunctionType<StoreMaterials, List<MaterialProperty>>();
        DelegateSupport.RegisterFunctionType<MaterialProperty, ShaderUtil.ShaderPropertyType>();
        DelegateSupport.RegisterFunctionType<MaterialProperty, string>();
        DelegateSupport.RegisterFunctionType<StoredValue, MaterialProperty>();
        DelegateSupport.RegisterFunctionType<StoredValue, object>();
    }

    private void Awake() {
        OnEnable();
    }

    private void OnEnable() {
        cache.Clear();
        MaterialProperties = GetComponent<Renderer>().sharedMaterials.Where(m => m).SelectMany(m => GetShaderProperties(m)).Discrete(m => m.name).ToList();
    }

    private void OnDisable() {
        cache.Clear();
    }

    public List<StoredValue> GetValues(Material m) {
        var list = GetShaderProperties(m);
        var output = new List<StoredValue>();
        foreach (var p in list) {
            var o = new StoredValue {
                property = p
            };
            output.Add(o);
            switch (p.type) {
                case ShaderUtil.ShaderPropertyType.Color:
                    o.value = m.GetColor(p.name);
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                    o.value = m.GetFloat(p.name);
                    break;
                case ShaderUtil.ShaderPropertyType.Range:
                    o.value = m.GetFloat(p.name);
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    o.value = m.GetTexture(p.name);
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    o.value = m.GetVector(p.name);
                    break;
                default:
                    Debug.LogError("Unsupported type: " + p.type.ToString());
                    break;
            }
        }
        return output;
    }

    public void SetValues(Material m, IEnumerable<StoredValue> values) {
        foreach (var v in values) {
            switch (v.property.type) {
                case ShaderUtil.ShaderPropertyType.Color:
                    m.SetColor(v.property.name, (Color)v.value);
                    break;
                case ShaderUtil.ShaderPropertyType.Float:
                    m.SetFloat(v.property.name, (float)v.value);
                    break;
                case ShaderUtil.ShaderPropertyType.Range:
                    m.SetFloat(v.property.name, (float)v.value);
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    m.SetTexture(v.property.name, (Texture)v.value);
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    m.SetVector(v.property.name, (Vector4)v.value);
                    break;
                default:
                    Debug.LogError("Unsupported type: " + v.property.type.ToString());
                    break;
            }
        }
    }

    public List<MaterialProperty> GetShaderProperties(Material material) {
        if (cache.ContainsKey(material.shader.name))
            return cache[material.shader.name];

        var list = new List<MaterialProperty>();
        Shader s = material.shader;
        int count = ShaderUtil.GetPropertyCount(s);

        for (int i = 0; i < count; i++) {
            list.Add(new MaterialProperty() {
                type = ShaderUtil.GetPropertyType(s, i),
                description = ShaderUtil.GetPropertyDescription(s, i),
                name = ShaderUtil.GetPropertyName(s, i)
            });
        }

        cache[material.shader.name] = list;
        return list;
    }
}


[CustomEditor(typeof(StoreMaterials))]
public class StoreMaterialsEditor : Editor {
    bool show;

    public override void OnInspectorGUI() {
        show = EditorGUILayout.Foldout(show, "Material properties");
        if (show) {
            StoreMaterials script = (StoreMaterials)target;

            foreach (StoreMaterials.MaterialProperty property in script.MaterialProperties) {
                string type = "";

                switch (property.type) {
                    case ShaderUtil.ShaderPropertyType.Color:
                        type = "COLOR";
                        break;
                    case ShaderUtil.ShaderPropertyType.Float:
                        type = "FLOAT";
                        break;
                    case ShaderUtil.ShaderPropertyType.Range:
                        type = "FLOAT";
                        break;
                    case ShaderUtil.ShaderPropertyType.TexEnv:
                        type = "TEXTURE";
                        break;
                    case ShaderUtil.ShaderPropertyType.Vector:
                        type = "VECTOR";
                        break;
                }

                GUILayout.Label("[" + type + "]\t" + property.description);
            }
        }
    }
}