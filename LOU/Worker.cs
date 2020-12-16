using CoreUtil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LOU
{
    public class Worker : MonoBehaviour
    {
        private const bool VERBOSE_DEBUG = false;
        private bool Intercepting = false;

        private int ProcessId = -1;

        private Assembly AssemblyCSharp = null;

        private String ClientStatusMemoryMapMutexName;
        private String ClientStatusMemoryMapName;
        private Int32 ClientStatusMemoryMapSize;
        private MemoryMap ClientStatusMemoryMap;

        private long LastClientCommandTimestamp;
        private int ClientCommandId = 0;
        private String ClientCommandsMemoryMapMutexName;
        private String ClientCommandsMemoryMapName;
        private Int32 ClientCommandsMemoryMapSize;
        private MemoryMap ClientCommandsMemoryMap;

        private ApplicationController applicationController;
        private InputController inputController;
        private LocalPlayer player;

        private ClientStatus.FINDBUTTONStruct[] FindButtonResults;
        private ClientStatus.FINDINPUTStruct[] FindInputResults;
        private ClientStatus.FINDITEMStruct[] FindItemResults;
        private ClientStatus.FINDLABELStruct[] FindLabelResults;
        private ClientStatus.FINDMOBILEStruct[] FindMobileResults;
        private ClientStatus.FINDPANELStruct[] FindPanelResults;
        private ClientStatus.FINDPERMANENTStruct[] FindPermanentResults;

        private Dictionary<String, object> CustomVars;

        private float ScanJournalTime;
        private string ScanJournalMessage;

        private bool leftMouseDown;
        private bool rightMouseDown;

        private Vector3 lastMouseClickPosition;
        private ClientObject lastMouseClickClientObject;
        private string tooltipText;

        public void Start()
        {
            Utils.Log("EasyLOU - " + Assembly.GetExecutingAssembly().GetName().Version.ToString() + " - LOU.dll started!");

            this.ProcessId = Process.GetCurrentProcess().Id;
            Utils.Log("ProcessId: " + this.ProcessId.ToString());

            this.ClientStatusMemoryMapMutexName = "ELOU_CS_MX_" + this.ProcessId.ToString();
            this.ClientStatusMemoryMapName = "ELOU_CS_" + this.ProcessId.ToString();
            this.ClientStatusMemoryMapSize = 1024 * 1024 * 10;
            this.ClientStatusMemoryMap = new MemoryMap(this.ClientStatusMemoryMapName, this.ClientStatusMemoryMapSize, this.ClientStatusMemoryMapMutexName);
            if (!ClientStatusMemoryMap.OpenOrCreate())
            {
                throw new Exception("Could not open or create Client Status MemoryMap!");
            }

            this.ClientCommandsMemoryMapMutexName = "ELOU_CC_MX_" + this.ProcessId.ToString();
            this.ClientCommandsMemoryMapName = "ELOU_CC_" + this.ProcessId.ToString();
            this.ClientCommandsMemoryMapSize = 1024 * 1024;
            this.ClientCommandsMemoryMap = new MemoryMap(this.ClientCommandsMemoryMapName, this.ClientCommandsMemoryMapSize, this.ClientCommandsMemoryMapMutexName);
            if (!ClientCommandsMemoryMap.OpenOrCreate())
            {
                throw new Exception("Could not open or create Client Commands MemoryMap!");
            }

            // Cache AssemblyCSharp assembly, it will come handy for dynamic types resolution and other shenanigans
            Utils.Log("Loading Assemblies");
            Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly Assembly in Assemblies)
            {
                try
                {
                    if (Assembly.GetName().Name == "Assembly-CSharp")
                    {
                        Utils.Log("Assembly-CSharp found.");
                        this.AssemblyCSharp = Assembly;
                        break;
                    }
                }
                catch { }
            }
            if (AssemblyCSharp == null)
            {
                Utils.Log("Assembly-CSharp not found!");
            }
        }

        public void OnDestroy()
        {
            Utils.Log("OnDestroy!");
            this.ClientStatusMemoryMap = null;
            this.ClientCommandsMemoryMap = null;
            this.applicationController = null;
            this.inputController = null;
            this.player = null;
            this.FindItemResults = null;
            this.FindPermanentResults = null;
            this.FindPanelResults = null;
            this.FindButtonResults = null;
            this.FindInputResults = null;
            this.FindLabelResults = null;
            this.FindMobileResults = null;
            this.CustomVars = null;
            this.lastMouseClickClientObject = null;
        }

        // For backward compatibility with old command implementations - params are always threated as string
        // Will get rid of this once I've updated all the commands to be strong typed
        private string ExtractParam(Dictionary<String, ClientCommand.CommandParamStruct> Params, int Index)
        {
            if (Index >= 0 && Index <= (Params.Count - 1))
            {
                ClientCommand.CommandParamStruct CommandParam = Params.Values.ElementAt(Index);

                switch (CommandParam.CommandParamType)
                {
                    case ClientCommand.CommandParamTypeEnum.Boolean:
                        return CommandParam.Boolean.ToString();
                    case ClientCommand.CommandParamTypeEnum.Number:
                        return CommandParam.Number.ToString();
                    case ClientCommand.CommandParamTypeEnum.String:
                        return CommandParam.String;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }
        private string ExtractStringParam(Dictionary<String, ClientCommand.CommandParamStruct> Params, int Index)
        {
            if (Index >= 0 && Index <= (Params.Count - 1))
            {
                ClientCommand.CommandParamStruct CommandParam = Params.Values.ElementAt(Index);

                switch (CommandParam.CommandParamType)
                {
                    case ClientCommand.CommandParamTypeEnum.Boolean:
                        return CommandParam.Boolean.ToString();
                    case ClientCommand.CommandParamTypeEnum.Number:
                        return CommandParam.Number.ToString();
                    case ClientCommand.CommandParamTypeEnum.String:
                        return CommandParam.String.ToString();
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }
        private object ExtractObjectParam(Dictionary<String, ClientCommand.CommandParamStruct> Params, int Index)
        {
            if (Index >= 0 && Index <= (Params.Count - 1))
            {
                ClientCommand.CommandParamStruct CommandParam = Params.Values.ElementAt(Index);

                switch (CommandParam.CommandParamType)
                {
                    case ClientCommand.CommandParamTypeEnum.Boolean:
                        return CommandParam.Boolean;
                    case ClientCommand.CommandParamTypeEnum.Number:
                        return CommandParam.Number;
                    case ClientCommand.CommandParamTypeEnum.String:
                        return CommandParam.String;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        private void ProcessClientCommand(ClientCommand ClientCommand)
        {
            if (ClientCommand != null && ClientCommand.TimeStamp != LastClientCommandTimestamp && ClientCommand.CommandType != CommandType.NOP)
            {
                LastClientCommandTimestamp = ClientCommand.TimeStamp;
                Utils.Log("New command " + ClientCommand.CommandType.ToString() + " received at " + LastClientCommandTimestamp.ToString() + "! Params:");
                Utils.Log(string.Join(" ", ClientCommand.CommandParams));
                switch (ClientCommand.CommandType)
                {
                    case CommandType.FindItem:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            Dictionary<string, DynamicObject> items = new Dictionary<string, DynamicObject>();

                            // Try by ObjectId
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_objectId != null && ulong.TryParse(_objectId, out ulong objectId))
                            {
                                DynamicObject dynamicObject = Utils.FindDynamicObject(objectId);
                                if (dynamicObject != null)
                                {
                                    items[objectId.ToString()] = dynamicObject;
                                }
                            } else
                            {
                                // Try by Name (and ContainerId if required)
                                string objectName = ExtractParam(ClientCommand.CommandParams, 0);
                                string _containerId = ExtractParam(ClientCommand.CommandParams, 1);
                                ulong containerId;

                                if (_containerId != null && _containerId != "" && ulong.TryParse(_containerId, out containerId))
                                {
                                    items = Utils.FindDynamicObjectsByName(objectName, containerId);
                                }
                                else
                                {
                                    items = Utils.FindDynamicObjectsByName(objectName);
                                }
                            }

                            try {
                                this.FindItemResults =
                                    items?.Select(f => new ClientStatus.FINDITEMStruct()
                                    {
                                        CNTID = f.Value.ContainerId,
                                        DISTANCE = f.Value.ContainerId == 0 ? Vector3.Distance(f.Value.transform.position, this.player.transform.position) : 0,
                                        ID = f.Value.ObjectId,
                                        NAME = f.Value.EBHEDGHBHGI,
                                        X = f.Value.transform?.position.x,
                                        Y = f.Value.transform?.position.y,
                                        Z = f.Value.transform?.position.z,
                                    })
                                    .OrderBy(f => f.DISTANCE)
                                    .ToArray();
                            }
                            catch (Exception ex)
                            {
                                this.FindItemResults = null;
                                Utils.Log("Error building FindItemResults!");
                                Utils.Log(ex.ToString());
                            }

                            watch.Stop();
                            Utils.Log("FindItem took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.FindPermanent:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            Dictionary<string, ClientObject> permanentObjects = new Dictionary<string, ClientObject>();

                            // Try by PermanentId
                            string _permanentId = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_permanentId != null && int.TryParse(_permanentId, out int permanentId))
                            {
                                ClientObject clientObject = Utils.FindPermanentObject(permanentId);
                                if (clientObject != null)
                                {
                                    permanentObjects.Add(permanentId.ToString(), clientObject);
                                }
                            }
                            else
                            {
                                // Try by Name and distance (if required)
                                string permanentName = ExtractParam(ClientCommand.CommandParams, 0);
                                string _distance = ExtractParam(ClientCommand.CommandParams, 1);
                                if (_distance != null && _distance != "" && float.TryParse(_distance, out float distance))
                                {
                                    permanentObjects = Utils.FindPermanentObjectByName(permanentName, distance);
                                }
                                else
                                {
                                    permanentObjects = Utils.FindPermanentObjectByName(permanentName);
                                }
                            }

                            try {
                                this.FindPermanentResults = permanentObjects?.Select(f => new ClientStatus.FINDPERMANENTStruct()
                                {
                                    COLOR =
                                           f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.HasProperty("_Color") && m.color != null)?.Select(m => ColorUtility.ToHtmlStringRGBA(m.color)) != null
                                           ?
                                           String.Join(",", f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.HasProperty("_Color") && m.color != null)?.Select(m => ColorUtility.ToHtmlStringRGBA(m.color)))
                                           :
                                           null,
                                    DISTANCE = Vector3.Distance(f.Value.transform.position, this.player.transform.position),
                                    HUE =
                                           f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.HasProperty("_Hue"))?.Select(m => m.GetInt("_Hue").ToString()) != null
                                           ?
                                           String.Join(",", f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.HasProperty("_Hue"))?.Select(m => m.GetInt("_Hue").ToString()))
                                           :
                                           null,
                                    ID = f.Value?.PermanentId,
                                    NAME = f.Value?.name,
                                    STONESTATE = (int?)Utils.GetInstanceField(f.Value?.GetComponent<StoneStateHandler>(), "IKKDABEEPAF"),
                                    TEXTURE =
                                           f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.mainTexture != null).Select(m => m.mainTexture.name) != null
                                           ?
                                           String.Join(",", f.Value?.GetComponentInChildren<Renderer>()?.materials?.Where(m => m != null && m.mainTexture != null).Select(m => m.mainTexture.name))
                                           :
                                           null,
                                    TREESTATE = (int?)Utils.GetInstanceField(f.Value?.GetComponent<TreeStateHandler>(), "IKKDABEEPAF"),
                                    X = f.Value?.transform?.position.x,
                                    Y = f.Value?.transform?.position.y,
                                    Z = f.Value?.transform?.position.z,
                                })
                                .OrderBy(f => f.DISTANCE)
                                .ToArray();
                            } catch (Exception ex)
                            {
                                this.FindPermanentResults = null;
                                Utils.Log("Error building FindPermanentResults!");
                                Utils.Log(ex.ToString());
                            }

                            watch.Stop();
                            Utils.Log("FindPermanent took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.FindPanel:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            string _panelName = ExtractParam(ClientCommand.CommandParams, 0);

                            Dictionary<string, FloatingPanel> panels = Utils.FindPanelByName(_panelName);

                            try {
                                this.FindPanelResults =
                                    panels?.Select(f => new ClientStatus.FINDPANELStruct()
                                    {
                                        ID = f.Key
                                    })
                                    .ToArray();
                            } catch (Exception ex)
                            {
                                this.FindPanelResults = null;
                                Utils.Log("Error building FindPanelResults!");
                                Utils.Log(ex.ToString());
                            }

                            watch.Stop();
                            Utils.Log("FindPanel took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.ClosePanel:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            string _panelnName = ExtractParam(ClientCommand.CommandParams, 0);

                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel FloatingPanel = fpm.GetPanel(_panelnName);
                                if (FloatingPanel != null)
                                {
                                    FloatingPanel.CloseWindow();
                                }
                            }

                            break;
                        }

                    case CommandType.FindButton:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            List<ClientStatus.FINDBUTTONStruct> buttons = new List<ClientStatus.FINDBUTTONStruct>();

                            string _containerName = ExtractParam(ClientCommand.CommandParams, 0);

                            string _buttonName = ExtractParam(ClientCommand.CommandParams, 1);
                            string _x = ExtractParam(ClientCommand.CommandParams, 1);
                            string _y = ExtractParam(ClientCommand.CommandParams, 2);

                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel floatingPanel = fpm.GetPanel(_containerName);
                                if (floatingPanel != null)
                                {
                                    DynamicWindow dynamicWindow = floatingPanel.GetComponent<DynamicWindow>();
                                    if (dynamicWindow != null)
                                    {
                                        if (int.TryParse(_x, out int x) && int.TryParse(_y, out int y))
                                        {
                                            //////// Coordinates Search: find collider by coordinate

                                            BoxCollider[] Colliders = dynamicWindow.GetComponentsInChildren<BoxCollider>();

                                            foreach (BoxCollider Collider in Colliders)
                                            {
                                                float ColliderX1 = Collider.transform.localPosition.x;
                                                float ColliderX2 = Collider.transform.localPosition.x + Collider.size.x;
                                                float ColliderY1 = Collider.transform.localPosition.y;
                                                float ColliderY2 = Collider.transform.localPosition.y - Collider.size.y;
                                                if (ColliderX1 <= x && x <= ColliderX2 &&
                                                    ColliderY2 <= y && y <= ColliderY1)
                                                {
                                                    if (int.TryParse(Collider.name, out int ButtonID))
                                                    {
                                                        Utils.Log("Collider " + Collider.name + " found by coordinates!");
                                                        buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                        {
                                                            NAME = ButtonID.ToString(),
                                                            TEXT = x.ToString() + "-" + y.ToString()
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //////// Textual Search 1: find label containing text, and corresponding collider

                                            UILabel[] Labels = dynamicWindow.GetComponentsInChildren<UILabel>();
                                            BoxCollider[] Colliders = dynamicWindow.GetComponentsInChildren<BoxCollider>();

                                            // Let's search for a label containing the given text
                                            foreach (UILabel Label in Labels)
                                            {
                                                if (string.IsNullOrEmpty(_buttonName) || Label.FJNGNLHHOEI.ToLower().Contains(_buttonName.ToLower()))
                                                {
                                                    Utils.Log("Label " + Label.FJNGNLHHOEI + " found! Searching for collider...");

                                                    // Calculate the label's center, relative to the dynamic window
                                                    Vector3 LabelPositionRelativeToDynamicWindow = Utils.CalculateRelativePosition(Label.transform, dynamicWindow.transform);
                                                    Vector3 LabelCenterRelativeToDynamicWindow = LabelPositionRelativeToDynamicWindow + Label.IOGCKBNELGA; // Label.IOGCKBNELGA is the UIWidget's localCenter property documented here http://tasharen.com/ngui/docs/class_u_i_widget.html

                                                    // And search for a collider that is colliding with the label's center
                                                    foreach (BoxCollider Collider in Colliders)
                                                    {
                                                        // Calculate the collider's center, relative to the dynamic window
                                                        Vector3 ColliderPositionRelativeToDynamicWindow = Utils.CalculateRelativePosition(Collider.transform, dynamicWindow.transform);
                                                        Vector3 ColliderCenterRelativeToDynamicWindow = ColliderPositionRelativeToDynamicWindow + Collider.center;
                                                        
                                                        // Calculate collider boundaries
                                                        float ColliderX1 = ColliderCenterRelativeToDynamicWindow.x - (Collider.size.x / 2);
                                                        float ColliderX2 = ColliderCenterRelativeToDynamicWindow.x + (Collider.size.x / 2);
                                                        float ColliderY1 = ColliderCenterRelativeToDynamicWindow.y - (Collider.size.y / 2);
                                                        float ColliderY2 = ColliderCenterRelativeToDynamicWindow.y + (Collider.size.y / 2);

                                                        // Check if the label's center falls within this collider's boundaries
                                                        if (ColliderX1 <= LabelCenterRelativeToDynamicWindow.x && LabelCenterRelativeToDynamicWindow.x <= ColliderX2 &&
                                                            ColliderY1 <= LabelCenterRelativeToDynamicWindow.y && LabelCenterRelativeToDynamicWindow.y <= ColliderY2)
                                                        {
                                                            if (int.TryParse(Collider.name, out int ButtonID))
                                                            {
                                                                Utils.Log("Collider " + Collider.name + " found!");
                                                                buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                                {
                                                                    NAME = ButtonID.ToString(),
                                                                    TEXT = Label.FJNGNLHHOEI
                                                                });
                                                            }
                                                        }
                                                    }
                                                }
                                            }

                                            //////// Textual Search 2: find action containing text, and corresponding action id

                                            // KAAFKBBECEF is an internal type, we need to use reflection
                                            Type KAAFKBBECEF = AssemblyCSharp.GetType("KAAFKBBECEF");

                                            object HGBANEEHBLH = Utils.GetInstanceField(dynamicWindow, "HGBANEEHBLH");

                                            int i = 0;
                                            foreach (object o in (HGBANEEHBLH as IEnumerable))
                                            {
                                                object casted = Convert.ChangeType(o, KAAFKBBECEF);

                                                string KFBLLAJBKAD = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "KFBLLAJBKAD");
                                                if (KFBLLAJBKAD != null && (string.IsNullOrEmpty(_buttonName) || KFBLLAJBKAD.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by KFBLLAJBKAD!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = KFBLLAJBKAD
                                                    });
                                                }

                                                string KFFEBNDBIPA = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "KFFEBNDBIPA");
                                                if (KFFEBNDBIPA != null && (string.IsNullOrEmpty(_buttonName) || KFFEBNDBIPA.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by KFFEBNDBIPA!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = KFFEBNDBIPA
                                                    });
                                                }

                                                // Seems to be ToolTip Text
                                                string ELGLAFGJGAO = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "ELGLAFGJGAO");
                                                if (ELGLAFGJGAO != null && (string.IsNullOrEmpty(_buttonName) || ELGLAFGJGAO.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by ELGLAFGJGAO!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = ELGLAFGJGAO
                                                    });
                                                }

                                                string PDENMACFHFK = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "PDENMACFHFK");
                                                if (PDENMACFHFK != null && (string.IsNullOrEmpty(_buttonName) || PDENMACFHFK.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by PDENMACFHFK!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = PDENMACFHFK
                                                    });
                                                }

                                                string OEFOJOODPBK = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "OEFOJOODPBK");
                                                if (OEFOJOODPBK != null && (string.IsNullOrEmpty(_buttonName) || OEFOJOODPBK.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by OEFOJOODPBK!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = OEFOJOODPBK
                                                    });
                                                }

                                                string JCIPDLPHPFB = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "JCIPDLPHPFB");
                                                if (JCIPDLPHPFB != null && (string.IsNullOrEmpty(_buttonName) || ELGLAFGJGAO.ToLower().Contains(_buttonName.ToLower())))
                                                {
                                                    Utils.Log("Found DynamicWindow Action by JCIPDLPHPFB!");
                                                    buttons.Add(new ClientStatus.FINDBUTTONStruct()
                                                    {
                                                        NAME = i.ToString(),
                                                        TEXT = JCIPDLPHPFB
                                                    });
                                                }

                                                i++;
                                            }
                                        }
                                    }
                                }
                            }

                            this.FindButtonResults = buttons.ToArray();

                            watch.Stop();
                            Utils.Log("FindButton took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.FindInput:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            List<ClientStatus.FINDINPUTStruct> inputs = new List<ClientStatus.FINDINPUTStruct>();

                            string _containerName = ExtractParam(ClientCommand.CommandParams, 0);
                            string _inputName = ExtractParam(ClientCommand.CommandParams, 1);

                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel floatingPanel = fpm.GetPanel(_containerName);
                                if (floatingPanel != null)
                                {
                                    DynamicWindow dynamicWindow = floatingPanel.GetComponent<DynamicWindow>();
                                    if (dynamicWindow != null)
                                    {
                                        DynamicWindowTextField[] Inputs = dynamicWindow.GetComponentsInChildren<DynamicWindowTextField>();

                                        // Let's search for an input containing the given text
                                        foreach (DynamicWindowTextField Input in Inputs)
                                        {
                                            if (string.IsNullOrEmpty(_inputName) || Input.gameObject.name.ToLower().Contains(_inputName.ToLower()))
                                            {
                                                Utils.Log("DynamicWindowTextField " + Input.gameObject.name + " found!");
                                                inputs.Add(new ClientStatus.FINDINPUTStruct()
                                                {
                                                    ID = Input.gameObject?.name
                                                });
                                            }
                                        }
                                    }
                                }
                            }

                            this.FindInputResults = inputs.ToArray();

                            watch.Stop();
                            Utils.Log("FindInput took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.FindLabel:
                        {
                            var watch = new System.Diagnostics.Stopwatch();
                            watch.Start();

                            List<KeyValuePair<int, string>> labels = new List<KeyValuePair<int, string>>();

                            string _containerName = ExtractParam(ClientCommand.CommandParams, 0);

                            string _labelText = ExtractParam(ClientCommand.CommandParams, 1);
                            string _labelName = ExtractParam(ClientCommand.CommandParams, 1);
                            string _x = ExtractParam(ClientCommand.CommandParams, 1);
                            string _y = ExtractParam(ClientCommand.CommandParams, 2);

                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel floatingPanel = fpm.GetPanel(_containerName);
                                if (floatingPanel != null)
                                {
                                    UILabel[] Labels = floatingPanel.GetComponentsInChildren<UILabel>();
                                    BoxCollider[] Colliders = floatingPanel.GetComponentsInChildren<BoxCollider>();

                                    if (int.TryParse(_x, out int x) && int.TryParse(_y, out int y))
                                    {
                                        //////// Coordinates Search: find collider by coordinate

                                        foreach (BoxCollider Collider in Colliders)
                                        {
                                            float ColliderX1 = Collider.transform.localPosition.x;
                                            float ColliderX2 = Collider.transform.localPosition.x + Collider.size.x;
                                            float ColliderY1 = Collider.transform.localPosition.y;
                                            float ColliderY2 = Collider.transform.localPosition.y - Collider.size.y;
                                            if (ColliderX1 <= x && x <= ColliderX2 &&
                                                ColliderY2 <= y && y <= ColliderY1)
                                            {
                                                if (int.TryParse(Collider.name, out int ButtonID))
                                                {
                                                    Utils.Log("Collider " + Collider.name + " found by coordinates! Searching all labels within this collider...");

                                                    String FullLabelText = "";

                                                    // Now search ALL labels within this collider, so that we have the complete text
                                                    foreach (UILabel PartialLabel in Labels)
                                                    {
                                                        // Calculate the label's center
                                                        float PartialLabelCenterX = PartialLabel.transform.localPosition.x + PartialLabel.IOGCKBNELGA.x;
                                                        float PartialLabelCenterY = PartialLabel.transform.localPosition.y + PartialLabel.IOGCKBNELGA.y;

                                                        if (ColliderX1 <= PartialLabelCenterX && PartialLabelCenterX <= ColliderX2 &&
                                                            ColliderY2 <= PartialLabelCenterY && PartialLabelCenterY <= ColliderY1)
                                                        {
                                                            Utils.Log("Found partial label:" + PartialLabel.FJNGNLHHOEI);
                                                            if (!FullLabelText.Contains(PartialLabel.FJNGNLHHOEI))
                                                            {
                                                                FullLabelText += PartialLabel.FJNGNLHHOEI;
                                                            }
                                                        }
                                                    }

                                                    labels.Add(new KeyValuePair<int, string>(ButtonID, FullLabelText));
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (int.TryParse(_labelName, out int labelName)) {
                                        //////// Textual Search 1: find collider with given name, and corresponding labels

                                        // Let's search for a collider with the given name
                                        foreach (BoxCollider Collider in Colliders)
                                        {
                                            float ColliderX1 = Collider.transform.localPosition.x;
                                            float ColliderX2 = Collider.transform.localPosition.x + Collider.size.x;
                                            float ColliderY1 = Collider.transform.localPosition.y;
                                            float ColliderY2 = Collider.transform.localPosition.y - Collider.size.y;

                                            if (int.TryParse(Collider.name, out int ButtonID) && ButtonID == labelName)
                                            {
                                                Utils.Log("Collider " + Collider.name + " found! Searching all labels within this collider...");

                                                String FullLabelText = "";

                                                // Now search ALL labels within this collider, so that we have the complete text
                                                foreach (UILabel PartialLabel in Labels)
                                                {
                                                    // Calculate the label's center
                                                    float PartialLabelCenterX = PartialLabel.transform.localPosition.x + PartialLabel.IOGCKBNELGA.x;
                                                    float PartialLabelCenterY = PartialLabel.transform.localPosition.y + PartialLabel.IOGCKBNELGA.y;

                                                    if (ColliderX1 <= PartialLabelCenterX && PartialLabelCenterX <= ColliderX2 &&
                                                        ColliderY2 <= PartialLabelCenterY && PartialLabelCenterY <= ColliderY1)
                                                    {
                                                        Utils.Log("Found partial label:" + PartialLabel.FJNGNLHHOEI);
                                                        if (!FullLabelText.Contains(PartialLabel.FJNGNLHHOEI))
                                                        {
                                                            FullLabelText += PartialLabel.FJNGNLHHOEI;
                                                        }
                                                    }
                                                }

                                                labels.Add(new KeyValuePair<int, string>(ButtonID, FullLabelText));
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //////// Textual Search 2: find label containing text, and corresponding collider

                                        // Let's search for a label containing the given text
                                        foreach (UILabel Label in Labels)
                                        {
                                            if (string.IsNullOrEmpty(_labelText) || Label.FJNGNLHHOEI.ToLower().Contains(_labelText.ToLower()))
                                            {
                                                Utils.Log("Label " + Label.FJNGNLHHOEI + " found! Searching for collider...");

                                                // Calculate the label's center
                                                float LabelCenterX = Label.transform.localPosition.x + Label.IOGCKBNELGA.x;
                                                float LabelCenterY = Label.transform.localPosition.y + Label.IOGCKBNELGA.y;

                                                // And search for a collider that is colliding with the label's center
                                                foreach (BoxCollider Collider in Colliders)
                                                {
                                                    float ColliderX1 = Collider.transform.localPosition.x;
                                                    float ColliderX2 = Collider.transform.localPosition.x + Collider.size.x;
                                                    float ColliderY1 = Collider.transform.localPosition.y;
                                                    float ColliderY2 = Collider.transform.localPosition.y - Collider.size.y;
                                                    if (ColliderX1 <= LabelCenterX && LabelCenterX <= ColliderX2 &&
                                                        ColliderY2 <= LabelCenterY && LabelCenterY <= ColliderY1)
                                                    {
                                                        if (int.TryParse(Collider.name, out int ButtonID))
                                                        {
                                                            Utils.Log("Collider " + Collider.name + " found! Searching all labels within this collider...");

                                                            String FullLabelText = "";

                                                            // Now search ALL labels within this collider, so that we have the complete text
                                                            foreach (UILabel PartialLabel in Labels)
                                                            {
                                                                // Calculate the label's center
                                                                float PartialLabelCenterX = PartialLabel.transform.localPosition.x + PartialLabel.IOGCKBNELGA.x;
                                                                float PartialLabelCenterY = PartialLabel.transform.localPosition.y + PartialLabel.IOGCKBNELGA.y;

                                                                if (ColliderX1 <= PartialLabelCenterX && PartialLabelCenterX <= ColliderX2 &&
                                                                    ColliderY2 <= PartialLabelCenterY && PartialLabelCenterY <= ColliderY1)
                                                                {
                                                                    Utils.Log("Found partial label:" + PartialLabel.FJNGNLHHOEI);
                                                                    if (!FullLabelText.Contains(PartialLabel.FJNGNLHHOEI))
                                                                    {
                                                                        FullLabelText += PartialLabel.FJNGNLHHOEI;
                                                                    }
                                                                }
                                                            }

                                                            labels.Add(new KeyValuePair<int, string>(ButtonID, FullLabelText));
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            try {
                                this.FindLabelResults = 
                                    labels.Select(f => new ClientStatus.FINDLABELStruct()
                                    {
                                        NAME = f.Key.ToString(),
                                        TEXT = f.Value
                                    })
                                    .ToArray();
                            }
                            catch (Exception ex)
                            {
                                this.FindLabelResults = null;
                                Utils.Log("Error building FindLabelResults!");
                                Utils.Log(ex.ToString());
                            }

                            watch.Stop();
                            Utils.Log("FindLabel took " + watch.ElapsedMilliseconds.ToString() + "ms");
                            break;
                        }

                    case CommandType.Key:
                        {
                            string keyCode = ExtractParam(ClientCommand.CommandParams, 0);

                            foreach (InputController.JFNFCCGHCLJ KeyMapping in this.inputController.KeyMappings)
                            {
                                //new InputController.JFNFCCGHCLJ
                                //{
                                //    PCFPPFOLBEM = KeyCode.Space,
                                //    JEJHPLPCHNB = new PFJDGPGOOBP(),
                                //    MKNOODPFNEL = "ToggleCombatModeAction"
                                //}
                                if (KeyMapping.PCFPPFOLBEM == (KeyCode)Enum.Parse(typeof(KeyCode), keyCode))
                                {
                                    KeyMapping.JEJHPLPCHNB.Execute();
                                }
                            }
                            foreach (InputController.JFNFCCGHCLJ KeyMapping in this.inputController.GodKeyMappings)
                            {
                                if (KeyMapping.PCFPPFOLBEM == (KeyCode)Enum.Parse(typeof(KeyCode), keyCode))
                                {
                                    KeyMapping.JEJHPLPCHNB.Execute();
                                }
                            }

                        }
                        break;

                    case CommandType.Move:
                        {
                            string _x = ExtractParam(ClientCommand.CommandParams, 0);
                            float x;
                            string _y = ExtractParam(ClientCommand.CommandParams, 1);
                            float y;
                            string _z = ExtractParam(ClientCommand.CommandParams, 2);
                            float z;
                            if (float.TryParse(_x, out x) && float.TryParse(_y, out y) && float.TryParse(_z, out z))
                            {
                                Utils.Log("Moving to x=" + x + " y=" + y + " z=" + z);
                                Vector3 location = new Vector3(x, y, z);
                                this.player.SetPathLocation(location, false);
                                return;
                            }
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            if (ulong.TryParse(_objectId, out ulong objectId))
                            {
                                ClientObject clientObject = Utils.FindClientObject(objectId);
                                if (clientObject != null)
                                {
                                    Utils.Log("Moving to objectId=" + objectId.ToString());
                                    this.player.SetPathObject(clientObject, LocalPlayer.FHAIDCMBMHC.None);
                                    return;
                                }
                                clientObject = Utils.FindPermanentObject((int)objectId);
                                if (clientObject != null)
                                {
                                    Utils.Log("Moving to permanentId=" + objectId.ToString());
                                    this.player.SetPathObject(clientObject, LocalPlayer.FHAIDCMBMHC.None);
                                    break;
                                }
                                return;
                            }
                        }
                        break;

                    case CommandType.Stop:
                        {
                            this.player.StopPathing(0);
                        }
                        break;

                    case CommandType.ScanJournal:
                        {
                            if (this.applicationController?.GameUI?.ChatWindow != null)
                            {
                                string _timeStamp = ExtractParam(ClientCommand.CommandParams, 0);
                                if (_timeStamp != null && _timeStamp != "" && float.TryParse(_timeStamp, out float timeStamp))
                                {
                                    // Obfuscation guessed from GameUI.ChatWindow.SystemMessage()
                                    List<ChatWindow.AEDJOHFMLDG> MEMFCHFEKPN = (List<ChatWindow.AEDJOHFMLDG>)Utils.GetInstanceField(this.applicationController.GameUI.ChatWindow, "MEMFCHFEKPN");
                                    if (MEMFCHFEKPN != null)
                                    {
                                        ChatWindow.AEDJOHFMLDG message = MEMFCHFEKPN.FindLast(m => m.KCFOFGNMOBE >= timeStamp + 0.001f);
                                        if (message != null)
                                        {
                                            this.ScanJournalMessage = message.PIIMGFNGPEI;
                                            this.ScanJournalTime = message.KCFOFGNMOBE;
                                        }
                                        else
                                        {
                                            this.ScanJournalMessage = "N/A";
                                            this.ScanJournalTime = 0;
                                        }
                                    }
                                }
                            }
                        }
                        break;

                    case CommandType.Macro:
                        {
                            string _macro = ExtractParam(ClientCommand.CommandParams, 0);
                            int macro;

                            if (_macro != null && _macro != "" && int.TryParse(_macro, out macro))
                            {
                                KLKFNLBFIAK Macro = new KLKFNLBFIAK(macro);
                                Macro.Execute();
                            }
                        }
                        break;

                    case CommandType.Say:
                        {
                            string _text = ExtractParam(ClientCommand.CommandParams, 0);
                            string text = "/say " + _text;
                            this.applicationController.GPLIHPHPNKL.SendChat(text);
                        }
                        break;

                    case CommandType.SayCustom:
                        {
                            string _text = ExtractParam(ClientCommand.CommandParams, 0);
                            string text = _text;
                            this.applicationController.GPLIHPHPNKL.SendChat(text);
                        }
                        break;

                    case CommandType.ToggleWarPeace:
                        {
                            PFJDGPGOOBP Toggle = new PFJDGPGOOBP();
                            Toggle.Execute();
                        }
                        break;

                    case CommandType.TargetDynamic:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            ulong objectId;

                            if (_objectId != null && _objectId != "" && ulong.TryParse(_objectId, out objectId))
                            {
                                ClientObject clientObject = Utils.FindClientObject(objectId);
                                if (clientObject != null)
                                {
                                    this.inputController.HandleTargetResponse(clientObject);
                                }
                            }
                        }
                        break;

                    case CommandType.TargetPermanent:
                        {
                            string _permanentId = ExtractParam(ClientCommand.CommandParams, 0);
                            int permanentId;

                            if (_permanentId != null && _permanentId != "" && int.TryParse(_permanentId, out permanentId))
                            {
                                ClientObject clientObject = Utils.FindPermanentObject(permanentId);
                                if (clientObject != null)
                                {
                                    this.inputController.HandleTargetResponse(clientObject);
                                }
                            }
                        }
                        break;

                    case CommandType.TargetLoc:
                        {
                            string _x = ExtractParam(ClientCommand.CommandParams, 0);
                            string _y = ExtractParam(ClientCommand.CommandParams, 1);
                            string _z = ExtractParam(ClientCommand.CommandParams, 2);
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 3);

                            if (float.TryParse(_x, out float x) &&
                                float.TryParse(_y, out float y) &&
                                float.TryParse(_z, out float z))
                            {
                                Vector3 loc = new Vector3(x, y, z);
                                if (ulong.TryParse(_objectId, out ulong objectId))
                                {
                                    ClientObject clientObject = Utils.FindClientObject(objectId);
                                    if (clientObject != null)
                                    {
                                        this.inputController.HandleTargetLocResponse(loc, clientObject);
                                    }
                                    else
                                    {
                                        this.inputController.HandleTargetLocResponse(loc, null);
                                    }
                                }
                                else
                                {
                                    this.inputController.HandleTargetLocResponse(loc, null);
                                }

                            }
                        }
                        break;

                    case CommandType.LastTarget:
                        {
                            this.inputController.TargetLast();
                        }
                        break;

                    case CommandType.TargetSelf:
                        {
                            this.inputController.TargetSelf();
                        }
                        break;

                    case CommandType.WaitForTarget:
                        {
                            // This is implemented client side! See ScriptDebugger.cs in EasyLOU project
                        }
                        break;

                    case CommandType.AttackSelected:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            ulong objectId;
                            if (_objectId != null && _objectId != "" && ulong.TryParse(_objectId, out objectId))
                            {
                                GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.GPLIHPHPNKL.SendScriptCommand(string.Concat(new object[]
                                {
                                    "use ",
                                    objectId,
                                    " ",
                                    "Attack"
                                }), 0UL);
                            }
                        }
                        break;

                    case CommandType.UseSelected:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            ulong objectId;
                            if (_objectId != null && _objectId != "" && ulong.TryParse(_objectId, out objectId))
                            {
                                DynamicObject dynamicObject = Utils.FindDynamicObject(objectId, 0);
                                if (dynamicObject != null)
                                {
                                    dynamicObject.DoDoubleClickAction();
                                }
                            }
                        }
                        break;

                    case CommandType.ContextMenu:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            string _command = ExtractParam(ClientCommand.CommandParams, 1);

                            string FullCommand = String.Format(".x use {0} {1}",
                                    _objectId,
                                    _command);

                            this.applicationController.GPLIHPHPNKL.SendChat(FullCommand);
                        }
                        break;

                    case CommandType.Drag:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            string _containerId = ExtractParam(ClientCommand.CommandParams, 1);
                            ulong objectId;
                            ulong containerId;
                            if (_objectId != null && _objectId != "" && ulong.TryParse(_objectId, out objectId))
                            {
                                ClientObject clientObject = null;
                                if (_containerId != null && _containerId != "" && ulong.TryParse(_containerId, out containerId))
                                {
                                    clientObject = Utils.FindClientObject(objectId, containerId);
                                }
                                else
                                {
                                    clientObject = Utils.FindClientObject(objectId);
                                }
                                if (clientObject != null)
                                {
                                    DynamicObject dynamicObject = clientObject.DynamicInst;
                                    if (dynamicObject != null)
                                    {
                                        player.DoPickup(dynamicObject);
                                    }
                                }
                            }
                        }
                        break;

                    case CommandType.Dropc:
                        {
                            string container = ExtractParam(ClientCommand.CommandParams, 0);

                            DynamicObject carriedObject = ClientObjectManager.DJCGIMIDOPB.GetCarriedObject();
                            if (carriedObject != null)
                            {
                                this.applicationController.GPLIHPHPNKL.SendRequestDrop(carriedObject.ObjectId, null, ulong.Parse(container), false);
                            }
                        }
                        break;

                    case CommandType.Dropg:
                        {
                            float x = float.Parse(ExtractParam(ClientCommand.CommandParams, 0));
                            float y = float.Parse(ExtractParam(ClientCommand.CommandParams, 1));
                            float z = float.Parse(ExtractParam(ClientCommand.CommandParams, 2));

                            DynamicObject carriedObject = ClientObjectManager.DJCGIMIDOPB.GetCarriedObject();
                            if (carriedObject != null)
                            {
                                this.applicationController.GPLIHPHPNKL.SendRequestDrop(carriedObject.ObjectId, new Vector3(x, y, z), 0UL, false);
                            }
                        }
                        break;

                    case CommandType.FindMobile:
                        {
                            List<MobileInstance> mobiles = new List<MobileInstance>();

                            // Try by ObjectId
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_objectId != null && ulong.TryParse(_objectId, out ulong objectId))
                            {
                                Utils.Log("Trying by object id");
                                MobileInstance mobile = Utils.GetMobile(objectId);
                                if (mobile != null)
                                {
                                    mobiles.Add(mobile);
                                }
                            } else
                            {
                                // Try by Name and distance (if required)
                                string name = ExtractParam(ClientCommand.CommandParams, 0);
                                string _distance = ExtractParam(ClientCommand.CommandParams, 1);
                                if (_distance != null && _distance != "" && float.TryParse(_distance, out float distance))
                                {
                                    mobiles = Utils.FindMobile(name, distance);
                                }
                                else
                                {
                                    mobiles = Utils.FindMobile(name);
                                }
                            }

                            try {
                                this.FindMobileResults =
                                    mobiles?.Select(f => new ClientStatus.FINDMOBILEStruct()
                                    {

                                        DISTANCE = Vector3.Distance(f.transform.position, this.player.transform.position),
                                        HP = f.GetStatByName("Health"),
                                        ID = f.ObjectId,
                                        NAME = f.EBHEDGHBHGI,
                                        TYPE = f.DKCMJFOPPDL,
                                        X = f.transform?.position.x,
                                        Y = f.transform?.position.y,
                                        Z = f.transform?.position.z
                                    })
                                    .OrderBy(f => f.DISTANCE)
                                    .ToArray();
                            }
                            catch (Exception ex)
                            {
                                this.FindMobileResults = null;
                                Utils.Log("Error building FindMobileResults!");
                                Utils.Log(ex.ToString());
                            }

                        break;
                        }

                    case CommandType.SetUsername:
                        {
                            LoginUI loginUI = UnityEngine.Object.FindObjectOfType<LoginUI>();
                            if (loginUI != null)
                            {
                                string _username = ExtractParam(ClientCommand.CommandParams, 0);
                                loginUI.UsernameTextField.KEJLIDGLCDP = _username;
                            }
                        }
                        break;

                    case CommandType.SetPassword:
                        {
                            LoginUI loginUI = UnityEngine.Object.FindObjectOfType<LoginUI>();
                            if (loginUI != null)
                            {
                                string _password = ExtractParam(ClientCommand.CommandParams, 0);
                                loginUI.PasswordTextField.KEJLIDGLCDP = _password;
                            }
                        }
                        break;

                    case CommandType.Login:
                        {
                            LoginUI loginUI = UnityEngine.Object.FindObjectOfType<LoginUI>();
                            if (loginUI != null)
                            {
                                loginUI.OnMultiplayerClicked();
                            }
                        }
                        break;

                    case CommandType.SelectServer:
                        {
                            LoginUI loginUI = UnityEngine.Object.FindObjectOfType<LoginUI>();
                            if (loginUI != null)
                            {
                                string _server = ExtractParam(ClientCommand.CommandParams, 0);
                                this.applicationController.SendServerSelected(_server);
                            }
                        }
                        break;

                    case CommandType.CharacterSelect:
                        {
                            LoginUI loginUI = UnityEngine.Object.FindObjectOfType<LoginUI>();
                            if (loginUI != null)
                            {
                                string _character = ExtractParam(ClientCommand.CommandParams, 0);
                                if (_character != null && _character != "" && int.TryParse(_character, out int character))
                                {
                                    if (character >= 0 && character <= loginUI.CharacterSelectButtons.Length - 1)
                                    {
                                        string name = loginUI.CharacterSelectButtons[character].transform.name;
                                        GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.GPLIHPHPNKL.SendCharacterSelected(int.Parse(name));
                                    }
                                }
                            }
                        }
                        break;

                    case CommandType.ClickButton:
                        {
                            string _containerName = ExtractParam(ClientCommand.CommandParams, 0);
                            string _buttonName = ExtractParam(ClientCommand.CommandParams, 1);
                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel floatingPanel = fpm.GetPanel(_containerName);
                                if (floatingPanel != null)
                                {
                                    DynamicWindow dynamicWindow = floatingPanel.GetComponent<DynamicWindow>();
                                    if (dynamicWindow != null)
                                    {
                                        GameObject objectFound = null;

                                        // Search within all the BoxCollider's of the DynamicWindow for a BoxCollider with the given name
                                        BoxCollider[] BoxColliders = dynamicWindow.GetComponentsInChildren<BoxCollider>();
                                        foreach (BoxCollider BoxCollider in BoxColliders)
                                        {
                                            if (BoxCollider.gameObject.name == _buttonName)
                                            {
                                                Utils.Log("Collider found!");
                                                objectFound = BoxCollider.gameObject;
                                            }
                                        }
                                        if (objectFound != null)
                                        {
                                            Utils.Log("OnButtonClicked(" + objectFound.name + ")");
                                            dynamicWindow.OnButtonClicked(objectFound);
                                            break;
                                        } else
                                        {
                                            Utils.Log("BoxCollider " + _buttonName + " not found!");
                                        }
                                    } else
                                    {
                                        Utils.Log("FloatingPanel " + _containerName + " found, but no dynamic window?!");
                                    }

                                } else
                                {
                                    Utils.Log("FloatingPanel " + _containerName + " not found!");
                                }
                            }
                        }
                        break;

                    case CommandType.SetInput:
                        {
                            string _containerName = ExtractParam(ClientCommand.CommandParams, 0);
                            string _inputName = ExtractParam(ClientCommand.CommandParams, 1);
                            string _newValue = ExtractParam(ClientCommand.CommandParams, 2);
                            FloatingPanelManager fpm = FloatingPanelManager.DJCGIMIDOPB;
                            if (fpm != null)
                            {
                                FloatingPanel floatingPanel = fpm.GetPanel(_containerName);
                                if (floatingPanel != null)
                                {
                                    DynamicWindow dynamicWindow = floatingPanel.GetComponent<DynamicWindow>();
                                    if (dynamicWindow != null)
                                    {
                                        GameObject objectFound = null;
                                        List<GameObject> Children = new List<GameObject>();
                                        for (var c = 0; c < dynamicWindow.transform.childCount; c++)
                                        {
                                            Transform child = dynamicWindow.transform.GetChild(c);
                                            Children.Add(child.gameObject);
                                            Utils.Log("Child gameobject name: " + child.gameObject.name);
                                            if (child.gameObject.name == _inputName)
                                            {
                                                Utils.Log("GameObject found!");
                                                objectFound = child.gameObject;
                                            }
                                            Component[] components = child.GetComponents<Component>();
                                            foreach (Component component in components)
                                            {
                                                Utils.Log("Child component: " + component.name + "," + component.GetType().ToString());
                                            }
                                        }
                                        if (objectFound != null)
                                        {
                                            Utils.Log("Set(" + _newValue + ")");
                                            DynamicWindowTextField input = objectFound.GetComponent<DynamicWindowTextField>();
                                            input.GOAGGCMCIBB = _newValue;
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                        break;

                    case CommandType.SetTargetFrameRate:
                        {
                            string _targetFrameRate = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_targetFrameRate != null && _targetFrameRate != "" && int.TryParse(_targetFrameRate, out int targetFrameRate))
                            {
                                Application.targetFrameRate = targetFrameRate;
                            }
                        }
                        break;

                    case CommandType.SetVSyncCount:
                        {
                            string _vSyncCount = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_vSyncCount != null && _vSyncCount != "" && int.TryParse(_vSyncCount, out int vSyncCount))
                            {
                                QualitySettings.vSyncCount = vSyncCount;
                            }
                        }
                        break;

                    case CommandType.SetMainCameraMask:
                        {
                            string _cullingMask = ExtractParam(ClientCommand.CommandParams, 0);
                            if (_cullingMask != null && _cullingMask != "" && int.TryParse(_cullingMask, out int cullingMask))
                            {
                                Camera.main.cullingMask = cullingMask;
                            }
                        }
                        break;

                    case CommandType.GetTooltip:
                        {
                            string _objectId = ExtractParam(ClientCommand.CommandParams, 0);
                            ulong objectId;
                            string ttText = "N/A";

                            if (_objectId != null && _objectId != "" && ulong.TryParse(_objectId, out objectId))
                            {
                                DynamicObject dynamicObject = Utils.FindDynamicObject(objectId);
                                if (dynamicObject != null)
                                {
                                    ttText = dynamicObject.GetTooltip();
                                    if (ttText != null)
                                    {
                                        ttText = ttText.Replace('\n', '|');
                                    }
                                }
                            }

                            this.tooltipText = ttText;
                        }
                        break;

                    case CommandType.ResetVars:
                        {
                            FindItemResults = null;
                            FindPermanentResults = null;
                            FindPanelResults = null;
                            FindButtonResults = null;
                            FindInputResults = null;
                            FindLabelResults = null;
                            FindMobileResults = null;

                            CustomVars = null;

                            ScanJournalTime = 0;
                            ScanJournalMessage = null;

                            leftMouseDown = false;
                            rightMouseDown = false;

                            lastMouseClickPosition = new Vector3();
                            lastMouseClickClientObject = null;
                            tooltipText = null;
                        }
                        break;

                    case CommandType.SetCustomVar:
                        {
                            string name = ExtractStringParam(ClientCommand.CommandParams, 0);
                            object value = ExtractObjectParam(ClientCommand.CommandParams, 1);

                            if (!String.IsNullOrEmpty(name)) {
                                name = name.ToUpper();

                                if (CustomVars == null)
                                    CustomVars = new Dictionary<string, object>();

                                if (CustomVars.ContainsKey(name))
                                {
                                    CustomVars[name] = value;
                                    Utils.Log("CustomVar " + name + " exists, setting it to " + value.ToString());
                                }
                                else
                                {
                                    CustomVars.Add(name, value);
                                    Utils.Log("CustomVar " + name + " does not exist, creating it with value " + value.ToString());
                                }
                            }
                        }
                        break;

                    case CommandType.ClearCustomVar:
                        {
                            string name = ExtractParam(ClientCommand.CommandParams, 0);

                            if (!String.IsNullOrEmpty(name))
                            {
                                name = name.ToUpper();

                                if (CustomVars != null)
                                {
                                    if (CustomVars.ContainsKey(name))
                                    {
                                        CustomVars.Remove(name);
                                        Utils.Log("CustomVar " + name + " removed.");
                                    }
                                }
                            }
                        }
                        break;

                    case CommandType.Logout:
                        {
                            GameObjectSingleton<ApplicationController>.DJCGIMIDOPB.ExitGame(false, true);
                        }
                        break; 

                    default:
                        Utils.Log("Not Implemented!");
                        break;
                    
                }
            }
        }


        private void UpdateClientStatus()
        {
            ClientStatus ClientStatus = new ClientStatus();
            ClientStatus.TimeStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

            //Utils.Log("Props:");
            //Dictionary<string, object> EBNBHBHNCFC = (Dictionary<string, object>)Utils.GetInstanceField(this.player, "EBNBHBHNCFC");

            // Character Info

            ClientStatus.CharacterInfo.BACKPACKID = this.player?.GetEquippedObject("Backpack")?.DMCIODGEHCN;

            List<String> Buffs = new List<string>();
            FloatingPanel BuffBar = Utils.FindPanelByName("BuffBar")?.FirstOrDefault().Value;
            if (BuffBar != null)
            {
                DynamicWindow window = BuffBar.GetComponent<DynamicWindow>();
                if (window != null)
                {
                    Type KAAFKBBECEF = AssemblyCSharp.GetType("KAAFKBBECEF");
                    if (KAAFKBBECEF != null)
                    {
                        object HGBANEEHBLH = Utils.GetInstanceField(window, "HGBANEEHBLH");
                        if (HGBANEEHBLH != null)
                        {
                            foreach (object o in (HGBANEEHBLH as IEnumerable))
                            {
                                object casted = Convert.ChangeType(o, KAAFKBBECEF);
                                if (casted != null)
                                {
                                    string ELGLAFGJGAO = (string)Utils.GetInstanceField(KAAFKBBECEF, casted, "ELGLAFGJGAO");
                                    if (!String.IsNullOrEmpty(ELGLAFGJGAO))
                                    {
                                        Buffs.Add(ELGLAFGJGAO.Replace('\n', '|'));
                                    }
                                }

                            }
                        }
                    }
                }
            }
            ClientStatus.CharacterInfo.CHARBUFFS = Buffs.ToArray();

            ClientStatus.CharacterInfo.CHARNAME = this.player?.EBHEDGHBHGI;
            ClientStatus.CharacterInfo.CHARID = this.player?.ObjectId;
            ClientStatus.CharacterInfo.CHARPOSX = this.player?.transform?.position.x;
            ClientStatus.CharacterInfo.CHARPOSY = this.player?.transform?.position.y;
            ClientStatus.CharacterInfo.CHARPOSZ = this.player?.transform?.position.z;
            ClientStatus.CharacterInfo.CHARDIR = null;
            if (this.player != null)
            {
                ClientStatus.CharacterInfo.CHARSTATUS =
                    (this.player.IsInCombatMode() ? "G" : "") +
                    (this.player.PLFMFNKLBON == CoreUtil.ShardShared.MobileFrozenState.MoveFrozen || this.player.PLFMFNKLBON == CoreUtil.ShardShared.MobileFrozenState.MoveAndTurnFrozen ? "A" : "");
            }
            else
            {
                ClientStatus.CharacterInfo.CHARSTATUS = null;
            }
            ClientStatus.CharacterInfo.CHARGHOST = (bool?)this.player?.GetObjectProperty("IsDead");

            if (this.player != null)
            {
                double? CharWeight = 0;
                List<EquipmentObject> ICGEHBHPFOA = (List<EquipmentObject>)Utils.GetInstanceField(this.player, "ICGEHBHPFOA");
                if (ICGEHBHPFOA != null)
                {
                    foreach (EquipmentObject obj in ICGEHBHPFOA)
                    {
                        CharWeight += (int)obj.GetComponent<DynamicObject>().IAIEDKKKPPK;
                    }
                }
                ClientStatus.CharacterInfo.CHARWEIGHT = CharWeight;
            }
            else
            {
                ClientStatus.CharacterInfo.CHARWEIGHT = null;
            }

            ClientStatus.CharacterInfo.HEADID = this.player?.GetEquippedObject("Head")?.DMCIODGEHCN;
            ClientStatus.CharacterInfo.HEADNAME = this.player?.GetEquippedObject("Head")?.GetComponent<DynamicObject>()?.EBHEDGHBHGI;
            ClientStatus.CharacterInfo.CHESTID = this.player?.GetEquippedObject("Chest")?.DMCIODGEHCN;
            ClientStatus.CharacterInfo.CHESTNAME = this.player?.GetEquippedObject("Chest")?.GetComponent<DynamicObject>()?.EBHEDGHBHGI;
            ClientStatus.CharacterInfo.LEGSID = this.player?.GetEquippedObject("Legs")?.DMCIODGEHCN;
            ClientStatus.CharacterInfo.LEGSNAME = this.player?.GetEquippedObject("Legs")?.GetComponent<DynamicObject>()?.EBHEDGHBHGI;
            ClientStatus.CharacterInfo.RIGHTHANDID = this.player?.GetEquippedObject("RightHand")?.DMCIODGEHCN;
            ClientStatus.CharacterInfo.RIGHTHANDNAME = this.player?.GetEquippedObject("RightHand")?.GetComponent<DynamicObject>()?.EBHEDGHBHGI;
            ClientStatus.CharacterInfo.LEFTHANDID = this.player?.GetEquippedObject("LeftHand")?.DMCIODGEHCN;
            ClientStatus.CharacterInfo.LEFTHANDNAME = this.player?.GetEquippedObject("LeftHand")?.GetComponent<DynamicObject>()?.EBHEDGHBHGI;

            // Status Bar

            ClientStatus.StatusBar.STR = this.player?.GetStatByName("Str");
            ClientStatus.StatusBar.HEALTH = this.player?.GetStatByName("Health");
            ClientStatus.StatusBar.INT = this.player?.GetStatByName("Int");
            ClientStatus.StatusBar.MANA = this.player?.GetStatByName("Mana");
            ClientStatus.StatusBar.AGI = this.player?.GetStatByName("Agi");
            ClientStatus.StatusBar.ATTACKSPEED = this.player?.GetStatByName("AttackSpeed");
            ClientStatus.StatusBar.STAMINA = this.player?.GetStatByName("Stamina");
            ClientStatus.StatusBar.DEFENSE = this.player?.GetStatByName("Defense");
            ClientStatus.StatusBar.VITALITY = this.player?.GetStatByName("Vitality");
            ClientStatus.StatusBar.PRESTIGEXPMAX = this.player?.GetStatByName("PrestigeXPMax");
            ClientStatus.StatusBar.STEALTH = this.player?.GetStatByName("Stealth");

            // LastAction

            ClientStatus.LastAction.COBJECTID = this.inputController?.BODCEBEPNMH?.ObjectId;
            ClientStatus.LastAction.LOBJECTID = this.inputController?.MFJFNHLOHOI?.ObjectId;

            // Find

            ClientStatus.Find.FINDBUTTON = this.FindButtonResults;
            ClientStatus.Find.FINDINPUT = this.FindInputResults;
            ClientStatus.Find.FINDITEM = this.FindItemResults;
            ClientStatus.Find.FINDLABEL = this.FindLabelResults;
            ClientStatus.Find.FINDMOBILE = this.FindMobileResults;
            ClientStatus.Find.FINDPANEL = this.FindPanelResults;
            ClientStatus.Find.FINDPERMANENT = this.FindPermanentResults;

            // Client Info

            ClientStatus.ClientInfo.CLIVER = ApplicationController.c_clientVersion;
            ClientStatus.ClientInfo.CLIID = this.ProcessId;
            ClientStatus.ClientInfo.CLIXRES = Screen.width;
            ClientStatus.ClientInfo.CLIYRES = Screen.height;
            ClientStatus.ClientInfo.FULLSCREEN = Screen.fullScreen;
            ClientStatus.ClientInfo.CLIGAMESTATE = this.applicationController?.JOJPMHOLNHA.ToString();
            ClientStatus.ClientInfo.SERVER = Utils.GetInstanceField(this.applicationController, "EGBNKJDFBEJ")?.ToString();
            ClientStatus.ClientInfo.TARGETFRAMERATE = Application.targetFrameRate;
            ClientStatus.ClientInfo.VSYNCCOUNT = QualitySettings.vSyncCount;
            ClientStatus.ClientInfo.MAINCAMERAMASK = Camera.main?.cullingMask;

            // Miscellanous

            ClientStatus.Miscellaneous.CLICKOBJ.CNTID = this.lastMouseClickClientObject?.DynamicInst?.ContainerId;
            ClientStatus.Miscellaneous.CLICKOBJ.NAME = this.lastMouseClickClientObject?.DynamicInst?.EBHEDGHBHGI;
            ClientStatus.Miscellaneous.CLICKOBJ.OBJECTID = this.lastMouseClickClientObject?.DynamicInst?.ObjectId;
            ClientStatus.Miscellaneous.CLICKOBJ.PERMANENTID = this.lastMouseClickClientObject?.PermanentId;

            ClientStatus.Miscellaneous.CLICKWINDOWX = this.lastMouseClickPosition.x;
            ClientStatus.Miscellaneous.CLICKWINDOWY = this.lastMouseClickPosition.y;

            if (Camera.main != null)
            {
                JKDPNLPCCNI.GetSurfaceHit(Camera.main.ScreenPointToRay(this.lastMouseClickPosition), out RaycastHit raycastHit, JKDPNLPCCNI.IMHGKJPOBHP.All);
                ClientStatus.Miscellaneous.CLICKWORLDX = raycastHit.point.x;
                ClientStatus.Miscellaneous.CLICKWORLDY = raycastHit.point.y;
                ClientStatus.Miscellaneous.CLICKWORLDZ = raycastHit.point.z;
            }
            else
            {
                ClientStatus.Miscellaneous.CLICKWORLDX = null;
                ClientStatus.Miscellaneous.CLICKWORLDY = null;
                ClientStatus.Miscellaneous.CLICKWORLDZ = null;
            }

            ClientStatus.Miscellaneous.COMMANDID = this.ClientCommandId;

            ClientStatus.Miscellaneous.CUSTOMVARS = this.CustomVars?.ToDictionary(v => v.Key, v => new ClientStatus.CustomVarStruct(v.Value)) ?? null;

            if (this.player != null)
            {
                try
                {
                    ClientStatus.Miscellaneous.NEARBYMONSTERS =
                        Utils.GetNearbyMobiles(5)?
                        .Where(Mobile =>
                            Mobile.DKCMJFOPPDL == "Monster" &&
                            Mobile.BMHLGHANHDL != null &&
                            !Mobile.GetObjectProperty<bool>("IsDead")
                            )
                        .Select(f => new ClientStatus.NEARBYMONSTERStruct()
                        {
                            DISTANCE = Vector3.Distance(f.transform.position, this.player.transform.position),
                            HP = f.GetStatByName("Health"),
                            ID = f.ObjectId,
                            NAME = f.EBHEDGHBHGI
                        })
                        .ToArray();
                    ClientStatus.Miscellaneous.MONSTERSNEARBY = (ClientStatus.Miscellaneous.NEARBYMONSTERS?.Count() ?? 0) > 0;
                }
                catch (Exception ex)
                {
                    ClientStatus.Miscellaneous.NEARBYMONSTERS = null;
                    ClientStatus.Miscellaneous.MONSTERSNEARBY = null;
                    Utils.Log("Error building NEARBYMONSTERS!");
                    Utils.Log(ex.ToString());
                }
            }
            else
            {
                ClientStatus.Miscellaneous.NEARBYMONSTERS = null;
                ClientStatus.Miscellaneous.MONSTERSNEARBY = null;
            }

            ClientStatus.Miscellaneous.MOUSEOVEROBJ.CNTID = this.inputController?.HFHBOINDMAJ?.DynamicInst?.ContainerId;
            ClientStatus.Miscellaneous.MOUSEOVEROBJ.NAME = this.inputController?.HFHBOINDMAJ?.name;
            ClientStatus.Miscellaneous.MOUSEOVEROBJ.OBJECTID = this.inputController?.HFHBOINDMAJ?.DynamicInst?.ObjectId;
            ClientStatus.Miscellaneous.MOUSEOVEROBJ.PERMANENTID = this.inputController?.HFHBOINDMAJ?.PermanentId;

            UICamera.Raycast(Input.mousePosition);
            ClientStatus.Miscellaneous.MOUSEOVERUI.NAME = UICamera.EHDALGCGPEK?.name;
            ClientStatus.Miscellaneous.MOUSEOVERUI.X = UICamera.EHDALGCGPEK?.transform?.localPosition.x;
            ClientStatus.Miscellaneous.MOUSEOVERUI.Y = UICamera.EHDALGCGPEK?.transform?.localPosition.y;

            if (Input.mousePosition != null &&
                Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
                Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
            {
                ClientStatus.Miscellaneous.MOUSEWINDOWPOSX = Input.mousePosition.x;
                ClientStatus.Miscellaneous.MOUSEWINDOWPOSY = Input.mousePosition.y;

                if (Camera.main != null)
                {
                    JKDPNLPCCNI.GetSurfaceHit(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit raycastHit, JKDPNLPCCNI.IMHGKJPOBHP.All);
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSX = raycastHit.point.x;
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSY = raycastHit.point.y;
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSZ = raycastHit.point.z;
                }
                else
                {
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSX = null;
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSY = null;
                    ClientStatus.Miscellaneous.MOUSEWORLDPOSZ = null;
                }
            } else
            {
                ClientStatus.Miscellaneous.MOUSEWINDOWPOSX = null;
                ClientStatus.Miscellaneous.MOUSEWINDOWPOSY = null;
                ClientStatus.Miscellaneous.MOUSEWORLDPOSX = null;
                ClientStatus.Miscellaneous.MOUSEWORLDPOSY = null;
                ClientStatus.Miscellaneous.MOUSEWORLDPOSZ = null;
            }

            ClientStatus.Miscellaneous.RANDOM = new System.Random().Next(0, 1000);

            ClientStatus.Miscellaneous.SCANJOURNALTIME = this.ScanJournalTime;
            ClientStatus.Miscellaneous.SCANJOURNALMESSAGE = this.ScanJournalMessage;

            if (inputController != null)
            {
                ClientStatus.Miscellaneous.TARGETLOADING = inputController.MAHPFOEKHPO;
                ClientStatus.Miscellaneous.TARGETTYPE = ((InputController.FBKEBHPKOIC)(Utils.GetInstanceField(inputController, "BFNLCIMBCJF") ?? InputController.FBKEBHPKOIC.None)).ToString();
            } else
            {
                ClientStatus.Miscellaneous.TARGETLOADING = null;
                ClientStatus.Miscellaneous.TARGETTYPE = null;
            }

            ClientStatus.Miscellaneous.TIME = Time.time;

            ClientStatus.Miscellaneous.TOOLTIPTEXT = this.tooltipText;

            //Utils.Log("UpdateStatus!");
            if (this.ProcessId != -1 && ClientStatusMemoryMap != null)
            {
                ClientStatusMemoryMap.WriteMemoryMap<ClientStatus>(ClientStatus);
            }
        }

        private float update;
        void Update()
        {
            //Utils.Log("Update() Start");

            try
            {
                //Utils.Log("DeltaTime = " + Time.deltaTime.ToString());
                update += Time.deltaTime;

                if (
                    Input.mousePosition != null &&
                    Input.mousePosition.x >= 0 && Input.mousePosition.x <= Screen.width &&
                    Input.mousePosition.y >= 0 && Input.mousePosition.y <= Screen.height)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!this.leftMouseDown)
                        {
                            this.lastMouseClickPosition = Input.mousePosition;
                            this.lastMouseClickClientObject = this.inputController?.HFHBOINDMAJ;
                        }
                        this.leftMouseDown = true;
                    }
                    else
                    {
                        this.leftMouseDown = false;
                    }

                    if (Input.GetMouseButtonDown(1))
                    {
                        if (!this.rightMouseDown)
                        {
                        }
                        this.rightMouseDown = true;
                    }
                    else
                    {
                        this.rightMouseDown = false;
                    }
                }

                //Utils.Log("update = " + update.ToString());
                if (update > 0.5f)
                {
                    //Utils.Log("Update!");
                    update = 0;

                    var updateWatch = new System.Diagnostics.Stopwatch();
                    updateWatch.Start();

                    this.applicationController = GameObjectSingleton<ApplicationController>.DJCGIMIDOPB;

                    if (this.applicationController?.JOJPMHOLNHA == ApplicationController.IJFOCJIHKHC.Game)
                    {
                        this.player = this.applicationController?.Player;
                        this.inputController = InputController.Instance;
                    }
                    else
                    {
                        this.player = null;
                        this.inputController = null;
                    }

                    if (this.applicationController != null && VERBOSE_DEBUG && !this.Intercepting)
                    {
                        // The logic and the obfuscated names used in this logic
                        // can be inferred by looking at SendMessage() in MessageCore.dll
                        if (applicationController.GPLIHPHPNKL != null)
                        {
                            AOHDPDIPMKO GEDMGBHAEAB = (AOHDPDIPMKO)Utils.GetInstanceField<JFFJBADOENN>(applicationController.GPLIHPHPNKL, "GEDMGBHAEAB");
                            if (GEDMGBHAEAB != null)
                            {
                                try
                                {
                                    Utils.SetInstanceField(GEDMGBHAEAB, "HCBFBBABLDC", true);
                                    Utils.Log("INTERCEPTING ENABLED!");
                                }
                                catch (Exception ex)
                                {
                                    Utils.Log(ex.ToString());
                                }
                                finally
                                {
                                    this.Intercepting = true;
                                }
                            }
                        }
                    }

                    Queue<ClientCommand> ClientCommandsQueue = null;
                    ClientCommand[] ClientCommandsArray = null;
                    ClientCommand ClientCommand = null;

                    if (this.ProcessId != -1 && this.ClientCommandsMemoryMap != null)
                    {
                        try
                        {
                            ClientCommandsMemoryMap.ReadMemoryMap(out this.ClientCommandId, out ClientCommandsArray);
                        }
                        catch (Exception ex)
                        {
                            Utils.Log("Error reading memory map: " + ex.ToString());
                        }
                        if (ClientCommandsArray != null)
                        {
                            ClientCommandsQueue = new Queue<ClientCommand>(ClientCommandsArray);
                            if (ClientCommandsQueue.Count > 0)
                            {
                                Utils.Log("Command found");
                                ClientCommand = ClientCommandsQueue.Dequeue();
                                ClientCommandId++;
                                try
                                {
                                    ProcessClientCommand(ClientCommand);
                                    Utils.Log("Command " + ClientCommandId.ToString() + " processed");
                                }
                                catch (Exception ex)
                                {
                                    Utils.Log("Error processing client command: " + ex.ToString());
                                }
                            }
                        }
                    }

                    try
                    {
                        UpdateClientStatus();
                    } catch (Exception ex)
                    {
                        Utils.Log("Error updating status: " + ex.ToString());
                    }

                    if (this.ProcessId != -1 && this.ClientCommandsMemoryMap != null)
                    {
                        if (ClientCommandId > 0 && ClientCommandsQueue != null)
                        {
                            try
                            {
                                ClientCommandsMemoryMap.WriteMemoryMap(ClientCommandId, ClientCommandsQueue.ToArray());
                            }
                            catch (Exception ex)
                            {
                                Utils.Log("Error reading memory map: " + ex.ToString());
                            }
                        }
                    }

                    updateWatch.Stop();
                    //Utils.Log("Update finished in " + updateWatch.ElapsedMilliseconds.ToString());
                }
                //Utils.Log("Update() finish");
            }
            catch (Exception ex)
            {
                Utils.Log(ex.ToString());
                Utils.Log(ex.StackTrace);
            }
        }

        private void OnGUI()
        {
        }

        void OnEnable()
        {
            Utils.Log("OnEnable");

            try
            {
                if (VERBOSE_DEBUG)
                {
                    Utils.Log("VERBOSE_DEBUG enabled!");
                    TraceModule module = new TraceModule("messagelog.");
                    module.AddDebugConsoleListener(SourceLevels.All);
                    ShardEngineDebug.AddTraceModule("messagelog.", module);
                    ShardEngineDebug.PushDebugScope("messagelog.");
                }
            }
            catch (Exception ex)
            {
                Utils.Log(ex.ToString());
            }
        }

        void OnDisable()
        {
            Utils.Log("OnDisable");
        }
    }
}