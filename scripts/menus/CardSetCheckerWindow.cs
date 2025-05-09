using Godot;
using System;
using System.Collections.Generic;

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
        creator.Pressed += () => EnterInCardSet(null);
        changer = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/change_cardSet_Button"));
        changer.Pressed += Update;
        deleter = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/delete_cardSet_Button"));
        deleter.Pressed += Update;
        backButton = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/exit_Button"));
        backButton.Pressed += Update;
        enterButton = GetNode<Button>(new NodePath("VBoxContainer/HFlowContainer/start_Button"));
        enterButton.Pressed += Update;
        cardSetList = GetNode<VBoxContainer>(new NodePath("VBoxContainer/VBoxContainer"));
        //Поправить, когда будет сохранение наборов карт
        //UploadCardSets(*СПИСОК СТРОК*);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
    public void Update() { }

    public void EnterInCardSet(string? cardSetName) {
        GetTree().ChangeSceneToFile("res://scenes/SetsCard/card_colection.tscn");
    }

    public void UploadCardSets(List<string> sets) {
        foreach (string setName in sets) {
            Label lb = new Label();
            lb.Text = setName;
            cardSetList.AddChild(lb);
        }
    }
    public void Quit() 
    {
        this.Hide();
    }
}
