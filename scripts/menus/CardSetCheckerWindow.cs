using CCSpace;
using Godot;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

public partial class CardSetCheckerWindow : Window
{
    [Export]
    Button creator;
    [Export]
    Button changer;
    [Export]
    Button deleter;
    [Export]
    Button backButton;
    [Export]
    Button enterButton;

    [Export]
    ItemList list;

    string selected_text;
    CardController cc;

    SqliteContoller sc;

    public static readonly string DEFAULT_NAME_DECK = "NewDeck(unsaved)";

    public override void _Ready()
    {

        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен...");
        }

        sc = GetNode<SqliteContoller>("/root/SqliteContoller");
        if (sc == null)
        {
            GD.PrintErr("SqliteContoller не загружен... card_collection");
        }

        foreach (var i in sc.GetAllDeckNames())
        {
            list.AddItem(i);
        }

        CloseRequested += Exit;

        list.ItemSelected += Selected;
        list.ItemActivated += Enter;
        creator.Pressed += () => list.AddItem(DEFAULT_NAME_DECK);
        changer.Pressed += EditDeck;
        enterButton.Pressed += ChoiseDeck;
    }


    void ChoiseDeck()
    {
        if (selected_text == "" || !sc.DeckExists(selected_text))
        {
            GD.Print("Select deck to start, or this deck doesn't exists in bd");
            return;
        }
        var sqliteData = sc.GetCardsInDeck(selected_text);
        CardSetData csd = new CardSetData();
        foreach (var sqlite_item in sqliteData)
        {
            foreach (var storage_item in cc.StorageCards.LoadedCards)
            {
                if (sqlite_item.Key == storage_item.DefaultName)
                {
                    csd.TryAdd(storage_item);
                }
                else
                {
                    GD.Print("Its may be problem with generating deck from bd... ");
                }
            }
        }
        cc.SetDefaultDeck(csd);
        GetTree().ChangeSceneToFile("res://scenes/game_scene.tscn");
    }

    void EditDeck()
    {
        int idx = list.GetSelectedItems()[0];
        cc.nameDeckToEdit = list.GetItemText(idx);
        GetTree().ChangeSceneToFile("res://scenes/SetsCard/card_colection.tscn");
    }

    void Selected(long index) {
        selected_text = list.GetItemText((int)index);
    }

    void Enter(long index)
    {
        GD.Print("Choice deck", list.GetItemText((int)index));
        cc.nameDeckToEdit = list.GetItemText((int)index);
        GetTree().ChangeSceneToFile("res://scenes/SetsCard/card_colection.tscn");
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

    public void Quit() 
    {
        this.Hide();
    }
}
