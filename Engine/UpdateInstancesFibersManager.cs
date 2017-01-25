namespace EmergencyV
{
    // System
    using System;
    using System.Linq;
    using System.Collections.Generic;

    // RPH
    using Rage;

    internal sealed class UpdateInstancesFibersManager
    {
        private static UpdateInstancesFibersManager instance;
        public static UpdateInstancesFibersManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new UpdateInstancesFibersManager();
                return instance;
            }
        }

        private UpdateInstancesFibersManager()
        {
        }

        public int CurrentFibersCount { get { return UpdateInstancesFibersByType.Count; } }

        public T[] GetAllInstancesOfType<T>()
        {
            return CurrentInstancesByType.ContainsKey(typeof(T)) ? CurrentInstancesByType[typeof(T)].Cast<T>().ToArray() : null;
        }

        public object[] GetAllInstancesOfType(Type type)
        {
            return CurrentInstancesByType.ContainsKey(type) ? CurrentInstancesByType[type].ToArray() : null;
        }

        private Dictionary<Type, List<object>> CurrentInstancesByType { get; } = new Dictionary<Type, List<object>>();
        private Dictionary<Type, GameFiber> UpdateInstancesFibersByType { get; } = new Dictionary<Type, GameFiber>();
        private Dictionary<Type, UpdateTypeData> UpdatableDataByType { get; } = new Dictionary<Type, UpdateTypeData>();

        public bool IsUpdateDataSetForType<T>() => UpdatableDataByType.ContainsKey(typeof(T));

        public void SetUpdateDataForType<T>(Func<T, bool> canDoUpdateCallback, Action<T> onInstanceUpdateCallback, Action<T> onInstanceUnregisteredCallback, int updatesInterval = 0)
        {
            Type t = typeof(T);

            UpdateTypeData d = new UpdateTypeData()
            {
                UpdatesInterval = updatesInterval,
                InstanceCanDoUpdateCallback = canDoUpdateCallback == null ? (Func<object, bool>)null : (o) => { return canDoUpdateCallback((T)o); },
                OnInstanceUpdateCallback = onInstanceUpdateCallback == null ? (Action<object>)null : (o) => { onInstanceUpdateCallback((T)o); },
                OnInstanceUnregisteredCallback = onInstanceUnregisteredCallback == null ? (Action<object>)null : (o) => { onInstanceUnregisteredCallback((T)o); }
            };

            if (UpdatableDataByType.ContainsKey(t))
            {
                UpdatableDataByType[t] = d;
            }
            else
            {
                UpdatableDataByType.Add(t, d);
            }
        }

        public void RegisterInstance<T>(T o)
        {
            Type t = o.GetType();

            if (CurrentInstancesByType.ContainsKey(t))
            {
                CurrentInstancesByType[t].Add(o);
            }
            else
            {
                CurrentInstancesByType.Add(t, new List<object>() { o });
            }

            if (!UpdateInstancesFibersByType.ContainsKey(t))
            {
                Game.LogTrivial($"[{nameof(UpdateInstancesFibersManager)}] Creating update fiber for {t.Name} instances");
                GameFiber fiber = GameFiber.StartNew(() => { UpdateInstancesLoop<T>(CurrentInstancesByType[t]); }, $"{t.Name} Update Fiber");
                UpdateInstancesFibersByType.Add(t, fiber);
            }
        }

        private void UpdateInstancesLoop<T>(List<object> list)
        {
            Func<object, bool> instanceCanDoUpdate = UpdatableDataByType[typeof(T)].InstanceCanDoUpdateCallback;
            Action<object> onInstanceUpdateCallback = UpdatableDataByType[typeof(T)].OnInstanceUpdateCallback;
            Action<object> onInstanceUnregisteredCallback = UpdatableDataByType[typeof(T)].OnInstanceUnregisteredCallback;
            int updatesInterval = UpdatableDataByType[typeof(T)].UpdatesInterval;

            while (true)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    object o = list[i];
                    if (o != null && (instanceCanDoUpdate == null || instanceCanDoUpdate(o)))
                    {
                        onInstanceUpdateCallback?.Invoke(o);
                    }
                    else
                    {
                        onInstanceUnregisteredCallback?.Invoke(o);
                        list.RemoveAt(i);
                    }
                }

                GameFiber.Sleep(updatesInterval);
            }
        }

        private class UpdateTypeData
        {
            public int UpdatesInterval { get; set; }
            public Func<object, bool> InstanceCanDoUpdateCallback { get; set; }
            public Action<object> OnInstanceUpdateCallback { get; set; }
            public Action<object> OnInstanceUnregisteredCallback { get; set; }
        }
    }
}
