using Godot;
using System;
using System.IO;
using System.Text.Json;

public partial class story_menu : Node2D
{
	Button newGameB;
	Button loadGameB;
	Button continueGameB;
	Button back2menuB;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		newGameB = GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/new_game_button"));
		newGameB.Pressed += NewGame;
        loadGameB = GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/choose_save_button"));
        loadGameB.Pressed += LoadGame;
        continueGameB = GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/continue_button"));
        continueGameB.Pressed += ContinGame;
        back2menuB = GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/HBoxContainer/back2menu_button"));
        back2menuB.Pressed += B2M;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void NewGame() {
		GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
	}
	public void ContinGame() {
		//Загрузить последний файл сохранения и выгрузить в тмп
		GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
	}
	public void B2M() {		
		GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
	}
    public void LoadGame()
	{
        
        //Загрузить выбранный файл сохранения и выгрузить в тмп
        CreateWriteTmp();
        GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
    }
	public void CreateWriteTmp(string? inf = null) {
		string path = "res://saves/loadNow.json";
		if (!File.Exists(path)) {
			if (inf != null) {
                inf = JsonSerializer.Serialize(inf);
            }
			using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
			file.StoreString(inf ?? "[]");
		}
	}
}	
