using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;

namespace SC2RAR
{
    public struct Config
    {
        public string watchPath;
        public bool copyReplay;
        public bool copyToGametypeFolder;
        public bool moveReplay;
        public string outputPath;

        public bool playSound;

        public bool autoPosition;
        public string typeOfSort;
        public string playerName;
        public string position;
        public string race;

        public string format;
        public string dynamicParamString;
        public string[] dynamicParams;

        public bool[] filters;

        public string[] optionParams;
        public int debugWaitSeconds;
    }

    static class Program
    {
        static NotifyIcon notifyIcon1;
        static System.IO.FileSystemWatcher watch;
        static string[] optionParams;
        static string[] filterParams;

        private static Config config = new Config();

        struct Player
        {
            public string name;
            public string nameAndIdent;
            public string race;
            public string teamId;
        }
        static byte[] koreanTerran = { 237, 133, 140, 235, 158, 128 };
        static byte[] koreanZerg = { 236, 160, 128, 234, 183, 184 };
        static byte[] koreanProtoss = { 237, 148, 132, 235, 161, 156, 237, 134, 160, 236, 138, 164 };
        static byte[] russianTerran = { 208, 162, 208, 181, 209, 128, 209, 128, 208, 176, 208, 189 };
        static byte[] germanTerran = { (byte)'T', (byte)'e', (byte)'r', (byte)'r', (byte)'a', (byte)'n', (byte)'e', (byte)'r' };
        static byte[] polishTerran = { (byte)'T', (byte)'e', (byte)'r', (byte)'r', (byte)'a', (byte)'n', (byte)'i', (byte)'e' };
        static byte[] polishZerg = { (byte)'Z', (byte)'e', (byte)'r', (byte)'g', (byte)'i' };
        static byte[] polishProtoss = { (byte)'P', (byte)'r', (byte)'o', (byte)'t', (byte)'o', (byte)'s', (byte)'i' };
        static byte[] engTerran = { (byte)'T', (byte)'e', (byte)'r', (byte)'r', (byte)'a', (byte)'n' };
        static byte[] engZerg = { (byte)'Z', (byte)'e', (byte)'r', (byte)'g' };
        static byte[] engProtoss = { (byte)'P', (byte)'r', (byte)'o', (byte)'t', (byte)'o', (byte)'s', (byte)'s' };

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            notifyIcon1 = new NotifyIcon();
            ContextMenu contextMenu1 = new ContextMenu();
            MenuItem menuItem1 = new MenuItem();
            MenuItem menuItem2 = new MenuItem();
            contextMenu1.MenuItems.AddRange(new MenuItem[] { menuItem2, menuItem1 });
            menuItem1.Index = 1;
            menuItem1.Text = "E&xit";
            menuItem1.Click += new EventHandler(menuItem1_Click);
            menuItem2.Index = 0;
            menuItem2.Text = "&Setup";
            menuItem2.Click += new EventHandler(menuItem5_Click);
            
            notifyIcon1.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("SC2RAR.sc2rari.ico"));
            notifyIcon1.Text = "SC2RAR";
            notifyIcon1.ContextMenu = contextMenu1;
            notifyIcon1.Visible = true;

            initialize();
            notifyIcon1.ShowBalloonTip(100, "SC2 RAR Watching", config.watchPath, ToolTipIcon.Info);
            Application.Run();
            //notifyIcon1.Visible = false;
        }

        static void menuItem5_Click(object sender, EventArgs e)
        {
            Setup setupWizard = new Setup(config);
            if (setupWizard.ShowDialog() == DialogResult.OK)
            {
                config = setupWizard.config;
                writeConfigIni();
            }
        }

        static void initialize()
        {
            StreamReader read = new StreamReader("config.ini");
            config.watchPath = @read.ReadLine();
            read.Close();
            if (config.watchPath == null)
            {
                Setup setupWizard = new Setup(config);
                if (setupWizard.ShowDialog() == DialogResult.OK)
                {
                    config = setupWizard.config;
                    writeConfigIni();
                }
            }
            else
            {
                read = new StreamReader("config.ini");
                config.watchPath = read.ReadLine();
                string parameters = read.ReadLine();
                optionParams = parameters.Split(' ');
                config.format = optionParams[0];
                bool didItWork;

                didItWork = bool.TryParse(optionParams[1], out config.autoPosition);
                config.typeOfSort = optionParams[2];
                config.playerName = optionParams[3];
                config.position = optionParams[4];
                config.race = optionParams[5];

                didItWork = bool.TryParse(optionParams[6], out config.copyReplay);
                didItWork = bool.TryParse(optionParams[7], out config.moveReplay);
                didItWork = bool.TryParse(optionParams[8], out config.playSound);
                didItWork = bool.TryParse(optionParams[9], out config.copyToGametypeFolder);
                config.debugWaitSeconds = Int32.Parse(optionParams[10]);
                //filters
                string filterparameters = read.ReadLine();
                filterParams = filterparameters.Split(' ');
                config.filters = new bool[6];
                didItWork = bool.TryParse(filterParams[0], out config.filters[0]);
                didItWork = bool.TryParse(filterParams[1], out config.filters[1]);
                didItWork = bool.TryParse(filterParams[2], out config.filters[2]);
                didItWork = bool.TryParse(filterParams[3], out config.filters[3]);
                didItWork = bool.TryParse(filterParams[4], out config.filters[4]);
                didItWork = bool.TryParse(filterParams[5], out config.filters[5]);

                //dynamic
                config.dynamicParamString = read.ReadLine();
                config.dynamicParams = config.dynamicParamString.Split('|');
                config.outputPath = read.ReadLine();
                read.Close();
            }

            watch = new System.IO.FileSystemWatcher(config.watchPath, "*.SC2Replay");
            watch.IncludeSubdirectories = true;
            watch.Created += new System.IO.FileSystemEventHandler(watch_Created);
            watch.EnableRaisingEvents = true;
        }

        private static void writeConfigIni()
        {
            StreamWriter write = new StreamWriter("config.ini");
            write.WriteLine(config.watchPath);
            write.WriteLine(config.format + " " + config.autoPosition.ToString() + " " + config.typeOfSort + " " + config.playerName + " " + config.position + " " + config.race + " " + config.copyReplay.ToString() + " " + config.moveReplay.ToString() + " " + config.playSound.ToString() + " " + config.copyToGametypeFolder.ToString() + " " + config.debugWaitSeconds.ToString());
            write.WriteLine(config.filters[0] + " " + config.filters[1] + " " + config.filters[2] + " " + config.filters[3] + " " + config.filters[4] + " " + config.filters[5]);
            write.WriteLine(config.dynamicParamString);
            write.WriteLine(config.outputPath);

            write.WriteLine("");
            write.WriteLine("#HELP:");
            write.WriteLine("#Please use the built in Setup menu. Access it by right clicking the tray icon.");

            write.Close();
        }

        static void watch_Created(object sender, System.IO.FileSystemEventArgs e)
        {
            startRenameThread(e.FullPath);
        }

        static bool isFileOpenOrReadOnly(string file)
        {
            try
            {
                //first make sure it's not a read only file
                if ((File.GetAttributes(file) & FileAttributes.ReadOnly) != FileAttributes.ReadOnly)
                {
                    //first we open the file with a FileStream
                    using (FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None))
                    {
                        try
                        {
                            stream.ReadByte();
                            return false;
                        }
                        catch (IOException)
                        {
                            return true;
                        }
                        finally
                        {
                            stream.Close();
                            stream.Dispose();
                        }
                    }
                }
                else
                    return true;
            }
            catch (IOException)
            {
                return true;
            }
        }

        static void startRenameThread(string path)
        {
            ThreadStart starter = delegate {
                    try
                    {
                        readBytesAndRenameReplay(path);
                    }
                    catch
                    { 
                        //ACTUAL ERROR REPORTING TO LOG?
                    }
                };
            new Thread(starter).Start();
        }

        static string shortRace(string race)
        {
            if (race.Equals("Terran")) return "T";
            else if (race.Equals("Zerg")) return "Z";
            else if (race.Equals("Protoss")) return "P";
            else if (race.Equals("Random")) return "R";
            else return "X";
        }

        public static string ByteArrayToStr(byte[] byteArray)
        {
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(byteArray);
        }

        public static string ByteArrayToStr(byte[] byteArray, int index, int count)
        {
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(byteArray, index, count);
        }

        public static byte[] StrToByteArray(string str)
        {
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(str);
        }

        static bool compareByteArray(byte[] array1, byte[] array2)
        {
            bool isEqual = false;
            if (array1.Length == array2.Length)
            {
                isEqual = true;
                for (int i = 0; i < array1.Length; i++)
                {
                    if (array1[i] != array2[i]) isEqual = false;
                }
            }
            return isEqual;
        }

        static byte[] reverseBytes(byte[] array)
        {
            int cnt = 0;
            for (int i = 0; i <= array.Length; i++)
            {
                if (array[i + 1] == 0)
                {
                    cnt = i;
                    break;
                }
            }
            byte[] reversedArray = new byte[cnt + 1];
            int revCnt = 0;
            for (int i = cnt; i >= 0; i--)
            {
                reversedArray[revCnt] = array[i];
                revCnt++;
            }
            return reversedArray;
        }

        static void readBytesAndRenameReplay(string path)
        {
            if (config.debugWaitSeconds != 0)
            {
                System.Threading.Thread.Sleep(config.debugWaitSeconds * 1000);
            }
            while (isFileOpenOrReadOnly(path))
            {
                System.Threading.Thread.Sleep(1000);
            }

            bool validPathName = Regex.IsMatch(path, "^[a-zA-Z0-9 .]*$");

            byte[] replayDataBuffer = new byte[2000];
            MpqLib.Mpq.CArchive replay = null;
            try{
                replay = new MpqLib.Mpq.CArchive(path);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }
            //replay.ExportFile("replay.info", replayDataBuffer); //PRE 0.9
            replay.ExportFile("replay.details", replayDataBuffer);

            List<byte> list = new List<byte>();

            for (int i = 1; i < 2000; i++)
            {
                if ((replayDataBuffer[i - 1] == 0) && (replayDataBuffer[i] != 0)) list.Add(replayDataBuffer[i - 1]);
                if (replayDataBuffer[i - 1] != 0) list.Add(replayDataBuffer[i - 1]);
            }

            replay.Close();

            byte[] replayData = list.ToArray();

            //WRITE TO TEXT FILE FOR EASY VIEWING
            //writeDebugTxt(replayData, true);
            //writeDebugTxt(replayData, false);

            //read backwards until i hit 0 or value that equals buffer size, 
            //reverse and add to a list until i  hit {0, 16, 0}
            List<string> playerdata = readPlayerAndMapDataBeta(replayData);
            //List<string> playerdata = readPlayerAndMapData(replayData);


            string map = playerdata[0];

            //REPLACE MAP NAME WITH PLAYER SPECIFIC NAME FROM maplist.txt
            Dictionary<string, string> maplist = new Dictionary<string, string>();
            StreamReader readMapList = new StreamReader("maplist.txt");
            string mapDataString;
            while ((mapDataString = readMapList.ReadLine()) != null)
            {
                string[] mapDataStringSplit = mapDataString.Split('|');
                maplist.Add(mapDataStringSplit[0], mapDataStringSplit[1]);
            }
            readMapList.Close();
            if (maplist.ContainsKey(map)) map = maplist[map];

            //OBSOLETE, KEEP FOR REGEX
            //bool validMapName = Regex.IsMatch(map, "^[a-zA-Z0-9 ]*$");
            //if (!validMapName) map = "X";
            List<Player> players = new List<Player>();
            int teamA = 0;
            int teamB = 0;
            int teamC = 0;

            for (int i = 1; i < playerdata.Count; i = i + 4)
            {
                Player p = new Player();
                p.name = playerdata[i];
                p.nameAndIdent = playerdata[i + 1];
                p.race = playerdata[i + 2];
                p.teamId = playerdata[i + 3];
                if(p.teamId.Equals("0")) teamA++;
                if(p.teamId.Equals("2")) teamB++;
                players.Add(p);
            }

            string gameTypeString;
            int gameType;
            if((teamA == teamB) && (teamC == 0))
            {
                gameTypeString = teamA.ToString() + "v" + teamA.ToString();
                gameType = teamA;
            }
            else if ((teamA == teamB) && (teamB == teamC) && (teamA == 1))
            {
                gameTypeString = "FFA";
                gameType = 5;
            }
            else
            {
                gameTypeString = "Other";
                gameType = 0;
            }

            if (config.filters[gameType] == false)
            {
                return;
            }
            else
            { 
                //game type not filtered, continue.
            }

            if (config.autoPosition)
            {
                //GENERAL SORTING OF PLAYERS PLZX
                //players.Sort(delegate(Player p1, Player p2) { return p1.name.CompareTo(p2.name); });

                if(config.typeOfSort.Equals("Player"))
                {
                    int index = players.FindIndex(delegate(Player p) { return p.name.Equals(config.playerName); });
                    if (index != -1)
                    {
                        if (index >= 0 && index < ((players.Count / 2))) //In the first half
                        {
                            if (config.position.Equals("Front"))
                            {
                                players.Insert(0, players[index]);
                                players.RemoveAt(index + 1);
                            }
                            if (config.position.Equals("Back"))
                            {
                                players.Insert(players.Count, players[index]);
                                players.RemoveAt(index);
                                int teammates = (players.Count / 2) - 1;
                                while (teammates > 0)
                                {
                                    players.Insert(players.Count, players[0]);
                                    players.RemoveAt(0);
                                    teammates--;
                                }
                            }
                        }
                        else if (index >= ((players.Count / 2))) //In the second half
                        {
                            if (config.position.Equals("Front"))
                            {
                                players.Insert(0, players[index]);
                                players.RemoveAt(index + 1);
                                int teammates = (players.Count / 2) - 1;
                                while (teammates > 0)
                                {
                                    players.Insert(1, players[players.Count - 1]);
                                    players.RemoveAt(players.Count - 1);
                                    teammates--;
                                }
                            }
                            if (config.position.Equals("Back"))
                            {
                                players.Insert(((players.Count / 2)), players[index]);
                                players.RemoveAt(index + 1);
                            }
                        }
                    }
                }

                if (config.typeOfSort.Equals("Race"))
                {
                    int index = players.FindIndex(delegate(Player p) { return p.race.Equals(config.race); });
                    if (index != -1)
                    {
                        if (index >= 1 && index < ((players.Count / 2))) //In the first half
                        {
                            players.Insert(0, players[index]);
                            players.RemoveAt(index + 1);
                        }
                        else if (index >= ((players.Count / 2))) //In the second half
                        {
                            players.Insert(0, players[index]);
                            players.RemoveAt(index + 1);
                            int teammates = (players.Count / 2) - 1;
                            while (teammates > 0)
                            {
                                players.Insert(1, players[players.Count - 1]);
                                players.RemoveAt(players.Count - 1);
                                teammates--;
                            }
                        }
                    }
                }
            }

            string filename = formatFilename(path, replayDataBuffer, ref map, players, gameTypeString);

            bool exists = System.IO.File.Exists(filename);
            if (System.IO.File.Exists(filename))
            {
            }
            else
            {
                try
                {
                    if (config.copyReplay)
                    {
                        System.IO.File.Copy(path, filename);
                        if (config.moveReplay)
                        {
                            System.IO.File.Delete(path);
                        }
                        if (config.playSound)
                        {
                            playDoneSound();
                        }
                    }
                    else
                    {
                        System.IO.File.Move(path, filename);
                        if (config.playSound)
                        {
                            playDoneSound();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private static void playDoneSound()
        {
            System.Media.SoundPlayer soundplayer = new System.Media.SoundPlayer("sound.wav");
            soundplayer.Play();
            soundplayer.Dispose();
        }

        private static List<string> readPlayerAndMapDataBeta(byte[] replayData)
        {
            //StreamReader read = new StreamReader(
            //config.watchPath = @read.ReadLine();
            //read.Close();
            byte lf = (byte)'\n';
            List<string> playerdata = new List<string>();
            ArrayList bufferList = new ArrayList();
            ArrayList byteList = new ArrayList();

            //byte[,] buffer = new byte[25, 450];
            byte[][] buffer = new byte[25][];
            buffer[0] = new byte[450];
            int bufferCounter = 0;
            int indexCounter = 0;
            int i = 0;
            int dataLenght = replayData.Length;
            while(i <= dataLenght)
            {
                if (i == dataLenght)
                {
                    byteList.Add(bufferList);
                    break;
                }
                
                if ((replayData[i] == lf) && (replayData[i-2] == 9))
                {
                    bufferCounter++;
                    //buffer[bufferCounter] = new byte[450];
                    indexCounter = 0;
                    byteList.Add(bufferList);
                    bufferList = new ArrayList();
                }
                else
                {
                    bufferList.Add(replayData[i]);
                    //buffer[bufferCounter][indexCounter] = replayData[i];
                    indexCounter++;
                }
                i++;
            }
            
            StringBuilder sbuilder = new StringBuilder();
            ArrayList arrayList = new ArrayList();
            foreach (ArrayList l in byteList)
            {
                byte[] tempArray = (byte[])l.ToArray(typeof(byte));
                arrayList.Add(tempArray);
            }

            byte[] tempArray2 = (byte[])arrayList[arrayList.Count-1];
            int mapNameSizeLocation = 13;
            int mapNameSize = tempArray2[mapNameSizeLocation];
            if (mapNameSize < 4) mapNameSizeLocation++;
            string tempString2 = ByteArrayToStr(tempArray2, mapNameSizeLocation + 1, (tempArray2[mapNameSizeLocation] / 2));
            playerdata.Add(tempString2);

            for (int x = 0; x < (arrayList.Count-1); x++)
            {
                byte[] tempArray = (byte[])arrayList[x];
                if (x == 0)
                {
                    string tempString = ByteArrayToStr((byte[])arrayList[x], 12, (tempArray[11]) / 2);
                    playerdata.Add(tempString);
                    //int nameIdentOffset = ((tempArray[12] / 2) + 23);
                    //tempString = ByteArrayToStr((byte[])arrayList[x], (13 + nameIdentOffset), (tempArray[(13 + nameIdentOffset) - 1]) / 2);
                    playerdata.Add(tempString); //add name as name.ident as that doesnt exist, workaround :/
                    //int raceOffset = (3 + (tempArray[(16 + nameIdentOffset) - 1]) / 2);
                    int raceOffset = ((tempArray[11] / 2) + 23);
                    if ((tempArray[(12 + raceOffset) - 1]) > 24) raceOffset--;
                    tempString = ByteArrayToStr((byte[])arrayList[x], (12 + raceOffset), (tempArray[(12 + raceOffset) - 1]) / 2);
                    tempString = isNotEngRace(tempString);
                    playerdata.Add(tempString);
                    tempString = tempArray[1].ToString();
                    tempString = "0"; //workaround for crappy blizzard setup
                    playerdata.Add(tempString);
                }
                else
                {
                    //int nameIdentOffset = ((tempArray[16] / 2) + 18);
                    //int raceOffset = (3 + (tempArray[(17 + nameIdentOffset) - 1]) / 2);
                    //if (tempArray[(17 + nameIdentOffset) - 1] == 0) break;
                    int raceOffset = (22 + (tempArray[(17) - 1]) / 2);
                    int nameIdentOffset = 0;
                    string tempString = ByteArrayToStr((byte[])arrayList[x], 17, (tempArray[16]) / 2);
                    playerdata.Add(tempString);
                    //tempString = ByteArrayToStr((byte[])arrayList[x], (17 + nameIdentOffset), ((tempArray[(17 + nameIdentOffset) - 1]) / 2));
                    playerdata.Add(tempString);
                    //tempString = ByteArrayToStr((byte[])arrayList[x], (17 + nameIdentOffset + raceOffset), (tempArray[(17 + nameIdentOffset + raceOffset) - 1]) / 2);
                    if ((tempArray[(17 + raceOffset) -1] < 8)) raceOffset++;
                    tempString = ByteArrayToStr((byte[])arrayList[x], (17 + nameIdentOffset + raceOffset), (tempArray[(17 + nameIdentOffset + raceOffset) - 1]) / 2);
                    tempString = isNotEngRace(tempString);
                    playerdata.Add(tempString);
                    tempString = tempArray[1].ToString();
                    playerdata.Add(tempString);
                }
                if (tempArray[tempArray.Length - 1] == 0) break;
            }
            return playerdata;
        }

        private static List<string> readPlayerAndMapData(byte[] replayData)
        {
            byte[] buffer = new byte[42];
            int bufferCnt = 0;
            List<string> playerdata = new List<string>();
            bool lastRun = false;

            for (int i = replayData.Length - 1; i > 0; i--)
            {
                if (replayData[i] == 16 && !lastRun)
                {
                    lastRun = true;
                    i--;
                }
                else if ((replayData[i] == 0) || (replayData[i] == bufferCnt || replayData[i] < 13))
                {
                    buffer = reverseBytes(buffer);
                    if (buffer[0] != 0)
                    {
                        buffer = isNotEngRace(buffer);
                        string temp = ByteArrayToStr(buffer);
                        playerdata.Add(temp);
                    }
                    buffer = new byte[42];
                    bufferCnt = 0;

                    if (lastRun == true) break;
                }
                else
                {
                    buffer[bufferCnt] = replayData[i];
                    bufferCnt++;
                }
            }
            playerdata.Reverse();
            return playerdata;
        }

        private static void writeDebugTxt(byte[] replayData, bool asChar)
        {
            System.Text.UTF8Encoding encoding = new UTF8Encoding();
            StreamWriter replayDataOut = new StreamWriter("debug.txt");
            for (int i = 0; i < replayData.Length; i++)
            {
                if ((replayData[i] == 10))
                {
                    if (!asChar)
                    {
                        replayDataOut.Write("\n" + replayData[i].ToString() + "\n");
                    }
                    else
                    {
                        //replayDataOut.Write("\n" + ((char)replayData[i]).ToString() + "\n");
                        replayDataOut.Write("\n" + encoding.GetString(replayData, i, 1) + "\n");
                    }
                }
                else
                {
                    if (!asChar)
                    {
                        replayDataOut.Write(replayData[i].ToString() + " ");
                    }
                    else
                    {
                        //replayDataOut.Write(((char)replayData[i]).ToString() + " ");
                        replayDataOut.Write(encoding.GetString(replayData, i, 1) + " ");
                    }
                }
            }
            replayDataOut.Flush();
            replayDataOut.Close();
        }

        private static string formatFilename(string path, byte[] replayDataBuffer, ref string map, List<Player> players, string gameType)
        {
            bool addId = true;
            string filename = "";

            string tail = getUniqueIdentifier(replayDataBuffer);

            int v = 0;
            //#Normal: Player(R) vs Player(R) on Map[id].SC2Replay
            if (config.format.Equals("Normal"))
            {
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + "(" + shortRace(p.race) + ") ";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs ";
                }
                filename += "on " + map;
            }
            //#Normal.: Player(R).vs.Player(R).on.Map[id].SC2Replay
            if (config.format.Equals("Normal."))
            {
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + "(" + shortRace(p.race) + ").";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs.";
                }
                filename += "on." + map;
            }
            //#Matchup: RvR Player vs Player on Map[id].SC2Replay
            if (config.format.Equals("Matchup"))
            {
                foreach (Player p in players)
                {
                    v++;
                    filename += shortRace(p.race);
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
                }
                filename += " ";
                v = 0;
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + " ";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs ";
                }
                filename += "on " + map;
            }
            //#Matchup.: RvR.Player.vs.Player.on.Map[id].SC2Replay
            if (config.format.Equals("Matchup."))
            {
                foreach (Player p in players)
                {
                    v++;
                    filename += shortRace(p.race);
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
                }
                filename += ".";
                v = 0;
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + ".";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs.";
                }
                filename += "on." + map;
            }
            //#Map: Map.RvR.Player vs Player[id].SC2Replay
            if (config.format.Equals("Map"))
            {
                filename += map + " ";

                foreach (Player p in players)
                {
                    v++;
                    filename += shortRace(p.race);
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
                }
                filename += " ";
                v = 0;
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + " ";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs ";
                }

            }
            //#Map.: Map.RvR.Player.vs.Player[id].SC2Replay
            if (config.format.Equals("Map."))
            {

                filename += map + ".";

                foreach (Player p in players)
                {
                    v++;
                    filename += shortRace(p.race);
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
                }
                filename += ".";
                v = 0;
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + ".";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs.";
                }

            }
            //#Date: yyyy.m.dd.hhmm Player(R) vs Player(R) on Map.SC2Replay
            if (config.format.Equals("Date"))
            {
                addId = false;

                filename = getDateFromFile(path, filename);
                filename += " ";

                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + "(" + shortRace(p.race) + ") ";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs ";
                }
                filename += "on " + map;
            }
            //#DateMatchup: yyyy.mm.dd.hhmm.RvR.Player.vs.Player.on.Map.SC2Replay
            if (config.format.Equals("DateMatchup"))
            {
                addId = false;

                filename = getDateFromFile(path, filename);
                filename += ".";

                foreach (Player p in players)
                {
                    v++;
                    filename += shortRace(p.race);
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
                }
                filename += ".";
                v = 0;
                foreach (Player p in players)
                {
                    v++;
                    filename += p.name + ".";
                    if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs.";
                }
                filename += "on." + map;
            }

            if (config.format.Equals("Dynamic"))
            {

                addId = false;
                string[] dynamicParams = config.dynamicParamString.Split('|');
                foreach (string s in dynamicParams)
                {
                    v = 0;
                    if (s.Equals("Matchup"))
                    {
                        filename = getMatchupString(players, filename);
                    }
                    else if (s.Equals("Map"))
                    {
                        filename += map;
                    }
                    else if (s.Equals("Players"))
                    {
                        filename = getPlayersString(players, filename, " ");
                    }
                    else if (s.Equals("Players."))
                    {
                        filename = getPlayersString(players, filename, ".");
                    }
                    else if (s.Equals("Normal"))
                    {
                        filename = getNormalString(players, filename, " ");
                    }
                    else if (s.Equals("Normal."))
                    {
                        filename = getNormalString(players, filename, ".");
                    }
                    else if (s.Equals("Date"))
                    {
                        filename = getDateFromFile(path, filename);
                    }
                    else if (s.Equals("ID"))
                    {
                        filename += tail;
                    }
                    else
                    {
                        filename += s;
                    }
                }
            }

            if (addId)
            {
                filename += "[" + tail + "]";
            }
            filename += ".SC2Replay";

            filename = validateFilename(filename);

            //ADD PATH IF ANY
            string originalPath= "";
            if (config.copyReplay)
            {
                originalPath = config.outputPath + "\\";
                if (config.copyToGametypeFolder)
                {
                    originalPath = originalPath + gameType + "\\";
                    System.IO.Directory.CreateDirectory(originalPath);
                }
            }
            else
            {
                originalPath = System.IO.Path.GetDirectoryName(path);
                if (!originalPath.Equals("")) originalPath += "\\";
            }

            filename = originalPath + filename;

            return filename;
        }

        private static string getPlayersString(List<Player> players, string filename, string separator)
        {
            int v = 0;
            foreach (Player p in players)
            {
                v++;
                filename += p.name;
                if (v != players.Count) filename += separator;
                if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs" + separator;
            }
            return filename;
        }

        private static string getMatchupString(List<Player> players, string filename)
        {
            int v = 0;
            foreach (Player p in players)
            {
                v++;
                filename += shortRace(p.race);
                if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "v";
            }
            return filename;
        }

        private static string getNormalString(List<Player> players, string filename, string separator)
        {
            int v = 0;
            foreach (Player p in players)
            {
                v++;
                filename += p.name + "(" + shortRace(p.race) + ")";
                if (v != players.Count) filename += separator;
                if (v == (players.Count / 2) && (players.Count % 2 == 0)) filename += "vs" + separator;
            }
            return filename;
        }

        private static string getDateFromFile(string path, string filename)
        {
            DateTime dtest = System.IO.File.GetLastWriteTime(path);
            string date = dtest.ToString("yyyy'.'MM'.'dd'.'HHmm");
            filename += date;
            return filename;
        }

        private static string validateFilename(string filename)
        {
            char[] invalidFilenameChars = Path.GetInvalidFileNameChars();
            string invalidString = Regex.Escape(new string(invalidFilenameChars));
            filename = Regex.Replace(filename, "[" + invalidString + "]", "");
            return filename;
        }

        private static string getUniqueIdentifier(byte[] replayDataBuffer)
        {
            CRC32 crc = new CRC32();

            byte[] tailInBytes = crc.ComputeHash(replayDataBuffer);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tailInBytes.Length; i++)
            {
                sb.Append(tailInBytes[i].ToString("x2"));
            }
            string tail = sb.ToString();
            return tail;
        }

        private static byte[] isNotEngRace(byte[] buffer)
        {
            if (compareByteArray(buffer, koreanTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, koreanZerg))
            { buffer = engZerg; }
            if (compareByteArray(buffer, koreanProtoss))
            { buffer = engProtoss; }
            if (compareByteArray(buffer, russianTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, germanTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, polishTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, polishZerg))
            { buffer = engZerg; }
            if (compareByteArray(buffer, polishProtoss))
            { buffer = engProtoss; }
            return buffer;
        }

        private static string isNotEngRace(string inputString)
        {
            byte[] buffer = StrToByteArray(inputString);
            if (compareByteArray(buffer, koreanTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, koreanZerg))
            { buffer = engZerg; }
            if (compareByteArray(buffer, koreanProtoss))
            { buffer = engProtoss; }
            if (compareByteArray(buffer, russianTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, germanTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, polishTerran))
            { buffer = engTerran; }
            if (compareByteArray(buffer, polishZerg))
            { buffer = engZerg; }
            if (compareByteArray(buffer, polishProtoss))
            { buffer = engProtoss; }
            return ByteArrayToStr(buffer);
        }

        private static void menuItem1_Click(object Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
