using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasyLoU
{
    public partial class Settings : Form
    {
        public static IntPtr HotkeysWindowHandle;

        public static Keys StartScriptHotkey = Keys.F8;
        public static int StartScriptHotkeyModifiers = (int)KeyModifiers.None;

        public static Keys StopScriptHotkey = Keys.F9;
        public static int StopScriptHotkeyModifiers = (int)KeyModifiers.None;

        public static Keys StopAllScriptsHotkey = Keys.F10;
        public static int StopAllScriptsHotkeyModifiers = (int)KeyModifiers.None;

        public Settings()
        {
            InitializeComponent();
        }

        public static void LoadSettings()
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey EasyLoUKey = SoftwareKey.OpenSubKey("EasyLoU", true);
            if (EasyLoUKey == null)
            {
                EasyLoUKey = SoftwareKey.CreateSubKey("EasyLoU", true);
            }

            StartScriptHotkey = (Keys)Enum.Parse(typeof(Keys), (string)EasyLoUKey.GetValue("StartScriptHotkey", "None"));
            StartScriptHotkeyModifiers = (int)EasyLoUKey.GetValue("StartScriptHotkeyModifiers", KeyModifiers.None);

            StopScriptHotkey = (Keys)Enum.Parse(typeof(Keys), (string)EasyLoUKey.GetValue("StopScriptHotkey", "None"));
            StopScriptHotkeyModifiers = (int)EasyLoUKey.GetValue("StopScriptHotkeyModifiers", KeyModifiers.None);

            StopAllScriptsHotkey = (Keys)Enum.Parse(typeof(Keys), (string)EasyLoUKey.GetValue("StopAllScriptsHotkey", "None"));
            StopAllScriptsHotkeyModifiers = (int)EasyLoUKey.GetValue("StopAllScriptsHotkeyModifiers", KeyModifiers.None);
        }

        public static void SaveSettings()
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("Software", true);

            RegistryKey EasyLoUKey = SoftwareKey.OpenSubKey("EasyLoU", true);
            if (EasyLoUKey == null)
            {
                EasyLoUKey = SoftwareKey.CreateSubKey("EasyLoU", true);
            }

            EasyLoUKey.SetValue("StartScriptHotkey", StartScriptHotkey);
            EasyLoUKey.SetValue("StartScriptHotkeyModifiers", StartScriptHotkeyModifiers);

            EasyLoUKey.SetValue("StopScriptHotkey", StopScriptHotkey);
            EasyLoUKey.SetValue("StopScriptHotkeyModifiers", StopScriptHotkeyModifiers);

            EasyLoUKey.SetValue("StopAllScriptsHotkey", StopAllScriptsHotkey);
            EasyLoUKey.SetValue("StopAllScriptsHotkeyModifiers", StopAllScriptsHotkeyModifiers);
        }

        public static void RegisterHotkeys(IntPtr Handle)
        {
            try
            {
                HotkeysWindowHandle = Handle;

                if (Settings.StartScriptHotkey != Keys.None)
                {
                    KeyboardHook.RegisterHotKey(Handle, 1, (int)Settings.StartScriptHotkeyModifiers, (int)Settings.StartScriptHotkey).ToString();
                }
                if (Settings.StopAllScriptsHotkey != Keys.None)
                {
                    KeyboardHook.RegisterHotKey(Handle, 2, (int)Settings.StopScriptHotkeyModifiers, (int)Settings.StopScriptHotkey).ToString();
                }
                if (Settings.StopAllScriptsHotkey != Keys.None)
                {
                    KeyboardHook.RegisterHotKey(Handle, 3, (int)Settings.StopAllScriptsHotkeyModifiers, (int)Settings.StopAllScriptsHotkey).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(MainForm.TheMainForm, "Error registering hotkeys: " + ex.ToString());
            }
        }

        public static void UnregisterHotkeys(IntPtr Handle)
        {
            try
            {
                KeyboardHook.UnregisterHotKey(Handle, 1);
                KeyboardHook.UnregisterHotKey(Handle, 2);
                KeyboardHook.UnregisterHotKey(Handle, 3);
            } catch (Exception ex)
            {
                MessageBoxEx.Show(MainForm.TheMainForm, "Error unregistering hotkeys: " + ex.ToString());
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            foreach (Keys k in Enum.GetValues(typeof(Keys)))
            {
                StartScriptHotkeyComboBox.Items.Add(k);
                StopScriptHotkeyComboBox.Items.Add(k);
                StopAllScriptsHotkeyComboBox.Items.Add(k);
            }

            StartScriptHotkeyComboBox.SelectedIndex = StartScriptHotkeyComboBox.FindStringExact(StartScriptHotkey.ToString());
            StartScriptHotkeyAltModifierCheckBox.Checked = (StartScriptHotkeyModifiers & (int)KeyModifiers.Alt) > 0;
            StartScriptHotkeyControlModifierCheckBox.Checked = (StartScriptHotkeyModifiers & (int)KeyModifiers.Control) > 0;
            StartScriptHotkeyShiftModifierCheckBox.Checked = (StartScriptHotkeyModifiers & (int)KeyModifiers.Shift) > 0;
            StartScriptHotkeyWindowsModifierCheckBox.Checked = (StartScriptHotkeyModifiers & (int)KeyModifiers.Windows) > 0;

            StopScriptHotkeyComboBox.SelectedIndex = StopScriptHotkeyComboBox.FindStringExact(StopScriptHotkey.ToString());
            StopScriptHotkeyAltModifierCheckBox.Checked = (StopScriptHotkeyModifiers & (int)KeyModifiers.Alt) > 0;
            StopScriptHotkeyControlModifierCheckBox.Checked = (StopScriptHotkeyModifiers & (int)KeyModifiers.Control) > 0;
            StopScriptHotkeyShiftModifierCheckBox.Checked = (StopScriptHotkeyModifiers & (int)KeyModifiers.Shift) > 0;
            StopScriptHotkeyWindowsModifierCheckBox.Checked = (StopScriptHotkeyModifiers & (int)KeyModifiers.Windows) > 0;

            StopAllScriptsHotkeyComboBox.SelectedIndex = StopAllScriptsHotkeyComboBox.FindStringExact(StopAllScriptsHotkey.ToString());
            StopAllScriptsHotkeyAltModifierCheckBox.Checked = (StopAllScriptsHotkeyModifiers & (int)KeyModifiers.Alt) > 0;
            StopAllScriptsHotkeyControlModifierCheckBox.Checked = (StopAllScriptsHotkeyModifiers & (int)KeyModifiers.Control) > 0;
            StopAllScriptsHotkeyShiftModifierCheckBox.Checked = (StopAllScriptsHotkeyModifiers & (int)KeyModifiers.Shift) > 0;
            StopAllScriptsHotkeyWindowsModifierCheckBox.Checked = (StopAllScriptsHotkeyModifiers & (int)KeyModifiers.Windows) > 0;
        }

        private void SettingsOkButton_Click(object sender, EventArgs e)
        {
            StartScriptHotkey = (Keys)Enum.Parse(typeof(Keys), StartScriptHotkeyComboBox.SelectedItem.ToString());
            StartScriptHotkeyModifiers = (int)KeyModifiers.None;
            if (StartScriptHotkeyAltModifierCheckBox.Checked) StartScriptHotkeyModifiers |= (int)KeyModifiers.Alt;
            if (StartScriptHotkeyControlModifierCheckBox.Checked) StartScriptHotkeyModifiers |= (int)KeyModifiers.Control;
            if (StartScriptHotkeyShiftModifierCheckBox.Checked) StartScriptHotkeyModifiers |= (int)KeyModifiers.Shift;
            if (StartScriptHotkeyWindowsModifierCheckBox.Checked) StartScriptHotkeyModifiers |= (int)KeyModifiers.Windows;

            StopScriptHotkey = (Keys)Enum.Parse(typeof(Keys), StopScriptHotkeyComboBox.SelectedItem.ToString());
            StopScriptHotkeyModifiers = (int)KeyModifiers.None;
            if (StopScriptHotkeyAltModifierCheckBox.Checked) StopScriptHotkeyModifiers |= (int)KeyModifiers.Alt;
            if (StopScriptHotkeyControlModifierCheckBox.Checked) StopScriptHotkeyModifiers |= (int)KeyModifiers.Control;
            if (StopScriptHotkeyShiftModifierCheckBox.Checked) StopScriptHotkeyModifiers |= (int)KeyModifiers.Shift;
            if (StopScriptHotkeyWindowsModifierCheckBox.Checked) StopScriptHotkeyModifiers |= (int)KeyModifiers.Windows;

            StopAllScriptsHotkey = (Keys)Enum.Parse(typeof(Keys), StopAllScriptsHotkeyComboBox.SelectedItem.ToString());
            StopAllScriptsHotkeyModifiers = (int)KeyModifiers.None;
            if (StopAllScriptsHotkeyAltModifierCheckBox.Checked) StopAllScriptsHotkeyModifiers |= (int)KeyModifiers.Alt;
            if (StopAllScriptsHotkeyControlModifierCheckBox.Checked) StopAllScriptsHotkeyModifiers |= (int)KeyModifiers.Control;
            if (StopAllScriptsHotkeyShiftModifierCheckBox.Checked) StopAllScriptsHotkeyModifiers |= (int)KeyModifiers.Shift;
            if (StopAllScriptsHotkeyWindowsModifierCheckBox.Checked) StopAllScriptsHotkeyModifiers |= (int)KeyModifiers.Windows;

            SaveSettings();

            UnregisterHotkeys(HotkeysWindowHandle);
            RegisterHotkeys(HotkeysWindowHandle);

            this.Close();
        }

        private void SettingsCancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
