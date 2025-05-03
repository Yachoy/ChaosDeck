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

public partial class main_menu : Node2D
{
    Button story;
    Button duel;
    Button collection;
    Button sett;
	Button exit;
    public static bool fullscr;
    
    // Called when the node enters the scene tree for the first time.
    
    public override void _Ready()
	{
        GD.Print("Работаю");
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
        Theme themeManager = CreateMenuButtonTheme();
        var uiRoot = GetNode<Control>("all_menu_BoxContainer");
        if (uiRoot != null)
        {
            uiRoot.Theme = themeManager;
            GD.Print("Проверка проверки");
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

    private void _PressedCollection()
    {
        GetTree().ChangeSceneToFile("res://scenes/SetsCard/card_colection.tscn");
    }

    private void _PressedStory()
    {

        GetTree().ChangeSceneToFile("res://scenes/Menu/story_menu.tscn");
    }

    private void _PressedDuel()
    {
        throw new NotImplementedException();
    }

    private void _PressedSettings()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menu/settings.tscn");
        
    }

    public static void Save(string fullPath, string jsonKey)
    {
        var data = new Dictionary { { jsonKey, fullscr } };
        string jsonString = Json.Stringify(data, "\t");
        using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Write);
        file.StoreString(jsonString);       
    }
    


    private void Exit_Pressed()
    {
        Save("res://resources/Storage/settings.json", "Fullscreen");
        GetTree().Quit();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
    }
    public Theme CreateMenuButtonTheme()
    {
        Theme theme = new Theme();
        theme.SetColor("font_color", "Button", new Color(0.0f, 0.0f, 0.0f, 1f));
        Font customFont = ResourceLoader.Load<FontFile>("res://resources/menuThemes/black-and-white-picture-cyrillic.ttf");
        theme.SetFont("font", "Label", customFont);
        theme.SetFontSize("font_size", "Label", 40);
        theme.SetFont("font", "Button", customFont);
        theme.SetFontSize("font_size", "Button", 40);

        var normalButtonStyle = new StyleBoxFlat();
        normalButtonStyle.BgColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        normalButtonStyle.CornerRadiusTopLeft = 5;
        normalButtonStyle.CornerRadiusTopRight = 5;
        normalButtonStyle.CornerRadiusBottomLeft = 5;
        normalButtonStyle.CornerRadiusBottomRight = 5;
        normalButtonStyle.ContentMarginLeft = 10; // Отступ слева для текста/иконки
        normalButtonStyle.ContentMarginRight = 10;
        normalButtonStyle.ContentMarginTop = 5;
        normalButtonStyle.ContentMarginBottom = 5;
        theme.SetStylebox("normal", "Button", normalButtonStyle);

        // Стиль для кнопки при наведении
        var hoverButtonStyle = new StyleBoxFlat();
        hoverButtonStyle.BgColor = new Color((238/255), (209/255), (141/255), 0.4f);
        hoverButtonStyle.SetCornerRadiusAll(5); // Установить все углы сразу
        hoverButtonStyle.ContentMarginLeft = 10;
        hoverButtonStyle.ContentMarginRight = 10;
        hoverButtonStyle.ContentMarginTop = 5;
        hoverButtonStyle.ContentMarginBottom = 5;
        theme.SetStylebox("hover", "Button", hoverButtonStyle);

        return theme;
    }
}
