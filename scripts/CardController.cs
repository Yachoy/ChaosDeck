using CCSpace;
using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CCSpace{

    public interface ICardBehavior
    {
        void OnSpawn(CardController cardController, CardData cardData);
        void OnDeath(CardController cardController, CardData cardData);
        void OnAttacked(CardController cardController, CardData cardData, int damage);
        void OnStep(CardController cardController, CardData cardData);
        void OnEvent(CardController cardController, string eventName, object eventData);
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

            int Hp;
            int Damage;
            int Cost;
            public ICardBehavior Behavior {get; private set;}

            public Card(CardData createFrom){
                CardDataOriginal = createFrom;
                Hp = createFrom.DefaultHp;
                Damage = createFrom.DefaultDamage;
                Cost = createFrom.DefaultCost;
                Behavior = createFrom.Behavior;
            }
        }
        public string DefaultName { get; private set; }
        public int DefaultHp { get; set; }
        public int DefaultDamage { get; private set; }
        public int DefaultCost { get; private set; }
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
		List<CardData> cards;

		public CardSetData(){

		} 
        public class CardSet{
            public CardSet(CardSetData d){

            }
        }
        
        public CardSet MakeInstanceForGame(){
            return new CardSet(this);
        }

		public bool TryResolveThisCardSet(List<CardData> cards){
			return true;
		}

		public bool TryAdd(CardData card){
			return true;
		}

		public bool TryTakeAway(String cardName){
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
            if (!Directory.Exists(absoluteCardsFolderPath))
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
                    }
                    _cardDataList.Add(card);
                    GD.Print($"- Загружена карта: {card.DefaultName ?? "Без имени"}");
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
        int mana;
        int max_mana;
        int hp;

        List<CCSpace.CardSetData.CardSet> actualDeck;
    }

    public class Match{
        Player p1;
        Player p2;

        bool is_first_player_round = true;

        public Match(Player p1, Player p2){

        }

        public bool MakeStep(){
            return false;
        }

    }


}

public partial class CardController : Node
{

    HandPlayer hp1;
    HandPlayer hp2;

    private bool is_button_endr_set;
    private HotSpotPlaying.Match match;

    private bool round_player1 = true;

	public override void _Ready()
	{
	}

    

    public bool StartNewMatch(){
        
        if (hp1 is null || hp2 is  null){
            return false;
        }
        match = new HotSpotPlaying.Match(new HotSpotPlaying.Player(), new HotSpotPlaying.Player());
        if (!((CardPlayer)hp2.cards[0]).isCardHidden){
            foreach(CardPlayer n in hp2.cards){
                if (n is null) continue;
                n.SwitchViewCard();
            }
        }
        if (((CardPlayer)hp1.cards[0]).isCardHidden){
            foreach(CardPlayer n in hp1.cards){
                if (n is null) continue;
                n.SwitchViewCard();
            }
        } 
        return true;
    }

    public void RegisterHandPlayer1(HandPlayer hand) => hp1 = hand;
    public void RegisterHandPlayer2(HandPlayer hand) => hp2 = hand;
    
    public void RegisterButtonEndRound(Button btn){
        if (is_button_endr_set) return;
        btn.Pressed += MakeRound;
        is_button_endr_set = true;
    }
    
    public bool IsPlayer1Round(){
        return round_player1;
    }

    public void MakeRound(){
        round_player1 = !round_player1;
        foreach(CardPlayer n in hp1.cards.Concat(hp2.cards)){
            if (n is null) continue;
            n.SwitchViewCard();
        }
    }

    public bool ProcessCardPlacement(CardPlayer card, CardPlace place){
        return true;
    }

	public override void _Process(double delta)
	{

	}

    public bool SetActualmatch(){
        return false;
    }
}

