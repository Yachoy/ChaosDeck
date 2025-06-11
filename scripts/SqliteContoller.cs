using Godot;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public partial class SqliteContoller : Node
{
	private string _dbFilePath;
    private string _connectionString;

    private const string DATABASE_FILENAME = "game_decks.db";

    private const string TABLE_CARD_DECKS = "CardDecks";
    private const string COL_DECK_UID = "uid_name_card_deck";

    private const string TABLE_DECK_CARDS = "DeckCards";
    private const string COL_CARD_PK_UID = "uid";
    private const string COL_CARD_DECK_FK_UID = "uid_name_card_deck_fk";
    private const string COL_CARD_NAME = "card_name";
    private const string COL_CARD_AMOUNT = "card_amount";


    public override void _Ready()
    {
        string userDir = ProjectSettings.GlobalizePath("user://");

        if (!Directory.Exists(userDir))
        {
            try
            {
                Directory.CreateDirectory(userDir);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Failed to create user directory {userDir}: {ex.Message}");
                SetProcess(false);
                return;
            }
        }
        
        _dbFilePath = Path.Combine(userDir, DATABASE_FILENAME);
        _connectionString = $"Data Source={_dbFilePath}";

        GD.Print($"Database file path: {_dbFilePath}");

        InitializeDatabase();
        //TestDatabaseOperations();
    }

    /// <summary>
    /// Executes a non-query SQL command (INSERT, UPDATE, DELETE, CREATE).
    /// </summary>
    /// <param name="query">The SQL query string.</param>
    /// <param name="parameters">Optional parameters for the query.</param>
    /// <returns>The number of rows affected.</returns>
    /// <exception cref="SqliteException">Thrown for SQLite-specific errors.</exception>
    /// <exception cref="Exception">Thrown for general errors.</exception>
    private int ExecuteNonQuery(string query, params SqliteParameter[] parameters)
    {
        try
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    return command.ExecuteNonQuery();
                }
            }
        }
        catch (SqliteException ex)
        {
            GD.PrintErr($"SQLite Error: {ex.Message}\nQuery: {query}");
            throw;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"General Error: {ex.Message}\nQuery: {query}");
            throw;
        }
    }

    public void InitializeDatabase()
    {
        GD.Print("Initializing database...");

        string createDecksTableQuery = $@"
            CREATE TABLE IF NOT EXISTS {TABLE_CARD_DECKS} (
                {COL_DECK_UID} VARCHAR(50) PRIMARY KEY NOT NULL
            );";

        string createDeckCardsTableQuery = $@"
            CREATE TABLE IF NOT EXISTS {TABLE_DECK_CARDS} (
                {COL_CARD_PK_UID} INTEGER PRIMARY KEY AUTOINCREMENT,
                {COL_CARD_DECK_FK_UID} VARCHAR(50) NOT NULL,
                {COL_CARD_NAME} VARCHAR(50) NOT NULL,
                {COL_CARD_AMOUNT} INTEGER NOT NULL CHECK({COL_CARD_AMOUNT} >= 0 AND {COL_CARD_AMOUNT} <= 255),
                FOREIGN KEY ({COL_CARD_DECK_FK_UID}) REFERENCES {TABLE_CARD_DECKS}({COL_DECK_UID}) ON DELETE CASCADE,
                UNIQUE ({COL_CARD_DECK_FK_UID}, {COL_CARD_NAME})
            );";
        try
        {
            ExecuteNonQuery(createDecksTableQuery);
            GD.Print($"Table '{TABLE_CARD_DECKS}' checked/created.");

            ExecuteNonQuery(createDeckCardsTableQuery);
            GD.Print($"Table '{TABLE_DECK_CARDS}' checked/created.");
        }
        catch (Exception ex)
        {
             GD.PrintErr($"Failed to initialize database tables: {ex.Message}");
        }
    }

    /// <summary>
    /// Adds a new deck. Does nothing if a deck with the same name already exists.
    /// </summary>
    /// <param name="deckUidName">Unique name of the deck.</param>
    /// <returns>True if the deck was successfully added, false if it already existed or an error occurred.</returns>
    public bool AddDeck(string deckUidName)
    {
        if (string.IsNullOrWhiteSpace(deckUidName))
        {
            GD.PrintErr("Deck name cannot be empty.");
            return false;
        }
        if (deckUidName.Length > 50)
        {
            GD.PrintErr("Deck name is too long (max 50 characters).");
            return false;
        }

        string query = $"INSERT OR IGNORE INTO {TABLE_CARD_DECKS} ({COL_DECK_UID}) VALUES (@DeckName);";
        try
        {
            int rowsAffected = ExecuteNonQuery(query, new SqliteParameter("@DeckName", deckUidName));
            if (rowsAffected > 0)
            {
                GD.Print($"Deck '{deckUidName}' added.");
                return true;
            }
            else
            {
                GD.Print($"Deck '{deckUidName}' already exists or no changes made.");
                return false;
            }
        }
        catch (Exception ex) // Catch exceptions thrown by ExecuteNonQuery
        {
            GD.PrintErr($"Error adding deck '{deckUidName}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Adds a card to a deck or updates its amount if it already exists.
    /// </summary>
    /// <param name="deckUidName">Name of the deck.</param>
    /// <param name="cardName">Name of the card.</param>
    /// <param name="amount">Amount of the card (0-255).</param>
    /// <returns>True on success, false on error.</returns>
    public bool SetCardInDeck(string deckUidName, string cardName, byte amount)
    {
        if (string.IsNullOrWhiteSpace(deckUidName) || string.IsNullOrWhiteSpace(cardName))
        {
            GD.PrintErr("Deck name and card name cannot be empty.");
            return false;
        }
        if (cardName.Length > 50)
        {
             GD.PrintErr("Card name is too long (max 50 characters).");
            return false;
        }
        // The FOREIGN KEY constraint and the UNIQUE constraint handle the case
        // where the deck doesn't exist or the card is already in the deck.
        // INSERT ... ON CONFLICT ... DO UPDATE handles the update logic.

        string query = $@"
            INSERT INTO {TABLE_DECK_CARDS} ({COL_CARD_DECK_FK_UID}, {COL_CARD_NAME}, {COL_CARD_AMOUNT})
            VALUES (@DeckName, @CardName, @Amount)
            ON CONFLICT({COL_CARD_DECK_FK_UID}, {COL_CARD_NAME}) DO UPDATE SET
            {COL_CARD_AMOUNT} = @Amount;";
        try
        {
            // We don't necessarily care about rowsAffected here, as ON CONFLICT could be 0 or 1.
            ExecuteNonQuery(query,
                new SqliteParameter("@DeckName", deckUidName),
                new SqliteParameter("@CardName", cardName),
                new SqliteParameter("@Amount", amount) 
            );
            GD.Print($"Card '{cardName}' amount set to {amount} in deck '{deckUidName}'.");
            return true;
        }
        catch (Exception ex) // Catch exceptions thrown by ExecuteNonQuery (e.g., if deck doesn't exist due to FK constraint)
        {
            GD.PrintErr($"Error setting card '{cardName}' in deck '{deckUidName}': {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Deletes a deck and all associated cards (due to ON DELETE CASCADE).
    /// </summary>
    /// <param name="deckUidName">Name of the deck to delete.</param>
    /// <returns>True on success, false on error or if deck was not found.</returns>
    public bool DeleteDeck(string deckUidName)
    {
        if (string.IsNullOrWhiteSpace(deckUidName))
        {
            GD.PrintErr("Deck name cannot be empty for deletion.");
            return false;
        }

        string query = $"DELETE FROM {TABLE_CARD_DECKS} WHERE {COL_DECK_UID} = @DeckName;";
        try
        {
            int rowsAffected = ExecuteNonQuery(query, new SqliteParameter("@DeckName", deckUidName));
            if (rowsAffected > 0)
            {
                GD.Print($"Deck '{deckUidName}' and its cards deleted.");
                return true;
            }
            else
            {
                 GD.Print($"Deck '{deckUidName}' not found or no changes made during deletion.");
                 return false;
            }
        }
        catch (Exception ex) // Catch exceptions thrown by ExecuteNonQuery
        {
            GD.PrintErr($"Error deleting deck '{deckUidName}': {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of cards (name and amount) for the specified deck.
    /// </summary>
    /// <param name="deckUidName">Name of the deck.</param>
    /// <returns>A dictionary where the key is the card name and the value is the amount. Returns an empty dictionary if the deck is not found or empty.</returns>
    public Dictionary<string, byte> GetCardsInDeck(string deckUidName)
    {
        var cards = new Dictionary<string, byte>();
        if (string.IsNullOrWhiteSpace(deckUidName))
        {
            GD.PrintErr("Deck name cannot be empty for getting cards.");
            return cards;
        }

        string query = $"SELECT {COL_CARD_NAME}, {COL_CARD_AMOUNT} FROM {TABLE_DECK_CARDS} WHERE {COL_CARD_DECK_FK_UID} = @DeckName;";

        try
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@DeckName", deckUidName);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cardName = reader.GetString(0);
                            byte cardAmount = Convert.ToByte(reader.GetInt32(1)); // Convert from SQLite INTEGER to byte
                            cards[cardName] = cardAmount;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error getting cards for deck '{deckUidName}': {ex.Message}");
            // Return empty list in case of error
        }
        return cards;
    }

    /// <summary>
    /// Checks if a deck with the given name exists.
    /// </summary>
    /// <param name="deckUidName">Name of the deck.</param>
    /// <returns>True if the deck exists, false otherwise or on error.</returns>
    public bool DeckExists(string deckUidName)
    {
        if (string.IsNullOrWhiteSpace(deckUidName)) return false;

        string query = $"SELECT 1 FROM {TABLE_CARD_DECKS} WHERE {COL_DECK_UID} = @DeckName LIMIT 1;";
        try
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue("@DeckName", deckUidName);
                    using (var reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error checking if deck '{deckUidName}' exists: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of all saved deck names.
    /// </summary>
    /// <returns>A list of strings, where each string is a deck name. Returns an empty list if no decks are found or an error occurs.</returns>
    public List<string> GetAllDeckNames()
    {
        var deckNames = new List<string>();
        string query = $"SELECT {COL_DECK_UID} FROM {TABLE_CARD_DECKS};";

        try
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            deckNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error getting all deck names: {ex.Message}");
            // Return empty list in case of error
        }
        return deckNames;
    }

    // --- Test Operations (Commented Out by Default) ---

    public void TestDatabaseOperations()
    {
        GD.Print("\n--- Testing Database Operations ---");

        string testDeck1 = "MyAwesomeDeck";
        string testDeck2 = "AnotherDeck";

        // Добавление колод
        AddDeck(testDeck1);
        AddDeck(testDeck2);
        AddDeck(testDeck1); // Попытка добавить существующую

        // Добавление/изменение карт
        SetCardInDeck(testDeck1, "Dragon", 1);
        SetCardInDeck(testDeck1, "Warrior", 3);
        SetCardInDeck(testDeck1, "Mage", 1);
        SetCardInDeck(testDeck1, "Dragon", 2); // Изменение количества
        
        SetCardInDeck(testDeck2, "Goblin", 5);
        SetCardInDeck(testDeck2, "Slime", 10);

        // Попытка добавить карту в несуществующую колоду (должно вызвать ошибку из-за FOREIGN KEY)
        SetCardInDeck("NonExistentDeck", "Ghost", 1); 

        // Получение карт
        GD.Print($"\nCards in '{testDeck1}':");
        Dictionary<string, byte> deck1Cards = GetCardsInDeck(testDeck1);
        foreach (var cardEntry in deck1Cards)
        {
            GD.Print($"- {cardEntry.Key}: {cardEntry.Value}");
        }
        if (!deck1Cards.Any()) GD.Print("(No cards or deck not found)");


        GD.Print($"\nCards in '{testDeck2}':");
        var deck2Cards = GetCardsInDeck(testDeck2);
        foreach (var cardEntry in deck2Cards)
        {
			GD.Print($"- {cardEntry.Key}: {cardEntry.Value}");
        }
         if (!deck2Cards.Any()) GD.Print("(No cards or deck not found)");

        // Удаление колоды
        DeleteDeck(testDeck1);
        GD.Print($"\nCards in '{testDeck1}' after deletion:");
        deck1Cards = GetCardsInDeck(testDeck1);
        foreach (var cardEntry in deck1Cards)
        {
           GD.Print($"- {cardEntry.Key}: {cardEntry.Value}");
        }
        if (!deck1Cards.Any()) GD.Print("(No cards or deck not found)");

        GD.Print("\n--- Test Finished ---");
    }
}
