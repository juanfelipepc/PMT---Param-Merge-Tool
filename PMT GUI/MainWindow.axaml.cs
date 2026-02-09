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
        private string stateFilePath => Path.Combine(AppContext.BaseDirectory, "app_state.json");

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
                var state = new AppState
                {
                    GameFolderPath = gameFolderPath,
                    ModsFolderPath = modsFolderPath,
                    Mods = currentMods
                };

                var json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(stateFilePath, json);
                Log("Saved app state.");
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
                    var state = JsonSerializer.Deserialize<AppState>(json);

                    if (state != null)
                    {
                        gameFolderPath = state.GameFolderPath;
                        Log($"Game folder loaded: {gameFolderPath}.");
                        modsFolderPath = state.ModsFolderPath;
                        Log($"Mods folder loaded: {modsFolderPath}.");
                        currentMods = state.Mods ?? new List<ModEntry>();

                        ModListBox.ItemsSource = currentMods;
                        Log("Loaded app state.");
                    }
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
    
    public class AppState
    {
        public string GameFolderPath { get; set; }
        public string ModsFolderPath { get; set; }
        public List<ModEntry> Mods { get; set; } = new();
    }

}
