using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace FIMSpace.FEditor
{
    public static class FTypesCollecting
    {
        public static List<Type> GetDerivedTypes(Type baseType)
        {
            List<Type> types = new List<System.Type>();
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try { types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray()); }
                catch (ReflectionTypeLoadException) { }
            }

            return types;
        }

        /// <summary> Replce dots with '/', useful for namespace gathering </summary>
        public static string GetPathNameForGenericMenu(Type type, string initialNamespace)
        {
            string name = type.ToString();
            name = name.Replace(initialNamespace + ".", "");
            return name.Replace('.', '/');
        }

        public static List<Type> GetTypesByNamespace<T>(string rootPathNamespace = "YourNamespace.ToGather", string altNamespace = "") where T : ScriptableObject
        {
            List<Type> types = new List<Type>();

            string nmspc = typeof(T).Namespace;
            if (!string.IsNullOrEmpty(altNamespace)) nmspc = altNamespace;

            foreach (Type t in GetDerivedTypes(typeof(T)))
            {
                if (t == null) continue;
                if (t == typeof(T)) continue; // Ignore base class
                if (t.IsAbstract) continue;

                if (!string.IsNullOrEmpty(rootPathNamespace))
                    if (t.Namespace.StartsWith(nmspc) == false) continue; // Ignore other namespace nodes

                string path = GetPathNameForGenericMenu(t, rootPathNamespace);
                if (string.IsNullOrEmpty(path)) continue;
                types.Add(t);
            }

            return types;
        }

        public static List<T> GatherScriptablesByNamespace<T>(Func<T, string> AssignName, string baseNamespace = "", string nodeNamespace = "") where T : ScriptableObject
        {
            List<T> _assemblyScriptables = new List<T>();
            List<Type> types = GetTypesByNamespace<T>(baseNamespace, nodeNamespace);

            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (type == typeof(T)) continue;

                string path = GetPathNameForGenericMenu(type, baseNamespace);
                if (string.IsNullOrEmpty(path)) continue;

                string name = path;
                T scrptble = ScriptableObject.CreateInstance(type) as T;
                if (scrptble == null) continue;

                if (AssignName == null) name = scrptble.name;
                else name = AssignName.Invoke(scrptble);
                scrptble.name = name;

                _assemblyScriptables.Add(scrptble);
            }

            return _assemblyScriptables;
        }

        public static string FormatNameForSpaces(string name)
        {
            return System.Text.RegularExpressions.Regex.Replace(name, "(\\B[A-Z])", " $1");
        }
    }
}