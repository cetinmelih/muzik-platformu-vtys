using Microsoft.Data.SqlClient;

const string connectionString =
    @"Server=.\SQLEXPRESS;Database=MuzikPlatformuDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

var sqlStatements = new[]
{
    """
    INSERT INTO Ulkeler (ulkeAdi)
    SELECT v.ulkeAdi
    FROM (VALUES
        (N'Almanya'), (N'İspanya'), (N'İsveç')
    ) v(ulkeAdi)
    WHERE NOT EXISTS (
        SELECT 1 FROM Ulkeler u WHERE u.ulkeAdi = v.ulkeAdi
    );
    """,

    """
    INSERT INTO Cinsiyetler (cinsiyetID, cinsiyetAdi)
    SELECT v.cinsiyetID, v.cinsiyetAdi
    FROM (VALUES
        (4, N'Diğer')
    ) v(cinsiyetID, cinsiyetAdi)
    WHERE NOT EXISTS (
        SELECT 1 FROM Cinsiyetler c WHERE c.cinsiyetID = v.cinsiyetID
    );
    """,

    """
    INSERT INTO Kullanicilar
        (kullaniciAdi, kullaniciEmail, kullaniciSifreHash, kullaniciDogumTarihi, ulkeID, cinsiyetID)
    SELECT v.kullaniciAdi, v.email, N'123456', v.dogumTarihi, u.ulkeID, v.cinsiyetID
    FROM (VALUES
        (N'hans',   N'hans@mail.com',   '1996-08-10', N'Almanya', 1),
        (N'claire', N'claire@mail.com', '2000-04-15', N'Fransa', 2),
        (N'erik',   N'erik@mail.com',   '1995-12-02', N'İsveç', 1)
    ) v(kullaniciAdi, email, dogumTarihi, ulkeAdi, cinsiyetID)
    INNER JOIN Ulkeler u ON u.ulkeAdi = v.ulkeAdi
    WHERE NOT EXISTS (
        SELECT 1 FROM Kullanicilar k WHERE k.kullaniciEmail = v.email
    );
    """,

    """
    INSERT INTO Artistler
        (artistAdi, artistTipi, baslangicTarihi, bitisTarihi, ulkeID, biyografi, profilResmiUrl)
    SELECT v.artistAdi, v.artistTipi, v.baslangicTarihi, NULL, u.ulkeID, NULL, v.profilResmiUrl
    FROM (VALUES
        (N'Rammstein', N'Grup', '1994-01-01', N'Almanya', N'https://example.com/rammstein.jpg'),
        (N'Stromae',   N'Solo', '1985-03-12', N'Fransa', N'https://example.com/stromae.jpg'),
        (N'ABBA',      N'Grup', '1972-01-01', N'İsveç', N'https://example.com/abba.jpg')
    ) v(artistAdi, artistTipi, baslangicTarihi, ulkeAdi, profilResmiUrl)
    INNER JOIN Ulkeler u ON u.ulkeAdi = v.ulkeAdi
    WHERE NOT EXISTS (
        SELECT 1 FROM Artistler a WHERE a.artistAdi = v.artistAdi
    );
    """,

    """
    INSERT INTO Yayinlar
        (yayinAdi, yayinTuru, yayinTarihi, kapakResmiUrl, aciklama)
    SELECT v.yayinAdi, v.yayinTuru, v.yayinTarihi, v.kapakResmiUrl, NULL
    FROM (VALUES
        (N'Mutter', N'Album', '2001-04-02', N'https://example.com/mutter.jpg'),
        (N'Arrival', N'Album', '1976-10-11', N'https://example.com/arrival.jpg')
    ) v(yayinAdi, yayinTuru, yayinTarihi, kapakResmiUrl)
    WHERE NOT EXISTS (
        SELECT 1 FROM Yayinlar y WHERE y.yayinAdi = v.yayinAdi
    );
    """,

    """
    INSERT INTO YayinArtistleri (yayinID, artistID)
    SELECT y.yayinID, a.artistID
    FROM (VALUES
        (N'Mutter', N'Rammstein'),
        (N'Arrival', N'ABBA')
    ) v(yayinAdi, artistAdi)
    INNER JOIN Yayinlar y ON y.yayinAdi = v.yayinAdi
    INNER JOIN Artistler a ON a.artistAdi = v.artistAdi
    WHERE NOT EXISTS (
        SELECT 1 FROM YayinArtistleri ya
        WHERE ya.yayinID = y.yayinID AND ya.artistID = a.artistID
    );
    """,

    """
    INSERT INTO Etiketler (etiketAdi)
    SELECT v.etiketAdi
    FROM (VALUES
        (N'Metal'), (N'Dance-Pop'), (N'Eurovision')
    ) v(etiketAdi)
    WHERE NOT EXISTS (
        SELECT 1 FROM Etiketler e WHERE e.etiketAdi = v.etiketAdi
    );
    """,

    """
    INSERT INTO Playlistler
        (kullaniciID, playlistAdi, aciklama, herkeseAcikMi)
    SELECT k.kullaniciID, v.playlistAdi, NULL, v.herkeseAcikMi
    FROM (VALUES
        (N'hans@mail.com',   N'Metal Seçkisi',    CAST(1 AS BIT)),
        (N'claire@mail.com', N'Fransız Esintisi', CAST(1 AS BIT)),
        (N'erik@mail.com',   N'Kuzey Pop',        CAST(1 AS BIT)),
        (N'elif@mail.com',   N'Ders Çalışma',     CAST(0 AS BIT)),
        (N'burak@mail.com',  N'Gece Yürüyüşü',    CAST(1 AS BIT)),
        (N'melih@mail.com',  N'Sunum Test Listesi', CAST(1 AS BIT))
    ) v(email, playlistAdi, herkeseAcikMi)
    INNER JOIN Kullanicilar k ON k.kullaniciEmail = v.email
    WHERE NOT EXISTS (
        SELECT 1 FROM Playlistler p
        WHERE p.kullaniciID = k.kullaniciID
          AND p.playlistAdi = v.playlistAdi
    );
    """
};

using var connection = new SqlConnection(connectionString);
connection.Open();

foreach (var sql in sqlStatements)
{
    using var command = new SqlCommand(sql, connection);
    command.ExecuteNonQuery();
}

using var countCommand = new SqlCommand(
    """
    SELECT 'Ulkeler' AS tablo, COUNT(*) AS adet FROM Ulkeler
    UNION ALL SELECT 'Cinsiyetler', COUNT(*) FROM Cinsiyetler
    UNION ALL SELECT 'Kullanicilar', COUNT(*) FROM Kullanicilar
    UNION ALL SELECT 'Artistler', COUNT(*) FROM Artistler
    UNION ALL SELECT 'Yayinlar', COUNT(*) FROM Yayinlar
    UNION ALL SELECT 'YayinArtistleri', COUNT(*) FROM YayinArtistleri
    UNION ALL SELECT 'Sarkilar', COUNT(*) FROM Sarkilar
    UNION ALL SELECT 'SarkiArtistleri', COUNT(*) FROM SarkiArtistleri
    UNION ALL SELECT 'Etiketler', COUNT(*) FROM Etiketler
    UNION ALL SELECT 'SarkiEtiketleri', COUNT(*) FROM SarkiEtiketleri
    UNION ALL SELECT 'Playlistler', COUNT(*) FROM Playlistler
    UNION ALL SELECT 'PlaylistSarkilari', COUNT(*) FROM PlaylistSarkilari
    UNION ALL SELECT 'Begeniler', COUNT(*) FROM Begeniler
    UNION ALL SELECT 'DinlemeGecmisi', COUNT(*) FROM DinlemeGecmisi
    UNION ALL SELECT 'SarkiLoglari', COUNT(*) FROM SarkiLoglari
    UNION ALL SELECT 'PlaylistSarkiLoglari', COUNT(*) FROM PlaylistSarkiLoglari;
    """,
    connection);

using var reader = countCommand.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"{reader.GetString(0),-25} {reader.GetInt32(1)}");
}
