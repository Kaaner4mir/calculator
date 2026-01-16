using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Calculator
{
    public partial class form_calculator : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        Double resultValue = 0;
        String operationPerformed = "";
        bool isOperationPerformed = false;
        bool resultShown = false; // New flag to track if result is currently shown

        public form_calculator()
        {
            InitializeComponent();
            SetupCalculator();
        }

        private void SetupCalculator()
        {
            // Configure Result Screen to prevent native double-typing and cursor focus
            tbx_result_screen.ReadOnly = true;
            tbx_result_screen.BackColor = System.Drawing.Color.White;
            tbx_result_screen.TabStop = false;

            // Assign Tags effectively mapping buttons to their values
            btn_0.Tag = "0";
            btn_1.Tag = "1"; // Updated from button8
            btn_2.Tag = "2";
            btn_3.Tag = "3";
            btn_4.Tag = "4";
            btn_5.Tag = "5";
            btn_6.Tag = "6";
            btn_7.Tag = "7";
            btn_8.Tag = "8";
            btn_9.Tag = "9";
            // Use system decimal separator (comma or dot)
            btn_dot.Tag = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            btn_plus.Tag = "+";
            btn_minus.Tag = "-";
            btn_multiply.Tag = "*";
            btn_divide.Tag = "/";
            btn_mod.Tag = "%";

            // Enable Keyboard Input
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form_KeyDown);

            // Set standard form behaviors for Enter and Esc
            this.AcceptButton = btn_equal; 
            this.CancelButton = btn_ac;

            // Attach Event Handlers
            AttachNumberEvents();
            AttachOperatorEvents();

            // Special Buttons
            btn_equal.Click += new EventHandler(btn_equal_Click);
            btn_ac.Click += new EventHandler(btn_ac_Click);
            btn_delete.Click += new EventHandler(btn_delete_Click);
            
            // Attach Animation Events to all Buttons
            foreach (Control c in this.Controls)
            {
                if (c is Button)
                {
                    Button btn = (Button)c;
                    btn.TabStop = false; // Prevent focus retention
                    btn.MouseDown += new MouseEventHandler(Button_MouseDown);
                    btn.MouseUp += new MouseEventHandler(Button_MouseUp);
                }
            }
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad0:
                case Keys.D0:
                    btn_0.PerformClick();
                    break;
                case Keys.NumPad1:
                case Keys.D1:
                    btn_1.PerformClick();
                    break;
                case Keys.NumPad2:
                case Keys.D2:
                    btn_2.PerformClick();
                    break;
                case Keys.NumPad3:
                case Keys.D3:
                    btn_3.PerformClick();
                    break;
                case Keys.NumPad4:
                case Keys.D4:
                    btn_4.PerformClick();
                    break;
                case Keys.NumPad5:
                case Keys.D5:
                    if (e.Shift && e.KeyCode == Keys.D5) // Shift+5 (%)
                        btn_mod.PerformClick();
                    else
                        btn_5.PerformClick();
                    break;
                case Keys.NumPad6:
                case Keys.D6:
                    btn_6.PerformClick();
                    break;
                case Keys.NumPad7:
                case Keys.D7:
                    btn_7.PerformClick();
                    break;
                case Keys.NumPad8:
                case Keys.D8:
                    if (e.Shift) // Shift+8 (*)
                        btn_multiply.PerformClick();
                    else 
                        btn_8.PerformClick();
                    break;
                case Keys.NumPad9:
                case Keys.D9:
                    btn_9.PerformClick();
                    break;
                case Keys.Add:
                case Keys.Oemplus:
                    btn_plus.PerformClick();
                    break;
                case Keys.Subtract:
                case Keys.OemMinus:
                    btn_minus.PerformClick();
                    break;
                case Keys.Multiply:
                    btn_multiply.PerformClick();
                    break;
                case Keys.Divide:
                case Keys.OemQuestion:
                case Keys.Oem5: 
                    btn_divide.PerformClick();
                    break;
                case Keys.Decimal:
                case Keys.OemPeriod:
                    btn_dot.PerformClick();
                    break;
                case Keys.Enter:
                    btn_equal.PerformClick();
                    e.Handled = true;
                    e.SuppressKeyPress = true; // Prevents "Ding" sound
                    break;
                case Keys.Back:
                    btn_delete.PerformClick();
                    break;
                case Keys.Escape:
                    btn_ac.PerformClick();
                    break;
                default:
                    break;
            }
        }

        // Animation Handlers
        private void Button_MouseDown(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            // Shrink slightly by increasing padding or decreasing size. 
            // Since size is fixed in layout, using Padding is safer or checking logic. 
            // A simple visual trick is to slightly reduce font or move location, but padding works well for flat buttons.
            // Let's try scaling size a bit visually if possible, or just padding.
            // Simplified: Just add padding to simulate "press in"
            btn.Padding = new Padding(4); 
        }

        private void Button_MouseUp(object sender, MouseEventArgs e)
        {
            Button btn = (Button)sender;
            btn.Padding = new Padding(0); // Reset
        }

        private void AttachNumberEvents()
        {
            string decSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            foreach (Control c in this.Controls)
            {
                if (c is Button)
                {
                    Button btn = (Button)c;
                    // Check if it's a number button or dot based on Tag
                    if (btn.Tag != null && ("0123456789" + decSep).Contains(btn.Tag.ToString()) && btn.Tag.ToString().Length >= 1)
                    {
                        btn.Click += new EventHandler(button_click);
                    }
                }
            }
        }

        private void AttachOperatorEvents()
        {
             foreach (Control c in this.Controls)
            {
                if (c is Button)
                {
                    Button btn = (Button)c;
                    // Check if it's an operator button
                    if (btn.Tag != null && "+-*/%".Contains(btn.Tag.ToString()) && btn.Tag.ToString().Length == 1)
                    {
                        btn.Click += new EventHandler(operator_click);
                    }
                }
            }
        }

        private void button_click(object sender, EventArgs e)
        {
            // If recovering from Error, perform full reset
            if (tbx_result_screen.Text == "Error")
            {
                tbx_result_screen.Clear();
                tbx_history.Text = "";
                resultValue = 0;
                operationPerformed = "";
            }
            // If starting fresh (0), or after an operation, or showing result
            else if ((tbx_result_screen.Text == "0") || (isOperationPerformed) || (resultShown))
            {
                tbx_result_screen.Clear();
            }

            isOperationPerformed = false;
            resultShown = false; // Reset the flag since we started typing a new number
            Button button = (Button)sender;
            string decSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;

            // Limit to one decimal point
            if (button.Tag.ToString() == decSep)
            {
                if (!tbx_result_screen.Text.Contains(decSep))
                {
                    // If empty (e.g. after clear), prepend 0 -> "0."
                    if (tbx_result_screen.Text == "")
                        tbx_result_screen.Text = "0" + button.Tag.ToString();
                    else
                        tbx_result_screen.Text = tbx_result_screen.Text + button.Tag.ToString();
                }
            }
            else
            {
                tbx_result_screen.Text = tbx_result_screen.Text + button.Tag.ToString();
            }
            this.ActiveControl = null; // Remove focus from button to fix Enter key behavior
        }

        private void PerformCalculation()
        {
             switch (operationPerformed)
                {
                    case "+":
                        tbx_result_screen.Text = (resultValue + Double.Parse(tbx_result_screen.Text)).ToString();
                        break;
                    case "-":
                        tbx_result_screen.Text = (resultValue - Double.Parse(tbx_result_screen.Text)).ToString();
                        break;
                    case "*":
                        tbx_result_screen.Text = (resultValue * Double.Parse(tbx_result_screen.Text)).ToString();
                        break;
                    case "/":
                        if (Double.Parse(tbx_result_screen.Text) != 0)
                            tbx_result_screen.Text = (resultValue / Double.Parse(tbx_result_screen.Text)).ToString();
                        else
                            tbx_result_screen.Text = "Error";
                        break;
                    case "%":
                        if (Double.Parse(tbx_result_screen.Text) != 0)
                             tbx_result_screen.Text = (resultValue % Double.Parse(tbx_result_screen.Text)).ToString();
                        else
                             tbx_result_screen.Text = "Error";
                        break;
                    default:
                        break;
                }
             
             if (tbx_result_screen.Text != "Error")
                resultValue = Double.Parse(tbx_result_screen.Text);
        }

        private void operator_click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            bool wasError = false;
            Double currentInput = 0;

            try 
            {
                 currentInput = Double.Parse(tbx_result_screen.Text);
            }
            catch (FormatException) 
            {
                 currentInput = 0;
                 wasError = true;
                 tbx_result_screen.Text = "0"; // Treat error as 0 for continuity logic below
                 tbx_history.Text = ""; 
            }

            // Append to history BEFORE updating result screen with calculation
            string currentValText = wasError ? "0" : currentInput.ToString(); // Use cleaned/parsed value or 0
            // If checking chaining operations...
            // If we have a pending op, we calculate. 
            if ((resultValue != 0 || operationPerformed != "") && !isOperationPerformed)
            {
                // If it's the very first op (resultValue was 0, op empty), we don't calculate.
                // But currentInput is what we just typed. 
                // Wait, if resultValue is 0 (initial), we shouldn't calc.
                // if operationPerformed is NOT empty, we definitely calc.
                if (operationPerformed != "")
                {
                     PerformCalculation();
                }
                else
                {
                     resultValue = currentInput;
                }
            }
            else
            {
                 resultValue = currentInput;
            }

            // Update History
            // Note: If we just calculated (e.g. 50+50=100), screen shows 100.
            // But we wanted history to show "50 + 50 + ". 
            // So we use 'currentInput' (the 50 we typed) for history, NOT the new result.
            tbx_history.Text = tbx_history.Text + currentValText + " " + button.Tag.ToString() + " "; 

            operationPerformed = button.Tag.ToString();
            isOperationPerformed = true;
            resultShown = false; 
            this.ActiveControl = null; 
        }

        private void btn_equal_Click(object sender, EventArgs e)
        {
            try
            {
                PerformCalculation();
                
                operationPerformed = "";
                tbx_history.Text = ""; 
                resultShown = true; 
            }
            catch (Exception)
            {
               tbx_result_screen.Text = "Error";
            }
        }

        private void btn_ac_Click(object sender, EventArgs e)
        {
            tbx_result_screen.Text = "0";
            resultValue = 0;
            operationPerformed = "";
            tbx_history.Text = "";
            resultShown = false;
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (tbx_result_screen.Text == "Error")
            {
                tbx_result_screen.Text = "0";
                return;
            }

            if (tbx_result_screen.Text.Length > 0)
            {
                tbx_result_screen.Text = tbx_result_screen.Text.Remove(tbx_result_screen.Text.Length - 1, 1);
            }

            if (tbx_result_screen.Text == "")
            {
                tbx_result_screen.Text = "0";
            }
        }
        private void btn_close_mark_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pnl_title_bar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
