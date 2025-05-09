using CCSpace;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;

struct card 
{
    public int hp;
    public int atac;
    public int mana;
    public string name;
    public string desc;
    public string element;
}

public partial class CardColection : Node
{
    GridContainer gc { get; set; }
    Label description;
    Button b2m;
    CardLoader loader;
    Node2D[] allCards;
    string databasePath;
    Button chaosButton;
    Button destinyButton;
    Button luckButton;
    Button lifeButton;
    
    
    

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        gc = GetNode<GridContainer>(new NodePath("MainContainer/VFlowContainer/allCards_GridContainer/cards_GridContainer"));
        description = GetNode<Label>(new NodePath("MainContainer/VFlowContainer2/cardDescriprtion_Label"));
        //loader = new CardLoader("res://resources/CardsStorage/cards.json", );
        //loader._
        b2m = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/HBoxContainer/menu_VBoxContainer/back2menu_button"));
        b2m.Pressed += B2M;
        VisualiseAllCards("Chaos");
        chaosButton = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/elements_HBoxContainer/Chaos_Button"));
        chaosButton.Pressed += () => VisualiseAllCards("Chaos");
        destinyButton = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/elements_HBoxContainer/Destiny_Button"));
        destinyButton.Pressed += () => VisualiseAllCards("Destiny");
        luckButton = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/elements_HBoxContainer/Luck_Button"));
        luckButton.Pressed += () => VisualiseAllCards("Luck");
        lifeButton = GetNode<Button>(new NodePath("MainContainer/VFlowContainer/elements_HBoxContainer/Life_Button"));
        lifeButton.Pressed += () => VisualiseAllCards("Life");
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

    public void VisualiseAllCards(string element)
    {
        CardController cc = GetNode<CardController>("/root/CardController");
        foreach (CardData card in cc.StorageCards.LoadedCards) 
        {
            if (element == card.Element)
            {
                CardUI cardUI = new CardUI();
                Label crutch = new Label();
                crutch.Text = card.DefaultCost.ToString();
                cardUI.mana = crutch;
                crutch.Text = card.DefaultDamage.ToString();
                cardUI.damage = crutch;
                crutch.Text = card.DefaultHp.ToString();
                cardUI.hp = crutch;
                cardUI.ChangeTexture(card.ImageTexture.ResourcePath);
                gc.AddChild(cardUI);
                cardUI.description = $"{card.DefaultName}\n Существо {card.Element}, стоимость {card.DefaultCost} \n Атака {card.DefaultDamage}, жизнь {card.DefaultHp}";
                cardUI.Control.MouseEntered += () => WriteDescription(cardUI.description);
            }
        }
    }

    public void WriteDescription(string desc) {
        description.Text = desc;
    }
}
