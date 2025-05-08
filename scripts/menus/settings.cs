using Godot;
using System;
using System.Security.Cryptography.X509Certificates;
using ThemeManager;

[GlobalClass]
public partial class settings : Node
{
	Button back;
	CheckBox fullScreen;
	public static bool fs;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		back = GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/back2menu_button"));
		back.Pressed += Back_Pressed;
		fullScreen = GetNode<CheckBox>(new NodePath("MainContainer/VFlowContainer/FullScrean_CheckBox"));
        fullScreen.Toggled += FullScreen_Toggled;
		fs = main_menu.fullscr;
		Theme theme = Themes.CreateMenuButtonTheme();
		var uiRoot = GetNode<Control>(new NodePath("MainContainer"));
		uiRoot.Theme = theme;
		if (fs) {
			fullScreen.ButtonPressed = true;
		}
    }

    private void FullScreen_Toggled(bool toggledOn)
    {
		fs = toggledOn;
		
		if (toggledOn)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else 
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
		}
		main_menu.fullscr = toggledOn;
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
