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
    [Export]
    Button story;
    [Export]
    Button duel;
    [Export]
    Button collection;
    [Export]
    Button sett;
    [Export]
	Button exit;
    public static bool fullscr;
    [Export]
    Window cardSetChoose;
    
    // Called when the node enters the scene tree for the first time.
    
    public override void _Ready()
	{
        GD.Print("Работаю");
        cardSetChoose.Hide();
        story.Pressed += _PressedStory;
        duel.Pressed += _PressedDuel;
        collection.Pressed += _PressedCollection;
        sett.Pressed += _PressedSettings;
        exit.Pressed += Exit_Pressed;
        Read();
        Theme themeManager = Themes.CreateMenuButtonTheme();
        var uiRoot = GetNode<HSplitContainer>(new NodePath("HSplitContainer"));
        if (uiRoot != null)
        {
            GD.Print("AAAAAAAAAAAAAAAA");
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

    private void _PressedCollection() => cardSetChoose.Show();


    private void _PressedStory()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menu/story_menu.tscn");
    }

    private void _PressedDuel()
    {
        cardSetChoose.Show();
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
