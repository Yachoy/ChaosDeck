using CCSpace;
using Godot;
using Godot.Collections;
using Godot.NativeInterop;
using System;
using System.Linq.Expressions;
using System.Net;

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
    [Export]
    Label description;
    [Export]
    Button b2m;

    [Export]
    Button saveButton;
    [Export]
    Button clearButton;
    [Export]
    private PackedScene _itemTemplateScene;
    [Export]
    private GridContainer _source_gridContainer;

    [Export]
    private GridContainer _deck_gridContainer;
    [Export]
    public TextEdit nameDeck;

    CardController cc;
    SqliteContoller sc;
    public override void _Ready()
    {
        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен... card_collection");
        }
        sc = GetNode<SqliteContoller>("/root/SqliteContoller");
        if (sc == null)
        {
            GD.PrintErr("SqliteContoller не загружен... card_collection");
        }


        GenerateSourceGrid();

        b2m.Pressed += B2M;
        saveButton.Pressed += SaveClicked;

        if (sc.DeckExists(cc.nameDeckToEdit))
        {
            System.Collections.Generic.Dictionary<string, byte> data = sc.GetCardsInDeck(cc.nameDeckToEdit);
            foreach (var sql_item in data)
            {
                foreach (var storage_item in cc.StorageCards.LoadedCards)
                {
                    if (sql_item.Key == storage_item.DefaultName)
                    {
                        AddElementIntoGridDeck(storage_item);
                    }
                }
            }
        }
        nameDeck.Text = cc.nameDeckToEdit;


        clearButton.Pressed += () => ClearGrid(_deck_gridContainer);
    }
    
    void SaveClicked()
    {
        if (nameDeck.Text == CardSetCheckerWindow.DEFAULT_NAME_DECK) { return; }
        if (sc.DeckExists(nameDeck.Text))
        {
            sc.DeleteDeck(nameDeck.Text);
        }
        GD.Print("Save deck ", nameDeck.Text);
        sc.AddDeck(nameDeck.Text);
        foreach (CardUI child in _deck_gridContainer.GetChildren())
        {
            sc.SetCardInDeck(nameDeck.Text, child.nameCard, 1);
        }
        
    }

    void AddElementIntoGridDeck(CardUI ui)
    {
        CardUI itemInstance = _itemTemplateScene.Instantiate<CardUI>();
        itemInstance.MouseRLClick += ClickOnTheElementDeck;
        itemInstance.ChangeTexture(ui.target.Texture);
        _deck_gridContainer.AddChild(itemInstance);

        itemInstance.nameCard = ui.nameCard;
        itemInstance.mana.Text = ui.mana.Text;
        itemInstance.damage.Text = ui.damage.Text;
        itemInstance.hp.Text = ui.hp.Text;
        itemInstance.type.Text = "0";
    }

    void AddElementIntoGridDeck(CardData ui)
    {
        CardUI itemInstance = _itemTemplateScene.Instantiate<CardUI>();
        itemInstance.MouseRLClick += ClickOnTheElementDeck;
        itemInstance.ChangeTexture(ui.ImageTexture);
        _deck_gridContainer.AddChild(itemInstance);

        itemInstance.nameCard = ui.DefaultName;
        itemInstance.mana.Text = ui.DefaultCost.ToString();
        itemInstance.damage.Text = ui.DefaultDamage.ToString();
        itemInstance.hp.Text = ui.DefaultHp.ToString();
        itemInstance.type.Text = "0";
    }

    void AddElementIntoGridSource(CardData cd)
    {
        CardUI itemInstance = _itemTemplateScene.Instantiate<CardUI>();
        itemInstance.description = $"{cd.DefaultName}\n Существо {cd.Element}, стоимость {cd.DefaultCost} \n Атака {cd.DefaultDamage}, жизнь {cd.DefaultHp}";
        itemInstance.MouseRLClick += ClickOnTheElementSource;
        itemInstance.target.MouseEntered += () => WriteDescription(itemInstance.description);

        itemInstance.ChangeTexture(cd.ImageTexture);
        _source_gridContainer.AddChild(itemInstance);

        itemInstance.nameCard = cd.DefaultName;
        itemInstance.mana.Text = cd.DefaultCost.ToString();
        itemInstance.damage.Text = cd.DefaultDamage.ToString();
        itemInstance.hp.Text = cd.DefaultHp.ToString();
        itemInstance.type.Text = "0";
    }


    void ClickOnTheElementSource(CardUI card, MouseButton mb)
    {
        if (mb == MouseButton.Left)
        {
            if (IsGridHaveUICardWithName(_deck_gridContainer, card.nameCard)){
                GD.Print("Ignore ", card.nameCard);
                return; }
            AddElementIntoGridDeck(card);
        }
        GetViewport().SetInputAsHandled();
    }

    void ClickOnTheElementDeck(CardUI card, MouseButton mb)
    {
        if (mb == MouseButton.Left) {
        } else if (mb == MouseButton.Right) {
            card.QueueFree();   
        }
        GetViewport().SetInputAsHandled();
    }

    public void GenerateSourceGrid()
    {
        ClearGrid(_source_gridContainer);

        foreach (CardData cd in cc.StorageCards.LoadedCards)
        {
            AddElementIntoGridSource(cd);
        }
    }

    public void ClearGrid(GridContainer grid)
    {
        foreach (Node child in grid.GetChildren())
        {
            child.QueueFree();
        }
    }
    public bool IsGridHaveUICardWithName(GridContainer gc, string name)
    {
        foreach (CardUI child in gc.GetChildren())
        {
            if (child.nameCard == name)
            {
                return true;
            }
        }
        return false;
    } 


    public void Enter(Control control)
    {
        description.Text = control.GetMeta("Description").AsString();
    }

    public void B2M()
    {
        GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
    }

    public void WriteDescription(string desc) {
        description.Text = desc;
    }
}
