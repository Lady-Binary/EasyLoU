using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LOU
{
    class Utils
    {
        public static bool IsList(object o)
        {
            if (o == null) return false;
            return o is IList &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static Dictionary<string, FloatingPanel> FindPanelByName(String name)
        {
            Dictionary<string, FloatingPanel> FoundPanels = new Dictionary<string, FloatingPanel>();

            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
            if (fpm != null)
            {
                List<FloatingPanel> AGLMPFPPEDK = (List<FloatingPanel>)Utils.GetInstanceField(fpm, "AGLMPFPPEDK");
                if (AGLMPFPPEDK != null)
                {
                    foreach (FloatingPanel floatingPanel in AGLMPFPPEDK)
                    {
                        //Utils.Log("Panel " + floatingPanel.PanelId);
                        if (name == null || name == "" || floatingPanel.PanelId.ToLower().Contains(name.ToLower()))
                        {
                            //Utils.Log("Panel " + floatingPanel.PanelId + " matches!");
                            FoundPanels.Add(floatingPanel.PanelId, floatingPanel);
                            //if (this.FindPanelResults.Count == 20)
                            //{
                            //    Utils.Log("Breaking at 20, too many.");
                            //    break;
                            //}
                        }

                    }
                }
            }

            //Log("Found total of " + FoundPanels.Count.ToString() + " panels.");
            return FoundPanels;
        }

        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(String name)
        {
            Dictionary<string, DynamicObject> FoundObjects = new Dictionary<string, DynamicObject>();
            IEnumerable Objects;

            Objects = GetDynamicObjects();
            foreach (DynamicObject obj in Objects)
            {
                if (obj.EBHEDGHBHGI.ToLower().Contains(name.ToLower()))
                {
                    //Log("Found " + obj.EBHEDGHBHGI);
                    FoundObjects.Add(obj.ObjectId.ToString(), obj);
                }
                //if (FoundObjects.Count == 20)
                //{
                //    Log("Breaking at 20, too many.");
                //    break;
                //}
            }
            //Log("Found total of " + FoundObjects.Count.ToString() + " items.");
            return FoundObjects;
        }
        public static Dictionary<string, DynamicObject> FindDynamicObjectsByName(String name, ulong containerId)
        {
            Dictionary<string, DynamicObject> FoundObjects = new Dictionary<string, DynamicObject>();
            IEnumerable Objects;

            Objects = ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId);
            foreach (DynamicObject obj in Objects)
            {
                if (name == null || name == "" || obj.EBHEDGHBHGI.ToLower().Contains(name.ToLower()))
                {
                    //Log("Found " + obj.EBHEDGHBHGI);
                    FoundObjects.Add(obj.ObjectId.ToString(), obj);
                }
                //if (FoundObjects.Count == 20)
                //{
                //    Log("Breaking at 20, too many.");
                //    break;
                //}
            }
            //Log("Found total of " + FoundObjects.Count.ToString() + " items.");
            return FoundObjects;
        }
        public static ClientObject FindClientObject(ulong objectId)
        {
            return FindClientObject(objectId, 0);
        }
        public static ClientObject FindClientObject(ulong objectId, ulong containerId)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                    {
                        return obj.GetComponent<ClientObject>();
                    }
                }
            }
            else
            {
                ClientObject clientObject = ClientObjectManager.DJCGIMIDOPB.GetClientObjectById(objectId);
                if (clientObject != null)
                {
                    return clientObject;
                }
            }
            return null;
        }

        public static DynamicObject FindDynamicObject(ulong objectId)
        {
            return FindDynamicObject(objectId, 0);
        }
        public static DynamicObject FindDynamicObject(ulong objectId, ulong containerId)
        {
            if (containerId > 0)
            {
                foreach (DynamicObject obj in ClientObjectManager.DJCGIMIDOPB.GetObjectsInContainer(containerId))
                {
                    if (obj.ObjectId == objectId)
                    {
                        return obj;
                    }
                }
            }
            else
            {
                DynamicObject dynamicObject = ClientObjectManager.DJCGIMIDOPB.GetDynamicObjectById(objectId);
                return dynamicObject;
            }
            return null;
        }

        public static Dictionary<string, ClientObject> FindPermanentObjectByName(String name)
        {
            return FindPermanentObjectByName(name, 30);
        }
        public static Dictionary<string, ClientObject> FindPermanentObjectByName(String name, float distance)
        {
            Dictionary<string, ClientObject> FoundPermanents = new Dictionary<string, ClientObject>();

            IEnumerable Objects = ClientObjectManager.DJCGIMIDOPB.PermanentObjectLookup.Values.OrderBy(obj => Vector3.Distance(obj.transform.position, GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position));

            foreach (ClientObject obj in Objects)
            {
                if (Vector3.Distance(obj.transform.position, GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position) > distance)
                {
                    //Log("Breaking because of distance.");
                    break;
                }
                if (obj.name != null && obj.name != "" && (name == "" || obj.name.ToLower().Contains(name?.ToLower() ?? "")))
                {
                    //Log("Found " + obj.name);
                    FoundPermanents.Add(obj.PermanentId.ToString(), obj);
                }
                //if (FoundPermanents.Count == 20)
                //{
                //    Log("Breaking at 20, too many.");
                //    break;
                //}
            }
            //Log("Found total of " + FoundPermanents.Count.ToString() + " permanents.");
            return FoundPermanents;
        }
        public static ClientObject FindPermanentObject(int permanentId)
        {
            ClientObject clientObject = ClientObjectManager.DJCGIMIDOPB.GetPermanentObjectById(permanentId);
            if (clientObject != null)
            {
                return clientObject;
            }
            return null;
        }

        public static IEnumerable<DynamicObject> GetDynamicObjects()
        {
            //
            // In 0.9.2 was: return ClientObjectManager.DJCGIMIDOPB.__BB_OBFUSCATOR_89();
            // It returned all the dynamic objects.
            //
            // Changed in 0.9.3 to: return ClientObjectManager.DJCGIMIDOPB.MFGFGOCNCDG;
            // The __BB_OBFUSCATOR_89() disappeared.
            // However, the MFGFGOCNCDG property seem to hold all the cached dynamic objects.
            //
            return ClientObjectManager.DJCGIMIDOPB.MFGFGOCNCDG;
        }

        public static GameObject[] GetGameObjects()
        {
            return GameObject.FindObjectsOfType<GameObject>();
        }
        public static Dictionary<string, GameObject> FindGameObjectsByName(string name)
        {
            GameObject[] gameObjects = GetGameObjects();
            Dictionary<string, GameObject> foundGameObjects = new Dictionary<string, GameObject>();
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject.name != null && gameObject.name != "" && (name == "" || gameObject.name.ToLower().Contains(name?.ToLower() ?? "")))
                {
                    if (!foundGameObjects.ContainsKey(gameObject.name))
                    {
                        foundGameObjects.Add(gameObject.name, gameObject);
                    }
                }
            }
            return foundGameObjects;
        }

        public static IEnumerable<MobileInstance> GetNearbyMobiles(float distance)
        {
            if (ClientObjectManager.DJCGIMIDOPB != null)
                return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(distance);
            else
                return ClientObjectManager.DJCGIMIDOPB.GetNearbyMobiles(30);
        }
        public static MobileInstance GetMobile(ulong objectId)
        {
            return ClientObjectManager.DJCGIMIDOPB.GetMobileObjectById(objectId);
        }
        public static List<MobileInstance> FindMobile(string name)
        {
            return FindMobile(name, 30);
        }
        public static List<MobileInstance> FindMobile(string name, float distance)
        {
            IEnumerable<MobileInstance> mobiles = GetNearbyMobiles(distance).OrderBy(obj => Vector3.Distance(obj.transform.position, GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position));
            List<MobileInstance> foundMobiles = new List<MobileInstance>();
            foreach (MobileInstance mobile in mobiles)
            {
                if (Vector3.Distance(mobile.transform.position, GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.Player.transform.position) > distance)
                {
                    //Log("Breaking because of distance.");
                    break;
                }
                if (mobile.EBHEDGHBHGI != null && mobile.EBHEDGHBHGI != "" && (name == "" || mobile.EBHEDGHBHGI.ToLower().Contains(name?.ToLower() ?? "")))
                {

                    if (!foundMobiles.Contains(mobile))
                    {
                        //Utils.Log("found!");
                        foundMobiles.Add(mobile);
                    }
                }
                //if (foundMobiles.Count == 20)
                //{
                //    Log("Breaking at 20, too many.");
                //    break;
                //}
            }
            return foundMobiles;
        }

        public static Vector3 CalculateRelativePosition(Transform transform, Transform ancestor)
        {
            Vector3 position = transform.localPosition;
            Transform t = transform.parent;
            while (t != null && t != ancestor)
            {
                position = position + t.localPosition;
                t = t.parent;
            }
            return position;
        }

        #region reflection stuff
        public static object GetInstanceField(Type type, object instance, string fieldName)
        {
            if (instance == null)
                return null;
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }
        public static object GetInstanceField<T>(T instance, string fieldName)
        {
            if (instance == null)
                return null;
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(T).GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }
        public static object GetStaticClassField(Type type, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(null);
        }
        public static void SetInstanceField<T1>(T1 instance, string fieldName, object value)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            FieldInfo field = typeof(T1).GetField(fieldName, bindFlags);
            field.SetValue(instance, value);
        }
        #endregion reflection stuff

        #region debug and logging stuff
        private static bool IsDictionary(object o)
        {
            if (o == null) return false;
            return o is IDictionary &&
                   o.GetType().IsGenericType &&
                   o.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(Dictionary<,>));
        }

        public static void Log(string s)
        {
            string message = string.Format("{0} - LOU - {1}", DateTime.UtcNow.ToString("o"), s);
            try
            {
                System.Diagnostics.Debug.WriteLine(message);
                UnityEngine.Debug.Log(message);
            }
            catch (Exception ex)
            {
            }
        }

        public static void LogComponents(MonoBehaviour c)
        {
            if (c.GetComponent<DynamicWindow>() != null)
            {
                Log("DynamicWindow");
                DynamicWindow comp = c.GetComponent<DynamicWindow>();
                LogProps(comp);
            }
            if (c.GetComponent<UIWidget>() != null)
            {
                Log("UIWidget");
                UIWidget comp = c.GetComponent<UIWidget>();
                LogProps(comp);

            }
            if (c.GetComponent<UnityEngine.BoxCollider>() != null)
            {
                Log("UnityEngine.BoxCollider");
                UnityEngine.BoxCollider comp = c.GetComponent<UnityEngine.BoxCollider>();
                LogProps(comp);
            }
            if (c.GetComponent<DynamicWindowTwoLabelButton>() != null)
            {
                Log("DynamicWindowTwoLabelButton");
                DynamicWindowTwoLabelButton comp = c.GetComponent<DynamicWindowTwoLabelButton>();
                LogProps(comp);
            }

            if (c.GetComponent<DynamicWindowDefaultButton>() != null)
            {
                Log("DynamicWindowDefaultButton");
                DynamicWindowDefaultButton comp = c.GetComponent<DynamicWindowDefaultButton>();
                LogProps(comp);
            }
            if (c.GetComponent<UIImageButton>() != null)
            {
                Log("UIImageButton");
                UIImageButton comp = c.GetComponent<UIImageButton>();
                LogProps(comp);
            }
            if (c.GetComponent<UIPlaySound>() != null)
            {
                Log("UIPlaySound");
                UIPlaySound comp = c.GetComponent<UIPlaySound>();
                LogProps(comp);
            }
            if (c.GetComponent<UIButtonMessage>() != null)
            {
                Log("UIButtonMessage");
                UIButtonMessage comp = c.GetComponent<UIButtonMessage>();
                LogProps(comp);
            }

            if (c.GetComponent<UIEventListener>() != null)
            {
                Log("c has listener!");
            }
            if (c.gameObject != null && c.gameObject.GetComponent<UIEventListener>() != null)
            {
                Log("c.gameObject has listener!");
            }
            if (c.transform != null && c.transform.GetComponent<UIEventListener>() != null)
            {
                Log("c.transform has listener!");
            }

            if (c.GetComponent<UILabel>() != null)
            {
                Log("UILabel");
                UILabel comp = c.GetComponent<UILabel>();

                LogProps(comp);

                if (comp.GBHBIODJFCD == "[412A08]Craft")
                {
                    Log("CRAFT BUTTON FOUND");

                    if (comp.GetComponent<UIEventListener>() != null)
                    {
                        Log("comp has listener!");
                    }
                    if (comp.transform != null && comp.transform.GetComponent<UIEventListener>() != null)
                    {
                        Log("comp.transform has listener!");
                    }
                    if (comp.gameObject != null && comp.gameObject.GetComponent<UIEventListener>() != null)
                    {
                        Log("comp.gameObject has listener!");
                    }
                    if (c.GetComponent<UIEventListener>() != null)
                    {
                        Log("c has listener!");
                    }
                }
            }
            if (c.GetComponent<DynamicWindowScrollableLabel>() != null)
            {
                Log("DynamicWindowScrollableLabel");
                DynamicWindowScrollableLabel comp = c.GetComponent<DynamicWindowScrollableLabel>();
                LogProps(comp);
            }
            if (c.GetComponent<UIEventListener>() != null)
            {
                Log("UIEventListener");
                UIEventListener comp = c.GetComponent<UIEventListener>();
                LogProps(comp);
            }
            if (c.GetComponent<BoxCollider>() != null)
            {
                Log("BoxCollider");
                BoxCollider comp = c.GetComponent<BoxCollider>();
                LogProps(comp);
            }
        }

        public static void LogComponents(GameObject c)
        {
            Log("trying to enumerate components " + c.GetComponents<UnityEngine.Component>());
            int i = 0;
            foreach (UnityEngine.Component comp in c.GetComponents<UnityEngine.Component>())
            {
                i = i + 1;
                Log(i.ToString());
                Log(comp.name);
                Log(comp.tag);
                Log(comp.GetType().ToString());
            }
            if (c.GetComponent<DynamicWindow>() != null)
            {
                Log("DynamicWindow");
                DynamicWindow comp = c.GetComponent<DynamicWindow>();
                LogProps(comp);

            }
            if (c.GetComponent<UIWidget>() != null)
            {
                Log("UIWidget");
                UIWidget comp = c.GetComponent<UIWidget>();
                LogProps(comp);

            }
            if (c.GetComponent<UnityEngine.BoxCollider>() != null)
            {
                Log("UnityEngine.BoxCollider");
                UnityEngine.BoxCollider comp = c.GetComponent<UnityEngine.BoxCollider>();
                LogProps(comp);
                if (comp.GetComponent<UIEventListener>() != null)
                {
                    Log("boxcollider2 comp has listener!");
                }
            }
            if (c.GetComponent<DynamicWindowTwoLabelButton>() != null)
            {
                Log("DynamicWindowTwoLabelButton");
                DynamicWindowTwoLabelButton comp = c.GetComponent<DynamicWindowTwoLabelButton>();
                LogProps(comp);
            }

            if (c.GetComponent<DynamicWindowDefaultButton>() != null)
            {
                Log("DynamicWindowDefaultButton");
                DynamicWindowDefaultButton comp = c.GetComponent<DynamicWindowDefaultButton>();
                LogProps(comp);
            }
            if (c.GetComponent<UIImageButton>() != null)
            {
                Log("UIImageButton");
                UIImageButton comp = c.GetComponent<UIImageButton>();
                LogProps(comp);
            }
            if (c.GetComponent<UIPlaySound>() != null)
            {
                Log("UIPlaySound");
                UIPlaySound comp = c.GetComponent<UIPlaySound>();
                LogProps(comp);
            }
            if (c.GetComponent<UIButtonMessage>() != null)
            {
                Log("UIButtonMessage");
                UIButtonMessage comp = c.GetComponent<UIButtonMessage>();
                LogProps(comp);
            }
            if (c.GetComponent<UIButtonMessage>() != null)
            {
                Log("UIEventListener");
                UIEventListener comp = c.GetComponent<UIEventListener>();
                LogProps(comp);
            }

            if (c.GetComponent<UILabel>() != null)
            {
                Log("UILabel");
                UILabel comp = c.GetComponent<UILabel>();

                LogProps(comp);

                if (comp.GBHBIODJFCD == "[412A08]Craft")
                {
                    Log("CRAFT BUTTON FOUND");

                    if (comp.GetComponent<UIEventListener>())
                    {
                        Log("comp has listener!");
                    }
                    if (comp.transform != null && comp.transform.GetComponent<UIEventListener>())
                    {
                        Log("comp.transform has listener!");
                    }
                    if (comp.gameObject != null && comp.gameObject.GetComponent<UIEventListener>())
                    {
                        Log("comp.gameObject has listener!");
                    }
                    if (c.GetComponent<UIEventListener>())
                    {
                        Log("c has listener!");
                    }
                }
            }
            if (c.GetComponent<DynamicWindowScrollableLabel>() != null)
            {
                Log("DynamicWindowScrollableLabel");
                DynamicWindowScrollableLabel comp = c.GetComponent<DynamicWindowScrollableLabel>();
                LogProps(comp);
            }
            if (c.GetComponent<UIEventListener>() != null)
            {
                Log("UIEventListener");
                UIEventListener comp = c.GetComponent<UIEventListener>();
                LogProps(comp);
            }
            if (c.GetComponent<BoxCollider>() != null)
            {
                Log("BoxCollider");
                BoxCollider comp = c.GetComponent<BoxCollider>();
                LogProps(comp);
                if (comp.GetComponent<UIEventListener>() != null)
                {
                    Log("boxcollider1 comp has listener!");
                }
            }
        }

        public static void LogChildren(Transform o)
        {
            Log("***STARTING CHILDREN OF " + o.name);
            List<GameObject> Children = new List<GameObject>();
            for (var c = 0; c < o.transform.childCount; c++)
            {
                Children.Add(o.transform.GetChild(c).gameObject);
            }
            Log(Children.Count.ToString() + " children");
            foreach (var c in Children)
            {
                LogProps(c);
                if (c.GetComponent<UIEventListener>() != null)
                {
                    Log("c has listener!");
                }
                if (c.gameObject != null && c.gameObject.GetComponent<UIEventListener>() != null)
                {
                    Log("c.gameObject has listener!");
                }
                if (c.transform != null && c.transform.GetComponent<UIEventListener>() != null)
                {
                    Log("c.transform has listener!");
                }
                //GBHBIODJFCD =[412A08] / Craft All
                Log("trying to enumerate " + c.GetComponents<UnityEngine.Component>());
                int i = 0;
                foreach (UnityEngine.Component comp in c.GetComponents<UnityEngine.Component>())
                {
                    i = i + 1;
                    Log(i.ToString());
                    Log(comp.name);
                    Log(comp.tag);
                    Log(comp.GetType().ToString());
                }
                Log("finish");
                LogComponents(c);
            }
            Log("***ENDING CHILDREN OF " + o.name);
        }

        public static void LogObject(DynamicObject obj)
        {
            Log(obj.GetInstanceID().ToString() + " object start ----------");
            LogProps(obj);
            Log("***CLIENT OBJECT***");
            LogProps(obj.AOJMJNFMBJO);
            Log("***TRANSFORM***");
            LogProps(obj.transform);
            Log("***GAME OBJECT***");
            LogProps(obj.gameObject);
            Log(obj.GetInstanceID().ToString() + " object end ----------");
        }

        public static void LogProps(System.Object obj)
        {
            Log("props start ---");
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(obj);
                Log(name + "=" + value);
                if (IsDictionary(value))
                {
                    foreach (String key in ((IDictionary)value).Keys)
                    {
                        Log(name + ".Key=" + key);
                        Log(name + ".Value=" + ((IDictionary)value)[key].ToString());
                    }
                }
            }

            Type type = obj.GetType();
            foreach (var f in type.GetFields().Where(f => f.IsPrivate | f.IsPublic | f.IsStatic))
            {
                Log(f.Name + "=" + f.GetValue(obj));
            }
            Log("props end ---");
        }
        #endregion
    }
}
