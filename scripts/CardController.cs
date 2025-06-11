using CCSpace;
using Godot;
using Godot.Collections;
using HotSpotPlaying;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CCSpace{

    public interface ICardBehavior
    {
        public void OnSpawn(CardController cardController, CardPlayer me);
        public void OnDeath(CardController cardController, CardPlayer notMe, CardPlayer me);
        public void OnAttacked(CardController cardController, CardPlayer notMe, CardPlayer me);
        public void OnStep(CardController cardController, CardPlayer me);
        public void OnEvent(CardController cardController, Player notMe, Player me, string eventName, object eventData);
    }
    

    public enum TypesCards
    {
        Melee = 0,
        Ranged = 1,
        Unknown = -1
    }

    public class CardData
    {
        public class Card{
            public CardData CardDataOriginal;

            public int Hp;
            public int Damage;
            public int Cost;
            public Texture2D ImageTexture;
            public ICardBehavior Behavior {get; private set;}

            public bool its_can_figth = false;

            public Card(CardData createFrom){
                CardDataOriginal = createFrom;
                Hp = createFrom.DefaultHp;
                Damage = createFrom.DefaultDamage;
                Cost = createFrom.DefaultCost;
                Behavior = createFrom.Behavior;
                ImageTexture = createFrom.ImageTexture;
            }
        }
        public string DefaultName { get; private set; }
        public int DefaultHp { get; private set; }
        public int DefaultDamage { get; private set; }
        public int DefaultCost { get; private set; }
        public string Element { get; private set; }
        public TypesCards CardType { get; private set; }
        public Texture2D ImageTexture { get; private set; }
        public Dictionary Vars { get; private set; }
        public ICardBehavior Behavior {get; private set;}
        public Card MakeInstanceForGame(){
            return new Card(this);
        }
        public static CardData CreateFromJsonObject(Variant jsonData, string imageBasePath)
        {
            if (jsonData.VariantType != Variant.Type.Dictionary)
            {
                GD.PrintErr("Ошибка: Данные карты не являются словарем (Dictionary).");
                return null;
            }

            var cardDict = jsonData.AsGodotDictionary();
            var card = new CardData();

            card.DefaultName = cardDict.GetValueOrDefault("name", "Unnamed Card").AsString();
            card.DefaultHp = cardDict.GetValueOrDefault("hp", 0).AsInt32();
            card.DefaultDamage = cardDict.GetValueOrDefault("damage", 0).AsInt32();
            card.DefaultCost = cardDict.GetValueOrDefault("cost", 0).AsInt32();
            if (Enum.TryParse(cardDict.GetValueOrDefault("type", "Unknown").AsString(), out TypesCards cardType))
            {
                card.CardType = cardType;
            }else{
                Console.WriteLine($"Неизвестный тип карты {card.DefaultName}");
                card.CardType = TypesCards.Unknown;
            }
            card.Vars = cardDict.GetValueOrDefault("vars", new Dictionary()).AsGodotDictionary();

            string relativeImagePath = cardDict.GetValueOrDefault("image_path", "").AsString();
            if (!string.IsNullOrEmpty(relativeImagePath))
            {
                string fullImagePath = System.IO.Path.Combine(imageBasePath, relativeImagePath);

                if (ResourceLoader.Exists(fullImagePath))
                {
                    card.ImageTexture = ResourceLoader.Load<Texture2D>(fullImagePath);
                    if (card.ImageTexture == null)
                    {
                        GD.PrintErr($"Ошибка загрузки изображения: {fullImagePath}");
                    }
                }
                else
                {
                    GD.PrintErr($"Файл изображения не найден: {fullImagePath}");
                }
            }
            else
            {
                GD.PrintErr($"Путь к изображению не указан для карты: {card.DefaultName}");
            }

            if (card.ImageTexture == null) //TODO default image if null
            {
                // card.ImageTexture = ResourceLoader.Load<Texture2D>("res://путь/к/дефолтной/текстуре.png");
                GD.PushWarning($"Текстура для карты '{card.DefaultName}' не загружена.");
            }

            return card;
        }
        public void SetBehavior(ICardBehavior bh){
            Behavior = bh;
        }
    }
    
    public class CardSetData{
        public readonly int MAX_CARDS_IN_DECK = 50;

        public class CardSet
        {
            private System.Collections.Generic.Dictionary<string, CardData> storage;
            private System.Collections.Generic.Dictionary<string, int> currentCounters;
            private System.Random random;
            public CardSet(CardSetData csd)
            {
                storage = csd.storage;
                currentCounters = new System.Collections.Generic.Dictionary<string, int>(csd.counters);
                random = new System.Random();
            }
            public CardData DrawCard()
            {
                int totalRemaining = 0;
                foreach (var count in currentCounters.Values)
                {
                    totalRemaining += count;
                }

                double randomNumber = random.NextDouble();
                double cumulativeProbability = 0.0;
                GD.Print("Propabilityes. Rand: ",randomNumber);
                foreach (var cardEntry in currentCounters)
                {
                    int count = cardEntry.Value;
                    double cardProbability = (double)count / totalRemaining;
                    GD.Print(cardProbability);
                }
                GD.Print("-------------");
                
                foreach (var cardEntry in currentCounters)
                {
                    string cardName = cardEntry.Key;
                    int count = cardEntry.Value;

                    double cardProbability = (double)count / totalRemaining;
                    cumulativeProbability += cardProbability;
                    if (randomNumber < cumulativeProbability)
                    {
                        foreach (var card in currentCounters){
                            if(card.Key != cardName){
                                currentCounters[card.Key]++;
                            }
                        }
                        return storage[cardName];
                    }
                }

                GD.PushWarning("Ошибка при извлечении карты: случайное число не попало ни в один диапазон!");
                return null;
            }
        }
        
        private System.Collections.Generic.Dictionary<string, CardData> storage;

        private System.Collections.Generic.Dictionary<string, int> counters;
        public int CountCards{get; private set;} = 0;
        public int CountTypesCards{get; private set;} = 0;
		public CardSetData(){
            storage = new System.Collections.Generic.Dictionary<string, CardData>();
            counters = new System.Collections.Generic.Dictionary<string, int>();
		} 
        
        public CardSet MakeInstanceForGame(){
            return new CardSet(this);
        }

		public bool TryAdd(CardData card){
            CardData value;
            if(storage.TryGetValue(card.DefaultName, out value)){
                //GD.Print("Find", value.DefaultName);
                 
                if (counters[card.DefaultName] >= 3){return false;}
                counters[card.DefaultName] = 1 + counters[card.DefaultName];
                CountCards += 1;
            }else{
                //GD.Print("Don't find", card.DefaultName);
                if (CountCards >= MAX_CARDS_IN_DECK){return false;}
                counters[card.DefaultName] = 1;
                storage[card.DefaultName] = card;
                CountCards += 1;
                CountTypesCards += 1;
            }
            return true;
		}

		public bool TryTakeAway(CardData card){
            CardData value;
            if(!storage.TryGetValue(card.DefaultName, out value)){return false;}
            CountCards -= 1;
            counters[value.DefaultName] -= 1;
            if(counters[value.DefaultName] <= 0){
                storage.Remove(card.DefaultName);
                counters.Remove(card.DefaultName);
                CountTypesCards -= 1;
            }
			return true;
		}
	}

    public class CardLoader
    {
        private readonly List<CardData> _cardDataList = new List<CardData>();
        private readonly string _cardsFolderPath; 
        private readonly string _imagesBasePath;
        private const string JsonFileName = "cards.json";

        public IReadOnlyList<CardData> LoadedCards => _cardDataList.AsReadOnly();

        public CardLoader(string absoluteCardsFolderPath, System.Collections.Generic.Dictionary<String, ICardBehavior> behaviorses)
        {
            if (string.IsNullOrEmpty(absoluteCardsFolderPath))
            {
                throw new System.ArgumentNullException(nameof(absoluteCardsFolderPath), "Путь к папке с картами не может быть пустым.");
            }
            
            if (!DirAccess.DirExistsAbsolute(absoluteCardsFolderPath))
            {
                throw new System.IO.DirectoryNotFoundException($"Папка с картами не найдена: {absoluteCardsFolderPath}");
            }

            _cardsFolderPath = absoluteCardsFolderPath;
            _imagesBasePath = Path.Combine(_cardsFolderPath, "images");

            LoadCardsFromJson(behaviorses);
        }

        private void LoadCardsFromJson(System.Collections.Generic.Dictionary<String, ICardBehavior> behaviorses)
        {
            string jsonFilePath = Path.Combine(_cardsFolderPath, JsonFileName);

            if (!Godot.FileAccess.FileExists(jsonFilePath))
            {
                GD.PrintErr($"Файл данных карт не найден: {jsonFilePath}");
                return;
            }

            using var file = Godot.FileAccess.Open(jsonFilePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null)
            {
                GD.PrintErr($"Не удалось открыть файл: {jsonFilePath}. Ошибка: {Godot.FileAccess.GetOpenError()}");
                return;
            }

            string jsonString = file.GetAsText();

            var json = new Json();
            Error parseError = json.Parse(jsonString);
            if (parseError != Error.Ok)
            {
                GD.PrintErr($"Ошибка парсинга JSON файла '{jsonFilePath}': {json.GetErrorMessage()} на строке {json.GetErrorLine()}");
                return;
            }

            if (json.Data.VariantType != Variant.Type.Array)
            {
                GD.PrintErr($"Ошибка: Корневой элемент JSON файла '{jsonFilePath}' должен быть массивом.");
                return;
            }

            var jsonCardArray = json.Data.AsGodotArray();
            _cardDataList.Clear(); 

            GD.Print($"Загрузка карт из {jsonFilePath}...");
            foreach (Variant cardJsonVariant in jsonCardArray)
            {
                CardData card = CardData.CreateFromJsonObject(cardJsonVariant, _imagesBasePath);
                if (card != null)
                {   
                    ICardBehavior bh;
                    if (!behaviorses.TryGetValue(card.DefaultName, out bh)){
                        GD.PrintErr($"Can't find ICardBehavior for card {card.DefaultName}");
                    }else{
                        card.SetBehavior(bh);
                        _cardDataList.Add(card);
                        GD.Print($"- Загружена карта: {card.DefaultName ?? "Без имени"}");
                    }
                    
                }
                else
                {
                    GD.PrintErr("Ошибка при создании объекта CardData из JSON.");
                }
            }
            GD.Print($"Загружено {_cardDataList.Count} карт.");
        }

        public CardData GetCardByName(string name)
        {
            return _cardDataList.Find(card => card.DefaultName == name);
        }
    }

}

namespace HotSpotPlaying{
    public class Player{
        public int mana = 1;
        public int max_mana = 1;
        public int hp = 40;

        public CardPlayer[] Field = new CardPlayer[6];

        CCSpace.CardSetData.CardSet actualDeck;

        public Player(CCSpace.CardSetData.CardSet deck)
        {
            actualDeck = deck;
        }

        public CardData.Card GetCardFromDeck(){
            CardData cd = actualDeck.DrawCard();
            return cd.MakeInstanceForGame();
        }
    }

    public class Match{
        public Player p1;
        public Player p2;

        bool is_first_player_round = true;

        public Match(Player p1, Player p2){
            this.p1 = p1;
            this.p2 = p2;
        }

        public bool MakeStep(){
            return false;
        }

    }


}


class BaseBegaviour : CCSpace.ICardBehavior {
        public void OnSpawn(CardController cardController, CardPlayer me){

        }
        public void OnDeath(CardController cardController, CardPlayer notMe, CardPlayer me){

        }
        public void OnAttacked(CardController cardController, CardPlayer notMe, CardPlayer me){
            me.cardInstanceInfo.Hp -= notMe.cardInstanceInfo.Damage;
        }
        public void OnStep(CardController cardController, CardPlayer me){

        }
        public void OnEvent(CardController cardController, Player notMe, Player me, string eventName, object eventData){

        }
}


public partial class CardController : Node
{

    HandPlayer hp1;
    HandPlayer hp2;
    ParamatersTwoPlayers ptp;

    private bool is_button_endr_set;
    private HotSpotPlaying.Match match;

    private bool round_player1 = true;

    public CardSetData DebugCardSet = new CardSetData();
    public CardSetData StoryDeck = new CardSetData();
    private CardSetData defaultDeck = null;

    public string nameDeckToEdit = "";

    public CardLoader StorageCards = new CardLoader("res://resources/CardsStorage/", new System.Collections.Generic.Dictionary<string, ICardBehavior>{
            {"Archer", new BaseBegaviour()},
            {"Warrior", new BaseBegaviour()},
            {"Dwarf", new BaseBegaviour()},
            {"Elf warrior", new BaseBegaviour()},
            {"Militia", new BaseBegaviour()},
            {"Elf scout", new BaseBegaviour()}
    });

	public override void _Ready()
    {
        int c = 0;
        foreach (CardData v in StorageCards.LoadedCards)
        {
            if (!DebugCardSet.TryAdd(v))
            {
                GD.Print("Can't add...");
            }
            c += 1;
            if (c >= 10) { break; }
        }
        GD.Print(DebugCardSet.CountCards);
    }

    public void SetDefaultDeck(CardSetData csd) => defaultDeck = csd;

    public Player GetCurrentPlayer(bool reverse = false) => (reverse ? !round_player1 : round_player1) ? match.p1 : match.p2;

    public CardData.Card GetNewCard(bool is_player2){
        Player p = !is_player2 ? match.p1 : match.p2;
        GD.Print("Player ", is_player2 ? "1" : "2");
        return p.GetCardFromDeck();
    }

    public CardSetData GenerateRandomDeck()
    {

        CardSetData csd_rnd = new CardSetData();

        List<CardData> tempCards = new List<CardData>(StorageCards.LoadedCards);
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize(); // Инициализация генератора случайных чисел на основе времени

        int n = tempCards.Count;
        while (n > 1)
        {
            n--;
            int k = (int)rng.RandiRange(0, n);
            CardData value = tempCards[k];
            tempCards[k] = tempCards[n];
            tempCards[n] = value;
        }

        int c = 0;
        foreach (CardData v in tempCards)
        {
            if (!csd_rnd.TryAdd(v))
            {
                GD.Print("Can't add...");
            }
            c += 1;
            if (c >= 10) { break; }
        }
        return csd_rnd;

    }
    
    public bool StartNewMatch(CardSetData deck1 = null, CardSetData deck2 = null)
    {
        if (deck1 is null || deck2 is null)
        {
            deck1 = defaultDeck;
            deck2 = GenerateRandomDeck();
        }
        if (deck1 is null || deck2 is null)
        {
            GD.Print("Default deck doesn't take");
            return false;
        }
        if (deck1.CountTypesCards < 1)
        {
            GD.PushWarning("Count cards in deck1 less then 10, return false");
            return false;
        }
        if (deck2.CountTypesCards < 1)
        {
            GD.PushWarning("Count cards in deck2 less then 10, return false");
            return false;
        }

        if (hp1 is null || hp2 is null)
        {
            return false;
        }
        GD.Print("Start new match");
        foreach (CardPlayer cp in hp1.cards.Concat(hp2.cards))
        {
            if (cp is not null)
            {
                cp.QueueFree();
            }
        }
        foreach (CardPlace cp in hp1.places.Concat(hp2.places))
        {
            if (cp is not null)
            {
                cp.QueueFree();
            }

        }
        round_player1 = true;

        match = new HotSpotPlaying.Match(
            new HotSpotPlaying.Player(deck1.MakeInstanceForGame()),
            new HotSpotPlaying.Player(deck2.MakeInstanceForGame())
        );
        hp1.SpawnStartPlaces();
        hp1.SpawnStartCards();
        hp2.SpawnStartPlaces();
        hp2.SpawnStartCards();
        if (!hp2.cards[0].isCardHidden)
        {
            foreach (CardPlayer n in hp2.cards)
            {
                if (n is null) continue;
                n.SwitchViewCard();
            }
        }
        if (hp1.cards[0].isCardHidden)
        {
            foreach (CardPlayer n in hp1.cards)
            {
                if (n is null) continue;
                n.SwitchViewCard();
            }
        }
        HandlePlayerStatForLabels();
        return true;
    }

    public void RegisterHandPlayer1(HandPlayer hand) => hp1 = hand;
    public void RegisterHandPlayer2(HandPlayer hand) => hp2 = hand;
    
    public void RegisterButtonEndRound(Button btn){
        if (is_button_endr_set) return;
        btn.Pressed += MakeRound;
        is_button_endr_set = true;
    }

    public void RegisterMenuParameters(ParamatersTwoPlayers ptp) => this.ptp = ptp;
    
    public bool IsPlayer1Round(){
        return round_player1;
    }

    public void MakeRound(){
        foreach(CardPlayer n in hp1.cards.Concat(hp2.cards)){
            if (n is null) continue;
            n.SwitchViewCard();
        }



        Player p_now = GetCurrentPlayer();
        HandPlayer hand_now = round_player1 ? hp1 : hp2; 
        Player p_next = GetCurrentPlayer(true);
        HandPlayer hand_next = !round_player1 ? hp1 : hp2;
        p_now.max_mana = p_now.max_mana + 1;
        p_now.mana = p_now.max_mana;
        int counter = 0;
        foreach (CardPlayer nowPlayerCard in p_now.Field){
            CardPlayer nextPlayerCard = p_next.Field[counter];
            if (nowPlayerCard is not null){
                if (nowPlayerCard.cardInstanceInfo.its_can_figth){
                    hand_now.AnimateCardAttack(nowPlayerCard);
                    if(nextPlayerCard is null){
                        p_next.hp -= nowPlayerCard.cardInstanceInfo.Damage;
                        if (p_next.hp <= 0){
                            hp1.QueueFree();
                            hp2.QueueFree();
                            hp1 = null;
                            hp2 = null;
                            is_button_endr_set = false;
                            GetTree().ChangeSceneToFile("res://scenes/Menu/main_menu.tscn");
                        }
                    }else{
                        nextPlayerCard.cardInstanceInfo.Behavior.OnAttacked(this, nowPlayerCard, nextPlayerCard);
                        if (nextPlayerCard.cardInstanceInfo.Hp <= 0){
                            nextPlayerCard.cardInstanceInfo.Behavior.OnDeath(this, nowPlayerCard, nextPlayerCard);
                            nextPlayerCard.Visible = false;
                            nextPlayerCard.QueueFree();
                            p_next.Field[counter] = null;
                        }else{
                            nextPlayerCard.UpdateStatisticeOnTheCards();
                        }
                    }
                    nowPlayerCard.UpdateStatisticeOnTheCards();
                    
                }
                nowPlayerCard.cardInstanceInfo.its_can_figth = true;
            }
            counter++;
        }
        round_player1 = !round_player1;
        HandlePlayerStatForLabels();
        hand_next.UpdateVisibilityCards();
        hand_now.UpdateVisibilityCards();
        foreach (CardPlayer nowPlayerCard in p_next.Field){
            if (nowPlayerCard is not null){
                nowPlayerCard.cardInstanceInfo.Behavior.OnStep(this, nowPlayerCard);
            }
        }

    }
    
    public bool ProcessCardPlacement(CardPlayer card, CardPlace place){
        Player player = GetCurrentPlayer();
        if (player.mana < card.cardInstanceInfo.Cost){
            GD.Print("Player haven't enought mana");
            return false;
        }
        player.mana -= card.cardInstanceInfo.Cost;
        player.Field[place.Index] = card;
        HandlePlayerStatForLabels();
        card.cardInstanceInfo.Behavior.OnSpawn(this, card);
        return true;
    }

    public void HandlePlayerStatForLabels(){
        ptp.set_mana(true, match.p1.mana);
        ptp.set_hp(true, match.p1.hp);
        ptp.set_power_mana(true, match.p1.max_mana);

        ptp.set_mana(false, match.p2.mana);
        ptp.set_hp(false, match.p2.hp);
        ptp.set_power_mana(false, match.p2.max_mana);
    }
    public bool SetActualmatch(){
        return false;
    }
}

