using Godot;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Godot.Collections;
using System.ComponentModel;
using ThemeManager;

public partial class main_menu : Node
{
    Button story;
    Button duel;
    Button collection;
    Button sett;
	Button exit;
    public static bool fullscr;
    Window cardSetChoose;
    
    // Called when the node enters the scene tree for the first time.
    
    public override void _Ready()
	{
        GD.Print("Работаю");
        cardSetChoose = GetNode<Window>(new NodePath("CardSetChecker_Window"));
        cardSetChoose.Visible = false;
        story = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/story_mod_button"));
        story.Pressed += _PressedStory;
        duel = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/duel_mod_button"));
        duel.Pressed += _PressedDuel;
        collection = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/card_collection_button"));
        collection.Pressed += _PressedCollection;
        sett = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/settings_button"));
        sett.Pressed += _PressedSettings;
        exit = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/exit_button"));
        exit.Pressed += Exit_Pressed;
        Read();
        Theme themeManager = Themes.CreateMenuButtonTheme();
        var uiRoot = GetNode<Control>("all_menu_BoxContainer");
        if (uiRoot != null)
        {
            uiRoot.Theme = themeManager;
        }
    }
    public static void Read() {
        fullscr = Load("res://resources/Storage/settings.json", "Fullscreen");
        if (fullscr)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        }
    }
    public static bool Load(string fullPath, string jsonKey)
    {
        using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
        string jsonString = file.GetAsText();
        Variant parsedResult = Json.ParseString(jsonString);
        Dictionary data = parsedResult.As<Dictionary>();
        Variant valueVariant = data[jsonKey];
        bool result = valueVariant.As<bool>();

        return result;
    }
    public static Dictionary LoadCardSet(string fullPath) 
    {
        using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
        string jsonString = file.GetAsText();
        Variant parsedResult = Json.ParseString(jsonString);
        Dictionary result = parsedResult.As<Dictionary>();
        
        return result;
    }

    private void _PressedCollection()
    {
        cardSetChoose.Visible = true;
    }

    private void _PressedStory()
    {

        GetTree().ChangeSceneToFile("res://scenes/Menu/story_menu.tscn");
    }

    private void _PressedDuel()
    {
        GD.Print("Ля, ну жуй!");
    }

    private void _PressedSettings()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menu/settings.tscn");
        
    }

    public static void Save(string fullPath, string jsonKey, bool param)
    {
        var data = new Dictionary { { jsonKey, param } };
        string jsonString = Json.Stringify(data, "\t");
        using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(jsonString);       
    }
    


    private void Exit_Pressed()
    {
        Save("res://resources/Storage/settings.json", "Fullscreen", fullscr);
        GetTree().Quit();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
    }
   
}
