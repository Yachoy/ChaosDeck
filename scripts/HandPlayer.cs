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
	public CardPlayer[] cards = new CardPlayer[MAX_CARDS_IN_HAND];

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
		}
		cc.StartNewMatch(cc.DebugCardSet, cc.DebugCardSet);
	}

	public void SpawnStartPlaces(int _numberOfPlaces = MAX_PLACES)
	{
		if (_numberOfPlaces > MAX_PLACES)
		{
			GD.PrintErr($"Requested number of places ({_numberOfPlaces}) exceeds max allowed ({MAX_PLACES}). Spawning {MAX_PLACES}.");
			_numberOfPlaces = MAX_PLACES;
		}

		for (int i = 0; i < _numberOfPlaces; i++)
		{
			//GD.Print($"Spawn place {i}");
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
				cardPlace.SetIndexed(i);
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

	public void SpawnStartCards(int _numberOfCards = MAX_CARDS_IN_HAND)
	{
		if (_numberOfCards > MAX_CARDS_IN_HAND)
		{
			GD.PrintErr($"Requested number of cards ({_numberOfCards}) exceeds max allowed ({MAX_CARDS_IN_HAND}). Spawning {MAX_CARDS_IN_HAND}.");
			_numberOfCards = MAX_CARDS_IN_HAND;
		}

		for (int i = 0; i < _numberOfCards; i++)
		{
			//GD.Print($"Spawn card {i}");
			Node3D cardInstance = _cardScene.Instantiate<Node3D>();
			// Начальная позиция будет определена функцией балансировки
			AddChild(cardInstance);

			if (cardInstance is CardPlayer cardPlayer)
			{
				if (IsAnotherTeam)
				{
					cardPlayer.SetIsAnotherTeam();
				}
				cardPlayer.GenerateCard();
				cardPlayer.Connect(
					CardPlayer.SignalName.ObjectSelected,
					new Callable(this, MethodName.OnCardSelected)
				);
				cards[i] = (CardPlayer)cardInstance;
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
		if(!cc.ProcessCardPlacement(_selected_card, selectedPlaceCard)){return;}
		
		GD.Print($"Trying to place card {_selected_card.Name} on place {selectedPlaceCard.Name}");

		// Удаляем карту из руки
		RemoveCardFromHand(_selected_card);

		_selected_card.is_placed = true;
		_selected_card.MoveTo(selectedPlaceCard);
		selectedPlaceCard.isAlreadyCardHerePumPumPum = true;

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

	public void UpdateVisibilityCards(){
		foreach (var card in cards)
		{
			if (card != null)
			{
				card.AdjustSpriteToTargetObject();
			}
		}
	}

	// Перебалансирует карты в руке, сдвигая их и анимируя движение
	private void BalanceCardsInHand()
	{
		UpdateVisibilityCards();
		GD.Print("Balancing cards in hand...");
		// Создаем временный список только из не-null элементов
		var activeCards = new List<CardPlayer>();
		foreach (var card in cards)
		{
			if (card != null)
			{
				activeCards.Add(card);
				card.AdjustSpriteToTargetObject();
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

	private const float CARD_ATTACK_DISTANCE = 0.5f; // How far the card moves along the Z-axis of the HandPlayer.
	private const float CARD_ATTACK_DURATION_FORWARD = 0.15f; // Duration for the forward movement.
	private const float CARD_ATTACK_DURATION_BACKWARD = 0.15f; // Duration for the backward movement.

	public void AnimateCardAttack(CardPlayer cardToAnimate)
	{
		if (cardToAnimate == null || !IsInstanceValid(cardToAnimate))
		{
			GD.PrintErr("AnimateCardAttack: Card to animate is null or invalid.");
			return;
		}

		Vector3 originalPosition = cardToAnimate.Position;

		float zDisplacementDirection = !IsAnotherTeam ? -1.0f : 1.0f;
		Vector3 attackDisplacement = new Vector3(0, 0, CARD_ATTACK_DISTANCE * zDisplacementDirection);
		
		Vector3 targetForwardPosition = originalPosition + attackDisplacement;

		Tween attackTween = cardToAnimate.CreateTween();
		attackTween.SetParallel(false); 

		attackTween.TweenProperty(cardToAnimate, "position", targetForwardPosition, CARD_ATTACK_DURATION_FORWARD)
				   .SetEase(Tween.EaseType.Out)       // Animation eases out (starts fast, ends slow).
				   .SetTrans(Tween.TransitionType.Quad); // Uses quadratic interpolation for smooth movement.

		// 2. Animate the card moving back to its original position.
		attackTween.TweenProperty(cardToAnimate, "position", originalPosition, CARD_ATTACK_DURATION_BACKWARD)
				   .SetEase(Tween.EaseType.In)        // Animation eases in (starts slow, ends fast).
				   .SetTrans(Tween.TransitionType.Quad); // Uses quadratic interpolation.
		
	}
}
