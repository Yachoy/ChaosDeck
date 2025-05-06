using CCSpace;
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
	[Export]
	private float _shiftDuration = 0.3f; // Длительность анимации сдвига

    [Export]
    public Button btnEndround;
	[Export]
    public Panel panelPTP;

	// Массив для карт в руке фиксированного размера
	private const int MAX_CARDS_IN_HAND = 8;
	public Node3D[] cards = new Node3D[MAX_CARDS_IN_HAND];

	// Массив для мест на поле
	private const int MAX_PLACES = 6;
	public CardPlace[] places = new CardPlace[MAX_PLACES];

	private CardPlayer _selected_card;

	private CardController cc;

	public override void _Ready()
	{
		if (_cardScene == null)
		{
			GD.PrintErr("Сцена карточки не назначена в CardSpawner!");
			return;
		}
		if (_placeCardScene == null)
		{
			GD.PrintErr("Сцена места для карты не назначена в CardSpawner!");
			return;
		}

		cc = GetNode<CardController>("/root/CardController");
		if (cc == null)
		{
			GD.PrintErr("CardController не загружен...");
		}

		SpawnStartCards();
		SpawnStartPlaces();

		if (cc != null)
		{
			if (IsAnotherTeam)
			{
				cc.RegisterHandPlayer2(this);
			}
			else
			{
				cc.RegisterHandPlayer1(this);
			}
            cc.RegisterButtonEndRound(btnEndround);
			cc.RegisterMenuParameters((ParamatersTwoPlayers)panelPTP);
            //TODO: Need to replace call to start match
			cc.StartNewMatch(cc.DebugCardSet, cc.DebugCardSet);
		}
	}

	private void SpawnStartPlaces(int _numberOfPlaces = MAX_PLACES)
	{
		if (_numberOfPlaces > MAX_PLACES)
		{
			GD.PrintErr($"Requested number of places ({_numberOfPlaces}) exceeds max allowed ({MAX_PLACES}). Spawning {MAX_PLACES}.");
			_numberOfPlaces = MAX_PLACES;
		}

		for (int i = 0; i < _numberOfPlaces; i++)
		{
			GD.Print($"Spawn place {i}");
			Node3D placeInstance = _placeCardScene.Instantiate<Node3D>();
			Vector3 currentPosition = _start_pos_places.Position;
			currentPosition.X += i * _horizontalSpacingPlaces;
			placeInstance.Position = currentPosition;
			AddChild(placeInstance);

			if (placeInstance is CardPlace cardPlace)
			{
				if (IsAnotherTeam)
				{
					cardPlace.SetIsAnotherTeam();
				}
				cardPlace.Connect(
					CardPlayer.SignalName.ObjectSelected, // Возможно, здесь должен быть сигнал от CardPlace?
					new Callable(this, MethodName.OnCardTryPut)
				);
				places[i] = cardPlace; // Добавляем в массив мест
			}
			else
			{
				GD.PrintErr("Instantiated place scene is not of type CardPlace!");
			}
		}
	}

	private void SpawnStartCards(int _numberOfCards = MAX_CARDS_IN_HAND)
	{
		if (_numberOfCards > MAX_CARDS_IN_HAND)
		{
			GD.PrintErr($"Requested number of cards ({_numberOfCards}) exceeds max allowed ({MAX_CARDS_IN_HAND}). Spawning {MAX_CARDS_IN_HAND}.");
			_numberOfCards = MAX_CARDS_IN_HAND;
		}

		for (int i = 0; i < _numberOfCards; i++)
		{
			GD.Print($"Spawn card {i}");
			Node3D cardInstance = _cardScene.Instantiate<Node3D>();
			// Начальная позиция будет определена функцией балансировки
			AddChild(cardInstance);

			if (cardInstance is CardPlayer cardPlayer)
			{
				if (IsAnotherTeam)
				{
					cardPlayer.SetIsAnotherTeam();
				}
				cardPlayer.Connect(
					CardPlayer.SignalName.ObjectSelected,
					new Callable(this, MethodName.OnCardSelected)
				);
				cards[i] = cardInstance;
			}
			else
			{
				GD.PrintErr("Instantiated card scene is not of type CardPlayer!");
				cardInstance.QueueFree();
			}
		}
		// После спавна всех начальных карт, перебалансируем их
		BalanceCardsInHand();
	}

	private void OnCardSelected(CardPlayer selectedCard)
	{
		_selected_card = selectedCard;
		GD.Print($"Card selected: {selectedCard.Name}");
		// Здесь можно добавить логику подсветки выбранной карты, если нужно
	}

	private void OnCardTryPut(CardPlace selectedPlaceCard)
	{
		if (_selected_card == null)
		{
			GD.Print("No card selected.");
			return;
		}
		if (selectedPlaceCard.isAlreadyCardHerePumPumPum)
		{
			GD.Print("Place already occupied.");
			return;
		}

		GD.Print($"Trying to place card {_selected_card.Name} on place {selectedPlaceCard.Name}");

		// Удаляем карту из руки
		RemoveCardFromHand(_selected_card);

		_selected_card.is_placed = true;
		_selected_card.MoveTo(selectedPlaceCard);
		selectedPlaceCard.isAlreadyCardHerePumPumPum = true;

        cc.ProcessCardPlacement(_selected_card, selectedPlaceCard);

		_selected_card = null;
	}

	// Удаляет карту из массива руки и запускает перебалансировку
	private void RemoveCardFromHand(CardPlayer cardToRemove)
	{
		for (int i = 0; i < cards.Length; i++)
		{
			if (cards[i] == cardToRemove)
			{
				GD.Print($"Removing card {cardToRemove.Name} from hand at index {i}");
				cards[i] = null; 
				BalanceCardsInHand();
				return;
			}
		}
	}

	public void AddCardToHand(CardPlayer newCard)
	{

		for (int i = 0; i < cards.Length; i++)
		{
			if (cards[i] == null)
			{
				GD.Print($"Adding new card {newCard.Name} to hand at index {i}");
				cards[i] = newCard;
				AddChild(newCard); // Добавляем новый узел в дерево сцены
				newCard.Connect( // Подключаем сигнал выбора карты
					CardPlayer.SignalName.ObjectSelected,
					new Callable(this, MethodName.OnCardSelected)
				);
				if (IsAnotherTeam)
				{
					newCard.SetIsAnotherTeam(); // Если нужно настроить сторону
				}
				BalanceCardsInHand(); // Запускаем перебалансировку после добавления
				return;
			}
		}
		GD.PrintErr("Hand is full! Cannot add new card.");
		// Возможно, здесь нужно как-то уведомить игрока или сбросить карту
		newCard.QueueFree(); // Удаляем узел, если руку полна
	}

	// Перебалансирует карты в руке, сдвигая их и анимируя движение
	private void BalanceCardsInHand()
	{
		GD.Print("Balancing cards in hand...");
		// Создаем временный список только из не-null элементов
		var activeCards = new List<Node3D>();
		foreach (var card in cards)
		{
			if (card != null)
			{
				activeCards.Add(card);
			}
		}

		// Очищаем оригинальный массив
		for (int i = 0; i < cards.Length; i++)
		{
			cards[i] = null;
		}

		// Заполняем оригинальный массив снова, сдвигая элементы
		for (int i = 0; i < activeCards.Count; i++)
		{
			cards[i] = activeCards[i];

			// Рассчитываем целевую позицию
			Vector3 targetPosition = _start_pos_cards.Position;
			targetPosition.X += i * _horizontalSpacingCards;

			// Анимируем перемещение карты к целевой позиции
			if (cards[i] != null)
			{
				// Получаем Tween для узла (или создаем новый)
				var tween = cards[i].CreateTween();
				// Анимируем свойство Position
                tween.SetEase(Tween.EaseType.Out);
                tween.SetTrans(Tween.TransitionType.Quad);
                tween.TweenProperty(cards[i], "position", targetPosition, _shiftDuration);
			}
		}

		GD.Print("Card balancing complete.");
	}

	// Вспомогательный метод для получения первой свободной позиции в массиве карт
	private int GetFirstFreeCardIndex()
	{
		for (int i = 0; i < cards.Length; i++)
		{
			if (cards[i] == null)
			{
				return i;
			}
		}
		return -1; // Рука полна
	}
}
