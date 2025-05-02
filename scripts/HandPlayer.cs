using Godot;
using System;
using System.Collections.Generic;

public partial class HandPlayer : Node3D
{
	[Export]
    private PackedScene _cardScene;
    [Export]
    private Node3D _start_pos_cards;

    [Export]
    private PackedScene _placeCardScene;
    [Export]
    private Node3D _start_pos_places;


    [Export]
    private bool IsAnotherTeam;

    [Export]
    private float _horizontalSpacingCards = 1.5f;
    [Export]
    private float _horizontalSpacingPlaces = 1.5f;

    public List<Node3D> cards = new List<Node3D>();
    public List<Node3D> places = new List<Node3D>();

    private CardPlayer _selected_card;

    private CardController cc;

    public override void _Ready()
    {
        if (_cardScene == null)
        {
            GD.PrintErr("Сцена карточки не назначена в CardSpawner!");
            return;
        }
        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен...");
        }
        SpawnStartCards();
        SpawnStartPlaces();
        if (IsAnotherTeam){
            cc.RegisterHandPlayer2(this);
        }else{
            cc.RegisterHandPlayer1(this);
        }

        cc.StartNewMatch();
    }

    private void SpawnStartPlaces(int _numberOfPlaces = 6){
        for (int i = 0; i < _numberOfPlaces; i++)
        {
         
            GD.Print($"Spawn {i}");
            Node3D cardInstance = _placeCardScene.Instantiate<Node3D>();
            Vector3 currentPosition = _start_pos_places.Position;
            currentPosition.X += i * _horizontalSpacingPlaces;
            cardInstance.Position = currentPosition;
            AddChild(cardInstance);
            CardPlace a = (CardPlace)cardInstance;
            if (IsAnotherTeam){
                a.SetIsAnotherTeam();
            }
            a.Connect(
                CardPlayer.SignalName.ObjectSelected,
                new Callable(this, MethodName.OnCardTryPut)
            );
            places.Add(cardInstance);
        }
    }

    private void SpawnStartCards(int _numberOfCards = 8)
    {
        for (int i = 0; i < _numberOfCards; i++)
        {
            GD.Print($"Spawn {i}");
            Node3D cardInstance = _cardScene.Instantiate<Node3D>();
            Vector3 currentPosition = _start_pos_cards.Position;
            currentPosition.X += i * _horizontalSpacingCards;
            cardInstance.Position = currentPosition;
            AddChild(cardInstance);
            CardPlayer a = (CardPlayer)cardInstance;
            if (IsAnotherTeam){
                a.SetIsAnotherTeam();
            }
            a.Connect(
                CardPlayer.SignalName.ObjectSelected,
                new Callable(this, MethodName.OnCardSelected)
            );

            cards.Add(cardInstance);
            
        }
    }

    private void OnCardSelected(CardPlayer selectedCard){
        _selected_card = selectedCard;
    }

    private void OnCardTryPut(CardPlace selectedPlaceCard){
        if(_selected_card is null) return;
        if (selectedPlaceCard.isAlreadyCardHerePumPumPum) return;
        _selected_card.is_placed = true;
        cc.TryMakeRound(_selected_card, selectedPlaceCard);
        _selected_card.MoveTo(selectedPlaceCard);

    }
}
