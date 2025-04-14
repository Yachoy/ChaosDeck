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
        this.story = this.GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/story_mod_button"));
        this.story.Connect("story", _Pressed());
        this.duel = this.GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/duel_mod_button"));
        this.duel.Pressed += _Pressed;
        this.collection = this.GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/card_collection_button"));
        this.collection.Pressed += _Pressed;
        this.settings = this.GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/settings_button"));
        this.settings.Pressed += _Pressed;
        this.exit = this.GetNode<Button>(new NodePath("all_menu_BoxContainer/menu_BoxContainer/exit_button"));
        this.exit.Pressed += Exit_Pressed; 

    }
    private void _Pressed()
    {
        SceneTree st = this.GetTree();
        switch Button. ;
        //st.ChangeSceneToFile("res://scenes/settings.tscn");
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
