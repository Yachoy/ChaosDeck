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

    CardController cc;

    public override void _Ready()
    {
        cardSetChoose.Hide();
        story.Pressed += _PressedStory;
        duel.Pressed += _PressedDuel;
        collection.Pressed += _PressedCollection;
        sett.Pressed += _PressedSettings;
        exit.Pressed += Exit_Pressed;
        Theme themeManager = Themes.CreateMenuButtonTheme();
        var uiRoot = GetNode<HSplitContainer>(new NodePath("HSplitContainer"));
        if (uiRoot != null)
        {
            uiRoot.Theme = themeManager;
        }

        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен... main_menu");
        }

    }

    private void _PressedStory()
    {
        cc.SetDefaultDeck(cc.StoryDeck);
        GetTree().ChangeSceneToFile("res://scenes/Menu/story_menu.tscn");
    }
    
    private void _PressedCollection() => cardSetChoose.Show();
    private void _PressedDuel() => cardSetChoose.Show();
    private void _PressedSettings() => GetTree().ChangeSceneToFile("res://scenes/Menu/settings.tscn");
    private void Exit_Pressed() => GetTree().Quit();
   
}
