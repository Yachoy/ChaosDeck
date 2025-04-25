using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class main_menu : Node2D
{
    Button story;
    Button duel;
    Button collection;
    Button settings;
	Button exit;
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
        settings = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/settings_button"));
        settings.Pressed += _PressedSettings;
        exit = GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/exit_button"));
        exit.Pressed += Exit_Pressed; 

    }

    private void _PressedCollection()
    {
        throw new NotImplementedException();
    }

    private async void _PressedStory()
    {
        GetTree().ChangeSceneToFile("res://scenes/story_menu.tscn");
        
    }

    private void _PressedDuel()
    {
        throw new NotImplementedException();
    }

    private void _PressedSettings()
    {
        GetTree().ChangeSceneToFile("res://scenes/settings.tscn");
        
    }

    private void Exit_Pressed()
    {
        GetTree().Quit();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        GD.Print(@event.AsText());    
       switch (@event){
            case InputEventKey eventKey:
                if (eventKey.Pressed && eventKey.Keycode == Key.Escape)
                {
                    GD.Print("не работаю");
                    GetTree().Quit();
                }
                break;
            case InputEventMouseButton eventMouse:
                
                if (false)
                {
                    
                    GD.Print("Левую кнопку нажал!");
                    GetTree().Quit();
                }
                break;
            
        }
    }
   
}
