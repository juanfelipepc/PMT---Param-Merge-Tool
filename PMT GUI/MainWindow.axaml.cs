using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace PMT_GUI
{
    public partial class MainWindow : Window
    {
        private string gameFolderPath;
        private string modsFolderPath;
        private List<ModEntry> currentMods = new List<ModEntry>();

        private string stateFilePath => Path.Combine(AppContext.BaseDirectory, "mods_state.json");

        public MainWindow()
        {
            InitializeComponent();

            LoadState();

            SetGameFolderButton.Click += async (_, __) =>
            {
                var dialog = new OpenFolderDialog();
                var result = await dialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(result))
                {
                    gameFolderPath = result;
                    Log($"Game folder set: {gameFolderPath}");
                }
            };

            SetModsFolderButton.Click += async (_, __) =>
            {
                var dialog = new OpenFolderDialog();
                var result = await dialog.ShowAsync(this);
                if (!string.IsNullOrEmpty(result))
                {
                    modsFolderPath = result;
                    Log($"Mods folder set: {modsFolderPath}");
                    UpdateModsList();
                }
            };

            UpdateModsButton.Click += (_, __) => UpdateModsList();

            StartButton.Click += (_, __) =>
            {
                var selectedMods = GetSelectedMods();
                Log("Start pressed. Selected mods: " + string.Join(", ", selectedMods));
            };

            this.Closing += (_, __) => SaveState();
        }

        private void UpdateModsList()
        {
            if (string.IsNullOrEmpty(modsFolderPath) || !Directory.Exists(modsFolderPath))
            {
                Log("Mods folder not set or invalid.");
                return;
            }

            var newMods = new List<ModEntry>();

            foreach (var dir in Directory.GetDirectories(modsFolderPath))
            {
                var name = Path.GetFileName(dir);
                var existing = currentMods.FirstOrDefault(m => m.Name == name);

                if (existing != null)
                {
                    newMods.Add(existing);
                }
                else
                {
                    newMods.Add(new ModEntry { Name = name, IsChecked = true });
                }
            }

            currentMods = newMods;
            ModListBox.ItemsSource = currentMods;

            Log("Mods list updated (preserving checked state).");
        }

        private void Log(string message)
        {
            ConsoleBox.Text += message + "\n";
        }

        private List<string> GetSelectedMods()
        {
            return currentMods.Where(m => m.IsChecked).Select(m => m.Name).ToList();
        }

        private void SaveState()
        {
            try
            {
                var json = JsonSerializer.Serialize(currentMods);
                File.WriteAllText(stateFilePath, json);
                Log("Saved mod checkbox state.");
            }
            catch (Exception ex)
            {
                Log("Error saving state: " + ex.Message);
            }
        }

        private void LoadState()
        {
            try
            {
                if (File.Exists(stateFilePath))
                {
                    var json = File.ReadAllText(stateFilePath);
                    currentMods = JsonSerializer.Deserialize<List<ModEntry>>(json) ?? new List<ModEntry>();
                    ModListBox.ItemsSource = currentMods;
                    Log("Loaded mod checkbox state.");
                }
            }
            catch (Exception ex)
            {
                Log("Error loading state: " + ex.Message);
            }
        }
    }

    public class ModEntry
    {
        public string Name { get; set; }
        public bool IsChecked { get; set; }
    }
}
