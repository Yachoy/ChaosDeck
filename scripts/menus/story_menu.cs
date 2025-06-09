using Godot;
using System;
using System.IO;
using System.Text.Json;
using ThemeManager;

public partial class story_menu : Node
{
	[Export]
	Button newGameB;
	[Export]
	Button continueGameB;
	[Export]
	Button back2menuB;
	public override void _Ready()
	{
		newGameB.Pressed += NewGame;
		continueGameB.Pressed += ContinGame;
		back2menuB.Pressed += B2M;
		Theme themeManager = Themes.CreateMenuButtonTheme();
		var uiRoot = GetNode<Control>("MainContainer");
		if (uiRoot != null)
		{
			uiRoot.Theme = themeManager;
		}
	}

	public override void _Process(double delta)
	{
	}
	public void NewGame() {
		GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
	}
	public void ContinGame() {
		GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
	}
	public void B2M() {		
		GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
	}
    public void LoadGame()
	{
        CreateWriteTmp();
        GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
    }
	public void CreateWriteTmp(string? inf = null) {
		string path = "res://resources/Storage/loadNow.json";
		if (!File.Exists(path)) {
			if (inf != null) {
                inf = JsonSerializer.Serialize(inf);
            }
			using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
			file.StoreString(inf ?? "[]");
		}
	}
}	
