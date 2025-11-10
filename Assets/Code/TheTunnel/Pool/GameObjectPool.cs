using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TheTunnel.Pool
{
    public class GameObjectPool<T> where T : MonoBehaviour
    {
        private Stack<T> _objectStack = new();
        private T _prefab;
        private Transform _holderTransform;
        private int _initialSize;
        private Action<T> _onObjectCreated;

        public GameObjectPool(T prefab, Transform holderTransform, int initialSize, Action<T> onObjectCreated = null)
        {
            _prefab = prefab;
            _holderTransform = holderTransform;
            _initialSize = initialSize;
            _onObjectCreated = onObjectCreated;
            PreparePool();
        }

        public T GetObject()
        {
            if (_objectStack.Count == 0)
            {
                CreateObject();
            }
            T obj = _objectStack.Pop();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void ReturnObject(T obj)
        {
            obj.gameObject.SetActive(false);
            _objectStack.Push(obj);
        }

        private void PreparePool()
        {
            for (int i = 0; i < _initialSize; i++)
            {
                CreateObject();
            }
        }

        private T CreateObject()
        {
            T obj = Object.Instantiate(_prefab, _holderTransform);
            obj.transform.rotation = Quaternion.identity;
            obj.gameObject.SetActive(false);
            _objectStack.Push(obj);
            _onObjectCreated?.Invoke(obj);
            return obj;
        }

        public List<T> GetActiveObjects()
        {
            List<T> activeObjects = new List<T>();
            foreach (Transform child in _holderTransform)
            {
                if (child.gameObject.activeSelf)
                {
                    var obj = child.GetComponent<T>();
                    if (obj != null)
                    {
                        activeObjects.Add(obj);
                    }
                }
            }
            return activeObjects;
        }
    }
}