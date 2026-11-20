using Jypeli;

using System.Collections.Generic;

using System;

namespace Koskimelonta
{
    /// <summary>
    /// Koskimelonta-peli, jossa väistellään kiviä joessa.
    /// </summary>
    /// <author>Sami Tuohimaa</author>
    /// <version>1.0</version>

    
    public class Peli : Game
    {
        /// <summary>
        /// Arpoo kiville satunnaisia sijainteja.
        /// </summary>
        private Random _satunnaisluku = new Random();

        
        /// <summary>
        /// Kiven liikkumisnopeus pelissä.
        /// </summary>
        private double _kivenNopeus = 4;
        
        
        /// <summary>
        /// Aika uuden kiven luomiseen sekunteina.
        /// </summary>
        private double _kivenLuontivali = 2;
        
        
        /// <summary>
        /// Pelissä kuljettu matka.
        /// </summary>
        private double _matka;
        
        
        /// <summary>
        /// Pienin kiven koko.
        /// </summary>
        private const int KivenMinimiKoko = 45;

        
        /// <summary>
        /// Suurinta kiven kokoa seuraava arvo.
        /// </summary>
        private const int KivenMaksimiKoko = 76;
        
        
        /// <summary>
        /// Kiven pienin mahdollinen vaakasijainti.
        /// </summary>
        private const int KivenMinimiX = -250;

        
        /// <summary>
        /// Kiven suurinta vaakasijaintia seuraava arvo.
        /// </summary>
        private const int KivenMaksimiX = 251;
        
        
        /// <summary>
        /// Kahden kiven välinen pienin sallittu vaaka etäisyys.
        /// </summary>
        private const double KivienMinimietaisyys = 100;
        
        
        /// <summary>
        /// Kuljetun matkan lisäys.
        /// </summary>
        private const double MatkanLisays = 0.1;
        
        
        /// <summary>
        /// Kiven nopeuden lisäys seuraavalla vaikeustasolla.
        /// </summary>
        private const double KivenNopeudenLisays = 0.5;
        
        
        /// <summary>
        /// Kivien maksimi liikkumisnopeus.
        /// </summary>
        private const double KivenMaksiminopeus = 15;
        
        
        /// <summary>
        /// Kivien minimi luontiväli sekunteina.
        /// </summary>
        private const double KivenMinimiLuontivali = 0.3;
        
        
        /// <summary>
        /// Kivien luontivälin pieneneminen vaikeustason kasvaessa.
        /// </summary>
        private const double KivenLuontivalinLyhennys = 0.05;
        
        
        /// <summary>
        /// Seuraava matka, kun vaikeustaso kasvaa.
        /// </summary>
        private double _seuraavaVaikeustaso = VaikeustasonNousuvali;
        
        
        /// <summary>
        /// Matkan, jonka välein vaikeustaso kasvaa.
        /// </summary>
        private const double VaikeustasonNousuvali = 20;
        
        
        /// <summary>
        /// Kivien liikuttamisesta vastaava ajastin.
        /// </summary>
        private Timer _liikeTimer;

        
        /// <summary>
        /// Uusien kivien luomisesta vastaava ajastin.
        /// </summary>
        private Timer _kiviTimer;

        
        /// <summary>
        /// Joen ylä- ja alarajan etäisyys keskeltä.
        /// </summary>
        private const double JoenRaja = 300;
        
        
        /// <summary>
        /// Kajakin puolikkaan leveys.
        /// </summary>
        private const double KajakinPuolikasLeveys = 25;
        

        /// <summary>
        /// Kajakin puolikkaan korkeus.
        /// </summary>
        private const double KajakinPuolikasKorkeus = 45;
        
        
        /// <summary>
        /// Lista pelin kivistä.
        /// </summary>
        private List<GameObject> _kivet = new List<GameObject>();

        
        /// <summary>
        /// Pelaajan ohjaama kajakki.
        /// </summary>
        private GameObject _kajakki;
        
        
        /// <summary>
        /// Kertoo, onko peli päättynyt.
        /// </summary>
        private bool _peliPaattynyt;

        
        /// <summary>
        /// Kuljetun matkan näyttävä tekstikenttä.
        /// </summary>
        private Label _matkaNaytto;
        
        
        /// <summary>
        /// Pelin aikana saavutettu pisin matka.
        /// </summary>
        private double _ennatys;
        
        
        /// <summary>
        /// Pelin alustus ja pelielementtien luonti.
        /// </summary>
        public override void Begin()
        {
            _kajakki = new GameObject(50, 90, Shape.Ellipse);
            _kajakki.Color = Color.Orange;
            _kajakki.Position = new Vector(0, -150);
            Add(_kajakki);

            SetWindowSize(800, 600);
            Level.Background.Color = new Color(40, 130, 190);

            _matkaNaytto = new Label("Matka: 0 m");
            _matkaNaytto.Position = new Vector(0, 260);
            Add(_matkaNaytto);

            GameObject vasenRanta = new GameObject(100, 600);
            vasenRanta.Color = Color.DarkGreen;
            vasenRanta.Position = new Vector(-350, 0);
            Add(vasenRanta);

            GameObject oikeaRanta = new GameObject(100, 600);
            oikeaRanta.Color = Color.DarkGreen;
            oikeaRanta.Position = new Vector(350, 0);
            Add(oikeaRanta);
            
            Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liiku vasemmalle", -5, 0);
            Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liiku oikealle", 5, 0);
            Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaUusiPeli, "Aloita uusi peli");

            _liikeTimer = Timer.CreateAndStart(0.05, LiikutaKivia);
            _kiviTimer = Timer.CreateAndStart(_kivenLuontivali, LuoSatunnainenKivi);
        }

        
        /// <summary>
        /// Siirtää kajakkia ja tarkistaa törmäykset.
        /// </summary>
        /// <param name="x">Vaakasuuntainen siirtymä.</param>
        /// <param name="y">Pystysuuntainen siirtymä.</param>
        private void Liikuta(int x, int y)
        {
            if (_peliPaattynyt) return;

            Vector uusiSijainti = _kajakki.Position + new Vector(x, y);

            bool osuuKiveen = OsuukoKiveen(uusiSijainti);

            bool osuuRantaan = uusiSijainti.X - KajakinPuolikasLeveys < -JoenRaja ||
                               uusiSijainti.X + KajakinPuolikasLeveys > JoenRaja;

            if (osuuKiveen)
            {
                PaataPeli();
                return;
            }

            if (osuuRantaan)
            {
                PaataPeli();
                return;
            }

            _kajakki.Position = uusiSijainti;
        }
        
        
        /// <summary>
        /// Liikuttaa kiviä, päivittää kuljetun matkan ja vaikeustason.
        /// </summary>
        private void LiikutaKivia()
        {
            if (_peliPaattynyt) return;

            _matka += MatkanLisays;
            _matkaNaytto.Text = "Matka: " + _matka.ToString("F1") + " m";

            if (_matka >= _seuraavaVaikeustaso)
            {
                if (_kivenNopeus < KivenMaksiminopeus)
                {
                    _kivenNopeus += KivenNopeudenLisays;
                }

                if (_kivenLuontivali > KivenMinimiLuontivali)
                {
                    _kivenLuontivali -= KivenLuontivalinLyhennys;
                    _kiviTimer.Interval = _kivenLuontivali;
                }

                _seuraavaVaikeustaso += VaikeustasonNousuvali;
            }
            
            for (int i = _kivet.Count - 1; i >= 0; i--)
            {
                GameObject kivi = _kivet[i];
                kivi.Y -= _kivenNopeus;

                double etaisyysX = Math.Abs(_kajakki.X - kivi.X);
                double etaisyysY = Math.Abs(_kajakki.Y - kivi.Y);

                double rajaX = KajakinPuolikasLeveys + kivi.Width / 2;
                double rajaY = KajakinPuolikasKorkeus + kivi.Height / 2;

                double suhdeX = etaisyysX / rajaX;
                double suhdeY = etaisyysY / rajaY;

                if (suhdeX * suhdeX + suhdeY * suhdeY < 1)
                {
                    PaataPeli();
                    return;
                }

                if (kivi.Y < -JoenRaja)
                {
                    kivi.Destroy();
                    _kivet.RemoveAt(i);
                }
            }
        }

        
        /// <summary>
        /// Tarkistaa, osuuko kajakki kiveen annetussa sijainnissa.
        /// </summary>
        /// <param name="sijainti">Kajakin tarkistettava sijainti.</param>
        /// <returns>True, jos kajakki osuu kiveen, muuten false.</returns>
        private bool OsuukoKiveen(Vector sijainti)
        {
            foreach (GameObject kivi in _kivet)
            {
                double etaisyysX = Math.Abs(sijainti.X - kivi.X);
                double etaisyysY = Math.Abs(sijainti.Y - kivi.Y);

                double rajaX = KajakinPuolikasLeveys + kivi.Width / 2;
                double rajaY = KajakinPuolikasKorkeus + kivi.Height / 2;

                double suhdeX = etaisyysX / rajaX;
                double suhdeY = etaisyysY / rajaY;

                if (suhdeX * suhdeX + suhdeY * suhdeY < 1)
                {
                    return true;
                }
            }

            return false;
        }
        
        
        /// <summary>
        /// Päättää pelin, päivittää ennätyksen ja näyttää tuloksen.
        /// </summary>
        private void PaataPeli()
        {
            if (_peliPaattynyt) return;

            _peliPaattynyt = true;

            _liikeTimer.Stop();
            _kiviTimer.Stop();

            if (_matka > _ennatys)
            {
                _ennatys = _matka;
            }

            Label tulos = new Label("Peli päättyi!");
            tulos.Position = new Vector(0, 80);
            Add(tulos);

            Label matka = new Label("Kuljettu matka: " + _matka.ToString("F1") + " m");
            matka.Position = new Vector(0, 30);
            Add(matka);

            Label ennatys = new Label("Ennätys: " + _ennatys.ToString("F1") + " m");
            ennatys.Position = new Vector(0, -20);
            Add(ennatys);

            Label ohje = new Label("Paina ENTER pelataksesi uudelleen");
            ohje.Position = new Vector(0, -80);
            Add(ohje);
        }

        
        /// <summary>
        /// Luo uuden kiven ja lisää sen kivien listaan.
        /// </summary>
        /// <param name="x">Kiven vaakasijainti.</param>
        /// <param name="y">Kiven pystysijainti.</param>
        private void LuoKivi(double x, double y)
        {
            double koko = _satunnaisluku.Next(KivenMinimiKoko, KivenMaksimiKoko);
            GameObject kivi = new GameObject(koko, koko, Shape.Ellipse);
            kivi.Color = Color.DarkGray;
            kivi.Position = new Vector(x, y);
            Add(kivi);
            _kivet.Add(kivi);
        }

        
        /// <summary>
        /// Luo yhden tai kaksi kiveä satunnaisesti joen ylös.
        /// </summary>
        private void LuoSatunnainenKivi()
        {
            if (_peliPaattynyt) return;

            double x = _satunnaisluku.Next(KivenMinimiX, KivenMaksimiX);
            LuoKivi(x, JoenRaja);

            if (_satunnaisluku.Next(0, 4) == 0)
            {
                double toinenX = _satunnaisluku.Next(KivenMinimiX, KivenMaksimiX);

                while (Math.Abs(x - toinenX) < KivienMinimietaisyys)
                {
                    toinenX = _satunnaisluku.Next(KivenMinimiX, KivenMaksimiX);
                }

                LuoKivi(toinenX, JoenRaja);
            }
        }
        
        
        /// <summary>
        /// Aloittaa pelin uudelleen.
        /// </summary>
        private void AloitaUusiPeli()
        {
            _liikeTimer?.Stop();
            _kiviTimer?.Stop();

            ClearAll();
            _kivet.Clear();
            
            _matka = 0;
            _kivenLuontivali = 2;
            _kivenNopeus = 4;
            _seuraavaVaikeustaso = VaikeustasonNousuvali;
            _peliPaattynyt = false;

            Begin();
        }
    }
    
    
}