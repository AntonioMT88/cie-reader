using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieReader.View
{
    public class CanPromptDialog : Form
    {
        private TextBox inputBox;
        public string CanCode => inputBox.Text;
        public CanPromptDialog()
        {
            Text = "Inserisci il codice CAN";
            Width = 300;
            Height = 150;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            Label prompt = new Label() { Left = 10, Top = 10, Text = "Codice CAN:", Width = 260 };
            inputBox = new TextBox() { Left = 10, Top = 35, Width = 260, MaxLength = 6 };

            Button confirmation = new Button() { Text = "OK", Left = 110, Width = 75, Top = 70, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "Annulla", Left = 195, Width = 75, Top = 70, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { DialogResult = DialogResult.OK; Close(); };
            cancel.Click += (sender, e) => { DialogResult = DialogResult.Cancel; Close(); };

            Controls.Add(prompt);
            Controls.Add(inputBox);
            Controls.Add(confirmation);
            Controls.Add(cancel);

            AcceptButton = confirmation;
            CancelButton = cancel;
        }
    }
}
