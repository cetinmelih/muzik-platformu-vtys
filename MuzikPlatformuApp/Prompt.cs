namespace MuzikPlatformuApp;

internal static class Prompt
{
    public static string? Show(string labelText, string title)
    {
        using var form = new Form
        {
            Text = title,
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = new Size(360, 140),
            MinimizeBox = false,
            MaximizeBox = false
        };

        var label = new Label
        {
            Text = labelText,
            Location = new Point(20, 20),
            AutoSize = true
        };

        var textBox = new TextBox
        {
            Location = new Point(20, 50),
            Width = 320
        };

        var okButton = new Button
        {
            Text = "Tamam",
            DialogResult = DialogResult.OK,
            Location = new Point(180, 90),
            Width = 75
        };

        var cancelButton = new Button
        {
            Text = "Iptal",
            DialogResult = DialogResult.Cancel,
            Location = new Point(265, 90),
            Width = 75
        };

        form.Controls.AddRange([label, textBox, okButton, cancelButton]);
        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;

        return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
    }
}
