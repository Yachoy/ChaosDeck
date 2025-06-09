using Godot;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public partial class CardSetCheckerWindow : Window
{
    Button creator;
    Button changer;
    Button deleter;
    Button backButton;
    Button enterButton;
    VBoxContainer cardSetList;

    [Export]
    ItemList list;

    string selected_text;
    CardController cc;

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


        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен...");
        }

        CloseRequested += Exit;

//        list.ItemSelected += Selected;
//        list.ItemActivated += Enter;
    }

    void Selected(long index) {
        selected_text = list.GetItemText((int)index);
    }

    void Enter(long index)
    {
        GD.Print("Choice deck", list.GetItemText((int)index));
    }

    public void Exit()
    {
        Hide();
    }
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
