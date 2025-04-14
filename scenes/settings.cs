using Godot;
using System;
using System.Security.Cryptography.X509Certificates;

public partial class settings : Node2D
{
	Button back;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		base._Ready();
		this.back = this.GetNode<Button>(new NodePath("MainContainer/VFlowContainer2/back_button"));
		this.back.Pressed += Back_Pressed;

	}

	public void Back_Pressed() {
        SceneTree st = this.GetTree();
        st.ChangeSceneToFile("res://scenes/main_menu.tscn");
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
