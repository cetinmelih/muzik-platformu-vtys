using Microsoft.Data.SqlClient;

namespace MuzikPlatformuApp;

public sealed class LoginForm : Form
{
    private readonly TextBox emailTextBox = new();
    private readonly TextBox passwordTextBox = new();
    private readonly Button loginButton = new();
    private readonly Button registerButton = new();

    public LoginForm()
    {
        Text = "Muzik Platformu - Giris";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        ClientSize = new Size(360, 250);

        var titleLabel = new Label
        {
            Text = "Muzik Platformu",
            Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(95, 20)
        };

        var emailLabel = new Label { Text = "Email", AutoSize = true, Location = new Point(35, 72) };
        emailTextBox.Location = new Point(120, 68);
        emailTextBox.Width = 210;

        var passwordLabel = new Label { Text = "Sifre", AutoSize = true, Location = new Point(35, 112) };
        passwordTextBox.Location = new Point(120, 108);
        passwordTextBox.Width = 210;
        passwordTextBox.UseSystemPasswordChar = true;

        loginButton.Text = "Giris Yap";
        loginButton.Location = new Point(120, 150);
        loginButton.Width = 210;
        loginButton.Click += LoginButton_Click;

        registerButton.Text = "Kayit Ol";
        registerButton.Location = new Point(120, 190);
        registerButton.Width = 210;
        registerButton.Click += RegisterButton_Click;

        Controls.AddRange([titleLabel, emailLabel, emailTextBox, passwordLabel, passwordTextBox, loginButton, registerButton]);
    }

    private void LoginButton_Click(object? sender, EventArgs e)
    {
        try
        {
            var result = Db.StoredProcedure(
                "sp_KullaniciGiris",
                new SqlParameter("@kullaniciEmail", emailTextBox.Text.Trim()),
                new SqlParameter("@kullaniciSifreHash", passwordTextBox.Text.Trim()));

            if (result.Rows.Count == 0)
            {
                MessageBox.Show("Email veya sifre hatali.", "Giris Basarisiz", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var userId = Convert.ToInt32(result.Rows[0]["kullaniciID"]);
            var username = Convert.ToString(result.Rows[0]["kullaniciAdi"]) ?? "";

            Hide();
            using (var mainForm = new MainForm(userId, username))
            {
                mainForm.ShowDialog();
            }

            passwordTextBox.Clear();
            Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Baglanti/SQL Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RegisterButton_Click(object? sender, EventArgs e)
    {
        using var form = new RegisterForm();
        form.ShowDialog(this);
    }
}
