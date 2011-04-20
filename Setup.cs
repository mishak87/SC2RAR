using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SC2RAR
{
    public partial class Setup : Form
    {
        public Config config;

        public Setup(Config _config)
        {
            InitializeComponent();

            config = _config;
            //Read or initialize variables
            if (config.watchPath == null)
            {
                config.format = "Normal";
                config.copyReplay = false;
                config.moveReplay = false;
                config.copyToGametypeFolder = false;
                config.playSound = false;
                config.autoPosition = false;
                config.typeOfSort = "Player";
                config.playerName = "NA";
                textBox1.Text = "NA";
                config.position = "Front";
                comboBox1.Text = "Front";
                config.race = "Zerg";
                comboBox2.Text = "Zerg";
                config.dynamicParamString = "|Matchup|.|Players|.|on|.|Map|[|ID|]";
                string[] dynamicParams = config.dynamicParamString.Split('|');
                label15.Text = previewDynamic(dynamicParams);
                config.debugWaitSeconds = 0;
                config.filters = new bool[6];
                config.filters[0] = false; //other
                config.filters[1] = true; //1v1
                config.filters[2] = true; //2v2
                config.filters[3] = true; //3v3
                config.filters[4] = true; //4v4
                config.filters[5] = true; //FFA
                
            }
            else
            {
                textBox2.Text = config.watchPath;
                textBox3.Text = config.outputPath;
                checkBox1.Checked = config.copyReplay;
                checkBox2.Checked = config.moveReplay;
                checkBox3.Checked = config.autoPosition;
                checkBox11.Checked = config.copyToGametypeFolder;
                checkBox10.Checked = config.playSound;
                textBox1.Text = config.playerName;
                comboBox1.Text = config.position;
                comboBox2.Text = config.race;

                if (config.autoPosition)
                {
                    if (config.typeOfSort.Equals("Player"))
                    {
                        radioButton12.Checked = true;
                    }
                    else if (config.typeOfSort.Equals("Race"))
                    {
                        radioButton13.Checked = true;
                    }
                }
                if (config.position.Equals("Front"))
                {
                    //radioButton1.Checked = true;
                    comboBox1.Text = "Front";
                }
                else if (config.position.Equals("Back"))
                {
                    //radioButton2.Checked = true;
                    comboBox2.Text = "Back";
                }

                if (config.format.Equals("Normal"))
                {
                    radioButton11.Checked = true;
                }
                else if (config.format.Equals("Normal."))
                {
                    radioButton10.Checked = true;
                }
                else if (config.format.Equals("Matchup"))
                {
                    radioButton3.Checked = true;
                }
                else if (config.format.Equals("Matchup."))
                {
                    radioButton4.Checked = true;
                }
                else if (config.format.Equals("Map"))
                {
                    radioButton5.Checked = true;
                }
                else if (config.format.Equals("Map."))
                {
                    radioButton6.Checked = true;
                }
                else if (config.format.Equals("Date"))
                {
                    radioButton7.Checked = true;
                }
                else if (config.format.Equals("DateMatchup"))
                {
                    radioButton8.Checked = true;
                }
                else if (config.format.Equals("Dynamic"))
                {
                    radioButton9.Checked = true;
                }
                textBox4.Text = config.dynamicParamString;
                string[] dynamicParams = config.dynamicParamString.Split('|');
                label15.Text = previewDynamic(dynamicParams);

                //filters
                checkBox9.Checked = config.filters[0];
                checkBox4.Checked = config.filters[1];
                checkBox5.Checked = config.filters[2];
                checkBox6.Checked = config.filters[3];
                checkBox7.Checked = config.filters[4];
                checkBox8.Checked = config.filters[5];
            }

            this.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SC2RAR.sc2rari.ico"));
            tabControl1.SelectedIndexChanged += new EventHandler(tabControl1_SelectedIndexChanged);
        }

        void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                button2.Enabled = false;
                button3.Enabled = true;
            }
            else if (tabControl1.SelectedIndex == (tabControl1.TabCount - 1))
            {
                button3.Enabled = false;
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = true;
                button3.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //save & close
            if (textBox2.Text.Equals(""))
            {
                MessageBox.Show("You did not enter a folder path");
            }
            else
            {
                if (config.outputPath == null)
                {
                    config.outputPath = config.watchPath;
                }
                this.DialogResult = DialogResult.OK;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //back
            if (tabControl1.SelectedIndex > 0)
            {
                tabControl1.SelectedIndex--;
                button3.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //next
            if (tabControl1.SelectedIndex < (tabControl1.TabCount-1))
            {
                tabControl1.SelectedIndex++;
                button2.Enabled = true;
            }
            else
            {
                button2.Enabled = false;
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //copy replays
            if (checkBox1.Checked)
            {
                textBox3.Enabled = true;
                button5.Enabled = true;
                config.copyReplay = true;
                checkBox2.Enabled = true;
                checkBox11.Enabled = true;
            }
            else
            {
                textBox3.Enabled = false;
                button5.Enabled = false;
                config.copyReplay = false;
                checkBox2.Enabled = false;
                checkBox11.Enabled = false;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            config.playerName = textBox1.Text;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //Front
            config.position = "Front";
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //back
            config.position = "Back";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //browse watch
            FolderBrowserDialog fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = fbd1.SelectedPath;
                config.watchPath = fbd1.SelectedPath;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            config.watchPath = textBox2.Text;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //browse copy to
            FolderBrowserDialog fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = fbd1.SelectedPath;
                config.outputPath = fbd1.SelectedPath;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            config.outputPath = textBox3.Text;
        }

        private void radioButton11_CheckedChanged(object sender, EventArgs e)
        {
            //normal
            config.format = "Normal";
        }

        private void radioButton10_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Normal.";
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Matchup";
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Matchup.";
        }

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Map";
        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Map.";
        }

        private void radioButton7_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "Date";
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            config.format = "DateMatchup";
        }

        private void radioButton9_CheckedChanged(object sender, EventArgs e)
        {
            //dynamic
            if (radioButton9.Checked)
            {
                textBox4.Enabled = true;
                config.format = "Dynamic";
                //config.dynamicParamString = textBox4.Text;
            }
            else
            {
                textBox4.Enabled = false;
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            string[] dynamicParams = textBox4.Text.Split('|');
            label15.Text = previewDynamic(dynamicParams);
            config.dynamicParamString = textBox4.Text;
        }

        private string previewDynamic(string[] dynamicParams)
        {
            string filename = "";
            foreach (string s in dynamicParams)
            {
                if (s.Equals("Matchup"))
                {
                    filename += "PvT";
                }
                else if (s.Equals("Map"))
                {
                    filename += "Lost Temple";
                }
                else if (s.Equals("Players"))
                {
                    filename += "warcode vs OED";
                }
                else if (s.Equals("Players."))
                {
                    filename += "warcode.vs.OED";
                }
                else if (s.Equals("Normal"))
                {
                    filename += "warcode(T) vs OED(Z)";
                }
                else if (s.Equals("Normal."))
                {
                    filename += "warcode(T).vs.OED(Z)";
                }
                else if (s.Equals("Date"))
                {
                    filename += "2010.02.20.0545";
                }
                else if (s.Equals("ID"))
                {
                    filename += "1e8f513d";
                }
                else
                {
                    filename += s;
                }
            }
            return "Preview: " + filename + ".SC2Replay";
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            config.debugWaitSeconds = (int)numericUpDown1.Value;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            config.position = comboBox1.SelectedItem.ToString();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            config.race = comboBox2.SelectedItem.ToString();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                radioButton12.Enabled = true;
                radioButton13.Enabled = true;
                config.autoPosition = true;
            }
            else
            {
                radioButton12.Enabled = false;
                radioButton12.Checked = false;
                radioButton13.Enabled = false;
                radioButton13.Checked = false;
                config.autoPosition = false;
            }
        }

        private void radioButton12_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton12.Checked == true)
            {
                textBox1.Enabled = true;
                comboBox1.Enabled = true;
                label1.Enabled = true;
                label6.Enabled = true;
                config.typeOfSort = "Player";
            }
            else
            {
                textBox1.Enabled = false;
                comboBox1.Enabled = false;
                label1.Enabled = false;
                label6.Enabled = false;
            }
        }

        private void radioButton13_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton13.Checked == true)
            {
                config.typeOfSort = "Race";
                comboBox2.Enabled = true;
                label5.Enabled = true;
            }
            else
            {
                comboBox2.Enabled = false;
                label5.Enabled = false;
            }
        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {
            config.playerName = textBox1.Text;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            //copy replays
            if (checkBox1.Checked)
            {
                config.moveReplay = true;
            }
            else
            {
                config.moveReplay = false;
            }
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                config.filters[1] = true;
            }
            else
            {
                config.filters[1] = false;
            }
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
            {
                config.filters[2] = true;
            }
            else
            {
                config.filters[2] = false;
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
            {
                config.filters[3] = true;
            }
            else
            {
                config.filters[3] = false;
            }
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
            {
                config.filters[4] = true;
            }
            else
            {
                config.filters[4] = false;
            }
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
            {
                config.filters[5] = true;
            }
            else
            {
                config.filters[5] = false;
            }
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {

            if (checkBox9.Checked)
            {
                config.filters[0] = true;
            }
            else
            {
                config.filters[0] = false;
            }
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
            {
                config.playSound = true;
            }
            else
            {
                config.playSound = false;
            }
        }

        private void checkBox11_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox11.Checked)
            {
                config.copyToGametypeFolder = true;
            }
            else
            {
                config.copyToGametypeFolder = false;
            }
        }
    }
}
