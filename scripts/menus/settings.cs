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
	UIController uic;
	
	public override void _Ready()
	{
		back.Pressed += Back_Pressed;
		fullScreen.Toggled += FullScreen_Toggled;
		Theme theme = Themes.CreateMenuButtonTheme();
		uiRoot.Theme = theme;

		uic = GetNode<UIController>("/root/UiController");
		if (uic == null)
		{
			GD.PrintErr("UiController не загружен...");
		}
	}

    private void FullScreen_Toggled(bool toggledOn)
    {
		uic.SetFullscreen(toggledOn);
    }

    public void Back_Pressed() {
        GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
    }
}
