using System;

namespace EncryText
{
    public partial class Form : MetroFramework.Forms.MetroForm
    {
        public Form()
        {
            InitializeComponent();
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            string pri, pub;
            RetChen.Encryption.RSAEncryption.RandomKey(out pri, out pub, int.Parse(keyLength.Text));
            this.privateKey.Text = pri;
            this.publicKey.Text = pub;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var en = new RetChen.Encryption.RSAEncryption(this.privateKey.Text, this.publicKey.Text);
            this.decryString.Text = en.Encrypt(this.encryString.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var de = new RetChen.Encryption.RSAEncryption(this.privateKey.Text, this.publicKey.Text);
            this.encryString.Text = de.Decrypt(this.decryString.Text);
        }
    }
}
