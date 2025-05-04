using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.RagdollAnimatorDemo
{
    public class Demo_Ragd_ObjectPoolingManager : MonoBehaviour
    {
        public static Demo_Ragd_ObjectPoolingManager Get;

        public GameObject ToSpawn;
        public int InitialSpawnCount = 10;
        public int InitialPoolSize = 20;

        public Vector2 SpawnArea = new Vector2(4f, 4f);

        List<GameObject> availableList = new List<GameObject>();
        List<GameObject> activeSpawnedList = new List<GameObject>();

        private void Start()
        {
            Get = this;

            for (int i = 0; i < InitialPoolSize; i++)
            {
                GenerateObjectForThePool();
            }

            for (int i = 0; i < InitialSpawnCount; i++)
            {
                SpawnNewObject();
            }
        }

        public void SpawnObjects(int count)
        {
            for (int i = 0; i < count; i += 1) SpawnNewObject();
        }

        void SpawnNewObject()
        {
            if (availableList.Count == 0) GenerateObjectForThePool();

            GameObject target = availableList[availableList.Count - 1];
            availableList.RemoveAt(availableList.Count - 1);

            target.transform.SetParent(null);

            target.transform.position = GetSpawnPosition();
            target.transform.rotation = Quaternion.identity;

            target.SetActive(true);
            target.SendMessage("ResetOnStart");
        }

        Vector3 GetSpawnPosition()
        {
            Vector3 spawnPos = transform.position;
            spawnPos.x += Random.Range(-SpawnArea.x, SpawnArea.x) * 0.5f;
            spawnPos.z += Random.Range(-SpawnArea.y, SpawnArea.y) * 0.5f;
            return spawnPos;
        }

        void GenerateObjectForThePool()
        {
            GameObject forPool = Instantiate(ToSpawn);
            GiveBackObject(forPool);
        }

        internal void GiveBackObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(transform, true);
            availableList.Add(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnArea.x, 0f, SpawnArea.y));
        }


#if UNITY_EDITOR

        [UnityEditor.CustomEditor(typeof(Demo_Ragd_ObjectPoolingManager))]
        public class Demo_Ragd_ObjectPoolingManagerEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                Demo_Ragd_ObjectPoolingManager targetScript = (Demo_Ragd_ObjectPoolingManager)target;
                DrawDefaultInspector();

                GUILayout.Space(10f);

                if (Application.isPlaying == false) GUI.enabled = false;
                if (GUILayout.Button("Spawn 1 new object")) { targetScript.SpawnNewObject(); }
                if (GUILayout.Button("Spawn 4 new objects")) { targetScript.SpawnObjects(4); }
                if (GUILayout.Button("Spawn 10 new objects")) { targetScript.SpawnObjects(10); }
            }
        }
#endif


    }
}