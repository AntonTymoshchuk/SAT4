using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Chromius.Window;

namespace SAT4
{
    public partial class Wnd : Form
    {
        private Window wnd;
        private Button buyButton;
        private Button sellButton;
        private Button buyExit;
        private Button sellExit;
        private Button updateButton;
        private TextBox buyValue;
        private TextBox sellValue;
        private TextBox buyStopLoss;
        private TextBox sellStopLoss;
        private TextBox buyTakeProfit;
        private TextBox sellTakeProfit;
        private DirectoryInfo SAT4mainDir = new DirectoryInfo(@"C:\SAT4");
        private DirectoryInfo Container;
        private bool buyActive = false;
        private bool sellActive = false;
        private bool buyEnter = false;
        private bool sellEnter = false;
        private bool updateAviliable = false;
        private bool buyFlashingLocked = false;
        private bool sellFlashingLocked = false;
        private bool buyMiddleState = false;
        private bool sellMiddleState = false;
        private Thread ParalThread;
        private Thread BuyFlashingThread;
        private Thread SellFlashingThread;
        public Wnd()
        {
            InitializeComponent();
            wnd = new Window(this);
            wnd.Header.Maximize = false;
            wnd.Header.Font = new Font("Calibri", 12);
            wnd.Header.TextLocation = new Point(4, 7);
            this.Size = new Size(274, 316);
            this.TopMost = true;
            this.FormClosed += AfterClosing;
            this.Resize += OnResize;
            this.MinimumSize = new Size(186, 316);
            LoadControls();
            ParalThread = new Thread(ParalMethod);
            if (FindInstrument() == false)
                this.Close();
        }

        private void OnResize(object Sender, EventArgs Info)
        {
            buyButton.Width = (this.Width - 32) / 2;
            sellButton.Width = (this.Width - 32) / 2;
            sellButton.Left = buyButton.Width + 20;
            buyValue.Width = (this.Width - 32) / 2;
            sellValue.Width = (this.Width - 32) / 2;
            sellValue.Left = buyValue.Width + 20;
            buyExit.Width = (this.Width - 32) / 2;
            sellExit.Width = (this.Width - 32) / 2;
            sellExit.Left = buyExit.Width + 20;
            updateButton.Width = this.Width - 24;
            buyStopLoss.Width = (this.Width - 32) / 2;
            sellStopLoss.Width = (this.Width - 32) / 2;
            sellStopLoss.Left = buyValue.Width + 20;
            buyTakeProfit.Width = (this.Width - 32) / 2;
            sellTakeProfit.Width = (this.Width - 32) / 2;
            sellTakeProfit.Left = buyValue.Width + 20;
        }

        private bool FindInstrument()
        {
            if (SAT4mainDir.Exists == false)
            {
                MessageBox.Show("Вы запускаете SAT4 впервые\nСначало запустите стратегию SAT4 в NinjaTrader 8", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            else
            {
                DirectoryInfo[] Containers = SAT4mainDir.GetDirectories();
                if (Containers.Length == 0)
                {
                    MessageBox.Show("Вы не запустили ни одной стратегии SAT4\nСначало запустите стратегию SAT4 в NinjaTrader 8", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                else
                {
                    FileStream Variable;
                    StreamReader Reader;
                    for (int i = 0; i < Containers.Length; i++)
                    {
                        Variable = new FileStream(Containers[i].FullName + @"\Joined.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        Reader = new StreamReader(Variable);
                        if (Reader.ReadLine() == "false")
                        {
                            Reader.Close();
                            Variable.Close();
                            Variable = new FileStream(Containers[i].FullName + @"\Joined.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            StreamWriter Writer = new StreamWriter(Variable);
                            Writer.WriteLine("true");
                            Writer.Close();
                            Variable.Close();
                            Container = Containers[i];
                            wnd.Header.Text += " : (" + Container.Name + ")";
                            ParalThread.Start();
                            return true;
                        }
                        Reader.Close();
                        Variable.Close();
                    }
                    MessageBox.Show("Не найдено ни одной свободной стратегии SAT4\nЗапустите новую стратегию SAT4 в NinjaTrader 8", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
        }

        private void AfterClosing(object Sender, EventArgs Info)
        {
            ParalThread.Abort();
            FileStream Variable = new FileStream(Container.FullName + @"\Joined.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
            StreamWriter Writer = new StreamWriter(Variable);
            Writer.WriteLine("false");
            Writer.Close();
            Variable.Close();
            bool Locked;
            Mutex Unbind_Mutex = new Mutex(false, "Unbind_Mutex", out Locked);
            Unbind_Mutex.WaitOne();
            FileStream Unbind = new FileStream(Container.FullName + @"\Unbind.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
            StreamWriter UnbindWriter = new StreamWriter(Unbind);
            UnbindWriter.WriteLine("true");
            UnbindWriter.Close();
            Unbind.Close();
            Unbind_Mutex.ReleaseMutex();
        }

        private void LoadControls()
        {
            //buyButton
            buyButton = new Button();
            buyButton.Name = "buyButton";
            buyButton.TabIndex = 0;
            buyButton.TabStop = false;
            buyButton.Location = new Point(12, 91);
            buyButton.Size = new Size(120, 60);
            buyButton.BackColor = Color.DimGray;
            buyButton.FlatStyle = FlatStyle.Flat;
            buyButton.FlatAppearance.BorderSize = 0;
            buyButton.FlatAppearance.MouseOverBackColor = Color.DimGray;
            buyButton.FlatAppearance.MouseDownBackColor = Color.DimGray;
            buyButton.Text = "Buy";
            buyButton.Font = new Font("Calibri", 18);
            buyButton.TextAlign = ContentAlignment.MiddleCenter;
            buyButton.ForeColor = Color.White;
            buyButton.MouseClick += buyButtonClick;
            this.Controls.Add(buyButton);

            //sellButton
            sellButton = new Button();
            sellButton.Name = "sellButton";
            sellButton.TabIndex = 0;
            sellButton.TabStop = false;
            sellButton.Location = new Point(140, 91);
            sellButton.Size = new Size(120, 60);
            sellButton.BackColor = Color.DimGray;
            sellButton.FlatStyle = FlatStyle.Flat;
            sellButton.FlatAppearance.BorderSize = 0;
            sellButton.FlatAppearance.MouseOverBackColor = Color.DimGray;
            sellButton.FlatAppearance.MouseDownBackColor = Color.DimGray;
            sellButton.Text = "Sell";
            sellButton.Font = new Font("Calibri", 18);
            sellButton.TextAlign = ContentAlignment.MiddleCenter;
            sellButton.ForeColor = Color.White;
            sellButton.MouseClick += sellButtonClick;
            this.Controls.Add(sellButton);

            //buyExit
            buyExit = new Button();
            buyExit.Name = "buyExit";
            buyExit.TabIndex = 0;
            buyExit.TabStop = false;
            buyExit.Location = new Point(12, 43);
            buyExit.Size = new Size(120, 40);
            buyExit.BackColor = Color.Silver;
            buyExit.FlatStyle = FlatStyle.Flat;
            buyExit.FlatAppearance.BorderSize = 0;
            buyExit.FlatAppearance.MouseOverBackColor = Color.Silver;
            buyExit.FlatAppearance.MouseDownBackColor = Color.Silver;
            buyExit.Text = "Close";
            buyExit.Font = new Font("Calibri", 18);
            buyExit.TextAlign = ContentAlignment.MiddleCenter;
            buyExit.ForeColor = Color.White;
            buyExit.MouseClick += buyExitClick;
            this.Controls.Add(buyExit);

            //sellExit
            sellExit = new Button();
            sellExit.Name = "sellExit";
            sellExit.TabIndex = 0;
            sellExit.TabStop = false;
            sellExit.Location = new Point(140, 43);
            sellExit.Size = new Size(120, 40);
            sellExit.BackColor = Color.Silver;
            sellExit.FlatStyle = FlatStyle.Flat;
            sellExit.FlatAppearance.BorderSize = 0;
            sellExit.FlatAppearance.MouseOverBackColor = Color.Silver;
            sellExit.FlatAppearance.MouseDownBackColor = Color.Silver;
            sellExit.Text = "Close";
            sellExit.Font = new Font("Calibri", 18);
            sellExit.TextAlign = ContentAlignment.MiddleCenter;
            sellExit.ForeColor = Color.White;
            sellExit.MouseClick += sellExitClick;
            this.Controls.Add(sellExit);

            //updateButton
            updateButton = new Button();
            updateButton.Name = "updateButton";
            updateButton.TabIndex = 0;
            updateButton.TabStop = false;
            updateButton.Location = new Point(12, 194);
            updateButton.Size = new Size(248, 40);
            updateButton.BackColor = Color.Silver;
            updateButton.FlatStyle = FlatStyle.Flat;
            updateButton.FlatAppearance.BorderSize = 0;
            updateButton.FlatAppearance.MouseOverBackColor = Color.Silver;
            updateButton.FlatAppearance.MouseDownBackColor = Color.Silver;
            updateButton.Text = "Update";
            updateButton.Font = new Font("Calibri", 18);
            updateButton.TextAlign = ContentAlignment.MiddleCenter;
            updateButton.ForeColor = Color.White;
            updateButton.MouseClick += updateButtonClick;
            this.Controls.Add(updateButton);

            //buyValue
            buyValue = new TextBox();
            buyValue.Name = "buyValue";
            buyValue.TabIndex = 0;
            buyValue.TabStop = false;
            buyValue.Location = new Point(12, 159);
            buyValue.Width = 120;
            buyValue.BackColor = Color.Gainsboro;
            buyValue.ForeColor = Color.Black;
            buyValue.Font = new Font("Calibri", 16);
            buyValue.TextAlign = HorizontalAlignment.Center;
            buyValue.BorderStyle = BorderStyle.None;
            buyValue.TextChanged += contentUpdated;
            this.Controls.Add(buyValue);

            //sellValue
            sellValue = new TextBox();
            sellValue.Name = "sellValue";
            sellValue.TabIndex = 0;
            sellValue.TabStop = false;
            sellValue.Location = new Point(140, 159);
            sellValue.Width = 120;
            sellValue.BackColor = Color.Gainsboro;
            sellValue.ForeColor = Color.Black;
            sellValue.Font = new Font("Calibri", 16);
            sellValue.TextAlign = HorizontalAlignment.Center;
            sellValue.BorderStyle = BorderStyle.None;
            sellValue.TextChanged += contentUpdated;
            this.Controls.Add(sellValue);

            //buyStopLoss
            buyStopLoss = new TextBox();
            buyStopLoss.Name = "buyStopLoss";
            buyStopLoss.TabIndex = 0;
            buyStopLoss.TabStop = false;
            buyStopLoss.Location = new Point(12, 242);
            buyStopLoss.Width = 120;
            buyStopLoss.BackColor = Color.Gainsboro;
            buyStopLoss.ForeColor = Color.FromArgb(151, 0, 0);
            buyStopLoss.Font = new Font("Calibri", 16);
            buyStopLoss.TextAlign = HorizontalAlignment.Center;
            buyStopLoss.BorderStyle = BorderStyle.None;
            buyStopLoss.TextChanged += contentUpdated;
            this.Controls.Add(buyStopLoss);

            //sellStopLoss
            sellStopLoss = new TextBox();
            sellStopLoss.Name = "sellStopLoss";
            sellStopLoss.TabIndex = 0;
            sellStopLoss.TabStop = false;
            sellStopLoss.Location = new Point(140, 242);
            sellStopLoss.Width = 120;
            sellStopLoss.BackColor = Color.Gainsboro;
            sellStopLoss.ForeColor = Color.FromArgb(151, 0, 0);
            sellStopLoss.Font = new Font("Calibri", 16);
            sellStopLoss.TextAlign = HorizontalAlignment.Center;
            sellStopLoss.BorderStyle = BorderStyle.None;
            sellStopLoss.TextChanged += contentUpdated;
            this.Controls.Add(sellStopLoss);

            //buyTakeProfit
            buyTakeProfit = new TextBox();
            buyTakeProfit.Name = "buyTakeProfit";
            buyTakeProfit.TabIndex = 0;
            buyTakeProfit.TabStop = false;
            buyTakeProfit.Location = new Point(12, 277);
            buyTakeProfit.Width = 120;
            buyTakeProfit.BackColor = Color.Gainsboro;
            buyTakeProfit.ForeColor = Color.FromArgb(0, 115, 0);
            buyTakeProfit.Font = new Font("Calibri", 16);
            buyTakeProfit.TextAlign = HorizontalAlignment.Center;
            buyTakeProfit.BorderStyle = BorderStyle.None;
            buyTakeProfit.TextChanged += contentUpdated;
            this.Controls.Add(buyTakeProfit);

            //sellTakeProfit
            sellTakeProfit = new TextBox();
            sellTakeProfit.Name = "sellTakeProfit";
            sellTakeProfit.TabIndex = 0;
            sellTakeProfit.TabStop = false;
            sellTakeProfit.Location = new Point(140, 277);
            sellTakeProfit.Width = 120;
            sellTakeProfit.BackColor = Color.Gainsboro;
            sellTakeProfit.ForeColor = Color.FromArgb(0, 115, 0);
            sellTakeProfit.Font = new Font("Calibri", 16);
            sellTakeProfit.TextAlign = HorizontalAlignment.Center;
            sellTakeProfit.BorderStyle = BorderStyle.None;
            sellTakeProfit.TextChanged += contentUpdated;
            this.Controls.Add(sellTakeProfit);
        }

        private void buyButtonClick(object Sender, EventArgs Info)
        {
            if (buyEnter == false)
            {
                if (buyActive == false)
                {
                    if (buyValue.Text == "")
                    {
                        MessageBox.Show("Вы не указали цену покупки", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(buyValue.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректная цена покупки", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream BuyValue = new FileStream(Container.FullName + @"\BuyValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(BuyValue);
                        StreamWriter BuyValueWriter = new StreamWriter(BuyValue);
                        BuyValueWriter.WriteLine(buyValue.Text);
                        BuyValueWriter.Close();
                        BuyValue.Close();
                        Monitor.Exit(BuyValue);
                    }

                    //buyStopLoss
                    if (buyStopLoss.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Stop loss на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(buyStopLoss.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Stop loss на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream BuyStopLossValue = new FileStream(Container.FullName + @"\BuyStopLossValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(BuyStopLossValue);
                        StreamWriter BuyStopLossValueWriter = new StreamWriter(BuyStopLossValue);
                        BuyStopLossValueWriter.WriteLine(buyStopLoss.Text);
                        BuyStopLossValueWriter.Close();
                        BuyStopLossValue.Close();
                        Monitor.Exit(BuyStopLossValue);
                    }

                    //buyTakeProfit
                    if (buyTakeProfit.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Take profit на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(buyTakeProfit.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Take profit на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream BuyTakeProfitValue = new FileStream(Container.FullName + @"\BuyTakeProfitValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(BuyTakeProfitValue);
                        StreamWriter BuyTakeProfitValueWriter = new StreamWriter(BuyTakeProfitValue);
                        BuyTakeProfitValueWriter.WriteLine(buyTakeProfit.Text);
                        BuyTakeProfitValueWriter.Close();
                        BuyTakeProfitValue.Close();
                        Monitor.Exit(BuyTakeProfitValue);
                    }

                    //continue
                    FileStream BuyActive = new FileStream(Container.FullName + @"\BuyActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    Monitor.Enter(BuyActive);
                    StreamWriter BuyActiveWriter = new StreamWriter(BuyActive);
                    BuyActiveWriter.WriteLine("true");
                    BuyActiveWriter.Close();
                    BuyActive.Close();
                    Monitor.Exit(BuyActive);
                    buyActive = true;
                }
                else
                {
                    FileStream BuyActive = new FileStream(Container.FullName + @"\BuyActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    Monitor.Enter(BuyActive);
                    StreamWriter BuyActiveWriter = new StreamWriter(BuyActive);
                    BuyActiveWriter.WriteLine("false");
                    BuyActiveWriter.Close();
                    BuyActive.Close();
                    Monitor.Exit(BuyActive);
                    buyActive = false;
                    buyValue.ReadOnly = false;
                    buyStopLoss.ReadOnly = false;
                    buyTakeProfit.ReadOnly = false;
                }
            }
        }

        private void sellButtonClick(object Sender, EventArgs Info)
        {
            if (sellEnter == false)
            {
                if (sellActive == false)
                {
                    if (sellValue.Text == "")
                    {
                        MessageBox.Show("Вы не указали цену продажи", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(sellValue.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректная цена продажи", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream SellValue = new FileStream(Container.FullName + @"\SellValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(SellValue);
                        StreamWriter SellValueWriter = new StreamWriter(SellValue);
                        SellValueWriter.WriteLine(sellValue.Text);
                        SellValueWriter.Close();
                        SellValue.Close();
                        Monitor.Exit(SellValue);
                    }

                    //sellStopLoss
                    if (sellStopLoss.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Stop loss на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(sellStopLoss.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Stop loss на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream SellStopLossValue = new FileStream(Container.FullName + @"\SellStopLossValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(SellStopLossValue);
                        StreamWriter SellStopLossValueWriter = new StreamWriter(SellStopLossValue);
                        SellStopLossValueWriter.WriteLine(sellStopLoss.Text);
                        SellStopLossValueWriter.Close();
                        SellStopLossValue.Close();
                        Monitor.Exit(SellStopLossValue);
                    }

                    //sellTakeProfit
                    if (sellTakeProfit.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Take profit на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(sellTakeProfit.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Take profit на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream SellTakeProfitValue = new FileStream(Container.FullName + @"\SellTakeProfitValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(SellTakeProfitValue);
                        StreamWriter SellTakeProfitValueWriter = new StreamWriter(SellTakeProfitValue);
                        SellTakeProfitValueWriter.WriteLine(sellTakeProfit.Text);
                        SellTakeProfitValueWriter.Close();
                        SellTakeProfitValue.Close();
                        Monitor.Exit(SellTakeProfitValue);
                    }

                    //continue
                    FileStream SellActive = new FileStream(Container.FullName + @"\SellActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    Monitor.Enter(SellActive);
                    StreamWriter SellActiveWriter = new StreamWriter(SellActive);
                    SellActiveWriter.WriteLine("true");
                    SellActiveWriter.Close();
                    SellActive.Close();
                    Monitor.Exit(SellActive);
                    sellActive = true;
                }
                else
                {
                    FileStream SellActive = new FileStream(Container.FullName + @"\SellActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    Monitor.Enter(SellActive);
                    StreamWriter SellActiveWriter = new StreamWriter(SellActive);
                    SellActiveWriter.WriteLine("false");
                    SellActiveWriter.Close();
                    SellActiveWriter.Close();
                    Monitor.Exit(SellActive);
                    sellActive = false;
                    sellValue.ReadOnly = false;
                    sellStopLoss.ReadOnly = false;
                    sellTakeProfit.ReadOnly = false;
                }
            }
        }

        private void buyExitClick(object Sender, EventArgs Info)
        {
            if (buyEnter == true)
            {
                bool Locked;
                Mutex BuyExit_Mutex = new Mutex(false, "BuyExit_Mutex", out Locked);
                if (Locked == false)
                {
                    BuyExit_Mutex.WaitOne();
                    FileStream BuyExit = new FileStream(Container.FullName + @"\BuyExit.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    StreamWriter BuyExitWriter = new StreamWriter(BuyExit);
                    BuyExitWriter.WriteLine("true");
                    BuyExitWriter.Close();
                    BuyExit.Close();
                    BuyExit_Mutex.ReleaseMutex();
                    buyExit.BackColor = Color.Silver;
                    buyExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                    buyExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                }
            }
        }

        private void sellExitClick(object Sender, EventArgs Info)
        {
            if (sellEnter == true)
            {
                bool Locked;
                Mutex SellExit_Mutex = new Mutex(false, "SellExit_Mutex", out Locked);
                if (Locked == false)
                {
                    SellExit_Mutex.WaitOne();
                    FileStream SellExit = new FileStream(Container.FullName + @"\SellExit.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    StreamWriter SellExitWriter = new StreamWriter(SellExit);
                    SellExitWriter.WriteLine("true");
                    SellExitWriter.Close();
                    SellExit.Close();
                    SellExit_Mutex.ReleaseMutex();
                    sellExit.BackColor = Color.Silver;
                    sellExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                    sellExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                }
            }
        }

        private void updateButtonClick(object Sender, EventArgs Info)
        {
            if (updateAviliable == true)
            {
                if (buyActive == true)
                {
                    //buyValue
                    if (buyEnter == false)
                    {
                        if (buyValue.Text == "")
                        {
                            MessageBox.Show("Вы не указали цену покупки", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            try
                            { double test = Convert.ToDouble(buyValue.Text); }
                            catch
                            {
                                MessageBox.Show("Некорректная цена покупки", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            FileStream BuyValue = new FileStream(Container.FullName + @"\BuyValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            Monitor.Enter(BuyValue);
                            StreamWriter BuyValueWriter = new StreamWriter(BuyValue);
                            BuyValueWriter.WriteLine(buyValue.Text);
                            BuyValueWriter.Close();
                            BuyValue.Close();
                            Monitor.Exit(BuyValue);
                        }
                    }

                    //buyStopLoss
                    if (buyStopLoss.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Stop loss на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(buyStopLoss.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Stop loss на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream BuyStopLossValue = new FileStream(Container.FullName + @"\BuyStopLossValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(BuyStopLossValue);
                        StreamWriter BuyStopLossValueWriter = new StreamWriter(BuyStopLossValue);
                        BuyStopLossValueWriter.WriteLine(buyStopLoss.Text);
                        BuyStopLossValueWriter.Close();
                        BuyStopLossValue.Close();
                        Monitor.Exit(BuyStopLossValue);
                    }

                    //buyTakeProfit
                    if (buyTakeProfit.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Take profit на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(buyTakeProfit.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Take profit на покупку", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream BuyTakeProfitValue = new FileStream(Container.FullName + @"\BuyTakeProfitValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(BuyTakeProfitValue);
                        StreamWriter BuyTakeProfitValueWriter = new StreamWriter(BuyTakeProfitValue);
                        BuyTakeProfitValueWriter.WriteLine(buyTakeProfit.Text);
                        BuyTakeProfitValueWriter.Close();
                        BuyTakeProfitValue.Close();
                        Monitor.Exit(BuyTakeProfitValue);
                    }
                }

                if (sellActive == true)
                {
                    //sellValue
                    if (sellEnter == false)
                    {
                        if (sellValue.Text == "")
                        {
                            MessageBox.Show("Вы не указали цену продажи", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        else
                        {
                            try
                            { double test = Convert.ToDouble(sellValue.Text); }
                            catch
                            {
                                MessageBox.Show("Некорректная цена продажи", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                            FileStream SellValue = new FileStream(Container.FullName + @"\SellValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            Monitor.Enter(SellValue);
                            StreamWriter SellValueWriter = new StreamWriter(SellValue);
                            SellValueWriter.WriteLine(sellValue.Text);
                            SellValueWriter.Close();
                            SellValue.Close();
                            Monitor.Exit(SellValue);
                        }
                    }

                    //sellStopLoss
                    if (sellStopLoss.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Stop loss на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(sellStopLoss.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Stop loss на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream SellStopLossValue = new FileStream(Container.FullName + @"\SellStopLossValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(SellStopLossValue);
                        StreamWriter SellStopLossValueWriter = new StreamWriter(SellStopLossValue);
                        SellStopLossValueWriter.WriteLine(sellStopLoss.Text);
                        SellStopLossValueWriter.Close();
                        SellStopLossValue.Close();
                        Monitor.Exit(SellStopLossValue);
                    }

                    //sellTakeProfit
                    if (sellTakeProfit.Text == "")
                    {
                        MessageBox.Show("Вы не указали значение Take profit на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    else
                    {
                        try
                        { double test = Convert.ToDouble(sellTakeProfit.Text); }
                        catch
                        {
                            MessageBox.Show("Некорректное значение Take profit на продажу", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        FileStream SellTakeProfitValue = new FileStream(Container.FullName + @"\SellTakeProfitValue.ftype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        Monitor.Enter(SellTakeProfitValue);
                        StreamWriter SellTakeProfitValueWriter = new StreamWriter(SellTakeProfitValue);
                        SellTakeProfitValueWriter.WriteLine(sellTakeProfit.Text);
                        SellTakeProfitValueWriter.Close();
                        SellTakeProfitValue.Close();
                        Monitor.Exit(SellTakeProfitValue);
                    }
                }
                bool Locked;
                Mutex Updated_Mutex = new Mutex(false, "Updated_Mutex", out Locked);
                if (Locked == false)
                {
                    Updated_Mutex.WaitOne();
                    FileStream Updated = new FileStream(Container.FullName + @"\Updated.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                    StreamWriter UpdatedWriter = new StreamWriter(Updated);
                    UpdatedWriter.WriteLine("true");
                    UpdatedWriter.Close();
                    Updated.Close();
                    updateButton.BackColor = Color.Silver;
                    updateButton.FlatAppearance.MouseOverBackColor = Color.Silver;
                    updateButton.FlatAppearance.MouseDownBackColor = Color.Silver;
                    Updated_Mutex.ReleaseMutex();
                    updateAviliable = false;
                }
            }
        }

        private void contentUpdated(object Sender, EventArgs Info)
        {
            TextBox Param = Sender as TextBox;
            if (buyActive == true || sellActive == true)
            {
                updateAviliable = true;
                updateButton.BackColor = Color.Orange;
                updateButton.FlatAppearance.MouseOverBackColor = Color.Orange;
                updateButton.FlatAppearance.MouseDownBackColor = Color.Orange;
            }
        }

        private void ParalMethod()
        {
            FileStream BuyEnter;
            Mutex BuyEnter_Mutex;
            bool BuyEnterLocked;

            FileStream BuyProfit;
            Mutex BuyProfit_Mutex;
            bool BuyProfitLocked;

            FileStream SellEnter;
            Mutex SellEnter_Mutex;
            bool SellEnterLocked;

            FileStream SellProfit;
            Mutex SellProfit_Mutex;
            bool SellProfitLocked;

            FileStream BuySt1Done;
            Mutex BuySt1Done_Mutex;
            bool BuySt1DoneLocked;

            FileStream SellSt1Done;
            Mutex SellSt1Done_Mutex;
            bool SellSt1DoneLocked;

            while (true)
            {
                try
                {
                    //Buy thread
                    BuySt1Done_Mutex = new Mutex(false, "BuySt1Done_Mutex", out BuySt1DoneLocked);
                    if (BuySt1DoneLocked == false)
                    {
                        BuySt1Done_Mutex.WaitOne();
                        BuySt1Done = new FileStream(Container.FullName + @"\BuySt1Done.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader BuySt1DoneReader = new StreamReader(BuySt1Done);
                        if (BuySt1DoneReader.ReadLine() == "true")
                        {
                            BuySt1DoneReader.Close();
                            BuySt1Done.Close();
                            buyFlashingLocked = true;
                            BuyFlashingThread = new Thread(BuyFlashingMethod);
                            BuyFlashingThread.Start();
                            BuySt1Done = new FileStream(Container.FullName + @"\BuySt1Done.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            StreamWriter BuySt1DoneWriter = new StreamWriter(BuySt1Done);
                            BuySt1DoneWriter.WriteLine("false");
                            BuySt1DoneWriter.Close();
                            BuySt1Done.Close();
                        }
                        else
                        {
                            BuySt1DoneReader.Close();
                            BuySt1Done.Close();
                        }
                        BuySt1Done_Mutex.ReleaseMutex();
                    }

                    BuyEnter_Mutex = new Mutex(false, "BuyEnter_Mutex", out BuyEnterLocked);
                    if (BuyEnterLocked == false)
                    {
                        BuyEnter_Mutex.WaitOne();
                        BuyEnter = new FileStream(Container.FullName + @"\BuyEnter.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader BuyEnterReader = new StreamReader(BuyEnter);
                        if (BuyEnterReader.ReadLine() == "true")
                            buyEnter = true;
                        else
                            buyEnter = false;
                        BuyEnterReader.Close();
                        BuyEnter.Close();
                        BuyEnter_Mutex.ReleaseMutex();
                    }

                    BuyProfit_Mutex = new Mutex(false, "BuyProfit_Mutex", out BuyProfitLocked);
                    if (BuyProfitLocked == false)
                    {
                        BuyProfit_Mutex.WaitOne();
                        BuyProfit = new FileStream(Container.FullName + @"\BuyProfit.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader BuyProfitReader = new StreamReader(BuyProfit);
                        if (BuyProfitReader.ReadLine() == "true")
                        {
                            buyActive = false;
                            FileStream BuyActive = new FileStream(Container.FullName + @"\BuyActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            Monitor.Enter(BuyActive);
                            StreamWriter BuyActiveWriter = new StreamWriter(BuyActive);
                            BuyActiveWriter.WriteLine("false");
                            BuyActiveWriter.Close();
                            BuyActive.Close();
                            Monitor.Exit(BuyActive);
                            buyActive = false;
                            buyValue.ReadOnly = false;
                            buyStopLoss.ReadOnly = false;
                            buyTakeProfit.ReadOnly = false;
                        }
                        BuyProfitReader.Close();
                        BuyProfit.Close();
                        BuyProfit = new FileStream(Container.FullName + @"\BuyProfit.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        StreamWriter BuyProfitWriter = new StreamWriter(BuyProfit);
                        BuyProfitWriter.WriteLine("false");
                        BuyProfitWriter.Close();
                        BuyProfit.Close();
                        BuyProfit_Mutex.ReleaseMutex();
                    }

                    if (buyEnter == true && buyActive == true)
                    {
                        buyMiddleState = false;
                        if (buyFlashingLocked == true)
                        {
                            BuyFlashingThread.Abort();
                            buyFlashingLocked = false;
                        }
                        //buyButton.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyButton.BackColor = Color.FromArgb(0, 225, 0);
                        buyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 225, 0);
                        buyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 225, 0);
                        //}));
                        //buyValue.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyValue.ReadOnly = true;
                        //}));
                        //buyExit.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyExit.BackColor = Color.Blue;
                        buyExit.FlatAppearance.MouseOverBackColor = Color.Blue;
                        buyExit.FlatAppearance.MouseDownBackColor = Color.Blue;
                        //}));
                    }
                    else if (buyEnter == false && buyActive == true)
                    {
                        if (buyMiddleState == false)
                        {
                            //buyButton.Invoke((MethodInvoker)(delegate ()
                            //{
                            buyButton.BackColor = Color.FromArgb(0, 115, 0);
                            buyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 115, 0);
                            buyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 115, 0);
                            //}));
                            //buyValue.Invoke((MethodInvoker)(delegate ()
                            //{
                            buyValue.ReadOnly = false;
                            //}));
                            //buyExit.Invoke((MethodInvoker)(delegate ()
                            //{
                            buyExit.BackColor = Color.Silver;
                            buyExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                            buyExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                            //}));
                            buyMiddleState = true;
                        }
                    }
                    else if (buyEnter == false && buyActive == false)
                    {
                        buyMiddleState = false;
                        if (buyFlashingLocked == true)
                        {
                            BuyFlashingThread.Abort();
                            buyFlashingLocked = false;
                        }
                        //buyButton.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyButton.BackColor = Color.DimGray;
                        buyButton.FlatAppearance.MouseOverBackColor = Color.DimGray;
                        buyButton.FlatAppearance.MouseDownBackColor = Color.DimGray;
                        //}));
                        //buyValue.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyValue.ReadOnly = false;
                        //}));
                        //buyExit.Invoke((MethodInvoker)(delegate ()
                        //{
                        buyExit.BackColor = Color.Silver;
                        buyExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                        buyExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                        //}));
                    }


                    //Sell thread
                    SellSt1Done_Mutex = new Mutex(false, "SellSt1Done_Mutex", out SellSt1DoneLocked);
                    if (SellSt1DoneLocked == false)
                    {
                        SellSt1Done_Mutex.WaitOne();
                        SellSt1Done = new FileStream(Container.FullName + @"\SellSt1Done.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader SellSt1DoneReader = new StreamReader(SellSt1Done);
                        if (SellSt1DoneReader.ReadLine() == "true")
                        {
                            SellSt1DoneReader.Close();
                            SellSt1Done.Close();
                            sellFlashingLocked = true;
                            SellFlashingThread = new Thread(SellFlashingMethod);
                            SellFlashingThread.Start();
                            SellSt1Done = new FileStream(Container.FullName + @"\SellSt1Done.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            StreamWriter SellSt1DoneWriter = new StreamWriter(SellSt1Done);
                            SellSt1DoneWriter.WriteLine("false");
                            SellSt1DoneWriter.Close();
                            SellSt1Done.Close();
                        }
                        else
                        {
                            SellSt1DoneReader.Close();
                            SellSt1Done.Close();
                        }
                        SellSt1Done_Mutex.ReleaseMutex();
                    }

                    SellEnter_Mutex = new Mutex(false, "SellEnter_Mutex", out SellEnterLocked);
                    if (SellEnterLocked == false)
                    {
                        SellEnter_Mutex.WaitOne();
                        SellEnter = new FileStream(Container.FullName + @"\SellEnter.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader SellEnterReader = new StreamReader(SellEnter);
                        if (SellEnterReader.ReadLine() == "true")
                            sellEnter = true;
                        else
                            sellEnter = false;
                        SellEnterReader.Close();
                        SellEnter.Close();
                        SellEnter_Mutex.ReleaseMutex();
                    }

                    SellProfit_Mutex = new Mutex(false, "SellProfit_Mutex", out SellProfitLocked);
                    if (SellProfitLocked == false)
                    {
                        SellProfit_Mutex.WaitOne();
                        SellProfit = new FileStream(Container.FullName + @"\SellProfit.btype", FileMode.Open, FileAccess.Read, FileShare.Read);
                        StreamReader SellProfitReader = new StreamReader(SellProfit);
                        if (SellProfitReader.ReadLine() == "true")
                        {
                            sellActive = false;
                            FileStream SellActive = new FileStream(Container.FullName + @"\SellActive.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                            Monitor.Enter(SellActive);
                            StreamWriter SellActiveWriter = new StreamWriter(SellActive);
                            SellActiveWriter.WriteLine("false");
                            SellActiveWriter.Close();
                            SellActiveWriter.Close();
                            Monitor.Exit(SellActive);
                            sellValue.ReadOnly = false;
                            sellStopLoss.ReadOnly = false;
                            sellTakeProfit.ReadOnly = false;
                        }
                        SellProfitReader.Close();
                        SellProfit.Close();
                        SellProfit = new FileStream(Container.FullName + @"\SellProfit.btype", FileMode.Open, FileAccess.Write, FileShare.Write);
                        StreamWriter SellProfitWriter = new StreamWriter(SellProfit);
                        SellProfitWriter.WriteLine("false");
                        SellProfitWriter.Close();
                        SellProfit.Close();
                        SellProfit_Mutex.ReleaseMutex();
                    }

                    if (sellEnter == true && sellActive == true)
                    {
                        sellMiddleState = false;
                        if (sellFlashingLocked == true)
                        {
                            SellFlashingThread.Abort();
                            sellFlashingLocked = false;
                        }
                        //sellButton.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellButton.BackColor = Color.FromArgb(225, 0, 0);
                        sellButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 0, 0);
                        sellButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(225, 0, 0);
                        //}));
                        //sellValue.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellValue.ReadOnly = true;
                        //}));
                        //sellExit.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellExit.BackColor = Color.Blue;
                        sellExit.FlatAppearance.MouseOverBackColor = Color.Blue;
                        sellExit.FlatAppearance.MouseDownBackColor = Color.Blue;
                        //}));
                    }
                    else if (sellEnter == false && sellActive == true)
                    {
                        if (sellMiddleState == false)
                        {
                            //sellButton.Invoke((MethodInvoker)(delegate ()
                            //{
                            sellButton.BackColor = Color.FromArgb(115, 0, 0);
                            sellButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(115, 0, 0);
                            sellButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(115, 0, 0);
                            //}));
                            //sellValue.Invoke((MethodInvoker)(delegate ()
                            //{
                            sellValue.ReadOnly = false;
                            //}));
                            //sellExit.Invoke((MethodInvoker)(delegate ()
                            //{
                            sellExit.BackColor = Color.Silver;
                            sellExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                            sellExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                            //}));
                            sellMiddleState = true;
                        }
                    }
                    else if (sellEnter == false && sellActive == false)
                    {
                        sellMiddleState = false;
                        if (sellFlashingLocked == true)
                        {
                            SellFlashingThread.Abort();
                            sellFlashingLocked = false;
                        }
                        //sellButton.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellButton.BackColor = Color.DimGray;
                        sellButton.FlatAppearance.MouseOverBackColor = Color.DimGray;
                        sellButton.FlatAppearance.MouseDownBackColor = Color.DimGray;
                        //}));
                        //sellValue.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellValue.ReadOnly = false;
                        //}));
                        //sellExit.Invoke((MethodInvoker)(delegate ()
                        //{
                        sellExit.BackColor = Color.Silver;
                        sellExit.FlatAppearance.MouseOverBackColor = Color.Silver;
                        sellExit.FlatAppearance.MouseDownBackColor = Color.Silver;
                        //}));
                    }
                    Thread.Sleep(1);
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message, "Error");
                }
            }
        }

        private void BuyFlashingMethod()
        {
            while (true)
            {
                try
                {
                    //buyButton.Invoke((MethodInvoker)(delegate ()
                    //{
                    buyButton.BackColor = Color.FromArgb(0, 225, 0);
                    buyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 225, 0);
                    buyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 225, 0);
                    //}));
                    Thread.Sleep(500);
                    //buyButton.Invoke((MethodInvoker)(delegate ()
                    //{
                    buyButton.BackColor = Color.FromArgb(0, 115, 0);
                    buyButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 115, 0);
                    buyButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 115, 0);
                    //}));
                    Thread.Sleep(500);
                }
                catch { }
            }
        }

        private void SellFlashingMethod()
        {
            while (true)
            {
                try
                {
                    //sellButton.Invoke((MethodInvoker)(delegate ()
                    //{
                    sellButton.BackColor = Color.FromArgb(225, 0, 0);
                    sellButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 0, 0);
                    sellButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(225, 0, 0);
                    //}));
                    Thread.Sleep(500);
                    //sellButton.Invoke((MethodInvoker)(delegate ()
                    //{
                    sellButton.BackColor = Color.FromArgb(115, 0, 0);
                    sellButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(115, 0, 0);
                    sellButton.FlatAppearance.MouseDownBackColor = Color.FromArgb(115, 0, 0);
                    //}));
                    Thread.Sleep(500);
                }
                catch { }
            }
        }
    }
}
