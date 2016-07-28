using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace PES_BL_ML_EDITOR_2016
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class ListBoxItem
        {
            public string Text { get; set; }
            public string[] Tag { get; set; }

            public void MyObject(string text, string[] tag)
            {
                this.Text = text;
                this.Tag = tag;
            }
            public override string ToString()
            {
                return Text;
            }

           
        }

        public class ComboboxItem
        {
            public string Text { get; set; }
            public string[] Tag { get; set; }


            public override string ToString()
            {
                return Text;
            }
        }

        public class player_team_class{
            public int id { get; set; }
            public string name { get;set;}

            public  player_team_class(int i, string n)
            {
                id = i;
                name = n;             
            }
        }
        List<player_team_class> players_data = new List<player_team_class>();

        string file_id = Application.StartupPath + "\\id_no.txt";
        List<player_team_class> team_data = new List<player_team_class>();
        int total_sp = 0;


        List<string[]> sostavy = new List<string[]>();


        void delete_item(string id)
        {
            if (File.Exists(file_id))
            {
                string old_value = "";
                string[] massiveOfString = File.ReadAllLines(file_id);
                foreach (string g in massiveOfString)
                {
                    if (g != id) old_value += g + Environment.NewLine;
                }

                File.Delete(file_id);
                File.AppendAllText(file_id, old_value, stan);
            }
        }

        public string get_name(int id, List<player_team_class> list)
        {
            foreach (player_team_class i in list)
            {
                if (i.id == id) return i.name;
            }
            return "";
        }


        public string get_name_team(string id, List<string[]> list)
        {
            foreach (string[] i in list)
            {
                if (i[0] == id) return get_name(Convert.ToInt32(i[1]),team_data);
            }

            return "";
        }


        List<byte[]> edit_bin = new List<byte[]>();
        List<byte[]> ml_bin = new List<byte[]>();


        List<string[]> id_transfer = new List<string[]>();

        Encoding stan = Encoding.UTF8;


        public static string edit_file = "";


        string parse(byte[] str, int nach, int len)
        {
            MemoryStream ss = new MemoryStream(str);
            ss.Seek(nach, 0);
            BinaryReader bb = new BinaryReader(ss, stan);
            byte[] nam = bb.ReadBytes(len);
            string name = stan.GetString(nam);
            //return name.Replace("\0", "");
            return name;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            edit_bin.Clear();
            ml_bin.Clear();
            players_data.Clear();
            team_data.Clear();
            comboBox1.Items.Clear();
            comboBox2.Items.Clear();
            listBox3.Items.Clear();
            id_transfer.Clear();
            edit_file = "";
            sostavy.Clear();
            listBox4.Items.Clear();


            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                edit_bin=Crypter.decrypt(openFileDialog1.FileName);
                if (edit_bin.Count == 6)
                {
                    DialogResult result2 = openFileDialog2.ShowDialog();
                    ml_bin = Crypter.decrypt(openFileDialog2.FileName);

                    if (ml_bin.Count == 6)
                    {
                        edit_file = openFileDialog2.FileName;
                        this.Enabled = false;
                        total_sp = 0;

                        ////////////////////команды в ml
                        int intValue = 0;
                        MemoryStream op = new MemoryStream(ml_bin[0]);
                        BinaryReader cht = new BinaryReader(op, stan);
                        op.Seek(40, 0); ///начало команд

                        do
                        {

                            byte[] pl = cht.ReadBytes(1332); // размер блока с командой
                            intValue = BitConverter.ToInt32(pl, 0);
                            if (intValue != -1)
                            {
                                string tm = parse(pl, 4, 46);
                                player_team_class t = new player_team_class(intValue, tm);
                                team_data.Add(t);


                                string[] cmb = new string[] { intValue.ToString(), tm };
                                ComboboxItem itm = new ComboboxItem();
                                itm.Tag = new string[] { cmb[0] };
                                itm.Text = cmb[1];
                                comboBox1.Items.Add(itm);
                                comboBox2.Items.Add(itm);

                            };
                            if (intValue == -1) break;
                        } while (op.Position < 1133572); // конец команд
                        cht.Close();
                        op.Close();
                        //////////////////////имена в edit
                        int intValue2 = 0;
                        MemoryStream op2 = new MemoryStream(edit_bin[0]);
                        BinaryReader cht2 = new BinaryReader(op2, stan);
                        op2.Seek(76, 0); // начало имен


                        do
                        {

                            byte[] pl = cht2.ReadBytes(112); // размер блока с игроком
                            intValue2 = BitConverter.ToInt32(pl, 0);
                            if (intValue2 != 0)
                            {

                                player_team_class t = new player_team_class(intValue2, parse(pl, 50, 40));
                                players_data.Add(t);
                                
                            };
                            if (intValue2 == 0) break;
                        } while (op2.Position < 2800076); // конец игроков
                        cht2.Close();
                        op2.Close();
                        /////////////////


                        
                        add_all_players();


                        foreach (string l in id_no)
                        {
                            ListBoxItem lb = new ListBoxItem();
                            string[] tag = new string[] { l };
                            lb.Tag = tag;
                            lb.Text = get_name(Convert.ToInt32(l),players_data);
                            listBox4.Items.Add(lb);
                        }


                        Enabled = true;
                        button3.Enabled = true;
                        label6.Text = "Total teams: " + comboBox1.Items.Count;
                        label8.Text = "File: " + papki(edit_file)[0] +"/.../"+Path.GetFileName(edit_file);
                        label9.Text = "Total transfers: " + total_transfers() + " | "+total_sp;


                    }
                    else
                    {
                        MessageBox.Show("Error");
                    }
                }
                else
                {
                    MessageBox.Show("Error");
                }
            }

        }

        string[] papki(string put)
        {
            return put.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
        }


        void add_all_players()
        {
            int intValue2 = 0;
            MemoryStream op2 = new MemoryStream(ml_bin[0]);
            BinaryReader cht2 = new BinaryReader(op2, stan);
            List<int> tmp_i = new List<int>();
            op2.Seek(40, 0); // начало команд
            do
            {

                byte[] pl = cht2.ReadBytes(1332); // размер блока с командой
                intValue2 = BitConverter.ToInt32(pl,0);


                    MemoryStream tmp = new MemoryStream(pl);
                    BinaryReader tmpb = new BinaryReader(tmp, stan);
                    tmp.Seek(248, 0);  // переход к составу
                    int cnt = 0;
                    do
                    {
                        byte[] pla = tmpb.ReadBytes(8);
                        int iid = BitConverter.ToInt32(pla, 4); // доп ид
                        int iid_two = BitConverter.ToInt32(pla, 0); // осн ид

                        string[] tm = new string[] { iid.ToString(), intValue2.ToString() };
                        sostavy.Add(tm);

                        if (iid != 0 && !tmp_i.Contains(iid))
                        {

                            string[] nww = new string[] { iid.ToString(), get_name(iid,players_data), iid_two.ToString() };
                            ListBoxItem lb = new ListBoxItem();
                            lb.Text = nww[1];
                            lb.Tag = new string[] { nww[0], nww[2] };
                            listBox3.Items.Add(lb);
                            tmp_i.Add(iid);

                        }
                        cnt++;

                    } while (cnt < 32);
                    tmp.Close();
                    tmpb.Close();

            } while (intValue2 != -1); // конец команд
            cht2.Close();
            op2.Close();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox vm2 = (ComboBox)sender;
            ComboboxItem cm = new ComboboxItem() ;
            ListBox lm = new ListBox();
            GroupBox lbl = new GroupBox();
            string ff = "From";

            if (vm2.Name.Contains("1"))
            {
                cm = (ComboboxItem)comboBox1.SelectedItem;
                lm = listBox1;
                listBox1.Items.Clear();
                lbl = groupBox1;


            }
            else
            {
                cm = (ComboboxItem)comboBox2.SelectedItem;
                lm = listBox2;
                listBox2.Items.Clear();
                lbl = groupBox2;
                ff = "To";

            }
            int id_gl = Convert.ToInt32( cm.Tag[0]) ;
            //label9.Text = id_gl.ToString();

            ////////////////////составы
            int intValue2 = 0;
            MemoryStream op2 = new MemoryStream(ml_bin[0]);
            BinaryReader cht2 = new BinaryReader(op2, stan);
            op2.Seek(40, 0); // начало команд
            do
            {

                byte[] pl = cht2.ReadBytes(1332); // размер блока с командой
                intValue2 = BitConverter.ToInt32(pl, 0);

                if (intValue2 == id_gl)
                {
                    MemoryStream tmp = new MemoryStream(pl);
                    BinaryReader tmpb = new BinaryReader(tmp, stan);
                    tmp.Seek(248, 0);  // переход к составу
                    int cnt = 0;
                    do
                    {
                        byte[] pla = tmpb.ReadBytes(8); 
                        int iid = BitConverter.ToInt32(pla, 4); // доп ид
                        int iid_two = BitConverter.ToInt32(pla, 0); // осн ид

                        if (iid != 0)
                        {
                            string[] nww = new string[] { iid.ToString(), get_name(iid,players_data),iid_two.ToString()};
                            ListBoxItem lb = new ListBoxItem();
                            lb.Text = nww[1];
                            lb.Tag = new string[] { nww[0], nww[2] };
                            lm.Items.Add(lb);

                        }
                        cnt++;

                    } while (cnt < 32);
                    tmp.Close();
                    tmpb.Close();

                };
                if (intValue2 == 65535) break;
            } while (op2.Position < 8461152); // конец команд
            cht2.Close();
            op2.Close();
            lbl.Text = ff+" (" + lm.Items.Count + " / 32)";

            /////////////////
            
            ////////////////////трансферы
            if (vm2.Name.Contains("2"))
            {
                listView1.Items.Clear();
                int intValue3 = 0;
                MemoryStream op3 = new MemoryStream(ml_bin[0]);
                BinaryReader cht3 = new BinaryReader(op3, stan);
                op3.Seek(8321784, 0);  // начало трансферов


                do
                {

                    byte[] pl = cht3.ReadBytes(268); // размер блока с транчером одной команды
                    intValue3 = BitConverter.ToInt32(pl, 0);

                    if (intValue3 == id_gl)
                    {
                        textBox2.Text = id_gl.ToString();
                        MemoryStream tmp = new MemoryStream(pl);
                        BinaryReader tmpb = new BinaryReader(tmp, stan);
                        tmp.Seek(4, 0);
                        int cnt = 0;
                        do
                        {
                            byte[] pla = tmpb.ReadBytes(16); // один трансфер
                            int iid = BitConverter.ToInt32(pla, 8); // реальный ид

                            if (iid != 0)
                            {
                                string[] nww = new string[] { iid.ToString(), get_name(iid, players_data) };
                                ListViewItem ll = new ListViewItem(nww);
                                listView1.Items.Add(ll);

                            }
                            cnt++;

                        } while (cnt < 16);
                        tmp.Close();
                        tmpb.Close();
                        break;

                    }
                    else
                    {

                    }
                } while (op3.Position < 8461152); // конец трансферов
                cht3.Close();
                op3.Close();
            }

            /////////////////

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox gg = (ListBox)sender;
            ListBoxItem hj = (ListBoxItem)gg.SelectedItem;

            groupBox1.Text   =  "From (" +listBox1.Items.Count+ " / 32) | ID: " + hj.Tag[0];
        }


        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1 && listView1.CheckedItems.Count!=0)
            {
                ComboboxItem cm = (ComboboxItem)comboBox2.SelectedItem;
                int id_gl = Convert.ToInt32(cm.Tag[0]);
                foreach (ListViewItem hui in listView1.CheckedItems)
                {
                    int id_del =Convert.ToInt32(hui.SubItems[0].Text);
                    int intValue3 = 0;
                    MemoryStream op3 = new MemoryStream(ml_bin[0]);
                    BinaryReader cht3 = new BinaryReader(op3, stan);
                    BinaryWriter wr3 = new BinaryWriter(op3, stan);
                    op3.Seek(8321784, 0); // начало трансферов
                    int tmp_pos = -1;
                    do
                    {

                        byte[] pl = cht3.ReadBytes(268); // размер блока с трансферами
                        intValue3 = BitConverter.ToInt32(pl, 0);
                        if (intValue3 == id_gl)
                        {
                            MemoryStream tmp = new MemoryStream(pl);
                            BinaryReader tmpb = new BinaryReader(tmp, stan);
                            BinaryWriter tmpw = new BinaryWriter(tmp, stan);
                            tmp_pos = (int)op3.Position - 268;

                            tmp.Seek(4, 0);
                            int cnt = 0;
                            do
                            {
                                byte[] pla = tmpb.ReadBytes(16);

                                int iid = BitConverter.ToInt32(pla, 8);
                                if (iid == id_del)
                                {

                                    tmp.Seek(tmp.Position - 16, 0);
                                    tmpw.Write(6488064);
                                    tmpw.Write(65535);
                                    tmpw.Write(0);
                                    tmpw.Write(1664);
                                    listView1.Items.Remove(hui);
                                    delete_transfer(id_del.ToString());
                                    break;

                                }
                                cnt++;


                            } while (cnt < 16);
                            op3.Seek(tmp_pos, 0);
                            wr3.Write(tmp.ToArray());

                            tmp.Close();
                            tmpb.Close();
                            tmpw.Close();
                        }

                    } while (op3.Position < 8461152); // конец

                    cht3.Close();
                    op3.Close();
                    wr3.Close();



                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Really exit ?", "Exit",
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question);

            if (result == DialogResult.No)
            {
                e.Cancel = true;

            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Crypter.encrypt(edit_file, ml_bin);
            if (File.Exists(edit_file)) File.Copy(edit_file, Path.GetDirectoryName(edit_file) + "\\" + Path.GetFileName(edit_file) + ".bcp", true);
            MessageBox.Show("Save");
        }




       int total_transfers()
       {
           id_transfer.Clear();
           MemoryStream op3 = new MemoryStream(ml_bin[0]);
           BinaryReader cht3 = new BinaryReader(op3, stan);
           op3.Seek(8321784, 0); // начало трансферов
           do
           {

               byte[] pl = cht3.ReadBytes(268); // размер блока с трансферами
               int intValue3 = BitConverter.ToInt32(pl, 0);
               MemoryStream tmp = new MemoryStream(pl);
               BinaryReader tmpb = new BinaryReader(tmp, stan);


               tmp.Seek(4, 0);
               int cnt = 0;
               do
               {
                   byte[] pla = tmpb.ReadBytes(16);

                   int tmp_id = BitConverter.ToInt32(pla, 8);
                    if (tmp_id != 0)
                    {
                        id_transfer.Add(new string[] { tmp_id.ToString(), intValue3.ToString() });
                        if (id_no.Contains(tmp_id.ToString())) total_sp++;
                    }
   

                   cnt++;


               } while (cnt < 16);
      

               tmp.Close();
               tmpb.Close();

           } while (op3.Position < 8461152); // конец

           cht3.Close();
           op3.Close();
           return id_transfer.Count;
       }



       private void button5_Click(object sender, EventArgs e)
       {
           MemoryStream op3 = new MemoryStream(ml_bin[0]);
           BinaryReader cht3 = new BinaryReader(op3, stan);
           BinaryWriter wr3 = new BinaryWriter(op3, stan);
           op3.Seek(8321784, 0); // начало трансферов
           int tmp_pos = -1;
           do
           {

               byte[] pl = cht3.ReadBytes(268); // размер блока с трансферами
               int intValue3 = BitConverter.ToInt32(pl, 0);

                   MemoryStream tmp = new MemoryStream(pl);
                   BinaryReader tmpb = new BinaryReader(tmp, stan);
                   BinaryWriter tmpw = new BinaryWriter(tmp, stan);
                   tmp_pos = (int)op3.Position - 268;

                   tmp.Seek(4, 0);
                   int cnt = 0;
                   do
                   {
                       byte[] pla = tmpb.ReadBytes(16);

                       int iid = BitConverter.ToInt32(pla, 8);


                           tmp.Seek(tmp.Position - 16, 0);
                           tmpw.Write(6488064);
                           tmpw.Write(65535);
                           tmpw.Write(0);
                           tmpw.Write(1664);

                       cnt++;


                   } while (cnt < 16);
                   op3.Seek(tmp_pos, 0);
                   wr3.Write(tmp.ToArray());

                   tmp.Close();
                   tmpb.Close();
                   tmpw.Close();


           } while (op3.Position < 8461152); // конец

           cht3.Close();
           op3.Close();
           wr3.Close();
           id_transfer.Clear();
       }

       private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
       {
           foreach (ListViewItem bn in listView1.Items)
           {
               if (bn.Checked)
               {
                   bn.Checked = false;
               }
               else
               {
                   bn.Checked = true;
               }

           }
       }

       private void listBox1_MouseDown(object sender, MouseEventArgs e)
       {
           ListBox listbox = (ListBox)sender;
           ListBoxItem hj = (ListBoxItem)listbox.SelectedItem;
            if (hj != null)
            {

                groupBox1.Text = "From (" + listBox1.Items.Count + " / 32) | ID: " + hj.Tag[0];

                int indexOfItem = listbox.IndexFromPoint(e.X, e.Y);

                if (e.Button == MouseButtons.Left)
                {


                    if (indexOfItem >= 0 && indexOfItem < listbox.Items.Count)// check we clicked down on a string
                    {

                        listbox.DoDragDrop(listbox.Items[indexOfItem], DragDropEffects.Copy);

                    }
                }
            }
       }

       private void listView1_DragEnter(object sender, DragEventArgs e)
       {
           if (e.AllowedEffect == DragDropEffects.Copy)
           {
               e.Effect = DragDropEffects.Copy;
           }
       }

       int is_transfer(string id)
       {
           foreach (string[] bn in id_transfer)
           {
               if (bn[0] == id) return Convert.ToInt32(bn[1]);
           }
           return -1;

       }


       void delete_transfer(string id)
       {
           int n = 0;
           foreach (string[] bn in id_transfer.ToArray())
           {
               if (bn[0] == id) id_transfer.RemoveAt(n);

               n++;
           }
       }


        void delete_id_no(string id,List<string> list)
        {
            int n = 0;
            foreach (string bn in list)
            {
                if (bn == id)
                {
                    list.RemoveAt(n);
                    return;
                }

                n++;
            }
        }

        bool is_id_no(string id)
        {
            foreach (string bn in id_no)
            {
                if (bn == id) return true;
            }
            return false;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
       {
           if (comboBox2.SelectedIndex != -1)
           {
               if (listBox2.Items.Count + listView1.Items.Count < 32)
               {

                   object player = e.Data.GetData(typeof(ListBoxItem));


                   ComboboxItem cm = (ComboboxItem)comboBox2.SelectedItem;
                   int id_gl = Convert.ToInt32(cm.Tag[0]);
                   int intValue3 = 0;
                   MemoryStream op3 = new MemoryStream(ml_bin[0]);
                   BinaryReader cht3 = new BinaryReader(op3, stan);
                   BinaryWriter wr3 = new BinaryWriter(op3, stan);
                   op3.Seek(8321784, 0); // начало трансферов

                   ListBoxItem lbi = (ListBoxItem)player;
                   int id_igrok = Convert.ToInt32(lbi.Tag[0]); // осн ид
                   int id_igrok2 = Convert.ToInt32(lbi.Tag[1]); // доп ид
                   bool keepLooping = true;
                   int tmp_pos = -1;
                   do
                   {

                       byte[] pl = cht3.ReadBytes(268); // размер блока с трансферами
                       intValue3 = BitConverter.ToInt32(pl, 0);







                       ////////
                       if (intValue3 == id_gl)
                       {
                           int res = is_transfer(id_igrok.ToString());
                           if (res==-1)
                           {
                               listView1.AllowDrop = true;
                               MemoryStream tmp = new MemoryStream(pl);
                               BinaryReader tmpb = new BinaryReader(tmp, stan);
                               BinaryWriter tmpw = new BinaryWriter(tmp, stan);
                               tmp_pos = (int)op3.Position - 268;

                               tmp.Seek(4, 0);
                               int cnt = 0;
                               do
                               {
                                   byte[] pla = tmpb.ReadBytes(16); // один трансфер

                                   int iid = BitConverter.ToInt32(pla, 8); // осн ид
                                   if (iid == 0)
                                   {

                                       tmp.Seek(tmp.Position - 16, 0);
                                       tmpw.Write(0);
                                       tmpw.Write(id_igrok2);
                                       tmpw.Write(id_igrok);
                                       tmpw.Write(4211968);
                                       string[] hui = new string[] { id_igrok.ToString(), get_name(id_igrok, players_data) };
                                       ListViewItem jj = new ListViewItem(hui);
                                       listView1.Items.Add(jj);
                                       keepLooping = false;
                                       break;

                                   }
                                   cnt++;


                               } while (cnt < 16);
                               op3.Seek(tmp_pos, 0);
                               wr3.Write(tmp.ToArray());
                               tmp.Close();
                               tmpb.Close();
                               tmpw.Close();
                              
                               id_transfer.Add(new string[] { id_igrok.ToString(), id_gl.ToString() });
                               break;
                           }
                           else
                           {
                               MessageBox.Show("The player is already listed in the trasfer team:" + get_name(res,team_data));
                           }


                       }
                       else
                       {
                           if (!keepLooping) listView1.AllowDrop = false;
                       }
                       

                   } while (op3.Position < 8461152); //конец транферов


                   cht3.Close();
                   op3.Close();
                   wr3.Close();
               }
               else
               {
                   MessageBox.Show("The team is full");
               }
           }
       }

       private void label9_Click(object sender, EventArgs e)
       {
           label9.Text = "Total transfers: " + id_transfer.Count+" | "+total_sp;
       }

       private void textBox1_KeyUp(object sender, KeyEventArgs e)
       {
           FindAllOfMyString(textBox1.Text, listBox3);
       }

       private void FindAllOfMyString(string searchString, ListBox listbox)
       {
           int sel = 0;
           for (int i = 0; i < listbox.Items.Count; i++)
           {
               if (listbox.Items[i].ToString().ToLower().Contains(searchString.ToLower()))
               {
                   sel = i;
                   break;
               }
           }
           listbox.SetSelected(sel, true);
       }

        List<string> id_no = new List<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            using (var streamReader = File.OpenText(file_id))
            {
                string[] lines = streamReader.ReadToEnd().Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                   

                foreach (string s in lines) id_no.Add(s.ToString());

                Application.DoEvents();

            }
        }


        private void listBox3_MouseUp(object sender, MouseEventArgs e)
        {

            ListBox bn = (ListBox)sender;
            ListBoxItem lba = (ListBoxItem)bn.SelectedItem;
            string text = "";
            

            switch (e.Button)
            {
                case MouseButtons.Right:
                    {
                        if (is_id_no(lba.Tag[0]))
                        {
                            text = "Удалить";
                        }
                        else
                        {
                            text = "Добавить";
                        }
                        error = 0;
                        contextMenuStrip1.Items[0].Text = text;
                        contextMenuStrip1.Tag = lba.Tag;
                        contextMenuStrip1.ItemClicked += new ToolStripItemClickedEventHandler(this.contextMenuStrip1_ItemClicked);
                        contextMenuStrip1.Show(bn, e.Location);
                        break;

                    };
            }
        }

        int error = 0;


        

        private void contextMenuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            //try
            //{
                if (error == 0)
                {
                    ContextMenuStrip nn = (ContextMenuStrip)sender;
                    string[] q = (string[])nn.Tag;
                    if (is_id_no(q[0]))
                    {
                        delete_id_no(q[0], id_no);
                        delete_item(q[0]);
                        listBox4.Items.Clear();
                        foreach (string ll in id_no)
                        {
                            if (ll != q[0])
                            {
                            ListBoxItem lb = new ListBoxItem();
                            lb.Text = get_name(Convert.ToInt32( ll),players_data);
                            lb.Tag = new string[] { q[0] };
                            listBox4.Items.Add(lb);
                            }
                        }

                        
                    }
                    else
                    {
                        id_no.Add(q[0]);

                        ListBoxItem lb = new ListBoxItem();
                        lb.Text = get_name(Convert.ToInt32(q[0]),players_data) ;
                        lb.Tag = new string[] { q[0]};
                        listBox4.Items.Add(lb);

                        File.AppendAllText(file_id, q[0] + Environment.NewLine, stan);
                    }
                    error++;
                }     

            //}
           // catch (Exception q)
            //{
                //MessageBox.Show(q.Message);

            //}
        }

        private void button6_Click(object sender, EventArgs e)
        {
            MemoryStream op3 = new MemoryStream(ml_bin[0]);
            BinaryReader cht3 = new BinaryReader(op3, stan);
            BinaryWriter wr3 = new BinaryWriter(op3, stan);
            op3.Seek(8321784, 0); // начало трансферов
            int k = 0;
            int tmp_pos = -1;
            do
            {

                byte[] pl = cht3.ReadBytes(268); // размер блока с трансферами
                int intValue3 = BitConverter.ToInt32(pl, 0);

                MemoryStream tmp = new MemoryStream(pl);
                BinaryReader tmpb = new BinaryReader(tmp, stan);
                BinaryWriter tmpw = new BinaryWriter(tmp, stan);
                tmp_pos = (int)op3.Position - 268;

                tmp.Seek(4, 0);
                int cnt = 0;
                do
                {
                    byte[] pla = tmpb.ReadBytes(16);

                    int iid = BitConverter.ToInt32(pla, 8);

                    if (id_no.Contains(iid.ToString()))
                    {

                        tmp.Seek(tmp.Position - 16, 0);
                        tmpw.Write(6488064);
                        tmpw.Write(65535);
                        tmpw.Write(0);
                        tmpw.Write(1664);
                        delete_transfer(iid.ToString());
                        k++;
                        //break;

                    }
                    cnt++;


                } while (cnt < 16);
                op3.Seek(tmp_pos, 0);
                wr3.Write(tmp.ToArray());

                tmp.Close();
                tmpb.Close();
                tmpw.Close();


            } while (op3.Position < 8461152); // конец
            total_sp -=k;
            cht3.Close();
            op3.Close();
            wr3.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox4.SelectedItem != null)
            {
                ListBoxItem ll = (ListBoxItem)listBox4.SelectedItem;

                string id = ll.Tag[0];

                delete_id_no(id, id_no);
                delete_item(id);
                listBox4.Items.Clear();
                foreach (string lql in id_no)
                {
                    if (lql != id)
                    {
                        ListBoxItem lb = new ListBoxItem();
                        lb.Text = get_name(Convert.ToInt32(lql), players_data);
                        lb.Tag = new string[] { lql };
                        listBox4.Items.Add(lb);
                    }
                }
            }
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBoxItem ll = (ListBoxItem)listBox4.SelectedItem;
            if (ll!=null)
            {
                string res = ll.Tag[0].ToString();
                label1.Text = get_name_team(res, sostavy);
            }

        }
    }
}
