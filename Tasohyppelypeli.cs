using Jypeli;
using System.Collections.Generic;
///
/// Dinojump 
/// 
/// @author Panu Honkala
/// @version 8.1.2021
/// 
/// <summary>
/// Pelissä hypit dinolla ja yrität kerätä mahdollisimman paljon kypsiä kiivejä
/// </summary>
///
public class Tasohyppelypeli1 : PhysicsGame
{
    private List<GameObject> liikutettavat = new List<GameObject>();
    private double suunta = -2;
    private double tuhoamisX;

    private Image taustaKuva = LoadImage("level1");
    private Image esteKuva = LoadImage("kivi");
    private Image pelaajanKuva = LoadImage("dine");
    private Image kiiviKuva = LoadImage("kiivi");
    private Image pilviKuva = LoadImage("pilvi");

    private const double NOPEUS = 100000;
    private const double HYPPYNOPEUS = 750;
    private const int RUUDUN_KOKO = 40;

    private PlatformCharacter pelaaja1;

    private SoundEffect maaliAani = LoadSoundEffect("maali");

    private IntMeter pisteLaskuri;


    public override void Begin()
    {
        /// MediaPlayer.Play("musat");

        Gravity = new Vector(0, -1000);

        LuoKentta();
        LisaaNappaimet();

        Level.Background.Image = taustaKuva;
        Level.Background.TileToLevel();

        Camera.FollowOffset = new Vector(Screen.Width / 2.5 - RUUDUN_KOKO, 0.0);


        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1;
        Camera.StayInLevel = true;

        tuhoamisX = Level.Left;

        Timer liikutusAjastin = new Timer();
        liikutusAjastin.Interval = 0.015;
        liikutusAjastin.Timeout += LiikutaPilvia;
        liikutusAjastin.Start();

        LuoPistelaskuri();

        LuoPilvia();

    }

    /// <summary>
    /// Aliohjelma generoi satunnaisia pilviä kenttään.
    /// </summary>
    public void LuoPilvia()
    {
        for (int i = 0; i < 4; i++)
        {
            PhysicsObject pilvi = new PhysicsObject(100, 40);

            pilvi.Y = pelaaja1.Y + 100;
            pilvi.X = RandomGen.NextInt(0, 2000);

            Add(pilvi);
            liikutettavat.Add(pilvi);

            pilvi.Image = pilviKuva;
            pilvi.IgnoresGravity = true;
        }
    }
    /// <summary>
    /// Aliohjelma määrittää pilvien liikeen.
    /// </summary>
    public void LiikutaPilvia()
    {
        pelaaja1.Push(new Vector(NOPEUS, 0.0));
  
        for (int i = 0; i < liikutettavat.Count; i++)
        {
            GameObject olio = liikutettavat[i];
            olio.X += suunta;

            if (olio.X <= tuhoamisX)
            {
                olio.Destroy();
                liikutettavat.Remove(olio);
            }
        }
    }
    /// <summary>
    /// Aliohjelma luo pistelaskurin ja määrittää sen sijainnin
    /// </summary>
    public void LuoPistelaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Right - 100;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.LightBlue;

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);

        pisteNaytto.Title = "Kerätyt kiivit";

    }

    /// <summary>
    /// Aliohjelma luo kentän ja sen objektit.
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta1.txt");
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaTahti);
        kentta.SetTileMethod('N', LisaaPelaaja);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.HotPink, Color.SkyBlue);
    }

    /// <summary>
    /// Aliohjelma määrittää kentän esteet.
    /// </summary>
    public void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Image = esteKuva;
        taso.Tag = "seina";
        Add(taso);
    }

    /// <summary>
    /// Aliohjelma määrittää kentän maalin.
    /// </summary>
    public void LisaaTahti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject tahti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        tahti.IgnoresCollisionResponse = true;
        tahti.Position = paikka;
        tahti.Image = kiiviKuva;
        tahti.Tag = "tahti";
        Add(tahti);
    }

    /// <summary>
    /// Aliohjelma lisää pelaajan kentälle.
    /// </summary>
    public void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 7.0;

        pelaaja1.Image = pelaajanKuva;

        AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        AddCollisionHandler(pelaaja1, "seina", TormaaTasoon);

        Add(pelaaja1);
    }
    /// <summary>
    /// Aliohjelma määrittää, mitä tapahtuu, kun törmäät esteeseen.
    /// </summary>
    public void TormaaTasoon(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Törmäsit esteeseen, joten hävisit pelin!");
        Keyboard.Disable(Key.Up);

        pelaaja1.Destroy();
    }
    /// <summary>
    /// Lisätään pelissä käytettävät näppäimet.
    /// </summary>
    public void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");

        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, HYPPYNOPEUS);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }
    /// <summary>
    /// Aliohjelman avulla hypätään.
    /// </summary>
    public void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    /// <summary>
    /// Aliohjelma määrittää, mitä tapahtuu, kun törmäät maaliin.
    /// </summary>
    public void TormaaTahteen(PhysicsObject hahmo, PhysicsObject tahti)
    {
        maaliAani.Play();
        MessageDisplay.Add("Löysit kypsän kiivin!");
        tahti.Destroy();

        pisteLaskuri.Value += 1;
    }



}