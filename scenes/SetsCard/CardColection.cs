using Godot;
using Godot.NativeInterop;
using System;

public class ExampleCard
{
    public int manaC;
    public int hp;
    public int atc;
    public string desc;

    public ExampleCard(string a)
    {
        manaC = 10;
        hp = 8;
        atc = 3;
        if (a != null)
        {
            desc = a;
        }
        else { desc = ""; }
    }
}

public class ExamplePhisCard
{
    public GridContainer gc;
    public string descr;

    public ExamplePhisCard(string text)
    {
        gc = new GridContainer();
        ExampleCard ec = new ExampleCard(text);
        Label label = new Label();
        label.Text = ec.manaC.ToString();
        gc.AddChild(label);
        Label label1 = new Label();
        label1.Text = ec.hp.ToString();
        gc.AddChild(label1);
        Label label2 = new Label();
        label2.Text = ec.atc.ToString();
        gc.AddChild(label2);
        descr = ec.desc;
    }
}

public partial class CardColection : Node2D
{
    GridContainer gc { get; set; }
    Label description;
    Button b2m;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("Да работает коллекция, чё ты?");
        gc = GetNode<GridContainer>(new NodePath("MainContainer/VFlowContainer/allCards_GridContainer/cards_GridContainer"));
        description = GetNode<Label>(new NodePath("MainContainer/VFlowContainer2/cardDescriprtion_Label"));
        ExamplePhisCard[] exampleCards = new ExamplePhisCard[5];
        for (int i = 0; i < exampleCards.Length; i++)
        {
            exampleCards[i] = new ExamplePhisCard(i.ToString());
            gc.AddChild(exampleCards[i].gc);
            gc.GetChild(i).SetMeta("Description", exampleCards[i].descr);
            Control card = (GridContainer)gc.GetChild(i);
            card.MouseEntered += () => Enter(card);
        }
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
