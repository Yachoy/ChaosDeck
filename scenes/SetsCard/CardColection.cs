using Godot;
using Godot.NativeInterop;
using System;

public partial class CardColection : Node2D
{
    GridContainer gc { get; set; }
    Label description;
    Button b2m;
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gc = GetNode<GridContainer>(new NodePath("MainContainer/VFlowContainer/allCards_GridContainer/cards_GridContainer"));
        description = GetNode<Label>(new NodePath("MainContainer/VFlowContainer2/cardDescriprtion_Label"));
        
        b2m = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/HBoxContainer/menu_VBoxContainer/back2menu_button"));
        b2m.Pressed += B2M;
    }

    public void Enter(Control control)
    {
        description.Text = control.GetMeta("Description").AsString();
    }
    public void B2M()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        var s = delta;
    }
}
