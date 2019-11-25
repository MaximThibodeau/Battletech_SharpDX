using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4
{
    public partial class Game_Starter : Form
    {
        //**************************************************************** Galaxy *******************************************************

        Font m = new Font("Arial", 10, FontStyle.Regular);
        float scaleMap = 0.5f;
        int MapX = 0;
        int MapY = 0;
        int LMapX = 0;
        int LMapY = 0;

        int MouseScrolling = 0;


        //***************************************************************
        SwapChainDescription desc;
        SwapChain swapChain;
        SharpDX.Direct3D11.Device device;
        SharpDX.Direct3D11.DeviceContext context;

        Thread G;

        //***************************************************************
        Matrix proj;
        Matrix view;

        Static_Object SO;


        //****************************************************************


        public int m_Client_Nbr = -1;

        Shooter F;

        Socket m_clientSocket;
        public AsyncCallback m_pfnCallBack;
        IAsyncResult m_result;

        char m_Delim_End = '\u265E';
        char m_Delim_Message = '\u262F';
        char m_Delim_Start = '\u0506';


        List<MechClass> CustomMechs = new List<MechClass>();
        MechClass CustomMech = new MechClass();

        float[][] Engines_Data;

        string IconPath = "";
        string ImagePath = "";

        public string frameMessage = "";


        public Game_Starter()
        {
            InitializeComponent();

            this.panel4.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.panel4_MouseWheel);

            string[] lines = System.IO.File.ReadAllLines(".\\Data\\Engines.csv");

            Engines_Data = new float[79][];
            int cntr = 0;
            for (int i = 10; i <= 400; i += 5)
            {
                Engines_Data[cntr] = new float[5];
                for (int j = 0; j < 5; j++)
                {
                    Engines_Data[cntr][j] = float.Parse(lines[cntr].Split(',')[j]);
                }
                cntr++;
            }

            if (CustomMech.Tech == "Clan") Load_Weapons(true);
            else Load_Weapons(false);

            DirectoryInfo dir = new DirectoryInfo(".\\Data\\MechData\\");
            foreach (FileInfo file in dir.GetFiles())
                listBox6.Items.Add(file.Name);

            //*******************
            string[] data = File.ReadAllLines(".\\Data\\TempMaximDebug.txt");

            foreach (string S in data)
            {
                MechClass Mech = new MechClass();
                Mech.Set_Mech_String(S);
                CustomMechs.Add(Mech);
            }
            //*******************

            //FakeButtonConnectClick();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 F = new Form1(int.Parse(textBox1.Text), CustomMechs);
        }

        //********************************************************************************************************* Force **********************************************************************************************
        Hierarchy H;
        //********************************************************************************************************* Custom Mech *****************************************************************************************

        private void textBox56_Leave(object sender, EventArgs e)
        {
            CustomMech.MechType = textBox56.Text;
        }

        private void textBox49_Leave(object sender, EventArgs e)
        {
            CustomMech.WarriorName = textBox49.Text;
        }

        private void textBox55_Leave(object sender, EventArgs e)
        {
            int res;
            if (int.TryParse(textBox55.Text, out res))
            {
                if (res % 5 == 0 && res > 15 && res < 105)
                {
                    CustomMech.Weight = res;

                    textBox55.Text = res.ToString();
                    textBox68.Text = res.ToString();

                    textBox60.Text = Data_Mech.Get_Structural_Armor_Data(res, 3).ToString();
                    textBox61.Text = Data_Mech.Get_Structural_Armor_Data(res, 3).ToString();

                    textBox62.Text = Data_Mech.Get_Structural_Armor_Data(res, 2).ToString();
                    textBox63.Text = Data_Mech.Get_Structural_Armor_Data(res, 2).ToString();

                    textBox64.Text = Data_Mech.Get_Structural_Armor_Data(res, 1).ToString();
                    textBox65.Text = Data_Mech.Get_Structural_Armor_Data(res, 1).ToString();

                    textBox66.Text = Data_Mech.Get_Structural_Armor_Data(res, 0).ToString();

                    textBox67.Text = "3";

                    textBox57.Text = (2 * Data_Mech.Get_Structural_Armor_Data(res, 3) + 2 * Data_Mech.Get_Structural_Armor_Data(res, 2) + 2 * Data_Mech.Get_Structural_Armor_Data(res, 1) + Data_Mech.Get_Structural_Armor_Data(res, 0) + 3).ToString();
                }
                else textBox55.Text = "100";
            }
            else textBox55.Text = "100";
        }

        private void comboBox13_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.Tech = comboBox13.SelectedItem.ToString();

            //MessageBox.Show(CustomMech.Tech);
            listView5.Items.Clear();
            if (CustomMech.Tech == "Clan") Load_Weapons(true);
            else Load_Weapons(false);
        }

        private void comboBox14_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.InternalStruct = comboBox14.SelectedItem.ToString();
        }

        private void comboBox6_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.EngineType = comboBox6.SelectedItem.ToString();
        }

        private void textBox52_Leave(object sender, EventArgs e)
        {
            int res;
            if (int.TryParse(textBox52.Text, out res))
            {
                if (res % 5 == 0 && res > 5 && res < 405)
                {
                    textBox52.Text = res.ToString();

                    int etype = 0;
                    if (CustomMech.EngineType == "Compact") etype = 1;
                    else if (CustomMech.EngineType == "Standard") etype = 2;
                    else if (CustomMech.EngineType == "Light") etype = 3;
                    else if (CustomMech.EngineType == "XL") etype = 4;
                    else MessageBox.Show("Erreur de nom de Engine!");
                    CustomMech.EngineMass = Engines_Data[res / 5 - 2][etype];
                    CustomMech.EngineRating = res;
                    textBox53.Text = CustomMech.EngineMass.ToString();
                    CustomMech.Speed = (float)CustomMech.EngineRating / (float)CustomMech.Weight * 15.0f;
                    textBox54.Text = CustomMech.Speed.ToString();
                }
                else textBox52.Text = "300";
            }
            else textBox52.Text = "300";

            textBox78.Text = ((int)Math.Floor(int.Parse(textBox52.Text) / (float)CustomMech.Weight)).ToString();
            textBox79.Text = ((int)Math.Ceiling(int.Parse(textBox78.Text) * 1.5f)).ToString();
        }

        private void comboBox7_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.HeatSinkType = comboBox7.SelectedItem.ToString();
        }

        private void textBox50_Leave(object sender, EventArgs e)
        {
            CustomMech.HeatSinkSupp = int.Parse(textBox50.Text);
        }

        private void comboBox8_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.GyroType = comboBox8.SelectedItem.ToString();
        }

        private void comboBox10_SelectedValueChanged(object sender, EventArgs e)
        {
            CustomMech.CockpitType = comboBox10.SelectedItem.ToString();
        }

        private void Load_Weapons(bool clan)
        {
            string[] lines = System.IO.File.ReadAllLines(".\\Data\\Armements.csv");

            int nbr_records = int.Parse(lines[0].Split(';')[0]);

            for (int i = 0; i < nbr_records; i++)
            {
                if (clan)
                {
                    if (lines[i + 1].Split(';')[12] == "TRUE")
                    {
                        ListViewItem LVI = new ListViewItem(lines[i + 1].Split(';')[1]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[0]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[2]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[3]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[4]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[5]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[6]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[7]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[13]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[8]); // ammo name
                        LVI.SubItems.Add(lines[i + 1].Split(';')[9]); // ammo per ton
                        LVI.SubItems.Add(lines[i + 1].Split(';')[10]); // ammo cost

                        listView5.Items.Add(LVI);
                    }
                }
                else
                {
                    if (lines[i + 1].Split(';')[11] == "TRUE")
                    {
                        ListViewItem LVI = new ListViewItem(lines[i + 1].Split(';')[1]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[0]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[2]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[3]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[4]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[5]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[6]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[7]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[13]);
                        LVI.SubItems.Add(lines[i + 1].Split(';')[8]); // ammo name
                        LVI.SubItems.Add(lines[i + 1].Split(';')[9]); // ammo per ton
                        LVI.SubItems.Add(lines[i + 1].Split(';')[10]); // ammo cost

                        listView5.Items.Add(LVI);
                    }
                }
            }
        }

        private void button35_Click(object sender, EventArgs e)
        {

            if (listView5.SelectedItems.Count == 1)
            {
                listView4.Items.Add((ListViewItem)listView5.SelectedItems[0].Clone());
                listView4.Items[listView4.Items.Count - 1].SubItems.Add("");
                listBox20.Items.Add(listView5.SelectedItems[0].SubItems[8].Text + "#" + listView5.SelectedItems[0].SubItems[6].Text + "#" + listView5.SelectedItems[0].SubItems[2].Text + "#" + listView5.SelectedItems[0].SubItems[3].Text + "#" + listView5.SelectedItems[0].SubItems[4].Text);
            }
        }

        private void button36_Click(object sender, EventArgs e)
        {
            if (listView5.SelectedItems.Count == 1)
            {
                listView7.Items.Add((ListViewItem)listView5.SelectedItems[0].Clone());
                listBox20.Items.Add("Ammo " + listView5.SelectedItems[0].SubItems[8].Text + "#" + listView5.SelectedItems[0].SubItems[6].Text);
            }
        }

        private void button37_Click(object sender, EventArgs e)
        {
            listBox20.Items.Clear();
        }

        private void listBox17_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = e.AllowedEffect;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox16_DragDrop(object sender, DragEventArgs e)
        {
            string source = (string)e.Data.GetData(DataFormats.Text);
            string[] data = source.Split(new string[] { "#" }, StringSplitOptions.None);
            int dupli = int.Parse(data[1]);

            bool test = false;
            if (((ListBox)sender).Name == "listBox16" || ((ListBox)sender).Name == "listBox17")
            {
                if (12 - ((ListBox)sender).Items.Count >= dupli) test = true;
            }

            if (((ListBox)sender).Name == "listBox12" || ((ListBox)sender).Name == "listBox13" || ((ListBox)sender).Name == "listBox19")
            {
                if (6 - ((ListBox)sender).Items.Count >= dupli) test = true;
            }

            if (((ListBox)sender).Name == "listBox14" || ((ListBox)sender).Name == "listBox15")
            {
                if (12 - ((ListBox)sender).Items.Count >= dupli) test = true;
            }

            if (((ListBox)sender).Name == "listBox18")
            {
                if (6 - ((ListBox)sender).Items.Count >= dupli) test = true;
            }

            if (test)
            {
                if (data[0].Contains("Ammo"))
                {
                    ((ListBox)sender).Items.Add(data[0]);
                }
                else
                {

                    for (int i = 0; i < dupli; i++)
                    {
                        if (i == 0) ((ListBox)sender).Items.Add(data[0]);
                        else ((ListBox)sender).Items.Add("-" + data[0]);
                    }
                }

                int cntr = 0;
                int rem = -1;
                foreach (string s in listBox20.Items)
                {
                    if (s == source) rem = cntr;
                    cntr++;
                }

                listBox20.Items.RemoveAt(rem);

                if (data.Count() > 3)
                {
                    if (((ListBox)sender).Name == "listBox15") Add_Weapon(data[0] + "#LA" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                    if (((ListBox)sender).Name == "listBox17") Add_Weapon(data[0] + "#LT" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                    if (((ListBox)sender).Name == "listBox19") Add_Weapon(data[0] + "#HE" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                    if (((ListBox)sender).Name == "listBox18") Add_Weapon(data[0] + "#CT" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                    if (((ListBox)sender).Name == "listBox16") Add_Weapon(data[0] + "#RT" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                    if (((ListBox)sender).Name == "listBox14") Add_Weapon(data[0] + "#RA" + "#" + data[2] + "#" + data[3] + "#" + data[4]);
                }

                for (int i = 0; i < listView4.Items.Count; i++)
                {
                    if (listView4.Items[i].SubItems[8].Text == data[0] && listView4.Items[i].SubItems[12].Text == "")
                    {
                        if (((ListBox)sender).Name == "listBox15") listView4.Items[i].SubItems[12].Text = "LA";
                        if (((ListBox)sender).Name == "listBox17") listView4.Items[i].SubItems[12].Text = "LT";
                        if (((ListBox)sender).Name == "listBox19") listView4.Items[i].SubItems[12].Text = "HE";
                        if (((ListBox)sender).Name == "listBox18") listView4.Items[i].SubItems[12].Text = "CT";
                        if (((ListBox)sender).Name == "listBox16") listView4.Items[i].SubItems[12].Text = "RT";
                        if (((ListBox)sender).Name == "listBox14") listView4.Items[i].SubItems[12].Text = "RA";
                    }
                }
            }
        }

        private void Add_Weapon(string wpn)
        {
            listBox7.Items.Add(wpn);
            listBox8.Items.Add(wpn);
            listBox9.Items.Add(wpn);
            listBox10.Items.Add(wpn);
            listBox11.Items.Add(wpn);
        }

        private void listBox20_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (true) //listBox5.SelectedIndices.Count == 1)
                {
                    string source = listBox20.SelectedItems[0].ToString();
                    DataObject data = new DataObject(DataFormats.Text, source);
                    ((ListBox)sender).DoDragDrop(data, DragDropEffects.Copy);
                }
            }
        }

        private void listBox6_DoubleClick(object sender, EventArgs e)
        {
            if (listBox6.SelectedItems.Count == 1)
            {
                textBox39.Text = listBox6.SelectedItems[0].ToString();
                CustomMech.ModelFilename = textBox39.Text;
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            textBox_Head_Armor.Text = "9";
            textBox_CenterTorso_Armor.Text = ((int)Math.Ceiling(4.0f * int.Parse(textBox66.Text) / 3.0f)).ToString();
            textBox71.Text = (2 * int.Parse(textBox66.Text) - int.Parse(textBox_CenterTorso_Armor.Text)).ToString();

            textBox77.Text = ((int)Math.Ceiling(4.0f * int.Parse(textBox65.Text) / 3.0f)).ToString();
            textBox70.Text = (2 * int.Parse(textBox65.Text) - int.Parse(textBox77.Text)).ToString();
            textBox76.Text = ((int)Math.Ceiling(4.0f * int.Parse(textBox65.Text) / 3.0f)).ToString();
            textBox69.Text = (2 * int.Parse(textBox65.Text) - int.Parse(textBox76.Text)).ToString();

            textBox72.Text = (2 * int.Parse(textBox60.Text)).ToString();
            textBox73.Text = (2 * int.Parse(textBox61.Text)).ToString();

            textBox74.Text = (2 * int.Parse(textBox62.Text)).ToString();
            textBox75.Text = (2 * int.Parse(textBox63.Text)).ToString();


            textBox59.Text = (int.Parse(textBox_Head_Armor.Text) + int.Parse(textBox_CenterTorso_Armor.Text) + int.Parse(textBox71.Text) + int.Parse(textBox77.Text) + int.Parse(textBox70.Text) + int.Parse(textBox76.Text) + int.Parse(textBox69.Text) + int.Parse(textBox72.Text) + int.Parse(textBox73.Text) + int.Parse(textBox74.Text) + int.Parse(textBox75.Text)).ToString();
            textBox58.Text = ((float)(Math.Ceiling(int.Parse(textBox59.Text) / (16 * CustomMech.Get_Armor_Factor()) * 2) / 2.0f)).ToString();

            Save_Armor();
        }

        private void textBox_Head_Armor_Leave(object sender, EventArgs e)
        {
            textBox59.Text = (int.Parse(textBox_Head_Armor.Text) + int.Parse(textBox_CenterTorso_Armor.Text) + int.Parse(textBox71.Text) + int.Parse(textBox77.Text) + int.Parse(textBox70.Text) + int.Parse(textBox76.Text) + int.Parse(textBox69.Text) + int.Parse(textBox72.Text) + int.Parse(textBox73.Text) + int.Parse(textBox74.Text) + int.Parse(textBox75.Text)).ToString();
            textBox58.Text = ((float)(Math.Ceiling(int.Parse(textBox59.Text) / (16 * CustomMech.Get_Armor_Factor()) * 2) / 2.0f)).ToString();

            Save_Armor();
        }

        private void Save_Armor()
        {
            CustomMech.A_Head = int.Parse(textBox_Head_Armor.Text);

            CustomMech.A_CTorso = int.Parse(textBox_CenterTorso_Armor.Text);
            CustomMech.A_LTorso = int.Parse(textBox76.Text);
            CustomMech.A_RTorso = int.Parse(textBox77.Text);

            CustomMech.A_CTorsoB = int.Parse(textBox71.Text);
            CustomMech.A_LTorsoB = int.Parse(textBox69.Text);
            CustomMech.A_RTorsoB = int.Parse(textBox70.Text);

            CustomMech.A_LArm = int.Parse(textBox74.Text);
            CustomMech.A_RArm = int.Parse(textBox75.Text);

            CustomMech.A_LLeg = int.Parse(textBox72.Text);
            CustomMech.A_RLeg = int.Parse(textBox73.Text);
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            CustomMech.ArmorType = comboBox4.SelectedItem.ToString();

            textBox59.Text = (int.Parse(textBox_Head_Armor.Text) + int.Parse(textBox_CenterTorso_Armor.Text) + int.Parse(textBox71.Text) + int.Parse(textBox77.Text) + int.Parse(textBox70.Text) + int.Parse(textBox76.Text) + int.Parse(textBox69.Text) + int.Parse(textBox72.Text) + int.Parse(textBox73.Text) + int.Parse(textBox74.Text) + int.Parse(textBox75.Text)).ToString();
            textBox58.Text = ((float)(Math.Ceiling(int.Parse(textBox59.Text) / (16 * CustomMech.Get_Armor_Factor()) * 2) / 2.0f)).ToString();

            Save_Armor();
        }

        private void listBox7_MouseClick(object sender, MouseEventArgs e)
        {
            List<Wpn_Grp> tmp = new List<Wpn_Grp>();
            foreach (string s in listBox7.SelectedItems)
            {
                Wpn_Grp w = new Wpn_Grp();
                string[] S = s.Split('#');

                w.heat = int.Parse(S[2]);
                w.dmg = int.Parse(S[3]);
                w.rangeMax = int.Parse(S[4].Split('/')[2]);
                w.rangeMid = int.Parse(S[4].Split('/')[1]);
                w.rangeExtr = int.Parse(S[4].Split('/')[3]);
                w.rangeMin = int.Parse(S[4].Split('/')[0]);
                w.position = S[1];
                w.name = S[0];
                w.WpnType = Get_Wpn_Type(S[0]);

                tmp.Add(w);

            }

            CustomMech.Weapons = tmp;
        }

        private void listBox8_MouseClick(object sender, MouseEventArgs e)
        {
            List<Wpn_Grp> tmp = new List<Wpn_Grp>();
            foreach (string s in listBox8.SelectedItems)
            {
                Wpn_Grp w = new Wpn_Grp();
                string[] S = s.Split('#');

                w.heat = int.Parse(S[2]);
                w.dmg = int.Parse(S[3]);
                w.rangeMax = int.Parse(S[4].Split('/')[2]);
                w.rangeMid = int.Parse(S[4].Split('/')[1]);
                w.rangeExtr = int.Parse(S[4].Split('/')[3]);
                w.rangeMin = int.Parse(S[4].Split('/')[0]);
                w.position = S[1];
                w.name = S[0];
                w.WpnType = Get_Wpn_Type(S[0]);
                //MessageBox.Show("lb8");
                tmp.Add(w);
            }

            CustomMech.Weapons2 = tmp;
        }

        private int Get_Wpn_Type(string s)
        {
            int tmp = 0;

            if (s.Contains("PPC"))
            {
                if (s.Contains("Light")) tmp = 1;
                else if (s.Contains("Heavy")) tmp = 2;
            }
            else if (s.Contains("Ultra") || s.Contains("Rotary") || s.Contains("Light"))
            {
                if (s.Contains("AC/2") || s.Contains("AC/5")) tmp = 3;
                else if (s.Contains("AC/10") || s.Contains("AC/20")) tmp = 4;
            }
            else if (s.Contains("Autocannon"))
            {
                if (s.Contains("/2") || s.Contains("/5")) tmp = 3;
                else if (s.Contains("/10") || s.Contains("/20")) tmp = 4;
            }
            else if (s.Contains("LB"))
            {
                if (s.Contains("2")) tmp = 5;
                else if (s.Contains("5")) tmp = 6;
                else if (s.Contains("10")) tmp = 7;
                else if (s.Contains("20")) tmp = 8;
            }
            else if (s.Contains("LRM"))
            {
                if (s.Contains("5")) tmp = 9;
                else if (s.Contains("10")) tmp = 10;
                else if (s.Contains("15")) tmp = 11;
                else if (s.Contains("20")) tmp = 12;
            }
            else if (s.Contains("Gauss"))
            {
                tmp = 13;
            }
            else if (s.Contains("Laser"))
            {
                if (s.Contains("Micro")) tmp = 14;
                else if (s.Contains("Small")) tmp = 15;
                else if (s.Contains("Medium")) tmp = 16;
                else if (s.Contains("Large")) tmp = 17;
            }


            return tmp;
        }

        private void button40_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - Application.ExecutablePath.Split('\\')[Application.ExecutablePath.Split('\\').Count() - 1].Length) + "Data\\Logos\\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                IconPath = openFileDialog1.FileName;
                //textBox24.Text = IconPath.Split('\\')[IconPath.Split('\\').Count() - 1];
                pictureBox4.ImageLocation = IconPath;

                CustomMech.Portrait = new Bitmap(IconPath);
                CustomMech.PortraitPath = IconPath;
            }
        }

        private void button41_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - Application.ExecutablePath.Split('\\')[Application.ExecutablePath.Split('\\').Count() - 1].Length) + "Data\\Mechs_Images\\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImagePath = openFileDialog1.FileName;
                //textBox24.Text = IconPath.Split('\\')[IconPath.Split('\\').Count() - 1];
                pictureBox5.ImageLocation = ImagePath;
            }
        }

        private void button42_Click(object sender, EventArgs e)
        {
            Print_Preview PP = new Print_Preview(this, 0);
            PP.ShowDialog();
        }

        public void Draw_BattleMech(Graphics g, int marginX, int MarginY)
        {
            Critical_Hit_Table.Draw_CritTable(g, listBox15.Items.Cast<string>().ToList(), listBox17.Items.Cast<string>().ToList(), listBox13.Items.Cast<string>().ToList(), listBox19.Items.Cast<string>().ToList(), listBox18.Items.Cast<string>().ToList(), listBox14.Items.Cast<string>().ToList(), listBox16.Items.Cast<string>().ToList(), listBox12.Items.Cast<string>().ToList(), marginX, MarginY + 1023 - 431, 0, 0, 0, 0);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++

            float Global_Scale = 0.2f;
            float Global_ScaleY = Global_Scale * 1.1f;
            System.Drawing.Point P0 = new System.Drawing.Point(marginX + 630, -MarginY - 80);
            System.Drawing.Point P1 = new System.Drawing.Point(marginX + 400, -MarginY - 100);

            Painting.Paint_Mech(g, Global_Scale, Global_ScaleY, P0, System.Drawing.Color.Brown);

            Collection<System.Drawing.Point> col;
            double rayon;

            string F = "ArmorCircles_100.txt";
            if (int.Parse(textBox55.Text) <= 40) F = "ArmorCircles_40.txt";
            else if (int.Parse(textBox55.Text) <= 70) F = "ArmorCircles_70.txt";

            Painting.Read_File_ArmorCircles(int.Parse(textBox_Head_Armor.Text), int.Parse(textBox_CenterTorso_Armor.Text), int.Parse(textBox77.Text), int.Parse(textBox76.Text), int.Parse(textBox75.Text), int.Parse(textBox74.Text), int.Parse(textBox73.Text), int.Parse(textBox72.Text), F, out col, out rayon, Global_Scale, P0, 1);
            Painting.Paint_Definitive_Circles(rayon * Global_Scale, col, g, System.Drawing.Color.DarkViolet);


            Painting.Paint_BackMech(g, Global_Scale, Global_ScaleY, P1, System.Drawing.Color.Brown);

            Painting.Read_File_ArmorCircles_For_Back(int.Parse(textBox71.Text), int.Parse(textBox70.Text), int.Parse(textBox69.Text), F, out col, out rayon, Global_Scale, P1);
            Painting.Paint_Definitive_Circles(rayon * Global_Scale, col, g, System.Drawing.Color.DarkViolet);

            Global_Scale = 0.2f * 0.75f;
            Global_ScaleY = Global_Scale * 1.1f;
            P0 = new System.Drawing.Point(marginX + 610, -MarginY - 530);
            Painting.Paint_Mech(g, Global_Scale, Global_ScaleY, P0, System.Drawing.Color.Brown);


            Painting.Read_File_ArmorCircles(int.Parse(textBox67.Text), int.Parse(textBox66.Text), int.Parse(textBox65.Text), int.Parse(textBox64.Text), int.Parse(textBox63.Text), int.Parse(textBox62.Text), int.Parse(textBox61.Text), int.Parse(textBox60.Text), F, out col, out rayon, Global_Scale, P0, 1);
            Painting.Paint_Definitive_Circles(rayon * Global_Scale, col, g, System.Drawing.Color.Cyan);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++

            Heat_Scale_Printing.Draw_Heat_Scale(g, marginX + 500, MarginY + 1023 - 251);
            Heat_Scale_Printing.Draw_Heat_Scale_Column(g, marginX + 771 - 61, MarginY + 1023 - 373);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++

            //Icon_And_Image_Drawing.Draw_Images(g, pictureBox2.Image, ImagePath, IconPath, marginX + 305, MarginY + 387, marginX + 150, MarginY + 387, 400, 400);

            Title_Drawing.Draw_Title(g, marginX + -5, MarginY - 9);

            //+++++++++++++++++++++++++++++++++++++++++++++++++++

            int WHeat = 0;


            Collection<Weapon> WC = new Collection<Weapon>();
            foreach (ListViewItem LVI in listView4.Items)
            {
                Weapon W = new Weapon();
                W.quantity = 1;
                W.name = LVI.SubItems[8].Text;
                W.dammage = int.Parse(LVI.SubItems[3].Text);
                W.heat = int.Parse(LVI.SubItems[2].Text);
                W.range_min = int.Parse(LVI.SubItems[4].Text.Split('/')[0]);
                W.range_short = int.Parse(LVI.SubItems[4].Text.Split('/')[1]); ;
                W.range_med = int.Parse(LVI.SubItems[4].Text.Split('/')[2]);
                W.range_long = int.Parse(LVI.SubItems[4].Text.Split('/')[3]);

                W.location = LVI.SubItems[12].Text;

                WC.Add(W);

                WHeat += W.heat;
            }


            Collection<Ammo> AC = new Collection<Ammo>();
            foreach (ListViewItem LVI in listView7.Items)
            {
                Ammo A = new Ammo();
                A.name = LVI.SubItems[9].Text;
                A.rounds = int.Parse(LVI.SubItems[10].Text);

                AC.Add(A);
            }

            /*
            Collection<Equipement> Eq = new Collection<Equipement>();
            foreach (ListViewItem lvi in listView7.Items)
            {
                Equipement E = new Equipement();
                E.name = lvi.SubItems[0].Text;
                E.desc = lvi.SubItems[4].Text;
                Eq.Add(E);
            }
            */

            MechData_Drawing.Draw_MechData(g, marginX, MarginY + 60, textBox56.Text, int.Parse(textBox55.Text), int.Parse(textBox78.Text), int.Parse(textBox79.Text), 0, CustomMech.Tech, "Standard", "3025", WC, AC, int.Parse(textBox51.Text) + int.Parse(textBox50.Text), CustomMech.HeatSinkType, WHeat.ToString(), new Collection<Equipement>());


            Additional_Text_Painting.DrawTextSide(g, marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Back_RCT(g, int.Parse(textBox71.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Back_LST(g, int.Parse(textBox69.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Back_RST(g, int.Parse(textBox70.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_TotalArmor_Text(g, int.Parse(textBox59.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_InternalStructure_Text(g, int.Parse(textBox57.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_RST(g, int.Parse(textBox77.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_LST(g, int.Parse(textBox76.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_LA(g, int.Parse(textBox74.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_RA(g, int.Parse(textBox75.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_CT(g, int.Parse(textBox_CenterTorso_Armor.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_LL(g, int.Parse(textBox72.Text), marginX, MarginY);
            Additional_Text_Painting.Draw_Armor_Front_RL(g, int.Parse(textBox73.Text), marginX, MarginY);

            WarriorData_Painting.Draw_WarriorData(g, marginX + 300, MarginY + 303, textBox49.Text, 5, 4);


        }

        private void button2_Click(object sender, EventArgs e)
        {
            CustomMechs.Add(CustomMech);
            listBox1.Items.Add(CustomMech.MechType);
            CustomMech = new MechClass();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> data = new List<string>();

            foreach (MechClass MC in CustomMechs)
            {
                data.Add(MC.Get_Mech_String());
            }

            File.WriteAllLines(".\\Data\\TempMaximDebug.txt", data);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string[] data = File.ReadAllLines(".\\Data\\TempMaximDebug.txt");

            foreach (string S in data)
            {
                MechClass Mech = new MechClass();
                Mech.Set_Mech_String(S);
                CustomMechs.Add(Mech);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Espace F = new Espace();
            F.Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            F = new Shooter(this, CustomMechs[0].Get_Mech_String());
            F.Show();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            FakeButtonConnectClick();
        }

        public void FakeButtonConnectClick()
        {
            // See if we have text on the IP and Port text fields
            if (textBoxIP.Text == "" || textBoxPort.Text == "")
            {
                MessageBox.Show("IP Address and Port Number are required to connect to the Server\n");
                return;
            }
            try
            {
                // Create the socket instance
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Cet the remote IP address
                IPAddress ip = IPAddress.Parse(textBoxIP.Text);
                int iPortNo = System.Convert.ToInt16("6525");
                // Create the end point 
                IPEndPoint ipEnd = new IPEndPoint(ip, iPortNo);
                // Connect to the remote host
                m_clientSocket.Connect(ipEnd);
                if (m_clientSocket.Connected)
                {
                    //Wait for data asynchronously 
                    WaitForData();
                }
                //SendMessage("allo server");
            }
            catch (SocketException se)
            {
                string str;
                str = "\nConnection failed, is the server running?\n" + se.Message;
                MessageBox.Show(str);
            }
        }

        public void WaitForData()
        {
            try
            {
                if (m_pfnCallBack == null)
                {
                    m_pfnCallBack = new AsyncCallback(OnDataReceived);
                }
                SocketPacket theSocPkt = new SocketPacket();
                theSocPkt.thisSocket = m_clientSocket;
                // Start listening to the data asynchronously
                m_result = m_clientSocket.BeginReceive(theSocPkt.dataBuffer,
                                                        0, theSocPkt.dataBuffer.Length,
                                                        SocketFlags.None,
                                                        m_pfnCallBack,
                                                        theSocPkt);
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }

        }
        public class SocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[5000];
        }

        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket)asyn.AsyncState;
                int iRx = theSockId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.Unicode.GetDecoder();
                int charLen = d.GetChars(theSockId.dataBuffer, 0, iRx, chars, 0);
                System.String szData = new System.String(chars);

                string ttt = szData.TrimEnd('\0');
                ttt = ttt.TrimEnd(m_Delim_Message);
                ttt = ttt.Split(m_Delim_Start)[1];

                if (ttt.Length < 4096) this.Invoke((MethodInvoker)delegate { richTextRxMessage.AppendText("\n" + szData.TrimEnd('\0')); });
                else MessageBox.Show("Message over 4096");

                bool disconnected = false;

                if (ttt.StartsWith("Prepare"))
                {
                    string mecha = "";
                    string[] data = ttt.Split(m_Delim_End);
                    for (int i = 0; i < int.Parse(data[1]); i++)
                        mecha += data[2 + i] + m_Delim_End;
                    F.Game.SetGamePreparation(mecha, int.Parse(data[1]));
                }
                else if (ttt.StartsWith("GO"))
                {
                    F.Game.SetGameStart();
                }
                else if (ttt.StartsWith("UP"))
                {
                    SendMessage("UP" + m_Delim_End + m_Client_Nbr + m_Delim_End + F.Game.GetUP_DataMech() + m_Delim_End + m_Delim_Message);
                }
                else if (ttt.StartsWith("SetClientNbr"))
                {
                    m_Client_Nbr = int.Parse(ttt.Split(m_Delim_End)[1]);

                    this.Invoke(new Action(() => StartShooter()));
                }
                else if (ttt.StartsWith("Unfrost"))
                {
                    if (m_Client_Nbr == int.Parse(ttt.Split(m_Delim_End)[1])) F.Game.GameIsFrozen = false;
                    else F.Game.GameIsFrozen = true;

                    if (szData.TrimEnd('\0').Split(m_Delim_Message)[1].Length > 0)
                    {
                        string msgrrr = "";
                        for (int i = 1; i < szData.TrimEnd('\0').Split(m_Delim_Message).Count(); i++)
                            msgrrr += szData.TrimEnd('\0').Split(m_Delim_Message)[i].Split(m_Delim_Start)[1];

                        if (m_Client_Nbr != int.Parse(msgrrr.Split(m_Delim_End)[1])) F.Game.SendMessage(msgrrr);
                    }
                }
                else if (ttt.StartsWith("BackOrder"))
                {
                    if (m_Client_Nbr != int.Parse(ttt.Split(m_Delim_End)[1])) F.Game.SendMessage(szData.TrimEnd('\0'));
                }


                if (disconnected == false) WaitForData();
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void StartShooter()
        {
            F = new Shooter(this, CustomMechs[m_Client_Nbr].Get_Mech_String());
            //F.TopMost = true;
            F.Show();

            //Thread.Sleep(5000);



            System.Drawing.Point Pos = new System.Drawing.Point();
            if (m_Client_Nbr == 0) Pos = new System.Drawing.Point(0, 0);
            else if (m_Client_Nbr == 1) Pos = new System.Drawing.Point(1920, 0);
            else if (m_Client_Nbr == 2) Pos = new System.Drawing.Point(1920, 1080);
            else if (m_Client_Nbr == 3) Pos = new System.Drawing.Point(0, 1080);

            F.Location = Pos;




            F.Game.ClientNbr = m_Client_Nbr;
        }

        public void SendMessage(string s)
        {
            try
            {
                if (s.Length > 4096) MessageBox.Show("Message plus long que 4096");
                else
                {
                    string msg = s;
                    // New code to send strings
                    /*NetworkStream networkStream = new NetworkStream(m_clientSocket);
                    System.IO.StreamWriter streamWriter = new System.IO.StreamWriter(networkStream);
                    streamWriter.WriteLine(msg);
                    streamWriter.Flush();*/

                    byte[] byData = System.Text.Encoding.Unicode.GetBytes(msg);
                    m_clientSocket.Send(byData);

                    /* Use the following code to send bytes
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes(objData.ToString ());
                    if(m_clientSocket != null){
                        m_clientSocket.Send (byData);
                    }
                    */
                }
            }
            catch (SocketException se)
            {
                MessageBox.Show(se.Message);
            }
        }

        public void StockMessage(string s)
        {
            frameMessage += s;
        }

        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            SendMessage("Allo de Maxim" + m_Delim_Message);
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            if (m_clientSocket != null)
            {
                m_clientSocket.Close();
                m_clientSocket = null;
            }
        }

        private void ButtonDisconnectClick(object sender, EventArgs e)
        {

        }

        //********************************************************************************** Galaxy **********************************************************************************


        private void panel4_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            bool test = false;

            if (MouseScrolling < 600 && e.Delta > 0)
            {
                MouseScrolling += e.Delta;
                test = true;
            }
            if (MouseScrolling > 0 && e.Delta < 0)
            {
                MouseScrolling += e.Delta;
                test = true;
            }



            switch (MouseScrolling)
            {
                case 0:
                    scaleMap = 0.5f;
                    break;

                case 120:
                    scaleMap = 2.0f;
                    break;

                case 240:
                    scaleMap = 4.0f;
                    break;

                case 360:
                    scaleMap = 8.0f;
                    break;

                case 480:
                    scaleMap = 16.0f;
                    break;

                case 600:
                    scaleMap = 32.0f;
                    break;

                default:
                    scaleMap = 1.0f;
                    break;
            }
            //scaleMap += (e.Delta / (500.0f)) * (float)Math.Pow(2,e.Delta/20);
            if (test) panel4.Refresh();
            //textBox1.Text = scaleMap.ToString();
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(System.Drawing.Color.Black), new System.Drawing.Rectangle(0, 0, 965, 965)); // 810, 635));

            for (int i = 0; i < GalaxyDatabase.Planets.Count(); i++)
            {
                int PX = (int)((GalaxyDatabase.Planets[i].X + MapX) * (Math.Abs(scaleMap))) + 483;
                int PY = (int)((-GalaxyDatabase.Planets[i].Y + MapY) * (Math.Abs(scaleMap))) + 483;

                if (PX > 0 && PY > 0)
                    if (PX < 965 && PY < 965)
                    {
                        string faction = GalaxyDatabase.Planets[i].faction0;

                        //Collection<GalaxyDatabase.invading> ordered = GetOrdered(GalaxyDatabase.Planets[i].Invasion);

                        foreach (GalaxyDatabase.invading I in GalaxyDatabase.Planets[i].Invasion.Reverse())
                        {
                            if (dateTimePicker1.Value >= I.date)
                            {
                                if (I.faction.Split(',').Count() > 1) faction = "alliance";
                                faction = I.faction;

                                break;
                            }
                        }

                        System.Drawing.Color C = new System.Drawing.Color();
                        if (faction == "TA") C = System.Drawing.Color.MediumPurple;
                        else if (faction == "FWL") C = System.Drawing.Color.Purple;
                        else if (faction == "CJF") C = System.Drawing.Color.GreenYellow;
                        else if (faction == "CW") C = System.Drawing.Color.Orange;
                        else if (faction == "CGB") C = System.Drawing.Color.CornflowerBlue;
                        else if (faction == "WOB") C = System.Drawing.Color.DarkCyan;
                        else if (faction == "LA") C = System.Drawing.Color.Blue;
                        else if (faction == "DC") C = System.Drawing.Color.Red;
                        else if (faction == "FS") C = System.Drawing.Color.Yellow;
                        else if (faction == "CC") C = System.Drawing.Color.Green;
                        else if (faction == "alliance") C = System.Drawing.Color.Khaki;
                        else if (listBox1.SelectedItem != null)
                        {
                            if (faction == listBox1.SelectedItem.ToString()) C = System.Drawing.Color.Cyan;
                        }
                        else C = System.Drawing.Color.White;

                        if (scaleMap > 6.0f)
                        {
                            e.Graphics.FillEllipse(new SolidBrush(C), new System.Drawing.Rectangle(PX, PY, 25, 25));
                            e.Graphics.DrawString(GalaxyDatabase.Planets[i].name, m, new SolidBrush(System.Drawing.Color.Red), new PointF(PX, PY - 18));
                        }
                        else
                        {
                            e.Graphics.FillEllipse(new SolidBrush(C), new System.Drawing.Rectangle(PX, PY, 3, 3));
                            if (scaleMap > 2.5f) e.Graphics.DrawString(GalaxyDatabase.Planets[i].name, m, new SolidBrush(System.Drawing.Color.Red), new PointF(PX, PY - 18));
                        }
                    }
            }
        }

        private void panel4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //listBox2.Items.Clear();
            //listBox3.Items.Clear();


            for (int i = 0; i < GalaxyDatabase.Planets.Count(); i++)
            {
                int PX = (int)((GalaxyDatabase.Planets[i].X + MapX) * (Math.Abs(scaleMap))) + 483;
                int PY = (int)((-GalaxyDatabase.Planets[i].Y + MapY) * (Math.Abs(scaleMap))) + 483;

                if (PX > 0 && PY > 0)
                    if (PX < 965 && PY < 965)
                    {
                        if (e.X > PX && e.Y > PY)
                            if (e.X < PX + 25 && e.Y < PY + 25)
                            {
                                comboBox1.Text = GalaxyDatabase.Planets[i].faction0;

                                string[] T2 = new string[GalaxyDatabase.Planets[i].Invasion.Count()];
                                DateTime[] T3 = new DateTime[GalaxyDatabase.Planets[i].Invasion.Count()];

                                for (int j = 0; j < GalaxyDatabase.Planets[i].Invasion.Count(); j++)
                                {
                                    T3[j] = (GalaxyDatabase.Planets[i].Invasion[j].date);
                                    T2[j] = (GalaxyDatabase.Planets[i].Invasion[j].faction);
                                }
                                MessageBox.Show(GalaxyDatabase.Planets[i].Invasion.Count().ToString());
                                //----------------------------------------
                                Collection<DateTime> list = new Collection<DateTime>();
                                listBox2.Items.Clear();
                                listBox3.Items.Clear();

                                for (int j = 0; j < GalaxyDatabase.Planets[i].Invasion.Count(); j++)
                                {
                                    DateTime tmp = DateTime.MinValue;
                                    int pos = 0;
                                    for (int k = 0; k < GalaxyDatabase.Planets[i].Invasion.Count(); k++)
                                    {

                                        bool test = false;
                                        foreach (DateTime NN in list)
                                        {
                                            if (NN == T3[k])
                                            {
                                                test = true;
                                                //
                                            }
                                        }

                                        //MessageBox.Show(T3[k].ToString());
                                        if (tmp < T3[k])
                                        {
                                            if (test == false)
                                            {
                                                pos = k;
                                                tmp = T3[k];
                                            }
                                            else
                                            {
                                                //k = 0;

                                            }
                                        }
                                    }
                                    list.Add(T3[pos]);

                                    listBox2.Items.Add(T2[pos]);
                                    listBox3.Items.Add(T3[pos]);
                                }
                                //----------------------------------------

                                textBox8.Text = GalaxyDatabase.Planets[i].description;

                                textBox1.Text = GalaxyDatabase.Planets[i].name;
                                Show_Planet(GalaxyDatabase.Planets[i].name);

                                if (GalaxyDatabase.Planets[i].gravity == 0)
                                {
                                    checkBox1.Checked = false;
                                    checkBox1.BackColor = System.Drawing.Color.Red;
                                }
                                else
                                {
                                    checkBox1.Checked = true;
                                    checkBox1.BackColor = System.Drawing.Color.Green;
                                }

                                textBox7.Text = GalaxyDatabase.Planets[i].lifeform.ToString();
                                textBox11.Text = GalaxyDatabase.Planets[i].gravity.ToString();
                                textBox10.Text = GalaxyDatabase.Planets[i].syspos.ToString();
                                textBox9.Text = GalaxyDatabase.Planets[i].temperature.ToString();
                                textBox14.Text = GalaxyDatabase.Planets[i].spectral;
                                textBox12.Text = GalaxyDatabase.Planets[i].luminosity;
                                textBox13.Text = GalaxyDatabase.Planets[i].subtype.ToString();
                                textBox15.Text = GalaxyDatabase.Planets[i].water.ToString();

                                MapX = -(int)GalaxyDatabase.Planets[i].X;
                                MapY = (int)GalaxyDatabase.Planets[i].Y;

                                double Distance = Math.Sqrt(Math.Pow(MapX, 2) + Math.Pow(MapY, 2));
                                //textBox17.Text = Distance.ToString();

                                break;
                            }
                    }
            }

            panel4.Refresh();

            comboBox1.BackColor = System.Drawing.Color.White;
            textBox8.BackColor = System.Drawing.Color.White;
            textBox7.BackColor = System.Drawing.Color.White;
            textBox9.BackColor = System.Drawing.Color.White;
            textBox10.BackColor = System.Drawing.Color.White;
            textBox11.BackColor = System.Drawing.Color.White;
            textBox12.BackColor = System.Drawing.Color.White;
            textBox13.BackColor = System.Drawing.Color.White;
            textBox14.BackColor = System.Drawing.Color.White;
            textBox15.BackColor = System.Drawing.Color.White;
        }

        public void Show_Planet(string planet)
        {
            //m_Timer.Start();
            // SwapChain description
            desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(panel5.Width, panel5.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = panel5.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };


            // Create Device and SwapChain


            SharpDX.Direct3D11.Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new[] { SharpDX.Direct3D.FeatureLevel.Level_11_1 }, desc, out device, out swapChain);
            context = device.ImmediateContext;

            // Ignore all windows events
            //var factory = swapChain.GetParent<SharpDX.DXGI.Factory>();
            //factory.MakeWindowAssociation(this.Handle, WindowAssociationFlags.IgnoreAll);




            //SharpDX.WIC.Bitmap bmp = new SharpDX.WIC.Bitmap()

            //System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(pictureBox1.Image);

            SharpDX.Direct3D11.Resource R;

            if (System.IO.File.Exists(".\\Campagne\\NewPlanets\\" + planet + ".jpg")) R = TextureLoader.CreateTex2DFromFile(device, ".\\Campagne\\NewPlanets\\" + planet + ".jpg");
            else if (System.IO.File.Exists(".\\Campagne\\NewPlanets\\" + planet + ".bmp")) R = TextureLoader.CreateTex2DFromFile(device, ".\\Campagne\\NewPlanets\\" + planet + ".bmp");
            else R = TextureLoader.CreateTex2DFromFile(device, ".\\Data\\Galaxy\\" + planet + ".bmp");
            ShaderResourceView effects = new ShaderResourceView(device, R);


            SO = new Static_Object();
            SO.Initialize(device, context, "planete.blend.txt", effects);



            WorkThreadFunction();//;
            /*G = new Thread(new ThreadStart(WorkThreadFunction));
            G.Start();*/

        }

        public void WorkThreadFunction()
        {
            // Prepare matrices
            view = Matrix.LookAtLH(new Vector3(0, 0, -2.9f), new Vector3(0, 0, 0), Vector3.UnitY);
            proj = Matrix.Identity;

            // Use clock
            //var clock = new Stopwatch();
            //clock.Start();

            // Declare texture for rendering
            bool userResized = true;
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;
            Texture2D depthBuffer = null;
            DepthStencilView depthView = null;

            float rotation = 0.0f;

            //panel5.Invoke(new Action(() =>

            RenderLoop.Run(panel5, () =>
            {

                // If Form resized
                if (userResized)
                {
                    // Dispose all previous allocated resources
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref renderView);
                    Utilities.Dispose(ref depthBuffer);
                    Utilities.Dispose(ref depthView);

                    // Resize the backbuffer
                    swapChain.ResizeBuffers(desc.BufferCount, panel5.Width, panel5.Height, Format.Unknown, SwapChainFlags.None);

                    // Get the backbuffer from the swapchain
                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

                    // Renderview on the backbuffer
                    renderView = new RenderTargetView(device, backBuffer);

                    // Create the depth buffer
                    depthBuffer = new Texture2D(device, new Texture2DDescription()
                    {
                        Format = Format.D32_Float_S8X24_UInt,
                        ArraySize = 1,
                        MipLevels = 1,
                        Width = panel5.Width,
                        Height = panel5.Height,
                        SampleDescription = new SampleDescription(1, 0),
                        Usage = ResourceUsage.Default,
                        BindFlags = BindFlags.DepthStencil,
                        CpuAccessFlags = CpuAccessFlags.None,
                        OptionFlags = ResourceOptionFlags.None
                    });

                    // Create the depth buffer view
                    depthView = new DepthStencilView(device, depthBuffer);

                    // Setup targets and viewport for rendering
                    context.Rasterizer.SetViewport(new Viewport(0, 0, panel5.Width, panel5.Height, 0.0f, 1.0f));
                    context.OutputMerger.SetTargets(depthView, renderView);

                    // Setup new projection matrix with correct aspect ratio
                    proj = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, panel5.Width / (float)panel5.Height, 0.1f, 250.0f);

                    // We are done resizing
                    userResized = false;
                }

                //var time = clock.ElapsedMilliseconds / 1000.0f;

                //var viewProj = Matrix.Multiply(view, proj);

                // Clear views
                context.ClearDepthStencilView(depthView, DepthStencilClearFlags.Depth, 1.0f, 0);
                context.ClearRenderTargetView(renderView, SharpDX.Color.Black);


                //************************************************************************************ Render

                Matrix world = Matrix.Identity;


                SO.Render(view, proj, world * Matrix.RotationX(rotation) * Matrix.RotationY(3 * rotation), context);

                rotation += 0.002f;
                //SO.Render(view, proj, world * Matrix.Translation(10.0f, 5.0f, 0.0f), context, 0.0f, 0.0f);




                // Present!
                swapChain.Present(1, PresentFlags.None);
            });
            //})));
            depthBuffer.Dispose();
            depthView.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();

            //swapChain.Dispose();
            //device.Dispose();
            //context.Dispose();
            //contantBuffer.Dispose();
            //SO.Dispose();

            //MessageBox.Show("end Thread");
        }

        private void panel4_MouseDown(object sender, MouseEventArgs e)
        {
            LMapX = e.X;
            LMapY = e.Y;
        }

        private void panel4_MouseUp(object sender, MouseEventArgs e)
        {
            MapX += (int)(1 / scaleMap * (e.X - LMapX));// * 1 / (Math.Pow(scaleMap + 0.9, 2.5f)));
            MapY += (int)(1 / scaleMap * (e.Y - LMapY));// * 1 / (Math.Pow(scaleMap + 0.9, 2.5f)));

            panel4.Refresh();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                openFileDialog1.InitialDirectory = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - Application.ExecutablePath.Split('\\')[Application.ExecutablePath.Split('\\').Count() - 1].Length) + "Logos\\";
                openFileDialog1.Filter = "JPEG|*.jpg";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string IconPath = openFileDialog1.FileName;
                    //textBox24.Text = IconPath.Split('\\')[IconPath.Split('\\').Count() - 1];
                    //pictureBox1.ImageLocation = IconPath;
                    if (System.IO.File.Exists(".\\Campagne\\NewPlanets\\" + textBox1.Text + ".jpg") == false) System.IO.File.Copy(IconPath, Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - Application.ExecutablePath.Split('\\')[Application.ExecutablePath.Split('\\').Count() - 1].Length) + "Campagne\\NewPlanets\\" + textBox1.Text + ".jpg");

                    Show_Planet(textBox1.Text);
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            /*if (textBox1.Text != "")
            {
                //Show_Planet("Terra");
                FractalPlanet.Create_Planet(Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - Application.ExecutablePath.Split('\\')[Application.ExecutablePath.Split('\\').Count() - 1].Length) + "Campagne\\NewPlanets\\" + textBox1.Text + ".bmp");


                //panel5.Dispose();


                timer2.Start();

            }*/
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            /*
            timer2.Stop();

            //SO.Dispose();
            ////Thread.Sleep(1000);
            Show_Planet(textBox1.Text);
            */
        }

        private void button7_Click(object sender, EventArgs e)
        {
            /*
            AddConquest AC = new AddConquest(this);
            AC.ShowDialog();
            listBox2.BackColor = System.Drawing.Color.OrangeRed;
            listBox3.BackColor = System.Drawing.Color.OrangeRed;
            */
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            textBox8.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            textBox11.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            textBox10.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            textBox9.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            textBox7.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            textBox15.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            textBox13.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            textBox12.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            textBox14.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox1.BackColor = System.Drawing.Color.OrangeRed;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            GalaxyDatabase.Planet P = new GalaxyDatabase.Planet();

            P.name = textBox1.Text;
            P.faction0 = comboBox1.SelectedItem.ToString();
            P.description = textBox8.Text;
            P.lifeform = int.Parse(textBox7.Text);
            P.temperature = int.Parse(textBox9.Text);
            P.syspos = int.Parse(textBox10.Text);
            P.gravity = float.Parse(textBox11.Text);
            P.luminosity = textBox12.Text;
            P.subtype = int.Parse(textBox13.Text);
            P.spectral = textBox14.Text;
            P.water = int.Parse(textBox15.Text);

            Collection<GalaxyDatabase.invading> list = new Collection<GalaxyDatabase.invading>();
            for (int i = 0; i < listBox2.Items.Count; i++)
            {
                if (listBox2.Items[i].ToString() != "")
                {
                    GalaxyDatabase.invading I = new GalaxyDatabase.invading();
                    I.faction = listBox2.Items[i].ToString();
                    //MessageBox.Show(listBox3.Items[i].ToString().Split('-')[0]);
                    DateTime D = new DateTime(int.Parse(listBox3.Items[i].ToString().Split('-')[0]), int.Parse(listBox3.Items[i].ToString().Split('-')[1]), int.Parse(listBox3.Items[i].ToString().Split('-')[2].Substring(0, 2)));
                    I.date = D;

                    list.Add(I);
                }
            }

            P.Invasion = list;

            GalaxyDatabase.SaveData(P);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GalaxyDatabase.LoadData();

            List<string> list = new List<string>();
            foreach (GalaxyDatabase.Planet P in GalaxyDatabase.Planets)
            {
                foreach (GalaxyDatabase.invading i in P.Invasion)
                {
                    bool test = false;
                    foreach (string p in list)
                        if (i.faction == p) test = true;
                    if (!test) list.Add(i.faction);
                }
            }

            list.Sort();

            foreach (string s in list)
            {
                comboBox1.Items.Add(s);
            }
        }


        //********************************************************************************************************************************************************************************

        //******************************************************************************************** Force *****************************************************************************

        private void panel3_MouseClick(object sender, MouseEventArgs e)
        {
            H.Select_Unit(e.X, e.Y);
            panel3.Refresh();
        }

        private void panel3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            H.Select_Unit(e.X, e.Y);

            Modify_Node MD = new Modify_Node(H, this);
            MD.ShowDialog();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            if (H != null)
            {
                H.Draw_Hierarchy(e.Graphics);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            H = new Hierarchy();

            Image I = new Bitmap(".\\Logos\\Maxim_Avatar.png");

            personel P = new personel("Maximette", "copuleu", "scientiste", 79, "Male", "Grand champion galactique", I);

            H.Set_Personel(P);

            BattleUnits B = new BattleUnits("Mouche9", ".\\MechsImages\\Agrotera.png", "XL", null, null, null);

            H.Set_BattleUnit(B);

            H.HeadTree.Command = "Regiment";

            panel3.Refresh();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Image I = new Bitmap(".\\Logos\\Maxim_Avatar.png");

            personel P = new personel("Maximilien", "crosseu", "scientiste", 79, "Male", "Grand champion galactique", I);

            BattleUnits B = new BattleUnits("Mouche9", ".\\MechsImages\\Agrotera.png", "XL", null, null, null);

            H.Add_Child(P, B, "Platoon");


            panel3.Refresh();
        }
    }
}