using Godot;
using System;

public partial class CardSetCheckerWindow : Window
{
    Button creator;
    Button changer;
    Button deleter;
    Button backButton;
    Button enterButton;
    VBoxContainer cardSetList;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        creator = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/new_cardSet_Button"));
        creator.Pressed += Update;
        changer = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/change_cardSet_Button"));
        changer.Pressed += Update;
        deleter = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/delete_cardSet_Button"));
        deleter.Pressed += Update;
        backButton = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/exit_Button"));
        backButton.Pressed += Update;
        enterButton = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/start_Button"));
        enterButton.Pressed += Update;
        cardSetList = GetNode<VBoxContainer>(new NodePath("VBoxContainer/VBoxContainer"));

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    public void Update() { }
}
