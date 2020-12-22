using ICSharpCode.TextEditor;
using LoU;
using Microsoft.Win32;
using SharpMonoInjector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using menelabs.core;

namespace EasyLoU
{
    public partial class MainForm : Form
    {
        public static MainForm TheMainForm;

        public static int CurrentClientProcessId = -1;

        private static String ClientStatusMemoryMapMutexName;
        private static String ClientStatusMemoryMapName;
        private static Int32 ClientStatusMemoryMapSize;
        public static MemoryMap ClientStatusMemoryMap;

        private static String ClientCommandsMemoryMapMutexName;
        private static String ClientCommandsMemoryMapName;
        private static Int32 ClientCommandsMemoryMapSize;
        public static MemoryMap ClientCommandsMemoryMap;

        public static object ClientStatusLock = new object();
        public static ClientStatus ClientStatus;

        private static Dictionary<String, ScriptDebugger> ScriptDebuggers = new Dictionary<string, ScriptDebugger>();
        public Boolean onTopToggle = false;

        public MainForm()
        {
            MainForm.TheMainForm = this;
            InitializeComponent();
            this.Text = "EasyLoU - " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Settings.LoadSettings();
            Settings.RegisterHotkeys(this.Handle);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (CheckForUnsavedChanges())
            {
                Settings.UnregisterHotkeys(this.Handle);
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == KeyboardHook.WM_HOTKEY)
            {
                if (m.WParam.ToInt32() == 1) DoPlay();
                if (m.WParam.ToInt32() == 2) DoStop();
                if (m.WParam.ToInt32() == 3) DoStopAll();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.N))
            {
                DoNew();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.O))
            {
                DoOpen();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.S))
            {
                DoSave();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Q))
            {
                DoExit();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void UpdateVar(String Key, String Value)
        {
            if (!VarsTreeView.Nodes.ContainsKey(Key))
                VarsTreeView.Nodes.Add(Key, Key + "=" + Value);
            else
                VarsTreeView.Nodes[Key].Text = Key + "=" + Value;

        }
        private void UpdateNode(TreeNode Node, object Val)
        {
            if (Val != null)
            {
                Type ValType = Val.GetType();
                if (ValType.IsValueType && !ValType.IsPrimitive && !ValType.IsEnum)
                {
                    // It's a struct
                    Node.Text = Node.Name;
                    int p = 0;
                    foreach (var field in Val.GetType().GetFields())
                    {
                        UpdateOrCreateNode(Node.Nodes, field.Name, field.GetValue(Val));
                        p++;
                    }
                    // Get rid of remaining nodes
                    while (Node.Nodes.Count > p)
                    {
                        Node.Nodes.Remove(Node.Nodes[Node.Nodes.Count - 1]);
                    }
                }
                else if (ValType.IsArray)
                {
                    // It's an array
                    Node.Text = Node.Name;
                    int i = 0;
                    foreach (var item in ((IEnumerable)Val).Cast<object>())
                    {
                        UpdateOrCreateNode(Node.Nodes, (i + 1).ToString(), item);
                        i++;
                    }
                    // Get rid of remaining nodes
                    while (Node.Nodes.Count > i)
                    {
                        Node.Nodes.Remove(Node.Nodes[Node.Nodes.Count - 1]);
                    }
                }
                else if (Val is Dictionary<string, ClientStatus.CustomVarStruct>)
                {
                    // It's a dictionary
                    Dictionary<string, object> Values = ((Dictionary<string, ClientStatus.CustomVarStruct>)Val).ToDictionary(
                        v => v.Key,
                        v => v.Value.CommandParamType == ClientStatus.CutomVarTypeEnum.Boolean ? (object)v.Value.Boolean :
                            v.Value.CommandParamType == ClientStatus.CutomVarTypeEnum.Number ? (object)v.Value.Number :
                            v.Value.CommandParamType == ClientStatus.CutomVarTypeEnum.String ? (object)v.Value.String :
                            (object)null
                        );
                    Node.Text = Node.Name;
                    int i = 0;
                    foreach (var item in Values)
                    {
                        UpdateOrCreateNode(Node.Nodes, item.Key, item.Value);
                        i++;
                    }
                    // Get rid of remaining nodes
                    while (Node.Nodes.Count > i)
                    {
                        Node.Nodes.Remove(Node.Nodes[Node.Nodes.Count - 1]);
                    }
                }
                else if (Val is string || Val is char)
                {
                    Node.Text = Node.Name + "=\"" + Val.ToString() + "\"";
                }
                else if (Val is bool)
                {
                    Node.Text = Node.Name + "=" + Val.ToString().ToLower() + "";
                }
                else
                {
                    Node.Text = Node.Name + "=" + Val.ToString();
                }
            } else
            {
                Node.Text = Node.Name + "=nil";
                if (Node.Nodes.Count > 0) Node.Nodes.Clear();
            }
        }
        private void UpdateOrCreateNode(TreeNodeCollection ParentTreeNodeCollection, String Key, object Val)
        {
            TreeNode Node;
            if (!ParentTreeNodeCollection.ContainsKey(Key))
            {
                Node = ParentTreeNodeCollection.Add(Key, "");
            }
            else
            {
                Node = ParentTreeNodeCollection[Key];
            }
            UpdateNode(Node, Val);
        }

        public delegate void UpdateStatusTreeViewDelegate();
        public void UpdateStatusTreeView()
        {
            lock (ClientStatusLock)
            {
                StatusTreeView.BeginUpdate();
                UpdateOrCreateNode(StatusTreeView.Nodes, "Timestamp", ClientStatus.TimeStamp.ToString());
                if (new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds() - ClientStatus.TimeStamp <= 60000)
                {
                    MainStatusLabel.ForeColor = Color.Green;
                    MainStatusLabel.Text = string.Format(
                        "Connected to {0}.  ClientStatus MemMap: {1}/{2} ({3:0.00}%). ClientCommands MemMap: {4}/{5} ({6:0.00}%).",
                        MainForm.CurrentClientProcessId.ToString(),
                        ClientStatusMemoryMap.LastMemoryOccupation,
                        ClientStatusMemoryMap.MapSize,
                        ClientStatusMemoryMap.LastMemoryOccupationPerc,
                        ClientCommandsMemoryMap.LastMemoryOccupation,
                        ClientCommandsMemoryMap.MapSize,
                        ClientCommandsMemoryMap.LastMemoryOccupationPerc);

                    //UpdateAttributesGroup(ClientStatus.CharacterInfo, "Character Info");
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Character Info", ClientStatus.CharacterInfo);
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Status Bar", ClientStatus.StatusBar);
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Last Action", ClientStatus.LastAction);
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Find", ClientStatus.Find);
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Client Info", ClientStatus.ClientInfo);
                    UpdateOrCreateNode(StatusTreeView.Nodes, "Miscellaneous", ClientStatus.Miscellaneous);
                }
                else
                {
                    if (MainStatusLabel.Text.StartsWith("Connected to"))
                    {
                        MainStatusLabel.ForeColor = Color.Red;
                        MainStatusLabel.Text = "Client " + MainForm.CurrentClientProcessId.ToString() + " not responding!";
                        DoStopAll();
                        MessageBoxEx.Show("Client disconnected!");
                    }
                }
                StatusTreeView.EndUpdate();
            }
        }

        public static void RefreshClientStatus()
        {
            if (MainForm.CurrentClientProcessId == -1 || MainForm.ClientStatusMemoryMap == null)
                return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            lock (MainForm.ClientStatusLock)
            {
                MainForm.ClientStatusMemoryMap.ReadMemoryMap<ClientStatus>(out MainForm.ClientStatus);
            }

            if (MainForm.TheMainForm != null && MainForm.ClientStatus != null)
            {
                MainForm.TheMainForm.Invoke(new MainForm.ResetTimerReadClientStatusDelegate(MainForm.TheMainForm.ResetTimerReadClientStatus));
                MainForm.TheMainForm.Invoke(new MainForm.UpdateStatusTreeViewDelegate(MainForm.TheMainForm.UpdateStatusTreeView));
            }

            stopwatch.Stop();
            //Console.WriteLine("RefreshClientStatus completed in {0} ms", stopwatch.ElapsedMilliseconds);
        }

        public delegate void ResetTimerReadClientStatusDelegate();
        public void ResetTimerReadClientStatus()
        {
            this.TimerReadClientStatus.Stop();
            this.TimerReadClientStatus.Start();
        }

        private void TimerReadClientStatus_Tick(object sender, EventArgs e)
        {
            MainForm.RefreshClientStatus();
        }

        private void TimerRefreshScriptVars_Tick(object sender, EventArgs e)
        {
            String guid = ScriptsTab.SelectedTab.Tag.ToString();
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptDebugger Debugger;
            if (ScriptDebuggers.TryGetValue(guid, out Debugger))
            {
                if (Debugger.vars != null && Debugger.vars.Count > 0)
                {
                    VarsTreeView.BeginUpdate();
                    lock (Debugger.varsLock)
                    {
                        foreach (var v in Debugger.vars)
                        {
                            UpdateVar(v.Key, v.Value);
                        }
                    }
                    VarsTreeView.EndUpdate();
                }
            }
            else
            {
                VarsTreeView.Nodes.Clear();
            }
        }
        #region Actions

        private void DoNew()
        {
            TabPage NewScriptTab = new TabPage();
            SplitContainer NewScriptSplit = new System.Windows.Forms.SplitContainer();
            TextEditorControlEx NewScriptTextArea = new TextEditorControlEx();
            TextBox NewScriptOutput = new System.Windows.Forms.TextBox();

            // 
            // NewScriptTab
            // 
            NewScriptTab.Controls.Add(NewScriptSplit);
            NewScriptTab.Location = new System.Drawing.Point(4, 29);
            NewScriptTab.Name = "NewScriptTab";
            NewScriptTab.Padding = new System.Windows.Forms.Padding(3);
            NewScriptTab.Size = new System.Drawing.Size(492, 300);
            NewScriptTab.TabIndex = 0;
            NewScriptTab.Text = "new";
            NewScriptTab.Tag = Guid.NewGuid().ToString();
            NewScriptTab.UseVisualStyleBackColor = true;
            // 
            // ScriptSplit
            // 
            NewScriptSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            NewScriptSplit.Location = new System.Drawing.Point(3, 3);
            NewScriptSplit.Name = "ScriptSplit";
            NewScriptSplit.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // ScriptSplit.Panel1
            // 
            NewScriptSplit.Panel1.Controls.Add(NewScriptTextArea);
            // 
            // ScriptSplit.Panel2
            // 
            NewScriptSplit.Panel2.Controls.Add(NewScriptOutput);
            NewScriptSplit.Size = new System.Drawing.Size(486, 286);
            NewScriptSplit.SplitterDistance = 143;
            NewScriptSplit.TabIndex = 1;
            // 
            // NewScriptTextArea
            // 
            NewScriptTextArea.ContextMenuEnabled = true;
            NewScriptTextArea.ContextMenuShowDefaultIcons = true;
            NewScriptTextArea.ContextMenuShowShortCutKeys = true;
            NewScriptTextArea.Dock = System.Windows.Forms.DockStyle.Fill;
            NewScriptTextArea.FoldingStrategy = "Indent";
            NewScriptTextArea.Font = new System.Drawing.Font("Courier New", 10F);
            NewScriptTextArea.HideVScrollBarIfPossible = true;
            NewScriptTextArea.Location = new System.Drawing.Point(3, 3);
            NewScriptTextArea.Name = "ScriptTextArea";
            NewScriptTextArea.Size = new System.Drawing.Size(486, 294);
            NewScriptTextArea.SyntaxHighlighting = "Lua";
            NewScriptTextArea.TabIndex = 0;
            NewScriptTextArea.TextChanged += ScriptTextArea_TextChanged;
            NewScriptTextArea.Tag = "new";
            // 
            // ScriptOutput
            // 
            NewScriptOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            NewScriptOutput.Location = new System.Drawing.Point(0, 0);
            NewScriptOutput.Multiline = true;
            NewScriptOutput.Name = "ScriptOutput";
            NewScriptOutput.Size = new System.Drawing.Size(486, 139);
            NewScriptOutput.TabIndex = 0;

            ScriptsTab.TabPages.Insert(ScriptsTab.TabPages.Count - 1, NewScriptTab);
            ScriptsTab.SelectedIndex = ScriptsTab.TabPages.Count - 2;
        }

        private void DoOpen()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "LUA|*.lua|*.*|*.*";
                openFileDialog.Multiselect = false;
                if (openFileDialog.ShowDialog() == DialogResult.OK &&
                            openFileDialog.FileNames.Length > 0)
                {
                    String filePath = openFileDialog.FileName;
                    String fileName = Path.GetFileName(filePath);
                    String directryName = Path.GetDirectoryName(filePath);
                    DoNew();
                    ScriptsTab.SelectedTab.Text = fileName;
                    TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
                    ScriptTextArea.Tag = filePath;
                    ScriptTextArea.LoadFile(filePath);

                    FileSystemSafeWatcher watcher = new FileSystemSafeWatcher();
                    watcher.Path = directryName;
                    watcher.Filter = fileName;
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Changed += OnFileChanged;
                    watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(MainForm.TheMainForm, ex.ToString());
            }

        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            foreach (TabPage tabPageCollection in ScriptsTab.TabPages)
            {
                Control[] Controls = tabPageCollection.Controls.Find("ScriptTextArea", true);
                if (Controls.Count() > 0)
                {
                    ScriptTextArea = (TextEditorControlEx)Controls[0];
                    if (ScriptTextArea.FileName == e.FullPath) {
                        if (Settings.getAutoReloadFromDisk())
                        {
                            LoadFileThreadSafe(ScriptTextArea, "LoadFile", ScriptTextArea.FileName);
                            PrintGlobalOutput("File: " + ScriptTextArea.FileName + " reloaded from disk");
                        }
                    }
                }

            }
        }


        private delegate void LoadFileThreadSafeDelegate(TextEditorControlEx control, string propertyName, string propertyValue);
        public static void LoadFileThreadSafe(TextEditorControlEx control, string propertyName, string propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new LoadFileThreadSafeDelegate(LoadFileThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                do
                {
                    Thread.Sleep(100);
                }
                while (IsFileLocked(control.FileName));

                control.LoadFile(control.FileName);
            }
        }

        private static bool IsFileLocked(String filePath)
        {
            FileInfo file = new FileInfo(filePath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            return false;
        }

        private void DoReopen()
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);

            var confirmResult =  MessageBoxEx.Show(MainForm.TheMainForm, "Are you sure you want to reload " + ScriptTextArea.FileName + " from disk?", "Confirm reload", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                ScriptTextArea.LoadFile(ScriptTextArea.FileName);
            }
        }

        private void DoSave()
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            if ("new" == ScriptTextArea.Tag.ToString())
            {
                DoSaveAs();
            }
            else
            {
                try
                {
                    ScriptTextArea.SaveFile(ScriptTextArea.Tag.ToString());
                    ScriptTextArea.Document.UndoStack.ClearAll();
                    RefreshChangedStatus();
                }
                catch (Exception ex)
                {
                    MessageBoxEx.Show(MainForm.TheMainForm, ex.ToString());
                }
            }
        }

        private void DoSaveAs()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "LUA|*.lua|*.*|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK &&
                            saveFileDialog.FileNames.Length > 0)
                {
                    String filePath = saveFileDialog.FileName;
                    String directryName = Path.GetDirectoryName(filePath);
                    String fileName = Path.GetFileName(filePath);
                    TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
                    ScriptTextArea.Tag = filePath;
                    ScriptTextArea.SaveFile(filePath);
                    ScriptsTab.SelectedTab.Text = fileName;
                    FileSystemSafeWatcher watcher = new FileSystemSafeWatcher();
                    watcher.Path = directryName;
                    watcher.Filter = fileName;
                    watcher.NotifyFilter = NotifyFilters.LastWrite;
                    watcher.Changed += OnFileChanged;
                    watcher.EnableRaisingEvents = true;
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(MainForm.TheMainForm, ex.ToString());
            }
        }

        private void DoSaveAll()
        {
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                ScriptsTab.SelectedIndex = TabIndex;
                DoSave();
            }
            ScriptsTab.SelectedIndex = SelectedTabIndex;
        }

        private void DoClose()
        {
            bool UnsavedChanges = false;

            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            if (ScriptTextArea.Document.UndoStack.UndoItemCount > 0)
            {
                UnsavedChanges = true;
            }

            if (UnsavedChanges)
            {
                if (MessageBoxEx.Show(MainForm.TheMainForm, "You have unsaved changes. Are you sure you want to close the current script?", "Close script with unsaved changes", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            DoStop();

            ScriptsTab.SelectedTab.Controls.Clear();

            ScriptsTab.TabPages.Remove(ScriptsTab.SelectedTab);
        }

        private void DoCloseAll()
        {
            bool UnsavedChanges = false;
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptPage.Controls.Find("ScriptTextArea", true)[0]);
                if (ScriptTextArea.Document.UndoStack.UndoItemCount > 0)
                {
                    UnsavedChanges = true;
                    break;
                }
            }

            if (UnsavedChanges)
            {
                if (MessageBoxEx.Show(MainForm.TheMainForm, "You have unsaved changes. Are you sure you want to close all the open scripts?", "Close all script with unsaved changes", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            DoStopAll();

            for (int TabIndex = ScriptsTab.TabCount - 2; TabIndex >= 0; TabIndex--)
            {
                ScriptsTab.TabPages[TabIndex].Controls.Clear();
                ScriptsTab.TabPages.RemoveAt(TabIndex);
            }
        }

        private void DoCloseAllButThis()
        {
            bool UnsavedChanges = false;
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                if (TabIndex != ScriptsTab.SelectedIndex)
                {
                    TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                    TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptPage.Controls.Find("ScriptTextArea", true)[0]);
                    if (ScriptTextArea.Document.UndoStack.UndoItemCount > 0)
                    {
                        UnsavedChanges = true;
                        break;
                    }
                }
            }

            if (UnsavedChanges)
            {
                if (MessageBoxEx.Show(MainForm.TheMainForm, "You have unsaved changes. Are you sure you want to close all the open scripts?", "Close all but this script with unsaved changes", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return;
                }
            }

            for (int TabIndex = ScriptsTab.TabCount - 2; TabIndex >= 0; TabIndex--)
            {
                if (TabIndex != ScriptsTab.SelectedIndex)
                {
                    ScriptDebugger Debugger;
                    if (ScriptDebuggers.TryGetValue(ScriptsTab.TabPages[TabIndex].Tag.ToString(), out Debugger))
                    {
                        Debugger.Stop();
                    }
                    RefreshToolStripStatus();
                    ScriptsTab.TabPages[TabIndex].Controls.Clear();
                    ScriptsTab.TabPages.RemoveAt(TabIndex);
                }
            }
        }

        private bool CheckForUnsavedChanges()
        {
            bool UnsavedChanges = false;
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptPage.Controls.Find("ScriptTextArea", true)[0]);
                if (ScriptTextArea.Document.UndoStack.UndoItemCount > 0)
                {
                    UnsavedChanges = true;
                    break;
                }
            }

            if (UnsavedChanges)
            {
                if (MessageBoxEx.Show(MainForm.TheMainForm, "You have unsaved changes. Are you sure you want to exit?", "Exit with unsaved changes", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private void DoExit()
        {
            if (CheckForUnsavedChanges())
            {
                Application.Exit();
            }
        }

        private void DoPlay()
        {
            String guid = ScriptsTab.SelectedTab.Tag.ToString();
            ScriptDebugger Debugger;
            if (ScriptDebuggers.ContainsKey(guid))
            {
                Debugger = ScriptDebuggers[guid];
                Debugger.Name = ScriptsTab.SelectedTab.Text;
            }
            else
            {
                Debugger = new ScriptDebugger(this, guid, ScriptsTab.SelectedTab.Text);
                ScriptDebuggers.Add(guid, Debugger);
            }
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            Debugger.Play(ScriptTextArea.Text, Path.GetDirectoryName(ScriptTextArea.Tag.ToString()));
            RefreshToolStripStatus();
        }
        private void DoPause()
        {
            ScriptDebugger Debugger;
            if (ScriptDebuggers.TryGetValue(ScriptsTab.SelectedTab.Tag.ToString(), out Debugger))
            {
                Debugger.Pause();
            }
            RefreshToolStripStatus();
        }

        private void DoStop()
        {
            ScriptDebugger Debugger;
            if (ScriptDebuggers.TryGetValue(ScriptsTab.SelectedTab.Tag.ToString(), out Debugger))
            {
                Debugger.Stop();
            }
            RefreshToolStripStatus();
        }

        private void DoStopAll()
        {
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                ScriptDebugger Debugger;
                if (ScriptDebuggers.TryGetValue(ScriptsTab.TabPages[TabIndex].Tag.ToString(), out Debugger))
                {
                    Debugger.Stop();
                }
            }
            RefreshToolStripStatus();
        }

        private void DoStopAllButThis()
        {
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                if (TabIndex != ScriptsTab.SelectedIndex)
                {
                    ScriptDebugger Debugger;
                    if (ScriptDebuggers.TryGetValue(ScriptsTab.TabPages[TabIndex].Tag.ToString(), out Debugger))
                    {
                        Debugger.Stop();
                    }
                }
            }
            RefreshToolStripStatus();
        }

        private void DoVarDump()
        {
            DumpKeywords();
            DumpCommands();
        }

        private void DoManageVarList()
        {
            MessageBoxEx.Show(MainForm.TheMainForm, "Not implemented!");
        }

        private void DoDontMoveCursor()
        {
            MessageBoxEx.Show(MainForm.TheMainForm, "Not implemented!");
        }

        private void DoHelp()
        {
            System.Diagnostics.Process.Start("https://lmgtfy.com/?q=EasyLoU+help");
        }

        private void DoWebsite()
        {
            System.Diagnostics.Process.Start("https://lmgtfy.com/?q=EasyLoU+website");
        }

        private void DoConnectToClient()
        {
            TargetAriaClientPanel.Dock = DockStyle.Fill;
            TargetAriaClientPanel.Visible = true;

            MouseEventCallback handler = null;
            handler = (MouseEventType type, int x, int y) => {
                // Restore cursors
                // see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa
                // and also https://autohotkey.com/board/topic/32608-changing-the-system-cursor/
                MouseHook.SystemParametersInfo(0x57, 0, (IntPtr)0, 0);
                TargetAriaClientPanel.Visible = false;
                Cursor.Current = Cursors.Arrow;

                // Stop global hook
                MouseHook.HookEnd();
                MouseHook.MouseDown -= handler;

                // Get clicked coord
                MouseHook.POINT p;
                p.x = x;
                p.y = y;
                Debug.WriteLine("Clicked x=" + x.ToString() + " y=" + y.ToString());

                // Get clicked window handler, window title
                IntPtr hWnd = MouseHook.WindowFromPoint(p);
                int WindowTitleLength = MouseHook.GetWindowTextLength(hWnd);
                StringBuilder WindowTitle = new StringBuilder(WindowTitleLength + 1);
                MouseHook.GetWindowText(hWnd, WindowTitle, WindowTitle.Capacity);
                Debug.WriteLine("Clicked handle=" + hWnd.ToString() + " title=" + WindowTitle);

                if (WindowTitle.ToString() != "Legends of Aria")
                {
                    MessageBoxEx.Show(MainForm.TheMainForm, "The selected window is not a Legends of Aria client!");
                    return true;
                }

                // Get the processId, and connect
                uint processId;
                MouseHook.GetWindowThreadProcessId(hWnd, out processId);
                Debug.WriteLine("Clicked pid=" + processId.ToString());

                // Attempt connection (or injection, if needed)
                ConnectToClient((int)processId);
                return true;
            };

            // Prepare cursor image
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            Bitmap image = ((System.Drawing.Bitmap)(resources.GetObject("connectToClientToolStripMenuItem.Image")));

            // Set all cursors
            // see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setsystemcursor
            // and also https://autohotkey.com/board/topic/32608-changing-the-system-cursor/
            Cursor cursor = new Cursor(image.GetHicon());
            uint[] cursors = new uint[] { 32512, 32513, 32514, 32515, 32516, 32640, 32641, 32642, 32643, 32644, 32645, 32646, 32648, 32649, 32650, 32651 };
            foreach (uint i in cursors)
            {
                MouseHook.SetSystemCursor(cursor.Handle, i);
            }

            // Start mouse global hook
            MouseHook.MouseDown += handler;
            MouseHook.HookStart();
        }

        #endregion Actions

        #region Menus

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoNew();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoOpen();
        }

        private void reopenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoReopen();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSaveAs();
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoSaveAll();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void cloaseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoCloseAll();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoExit();
        }

        private void undoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoCut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoCopy();
        }

        private void pateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoPaste();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoDelete();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoSelectAll();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoFind();
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoReplace();
        }

        private void ScriptsTab_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ScriptsTab.SelectedIndex == ScriptsTab.TabPages.Count - 1)
            {
                DoNew();
                ScriptsTab.SelectedIndex = ScriptsTab.TabPages.Count - 2;
            }
            RefreshToolStripStatus();
            VarsTreeView.Nodes.Clear();
        }

        private void ScriptTextArea_TextChanged(object sender, EventArgs e)
        {
            RefreshChangedStatus();
            RefreshToolStripStatus();
        }

        private void helpToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DoHelp();
        }

        private void websiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoWebsite();
        }

        private void connectToClientToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoConnectToClient();
        }

        private void varDumpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoVarDump();
        }

        private void manageVarListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoManageVarList();
        }

        private void dontMoveCursorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoDontMoveCursor();
        }

        private void RefreshChangedStatus()
        {
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptPage.Controls.Find("ScriptTextArea", true)[0]);
                if (ScriptTextArea.Document.UndoStack.UndoItemCount > 0)
                {
                    if (ScriptPage.Text.Substring(ScriptPage.Text.Length - 1, 1) != "*")
                    {
                        ScriptPage.Text += "*";
                    }
                }
                else
                {
                    if (ScriptPage.Text.Substring(ScriptPage.Text.Length - 1, 1) == "*")
                    {
                        ScriptPage.Text = ScriptPage.Text.Substring(0, ScriptPage.Text.Length - 1);
                    }
                }
            }
        }

        public delegate void RefreshToolStripStatusDelegate();
        public void RefreshToolStripStatus()
        {
            bool AtLeastOneRunning = false;
            String guid = ScriptsTab.SelectedTab.Tag.ToString();
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptDebugger Debugger;
            if (ScriptDebuggers.TryGetValue(guid, out Debugger))
            {
                switch (ScriptDebuggers[guid].Status)
                {
                    case ScriptDebugger.DebuggerStatus.Idle:
                    case ScriptDebugger.DebuggerStatus.Stopped:
                        {
                            if (ScriptTextArea.Text != "")
                                PlayToolStripButton.Enabled = true;
                            else
                                PlayToolStripButton.Enabled = false;
                            PauseToolStripButton.Enabled = false;
                            StopToolStripButton.Enabled = false;
                        }
                        break;

                    case ScriptDebugger.DebuggerStatus.Running:
                        {
                            AtLeastOneRunning = true;
                            PlayToolStripButton.Enabled = false;
                            PauseToolStripButton.Enabled = true;
                            StopToolStripButton.Enabled = true;
                        }
                        break;

                    case ScriptDebugger.DebuggerStatus.Paused:
                        {
                            if (ScriptTextArea.Text != "")
                                PlayToolStripButton.Enabled = true;
                            else
                                PlayToolStripButton.Enabled = false;
                            PauseToolStripButton.Enabled = false;
                            StopToolStripButton.Enabled = true;
                        }
                        break;
                }
            }
            else
            {
                if (ScriptTextArea.Text != "")
                    PlayToolStripButton.Enabled = true;
                else
                    PlayToolStripButton.Enabled = false;
                PauseToolStripButton.Enabled = false;
                StopToolStripButton.Enabled = false;
            }
            if (!AtLeastOneRunning)
            {
                for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
                {
                    TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                    guid = ScriptPage.Tag.ToString();
                    if (ScriptDebuggers.TryGetValue(guid, out Debugger))
                    {
                        if (Debugger.Status == ScriptDebugger.DebuggerStatus.Running)
                            AtLeastOneRunning = true;
                    }
                }
            }
            if (AtLeastOneRunning)
            {
                StopAllToolStripButton.Enabled = true;
            }
            else
            {
                StopAllToolStripButton.Enabled = false;
            }
        }

        #endregion Menus

        #region PrimaryToolbar

        private void NewToolStripButton_Click(object sender, EventArgs e)
        {
            DoNew();
        }

        private void OpenToolStripButton_Click(object sender, EventArgs e)
        {
            DoOpen();
        }

        private void ReopenToolStripButton_Click(object sender, EventArgs e)
        {
            DoReopen();
        }

        private void SaveToolStripButton_Click(object sender, EventArgs e)
        {
            DoSave();
        }

        private void CloseToolStripButton_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void CutToolStripButton_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoCut();
        }

        private void CopyToolStripButton_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoCopy();
        }

        private void PasteToolStripButton_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoPaste();
        }

        private void FindToolStripButton_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoFind();
        }

        private void ReplaceToolStripButton_Click(object sender, EventArgs e)
        {
            TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
            ScriptTextArea.DoReplace();

        }

        #endregion PrimaryToolbar

        #region SecondaryToolbar

        private void PlayToolStripButton_Click(object sender, EventArgs e)
        {
            if (!MainStatusLabel.Text.StartsWith("Connected to"))
            {
                if (MessageBoxEx.Show(MainForm.TheMainForm, "EasyLoU not connected to any client. Run this script anyway?", "Client not connected", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    return;
                }
            }
            DoPlay();
            RefreshToolStripStatus();
        }

        private void connectToClientToolStripButton_Click(object sender, EventArgs e)
        {
            DoConnectToClient();
        }

        private void HelpToolStripButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Lady-Binary/EasyLoU/issues");
        }

        private void HomeToolStripButton_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Lady-Binary/EasyLoU");
        }


        private void DebugLogsToolStripButton_Click(object sender, EventArgs e)
        {
            var pathWithEnv = @"%USERPROFILE%\AppData\LocalLow\Citadel Studios Inc_\Legends of Aria\";
            var fileName = Environment.ExpandEnvironmentVariables(pathWithEnv);
            if (Directory.Exists(fileName))
            {
                Process.Start("explorer.exe", fileName);
            }
            else
            {
                MessageBox.Show("Expected path to debug log file not found.\n\nThe usual location is C:\\Users\\<username>\\AppData\\LocalLow\\Citadel Studios Inc_\\Legends of Aria\\Player.log");
            }
        }

        #endregion SecondaryToolbar

        #region Injection

        private IntPtr GetMonoModule(int ProcessId)
        {
            MainStatusLabel.ForeColor = Color.Orange;
            MainStatusLabel.Text = "Getting Mono Module...";

            IntPtr MonoModule = new IntPtr();
            string Name;

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id != ProcessId)
                    continue;

                const ProcessAccessRights flags = ProcessAccessRights.PROCESS_QUERY_INFORMATION | ProcessAccessRights.PROCESS_VM_READ;
                IntPtr handle;

                if ((handle = Native.OpenProcess(flags, false, p.Id)) != IntPtr.Zero)
                {
                    if (ProcessUtils.GetMonoModule(handle, out IntPtr mono))
                    {
                        MonoModule = mono;
                        Name = p.ProcessName;
                    }

                    Native.CloseHandle(handle);
                }
            }

            MainStatusLabel.ForeColor = Color.Orange;
            MainStatusLabel.Text = "Process refreshed";

            return MonoModule;
        }

        private void Inject(int ProcessId)
        {
            var MonoModule = GetMonoModule(ProcessId);

            String AssemblyPath = "LoU.dll";

            IntPtr handle = Native.OpenProcess(ProcessAccessRights.PROCESS_ALL_ACCESS, false, ProcessId);

            if (handle == IntPtr.Zero)
            {
                MainStatusLabel.ForeColor = Color.Red;
                MainStatusLabel.Text = "Failed to open process";
                return;
            }

            byte[] file;

            try
            {
                file = File.ReadAllBytes(AssemblyPath);
            }
            catch (IOException)
            {
                MainStatusLabel.ForeColor = Color.Red;
                MainStatusLabel.Text = "Failed to read the file " + AssemblyPath;
                return;
            }

            MainStatusLabel.Text = "Injecting " + Path.GetFileName(AssemblyPath);

            using (Injector injector = new Injector(handle, MonoModule))
            {
                try
                {
                    IntPtr asm = injector.Inject(file, "LoU", "Loader", "Load");
                    MainStatusLabel.ForeColor = Color.Orange;
                    MainStatusLabel.Text = "Injection on " + ProcessId.ToString() + " successful";
                }
                catch (InjectorException ie)
                {
                    MainStatusLabel.ForeColor = Color.Red;
                    MainStatusLabel.Text = "Injection on " + ProcessId.ToString() + " failed: " + ie.Message;
                }
                catch (Exception e)
                {
                    MainStatusLabel.ForeColor = Color.Red;
                    MainStatusLabel.Text = "Injection failed (unknown error): " + e.Message;
                }
            }
        }

        private Object GetKey(String KeyName)
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey key = SoftwareKey.OpenSubKey("EasyLoU", true);

            if (key == null)
            {
                SoftwareKey.CreateSubKey("EasyLoU");
                key = SoftwareKey.OpenSubKey("EasyLoU", true);
            }

            return key.GetValue("ExePath");
        }

        private void SetKey(String KeyName, Object KeyValue)
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey key = SoftwareKey.OpenSubKey("EasyLoU", true);

            if (key == null)
            {
                SoftwareKey.CreateSubKey("EasyLoU");
                key = SoftwareKey.OpenSubKey("EasyLoU", true);
            }

            key.SetValue(KeyName, KeyValue);
        }

        #endregion Injection

        #region Multiple Clients
        private void SearchClientAndConnect()
        {
            foreach (Process p in Process.GetProcesses())
            {
                if (p.ProcessName.Contains("Legends of Aria"))
                {
                    ConnectToClient(p.Id);
                    return;
                }
            }
        }

        private void ConnectToClient(int ProcessId)
        {
            MainStatusLabel.ForeColor = Color.Orange;
            MainStatusLabel.Text += " Connecting to " + ProcessId.ToString() + "...";
            MainForm.CurrentClientProcessId = ProcessId;

            MainForm.ClientStatusMemoryMapMutexName = "ELOU_CS_MX_" + ProcessId.ToString();
            MainForm.ClientStatusMemoryMapName = "ELOU_CS_" + ProcessId.ToString();
            MainForm.ClientStatusMemoryMapSize = 1024 * 1024 * 10;
            MainForm.ClientStatusMemoryMap = new MemoryMap(MainForm.ClientStatusMemoryMapName, MainForm.ClientStatusMemoryMapSize, MainForm.ClientStatusMemoryMapMutexName);

            MainForm.ClientCommandsMemoryMapMutexName = "ELOU_CC_MX_" + ProcessId.ToString();
            MainForm.ClientCommandsMemoryMapName = "ELOU_CC_" + ProcessId.ToString();
            MainForm.ClientCommandsMemoryMapSize = 1024 * 1024;
            MainForm.ClientCommandsMemoryMap = new MemoryMap(MainForm.ClientCommandsMemoryMapName, MainForm.ClientCommandsMemoryMapSize, MainForm.ClientCommandsMemoryMapMutexName);

            if (MainForm.ClientStatusMemoryMap.OpenExisting() && MainForm.ClientCommandsMemoryMap.OpenExisting())
            {
                // Client already patched, memorymaps open already all good
                MainStatusLabel.ForeColor = Color.Green;
                MainStatusLabel.Text = "Connection successful.";
                return;
            }

            if (MessageBoxEx.Show(MainForm.TheMainForm, "Client " + ProcessId.ToString() + " not yet injected. Inject now?", "Client not yet injected", MessageBoxButtons.OKCancel) != DialogResult.OK)
            {
                MainStatusLabel.ForeColor = Color.Red;
                MainStatusLabel.Text = "Connection aborted.";
                return;
            }

            MainStatusLabel.ForeColor = Color.Orange;
            MainStatusLabel.Text = "Connecting, please wait ...";

            Inject(ProcessId);

            System.Threading.Thread.Sleep(1000);

            if (MainForm.ClientStatusMemoryMap.OpenExisting() && MainForm.ClientCommandsMemoryMap.OpenExisting())
            {
                // Client already patched, memorymaps open already all good
                MainStatusLabel.ForeColor = Color.Green;
                MainStatusLabel.Text = "Connection successful.";
                return;
            }

            MainStatusLabel.ForeColor = Color.Red;
            MainStatusLabel.Text = "Connection failed.";
        }
        #endregion

        private void PauseToolStripButton_Click(object sender, EventArgs e)
        {
            DoPause();
        }

        private void StopToolStripButton_Click(object sender, EventArgs e)
        {
            DoStop();
        }


        private void StopAllToolStripButton_Click(object sender, EventArgs e)
        {
            DoStopAll();
        }

        private int CurrentSpin = 0;
        private void TimerSpinners_Tick(object sender, EventArgs e)
        {
            Char[] Spinners = "◴◷◶◵".ToCharArray();
            int SelectedTabIndex = ScriptsTab.SelectedIndex;
            for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; TabIndex++)
            {
                TabPage ScriptPage = ScriptsTab.TabPages[TabIndex];
                String guid = ScriptPage.Tag.ToString();
                ScriptDebugger Debugger;
                if (ScriptDebuggers.TryGetValue(guid, out Debugger))
                {
                    switch (Debugger.Status)
                    {
                        case ScriptDebugger.DebuggerStatus.Idle:
                        case ScriptDebugger.DebuggerStatus.Stopped:
                            {
                                if (Spinners.Contains(ScriptPage.Text[0]))
                                {
                                    ScriptPage.Text = ScriptPage.Text.Remove(0, 1);
                                }
                            }
                            break;
                        case ScriptDebugger.DebuggerStatus.Running:
                            {
                                if (Spinners.Contains(ScriptPage.Text[0]))
                                {
                                    ScriptPage.Text = ScriptPage.Text.Remove(0, 1);
                                }
                                ScriptPage.Text = Spinners[CurrentSpin] + ScriptPage.Text;
                            }
                            break;
                    }
                }
            }
            CurrentSpin = (CurrentSpin + 1) % 4;
        }

        private void StatusTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Text.Contains("="))
                {
                    StatusTreeView.SelectedNode = e.Node;
                    StatusTreeViewContextMenu.Show(Cursor.Position);
                }
            }
        }
        private void StatusTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Node.Text.Contains("="))
                {
                    int Separator = e.Node.Text.IndexOf('=');
                    String Value = e.Node.Text.Substring(Separator + 1, e.Node.Text.Length - 1 - Separator);
                    TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
                    ScriptTextArea.ActiveTextAreaControl.TextArea.InsertString(Value);
                }
            }
        }

        private void CopyNameStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (StatusTreeView.SelectedNode != null && StatusTreeView.SelectedNode.Text.Contains("="))
            {
                int Separator = StatusTreeView.SelectedNode.Text.IndexOf("=");
                string Value = StatusTreeView.SelectedNode.Text.Substring(0, Separator);
                Clipboard.SetText(Value);
            }
        }

        private void CopyValueStatusToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (StatusTreeView.SelectedNode != null && StatusTreeView.SelectedNode.Text.Contains("="))
            {
                int Separator = StatusTreeView.SelectedNode.Text.IndexOf("=");
                string Value = StatusTreeView.SelectedNode.Text.Substring(Separator + 1, StatusTreeView.SelectedNode.Text.Length - 1 - Separator);
                Clipboard.SetText(Value);
            }
        }

        private void closeThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoClose();
        }

        private void closeAllScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoCloseAll();
        }

        private void closeAllScriptsButThisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DoCloseAllButThis();
        }

        private void ScriptsTab_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                for (int TabIndex = 0; TabIndex < ScriptsTab.TabCount - 1; ++TabIndex)
                {
                    if (ScriptsTab.GetTabRect(TabIndex).Contains(e.Location))
                    {
                        ScriptsTab.SelectedIndex = TabIndex;
                        ScriptsTabContextMenu.Show(Cursor.Position);
                    }
                }
            }
        }

        private void DumpKeywords()
        {
            // <Key word = "FINDITEMIDS" />
            string prefix = " <Key word = \"";
            string postfix = "\" />";
            Console.WriteLine("RefreshKeywords()");
            foreach(TreeNode parent in StatusTreeView.Nodes)
            {
                foreach(TreeNode child in parent.Nodes)
                {
                    Console.WriteLine(prefix + child.Name + postfix);
                }
            }
        }

        static void DumpCommands()
        {
            // <Key word = "FINDITEMIDS" />
            string prefix = " <Key word = \"";
            string postfix = "\" />";
            Console.WriteLine("RefreshKeywords()");

            string s = string.Join(",", Enum.GetNames(typeof(LoU.CommandType)));

            foreach (String c in Enum.GetNames(typeof(LoU.CommandType)))
            {
                Debug.Print("- [" + c + "](#" + c + ")");
            }

            foreach (String c in Enum.GetNames(typeof(LoU.CommandType)))
            {
                Debug.Print("## " + c);
                Debug.Print("Description");
            }

            foreach (String c in Enum.GetNames(typeof(LoU.CommandType)))
            {
                Debug.Print(prefix + c + postfix);
            }
        }

        public delegate void PrintOutputDelegate(String guid, String s);
        public void PrintOutput(String guid, String s)
        {
            foreach (TabPage page in ScriptsTab.TabPages)
            {
                if (page.Tag.ToString() == guid)
                {
                    Control[] ScriptOutput = page.Controls.Find("ScriptOutput", true);
                    if (ScriptOutput != null)
                    {
                        TextBox ScriptOutputTextBox = (TextBox)ScriptOutput[0];
                        if (ScriptOutputTextBox.Text.Length > 1024 * 1024 * 100)
                        {
                            ScriptOutputTextBox.Text = ScriptOutputTextBox.Text.Remove(0, 1024 * 1024 * 1);
                        }
                        ((TextBox)ScriptOutput[0]).AppendText(s + Environment.NewLine);
                    }
                    return;
                }
            }
        }

        public delegate void PrintGlobalOutputDelegate(String s);
        public void PrintGlobalOutput(String s)
        {
            Control[] ScriptOutputs = Controls.Find("ScriptOutput", true);
            foreach (TextBox ScriptOutput in ScriptOutputs)
            {
                if (ScriptOutput != null)
                {
                    if (ScriptOutput.Text.Length > 1024 * 1024 * 100)
                    {
                        PrintGlobalThreadSafe(ScriptOutput, "Text", ScriptOutput.Text.Remove(0, 1024 * 1024 * 1));
                    }
                    PrintGlobalThreadSafe(ScriptOutput, "AppendText", s + Environment.NewLine);
                }
            }
        }

        private delegate void PrintGlobalThreadSafeDelegate(TextBox control, string propertyName, string propertyValue);
        public static void PrintGlobalThreadSafe(TextBox control, string propertyName, string propertyValue)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new PrintGlobalThreadSafeDelegate(PrintGlobalThreadSafe), new object[] { control, propertyName, propertyValue });
            }
            else
            {
                if(propertyName == "Text")
                {
                    control.Text = propertyValue;
                }
                if (propertyName == "AppendText")
                {
                    control.AppendText(propertyValue);
                }
            }
        }

        public delegate void WriteOutputDelegate(String guid, String s);
        public void WriteOutput(String guid, String s)
        {
            foreach (TabPage page in ScriptsTab.TabPages)
            {
                if (page.Tag.ToString() == guid)
                {
                    Control[] ScriptOutput = page.Controls.Find("ScriptOutput", true);
                    if (ScriptOutput != null)
                    {
                        TextBox ScriptOutputTextBox = (TextBox)ScriptOutput[0];
                        if (ScriptOutputTextBox.Text.Length > 1024 * 1024 * 100)
                        {
                            ScriptOutputTextBox.Text = ScriptOutputTextBox.Text.Remove(0, 1024 * 1024 * 1);
                        }
                        ((TextBox)ScriptOutput[0]).AppendText(s);
                    }
                    return;
                }
            }
        }

        public delegate void ClearOutputDelegate(String guid);
        public void ClearOutput(String guid)
        {
            foreach (TabPage page in ScriptsTab.TabPages)
            {
                if (page.Tag.ToString() == guid)
                {
                    Control[] ScriptOutput = page.Controls.Find("ScriptOutput", true);
                    if (ScriptOutput != null)
                    {
                        TextBox ScriptOutputTextBox = (TextBox)ScriptOutput[0];
                        ScriptOutputTextBox.Text = "";
                    }
                    return;
                }
            }
        }

        private void VarsTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.Node.Text.Contains("="))
                {
                    VarsTreeView.SelectedNode = e.Node;
                    VarsTreeViewContextMenu.Show(Cursor.Position);
                }
            }
        }
        private void VarsTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (e.Node.Text.Contains("="))
                {
                    int Separator = e.Node.Text.IndexOf("=");
                    String Value = e.Node.Text.Substring(Separator + 1, e.Node.Text.Length - 1 - Separator);
                    TextEditorControlEx ScriptTextArea = ((TextEditorControlEx)ScriptsTab.SelectedTab.Controls.Find("ScriptTextArea", true)[0]);
                    ScriptTextArea.ActiveTextAreaControl.TextArea.InsertString(Value);
                }
            }
        }

        private void CopyValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VarsTreeView.SelectedNode != null && VarsTreeView.SelectedNode.Text.Contains("="))
            {
                int Separator = VarsTreeView.SelectedNode.Text.IndexOf("=");
                string Value = VarsTreeView.SelectedNode.Text.Substring(0, Separator);
                Clipboard.SetText(Value);
            }
        }

        private void CopyValueVarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (VarsTreeView.SelectedNode != null && VarsTreeView.SelectedNode.Text.Contains("="))
            {
                int Separator = VarsTreeView.SelectedNode.Text.IndexOf("=");
                string Value = VarsTreeView.SelectedNode.Text.Substring(Separator + 1, VarsTreeView.SelectedNode.Text.Length - 1 - Separator);
                Clipboard.SetText(Value);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog(this);
        }

        private void pinToolStripButton_Click(object sender, EventArgs e)
        {
            if (this.onTopToggle == false)
            {
                Form.ActiveForm.TopMost = true;
                pinToolStripButton.Image = Properties.Resources.pin_blue;
                this.onTopToggle = true;
            }
            else
            {
                Form.ActiveForm.TopMost = false;
                pinToolStripButton.Image = Properties.Resources.pin_black;
                this.onTopToggle = false;
            }
        }
    }
}
