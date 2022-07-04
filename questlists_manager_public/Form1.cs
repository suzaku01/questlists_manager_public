using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Dictionary;

namespace questlists_manager
{
    public partial class questlists_manager : Form
    {
        public questlists_manager()
        {
            InitializeComponent();
        }

        int totalQuestCount = 0;
        Dictionary<int, List<string>> questDataDic = new Dictionary<int, List<string>>();
        Dictionary<int, List<int>> indexDic = new Dictionary<int, List<int>>();     //[{0,1,2,3,4,}
        int seaonVal = 0;       //値がどのデフォルト値にも合致しない場合、この変数に保存された値がそのままセーブ時に使用される
        int courseVal = 0;
        string[] dbByte = new string[60000];
        string[] dbNames = new string[60000];
        int selIndex = 0;
        int dbCount = 0;

        void ManageLogs(string text)
        {
            labelLog.Text = text;
        }

        private void buttonOpenFolder_Click(object sender, EventArgs e)
        {
            bool isLoaded = false;
            DialogResult folderBrowser = folderBrowserDialog1.ShowDialog();
            if (folderBrowser == DialogResult.OK)
            {
                listQuest.Items.Clear();
                listQuest.ClearSelected();
                questDataDic[0].Clear();
                totalQuestCount = 0;

                string folderLocation = folderBrowserDialog1.SelectedPath;
                string[] allFiles = Directory.GetFiles(folderBrowserDialog1.SelectedPath).Select(Path.GetFileName).ToArray();
                int fileNameNumber = 0;
                for (int i = 0; i < allFiles.Length; i++)
                {
                    string fileName = "list_" + fileNameNumber.ToString() + ".bin";
                    if (allFiles.Contains(fileName))
                    {
                        isLoaded = true;
                        byte[] byteData = File.ReadAllBytes(folderLocation + "/" + fileName);
                        int questCount = byteData[1];
                        fileNameNumber = fileNameNumber + questCount;
                        string nextFileName = "list_" + fileNameNumber.ToString() + ".bin";

                        int prevPointer = 8;
                        for (int u = 0; u < questCount; u++)
                        {
                            byte[] header = byteData.Skip(prevPointer).Take(16).ToArray();
                            int length = header[14] * 256 + header[15];
                            byte[] data = byteData.Skip(prevPointer + 16).Take(length).ToArray();
                            string str1 = BitConverter.ToString(header).Replace("-", string.Empty);
                            string str2 = BitConverter.ToString(data).Replace("-", string.Empty);
                            str1 = str1 + str2;

                            int num1 = 0;
                            if (u != questCount - 1)        //一番最後のクエストだけこの処理を行わない(意味がないため)
                            {
                                for (int t = 1; t < 250; t++)
                                {
                                    int val = prevPointer + 16 + length + t;
                                    if (val < byteData.Length)
                                    {
                                        if (byteData[val] == 64 && byteData[val + 1] == 1 && byteData[val - 1] == 0)
                                        {
                                            num1 = val - 56;
                                            break;
                                        }
                                    }
                                }
                                prevPointer = num1;
                            }
                            List<string> list = questDataDic[0];
                            list.Add(str1);
                            questDataDic[0] = list;
                            totalQuestCount = totalQuestCount + 1;
                        }

                        //check if there's next file
                        //if (allFiles.Contains(nextFileName))
                        //{
                        //    labelLog.Text = fileNameNumber.ToString();
                        //}
                        //else
                        //{
                        //    labelLog.Text = fileNameNumber.ToString();
                        //    break;
                        //}
                    }
                    else
                    {
                        MessageBox.Show("Could not find proper questlists file.");
                        break;
                    }
                }

                if (isLoaded)
                {
                    ManageLogs("Files has been loaded.");
                    numQuestCount.Value = totalQuestCount;

                    for (int i = 0; i < questDataDic[0].Count; i++)
                    {
                        List<string> list = questDataDic[0];
                        string text = list[i];
                        var listData = new List<byte>();
                        for (int r = 0; r < text.Length / 2; r++)
                        {
                            listData.Add(Convert.ToByte(text.Substring(r * 2, 2), 16));
                        }
                        listData.RemoveRange(0, 16);
                        byte[] data = listData.ToArray();
                        int pTitleAndName = BitConverter.ToInt32(data, 320);
                        int pMainoObj = BitConverter.ToInt32(data, 324);
                        string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(data.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
                        listQuest.Items.Add(tTitleAndName);
                    }

                    if (listQuest.SelectedIndex == -1 && listQuest.Items.Count != 0)    //インデックスがマイナス(未選択)かつ、個数が最低1以上の時
                    {
                        listQuest.SelectedIndex = 0;
                    }
                }

            }
        }

        private void questlists_manager_Load(object sender, EventArgs e)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var str = new List<string>();
            questDataDic.Add(0, str);       //queslists
            questDataDic.Add(1, str);       //databaseの各クエスト
            questDataDic.Add(2, str);       //databaseのクエストの名前だけ

            var num = new List<int>();
            indexDic.Add(0, num);

            comMonsterName.Items.AddRange(File.ReadAllLines("data/names/monster.txt"));
            comMonsterName.SelectedIndex = 0;
            comItemName.Items.AddRange(File.ReadAllLines("data/names/item.txt"));
            comItemName.SelectedIndex = 0;
            comMelee.Items.AddRange(File.ReadAllLines("data/names/melee.txt"));
            comMelee.SelectedIndex = 0;
            comRanged.Items.AddRange(File.ReadAllLines("data/names/ranged.txt"));
            comRanged.SelectedIndex = 0;
            comHead.Items.AddRange(File.ReadAllLines("data/names/head.txt"));
            comHead.SelectedIndex = 0;
            comChset.Items.AddRange(File.ReadAllLines("data/names/chest.txt"));
            comChset.SelectedIndex = 0;
            comArms.Items.AddRange(File.ReadAllLines("data/names/arms.txt"));
            comArms.SelectedIndex = 0;
            comboWasit.Items.AddRange(File.ReadAllLines("data/names/waist.txt"));
            comboWasit.SelectedIndex = 0;
            comLegs.Items.AddRange(File.ReadAllLines("data/names/legs.txt"));
            comLegs.SelectedIndex = 0;
            comRest.Items.AddRange(File.ReadAllLines("data/restrictions.txt"));
            comRest.SelectedIndex = 0;
            comMap.Items.AddRange(File.ReadAllLines("data/names/map.txt"));
            comMap.SelectedIndex = 0;
            listQuest.HorizontalScrollbar = true;

            //this.textBoxSearchBox.TextChanged += new System.EventHandler(this.textBoxSearchBox_TextChanged);
            //textBoxSearchBox.Leave += new System.EventHandler(this.textBoxSearchBox_Leave);
            listDataabse.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textBoxSearchBox_KeyUp);
            //this.Height = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height / 2;
            //this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width / 2;
        }

        private void listQuest_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listQuest.Items.Count != 0 && listQuest.SelectedIndex != -1)    //リストにあるクエスト数が1以上かつ、インデックスがマイナスではない時
            {
                List<string> list = questDataDic[0];
                string text = list[listQuest.SelectedIndex];
                var by = new List<byte>();
                for (int i = 0; i < text.Length / 2; i++)
                {
                    by.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                }

                if (by.Count != 0)
                {
                    comMark.SelectedIndex = by[11];
                    comQuestType.SelectedIndex = by[4];
                    numPlayers.Value = by[3];

                    by.RemoveRange(0, 16);
                    byte[] byteData = by.ToArray();     //新しいbyte[]にしているが特に意味はないと思われる

                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 48), out string targetOM);
                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 56), out string targetOA);
                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 64), out string targetOB);
                    comQuestTypeM.Text = targetOM;
                    comQuestTypeA.Text = targetOA;
                    comQuestTypeB.Text = targetOB;
                    int targetM = BitConverter.ToUInt16(byteData, 52);
                    int targetA = BitConverter.ToUInt16(byteData, 60);
                    int targetB = BitConverter.ToUInt16(byteData, 68);
                    if (targetOM != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetM, out string targetIDM);
                        textTargetM.Text = targetIDM;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetM, out string targetIDM);
                        textTargetM.Text = targetIDM;
                    }

                    if (targetOA != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetA, out string targetIDA);
                        textTargetA.Text = targetIDA;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetA, out string targetIDA);
                        textTargetA.Text = targetIDA;
                    }

                    if (targetOB != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetB, out string targetIDB);
                        textTargetB.Text = targetIDB;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetB, out string targetIDB);
                        textTargetB.Text = targetIDB;
                    }

                    if (comQuestTypeM.SelectedIndex == 4 || comQuestTypeM.SelectedIndex == 5)
                    {
                        numQuantityM.Value = BitConverter.ToUInt16(byteData, 54) * 100;
                    }
                    else
                    {
                        numQuantityM.Value = BitConverter.ToUInt16(byteData, 54);
                    }

                    if (comQuestTypeA.SelectedIndex == 4 || comQuestTypeA.SelectedIndex == 5)
                    {
                        numQuantityA.Value = BitConverter.ToUInt16(byteData, 62) * 100;
                    }
                    else
                    {
                        numQuantityA.Value = BitConverter.ToUInt16(byteData, 62);
                    }

                    if (comQuestTypeB.SelectedIndex == 4 || comQuestTypeB.SelectedIndex == 5)
                    {
                        numQuantityB.Value = BitConverter.ToUInt16(byteData, 70) * 100;
                    }
                    else
                    {
                        numQuantityB.Value = BitConverter.ToUInt16(byteData, 70);
                    }

                    numMonsterIcon1.Value = byteData[185];
                    numMonsterIcon2.Value = byteData[186];
                    numMonsterIcon3.Value = byteData[187];
                    numMonsterIcon4.Value = byteData[188];
                    numMonsterIcon5.Value = byteData[189];

                    //List.MonsterID.TryGetValue(byteData[185], out string Icon1);
                    //List.MonsterID.TryGetValue(byteData[186], out string Icon2);
                    //List.MonsterID.TryGetValue(byteData[187], out string Icon3);
                    //List.MonsterID.TryGetValue(byteData[188], out string Icon4);
                    //List.MonsterID.TryGetValue(byteData[189], out string Icon5);
                    //textMonsterIcon1.Text = Icon1;
                    //textMonsterIcon2.Text = Icon2;
                    //textMonsterIcon3.Text = Icon3;
                    //textMonsterIcon4.Text = Icon4;
                    //textMonsterIcon5.Text = Icon5;

                    //Text
                    int pTitleAndName = BitConverter.ToUInt16(byteData, 320);
                    int pMainoObj = BitConverter.ToUInt16(byteData, 324);
                    int pAObj = BitConverter.ToUInt16(byteData, 328);
                    int pBObj = BitConverter.ToUInt16(byteData, 332);
                    int pClearC = BitConverter.ToUInt16(byteData, 336);
                    int pFailC = BitConverter.ToUInt16(byteData, 340);
                    int pEmp = BitConverter.ToUInt16(byteData, 344);
                    int pText = BitConverter.ToUInt16(byteData, 348);

                    string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
                    textTitle.Text = tTitleAndName;
                    string tMainObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pMainoObj).Take(pAObj - pMainoObj).ToArray()).Replace("\n", "\r\n");
                    textMain.Text = tMainObj;

                    if (pAObj == pBObj)
                    {
                        string tAObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pAObj).Take(pClearC - pAObj).ToArray()).Replace("\n", "\r\n");
                        textA.Text = tAObj;
                        textB.Text = tAObj;
                    }
                    else
                    {
                        string tAObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pAObj).Take(pBObj - pAObj).ToArray()).Replace("\n", "\r\n");
                        textA.Text = tAObj;
                        string tBObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pBObj).Take(pClearC - pBObj).ToArray()).Replace("\n", "\r\n");
                        textB.Text = tBObj;
                    }

                    string tClearC = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pClearC).Take(pFailC - pClearC).ToArray()).Replace("\n", "\r\n");
                    textClear.Text = tClearC;
                    string tFailC = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pFailC).Take(pEmp - pFailC).ToArray()).Replace("\n", "\r\n");
                    textFail.Text = tFailC;
                    string tEmp = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pEmp).Take(pText - pEmp).ToArray()).Replace("\n", "\r\n");
                    textEmp.Text = tEmp;
                    string tText = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pText).Take(byteData.Length - pText).ToArray()).Replace("\n", "\r\n");
                    textText.Text = tText;

                    //Misc
                    numDifficulty.Value = byteData[4];
                    numQuestID.Value = BitConverter.ToUInt16(byteData, 46);
                    numFee.Value = BitConverter.ToUInt32(byteData, 12);
                    numMainReward.Value = BitConverter.ToUInt32(byteData, 16);
                    numSubAReward.Value = BitConverter.ToUInt32(byteData, 24);
                    numSubBReward.Value = BitConverter.ToUInt32(byteData, 28);
                    numMainPoint.Value = BitConverter.ToUInt32(byteData, 164);
                    numSubAPoint.Value = BitConverter.ToUInt32(byteData, 168);
                    numSubBPoint.Value = BitConverter.ToUInt32(byteData, 172);
                    numReqMin.Value = BitConverter.ToUInt16(byteData, 74);
                    numReqMax.Value = BitConverter.ToUInt16(byteData, 76);
                    numReqHost.Value = BitConverter.ToUInt16(byteData, 78);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 176), out string Item1);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 178), out string Item2);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 180), out string Item3);
                    textItem1.Text = Item1;
                    textItem2.Text = Item2;
                    textItem3.Text = Item3;
                    if (byteData[6] > 22)
                    {
                        comCourse.SelectedIndex = 27;
                    }
                    else
                    {
                        comCourse.SelectedIndex = byteData[6];
                    }
                    numTime.Value = BitConverter.ToUInt32(byteData, 32) / 30/ 60;
                    List.MapID.TryGetValue(BitConverter.ToUInt16(byteData, 36), out string map);
                    comMap.Text = map;
                    comRest.SelectedIndex = byteData[44];

                    if (byteData[151] > 33)
                    {
                        comHRGSRType.SelectedIndex = 33;
                    }
                    else
                    {
                        if (byteData[151] > 8)
                        {
                            if (byteData[152] != 0)
                            {
                                comHRGSRType.SelectedIndex = 10;
                            }
                            else
                            {
                                comHRGSRType.SelectedIndex = byteData[151];
                            }
                        }
                        else
                        {
                            comHRGSRType.SelectedIndex = byteData[151];
                        }
                    }

                    numQuestType1.Value = byteData[151];
                    numQuestType2.Value = BitConverter.ToUInt16(byteData, 152);

                    int num = 0;
                    switch (byteData[2])
                    {
                        case 10:
                            num = 0;
                            break;
                        case 18:
                            num = 1;
                            break;
                        case 12:
                            num = 2;
                            break;
                        case 20:
                            num = 3;
                            break;
                        case 9:
                            num = 4;
                            break;
                        case 17:
                            num = 5;
                            break;

                        case 74:
                            num = 6;
                            break;
                        case 82:
                            num = 7;
                            break;
                        case 76:
                            num = 8;
                            break;
                        case 84:
                            num = 9;
                            break;
                        case 73:
                            num = 10;
                            break;
                        case 81:
                            num = 11;
                            break;
                        case 0:
                            num = 12;
                            break;

                        default:
                            num = 13;
                            seaonVal = byteData[2];
                            break;
                    }
                    comSeason.SelectedIndex = num;

                    //Equipment
                    List.LegsID.TryGetValue(BitConverter.ToUInt16(byteData, 92), out string legs1);
                    textLegs1.Text = legs1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 94), out string legs2);
                    textLegs2.Text = legs2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 96), out string legs3);
                    textLegs3.Text = legs3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 98), out string legs4);
                    textLegs4.Text = legs4;
                    numLegs1.Value = BitConverter.ToUInt16(byteData, 92);

                    List.MeleeID.TryGetValue(BitConverter.ToUInt16(byteData, 100), out string melee1);
                    textWep.Text = melee1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 102), out string melee2);
                    textWep2.Text = melee2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 104), out string melee3);
                    textWep3.Text = melee3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 106), out string melee4);
                    textWep4.Text = melee4;
                    numWep1.Value = BitConverter.ToUInt16(byteData, 100);

                    List.HeadID.TryGetValue(BitConverter.ToUInt16(byteData, 108), out string head1);
                    textHead1.Text = head1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 110), out string head2);
                    textHead2.Text = head2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 112), out string head3);
                    textHead3.Text = head3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 114), out string head4);
                    textHead4.Text = head4;
                    numHead1.Value = BitConverter.ToUInt16(byteData, 108);

                    List.ChestID.TryGetValue(BitConverter.ToUInt16(byteData, 116), out string chest1);
                    textChest1.Text = chest1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 118), out string chest2);
                    textChest2.Text = chest2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 120), out string chest3);
                    textChest3.Text = chest3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 122), out string chest4);
                    textChest4.Text = chest4;
                    numChest1.Value = BitConverter.ToUInt16(byteData, 116);

                    List.ArmsID.TryGetValue(BitConverter.ToUInt16(byteData, 124), out string arms1);
                    textArms1.Text = arms1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 126), out string arms2);
                    textArms2.Text = arms2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 128), out string arms3);
                    textArms3.Text = arms3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 130), out string arms4);
                    textArms4.Text = arms4;
                    numArms1.Value = BitConverter.ToUInt16(byteData, 124);

                    List.WaistID.TryGetValue(BitConverter.ToUInt16(byteData, 132), out string waist1);
                    textWaist1.Text = waist1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 134), out string waist2);
                    textWaist2.Text = waist2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 136), out string waist3);
                    textWaist3.Text = waist3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 138), out string waist4);
                    textWaist4.Text = waist4;
                    numWaist1.Value = BitConverter.ToUInt16(byteData, 132);

                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 148), out string item);
                    textItemReqName.Text = item;
                    numItemReqID.Value = BitConverter.ToUInt16(byteData, 148);
                    numItemReqQua.Value = byteData[150];
                }
            }
        }

        private void buttonSaveAll_Click(object sender, EventArgs e)
        {
            if (listQuest.Items.Count != 0)
            {
                byte[] questEnd1 = { 18, 131, 89, 137, 91, 131, 58, 88, 182, 142, 89, 130, 204, 131, 88, 131, 88, 131, 129, 44, 151, 05, 65, 00, 00 };
                byte[] questEnd2 = { 0 }; //0,0
                byte[] header = { 00, 42, 13, 125, 143, 204, 00, 00 };
                byte[] fileEnd = File.ReadAllBytes("data/end.bin");
                int questCount = 0;
                int count = 42;
                int loopCount = questDataDic[0].Count / 42;   //170 / 42 = 4
                int rest = questDataDic[0].Count - (42 * loopCount);     //2
                for (int r = 0; r < loopCount + 1; r++)
                {
                    if (r != loopCount)
                    {
                        var by = new List<byte>();
                        List<string> list = questDataDic[0];

                        if (list.Count < 42)
                        {
                            count = list.Count;     //クエスト数が42個より少ない場合
                        }
                        else
                        {
                            count = 42;
                        }

                        for (int u = 0; u < count; u++)
                        {
                            string text = list[questCount];
                            var quest = new List<byte>();
                            for (int i = 0; i < text.Length / 2; i++)
                            {
                                quest.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                            }

                            if (u == 0)
                            {
                                by.AddRange(header);
                            }

                            if (u != count - 1)
                            {
                                by.AddRange(quest);
                                by.AddRange(questEnd1);
                            }
                            else        //最後のみ
                            {
                                by.AddRange(quest);
                                by.AddRange(questEnd2);
                                by.AddRange(fileEnd);
                                int val = questCount - count + 1;
                                string path = "output/" + "list_" + val.ToString() + ".bin";
                                File.WriteAllBytes(path, by.ToArray());
                            }
                            questCount = questCount + 1;

                        }
                    }
                    else
                    {
                        var by = new List<byte>();
                        if (rest != 0)
                        {
                            for (int u = 0; u < rest; u++)
                            {
                                List<string> list = questDataDic[0];
                                string text = list[questCount];
                                var quest = new List<byte>();
                                for (int i = 0; i < text.Length / 2; i++)
                                {
                                    quest.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                                }

                                header[1] = (byte)rest;
                                if (u == 0)
                                {
                                    by.AddRange(header);
                                }

                                if (u != rest - 1)
                                {
                                    by.AddRange(quest);
                                    by.AddRange(questEnd1);
                                }
                                else
                                {
                                    by.AddRange(quest);
                                    by.AddRange(questEnd2);
                                    by.AddRange(fileEnd);
                                }
                                questCount = questCount + 1;
                            }
                            int val = questCount - rest;
                            string path = "output/" + "list_" + val.ToString() + ".bin";
                            File.WriteAllBytes(path, by.ToArray());
                        }
                    }
                }
                ManageLogs("Export is complete.");
                System.Media.SystemSounds.Asterisk.Play();
                //DialogResult result = MessageBox.Show("Do you want to open the exported folder?", "Export is completed", MessageBoxButtons.YesNo);

                //if (result == System.Windows.Forms.DialogResult.Yes)
                //{
                //    System.Diagnostics.Process.Start("output/");
                //}
            }
        }

        private void buttonCreateNew_Click(object sender, EventArgs e)
        {
            byte[] template = File.ReadAllBytes("data/template.bin");
            List<string> list = questDataDic[0];
            list.Add(BitConverter.ToString(template).Replace("-", string.Empty));
            questDataDic[0] = list;
            numQuestCount.Value = numQuestCount.Value + 1;

            var listData = template.ToList();
            listData.RemoveRange(0, 16);
            byte[] data = listData.ToArray();
            int pTitleAndName = BitConverter.ToInt32(data, 320);
            int pMainoObj = BitConverter.ToInt32(data, 324);
            string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(data.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
            listQuest.Items.Add(tTitleAndName);
            ManageLogs("Blank quest have been added.");
        }

        private void buttonAddNew_Click(object sender, EventArgs e)
        {
            DialogResult opneFileDialog = openFileDialog1.ShowDialog();
            if (opneFileDialog == DialogResult.OK)
            {
                byte[] by = File.ReadAllBytes(openFileDialog1.FileName);
                if (by[0] == 192)
                {
                    by = File.ReadAllBytes(openFileDialog1.FileName);
                    AddNewQuestFromFile(by);
                }
                else
                {
                    MessageBox.Show("This is not a quest file.");
                }
            }
        }

        void AddNewQuestFromFile(byte[] by)
        {
            var by1 = new List<byte>();
            byte[] data = by.Skip(192).Take(320).ToArray();
            by1.AddRange(data);     //140

            int textPointer = BitConverter.ToInt16(by, 48);     //AE4
            int textPointer1 = BitConverter.ToInt16(by, textPointer);
            textPointer1 = textPointer1 - 32;
            byte[] pointerData = by.Skip(textPointer1).Take(32).ToArray();
            by1.AddRange(pointerData);  //160
            int leng = by1.Count;

            by1[40] = 64;
            by1[41] = 1;

            int pointer = BitConverter.ToInt16(by, textPointer + 4);
            int val = by[pointer];
            if (val == 0)
            {
                pointer = pointer + 1;
            }

            int pTitleAndName = BitConverter.ToInt16(by, textPointer1);
            int pMainoObj = BitConverter.ToInt16(by, textPointer1 + 4);
            int pAObj = BitConverter.ToInt16(by, textPointer1 + 8);
            int pBObj = BitConverter.ToInt16(by, textPointer1 + 12);
            int pClearC = BitConverter.ToInt16(by, textPointer1 + 16);
            int pFailC = BitConverter.ToInt16(by, textPointer1 + 20);
            int pEmp = BitConverter.ToInt16(by, textPointer1 + 24);
            int pText = BitConverter.ToInt16(by, textPointer1 + 28);

            int a1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray().Length;
            int a2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray().Length;
            int a3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray().Length;
            int a4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray().Length;
            int a5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray().Length;
            int a6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray().Length;
            int a7 = by.Skip(pEmp).Take(pText - pEmp).ToArray().Length;
            int a8 = by.Skip(pText).Take(by.Length - pText).ToArray().Length;

            byte[] b1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray();
            byte[] b2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray();
            byte[] b3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray();
            byte[] b4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray();
            byte[] b5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray();
            byte[] b6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray();
            byte[] b7 = by.Skip(pEmp).Take(pText - pEmp).ToArray();
            byte[] b8 = by.Skip(pText).Take(by.Length - pText).ToArray();
            string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
            listQuest.Items.Add(tTitleAndName);
            by1.AddRange(b1);
            by1.AddRange(b2);
            by1.AddRange(b3);
            by1.AddRange(b4);
            by1.AddRange(b5);
            by1.AddRange(b6);
            by1.AddRange(b7);
            by1.AddRange(b8);

            leng = leng - 32;
            int num = leng + 32;
            byte[] c1 = BitConverter.GetBytes(num);    //01,60
            by1[leng] = c1[0];
            by1[leng + 1] = c1[1];

            num = num + a1;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 4] = c1[0];
            by1[leng + 5] = c1[1];

            num = num + a2;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 8] = c1[0];
            by1[leng + 9] = c1[1];

            num = num + a3;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 12] = c1[0];
            by1[leng + 13] = c1[1];

            num = num + a4;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 16] = c1[0];
            by1[leng + 17] = c1[1];

            num = num + a5;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 20] = c1[0];
            by1[leng + 21] = c1[1];

            num = num + a6;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 24] = c1[0];
            by1[leng + 25] = c1[1];

            num = num + a7;
            c1 = BitConverter.GetBytes(num);
            by1[leng + 28] = c1[0];
            by1[leng + 29] = c1[1];

            //header
            leng = by1.Count;       //478
            byte[] b = BitConverter.GetBytes(leng); //2C8

            byte[] header = { 00, 00, 15, 04, 18, 01, 00, 00, 00, 00, 00, 00, 00, 00, 255, 255 };
            header[14] = b[1];
            header[15] = b[0];
            var by2 = new List<byte>();
            by2.AddRange(header);
            by2.AddRange(by1);

            List<string> list = questDataDic[0];
            list.Add(BitConverter.ToString(by2.ToArray()).Replace("-", string.Empty));
            questDataDic[0] = list;
            numQuestCount.Value = numQuestCount.Value + 1;
            ManageLogs("New quest have been added.");
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (listQuest.Items.Count != 0 && listQuest.SelectedIndex != -1)     //0個のときはそもそも実行されない
            {
                if (checkBoxAskDelete.Checked)
                {
                    DialogResult result = MessageBox.Show("Do you really want to remove this quest from list?", "Message box", MessageBoxButtons.YesNo);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                    {
                        int deleteQuestNo = listQuest.SelectedIndex;
                        List<string> list = questDataDic[0];
                        string text = listQuest.SelectedItems[0].ToString();
                        text = text.Replace("\r\n", "");
                        list.RemoveAt(deleteQuestNo);
                        questDataDic[0] = list;
                        listQuest.Items.RemoveAt(deleteQuestNo);
                        numQuestCount.Value = Math.Max(numQuestCount.Value - 1, 0);
                        if (deleteQuestNo == 0 && listQuest.Items.Count != 0)
                        {
                            listQuest.SelectedIndex = 0;
                        }
                        else
                        {
                            listQuest.SelectedIndex = deleteQuestNo - 1;
                        }

                        ManageLogs($"Removed {text}");
                    }
                }
                else
                {
                    int deleteQuestNo = listQuest.SelectedIndex;
                    List<string> list = questDataDic[0];
                    string text = listQuest.SelectedItems[0].ToString();
                    text = text.Replace("\r\n", "");
                    list.RemoveAt(deleteQuestNo);
                    questDataDic[0] = list;
                    listQuest.Items.RemoveAt(deleteQuestNo);
                    numQuestCount.Value = Math.Max(numQuestCount.Value - 1, 0);
                    if (deleteQuestNo == 0 && listQuest.Items.Count != 0)
                    {
                        listQuest.SelectedIndex = 0;
                    }
                    else
                    {
                        listQuest.SelectedIndex = deleteQuestNo - 1;
                    }

                    ManageLogs($"Removed {text}");
                }

                
            }
        }

        private void buttonToUp_Click(object sender, EventArgs e)
        {
            if (listQuest.Items.Count > 1 && listQuest.SelectedIndex != -1)
            {
                int curIndex = listQuest.SelectedIndex;
                int newIndex = listQuest.SelectedIndex - 1;
                if (curIndex != 0)
                {
                    List<string> list = questDataDic[0];
                    list.Swap(curIndex, newIndex);
                    questDataDic[0] = list;
                    MoveUp();
                }
            }
        }

        private void buttonToDown_Click(object sender, EventArgs e)
        {
            if (listQuest.Items.Count > 1 && listQuest.SelectedIndex != -1)
            {
                int curIndex = listQuest.SelectedIndex;
                int newIndex = Math.Min(listQuest.SelectedIndex + 1, listQuest.Items.Count);
                if (curIndex + 1 != listQuest.Items.Count)
                {
                    List<string> list = questDataDic[0];
                    list.Swap(curIndex, newIndex);
                    questDataDic[0] = list;
                    MoveDown();
                }
            }
        }

        private void buttonSaveChanegs_Click(object sender, EventArgs e)
        {
            if (listQuest.SelectedIndex != -1)
            {
                //byte[] header = { 00, 00, 15, 04, 18, 01, 00, 00, 00, 00, 00, 00, 00, 00, 255, 255 };
                byte[] divider = { 00 };

                List<string> list = questDataDic[0];
                string strData = list[listQuest.SelectedIndex];
                var byteList = new List<byte>();
                for (int i = 0; i < strData.Length / 2; i++)
                {
                    byteList.Add(Convert.ToByte(strData.Substring(i * 2, 2), 16));
                }

                byte[] header = byteList.Skip(0).Take(16).ToArray();
                header[3] = (byte)numPlayers.Value;
                header[4] = (byte)comQuestType.SelectedIndex;
                header[11] = (byte)comMark.SelectedIndex;

                byteList.RemoveRange(0, 16);
                byte[] byteArray = byteList.ToArray();

                ////////////////////////////////////////////////////////////////////////text
                int readPointer = BitConverter.ToInt32(byteArray, 40);       //140
                int textStart = BitConverter.ToInt32(byteArray, readPointer);        //160
                byteList.RemoveRange(352, byteList.Count - 352);


                byte[] fb = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textTitle.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Main obj
                byte[] mo = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textMain.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Sub A
                byte[] sa = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textA.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////SUb B
                byte[] sb = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textB.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Clear
                byte[] cc = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textClear.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Fail
                byte[] fc = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textFail.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Emp
                byte[] em = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textEmp.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                ////Text
                byte[] tx = BitConverter.GetBytes(byteList.Count);
                byteList.AddRange(Encoding.GetEncoding("Shift_JIS").GetBytes(textText.Text.Replace("\r\n", "\n")).ToList());
                byteList.AddRange(divider);

                byteArray = byteList.ToArray();
                byteList[readPointer + 0] = fb[0];
                byteList[readPointer + 1] = fb[1];
                byteList[readPointer + 4] = mo[0];
                byteList[readPointer + 5] = mo[1];
                byteList[readPointer + 8] = sa[0];
                byteList[readPointer + 9] = sa[1];
                byteList[readPointer + 12] = sb[0];
                byteList[readPointer + 13] = sb[1];
                byteList[readPointer + 16] = cc[0];
                byteList[readPointer + 17] = cc[1];
                byteList[readPointer + 20] = fc[0];
                byteList[readPointer + 21] = fc[1];
                byteList[readPointer + 24] = em[0];
                byteList[readPointer + 25] = em[1];
                byteList[readPointer + 28] = tx[0];
                byteList[readPointer + 29] = tx[1];
                /////////////////////////////////////////////////////////////////////objective
                int objectiveTypeInt = List.ObjectiveType.FirstOrDefault(x => x.Value == comQuestTypeM.Text).Key;
                byte[] objectiveTypeByte = BitConverter.GetBytes(objectiveTypeInt);
                byteList[48] = objectiveTypeByte[0];
                byteList[49] = objectiveTypeByte[1];
                byteList[50] = objectiveTypeByte[2];
                byteList[51] = objectiveTypeByte[3];

                int targetID;
                if (comQuestTypeM.SelectedIndex != 8)
                {
                    targetID = List.MonsterID.FirstOrDefault(x => x.Value == textTargetM.Text).Key;
                }
                else
                {
                    targetID = List.ItemID.FirstOrDefault(x => x.Value == textTargetM.Text).Key;
                }
                byte[] targetIDByte = BitConverter.GetBytes(targetID);
                byteList[52] = targetIDByte[0];
                byteList[53] = targetIDByte[1];

                int amount;
                if (comQuestTypeM.SelectedIndex != 4 || comQuestTypeM.SelectedIndex != 5)
                {
                    amount = (int)numQuantityM.Value;
                }
                else
                {
                    amount = (int)numQuantityM.Value / 100;
                }
                byte[] amountByte = BitConverter.GetBytes(amount);
                byteList[54] = amountByte[0];
                byteList[55] = amountByte[1];

                if (comQuestTypeA.SelectedIndex != 0)
                {
                    objectiveTypeInt = List.ObjectiveType.FirstOrDefault(x => x.Value == comQuestTypeA.Text).Key;
                    objectiveTypeByte = BitConverter.GetBytes(objectiveTypeInt);
                    byteList[56] = objectiveTypeByte[0];
                    byteList[57] = objectiveTypeByte[1];
                    byteList[58] = objectiveTypeByte[2];
                    byteList[59] = objectiveTypeByte[3];

                    if (comQuestTypeA.SelectedIndex != 8)
                    {
                        targetID = List.MonsterID.FirstOrDefault(x => x.Value == textTargetA.Text).Key;
                    }
                    else
                    {
                        targetID = List.ItemID.FirstOrDefault(x => x.Value == textTargetA.Text).Key;
                    }
                    targetIDByte = BitConverter.GetBytes(targetID);
                    byteList[60] = targetIDByte[0];
                    byteList[61] = targetIDByte[1];

                    if (comQuestTypeA.SelectedIndex != 4 || comQuestTypeA.SelectedIndex != 5)
                    {
                        amount = (int)numQuantityA.Value;
                    }
                    else
                    {
                        amount = (int)numQuantityA.Value / 100;
                    }
                    amountByte = BitConverter.GetBytes(amount);
                    byteList[62] = amountByte[0];
                    byteList[63] = amountByte[1];
                }

                if (comQuestTypeB.SelectedIndex != 0)
                {
                    objectiveTypeInt = List.ObjectiveType.FirstOrDefault(x => x.Value == comQuestTypeB.Text).Key;
                    objectiveTypeByte = BitConverter.GetBytes(objectiveTypeInt);
                    byteList[64] = objectiveTypeByte[0];
                    byteList[65] = objectiveTypeByte[1];
                    byteList[66] = objectiveTypeByte[2];
                    byteList[67] = objectiveTypeByte[3];

                    if (comQuestTypeB.SelectedIndex != 8)
                    {
                        targetID = List.MonsterID.FirstOrDefault(x => x.Value == textTargetB.Text).Key;
                    }
                    else
                    {
                        targetID = List.ItemID.FirstOrDefault(x => x.Value == textTargetB.Text).Key;
                    }
                    targetIDByte = BitConverter.GetBytes(targetID);
                    byteList[68] = targetIDByte[0];
                    byteList[69] = targetIDByte[1];

                    if (comQuestTypeB.SelectedIndex != 4 || comQuestTypeB.SelectedIndex != 5)
                    {
                        amount = (int)numQuantityB.Value;
                    }
                    else
                    {
                        amount = (int)numQuantityB.Value / 100;
                    }
                    amountByte = BitConverter.GetBytes(amount);
                    byteList[70] = amountByte[0];
                    byteList[71] = amountByte[1];
                }

                int iconID = List.MonsterID.FirstOrDefault(x => x.Value == textMonsterIcon1.Text).Key;
                byte[] iconIDByte = BitConverter.GetBytes(iconID);
                byteList[185] = iconIDByte[0];
                iconID = List.MonsterID.FirstOrDefault(x => x.Value == textMonsterIcon2.Text).Key;
                iconIDByte = BitConverter.GetBytes(iconID);
                byteList[186] = iconIDByte[0];
                iconID = List.MonsterID.FirstOrDefault(x => x.Value == textMonsterIcon3.Text).Key;
                iconIDByte = BitConverter.GetBytes(iconID);
                byteList[187] = iconIDByte[0];
                iconID = List.MonsterID.FirstOrDefault(x => x.Value == textMonsterIcon4.Text).Key;
                iconIDByte = BitConverter.GetBytes(iconID);
                byteList[188] = iconIDByte[0];
                iconID = List.MonsterID.FirstOrDefault(x => x.Value == textMonsterIcon5.Text).Key;
                iconIDByte = BitConverter.GetBytes(iconID);
                byteList[189] = iconIDByte[0];

                /////////////////////////////////////////////////////////////misc
                byte[] by;
                int id;

                byteList[4] = (byte)numDifficulty.Value;

                //byteList[151] = (byte)comHRGSRType.SelectedIndex;
                byteList[151] = (byte)numQuestType1.Value;
                by = BitConverter.GetBytes((int)numQuestType2.Value);
                byteList[152] = by[0];
                byteList[153] = by[1];

                int val = 0;
                switch (comSeason.SelectedIndex)
                {
                    case 0:
                        val = 10;
                        break;
                    case 1:
                        val = 18;
                        break;
                    case 2:
                        val = 12;
                        break;
                    case 3:
                        val = 20;
                        break;
                    case 4:
                        val = 9;
                        break;
                    case 5:
                        val = 17;
                        break;
                    case 6:
                        val = 74;
                        break;
                    case 7:
                        val = 82;
                        break;
                    case 8:
                        val = 76;
                        break;
                    case 9:
                        val = 84;
                        break;
                    case 10:
                        val = 73;
                        break;
                    case 11:
                        val = 81;
                        break;
                    case 12:
                        val = 0;
                        break;
                    case 13:
                        val = seaonVal;
                        break;
                }

                byteList[2] = (byte)val;

                by = BitConverter.GetBytes((int)numQuestID.Value);
                byteList[46] = by[0];
                byteList[47] = by[1];

                by = BitConverter.GetBytes((int)numFee.Value);
                byteList[12] = by[0];
                byteList[13] = by[1];
                byteList[14] = by[2];
                byteList[15] = by[3];

                by = BitConverter.GetBytes((int)numMainReward.Value);
                byteList[16] = by[0];
                byteList[17] = by[1];
                by = BitConverter.GetBytes((int)numSubAReward.Value);
                byteList[24] = by[0];
                byteList[25] = by[1];
                by = BitConverter.GetBytes((int)numSubBReward.Value);
                byteList[28] = by[0];
                byteList[29] = by[1];

                by = BitConverter.GetBytes((int)numMainPoint.Value);
                byteList[164] = by[0];
                byteList[165] = by[1];
                by = BitConverter.GetBytes((int)numSubAPoint.Value);
                byteList[168] = by[0];
                byteList[169] = by[1];
                by = BitConverter.GetBytes((int)numSubBPoint.Value);
                byteList[172] = by[0];
                byteList[173] = by[1];

                by = BitConverter.GetBytes((int)numReqMin.Value);
                byteList[74] = by[0];
                byteList[75] = by[1];
                by = BitConverter.GetBytes((int)numReqMax.Value);
                byteList[76] = by[0];
                byteList[77] = by[1];
                by = BitConverter.GetBytes((int)numReqHost.Value);
                byteList[78] = by[0];
                byteList[79] = by[1];

                id = List.ItemID.FirstOrDefault(x => x.Value == textItem1.Text).Key;
                by = BitConverter.GetBytes(id);
                byteList[176] = by[0];
                byteList[177] = by[1];
                id = List.ItemID.FirstOrDefault(x => x.Value == textItem2.Text).Key;
                by = BitConverter.GetBytes(id);
                byteList[178] = by[0];
                byteList[179] = by[1];
                id = List.ItemID.FirstOrDefault(x => x.Value == textItem3.Text).Key;
                by = BitConverter.GetBytes(id);
                byteList[180] = by[0];
                byteList[181] = by[1];

                if (comCourse.SelectedIndex == 27)
                {
                    by = BitConverter.GetBytes(courseVal);
                    byteList[6] = by[0];
                }
                else
                {
                    by = BitConverter.GetBytes(comCourse.SelectedIndex);
                    byteList[6] = by[0];
                }

                by = BitConverter.GetBytes((int)numTime.Value * 30 * 60);
                byteList[32] = by[0];
                byteList[33] = by[1];
                byteList[34] = by[2];
                byteList[35] = by[3];

                by = BitConverter.GetBytes(comRest.SelectedIndex);
                byteList[44] = by[0];

                //equiment
                by = BitConverter.GetBytes(List.HeadID.FirstOrDefault(x => x.Value == textHead1.Text).Key);
                byteList[92] = by[0];
                byteList[93] = by[1];
                by = BitConverter.GetBytes(List.ChestID.FirstOrDefault(x => x.Value == textChest1.Text).Key);
                byteList[116] = by[0];
                byteList[117] = by[1];
                by = BitConverter.GetBytes(List.ArmsID.FirstOrDefault(x => x.Value == textArms1.Text).Key);
                byteList[124] = by[0];
                byteList[125] = by[1];
                by = BitConverter.GetBytes(List.WaistID.FirstOrDefault(x => x.Value == textWaist1.Text).Key);
                byteList[132] = by[0];
                byteList[133] = by[1];
                by = BitConverter.GetBytes(List.LegsID.FirstOrDefault(x => x.Value == textLegs1.Text).Key);
                byteList[108] = by[0];
                byteList[109] = by[1];

                by = BitConverter.GetBytes(List.ItemID.FirstOrDefault(x => x.Value == textItemReqName.Text).Key);
                byteList[148] = by[0];
                byteList[149] = by[1];
                byteList[150] = (byte)numItemReqQua.Value;

                header[14] = BitConverter.GetBytes(byteList.Count)[1];
                header[15] = BitConverter.GetBytes(byteList.Count)[0];

                var by1 = new List<byte>();
                by1.AddRange(header);
                by1.AddRange(byteList);
                string result = BitConverter.ToString(by1.ToArray()).Replace("-", string.Empty);
                list[listQuest.SelectedIndex] = result;
                questDataDic[0] = list;

                string tTitleAndName = textTitle.Text.Replace("\r\n", "");
                int index = listQuest.SelectedIndex;
                listQuest.Items.RemoveAt(index);
                listQuest.Items.Insert(index, tTitleAndName);
                listQuest.SelectedIndex = index;

                ManageLogs($"Saved changes to the currently selected quest(No.{index + 1})");
            }
        }

        public void MoveUp()
        {
            MoveItem(-1);
        }

        public void MoveDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int direction)
        {
            //from StackOverFlow
            if (listQuest.SelectedItem == null || listQuest.SelectedIndex < 0)
                return;

            int newIndex = listQuest.SelectedIndex + direction;

            if (newIndex < 0 || newIndex >= listQuest.Items.Count)
                return;

            object selected = listQuest.SelectedItem;

            listQuest.Items.Remove(selected);
            listQuest.Items.Insert(newIndex, selected);
            listQuest.SetSelected(newIndex, true);
        }

        private void buttonDeleteAll_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to remove all quests from list?", "Message box", MessageBoxButtons.YesNo);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                listQuest.Items.Clear();
                List<string> list = questDataDic[0];
                list = new List<string>();
                questDataDic[0] = list;
                numQuestCount.Value = 0;
                ManageLogs("Removed all quests from list.");
            }
        }

        private void buttonHelp_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Show();
        }

        private void numMapID_ValueChanged(object sender, EventArgs e)
        {
            List.MapID.TryGetValue((int)numMapID.Value, out string map);
            comMap.Text = map;
        }

        private void comMonsterName_SelectedIndexChanged(object sender, EventArgs e)
        {
            numSearchMonsterID.Value = comMonsterName.SelectedIndex;
        }

        private void numSearchMonsterID_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numSearchMonsterID.Value, out string monsterName);
            comMonsterName.Text = monsterName;
        }

        private void comItemName_SelectedIndexChanged(object sender, EventArgs e)
        {
            numSearchItemID.Value = comItemName.SelectedIndex;
        }

        private void numSearchItemID_ValueChanged(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numSearchItemID.Value, out string itemName);
            comItemName.Text = itemName;
        }

        private void numTargetIDM_ValueChanged(object sender, EventArgs e)
        {
            if (comQuestTypeM.SelectedIndex != 8)
            {
                List.MonsterID.TryGetValue((int)numTargetIDM.Value, out string monsterName);
                textTargetM.Text = monsterName;
            }
            else
            {
                List.ItemID.TryGetValue((int)numTargetIDM.Value, out string itemName);
                textTargetM.Text = itemName;
            }
        }

        private void numTargetIDA_ValueChanged(object sender, EventArgs e)
        {
            if (comQuestTypeA.SelectedIndex != 8)
            {
                List.MonsterID.TryGetValue((int)numTargetIDA.Value, out string monsterName);
                textTargetA.Text = monsterName;
            }
            else
            {
                List.ItemID.TryGetValue((int)numTargetIDA.Value, out string itemName);
                textTargetA.Text = itemName;
            }
        }

        private void numTargetIDB_ValueChanged(object sender, EventArgs e)
        {
            if (comQuestTypeB.SelectedIndex != 8)
            {
                List.MonsterID.TryGetValue((int)numTargetIDB.Value, out string monsterName);
                textTargetB.Text = monsterName;
            }
            else
            {
                List.ItemID.TryGetValue((int)numTargetIDB.Value, out string itemName);
                textTargetB.Text = itemName;
            }
        }

        private void textTargetM_TextChanged(object sender, EventArgs e)
        {
            if (comQuestTypeM.SelectedIndex != 8)
            {
                numTargetIDM.Value = List.MonsterID.FirstOrDefault(x => x.Value == textTargetM.Text).Key;
            }
            else
            {
                numTargetIDM.Value = List.ItemID.FirstOrDefault(x => x.Value == textTargetM.Text).Key;
            }
        }

        private void textTargetA_TextChanged(object sender, EventArgs e)
        {
            if (comQuestTypeA.SelectedIndex != 8)
            {
                numTargetIDA.Value = List.MonsterID.FirstOrDefault(x => x.Value == textTargetA.Text).Key;
            }
            else
            {
                numTargetIDA.Value = List.ItemID.FirstOrDefault(x => x.Value == textTargetA.Text).Key;
            }
        }

        private void textTargetB_TextChanged(object sender, EventArgs e)
        {
            if (comQuestTypeB.SelectedIndex != 8)
            {
                numTargetIDB.Value = List.MonsterID.FirstOrDefault(x => x.Value == textTargetB.Text).Key;
            }
            else
            {
                numTargetIDB.Value = List.ItemID.FirstOrDefault(x => x.Value == textTargetB.Text).Key;
            }
        }

        private void numBR_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_ValueChanged_1(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numericUpDown1.Value, out string name);
            textItem1.Text = name;
        }

        private void numericUpDown2_ValueChanged_1(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numericUpDown2.Value, out string name);
            textItem2.Text = name;
        }

        private void numericUpDown3_ValueChanged_1(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numericUpDown3.Value, out string name);
            textItem3.Text = name;
        }

        private void numItemReqID_ValueChanged_1(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numItemReqID.Value, out string name);
            textItemReqName.Text = name;
        }

        private void numTargetIDM_ValueChanged_1(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numTargetIDM.Value, out string name);
            textTargetM.Text = name;
        }

        private void numTargetIDA_ValueChanged_1(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numTargetIDA.Value, out string name);
            textTargetA.Text = name;
        }

        private void numTargetIDB_ValueChanged_1(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numTargetIDB.Value, out string name);
            textTargetB.Text = name;
        }

        private void comMelee_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            numericUpDown8.Value = List.MeleeID.FirstOrDefault(x => x.Value == comMelee.Text).Key;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            List.MeleeID.TryGetValue((int)numericUpDown8.Value, out string name);
            comMelee.Text = name;
        }

        private void comRanged_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown31.Value = List.RangedID.FirstOrDefault(x => x.Value == comRanged.Text).Key;
        }

        private void numericUpDown31_ValueChanged(object sender, EventArgs e)
        {
            List.RangedID.TryGetValue((int)numericUpDown31.Value, out string name);
            comRanged.Text = name;
        }

        private void comHead_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown7.Value = List.HeadID.FirstOrDefault(x => x.Value == comHead.Text).Key;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            List.HeadID.TryGetValue((int)numericUpDown7.Value, out string name);
            comHead.Text = name;
        }

        private void comChset_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown6.Value = List.ChestID.FirstOrDefault(x => x.Value == comChset.Text).Key;
        }

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            List.ChestID.TryGetValue((int)numericUpDown6.Value, out string name);
            comChset.Text = name;
        }

        private void comArms_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown5.Value = List.ArmsID.FirstOrDefault(x => x.Value == comArms.Text).Key;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            List.ArmsID.TryGetValue((int)numericUpDown5.Value, out string name);
            comArms.Text = name;
        }

        private void comboWasit_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown4.Value = List.WaistID.FirstOrDefault(x => x.Value == comboWasit.Text).Key;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            List.WaistID.TryGetValue((int)numericUpDown4.Value, out string name);
            comboWasit.Text = name;
        }

        private void comLegs_SelectedIndexChanged(object sender, EventArgs e)
        {
            numericUpDown30.Value = List.LegsID.FirstOrDefault(x => x.Value == comLegs.Text).Key;
        }

        private void numericUpDown30_ValueChanged(object sender, EventArgs e)
        {
            List.LegsID.TryGetValue((int)numericUpDown30.Value, out string name);
            comLegs.Text = name;
        }

        private void numWep1_ValueChanged(object sender, EventArgs e)
        {
            List.MeleeID.TryGetValue((int)numWep1.Value, out string name);
            textWep.Text = name;
        }

        private void numHead1_ValueChanged(object sender, EventArgs e)
        {
            List.HeadID.TryGetValue((int)numHead1.Value, out string name);
            textHead1.Text = name;
        }

        private void numChest1_ValueChanged(object sender, EventArgs e)
        {
            List.ChestID.TryGetValue((int)numChest1.Value, out string name);
            textChest1.Text = name;
        }

        private void numArms1_ValueChanged(object sender, EventArgs e)
        {
            List.ArmsID.TryGetValue((int)numArms1.Value, out string name);
            textArms1.Text = name;
        }

        private void numWaist1_ValueChanged(object sender, EventArgs e)
        {
            List.WaistID.TryGetValue((int)numWaist1.Value, out string name);
            textWaist1.Text = name;
        }

        private void numLegs1_ValueChanged(object sender, EventArgs e)
        {
            List.LegsID.TryGetValue((int)numLegs1.Value, out string name);
            textLegs1.Text = name;
        }

        private void comMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            numMapID.Value = List.MapID.FirstOrDefault(x => x.Value == comMap.Text).Key;
        }

        private void comMonsterName_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            numSearchMonsterID.Value = comMonsterName.SelectedIndex;
        }

        private void numSearchMonsterID_ValueChanged_1(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numSearchMonsterID.Value, out string name);
            comMonsterName.Text = name;
        }

        private void comItemName_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            numSearchItemID.Value = List.ItemID.FirstOrDefault(x => x.Value == comItemName.Text).Key;
        }

        private void numSearchItemID_ValueChanged_1(object sender, EventArgs e)
        {
            List.ItemID.TryGetValue((int)numSearchItemID.Value, out string name);
            comItemName.Text = name;
        }

        private void numMonsterIcon1_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numMonsterIcon1.Value, out string name);
            textMonsterIcon1.Text = name;
        }

        private void numMonsterIcon2_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numMonsterIcon2.Value, out string name);
            textMonsterIcon2.Text = name;
        }

        private void numMonsterIcon3_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numMonsterIcon3.Value, out string name);
            textMonsterIcon3.Text = name;
        }

        private void numMonsterIcon4_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numMonsterIcon4.Value, out string name);
            textMonsterIcon4.Text = name;
        }

        private void numMonsterIcon5_ValueChanged(object sender, EventArgs e)
        {
            List.MonsterID.TryGetValue((int)numMonsterIcon5.Value, out string name);
            textMonsterIcon5.Text = name;
        }

        private void buttonCreateDatabase_Click(object sender, EventArgs e)
        {
            if (true)   //!radioButtonDatabase.Checked
            {
                DialogResult result = MessageBox.Show("Do you want to select all files? (e.g. 22051d0, 22051d1, 22051d2...) \n\r See Instruction for details.", "Message Box",MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Yes)
                {
                    DialogResult folderBrowser = folderBrowserDialog1.ShowDialog();
                    if (folderBrowser == DialogResult.OK)
                    {
                        //MessageBox.Show("Start to creating database. It takes few minutes.");
                        string[] fileNamesArray = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.bin");
                        var list = new List<byte>();
                        byte[] fileHeader = { 00, 00, 00, 00 };
                        byte[] devide = { 00, 00 };
                        int count = 0;
                        bool suc = false;

                        list.AddRange(fileHeader);
                        for (int i = 0; i < fileNamesArray.Length; i++) //fileNamesArray.Length
                        {
                            string fileName = fileNamesArray[i];
                            byte[] by = by = File.ReadAllBytes(fileNamesArray[i]);
                            if (by[0] == 192)
                            {
                                count = count + 1;
                                var by1 = new List<byte>();
                                byte[] data = by.Skip(192).Take(320).ToArray();
                                by1.AddRange(data);     //140

                                int textPointer = BitConverter.ToInt16(by, 48);     //AE4
                                int textPointer1 = BitConverter.ToInt16(by, textPointer);
                                textPointer1 = textPointer1 - 32;
                                byte[] pointerData = by.Skip(textPointer1).Take(32).ToArray();
                                by1.AddRange(pointerData);  //160
                                int leng = by1.Count;

                                by1[40] = 64;
                                by1[41] = 1;

                                int pointer = BitConverter.ToInt16(by, textPointer + 4);
                                int val = by[pointer];
                                if (val == 0)
                                {
                                    pointer = pointer + 1;
                                }

                                int pTitleAndName = BitConverter.ToInt16(by, textPointer1);
                                int pMainoObj = BitConverter.ToInt16(by, textPointer1 + 4);
                                int pAObj = BitConverter.ToInt16(by, textPointer1 + 8);
                                int pBObj = BitConverter.ToInt16(by, textPointer1 + 12);
                                int pClearC = BitConverter.ToInt16(by, textPointer1 + 16);
                                int pFailC = BitConverter.ToInt16(by, textPointer1 + 20);
                                int pEmp = BitConverter.ToInt16(by, textPointer1 + 24);
                                int pText = BitConverter.ToInt16(by, textPointer1 + 28);

                                int a1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray().Length;
                                int a2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray().Length;
                                int a3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray().Length;
                                int a4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray().Length;
                                int a5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray().Length;
                                int a6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray().Length;
                                int a7 = by.Skip(pEmp).Take(pText - pEmp).ToArray().Length;
                                int a8 = by.Skip(pText).Take(by.Length - pText).ToArray().Length;

                                byte[] b1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray();
                                byte[] b2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray();
                                byte[] b3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray();
                                byte[] b4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray();
                                byte[] b5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray();
                                byte[] b6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray();
                                byte[] b7 = by.Skip(pEmp).Take(pText - pEmp).ToArray();
                                byte[] b8 = by.Skip(pText).Take(by.Length - pText).ToArray();
                                by1.AddRange(b1);
                                by1.AddRange(b2);
                                by1.AddRange(b3);
                                by1.AddRange(b4);
                                by1.AddRange(b5);
                                by1.AddRange(b6);
                                by1.AddRange(b7);
                                by1.AddRange(b8);

                                leng = leng - 32;
                                int num = leng + 32;
                                byte[] c1 = BitConverter.GetBytes(num);    //01,60
                                by1[leng] = c1[0];
                                by1[leng + 1] = c1[1];

                                num = num + a1;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 4] = c1[0];
                                by1[leng + 5] = c1[1];

                                num = num + a2;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 8] = c1[0];
                                by1[leng + 9] = c1[1];

                                num = num + a3;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 12] = c1[0];
                                by1[leng + 13] = c1[1];

                                num = num + a4;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 16] = c1[0];
                                by1[leng + 17] = c1[1];

                                num = num + a5;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 20] = c1[0];
                                by1[leng + 21] = c1[1];

                                num = num + a6;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 24] = c1[0];
                                by1[leng + 25] = c1[1];

                                num = num + a7;
                                c1 = BitConverter.GetBytes(num);
                                by1[leng + 28] = c1[0];
                                by1[leng + 29] = c1[1];

                                leng = by1.Count;
                                byte[] b = BitConverter.GetBytes(leng); //2C8
                                byte[] questHeader = { 00, 00, 15, 04, 18, 01, 00, 00, 00, 00, 00, 00, 00, 00, 255, 255 };
                                questHeader[14] = b[1];
                                questHeader[15] = b[0];
                                list.AddRange(questHeader);

                                list.AddRange(by1);
                                list.AddRange(devide);
                                suc = true;
                            }
                            else
                            {
                                MessageBox.Show("It appears that the quest file is not included or is formatted differently.");
                                break;
                            }
                        }
                        if (suc)
                        {
                            listDataabse.Items.Clear();
                            byte[] amount = BitConverter.GetBytes(count);
                            dbCount = count;
                            list[0] = amount[0];
                            list[1] = amount[1];
                            File.WriteAllBytes("data/database.bin", list.ToArray());
                            ManageLogs("Database has created.");
                            DialogResult re = MessageBox.Show("Database has created. Do you want to load it now?", "Message box", MessageBoxButtons.YesNo);
                            if (re == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (radioButtonDatabase.Checked)
                                {
                                    radioButtonDatabase.Checked = false;
                                    radioButtonDatabase.Checked = true;
                                }
                                else
                                {
                                    radioButtonDatabase.Checked = true;
                                }
                            }
                        }
                    }
                }
                else if (result == DialogResult.No)
                {
                    DialogResult folderBrowser = folderBrowserDialog1.ShowDialog();
                    if (folderBrowser == DialogResult.OK)
                    {
                        //MessageBox.Show("Creating database. It takes few minutes.");
                        string[] fileNamesArray = Directory.GetFiles(folderBrowserDialog1.SelectedPath, "*.bin");
                        var listFileName = Directory.GetFiles(folderBrowserDialog1.SelectedPath).Select(Path.GetFileName).ToArray();
                        var list = new List<byte>();
                        byte[] fileHeader = { 00, 00, 00, 00 };
                        byte[] devide = { 00, 00 };
                        string prevName = "none";
                        int count = 0;
                        bool suc = false;

                        list.AddRange(fileHeader);
                        for (int i = 0; i < fileNamesArray.Length; i++) //fileNamesArray.Length
                        {
                            string fileName = listFileName[i];
                            char[] del = { 'd', 'n' };
                            string[] fileName2 = fileName.Split(del);

                            if (prevName != fileName2[0])
                            {
                                byte[] by = by = File.ReadAllBytes(fileNamesArray[i]);
                                if (by[0] == 192)
                                {
                                    count = count + 1;
                                    var by1 = new List<byte>();
                                    byte[] data = by.Skip(192).Take(320).ToArray();
                                    by1.AddRange(data);     //140

                                    int textPointer = BitConverter.ToInt16(by, 48);     //AE4
                                    int textPointer1 = BitConverter.ToInt16(by, textPointer);
                                    textPointer1 = textPointer1 - 32;
                                    byte[] pointerData = by.Skip(textPointer1).Take(32).ToArray();
                                    by1.AddRange(pointerData);  //160
                                    int leng = by1.Count;

                                    by1[40] = 64;
                                    by1[41] = 1;

                                    int pointer = BitConverter.ToInt16(by, textPointer + 4);
                                    int val = by[pointer];
                                    if (val == 0)
                                    {
                                        pointer = pointer + 1;
                                    }

                                    int pTitleAndName = BitConverter.ToInt16(by, textPointer1);
                                    int pMainoObj = BitConverter.ToInt16(by, textPointer1 + 4);
                                    int pAObj = BitConverter.ToInt16(by, textPointer1 + 8);
                                    int pBObj = BitConverter.ToInt16(by, textPointer1 + 12);
                                    int pClearC = BitConverter.ToInt16(by, textPointer1 + 16);
                                    int pFailC = BitConverter.ToInt16(by, textPointer1 + 20);
                                    int pEmp = BitConverter.ToInt16(by, textPointer1 + 24);
                                    int pText = BitConverter.ToInt16(by, textPointer1 + 28);

                                    int a1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray().Length;
                                    int a2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray().Length;
                                    int a3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray().Length;
                                    int a4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray().Length;
                                    int a5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray().Length;
                                    int a6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray().Length;
                                    int a7 = by.Skip(pEmp).Take(pText - pEmp).ToArray().Length;
                                    int a8 = by.Skip(pText).Take(by.Length - pText).ToArray().Length;

                                    byte[] b1 = by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray();
                                    byte[] b2 = by.Skip(pMainoObj).Take(pBObj - pMainoObj).ToArray();
                                    byte[] b3 = by.Skip(pAObj).Take(pBObj - pAObj).ToArray();
                                    byte[] b4 = by.Skip(pBObj).Take(pClearC - pBObj).ToArray();
                                    byte[] b5 = by.Skip(pClearC).Take(pFailC - pClearC).ToArray();
                                    byte[] b6 = by.Skip(pFailC).Take(pEmp - pFailC).ToArray();
                                    byte[] b7 = by.Skip(pEmp).Take(pText - pEmp).ToArray();
                                    byte[] b8 = by.Skip(pText).Take(by.Length - pText).ToArray();
                                    by1.AddRange(b1);
                                    by1.AddRange(b2);
                                    by1.AddRange(b3);
                                    by1.AddRange(b4);
                                    by1.AddRange(b5);
                                    by1.AddRange(b6);
                                    by1.AddRange(b7);
                                    by1.AddRange(b8);

                                    leng = leng - 32;
                                    int num = leng + 32;
                                    byte[] c1 = BitConverter.GetBytes(num);    //01,60
                                    by1[leng] = c1[0];
                                    by1[leng + 1] = c1[1];

                                    num = num + a1;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 4] = c1[0];
                                    by1[leng + 5] = c1[1];

                                    num = num + a2;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 8] = c1[0];
                                    by1[leng + 9] = c1[1];

                                    num = num + a3;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 12] = c1[0];
                                    by1[leng + 13] = c1[1];

                                    num = num + a4;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 16] = c1[0];
                                    by1[leng + 17] = c1[1];

                                    num = num + a5;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 20] = c1[0];
                                    by1[leng + 21] = c1[1];

                                    num = num + a6;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 24] = c1[0];
                                    by1[leng + 25] = c1[1];

                                    num = num + a7;
                                    c1 = BitConverter.GetBytes(num);
                                    by1[leng + 28] = c1[0];
                                    by1[leng + 29] = c1[1];

                                    leng = by1.Count;
                                    byte[] b = BitConverter.GetBytes(leng); //2C8
                                    byte[] questHeader = { 00, 00, 15, 04, 18, 01, 00, 00, 00, 00, 00, 00, 00, 00, 255, 255 };
                                    questHeader[14] = b[1];
                                    questHeader[15] = b[0];
                                    list.AddRange(questHeader);

                                    list.AddRange(by1);
                                    list.AddRange(devide);
                                    suc = true;
                                }
                                else
                                {
                                    MessageBox.Show("It appears that the quest file is not included or is formatted differently.");
                                    break;
                                }
                            }
                            prevName = fileName2[0];
                        }
                        if (suc)
                        {
                            listDataabse.Items.Clear();
                            byte[] amount = BitConverter.GetBytes(count);
                            dbCount = count;
                            list[0] = amount[0];
                            list[1] = amount[1];
                            File.WriteAllBytes("data/database.bin", list.ToArray());

                            DialogResult re = MessageBox.Show("Database has created. Do you want to load it now?", "Message box", MessageBoxButtons.YesNo);
                            if (re == System.Windows.Forms.DialogResult.Yes)
                            {
                                if (radioButtonDatabase.Checked)
                                {
                                    radioButtonDatabase.Checked = false;
                                    radioButtonDatabase.Checked = true;
                                }
                                else
                                {
                                    radioButtonDatabase.Checked = true;
                                }
                            }
                        }
                    }
                }
                else if (result == DialogResult.Cancel)
                {

                }
            }
        }

        private void buttonAddToList_Click(object sender, EventArgs e)
        {
            if (listDataabse.SelectedIndex != -1 && listDataabse.Items.Count != 0)
            {
                string str = dbByte[selIndex];
                List<string> list = questDataDic[0];
                list.Add(str);
                questDataDic[0] = list;
                numQuestCount.Value = numQuestCount.Value + 1;
                listQuest.Items.Add(dbNames[selIndex]);
                ManageLogs("New quest have been added.");
            }
        }

        private void listDataabse_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listDataabse.Items.Count != 0 && listDataabse.SelectedIndex != -1)
            {
                //selIndex = 0;
                List<int> db = indexDic[0];

                if (String.IsNullOrEmpty(textBoxSearchBox.Text))
                {
                    selIndex = listDataabse.SelectedIndex;
                }
                else
                {
                    if (db.Count > listDataabse.SelectedIndex)
                    {
                        selIndex = db[listDataabse.SelectedIndex];

                    }
                }

                //List<string> list = questDataDic[1];
                string text = dbByte[selIndex];
                var by = new List<byte>();
                for (int i = 0; i < text.Length / 2; i++)
                {
                    by.Add(Convert.ToByte(text.Substring(i * 2, 2), 16));
                }

                if (by.Count != 0)
                {
                    comMark.SelectedIndex = by[11];
                    comQuestType.SelectedIndex = by[4];
                    numPlayers.Value = by[3];

                    by.RemoveRange(0, 16);
                    byte[] byteData = by.ToArray();     //新しいbyte[]にしているが特に意味はないと思われる

                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 48), out string targetOM);
                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 56), out string targetOA);
                    List.ObjectiveType.TryGetValue(BitConverter.ToInt32(byteData, 64), out string targetOB);
                    comQuestTypeM.Text = targetOM;
                    comQuestTypeA.Text = targetOA;
                    comQuestTypeB.Text = targetOB;
                    int targetM = BitConverter.ToUInt16(byteData, 52);
                    int targetA = BitConverter.ToUInt16(byteData, 60);
                    int targetB = BitConverter.ToUInt16(byteData, 68);
                    if (targetOM != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetM, out string targetIDM);
                        textTargetM.Text = targetIDM;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetM, out string targetIDM);
                        textTargetM.Text = targetIDM;
                    }

                    if (targetOA != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetA, out string targetIDA);
                        textTargetA.Text = targetIDA;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetA, out string targetIDA);
                        textTargetA.Text = targetIDA;
                    }

                    if (targetOB != "Deliver")
                    {
                        List.MonsterID.TryGetValue(targetB, out string targetIDB);
                        textTargetB.Text = targetIDB;
                    }
                    else
                    {
                        List.ItemID.TryGetValue(targetB, out string targetIDB);
                        textTargetB.Text = targetIDB;
                    }

                    if (comQuestTypeM.SelectedIndex == 4 || comQuestTypeM.SelectedIndex == 5)
                    {
                        numQuantityM.Value = BitConverter.ToUInt16(byteData, 54) * 100;
                    }
                    else
                    {
                        numQuantityM.Value = BitConverter.ToUInt16(byteData, 54);
                    }

                    if (comQuestTypeA.SelectedIndex == 4 || comQuestTypeA.SelectedIndex == 5)
                    {
                        numQuantityA.Value = BitConverter.ToUInt16(byteData, 62) * 100;
                    }
                    else
                    {
                        numQuantityA.Value = BitConverter.ToUInt16(byteData, 62);
                    }

                    if (comQuestTypeB.SelectedIndex == 4 || comQuestTypeB.SelectedIndex == 5)
                    {
                        numQuantityB.Value = BitConverter.ToUInt16(byteData, 70) * 100;
                    }
                    else
                    {
                        numQuantityB.Value = BitConverter.ToUInt16(byteData, 70);
                    }

                    numMonsterIcon1.Value = byteData[185];
                    numMonsterIcon2.Value = byteData[186];
                    numMonsterIcon3.Value = byteData[187];
                    numMonsterIcon4.Value = byteData[188];
                    numMonsterIcon5.Value = byteData[189];

                    //List.MonsterID.TryGetValue(byteData[185], out string Icon1);
                    //List.MonsterID.TryGetValue(byteData[186], out string Icon2);
                    //List.MonsterID.TryGetValue(byteData[187], out string Icon3);
                    //List.MonsterID.TryGetValue(byteData[188], out string Icon4);
                    //List.MonsterID.TryGetValue(byteData[189], out string Icon5);
                    //textMonsterIcon1.Text = Icon1;
                    //textMonsterIcon2.Text = Icon2;
                    //textMonsterIcon3.Text = Icon3;
                    //textMonsterIcon4.Text = Icon4;
                    //textMonsterIcon5.Text = Icon5;

                    //Text
                    int pTitleAndName = BitConverter.ToInt16(byteData, 320);
                    int pMainoObj = BitConverter.ToInt16(byteData, 324);
                    int pAObj = BitConverter.ToInt16(byteData, 328);
                    int pBObj = BitConverter.ToInt16(byteData, 332);
                    int pClearC = BitConverter.ToInt16(byteData, 336);
                    int pFailC = BitConverter.ToInt16(byteData, 340);
                    int pEmp = BitConverter.ToInt16(byteData, 344);
                    int pText = BitConverter.ToInt16(byteData, 348);

                    string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
                    textTitle.Text = tTitleAndName;
                    string tMainObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pMainoObj).Take(pAObj - pMainoObj).ToArray()).Replace("\n", "\r\n");
                    textMain.Text = tMainObj;

                    if (pAObj == pBObj)
                    {
                        string tAObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pAObj).Take(pClearC - pAObj).ToArray()).Replace("\n", "\r\n");
                        textA.Text = tAObj;
                        textB.Text = tAObj;
                    }
                    else
                    {
                        string tAObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pAObj).Take(pBObj - pAObj).ToArray()).Replace("\n", "\r\n");
                        textA.Text = tAObj;
                        string tBObj = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pBObj).Take(pClearC - pBObj).ToArray()).Replace("\n", "\r\n");
                        textB.Text = tBObj;
                    }

                    string tClearC = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pClearC).Take(pFailC - pClearC).ToArray()).Replace("\n", "\r\n");
                    textClear.Text = tClearC;
                    string tFailC = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pFailC).Take(pEmp - pFailC).ToArray()).Replace("\n", "\r\n");
                    textFail.Text = tFailC;
                    string tEmp = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pEmp).Take(pText - pEmp).ToArray()).Replace("\n", "\r\n");
                    textEmp.Text = tEmp;
                    string tText = Encoding.GetEncoding("Shift_JIS").GetString(byteData.Skip(pText).Take(byteData.Length - pText).ToArray()).Replace("\n", "\r\n");
                    textText.Text = tText;

                    //Misc
                    numDifficulty.Value = byteData[4];
                    numQuestID.Value = BitConverter.ToUInt16(byteData, 46);
                    numFee.Value = BitConverter.ToUInt32(byteData, 12);
                    numMainReward.Value = BitConverter.ToUInt32(byteData, 16);
                    numSubAReward.Value = BitConverter.ToUInt32(byteData, 24);
                    numSubBReward.Value = BitConverter.ToUInt32(byteData, 28);
                    numMainPoint.Value = BitConverter.ToUInt32(byteData, 164);
                    numSubAPoint.Value = BitConverter.ToUInt32(byteData, 168);
                    numSubBPoint.Value = BitConverter.ToUInt32(byteData, 172);
                    numReqMin.Value = BitConverter.ToUInt16(byteData, 74);
                    numReqMax.Value = BitConverter.ToUInt16(byteData, 76);
                    numReqHost.Value = BitConverter.ToUInt16(byteData, 78);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 176), out string Item1);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 178), out string Item2);
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 180), out string Item3);
                    textItem1.Text = Item1;
                    textItem2.Text = Item2;
                    textItem3.Text = Item3;
                    if (byteData[6] > 22)
                    {
                        comCourse.SelectedIndex = 27;
                    }
                    else
                    {
                        comCourse.SelectedIndex = byteData[6];
                    }
                    numTime.Value = BitConverter.ToUInt32(byteData, 32) / 30 / 60;
                    List.MapID.TryGetValue(BitConverter.ToUInt16(byteData, 36), out string map);
                    comMap.Text = map;
                    comRest.SelectedIndex = byteData[44];

                    if (byteData[151] > 33)
                    {
                        comHRGSRType.SelectedIndex = 33;
                    }
                    else
                    {
                        if (byteData[151] > 8)
                        {
                            if (byteData[152] != 0)
                            {
                                comHRGSRType.SelectedIndex = 10;
                            }
                            else
                            {
                                comHRGSRType.SelectedIndex = byteData[151];
                            }
                        }
                        else
                        {
                            comHRGSRType.SelectedIndex = byteData[151];
                        }
                    }

                    numQuestType1.Value = byteData[151];
                    numQuestType2.Value = BitConverter.ToUInt16(byteData, 152);

                    int num = 0;
                    switch (byteData[2])
                    {
                        case 10:
                            num = 0;
                            break;
                        case 18:
                            num = 1;
                            break;
                        case 12:
                            num = 2;
                            break;
                        case 20:
                            num = 3;
                            break;
                        case 9:
                            num = 4;
                            break;
                        case 17:
                            num = 5;
                            break;

                        case 74:
                            num = 6;
                            break;
                        case 82:
                            num = 7;
                            break;
                        case 76:
                            num = 8;
                            break;
                        case 84:
                            num = 9;
                            break;
                        case 73:
                            num = 10;
                            break;
                        case 81:
                            num = 11;
                            break;
                        case 0:
                            num = 12;
                            break;

                        default:
                            num = 13;
                            seaonVal = byteData[2];
                            break;
                    }
                    comSeason.SelectedIndex = num;

                    //Equipment
                    List.LegsID.TryGetValue(BitConverter.ToUInt16(byteData, 92), out string legs1);
                    textLegs1.Text = legs1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 94), out string legs2);
                    textLegs2.Text = legs2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 96), out string legs3);
                    textLegs3.Text = legs3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 98), out string legs4);
                    textLegs4.Text = legs4;
                    numLegs1.Value = BitConverter.ToUInt16(byteData, 92);

                    List.MeleeID.TryGetValue(BitConverter.ToUInt16(byteData, 100), out string melee1);
                    textWep.Text = melee1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 102), out string melee2);
                    textWep2.Text = melee2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 104), out string melee3);
                    textWep3.Text = melee3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 106), out string melee4);
                    textWep4.Text = melee4;
                    numWep1.Value = BitConverter.ToUInt16(byteData, 100);

                    List.HeadID.TryGetValue(BitConverter.ToUInt16(byteData, 108), out string head1);
                    textHead1.Text = head1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 110), out string head2);
                    textHead2.Text = head2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 112), out string head3);
                    textHead3.Text = head3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 114), out string head4);
                    textHead4.Text = head4;
                    numHead1.Value = BitConverter.ToUInt16(byteData, 108);

                    List.ChestID.TryGetValue(BitConverter.ToUInt16(byteData, 116), out string chest1);
                    textChest1.Text = chest1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 118), out string chest2);
                    textChest2.Text = chest2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 120), out string chest3);
                    textChest3.Text = chest3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 122), out string chest4);
                    textChest4.Text = chest4;
                    numChest1.Value = BitConverter.ToUInt16(byteData, 116);

                    List.ArmsID.TryGetValue(BitConverter.ToUInt16(byteData, 124), out string arms1);
                    textArms1.Text = arms1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 126), out string arms2);
                    textArms2.Text = arms2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 128), out string arms3);
                    textArms3.Text = arms3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 130), out string arms4);
                    textArms4.Text = arms4;
                    numArms1.Value = BitConverter.ToUInt16(byteData, 124);

                    List.WaistID.TryGetValue(BitConverter.ToUInt16(byteData, 132), out string waist1);
                    textWaist1.Text = waist1;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 134), out string waist2);
                    textWaist2.Text = waist2;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 136), out string waist3);
                    textWaist3.Text = waist3;
                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 138), out string waist4);
                    textWaist4.Text = waist4;
                    numWaist1.Value = BitConverter.ToUInt16(byteData, 132);

                    List.ItemID.TryGetValue(BitConverter.ToUInt16(byteData, 148), out string item);
                    textItemReqName.Text = item;
                    numItemReqID.Value = BitConverter.ToUInt16(byteData, 148);
                    numItemReqQua.Value = byteData[150];
                }
            }

        }

        private void radioButtonDatabase_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDatabase.Checked)
            {
                string path = Directory.GetCurrentDirectory();
                path = path + "/data";
                string[] fileNamesArray = Directory.GetFiles(path).Select(Path.GetFileName).ToArray();

                if (fileNamesArray.Contains("database.bin"))
                {
                    byte[] byteData = File.ReadAllBytes("data/database.bin");
                    int count = BitConverter.ToUInt16(byteData, 0);
                    dbCount = count;
                    int prevPointer = 4;
                    //List<string> db = questDataDic[1];
                    //List<string> name = questDataDic[0];
                    for (int i = 0; i < count; i++)
                    {
                        byte[] header = byteData.Skip(prevPointer).Take(16).ToArray();
                        int length = header[14] * 256 + header[15];
                        byte[] by = byteData.Skip(prevPointer + 16).Take(length).ToArray();
                        string str1 = BitConverter.ToString(header).Replace("-", string.Empty);
                        string str2 = BitConverter.ToString(by).Replace("-", string.Empty);
                        str1 = str1 + str2;
                        dbByte[i] = str1;
                        //db.Add(str1);
                        //questDataDic[1] = db;

                        int pTitleAndName = BitConverter.ToInt32(by, 320);
                        int pMainoObj = BitConverter.ToInt32(by, 324);
                        string tTitleAndName = Encoding.GetEncoding("Shift_JIS").GetString(by.Skip(pTitleAndName).Take(pMainoObj - pTitleAndName).ToArray()).Replace("\n", "\r\n");
                        dbNames[i] = tTitleAndName;
                        listDataabse.Items.Add(tTitleAndName);

                        if (i != count - 1)
                        {
                            int num1 = 0;
                            for (int t = 1; t < 250; t++)
                            {
                                int val = prevPointer + 16 + length + t;
                                if (val < byteData.Length)
                                {
                                    if (byteData[val] == 64 && byteData[val + 1] == 1 && byteData[val - 1] == 0)
                                    {
                                        num1 = val - 56;
                                        break;
                                    }
                                }
                            }
                            prevPointer = num1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    //ManageLogs("Database has loaded.");
                    //MessageBox.Show("Database has loaded.");
                    if (listDataabse.SelectedIndex == -1 && listDataabse.Items.Count != 0)    //インデックスがマイナス(未選択)かつ、個数が最低1以上の時
                    {
                        listDataabse.SelectedIndex = 0;
                    }
                }
                else
                {
                    radioButtonDatabase.Checked = false;
                    MessageBox.Show("Could not find `data/database.bin` file.");
                }
            }
        }

        //private void textBoxSearchBox_Leave(object sender, System.EventArgs e)
        //{

        //}

        private void textBoxSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            //F1キーが押されたか調べる
            if (e.KeyData == Keys.Enter)
            {
                if (listDataabse.SelectedIndex != -1 && listDataabse.Items.Count != 0)
                {
                    string str = dbByte[selIndex];
                    List<string> list = questDataDic[0];
                    list.Add(str);
                    questDataDic[0] = list;
                    numQuestCount.Value = numQuestCount.Value + 1;
                    listQuest.Items.Add(dbNames[selIndex]);
                    ManageLogs("New quest have been added.");
                }
            }
        }

        private void textBoxSearchBox_TextChanged(object sender, EventArgs e)
        {
            if (true)  //(listDataabse.SelectedIndex != -1 && listDataabse.Items.Count != 0)
            {
                if (!String.IsNullOrEmpty(textBoxSearchBox.Text))
                {
                    var strings = new List<string>();
                    string input = textBoxSearchBox.Text;
                    listDataabse.Items.Clear();
                    List<int> list2 = indexDic[0];
                    list2 = new List<int>();
                    for (int i = 0; i < dbCount; i++)
                    {
                        string str = dbNames[i];
                        if (str != null)
                        {
                            if (str.Contains(input))
                            {
                                //listDataabse.Items.Add(str);
                                strings.Add(str);

                                list2.Add(i);
                                indexDic[0] = list2;
                            }
                        }
                    }
                    foreach (string str in strings)
                    {
                        listDataabse.Items.Add(str);
                    }
                }
                else
                {
                    string input = textBoxSearchBox.Text;
                    ManageLogs(input);
                    listDataabse.Items.Clear();
                    //List<string> risutto = questDataDic[2];
                    for (int i = 0; i < dbCount; i++)
                    {
                        if (dbNames[i] != null)
                        {
                            listDataabse.Items.Add(dbNames[i]);
                        }

                    }
                }
            }
        }
    }

    public static class ExtensionMethods
    {
        public static void Swap<T>(this List<T> list, int index1, int index2)
        {
            T temp = list[index1];
            list[index1] = list[index2];
            list[index2] = temp;
        }
    }
}