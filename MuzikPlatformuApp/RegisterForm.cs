using Microsoft.Data.SqlClient;

namespace MuzikPlatformuApp;

public sealed class RegisterForm : Form
{
    private readonly TextBox usernameTextBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly TextBox passwordTextBox = new();

    public RegisterForm()
    {
        Text = "Kayit Ol";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize = new Size(380, 240);
        MaximizeBox = false;
        MinimizeBox = false;

        var usernameLabel = new Label { Text = "Kullanici adi", AutoSize = true, Location = new Point(25, 35) };
        usernameTextBox.Location = new Point(140, 31);
        usernameTextBox.Width = 220;

        var emailLabel = new Label { Text = "Email", AutoSize = true, Location = new Point(25, 75) };
        emailTextBox.Location = new Point(140, 71);
        emailTextBox.Width = 220;

        var passwordLabel = new Label { Text = "Sifre", AutoSize = true, Location = new Point(25, 115) };
        passwordTextBox.Location = new Point(140, 111);
        passwordTextBox.Width = 220;
        passwordTextBox.UseSystemPasswordChar = true;

        var saveButton = new Button
        {
            Text = "Kaydet",
            Location = new Point(140, 160),
            Width = 100
        };
        saveButton.Click += SaveButton_Click;

        var cancelButton = new Button
        {
            Text = "Iptal",
            Location = new Point(260, 160),
            Width = 100
        };
        cancelButton.Click += (_, _) => Close();

        Controls.AddRange([usernameLabel, usernameTextBox, emailLabel, emailTextBox, passwordLabel, passwordTextBox, saveButton, cancelButton]);
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        var username = usernameTextBox.Text.Trim();
        var email = emailTextBox.Text.Trim();
        var password = passwordTextBox.Text.Trim();

        if (username.Length == 0 || email.Length == 0 || password.Length == 0)
        {
            MessageBox.Show("Kullanici adi, email ve sifre bos olamaz.");
            return;
        }

        try
        {
            Db.Query(
                """
                INSERT INTO Kullanicilar
                    (kullaniciAdi, kullaniciEmail, kullaniciSifreHash)
                VALUES
                    (@kullaniciAdi, @kullaniciEmail, @kullaniciSifreHash);
                """,
                new SqlParameter("@kullaniciAdi", username),
                new SqlParameter("@kullaniciEmail", email),
                new SqlParameter("@kullaniciSifreHash", password));

            MessageBox.Show("Kayit olusturuldu. Giris yapabilirsin.");
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Kayit Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
