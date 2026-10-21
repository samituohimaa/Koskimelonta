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
        private const double KivenNopeus = 4;
        
        
        /// <summary>
        /// Aika uuden kiven luomiseen sekunteina.
        /// </summary>
        private const double KivenLuontivali = 2;
        
        
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
        private const double KajakinPuolikasLeveys = 20;
        
        
        /// <summary>
        /// Kajakin ja kiven puolikkaiden leveyksien summa.
        /// </summary>
        private const double TormaysRajaX = 45;

        
        /// <summary>
        /// Kajakin ja kiven puolikkaiden korkeuksien summa.
        /// </summary>
        private const double TormaysRajaY = 65;

        
        /// <summary>
        /// Lista pelin kivistä.
        /// </summary>
        private List<GameObject> _kivet = new List<GameObject>();

        
        /// <summary>
        /// Pelaajan ohjaama kajakki.
        /// </summary>
        private GameObject _kajakki;

        
        /// <summary>
        /// Pelissä kuljettu matka.
        /// </summary>
        private double _matka;
        
        
        /// <summary>
        /// Kertoo, onko peli päättynyt.
        /// </summary>
        private bool _peliPaattynyt;

        
        /// <summary>
        /// Kuljetun matkan näyttävä tekstikenttä.
        /// </summary>
        private Label _matkaNaytto;
        
        
        /// <summary>
        /// Pelin alustus ja pelielementtien luonti.
        /// </summary>
        public override void Begin()
        {
            _kajakki = new GameObject(40, 80);
            _kajakki.Color = Color.Orange;
            _kajakki.Position = new Vector(0, 0);
            Add(_kajakki);

            SetWindowSize(800, 600);
            Level.Background.Color = Color.LightBlue;

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
            _kiviTimer = Timer.CreateAndStart(KivenLuontivali, LuoSatunnainenKivi);
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

            if (!osuuRantaan)
            {
                _kajakki.Position = uusiSijainti;
            }
        }
        
        
        /// <summary>
        /// Liikuttaa kiviä ja päivittää kuljetun matkan.
        /// </summary>
        private void LiikutaKivia()
        {
            if (_peliPaattynyt) return;
            _matka += 0.1;
            _matkaNaytto.Text = "Matka: " + _matka.ToString("F1") + " m";

            for (int i = _kivet.Count - 1; i >= 0; i--)
            {
                GameObject kivi = _kivet[i];
                kivi.Y -= KivenNopeus;
                
                if (Math.Abs(_kajakki.X - kivi.X) < TormaysRajaX &&
                    Math.Abs(_kajakki.Y - kivi.Y) < TormaysRajaY)
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
                if (Math.Abs(sijainti.X - kivi.Position.X) < TormaysRajaX &&
                    Math.Abs(sijainti.Y - kivi.Position.Y) < TormaysRajaY)
                {
                    return true;
                }
            }

            return false;
        }
        
        
        /// <summary>
        /// Päättää pelin ja näyttää tuloksen.
        /// </summary>
        private void PaataPeli()
        {
            _peliPaattynyt = true;
            MessageDisplay.Add("Osuit kiveen! Matka: " + _matka.ToString("F1") + " m");
        }

        
        /// <summary>
        /// Luo uuden kiven ja lisää sen kivien listaan.
        /// </summary>
        /// <param name="x">Kiven vaakasijainti.</param>
        /// <param name="y">Kiven pystysijainti.</param>
        private void LuoKivi(double x, double y)
        {
            GameObject kivi = new GameObject(50, 50);
            kivi.Color = Color.Gray;
            kivi.Position = new Vector(x, y);
            Add(kivi);
            _kivet.Add(kivi);
        }

        
        /// <summary>
        /// Luo uuden kiven satunnaiseen paikkaan joen ylös.
        /// </summary>
        private void LuoSatunnainenKivi()
        {
            if (_peliPaattynyt) return;
            
            double x = _satunnaisluku.Next(-250, 251);
            LuoKivi(x, JoenRaja);
        }
        
        /// <summary>
        /// Aloittaa pelin uudelleen Enter-näppäimellä.
        /// </summary>
        private void AloitaUusiPeli()
        {
            _liikeTimer?.Stop();
            _kiviTimer?.Stop();

            ClearAll();
            _kivet.Clear();
            _matka = 0;
            _peliPaattynyt = false;

            Begin();
        }
    }
    
    
}