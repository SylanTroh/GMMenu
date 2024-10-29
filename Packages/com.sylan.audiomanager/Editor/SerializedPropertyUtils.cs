#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Sylan.AudioManager.EditorUtilities
{
    public class SerializedPropertyUtils : Editor
    {
        /// <summary>
        /// Find exactly one object of Type T in the scene hierarchy.
        /// </summary>
        /// <typeparam name="T">Type of Object to get</typeparam>
        /// <param name="obj">Local variable to contain the object</param>
        /// <returns>True if successful, false if unsuccessful</returns>
        public static bool GetObject<T>(out T obj) where T : MonoBehaviour
        {
            T[] objects = UnityEngine.Object.FindObjectsOfType<T>();

            if (objects.Length == 0)
            {
                Debug.Log("[EditorUtilities] No Objects of type " + typeof(T).ToString());
                obj = null;
                return true;
            }
            if (objects.Length > 1)
            {
                Debug.LogError("[EditorUtilities] More than one object of type " + typeof(T).ToString());
                obj = null;
                return false;
            }
            obj = objects[0];
            return true;
        }
        public static bool GetObjects<T>(out T[] obj) where T : MonoBehaviour
        {
            T[] objects = UnityEngine.Object.FindObjectsOfType<T>();
            obj = objects;
            return true;
        }
        /// <summary>
        /// Set Serialized Property of Type T. Property must not be an array. 
        /// </summary>
        /// <typeparam name="T">Type of Object to set</typeparam>
        /// <param name="serializedObject">Object with the property</param>
        /// <param name="propertyName">Name of Serialized Property</param>
        public static void PopulateSerializedProperty<T>(SerializedObject serializedObject, string propertyName) where T : MonoBehaviour
        {
            if (serializedObject == null) return;
            SerializedProperty property;
            property = serializedObject.FindProperty(propertyName);

            // Get one matching components in the scene
            GetObject<T>(out T obj);
            property.objectReferenceValue = obj;

            // Apply the changes to the component
            serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// Set Serialized Property of Type T. Property must be an array, and will be filled with all the objects found.
        /// </summary>
        /// <typeparam name="T">Type of Object to set</typeparam>
        /// <param name="serializedObject">Object with the property</param>
        /// <param name="propertyName">Name of Serialized Property</param>
        public static void PopulateSerializedArray<T>(SerializedObject serializedObject, string propertyName) where T : MonoBehaviour
        {
            if(serializedObject == null) return;
            SerializedProperty arrayProperty;
            arrayProperty = serializedObject.FindProperty(propertyName);

            // Get all the matching components in the scene
            GetObjects<T>(out T[] objects);

            // Assign the serialized references to the array
            arrayProperty.ClearArray();
            arrayProperty.arraySize = objects.Length;
            for (int i = 0; i < objects.Length; i++)
            {
                arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = objects[i];
            }
            // Apply the changes to the component
            serializedObject.ApplyModifiedProperties();
        }
        /// <summary>
        /// Find an object of Type T in the scene hierarchy as a SerializedObject.
        /// </summary>
        /// <typeparam name="T">Type of Object to get</typeparam>
        /// <param name="serializedObject">Local variable to contain the object</param>
        /// <returns>True if successful, false if unsuccessful</returns>
        public static bool GetSerializedObject<T>(out SerializedObject serializedObject) where T : MonoBehaviour
        {
            serializedObject = null;
            if (!GetObject(out T obj))
            {
                EditorApplication.isPlaying = false;
                return false;
            }
            if(obj != null) serializedObject = new SerializedObject(obj);
            return true;
        }
        /// <summary>
        /// Find objects of Type T in the scene hierarchy as an array of type SerializedObject.
        /// </summary>
        /// <typeparam name="T">Type of Object to get</typeparam>
        /// <param name="serializedObjects">Local variable to contain the objects</param>
        /// <returns>True if successful, false if unsuccessful</returns>
        public static bool GetSerializedObjects<T>(out SerializedObject[] serializedObjects) where T : MonoBehaviour
        {
            serializedObjects = null;
            if (!GetObjects(out T[] obj))
            {
                EditorApplication.isPlaying = false;
                return false;
            }
            serializedObjects = new SerializedObject[obj.Length];
            for (int i = 0; i < obj.Length; i++)
            {
                serializedObjects[i] = new SerializedObject(obj[i]);
            }
            return true;
        }
    }
}
#endif