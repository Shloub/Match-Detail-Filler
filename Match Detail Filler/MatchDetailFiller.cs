﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Match_Detail_Filler
{
    public partial class MatchDetailFiller : Form
    {
        // Liquipedia parameters
        static string PLAYER1 = "p1";
        static string PLAYER2 = "p2";
        static string STOCK = "stock";
        static string CHAR = "char";
        static string WIN = "win";
        static string DATE = "date";
        static string DETAILS = "details={{BracketMatchDetails|reddit=|comment=|vod=";

        // Cue banner text
        static string DEFAULT_HEADER_PORT1 = "Port 1";
        static string DEFAULT_HEADER_PORT2 = "Port 2";
        static string DEFAULT_HEADER_PORT3 = "Port 3";
        static string DEFAULT_HEADER_PORT4 = "Port 4";
        static string DEFAULT_HEADER_P1 = "Player 1";
        static string DEFAULT_HEADER_P2 = "Player 2";

        const string COMBOBOX_ENTRY_T1P1 = "T1 P1";
        const string COMBOBOX_ENTRY_T1P2 = "T1 P2";
        const string COMBOBOX_ENTRY_T2P1 = "T2 P1";
        const string COMBOBOX_ENTRY_T2P2 = "T2 P2";

        static int SINGLES_WIDTH = 5;   // Number of textboxes in a row for the singles tab
        static int SINGLES_HEIGHT = 5;  // Number of textboxes in a column for the singles tab
        static int DOUBLES_WIDTH = 9;   // Number of textboxes in a row for the doubles tab
        static int DOUBLES_HEIGHT = 5;  // Number of textboxes in a column for the doubles tab
        static int TAB_NUMBER = 9;      // Where the generated textboxes' tab index should start being numbered from

        static string[] playerSlots = { COMBOBOX_ENTRY_T1P1, COMBOBOX_ENTRY_T1P2, COMBOBOX_ENTRY_T2P1, COMBOBOX_ENTRY_T2P2 };

        int[] teams = new int[4];
        int p1;
        int p2;
        int p3;
        int p4;
        int stocks;
        int invalid = 0;

        string order;

        enum SinglesField { p1char, p2char, stage, p1score, p2score }
        enum DoublesField { t1p1char, t1p2char, t2p1char, t2p2char, stage, t1p1score, t1p2score, t2p1score, t2p2score }

        AutoCompleteStringCollection meleeCharacterAutoCompleteList;
        AutoCompleteStringCollection meleeStageAutoComplete;
        AutoCompleteStringCollection ssbCharacterAutoCompleteList;
        AutoCompleteStringCollection ssbStageAutoComplete;
        AutoCompleteStringCollection wiiuCharacterAutoCompleteList;
        AutoCompleteStringCollection wiiuStageAutoComplete;
        AutoCompleteStringCollection pmCharacterAutoCompleteList;
        AutoCompleteStringCollection pmStageAutoComplete;
        AutoCompleteStringCollection sfvCharacterAutoCompleteList;

        string[] meleeStages = new string[] { "Dream Land", "Final Destination", "Pokémon Stadium", "Battlefield", "Fountain of Dreams", "Yoshi's Story" };
        string[] wiiuStages = new string[] { "Battlefield", "Final Destination", "Smashville", "Dream Land (64)", "Lylat Cruise", "Town and City", "Duck Hunt", "Castle Siege", "Delfino Plaza", "Halberd", "Umbra Clock Tower", "Pokémon Stadium 2",
                                             "Ω Palutena's Temple", "Ω Gaur Plain", "Ω Orbital Gate Assault", "Ω Mushroom Kingdom U", "Ω Mario Galaxy", "Ω Kalos Pokémon League"};
        string[] pmStages = new string[] { "Battlefield", "Smashville", "Pokémon Stadium 2", "Green Hill Zone", "Fountain of Dreams", "Yoshi's Story", "WarioWare, Inc.", "Wario Land", "Yoshi's Island", "Final Destination", "Dream Land", "Norfair", "Skyloft", "Skyworld", "Delfino's Secret", "Dracula's Castle", "Bowser's Castle", "Castle Siege", "Distant Planet", "Metal Cavern", "Rumble Falls", "Lylat Cruise" };
        string[] ssbStages = new string[] { "Dream Land", "Hyrule Castle", "Peach's Castle", "Congo Jungle", "Planet Zebes", "Saffron City" };
        string[] currentStageList;

        // A "matrix" of all generated textboxes in the tab control
        List<TextBox[]> matchList = new List<TextBox[]>();

        List<DoublesBoxAssociation> doublesPlayerList = new List<DoublesBoxAssociation>();

        DoublesBoxAssociation t1p1 = new DoublesBoxAssociation();
        DoublesBoxAssociation t1p2 = new DoublesBoxAssociation();
        DoublesBoxAssociation t2p1 = new DoublesBoxAssociation();
        DoublesBoxAssociation t2p2 = new DoublesBoxAssociation();

        // Constructor
        public MatchDetailFiller()
        {
            InitializeComponent();

            // Set cue text for textbox headers
            SetCueText(textBoxHeaderT1P1, DEFAULT_HEADER_PORT1);
            SetCueText(textBoxHeaderT1P2, DEFAULT_HEADER_PORT2);
            SetCueText(textBoxHeaderT2P1, DEFAULT_HEADER_PORT3);
            SetCueText(textBoxHeaderT2P2, DEFAULT_HEADER_PORT4);
            SetCueText(textBoxHeaderP1, DEFAULT_HEADER_P1);
            SetCueText(textBoxHeaderP2, DEFAULT_HEADER_P2);

            // Initialize the combobox for game selection
            comboBoxGame.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxGame.Items.Add("Melee");
            comboBoxGame.Items.Add("Wii U");
            comboBoxGame.Items.Add("64");
            comboBoxGame.Items.Add("Project M");
            comboBoxGame.Items.Add("SFV");

            // Initialize player slots
            comboBoxPlayer1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlayer1.Items.AddRange(playerSlots);
            comboBoxPlayer2.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlayer2.Items.AddRange(playerSlots);
            comboBoxPlayer3.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlayer3.Items.AddRange(playerSlots);
            comboBoxPlayer4.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPlayer4.Items.AddRange(playerSlots);

            // Create character and stage autocompletes for all games
            meleeCharacterAutoCompleteList = new AutoCompleteStringCollection();
            meleeCharacterAutoCompleteList.AddRange(new string[] { "mario", "luigi", "yoshi", "dk", "link", "samus", "kirby", "fox", "pikachu", "jigglypuff", "puff", "cf", "ness", "peach", "bowser", "doc", "zelda", "sheik", "ganon", "yl", "falco", "mewtwo", "pichu", "ic", "game and watch", "marth", "roy" });

            meleeStageAutoComplete = new AutoCompleteStringCollection();
            meleeStageAutoComplete.AddRange(meleeStages);

            ssbCharacterAutoCompleteList = new AutoCompleteStringCollection();
            ssbCharacterAutoCompleteList.AddRange(new string[] { "mario", "luigi", "yoshi", "dk", "link", "samus", "kirby", "fox", "pikachu", "jigglypuff", "puff", "cf", "ness" });

            ssbStageAutoComplete = new AutoCompleteStringCollection();
            ssbStageAutoComplete.AddRange(ssbStages);

            wiiuCharacterAutoCompleteList = new AutoCompleteStringCollection();
            wiiuCharacterAutoCompleteList.AddRange(new string[] { "mario","luigi","peach","bowser","doc","yoshi","dk","diddy","link","zelda","sheik","ganon","toon link","samus","kirby","zss","mk","fox","dedede","falco","pikachu","jigglypuff","puff","mewtwo","charizard","lucario","cf","ness","lucas","marth","roy","ike","game and watch","pit","wario","olimar","rob","sonic","rosalina","bowser jr","greninja","robin","lucina","corrin","palutena","villager","dark pit","little mac","wii fit","duck hunt","shulk","mega man","pac-man","ryu","cloud","bayonetta","mii brawler","mii swordfighter","mii gunner" });

            wiiuStageAutoComplete = new AutoCompleteStringCollection();
            wiiuStageAutoComplete.AddRange(wiiuStages);

            pmCharacterAutoCompleteList = new AutoCompleteStringCollection();
            pmCharacterAutoCompleteList.AddRange(new string[] { "mario", "luigi", "peach", "bowser", "yoshi", "dk", "diddy", "link", "zelda", "sheik", "ganon", "toon link", "tink", "samus", "zss", "kirby", "meta knight", "mk", "king dedede", "dedede", "fox", "falco", "wolf", "pikachu", "jigglypuff", "puff", "mewtwo", "squirtle", "ivysaur", "charizard", "lucario", "cf", "ness", "lucas", "ic", "marth", "roy", "ike", "mr game and watch", "gw", "pit", "wario", "olimar", "rob", "snake", "sonic" });

            pmStageAutoComplete = new AutoCompleteStringCollection();
            pmStageAutoComplete.AddRange(pmStages);

            sfvCharacterAutoCompleteList = new AutoCompleteStringCollection();
            sfvCharacterAutoCompleteList.AddRange(new string[] { "akuma", "alex", "balrog", "birdie", "cammy", "chun", "dhalsim", "fang", "guile", "ibuki", "juri", "karin", "ken", "kolin", "laura", "bison", "nash", "necalli", "mika", "rashid", "ryu", "urien", "vega", "zangief" });

            // Simulate selecting a tab so that the textboxes will generate for the first time
            tabControl_SelectedIndexChanged(tabControlType, new EventArgs());

            // Set the game
            comboBoxGame.SelectedItem = "Melee";

            // Set the player slots
            comboBoxPlayer1.SelectedItem = COMBOBOX_ENTRY_T1P1;
            comboBoxPlayer2.SelectedItem = COMBOBOX_ENTRY_T1P2;
            comboBoxPlayer3.SelectedItem = COMBOBOX_ENTRY_T2P1;
            comboBoxPlayer4.SelectedItem = COMBOBOX_ENTRY_T2P2;

            t1p1.player = comboBoxPlayer1;
            t1p2.player = comboBoxPlayer2;
            t2p1.player = comboBoxPlayer3;
            t2p2.player = comboBoxPlayer4;

            doublesPlayerList.Add(t1p1);
            doublesPlayerList.Add(t1p2);
            doublesPlayerList.Add(t2p1);
            doublesPlayerList.Add(t2p2);

            AddIndexChangeEventsToPlayerComboBoxes();
        }

        #region Buttons
        // Generate match info for Liquipedia
        private void buttonFill_Click(object sender, EventArgs e)
        {
            string output = string.Empty;
            int matchNumber = 1;

            foreach (TextBox[] match in matchList)
            {
                if (tabControlType.SelectedTab.Text == "Singles")
                {
                    if (comboBoxGame.SelectedItem.ToString() == "SFV")
                    {
                        if (match[(int)SinglesField.p1score].Text != string.Empty || match[(int)SinglesField.p2score].Text != string.Empty)
                        {
                            output += "|" + textBoxMatch.Text + "p1char" + matchNumber + "=" + match[(int)SinglesField.p1char].Text + " ";
                            output += "|" + textBoxMatch.Text + "p2char" + matchNumber + "=" + match[(int)SinglesField.p2char].Text + " ";
                            output += "|" + textBoxMatch.Text + "p1score" + matchNumber + "=" + match[(int)SinglesField.p1score].Text + " ";
                            output += "|" + textBoxMatch.Text + "p2score" + matchNumber + "=" + match[(int)SinglesField.p2score].Text + " ";

                            if (match[(int)SinglesField.p1score].Text != string.Empty && match[(int)SinglesField.p2score].Text != string.Empty)
                            {
                                if (int.Parse(match[(int)SinglesField.p1score].Text) > int.Parse(match[(int)SinglesField.p2score].Text))
                                {
                                    output += "|" + textBoxMatch.Text + "win" + matchNumber + "=1 " + "\r\n";
                                }
                                else
                                {
                                    output += "|" + textBoxMatch.Text + "win" + matchNumber + "=2 " + "\r\n";
                                }
                            }
                            else
                            {
                                output += "|" + textBoxMatch.Text + "win" + matchNumber + "= " + "\r\n";
                            }
                        }
                    }
                    else
                    {
                        if (match[(int)SinglesField.stage].Text != string.Empty)
                        {
                            output += "|" + textBoxMatch.Text + "p1char" + matchNumber + "=" + match[(int)SinglesField.p1char].Text + " ";
                            output += "|" + textBoxMatch.Text + "p2char" + matchNumber + "=" + match[(int)SinglesField.p2char].Text + " ";
                            output += "|" + textBoxMatch.Text + "p1stock" + matchNumber + "=" + match[(int)SinglesField.p1score].Text + " ";
                            output += "|" + textBoxMatch.Text + "p2stock" + matchNumber + "=" + match[(int)SinglesField.p2score].Text + " ";

                            if (match[(int)SinglesField.p1score].Text != string.Empty && match[(int)SinglesField.p2score].Text != string.Empty)
                            {
                                if (int.Parse(match[(int)SinglesField.p1score].Text) > int.Parse(match[(int)SinglesField.p2score].Text))
                                {
                                    output += "|" + textBoxMatch.Text + "win" + matchNumber + "=1 ";
                                }
                                else
                                {
                                    output += "|" + textBoxMatch.Text + "win" + matchNumber + "=2 ";
                                }
                            }
                            else
                            {
                                output += "|" + textBoxMatch.Text + "win" + matchNumber + "= ";
                            }
                            output += "|" + textBoxMatch.Text + "stage" + matchNumber + "=" + match[(int)SinglesField.stage].Text + "\r\n";
                        }
                    }
                }
                else
                {
                    if (match[(int)DoublesField.stage].Text != string.Empty)
                    {
                        // Error check
                        if (CheckComboBoxEntryIntegrity(doublesPlayerList) != string.Empty) richTextBoxOutput.Text = "Invalid Entry";
                        doublesPlayerList = doublesPlayerList.OrderBy(x => x.player.SelectedItem).ToList();

                        // Output each player and score
                        output += "|" + textBoxMatch.Text + "t1p1char" + matchNumber + "=" + doublesPlayerList[0].charList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t1p1stock" + matchNumber + "=" + doublesPlayerList[0].scoreList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t1p2char" + matchNumber + "=" + doublesPlayerList[1].charList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t1p2stock" + matchNumber + "=" + doublesPlayerList[1].scoreList[matchNumber - 1].Text + "\r\n";

                        output += "|" + textBoxMatch.Text + "t2p1char" + matchNumber + "=" + doublesPlayerList[2].charList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t2p1stock" + matchNumber + "=" + doublesPlayerList[2].scoreList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t2p2char" + matchNumber + "=" + doublesPlayerList[3].charList[matchNumber - 1].Text + " ";
                        output += "|" + textBoxMatch.Text + "t2p2stock" + matchNumber + "=" + doublesPlayerList[3].scoreList[matchNumber - 1].Text + " ";

                        if (match[(int)DoublesField.t1p1score].Text != string.Empty && match[(int)DoublesField.t1p2score].Text != string.Empty &&
                            match[(int)DoublesField.t2p1score].Text != string.Empty && match[(int)DoublesField.t2p2score].Text != string.Empty)
                        {
                            if (int.Parse(doublesPlayerList[0].scoreList[matchNumber - 1].Text) + int.Parse(doublesPlayerList[1].scoreList[matchNumber - 1].Text) >
                                int.Parse(doublesPlayerList[2].scoreList[matchNumber - 1].Text) + int.Parse(doublesPlayerList[3].scoreList[matchNumber - 1].Text))
                            {
                                output += "|" + textBoxMatch.Text + "win" + matchNumber + "=1 ";
                            }
                            else
                            {
                                output += "|" + textBoxMatch.Text + "win" + matchNumber + "=2 ";
                            }
                        }
                        else
                        {
                            output += "|" + textBoxMatch.Text + "win" + matchNumber + "= ";
                        }

                        output += "|" + textBoxMatch.Text + "stage" + matchNumber + "=" + match[(int)DoublesField.stage].Text + "\r\n";
                    }
                }

                matchNumber++;
            }

            if (textBoxDate.Text != string.Empty)
            {
                output += "|" + textBoxMatch.Text + DATE + "=" + textBoxDate.Text + "\r\n";
            }

            if (textBoxYoutube.Text != string.Empty)
            {
                output += "|" + textBoxMatch.Text + DETAILS + textBoxYoutube.Text + "}}";
            }

            richTextBoxOutput.Text = output;
        }

        // Clear all textboxes
        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxMatch.Clear();
            textBoxYoutube.Clear();
            richTextBoxOutput.Clear();

            foreach (TextBox box in tabControlType.SelectedTab.Controls.OfType<TextBox>())
            {
                box.Clear();
            }

            RemoveIndexChangeEventsToPlayerComboBoxes();
            comboBoxPlayer1.SelectedItem = COMBOBOX_ENTRY_T1P1;
            comboBoxPlayer2.SelectedItem = COMBOBOX_ENTRY_T1P2;
            comboBoxPlayer3.SelectedItem = COMBOBOX_ENTRY_T2P1;
            comboBoxPlayer4.SelectedItem = COMBOBOX_ENTRY_T2P2;
            AddIndexChangeEventsToPlayerComboBoxes();
        }

        // Trim youtube URLs to remove playlists and other such things
        private void buttonTrim_Click(object sender, EventArgs e)
        {
            int pos = textBoxYoutube.Text.IndexOf("&");

            if (pos != -1)
            {
                textBoxYoutube.Text = textBoxYoutube.Text.Substring(0, pos);
            }

            pos = textBoxYoutube.Text.IndexOf("?list");

            if (pos != -1)
            {
                textBoxYoutube.Text = textBoxYoutube.Text.Substring(0, pos);
            }
        }
        #endregion

        #region textBox Events
        // Capitalize starting letter and respect capitalization
        private void textBoxStage_Leave(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            if (box.Text != string.Empty)
            {
                string letter = box.Text.Substring(0, 1);
                letter = letter.ToUpper();
                box.Text = letter + box.Text.Substring(1);
            }

            // If the stage name matches, match the capitalization
            for (int i = 0; i < currentStageList.Count(); i++)
            {
                if(string.Compare(box.Text, currentStageList[i], true) == 0)
                {
                    box.Text = currentStageList[i];
                    break;
                }
            }
        }

        // Fill in char boxes below if they're empty
        private void textBoxChar_Leave(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            // First row characters only
            if (tabControlType.SelectedTab.Text == "Singles")
            {
                if (box == matchList[0][(int)SinglesField.p1char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)SinglesField.p1char].Text = matchList[0][(int)SinglesField.p1char].Text;
                    }
                }
                else if (box == matchList[0][(int)SinglesField.p2char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)SinglesField.p2char].Text = matchList[0][(int)SinglesField.p2char].Text;
                    }
                }
            }
            else
            {
                if (box == matchList[0][(int)DoublesField.t1p1char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)DoublesField.t1p1char].Text = matchList[0][(int)DoublesField.t1p1char].Text;
                    }
                }
                else if (box == matchList[0][(int)DoublesField.t1p2char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)DoublesField.t1p2char].Text = matchList[0][(int)DoublesField.t1p2char].Text;
                    }
                }
                else if (box == matchList[0][(int)DoublesField.t2p1char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)DoublesField.t2p1char].Text = matchList[0][(int)DoublesField.t2p1char].Text;
                    }
                }
                else if (box == matchList[0][(int)DoublesField.t2p2char])
                {
                    for (int i = 1; i < matchList.Count; i++)
                    {
                        matchList[i][(int)DoublesField.t2p2char].Text = matchList[0][(int)DoublesField.t2p2char].Text;
                    }
                }
            }
            
        }

        // Use the Ω symbol for Wii U Omega stages
        private void textBoxStage_KeyUp(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;

            if (comboBoxGame.Text == "Wii U")
            {
                if (box.Text == "o " || box.Text == "O " || box.Text == "Omega " || box.Text == "omega ")
                {
                    box.Text = "Ω ";
                    box.SelectionStart = box.Text.Length;
                }
            }
        }
        #endregion

        #region Autocomplete
        // Set autocomplete settings based on match type and game type
        private void comboBoxGame_SelectedValueChanged(object sender, EventArgs e)
        {
            if(sender == comboBoxGame)
            {
                if (tabControlType.SelectedTab.Text == "Singles")
                {
                    // Exits the function if nothing is selected. This is guaranteed to happen on form initialization.
                    if (comboBoxGame.SelectedItem == null) return;

                    foreach (TextBox[] match in matchList)
                    {
                        match[(int)SinglesField.stage].Enabled = true;
                    }

                    switch (comboBoxGame.SelectedItem.ToString())
                    {
                        case "Melee":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.stage], meleeStageAutoComplete);
                                currentStageList = meleeStages;
                            }
                            break;
                        case "Wii U":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.stage], wiiuStageAutoComplete);
                                currentStageList = wiiuStages;
                            }
                            break;
                        case "64":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.stage], ssbStageAutoComplete);
                                currentStageList = ssbStages;
                            }
                            break;
                        case "Project M":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.stage], pmStageAutoComplete);
                                currentStageList = pmStages;
                            }
                            break;
                        case "SFV":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], sfvCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], sfvCharacterAutoCompleteList);

                                match[(int)SinglesField.stage].Enabled = false;
                            }
                            break;
                    }
                }
                else if (tabControlType.SelectedTab.Text == "Doubles")
                {
                    switch (comboBoxGame.SelectedItem.ToString())
                    {
                        case "Melee":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p1char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p2char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p1char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p2char], meleeCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.stage], meleeStageAutoComplete);
                                currentStageList = meleeStages;
                            }
                            break;
                        case "Wii U":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p1char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p2char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p1char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p2char], wiiuCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.stage], wiiuStageAutoComplete);
                                currentStageList = wiiuStages;
                            }
                            break;
                        case "64":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p1char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p2char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p1char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p2char], ssbCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.stage], ssbStageAutoComplete);
                                currentStageList = ssbStages;
                            }
                            break;
                        case "Project M":
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p1char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t1p2char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p1char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.t2p2char], pmCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)DoublesField.stage], pmStageAutoComplete);
                                currentStageList = pmStages;
                            }
                            break;
                        case "SFV":
                            tabControlType.SelectTab(0);
                            foreach (TextBox[] match in matchList)
                            {
                                SetTextboxAutoComplete(match[(int)SinglesField.p1char], sfvCharacterAutoCompleteList);
                                SetTextboxAutoComplete(match[(int)SinglesField.p2char], sfvCharacterAutoCompleteList);
                            }
                            break;
                    }
                }
            }
        }

        // Enable autocomplete for a given textbox
        private void SetTextboxAutoComplete(TextBox box, AutoCompleteStringCollection autocompleteList)
        {
            box.AutoCompleteCustomSource = autocompleteList;
            box.AutoCompleteMode = AutoCompleteMode.Append;
            box.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }
        #endregion

        // Alter the form depending on whether the singles or doubles tab is selected
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tabs = (TabControl)sender;

            // Remove all existing textboxes except for the player name fields
            foreach (TextBox[] row in matchList)
            {
                if (row.Length == SINGLES_WIDTH)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        // Remove character auto-population event
                        if (i == (int)SinglesField.p1char || i == (int)SinglesField.p2char)
                        {
                            row[i].Leave -= new EventHandler(textBoxChar_Leave);
                        }

                        // Remove stage auto-capitalization event
                        if (i == (int)SinglesField.stage)
                        {
                            row[i].Leave -= new EventHandler(textBoxStage_Leave);
                        }

                        tabs.Controls.Remove(row[i]);
                        row[i].Dispose();
                    }
                }
                else if (row.Length == DOUBLES_WIDTH)
                {
                    for (int i = 0; i < row.Length; i++)
                    {
                        // Remove character auto-population event
                        if (i == (int)DoublesField.t1p1char || i == (int)DoublesField.t1p2char || i == (int)DoublesField.t2p1char || i == (int)DoublesField.t2p2char)
                        {
                            row[i].Leave -= new EventHandler(textBoxChar_Leave);
                        }

                        // Remove stage auto-capitalization event
                        if (i == (int)DoublesField.stage)
                        {
                            row[i].Leave -= new EventHandler(textBoxStage_Leave);
                        }

                        tabs.Controls.Remove(row[i]);
                        row[i].Dispose();
                    }
                }
            }

            matchList.Clear();

            // Create new textboxes depending on the selected tab
            if (tabControlType.SelectedTab.Text == "Singles")
            {
                // Set form width
                this.MinimumSize = new Size(562, 480);
                this.Width = 562;

                // Set tab box size
                this.tabControlType.Size = new System.Drawing.Size(520, 190);
                
                // Set base textbox properties
                for (int i = 0; i < SINGLES_HEIGHT; i++)
                {
                    TextBox[] newTextBoxArray = new TextBox[SINGLES_WIDTH];
                    int lastLeft = 0;
                    for (int j = 0; j < SINGLES_WIDTH; j++)
                    {
                        TextBox newTextBox = new TextBox();

                        // Set character auto-population for the first row
                        if (j == (int)SinglesField.p1char || j == (int)SinglesField.p2char)
                        {
                            if (i == 0)
                            {
                                newTextBox.Leave += new EventHandler(textBoxChar_Leave);
                            }
                        }

                        // Set auto-capitalization for stages
                        if (j == (int)SinglesField.stage)
                        {
                            newTextBox.Leave += new EventHandler(textBoxStage_Leave);
                            newTextBox.KeyUp += new KeyEventHandler(textBoxStage_KeyUp);
                        }

                        // Score/stock textboxes need to be smaller
                        if (j == (int)SinglesField.p1score || j == (int)SinglesField.p2score)
                        {
                            newTextBox.Width = 47;
                            newTextBox.Left = lastLeft + 6;
                        }
                        else
                        {
                            newTextBox.Width = 100;
                            newTextBox.Left = lastLeft + 6;
                        }

                        newTextBox.Height = 20;
                        newTextBox.Top = 32 + 26 * i;

                        // Keep track of the last textbox position
                        lastLeft = newTextBox.Left + newTextBox.Width;

                        newTextBoxArray[j] = newTextBox;
                        tabPageSingles.Controls.Add(newTextBox);
                    }

                    matchList.Add(newTextBoxArray);
                }

                // Set the tab order for easy tab navigation
                int tabNumber = TAB_NUMBER;
                foreach (TextBox[] match in matchList)
                {
                    for (int i = 0; i < SINGLES_WIDTH; i++)
                    {
                        match[i].TabIndex = tabNumber;
                        tabNumber++;
                    }
                }
            }
            else
            {
                // Set form width
                this.Width = 802;
                this.MinimumSize = new Size(802, 480);

                // Set box size
                this.tabControlType.Size = new System.Drawing.Size(760, 220);

                // Set base textbox properties
                for (int i = 0; i < DOUBLES_HEIGHT; i++)
                {
                    TextBox[] newTextBoxArray = new TextBox[DOUBLES_WIDTH];
                    int lastLeft = 0;
                    for (int j = 0; j < DOUBLES_WIDTH; j++)
                    {
                        TextBox newTextBox = new TextBox();

                        // Set character auto-population for the first row
                        if (j == (int)DoublesField.t1p1char || j == (int)DoublesField.t1p2char || j == (int)DoublesField.t2p1char || j == (int)DoublesField.t2p2char)
                        {
                            if (i == 0)
                            {
                                newTextBox.Leave += new EventHandler(textBoxChar_Leave);
                            }
                        }

                        // Set auto-capitalization for stages
                        if (j == (int)DoublesField.stage)
                        {
                            newTextBox.Leave += new EventHandler(textBoxStage_Leave);
                        }

                        // Score/stock textboxes need to be smaller
                        if (j == (int)DoublesField.t1p1score || j == (int)DoublesField.t1p2score || j == (int)DoublesField.t2p1score || j == (int)DoublesField.t2p2score)
                        {
                            newTextBox.Width = 47;
                            newTextBox.Left = lastLeft + 6;
                        }
                        else
                        {
                            newTextBox.Width = 100;
                            newTextBox.Left = lastLeft + 6;
                        }

                        // Set groupings for characters and score
                        SetDoublesBoxGroupings(ref newTextBox, (DoublesField)j);

                        newTextBox.Height = 20;
                        newTextBox.Top = 58 + 26 * i;

                        // Keep track of the last textbox position
                        lastLeft = newTextBox.Left + newTextBox.Width;

                        newTextBoxArray[j] = newTextBox;
                        tabPageDoubles.Controls.Add(newTextBox);
                    }

                    matchList.Add(newTextBoxArray);
                }

                // Set the tab order for easy tab navigation
                int tabNumber = TAB_NUMBER;
                foreach (TextBox[] match in matchList)
                {
                    for (int i = 0; i < DOUBLES_WIDTH; i++)
                    {
                        match[i].TabIndex = tabNumber;
                        tabNumber++;
                    }
                }
            }

            // Add autocomplete for all relevant textboxes
            comboBoxGame_SelectedValueChanged(comboBoxGame, new EventArgs());
        }

        #region Cue Banner
        // https://jasonkemp.ca/blog/the-missing-net-1-cue-banners-in-windows-forms-em_setcuebanner-text-prompt/
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg,
        int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        public static void SetCueText(Control control, string text)
        {
            SendMessage(control.Handle, EM_SETCUEBANNER, 0, text);
        }
        #endregion

        /// <summary>
        /// Reassigns doubles combobox values so that all values are filled
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxPlayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox thisBox = (ComboBox)sender;

            // If no dupes exist, return
            string missingSlot = CheckComboBoxEntryIntegrity(doublesPlayerList);
            if (missingSlot == string.Empty) return;

            // Fill the duplicate combobox with the missing slot
            foreach (DoublesBoxAssociation assoc in doublesPlayerList)
            {
                if (assoc.player == thisBox) continue;

                if (assoc.player.SelectedItem == thisBox.SelectedItem)
                {
                    assoc.player.SelectedItem = missingSlot;
                    break;
                }
            }
        }

        /// <summary>
        /// Adds the Index Changed event to Doubles comboboxes
        /// </summary>
        private void AddIndexChangeEventsToPlayerComboBoxes()
        {
            foreach (DoublesBoxAssociation assoc in doublesPlayerList)
            {
                assoc.player.SelectedIndexChanged += new EventHandler(comboBoxPlayer_SelectedIndexChanged);
            }
        }

        /// <summary>
        /// Removes the Index Changed event to Doubles comboboxes
        /// </summary>
        private void RemoveIndexChangeEventsToPlayerComboBoxes()
        {
            foreach (DoublesBoxAssociation assoc in doublesPlayerList)
            {
                assoc.player.SelectedIndexChanged -= comboBoxPlayer_SelectedIndexChanged;
            }
        }

        /// <summary>
        /// Checks if all doubles fields have been filled
        /// </summary>
        /// <param name="doublesPlayerList"></param>
        /// <returns>The string of the missing field. Otherwise, an empty string is returned.</returns>
        private string CheckComboBoxEntryIntegrity(List<DoublesBoxAssociation> doublesPlayerList)
        {
            // Find the missing value
            bool t1p1_exists = false;
            bool t1p2_exists = false;
            bool t2p1_exists = false;
            bool t2p2_exists = false;
            foreach (DoublesBoxAssociation assoc in doublesPlayerList)
            {
                switch ((string)assoc.player.SelectedItem)
                {
                    case COMBOBOX_ENTRY_T1P1:
                        t1p1_exists = true;
                        break;
                    case COMBOBOX_ENTRY_T1P2:
                        t1p2_exists = true;
                        break;
                    case COMBOBOX_ENTRY_T2P1:
                        t2p1_exists = true;
                        break;
                    case COMBOBOX_ENTRY_T2P2:
                        t2p2_exists = true;
                        break;
                }
            }

            if (!t1p1_exists)
            {
                return COMBOBOX_ENTRY_T1P1;
            }
            else if (!t1p2_exists)
            {
                return COMBOBOX_ENTRY_T1P2;
            }
            else if (!t2p1_exists)
            {
                return COMBOBOX_ENTRY_T2P1;
            }
            else if (!t2p2_exists)
            {
                return COMBOBOX_ENTRY_T2P2;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Assigns textboxes to DoublesBoxAssociation items
        /// </summary>
        /// <param name="box"></param>
        /// <param name="field"></param>
        private void SetDoublesBoxGroupings(ref TextBox box, DoublesField field)
        {
            switch (field)
            {
                case DoublesField.t1p1char:
                    t1p1.charList.Add(box);
                    break;
                case DoublesField.t1p2char:
                    t1p2.charList.Add(box);
                    break;
                case DoublesField.t2p1char:
                    t2p1.charList.Add(box);
                    break;
                case DoublesField.t2p2char:
                    t2p2.charList.Add(box);
                    break;
                case DoublesField.t1p1score:
                    t1p1.scoreList.Add(box);
                    break;
                case DoublesField.t1p2score:
                    t1p2.scoreList.Add(box);
                    break;
                case DoublesField.t2p1score:
                    t2p1.scoreList.Add(box);
                    break;
                case DoublesField.t2p2score:
                    t2p2.scoreList.Add(box);
                    break;
            }
        }
    }
}
