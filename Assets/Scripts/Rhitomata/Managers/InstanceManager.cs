using System;
using System.Collections.Generic;
using Rhitomata.Data;
using UnityEngine;

namespace Rhitomata {
    /// <summary>
    /// A class to handle an instance where the instance had to implement InstanceableObject.
    /// This includes managing the instances via an id based hierarchy.
    /// </summary>
    public class InstanceManager<T> : MonoBehaviour where T : IInstanceableObject {
        private static InstanceManager<T> Self {
            get => _self = _self ? _self : FindFirstObjectByType<InstanceManager<T>>();
            set => _self = value;
        }
        private static InstanceManager<T> _self;

        /// <summary>
        /// The main list where objects are mostly stored
        /// </summary>
        public static List<T> objects = new List<T>();

        /// <summary>
        /// This is used for getting a free to use id, will increment as more objects are spawned
        /// </summary>
        public static int lastInstanceId = 0;

        /// <summary>
        /// Register an object into the list.
        /// </summary>
        /// <param name="id">The target instance id, if it's below 0, it will try to get a free id to assign to.</param>
        /// <param name="obj">The object to register</param>
        /// <param name="overrideAnyway">If an existing object already occupies the id, it will replace it and redirect the old object to a new id.</param>
        /// <returns>The valid new id of the object.</returns>
        public static int Register(int id, T obj, bool overrideAnyway = false) {
            if (IsAlreadyRegistered(obj)) return id;

            T existingObject = default;
            if (id < 0) {
                // If id is -1, get a new free ID
                id = GetFreeIdInternal(lastInstanceId);
            } else {
                if (IsIdOccupied(id)) {
                    if (overrideAnyway) {
                        // The ID is occupied, remove the existing object with the same ID
                        existingObject = objects.Find(val => val.GetId() == id);
                        var equalityComparer = EqualityComparer<T>.Default;
                        if (!equalityComparer.Equals(existingObject, obj))
                            Remove(existingObject);
                        else
                            existingObject = default;
                    } else {
                        id = GetFreeIdInternal(lastInstanceId);
                    }
                }
            }

            obj.SetId(id);
            objects.Add(obj);
            Self.OnAdd(id, obj);
            RegisterIfLast(id);

            if (existingObject != null)
                existingObject.OnIdRedirected(existingObject.GetId(), Register(-1, existingObject));

            return id;
        }

        public static bool IsIdOccupied(int id) => objects.Exists(val => val.GetId() == id);
        public static bool IsAlreadyRegistered(T value) => objects.Contains(value);

        public static void Add(int id, T value, bool force = false) {
            if (objects.Contains(value) && !force) return;

            value.SetId(id);
            objects.Add(value);
            Self.OnAdd(id, value);
        }

        public static void Remove(T value) {
            if (objects.Contains(value)) {
                objects.Remove(value);
                Self.OnRemoved(value.GetId());
            }
        }

        private static void RegisterIfLast(int id) {
            if (id > lastInstanceId) lastInstanceId = id;
        }

        private static int GetFreeIdInternal(int id) {
            if (IsIdOccupied(id))
                return GetFreeIdInternal(id + 1);
            return id;
        }

        public static void Remove(int id) {
            if (id == lastInstanceId) lastInstanceId--;
            if (IsIdOccupied(id)) {
                objects.Remove(GetObject(id));
                Self.OnRemoved(id);
            }
        }

        public static int IndexOf(T value) =>
            objects.IndexOf(value);
        public static T Find(Predicate<T> predicate) =>
            objects.Find(predicate);
        public static List<T> FindAll(Predicate<T> predicate) =>
            objects.FindAll(predicate);

        /// <summary>
        /// Get an object based on the id
        /// </summary>
        /// <param name="id">The instance id of the object</param>
        /// <returns>The object with the instance id</returns>
        public static T GetObject(int id) =>
            objects.Find(val => val.GetId() == id);

        /// <summary>
        /// Unfinished method
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <param name="notify"></param>
        public static void MigrateId(T obj, int id, bool notify = true) {

        }

        /// <summary>
        /// Clears the whole dictionary on the instance manager
        /// </summary>
        /// <returns>The deleted count of the dictionary</returns>
        public static int ClearDictionary() {
            int count = objects.Count;
            objects.Clear();
            lastInstanceId = 0;
            return count;
        }

        public virtual int OnRegister(int id, T obj, bool overrideRequested) {
            return id;
        }

        public virtual void OnAdd(int id, T value) {

        }

        public virtual void OnRemoved(int id) {

        }

        public static void CleanUnreferencedObjects() {
            var invalidObjects = new List<T>();
            foreach (var obj in objects) {
                if (obj == null)
                    invalidObjects.Add(obj);
            }

            foreach (var inv in invalidObjects)
                objects.Remove(inv);
        }
    }
}