using System;
using System.IO;
using UnityEditor;

namespace Unity.Android.DependencyResolver
{
    class ResolverSettings
    {
        static readonly string Path = "UserSettings/ResolverSettings.json";

        [Serializable]
        class Data
        {
            public bool Enabled;
        }

        static Data m_Data;

        private static Data GetData()
        {
            if (m_Data == null)
            {
                m_Data = new Data()
                {
                    Enabled = true
                };
                if (File.Exists(Path))
                {
                    var contents = File.ReadAllText(Path);
                    EditorJsonUtility.FromJsonOverwrite(contents, m_Data);
                }
            }
            return m_Data;
        }

        private static void Save()
        {
            var contents = EditorJsonUtility.ToJson(m_Data);
            File.WriteAllText(Path, contents);
        }

        public static bool Enabled
        {
            set
            {
                if (GetData().Enabled == value)
                    return;

                GetData().Enabled = value;
                Save();
            }

            get => GetData().Enabled;
        }
    }
}