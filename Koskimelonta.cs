using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Controls;

/// @author samib
/// @version 23.09.2026
/// <summary>
/// Koskimelontapeli, jossa pelaaja ohjaa kanoottia
/// ja väistelee kosken kiviä.
/// </summary>
public class Koskimelonta : Game
{
    private GameObject kanootti;
    private GameObject meloja;
    private GameObject mela;

    private Timer kivenLuontiAjastin;
    private Timer kaislanLuontiAjastin;
    private Timer pelinAjastin;

    private Label matkaTeksti;
    private Label nopeusTeksti;

    private List<GameObject> kivet = new List<GameObject>();
    private List<GameObject> kaislat = new List<GameObject>();
    private List<GameObject> aallot = new List<GameObject>();

    private double matka;
    private double nopeus = 180;
    private double hidastus = 1.0;
    private bool peliOhi;


    public override void Begin()
    {
        LuoKentta();
        LuoKanootti();
        LuoMittarit();
        LuoOhjaimet();
        LuoAallot();
        AloitaPeli();

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Luo joen ja rannat.
    /// </summary>
    private void LuoKentta()
    {
        Level.Background.Color = Color.Green;

        // Sininen joki.
        GameObject joki = new GameObject(300, 1000);
        joki.Color = Color.Blue;
        joki.X = 0;
        joki.Y = 0;
        Add(joki);

        // Vasemman rannan maa.
        GameObject rantaVasen = new GameObject(120, 1000);
        rantaVasen.Color = Color.Green;
        rantaVasen.X = -210;
        rantaVasen.Y = 0;
        Add(rantaVasen);

        // Oikean rannan maa.
        GameObject rantaOikea = new GameObject(120, 1000);
        rantaOikea.Color = Color.Green;
        rantaOikea.X = 210;
        rantaOikea.Y = 0;
        Add(rantaOikea);

        // Joen mutkia vaaleammilla vesialueilla.
        LuoMutka(-55, 300);
        LuoMutka(45, 650);
        LuoMutka(-40, 1000);

        Camera.ZoomToLevel();
    }


    /// <summary>
    /// Luo joen mutkaan leveämmän vesialueen.
    /// </summary>
    /// <param name="x">Mutkan vaakasijainti.</param>
    /// <param name="y">Mutkan pystysijainti.</param>
    private void LuoMutka(double x, double y)
    {
        GameObject mutka = new GameObject(180, 250);
        mutka.Color = Color.Blue;
        mutka.Shape = Shape.Ellipse;
        mutka.X = x;
        mutka.Y = y;

        Add(mutka);
    }


    /// <summary>
    /// Luo kanootin, melojan ja melan.
    /// </summary>
    private void LuoKanootti()
    {
        kanootti = new GameObject(45, 85);
        kanootti.Shape = Shape.Ellipse;
        kanootti.Color = Color.Red;
        kanootti.X = 0;
        kanootti.Y = -250;
        Add(kanootti);

        meloja = new GameObject(18, 28);
        meloja.Shape = Shape.Ellipse;
        meloja.Color = Color.Yellow;
        Add(meloja);

        mela = new GameObject(6, 70);
        mela.Color = Color.Brown;
        mela.Angle = Angle.FromDegrees(25);
        Add(mela);

        PaivitaKanootti();
    }


    /// <summary>
    /// Päivittää melojan ja melan sijainnin kanootin suhteen.
    /// </summary>
    private void PaivitaKanootti()
    {
        meloja.X = kanootti.X;
        meloja.Y = kanootti.Y + 5;

        mela.X = kanootti.X + 27;
        mela.Y = kanootti.Y;
    }


    /// <summary>
    /// Luo näytön mittarit.
    /// </summary>
    private void LuoMittarit()
    {
        matkaTeksti = new Label();
        matkaTeksti.Text = "Matka: 0 m";
        matkaTeksti.TextColor = Color.White;
        matkaTeksti.X = Screen.Left + 70;
        matkaTeksti.Y = Screen.Top - 35;
        Add(matkaTeksti);

        nopeusTeksti = new Label();
        nopeusTeksti.Text = "Vauhti: 180";
        nopeusTeksti.TextColor = Color.White;
        nopeusTeksti.X = Screen.Right - 70;
        nopeusTeksti.Y = Screen.Top - 35;
        Add(nopeusTeksti);
    }


    /// <summary>
    /// Luo näppäinohjauksen.
    /// </summary>
    private void LuoOhjaimet()
    {
        Keyboard.Listen(
            Key.Left,
            ButtonState.Down,
            LiikutaKanoottia,
            "Liiku vasemmalle",
            -1);

        Keyboard.Listen(
            Key.Right,
            ButtonState.Down,
            LiikutaKanoottia,
            "Liiku oikealle",
            1);

        Keyboard.Listen(
            Key.R,
            ButtonState.Pressed,
            AloitaUusiPeli,
            "Aloita peli uudelleen");
    }


    /// <summary>
    /// Liikuttaa kanoottia.
    /// </summary>
    /// <param name="suunta">Liikkeen suunta.</param>
    private void LiikutaKanoottia(int suunta)
    {
        if (peliOhi)
            return;

        kanootti.X += suunta * 8 * hidastus;

        if (kanootti.Left < -130)
            kanootti.X = -110;

        if (kanootti.Right > 130)
            kanootti.X = 110;

        PaivitaKanootti();
    }


    /// <summary>
    /// Luo koskeen näkyviä aaltoja.
    /// </summary>
    private void LuoAallot()
    {
        for (int i = 0; i < 20; i++)
        {
            GameObject aalto = new GameObject(45, 8);
            aalto.Shape = Shape.Ellipse;
            aalto.Color = Color.White;

            aalto.X = RandomGen.NextDouble(-110, 110);
            aalto.Y = RandomGen.NextDouble(-450, 500);

            aallot.Add(aalto);
            Add(aalto);
        }
    }


    /// <summary>
    /// Käynnistää pelin ajastimet.
    /// </summary>
    private void AloitaPeli()
    {
        kivenLuontiAjastin = new Timer();
        kivenLuontiAjastin.Interval = 1.2;
        kivenLuontiAjastin.Timeout += LuoKivi;
        kivenLuontiAjastin.Start();

        kaislanLuontiAjastin = new Timer();
        kaislanLuontiAjastin.Interval = 2.5;
        kaislanLuontiAjastin.Timeout += LuoKaisla;
        kaislanLuontiAjastin.Start();

        pelinAjastin = new Timer();
        pelinAjastin.Interval = 0.05;
        pelinAjastin.Timeout += PaivitaPeli;
        pelinAjastin.Start();
    }


    /// <summary>
    /// Luo kiven.
    /// </summary>
    private void LuoKivi()
    {
        if (peliOhi)
            return;

        double koko = RandomGen.NextDouble(40, 70);

        GameObject kivi = new GameObject(koko, koko);
        kivi.Shape = Shape.Circle;
        kivi.Color = Color.Gray;

        kivi.X = RandomGen.NextDouble(-110, 110);
        kivi.Y = Screen.Top + 50;

        kivet.Add(kivi);
        Add(kivi);
    }


    /// <summary>
    /// Luo kolme kaislaa.
    /// </summary>
    private void LuoKaisla()
    {
        if (peliOhi)
            return;

        for (int i = 0; i < 3; i++)
        {
            GameObject kaisla = new GameObject(8, 55);
            kaisla.Color = Color.Green;

            kaisla.X = RandomGen.NextDouble(-145, 145);
            kaisla.Y = Screen.Top + 50;

            kaislat.Add(kaisla);
            Add(kaisla);
        }
    }


    /// <summary>
    /// Päivittää pelin tapahtumat.
    /// </summary>
    private void PaivitaPeli()
    {
        if (peliOhi)
            return;

        matka += 0.05;
        nopeus += 0.02;

        matkaTeksti.Text =
            "Matka: " + Math.Floor(matka) + " m";

        nopeusTeksti.Text =
            "Vauhti: " + Math.Floor(nopeus);

        PaivitaKivet();
        PaivitaKaislat();
        PaivitaAallot();

        if (hidastus < 1.0)
        {
            hidastus += 0.01;

            if (hidastus > 1.0)
                hidastus = 1.0;
        }
    }


    /// <summary>
    /// Liikuttaa kiviä ja tarkistaa törmäykset.
    /// </summary>
    private void PaivitaKivet()
    {
        foreach (GameObject kivi in kivet)
        {
            kivi.Y -= nopeus * 0.05;

            if (OnkoTormays(kivi))
            {
                PeliOhi();
                return;
            }
        }
    }


    /// <summary>
    /// Liikuttaa kaisloja ja hidastaa kanoottia.
    /// </summary>
    private void PaivitaKaislat()
    {
        foreach (GameObject kaisla in kaislat)
        {
            kaisla.Y -= nopeus * 0.05;

            if (OnkoTormays(kaisla))
                hidastus = 0.35;
        }
    }


    /// <summary>
    /// Liikuttaa aaltoja alaspäin.
    /// </summary>
    private void PaivitaAallot()
    {
        foreach (GameObject aalto in aallot)
        {
            aalto.Y -= nopeus * 0.05;

            if (aalto.Y < Screen.Bottom - 50)
                aalto.Y = Screen.Top + 50;
        }
    }


    /// <summary>
    /// Tarkistaa osuuko este kanoottiin.
    /// </summary>
    /// <param name="este">Tarkistettava este.</param>
    /// <returns>True, jos este on tarpeeksi lähellä.</returns>
    private bool OnkoTormays(GameObject este)
    {
        double etaisyys = Vector.Distance(
            kanootti.Position,
            este.Position);

        return etaisyys < 45;
    }


    /// <summary>
    /// Lopettaa pelin.
    /// </summary>
    private void PeliOhi()
    {
        if (peliOhi)
            return;

        peliOhi = true;

        kivenLuontiAjastin.Stop();
        kaislanLuontiAjastin.Stop();
        pelinAjastin.Stop();

        MessageDisplay.Add(
            "PELI OHI! Matka: " +
            Math.Floor(matka) +
            " m");

        MessageDisplay.Add(
            "Paina R aloittaaksesi uudelleen.");
    }


    /// <summary>
    /// Aloittaa uuden pelikierroksen.
    /// </summary>
    private void AloitaUusiPeli()
    {
        if (!peliOhi)
            return;

        ClearAll();

        kivet.Clear();
        kaislat.Clear();
        aallot.Clear();

        matka = 0;
        nopeus = 180;
        hidastus = 1.0;
        peliOhi = false;

        LuoKentta();
        LuoKanootti();
        LuoMittarit();
        LuoOhjaimet();
        LuoAallot();
        AloitaPeli();
    }
}