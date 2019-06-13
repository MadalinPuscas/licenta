using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Engine;
using System.IO;
using Engine.cs;

namespace AdventureGame2
{
    public partial class AdventureGame2 : Form
    {
        private Player player;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public AdventureGame2()
        {
            InitializeComponent();

            player = PlayerDataMapper.CreateFromDatabase();

            if(player == null)
            {
                if(File.Exists(PLAYER_DATA_FILE_NAME))
                {
                    player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
                }
                else
                {
                    player = Player.CreateDefaultPlayer();
                }
            }


            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", player, "Level");

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Quantity",
                DataPropertyName = "Quantity"
            });

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

            cboWeapons.DataSource = player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (player.CurrentWeapon != null)
            {
                cboWeapons.SelectedItem = player.CurrentWeapon;
            }

            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

            cboPotions.DataSource = player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += DisplayMessage;

            player.MoveTo(player.CurrentLocation);
        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

            if (messageEventArgs.AddExtraNewLine)
            {
                rtbMessages.Text += Environment.NewLine;
            }

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = player.Weapons;

                if (!player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.DataSource = player.Potions;

                if (!player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                btnTrade.Visible = (player.CurrentLocation.VendorWorkingHere != null);
                btnNorth.Visible = (player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = (player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = (player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = (player.CurrentLocation.LocationToWest != null);

                rtbLocation.Text = player.CurrentLocation.Name + Environment.NewLine;
                rtbLocation.Text += player.CurrentLocation.Description + Environment.NewLine;

                if (player.CurrentLocation.MonsterLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = player.Weapons.Any();
                    cboPotions.Visible = player.Potions.Any();
                    btnUseWeapon.Visible = player.Weapons.Any();
                    btnUsePotion.Visible = player.Potions.Any();
                }
            }
        }

        private void AdventureGame_Load(object sender, EventArgs e)
        {

        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            player.MoveNorth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            player.MoveEast();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            player.MoveSouth();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            player.MoveWest();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            player.UsePotions(potion);
        }

        private void AdventureGame2_FormClosing(object sender, FormClosingEventArgs e)
        {
            //File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());

            PlayerDataMapper.SaveToDatabase(player);
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            TradingScreen tradingScreen = new TradingScreen(player);
            tradingScreen.StartPosition = FormStartPosition.CenterParent;
            tradingScreen.ShowDialog(this);
        }
    }
}
