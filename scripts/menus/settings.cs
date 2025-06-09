using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
using ThemeManager;

[GlobalClass]
public partial class settings : Node
{
	[Export]
	Button back;
	[Export]
	CheckBox fullScreen;
	[Export]
	Control uiRoot;
	public static bool fs;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		back.Pressed += Back_Pressed;
        fullScreen.Toggled += FullScreen_Toggled;
		Theme theme = Themes.CreateMenuButtonTheme();
		uiRoot.Theme = theme;
    }

    private void FullScreen_Toggled(bool toggledOn)
    {
		
		if (toggledOn)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else 
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
        main_menu.Save("res://resources/storage/settings.json", "Fullscreen", main_menu.fullscr);
    }

    public void Back_Pressed() {
        GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
