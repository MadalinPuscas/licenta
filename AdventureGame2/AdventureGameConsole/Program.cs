using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using Engine;
using System.Data;
using System.Data.SqlClient;

namespace AdventureGameConsole
{
    public class Program
    {
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        private static Player player;


        static void Main(string[] args)
        {
            LoadGameData();

            Console.WriteLine("Type 'Help' to see a list of commands");
            Console.WriteLine("");

            DisplayCurrentLocation();

            player.PropertyChanged += Player_OnPropertyChanged;
            player.OnMessage += Player_OnMessage;

            while (true)
            {
                Console.WriteLine(">");

                string userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                string cleanedInput = userInput.ToLower();

                if (cleanedInput == "exit")
                {
                    SaveGameData();
                    break;
                }

                ParseInput(cleanedInput);
            }
        }

        public static void Player_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentLocation")
            {
                DisplayCurrentLocation();

                if (player.CurrentLocation.VendorWorkingHere != null)
                {
                    Console.WriteLine("You see a vendor here: {0}", player.CurrentLocation.VendorWorkingHere.Name);
                }
            }
        }

        public static void Player_OnMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message);

            if (e.AddExtraNewLine)
            {
                Console.WriteLine("");
            }
        }

        private static void ParseInput(string input)
        {
            if (input.Contains("help") || input == "?")
            {
                Console.WriteLine("Available Commands");
                Console.WriteLine("====================================");
                Console.WriteLine("Stats - Display player information");
                Console.WriteLine("Look - Get the description of your location");
                Console.WriteLine("Inventory - Display your inventory");
                Console.WriteLine("Quests - Display your quests");
                Console.WriteLine("Attack - Fight the monster");
                Console.WriteLine("Equip <weapon name> - Set your current weapon");
                Console.WriteLine("Drink <potion name> - Drink a potion");
                Console.WriteLine("Trade - display your inventory and vendor's inventory");
                Console.WriteLine("Buy <item name> - Buy an item from a vendor");
                Console.WriteLine("Sell <item name> - Sell an item to a vendor");
                Console.WriteLine("North - Move North");
                Console.WriteLine("South - Move South");
                Console.WriteLine("East - Move East");
                Console.WriteLine("West - Move West");
                Console.WriteLine("Exit - Save the game and exit");
            }
            else if (input == "stats")
            {
                Console.WriteLine("Current hit points: {0}", player.CurrentHitPoints);
                Console.WriteLine("Maximum hit points: {0}", player.MaximumHitPoints);
                Console.WriteLine("Experience Points: {0}", player.ExperiencePoints);
                Console.WriteLine("Level: {0}", player.Level);
                Console.WriteLine("Gold: {0}", player.Gold);
            }
            else if (input == "look")
            {
                DisplayCurrentLocation();
            }
            else if (input.Contains("north"))
            {
                if (player.CurrentLocation.LocationToNorth == null)
                {
                    Console.WriteLine("You cannot move North");
                }
                else
                {
                    player.MoveNorth();
                }
            }
            else if (input.Contains("east"))
            {
                if (player.CurrentLocation.LocationToEast == null)
                {
                    Console.WriteLine("You cannot move East");
                }
                else
                {
                    player.MoveEast();
                }
            }
            else if (input.Contains("south"))
            {
                if (player.CurrentLocation.LocationToSouth == null)
                {
                    Console.WriteLine("You cannot move South");
                }
                else
                {
                    player.MoveSouth();
                }
            }
            else if (input.Contains("west"))
            {
                if (player.CurrentLocation.LocationToWest == null)
                {
                    Console.WriteLine("You cannot move West");
                }
                else
                {
                    player.MoveWest();
                }
            }
            else if (input == "inventory")
            {
                foreach (InventoryItem inventoryItem in player.Inventory)
                {
                    Console.WriteLine("{0}: {1}", inventoryItem.Description, inventoryItem.Quantity);
                }
            }
            else if (input == "quests")
            {
                if (player.Quests.Count == 0)
                {
                    Console.WriteLine("You do not have any quests");
                }
                else
                {
                    foreach (PlayerQuest playerQuest in player.Quests)
                    {
                        Console.WriteLine("{0}: {1}", playerQuest.Name, playerQuest.IsCompleted ? "Completed" : "Incomplete");
                    }
                }
            }
            else if (input.Contains("attack"))
            {
                if (player.CurrentLocation.MonsterLivingHere == null)
                {
                    Console.WriteLine("There is nothing here to attack");
                }
                else
                {
                    if (player.CurrentWeapon == null)
                    {
                        Console.WriteLine("You do not have any weapons");
                    }
                    else
                    {
                        player.UseWeapon(player.CurrentWeapon);
                    }
                }
            }
            else if (input.StartsWith("equip "))
            {
                string inputWeaponName = input.Substring(6).Trim();

                if (string.IsNullOrEmpty(inputWeaponName))
                {
                    Console.WriteLine("You must enter the name of the weapon to equip");
                }
                else
                {
                    Weapon weaponToEquip = player.Weapons.SingleOrDefault(x => x.Name.ToLower() == inputWeaponName || x.NamePlural.ToLower() == inputWeaponName);

                    if (weaponToEquip == null)
                    {
                        Console.WriteLine("You do not have the weapon: {0}", inputWeaponName);
                    }
                    else
                    {
                        player.CurrentWeapon = weaponToEquip;
                        Console.WriteLine("You equip your {0}", player.CurrentWeapon.Name);
                    }
                }
            }
            else if (input.StartsWith("drink "))
            {
                string inputPotionName = input.Substring(6).Trim();

                if (string.IsNullOrEmpty(inputPotionName))
                {
                    Console.WriteLine("You must enter the name of the potion to drink");
                }
                else
                {
                    HealingPotion potionToDrink =
                        player.Potions.SingleOrDefault(x => x.Name.ToLower() == inputPotionName || x.NamePlural.ToLower() == inputPotionName);

                    if (potionToDrink == null)
                    {
                        Console.WriteLine("You do not have the potion: {0}", inputPotionName);
                    }
                    else
                    {
                        player.UsePotions(potionToDrink);
                    }
                }
            }
            else if (input == "trade")
            {
                if (player.CurrentLocation.VendorWorkingHere == null)
                {
                    Console.WriteLine("There is no vendor here");
                }
                else
                {
                    Console.WriteLine("PLAYER INVENTORY");
                    Console.WriteLine("================");

                    if (player.Inventory.Count(x => x.Price != World.UNSELLABLE_ITEM_PRICE) == 0)
                    {
                        Console.WriteLine("You do not have any inventory");
                    }
                    else
                    {
                        foreach (InventoryItem inventoryItem in player.Inventory.Where(x => x.Price != World.UNSELLABLE_ITEM_PRICE))
                        {
                            Console.WriteLine("{0} {1} Price: {2}", inventoryItem.Quantity, inventoryItem.Description, inventoryItem.Price);
                        }
                    }

                    Console.WriteLine("");
                    Console.WriteLine("VENDOR INVENTORY");
                    Console.WriteLine("================");

                    if (player.CurrentLocation.VendorWorkingHere.Inventory.Count == 0)
                    {
                        Console.WriteLine("The vendor does not have any inventory");
                    }
                    else
                    {
                        foreach (InventoryItem inventoryItem in player.CurrentLocation.VendorWorkingHere.Inventory)
                        {
                            Console.WriteLine("{0} {1} Price: {2}", inventoryItem.Quantity, inventoryItem.Description, inventoryItem.Price);
                        }
                    }
                }
            }
            else if (input.StartsWith("buy "))
            {
                if (player.CurrentLocation.VendorWorkingHere == null)
                {
                    Console.WriteLine("There is no vendor at this location");
                }
                else
                {
                    string itemName = input.Substring(4).Trim();

                    if (string.IsNullOrEmpty(itemName))
                    {
                        Console.WriteLine("You must enter the name of the item to buy");
                    }
                    else
                    {
                        InventoryItem itemToBuy = player.CurrentLocation.VendorWorkingHere.Inventory.SingleOrDefault(x => x.Details.Name.ToLower() == itemName);

                        if (itemToBuy == null)
                        {
                            Console.WriteLine("The vendor does not have any {0}", itemName);
                        }
                        else
                        {
                            if (player.Gold < itemToBuy.Price)
                            {
                                Console.WriteLine("You do not have enough gold to buy a {0}", itemToBuy.Description);
                            }
                            else
                            {
                                player.AddItemToInventory(itemToBuy.Details);
                                player.Gold -= itemToBuy.Price;

                                Console.WriteLine("You bought one {0} for {1} gold", itemToBuy.Details.Name, itemToBuy.Price);
                            }
                        }
                    }
                }
            }
            else if (input.StartsWith("sell "))
            {
                if (player.CurrentLocation.VendorWorkingHere == null)
                {
                    Console.WriteLine("There is no vendor at this location");
                }
                else
                {
                    string itemName = input.Substring(5).Trim();

                    if(string.IsNullOrEmpty(itemName))
                    {
                        Console.WriteLine("You must enter the name of the item to sell");
                    }
                    else
                    {
                        InventoryItem itemToSell = player.Inventory.SingleOrDefault(x => x.Details.Name.ToLower() == itemName && x.Quantity > 0 && x.Price != World.UNSELLABLE_ITEM_PRICE);

                        if(itemToSell == null)
                        {
                            Console.WriteLine("The player cannot sell any {0}", itemName);
                        }
                        else
                        {
                            player.RemoveItemFromInventory(itemToSell.Details);
                            player.Gold += itemToSell.Price;

                            Console.WriteLine("You receive {0} gold for your {1}", itemToSell.Price, itemToSell.Details.Name);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("I do not understand");
                Console.WriteLine("Type 'Help' to see a list of available commands");
            }

            Console.WriteLine("");
        }

        private static void DisplayCurrentLocation()
        {
            Console.WriteLine("You are at: {0}", player.CurrentLocation.Name);

            if(player.CurrentLocation.Description == "")
            {
                Console.WriteLine(player.CurrentLocation.Description);
            }
        }

        private static void LoadGameData()
        {
            player = PlayerDataMapper.CreateFromDatabase();

            if (player == null)
            {
                if (File.Exists(PLAYER_DATA_FILE_NAME))
                {
                    player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                }
                else
                {
                    player = Player.CreateDefaultPlayer();
                }
            }
        }

        private static void SaveGameData()
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());

            PlayerDataMapper.SaveToDatabase(player);
        }

        public static class PlayerDataMapper
        {
            private static readonly string connectionString = "Data Source=(local);Initial Catalog=AdventureGame2;Integrated Security=True; MultipleActiveResultSets=true";

            public static Player CreateFromDatabase()
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        Player player;

                        using (SqlCommand savedGameCommand = connection.CreateCommand())
                        {
                            savedGameCommand.CommandType = CommandType.Text;
                            savedGameCommand.CommandText = "SELECT TOP 1 * FROM SavedGame";

                            SqlDataReader reader = savedGameCommand.ExecuteReader();

                            if (!reader.HasRows)
                            {
                                return null;
                            }

                            reader.Read();

                            int currentHitPoints = (int)reader["CurrentHitPoints"];
                            int maximumHitPoints = (int)reader["MaximumHitPoints"];
                            int gold = (int)reader["Gold"];
                            int experiencePoints = (int)reader["ExperiencePoints"];
                            int currentLocationID = (int)reader["CurrentLocationID"];

                            player = Player.CreatePlayerFromDatabase(currentHitPoints, maximumHitPoints, gold,
                                experiencePoints, currentLocationID);
                        }

                        using (SqlCommand questCommand = connection.CreateCommand())
                        {
                            questCommand.CommandType = CommandType.Text;
                            questCommand.CommandText = "SELECT * FROM Quest";

                            SqlDataReader reader = questCommand.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int questID = (int)reader["QuestID"];
                                    bool isCompleted = (bool)reader["IsCompleted"];

                                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(questID));
                                    playerQuest.IsCompleted = isCompleted;

                                    player.Quests.Add(playerQuest);
                                }
                            }
                        }

                        using (SqlCommand inventoryCommand = connection.CreateCommand())
                        {
                            inventoryCommand.CommandType = CommandType.Text;
                            inventoryCommand.CommandText = "SELECT * FROM Inventory";

                            SqlDataReader reader = inventoryCommand.ExecuteReader();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int inventoryItemID = (int)reader["InventoryItemID"];
                                    int quantity = (int)reader["Quantity"];

                                    player.AddItemToInventory(World.ItemByID(inventoryItemID), quantity);
                                }
                            }
                        }
                        return player;
                    }
                }
                catch (Exception ex)
                {

                }

                return null;
            }

            public static void SaveToDatabase(Player player)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        using (SqlCommand existingRowCountCommand = connection.CreateCommand())
                        {
                            existingRowCountCommand.CommandType = CommandType.Text;
                            existingRowCountCommand.CommandText = "SELECT count(*) FROM SavedGame";

                            int existingRowCount = (int)existingRowCountCommand.ExecuteScalar();

                            if (existingRowCount == 0)
                            {
                                using (SqlCommand insertSavedGame = connection.CreateCommand())
                                {
                                    insertSavedGame.CommandType = CommandType.Text;
                                    insertSavedGame.CommandText =
                                        "INSERT INTO SavedGame " +
                                        "(CurrentHitPoints, MaximumHitPoints, Gold, ExperiencePoints, CurrentLocationID) " +
                                        "VALUES " +
                                        "(@CurrentHitPoints, @MaximumHitPoints, @Gold, @ExperiencePoints, @CurrentLocationID)";

                                    insertSavedGame.Parameters.Add("@CurrentHitPoints", SqlDbType.Int);
                                    insertSavedGame.Parameters["@CurrentHitPoints"].Value = player.CurrentHitPoints;

                                    insertSavedGame.Parameters.Add("@MaximumHitPoints", SqlDbType.Int);
                                    insertSavedGame.Parameters["@MaximumHitPoints"].Value = player.MaximumHitPoints;

                                    insertSavedGame.Parameters.Add("@Gold", SqlDbType.Int);
                                    insertSavedGame.Parameters["@Gold"].Value = player.Gold;

                                    insertSavedGame.Parameters.Add("@ExperiencePoints", SqlDbType.Int);
                                    insertSavedGame.Parameters["@ExperiencePoints"].Value = player.ExperiencePoints;

                                    insertSavedGame.Parameters.Add("@CurrentLocationID", SqlDbType.Int);
                                    insertSavedGame.Parameters["@CurrentLocationID"].Value = player.CurrentLocation.ID;

                                    insertSavedGame.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                using (SqlCommand updateSavedGame = connection.CreateCommand())
                                {
                                    updateSavedGame.CommandType = CommandType.Text;
                                    updateSavedGame.CommandText =
                                        "UPDATE SavedGame " +
                                        "SET CurrentHitPoints = @CurrentHitPoints, " +
                                        "MaximumHitPoints = @MaximumHitPoints, " +
                                        "Gold = @Gold, " +
                                        "ExperiencePoints = @ExperiencePoints, " +
                                        "CurrentLocationID = @CurrentLocationID";

                                    updateSavedGame.Parameters.Add("@CurrentHitPoints", SqlDbType.Int);
                                    updateSavedGame.Parameters["@CurrentHitPoints"].Value = player.CurrentHitPoints;

                                    updateSavedGame.Parameters.Add("@MaximumHitPoints", SqlDbType.Int);
                                    updateSavedGame.Parameters["@MaximumHitPoints"].Value = player.MaximumHitPoints;

                                    updateSavedGame.Parameters.Add("@Gold", SqlDbType.Int);
                                    updateSavedGame.Parameters["@Gold"].Value = player.Gold;

                                    updateSavedGame.Parameters.Add("@ExperiencePoints", SqlDbType.Int);
                                    updateSavedGame.Parameters["@ExperiencePoints"].Value = player.ExperiencePoints;

                                    updateSavedGame.Parameters.Add("@CurrentLocationID", SqlDbType.Int);
                                    updateSavedGame.Parameters["@CurrentLocationID"].Value = player.CurrentLocation.ID;

                                    updateSavedGame.ExecuteNonQuery();
                                }
                            }
                        }

                        using (SqlCommand deleteQuestsCommand = connection.CreateCommand())
                        {
                            deleteQuestsCommand.CommandType = CommandType.Text;
                            deleteQuestsCommand.CommandText = "DELETE FROM Quest";

                            deleteQuestsCommand.ExecuteNonQuery();
                        }

                        foreach (PlayerQuest playerQuest in player.Quests)
                        {
                            using (SqlCommand insertQuestCommand = connection.CreateCommand())
                            {
                                insertQuestCommand.CommandType = CommandType.Text;
                                insertQuestCommand.CommandText = "INSERT INTO Quest (QuestID, IsCompleted) VALUES (@QuestID, @IsCompleted)";

                                insertQuestCommand.Parameters.Add("@QuestID", SqlDbType.Int);
                                insertQuestCommand.Parameters["@QuestID"].Value = playerQuest.Details.ID;

                                insertQuestCommand.Parameters.Add("@IsCompleted", SqlDbType.Bit);
                                insertQuestCommand.Parameters["@IsCompleted"].Value = playerQuest.IsCompleted;

                                insertQuestCommand.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand deleteInventoryCommand = connection.CreateCommand())
                        {
                            deleteInventoryCommand.CommandType = CommandType.Text;
                            deleteInventoryCommand.CommandText = "DELETE FROM Inventory";

                            deleteInventoryCommand.ExecuteNonQuery();
                        }

                        foreach (InventoryItem inventoryItem in player.Inventory)
                        {
                            using (SqlCommand insertInventoryCommand = connection.CreateCommand())
                            {
                                insertInventoryCommand.CommandType = CommandType.Text;
                                insertInventoryCommand.CommandText = "INSERT INTO Inventory (InventoryItemID, Quantity) VALUES (@InventoryItemID, @Quantity)";

                                insertInventoryCommand.Parameters.Add("@InventoryItemID", SqlDbType.Int);
                                insertInventoryCommand.Parameters["@InventoryItemID"].Value = inventoryItem.Details.ID;
                                insertInventoryCommand.Parameters.Add("@Quantity", SqlDbType.Int);
                                insertInventoryCommand.Parameters["@Quantity"].Value = inventoryItem.Quantity;

                                insertInventoryCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}
