using System.Data;
using Microsoft.Data.SqlClient;

namespace MuzikPlatformuApp;

public sealed class MainForm : Form
{
    private readonly int userId;
    private readonly string username;

    private readonly DataGridView songsGrid = new();
    private readonly DataGridView playlistsGrid = new();
    private readonly DataGridView logsGrid = new();

    public MainForm(int userId, string username)
    {
        this.userId = userId;
        this.username = username;

        Text = $"Muzik Platformu - {username}";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 680);

        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        var header = new Label
        {
            Text = $"Hos geldin, {username}",
            Font = new Font(FontFamily.GenericSansSerif, 13, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(15, 15)
        };

        songsGrid.Location = new Point(15, 55);
        songsGrid.Size = new Size(690, 300);
        songsGrid.ReadOnly = true;
        songsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        songsGrid.MultiSelect = false;
        songsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        playlistsGrid.Location = new Point(725, 55);
        playlistsGrid.Size = new Size(350, 300);
        playlistsGrid.ReadOnly = true;
        playlistsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        playlistsGrid.MultiSelect = false;
        playlistsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        logsGrid.Location = new Point(15, 420);
        logsGrid.Size = new Size(1060, 230);
        logsGrid.ReadOnly = true;
        logsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        var refreshButton = CreateButton("Yenile", 15, 365, RefreshButton_Click);
        var listenButton = CreateButton("35sn Dinle", 125, 365, ListenButton_Click);
        var likeButton = CreateButton("Sarkiyi Begen", 235, 365, LikeButton_Click);
        var createPlaylistButton = CreateButton("Playlist Olustur", 385, 365, CreatePlaylistButton_Click);
        var addSongButton = CreateButton("Playlist'e Ekle", 545, 365, AddSongButton_Click);
        var removeSongButton = CreateButton("Playlist'ten Cikar", 705, 365, RemoveSongButton_Click);
        var playlistSongsButton = CreateButton("Playlist Icerigi", 865, 365, PlaylistSongsButton_Click);
        var logsButton = CreateButton("Loglari Goster", 15, 390, LogsButton_Click);
        var renamePlaylistButton = CreateButton("Playlist Adi Degistir", 155, 390, RenamePlaylistButton_Click);
        var logoutButton = CreateButton("Cikis Yap", 315, 390, LogoutButton_Click);

        Controls.AddRange([
            header,
            songsGrid,
            playlistsGrid,
            logsGrid,
            refreshButton,
            listenButton,
            likeButton,
            createPlaylistButton,
            addSongButton,
            removeSongButton,
            logsButton,
            playlistSongsButton,
            renamePlaylistButton,
            logoutButton
        ]);
    }

    private static Button CreateButton(string text, int x, int y, EventHandler clickHandler)
    {
        var button = new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(135, 28)
        };
        button.Click += clickHandler;
        return button;
    }

    private void LoadData()
    {
        songsGrid.DataSource = Db.Query("""
            SELECT
                g.sarkiID,
                g.sarkiAdi,
                d.yayinAdi,
                d.yayinTuru,
                g.gecerliDinlenmeSayisi
            FROM vw_GecerliDinlenmeSayilari g
            LEFT JOIN vw_SarkiDetaylari d ON g.sarkiID = d.sarkiID
            GROUP BY
                g.sarkiID,
                g.sarkiAdi,
                d.yayinAdi,
                d.yayinTuru,
                g.gecerliDinlenmeSayisi
            ORDER BY gecerliDinlenmeSayisi DESC, g.sarkiAdi;
            """);

        playlistsGrid.DataSource = Db.Query(
            """
            SELECT playlistID, playlistAdi, herkeseAcikMi, olusturulmaTarihi
            FROM Playlistler
            WHERE kullaniciID = @kullaniciID
            ORDER BY olusturulmaTarihi DESC;
            """,
            new SqlParameter("@kullaniciID", userId));
    }

    private int? SelectedSongId()
    {
        return SelectedInt(songsGrid, "sarkiID");
    }

    private int? SelectedPlaylistId()
    {
        return SelectedInt(playlistsGrid, "playlistID");
    }

    private int? SelectedLogSongId()
    {
        return SelectedInt(logsGrid, "sarkiID");
    }

    private static int? SelectedInt(DataGridView grid, string columnName)
    {
        if (grid.CurrentRow == null)
        {
            return null;
        }

        var value = grid.CurrentRow.Cells[columnName].Value;
        return value == DBNull.Value ? null : Convert.ToInt32(value);
    }

    private void RefreshButton_Click(object? sender, EventArgs e)
    {
        LoadData();
    }

    private void LikeButton_Click(object? sender, EventArgs e)
    {
        var songId = SelectedSongId();
        if (songId == null)
        {
            MessageBox.Show("Once bir sarki sec.");
            return;
        }

        try
        {
            Db.ExecuteStoredProcedure(
                "sp_SarkiBegen",
                new SqlParameter("@kullaniciID", userId),
                new SqlParameter("@sarkiID", songId.Value));

            MessageBox.Show("Sarki begenildi.");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Islem Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void ListenButton_Click(object? sender, EventArgs e)
    {
        var songId = SelectedSongId();
        if (songId == null)
        {
            MessageBox.Show("Once bir sarki sec.");
            return;
        }

        try
        {
            var endTime = DateTime.Now;
            var startTime = endTime.AddMilliseconds(-35000);

            Db.Query(
                """
                INSERT INTO DinlemeGecmisi
                    (kullaniciID, sarkiID, dinlemeBaslangicZamani, dinlemeBitisZamani, dinlenenMilisaniye)
                VALUES
                    (@kullaniciID, @sarkiID, @baslangic, @bitis, 35000);
                """,
                new SqlParameter("@kullaniciID", userId),
                new SqlParameter("@sarkiID", songId.Value),
                new SqlParameter("@baslangic", startTime),
                new SqlParameter("@bitis", endTime));

            MessageBox.Show("35 saniyelik dinleme kaydi eklendi. Gecerli dinlenme sayisi artmali.");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Dinleme Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void CreatePlaylistButton_Click(object? sender, EventArgs e)
    {
        var playlistName = Prompt.Show("Playlist adi", "Yeni Playlist");
        if (string.IsNullOrWhiteSpace(playlistName))
        {
            return;
        }

        try
        {
            Db.StoredProcedure(
                "sp_PlaylistOlustur",
                new SqlParameter("@kullaniciID", userId),
                new SqlParameter("@playlistAdi", playlistName.Trim()),
                new SqlParameter("@aciklama", DBNull.Value),
                new SqlParameter("@herkeseAcikMi", true));

            MessageBox.Show("Playlist olusturuldu.");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Islem Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RenamePlaylistButton_Click(object? sender, EventArgs e)
    {
        var playlistId = SelectedPlaylistId();
        if (playlistId == null)
        {
            MessageBox.Show("Once bir playlist sec.");
            return;
        }

        var newName = Prompt.Show("Yeni playlist adi", "Playlist Adi Degistir");
        if (string.IsNullOrWhiteSpace(newName))
        {
            return;
        }

        try
        {
            Db.Query(
                """
                UPDATE Playlistler
                SET playlistAdi = @playlistAdi
                WHERE playlistID = @playlistID
                  AND kullaniciID = @kullaniciID;
                """,
                new SqlParameter("@playlistAdi", newName.Trim()),
                new SqlParameter("@playlistID", playlistId.Value),
                new SqlParameter("@kullaniciID", userId));

            MessageBox.Show("Playlist adi degistirildi.");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Islem Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void AddSongButton_Click(object? sender, EventArgs e)
    {
        var songId = SelectedSongId();
        var playlistId = SelectedPlaylistId();

        if (songId == null || playlistId == null)
        {
            MessageBox.Show("Bir sarki ve bir playlist sec.");
            return;
        }

        try
        {
            Db.ExecuteStoredProcedure(
                "sp_PlaylisteSarkiEkle",
                new SqlParameter("@playlistID", playlistId.Value),
                new SqlParameter("@sarkiID", songId.Value),
                new SqlParameter("@siraNo", DBNull.Value));

            MessageBox.Show("Sarki playliste eklendi.");
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Islem Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void LogsButton_Click(object? sender, EventArgs e)
    {
        logsGrid.DataSource = Db.Query("""
            SELECT TOP 50
                playlistSarkiLogID AS logID,
                playlistAdi,
                sarkiAdi,
                islemTuru,
                islemTarihi
            FROM PlaylistSarkiLoglari
            ORDER BY islemTarihi DESC;
            """);
    }

    private void RemoveSongButton_Click(object? sender, EventArgs e)
    {
        var playlistId = SelectedPlaylistId();
        var songId = SelectedLogSongId() ?? SelectedSongId();

        if (playlistId == null || songId == null)
        {
            MessageBox.Show("Bir playlist ve cikarilacak sarkiyi sec. Playlist iceriginden sarki secersen daha net olur.");
            return;
        }

        try
        {
            Db.Query(
                """
                DELETE FROM PlaylistSarkilari
                WHERE playlistID = @playlistID
                  AND sarkiID = @sarkiID;
                """,
                new SqlParameter("@playlistID", playlistId.Value),
                new SqlParameter("@sarkiID", songId.Value));

            MessageBox.Show("Sarki playlistten cikarildi.");
            LoadData();
            ShowPlaylistSongs(playlistId.Value);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Islem Hatasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void PlaylistSongsButton_Click(object? sender, EventArgs e)
    {
        var playlistId = SelectedPlaylistId();
        if (playlistId == null)
        {
            MessageBox.Show("Once bir playlist sec.");
            return;
        }

        ShowPlaylistSongs(playlistId.Value);
    }

    private void LogoutButton_Click(object? sender, EventArgs e)
    {
        Close();
    }

    private void ShowPlaylistSongs(int playlistId)
    {
        logsGrid.DataSource = Db.Query(
            """
            SELECT
                playlistID,
                playlistAdi,
                sarkiID,
                sarkiAdi,
                siraNo,
                eklenmeTarihi
            FROM vw_PlaylistDetaylari
            WHERE playlistID = @playlistID
            ORDER BY
                CASE WHEN siraNo IS NULL THEN 1 ELSE 0 END,
                siraNo,
                eklenmeTarihi;
            """,
            new SqlParameter("@playlistID", playlistId));
    }
}
