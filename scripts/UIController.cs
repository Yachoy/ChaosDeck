using Godot;
using System;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using Godot.Collections;
using System.Data.SqlTypes;


public partial class UIController : Node
{
	private Vector2I _initialWindowSize_when_windowed = new Vector2I(1152, 648);

	readonly string FILE_SETTINGS = "res://resources/Storage/settings.json";

	public override void _Ready()
	{
		ReadFullscreenStateAndUpdate();
	}


	public static T Load<[MustBeVariant] T>(string fullPath, string jsonKey) 
	{
		using var file = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();
		Variant parsedResult = Json.ParseString(jsonString);
		Dictionary data = parsedResult.As<Dictionary>();
		Variant valueVariant = data[jsonKey];
		T result = valueVariant.As<T>();

		return result;
	}

	public static void Save<[MustBeVariant] T>(string fullPath, string jsonKey, T param)
	{
		var data = new Dictionary();

		if (Godot.FileAccess.FileExists(fullPath)) // check path and content
		{
			using var readFile = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Read);
			if (readFile != null)
			{
				string content = readFile.GetAsText();
				if (!string.IsNullOrEmpty(content))
				{
					Variant parsedResult = Json.ParseString(content);
					if (parsedResult.VariantType == Variant.Type.Dictionary)
					{
						data = parsedResult.As<Dictionary>();
					}
				}
			}
		}

		data[jsonKey] = Variant.From(param);

		var dirPath = fullPath.GetBaseDir();
		if (!DirAccess.DirExistsAbsolute(dirPath))
		{
			DirAccess.MakeDirRecursiveAbsolute(dirPath);
		}

		using var writeFile = Godot.FileAccess.Open(fullPath, Godot.FileAccess.ModeFlags.Write);
		if (writeFile == null)
		{
			var error = Godot.FileAccess.GetOpenError();
			GD.PrintErr($"Failed to write to file '{fullPath}'. Error: {error}");
			return;
		}

		string jsonString = Json.Stringify(data, "\t");
		writeFile.StoreString(jsonString);

		GD.Print("Write to ", jsonKey, " ", param);
    }

	public void ReadFullscreenStateAndUpdate() =>
		SetFullscreen(Load<bool>(FILE_SETTINGS, "Fullscreen"));


	public void SetFullscreen(bool state)
	{
		if (state)
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
		}
		else
		{
			DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
			DisplayServer.WindowSetSize(_initialWindowSize_when_windowed);
		}
		Save(FILE_SETTINGS, "Fullscreen", state);
	}

}
