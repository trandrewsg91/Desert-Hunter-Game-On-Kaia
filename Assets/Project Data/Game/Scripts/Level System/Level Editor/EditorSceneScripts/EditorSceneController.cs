#pragma warning disable 649

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.SquadShooter;

namespace Watermelon.LevelSystem
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] private GameObject container;
        public List<GameObject> rooms;
        private int selectedRoom = 0;
        [SerializeField] Vector3 spawnPoint;
        [SerializeField] Vector3 exitPoint;
        [SerializeField] float spawnPointSphereSize;
        [SerializeField] float exitPointSphereSize;
        [SerializeField] Color spawnPointColor;
        [SerializeField] Color exitPointColor;
        private Color backupColor;

        public GameObject Container { set => container = value; }
        public Vector3 SpawnPoint { get => spawnPoint; set => spawnPoint = value; }
        public Vector3 ExitPoint { get => exitPoint; set => exitPoint = value; }
        public Color SpawnPointColor { get => spawnPointColor; set => spawnPointColor = value; }
        public Color ExitPointColor { get => exitPointColor; set => exitPointColor = value; }

        public EditorSceneController()
        {
            instance = this;
            rooms = new List<GameObject>();
        }

        public void SpawnItem(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, int hash, bool selectSpawnedItem = false)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);

            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            LevelEditorItem levelEditorItem = gameObject.AddComponent<LevelEditorItem>();
            levelEditorItem.hash = hash;

            if (selectSpawnedItem)
            {
                Selection.activeGameObject = gameObject;
            }
        }


        public void SpawnEnemy(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, EnemyType type, bool isElite, Vector3[] pathPoints)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            LevelEditorEnemy levelEditorEnemy = gameObject.AddComponent<LevelEditorEnemy>();
            levelEditorEnemy.type = type;
            levelEditorEnemy.isElite = isElite;
            GameObject pointsContainer = new GameObject("PathPointsContainer");
            pointsContainer.transform.SetParent(gameObject.transform);
            levelEditorEnemy.pathPointsContainer = pointsContainer.transform;
            pointsContainer.transform.localPosition = Vector3.zero;

            GameObject sphere;

            for (int i = 0; i < pathPoints.Length; i++)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(levelEditorEnemy.pathPointsContainer);
                sphere.transform.localPosition = pathPoints[i] - gameObject.transform.localPosition;
                sphere.transform.localScale = Vector3.one * 0.78125f;
                levelEditorEnemy.pathPoints.Add(sphere.transform);
            }

            levelEditorEnemy.ApplyMaterialToPathPoints();
            Selection.activeGameObject = gameObject;
        }

        public void SpawnExitPoint(GameObject prefab, Vector3 position)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;

            LevelEditorExitPoint exitPoint = gameObject.AddComponent<LevelEditorExitPoint>();
        }

        public void SpawnChest(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, LevelChestType type)
        {
            GameObject gameObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            gameObject.transform.SetParent(rooms[selectedRoom].transform);
            gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = rotation;
            gameObject.transform.localScale = scale;
            var levelEditorChest = gameObject.AddComponent<LevelEditorChest>();
            levelEditorChest.type = type;
            Selection.activeGameObject = gameObject;
        }

        public void SpawnRoom()
        {
            GameObject room = new GameObject();
            room.transform.SetParent(container.transform);

            if(rooms.Count > 0)
            {
                room.transform.localPosition = new Vector3(0, 0, rooms.Count * 350);
            }

            rooms.Add(room);
            room.name = "Room #" + rooms.Count;
            selectedRoom = rooms.Count - 1;
        }

        public void DeleteRoom(int index)
        {
            GameObject room = rooms[index];
            rooms.RemoveAt(index);
            DestroyImmediate(room);

        }

        public void SelectRoom(int index)
        {
            selectedRoom = index;
            Selection.activeGameObject = rooms[selectedRoom];
            SceneView.lastActiveSceneView.FrameSelected();
        }
        public ItemEntityData[] CollectItemsFromRoom(int roomIndex)
        {
            LevelEditorItem[] editorData = rooms[roomIndex].GetComponentsInChildren<LevelEditorItem>();
            ItemEntityData[] result = new ItemEntityData[editorData.Length];

            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new ItemEntityData(editorData[i].hash, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale);
            }

            return result;
        }

        public EnemyEntityData[] CollectEnemiesFromRoom(int roomIndex)
        {
            LevelEditorEnemy[] editorData = rooms[roomIndex].GetComponentsInChildren<LevelEditorEnemy>();
            EnemyEntityData[] result = new EnemyEntityData[editorData.Length];

            for (int i = 0; i < editorData.Length; i++)
            {
                result[i] = new EnemyEntityData(editorData[i].type, editorData[i].transform.localPosition, editorData[i].transform.localRotation, editorData[i].transform.localScale, editorData[i].isElite,editorData[i].GetPathPoints());
            }

            return result;
        }

        public bool CollectExitPointFromRoom(int roomIndex, out Vector3 position)
        {
            LevelEditorExitPoint editorData = rooms[roomIndex].GetComponentInChildren<LevelEditorExitPoint>();

            if(editorData == null)
            {
                position = Vector3.zero;

                return false;
            }
            else
            {
                position = editorData.transform.localPosition;

                return true;
            }
        }

        public List<LevelEditorChest> CollectChestFromRoom(int roomIndex)
        {
            var result = new List<LevelEditorChest>();
            rooms[roomIndex].GetComponentsInChildren(result);

            return result;
        }


        public void SelectGameObject(GameObject selectedGameObject)
        {
            Selection.activeGameObject = selectedGameObject;
        }


        public void Clear()
        {
            rooms.Clear();

            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }

        public void OnDrawGizmos()
        {
            if(selectedRoom < rooms.Count)
            {
                backupColor = Gizmos.color;

                Gizmos.color = spawnPointColor;
                Gizmos.DrawWireSphere(rooms[selectedRoom].transform.position + spawnPoint, spawnPointSphereSize);

                Gizmos.color = exitPointColor;
                Gizmos.DrawWireSphere(rooms[selectedRoom].transform.position + exitPoint, exitPointSphereSize);

                Gizmos.color = backupColor;
            }
            
        }


#endif
    }
}