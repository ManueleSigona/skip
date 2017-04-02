// *************************************************
//
//        HEX (v1.1) Ultime modifiche: 19.03.2012
//
// *************************************************


using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Timers;                // per gestire i tempi di preselezione e attivazione nella modalita zero click
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

using Globals;



namespace Skip
{
    // ma come vengono gestiti gli eventi??  In particolare i primi 2 eventi sollevati all'inizio del progr sono:
    // 1)L'EVENTO LOAD sollevato quando l'utente carica la form principale
    // 2)L'EVENTO PAINT sollevato ogni volta che occorre ridisegn la form


    public struct IconInfo  // sta struct serve per il custom cursor
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }


    // =================================================
    //	Class Form1
    // =================================================


    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //----- queste istruz servono per il custom cursor -----
        [DllImport("user32.dll")]
        public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        //---------------------------------------------------

        // ---------- Aggiunti per skip ----------------
        Color orth_fg_col, orth_bg_col; // colori dei tasti "ortogonali"
        Color rorth_fg_col, rorth_bg_col; // colori dei tasti "orizzontali" (riga)
        Color corth_fg_col, corth_bg_col; // colori dei tasti "verticali" (colonna)
        Color other_fg_col, other_bg_col; // colori degli altri tasti
        Color indic_col, presel0_col, presel1_col; // colori di selezione
        int num_tastiere, num_righe, num_colonne; // numero tastiere, numero righe (tasti), numero colonne (tasti)
        int prima_riga, ultima_riga, prima_colonna, ultima_colonna; // area tastiera ortogonale (a partire da 0)
        int num_righe_complOrt, num_colonne_complOrt; // numero righe e colonne su cui fare compl ort
        List<Tastiera> tastiere = new List<Tastiera>(); // le tastiere
        bool completamentoOrt = true; // se usare o no il completamento ortogonale
        int num_completamento; // il numero di tasti da inserire nel completamento ortogonale

        int tastieraCorrente = 0; // la tastiera corrente da visualizzare
        int completamentoI = -1, completamentoJ = -1; // di quale tasto mostrare il completamento
        bool flagShift = false;
        int num_ultima_sillaba = 0;
        bool flagLock = false;
        // ---------------------------------------------


        Color coloreBackground, coloreTastoInattivo, coloreTastoTemporaneo, coloreTastoAttivo;
        Color colconfig1, colconfig2;

        public static Color coloreFont;   // aggiunta da poco per poter modif dal file config il colore del testo dei tasti

        Color coloreTastoAttivo1;       // serve per dare un colore diverso da cyan se il tasto è attivo in 1click
        Color coloreTriangoliDinamici;// colore per i triangoli dinamici

        Color colormenu;
        Color colormenuattivo;
        Color colorfontmenu;
        public static SolidBrush colorcontornobott = new SolidBrush(Color.Black);
        int flagTriangoliDinamici = 0;


        Font fontTasto;
        int tempoPreselezione;
        int tempoAttivazione;

        IntPtr aggancio = IntPtr.Zero;      // rappresenta un handle inizializz a 0
        public enum Modalita { zeroClick, unClick, touch }     // perchè Modalita.zeroClick restituisce 0 mentre Modalita.unClick = 1
        public Modalita modalita;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        System.Timers.Timer timer2 = new System.Timers.Timer();
        IntPtr handleTastiera;

        int cont_c1 = 0;
        int cont_c2 = 0;
        int cont_c3 = 0;

        int cont_v1 = 0;
        int cont_v2 = 0;
        int cont_v3 = 0;

        static Cursor cur;  // sta var serve a portare il cursore custom in tasti esag e nelle altre classi (con 1 metodo)

        bool change = false;

        //Windows management message constants
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int WM_NCMOUSEMOVE = 0x00A0;
        //Previous selected window handle pointer
        IntPtr selectedWindow;
        float defzoom = 1;
        // -------------------------------------------------
        //	salvataggio dell'handle della form
        // -------------------------------------------------

        public MainForm()
        {
            handleTastiera = this.Handle;   // da un riferimento alla tastiera un numero ad es
            InitializeComponent();          //Questa funzione viene utilizzata per associare oggetti Event controlli.

            // Definisco testo per la finestra (programma + versione)
            this.Text = "SKIP (v" + Glob.Versione + ")";

            Bitmap bitmap = new Bitmap(714, 155);
            Graphics g = Graphics.FromImage(bitmap);
            //using (Font f = new Font(FontFamily.GenericSansSerif, 20))
            Font f = new Font(FontFamily.GenericSansSerif, 20);
            g.DrawString("", f, colorcontornobott, -6, -21);  // i 2 numeri finali rappresentano le coordinate (prese a tentativi) 

            this.Cursor = CreateCursor(bitmap, 3, 3);
            cur = this.Cursor;

            bitmap.Dispose();

            Load += new EventHandler(MainForm_Load);
            panel1.Paint += new PaintEventHandler(panel1_Paint);

            //-------------------------------------------------
        }

        // -----------------------------------------------------------
        // Rende la finestra non selezionabile
        // -----------------------------------------------------------

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;
                param.ExStyle |= WS_EX_NOACTIVATE;
                return param;
            }
        }

        // -----------------------------------------------------------
        // Gestisce problemi di visualizzazione del dragging della finestra
        // -----------------------------------------------------------

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCLBUTTONDOWN:
                    selectedWindow = GetForegroundWindow();
                    SetForegroundWindow(this.Handle);
                    break;
                case WM_NCMOUSEMOVE:
                    if (GetForegroundWindow() == this.Handle && selectedWindow != IntPtr.Zero)
                    {
                        SetForegroundWindow(selectedWindow);
                        selectedWindow = IntPtr.Zero;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        // -------------------------------------------------
        //	provo a creare una funz x passare il cursore a tasti esagonali
        // -------------------------------------------------

        public static Cursor getCustomCursor()
        {
            return cur;
        }



        // -------------------------------------------------
        //	queste istruz servono per il custom cursor -----
        // -------------------------------------------------

        public static Cursor CreateCursor(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            IconInfo tmp = new IconInfo();
            GetIconInfo(bmp.GetHicon(), ref tmp);
            tmp.xHotspot = xHotSpot;
            tmp.yHotspot = yHotSpot;
            tmp.fIcon = false;
            return new Cursor(CreateIconIndirect(ref tmp));
        }



        // -------------------------------------------------
        //	quando il focus passa ad un'altra finestra, si salva 
        //	l'handle di quest'ultima (è un evento,gli passa eventsArgs)
        // -------------------------------------------------

        void Form1_LostFocus(object sender, EventArgs e)        //si occupa di gestire l’aggancio della tastiera esagonale con altre applicazioni, dove è possibile inserire il testo, recuperando l’handle della finestra a cui verra inviata la pseudo-sillaba.
        {
            aggancio = GetForegroundWindow();   // salva l'handle della finestra che è apena passata in primo piano (non avrei potuto farlo senza la libreria "user32.dll")
            while (aggancio == IntPtr.Zero)     // finchè non cè aggancio non esci,
                aggancio = GetForegroundWindow();
        }



        // -------------------------------------------------
        //	timer per gestire il tempo di preselezione, si occupa di 
        //	controllare lo stato di tutti i tasti presenti nella tastiera
        // -------------------------------------------------


        void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
                  
        }



        // -------------------------------------------------
        //	operazioni da eseguire al caricamento della form, 
        //	PRIMO EVENTO SOLLEVATO
        // -------------------------------------------------


        private void MainForm_Load(object sender, EventArgs e)
        {

            leggiConfigurazione("configurazione.cfg"); // leggi configurazione e tastiera

            textBox1.Text = "Tp: " + tempoPreselezione.ToString() + " ms";
            textBox2.Text = "Ta: " + tempoAttivazione.ToString() + " ms";
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);  // genera l'evento ogni 2000 ms (dal file config)
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);
            //this.LostFocus += new EventHandler(Form1_LostFocus);    //gestisce il cambio di foregroundWindow (finestra in primo piano)
            TopMost = true;
            this.menuStrip1.BackColor = colormenu;
            this.menuStrip1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.menuStrip1.ForeColor = colorfontmenu;


            Glob.poslettere = Convert.ToInt32(Glob.poslettere * defzoom);
            if (Glob.wina > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 0.35 && Glob.winl > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.35)
            {

                Glob.wina = Glob.wina * defzoom;
                Glob.winl = Glob.winl * defzoom;
                Size = new System.Drawing.Size(Convert.ToInt32(Glob.winl), Convert.ToInt32(Glob.wina));


                this.Refresh();
            }
        }



        // -------------------------------------------------
        //	ritorna il color font
        // -------------------------------------------------

        public static Color getColoreFont()
        {
            return coloreFont;
        }



        // -------------------------------------------------
        //	l'evento paint è sollevato ogni volta che 
        //	si vuole ridisegnare la form
        // -------------------------------------------------


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (flagLock)
            { // il tasto lock è attivo => tutti maiuscoli
                for (int i = 1; i < num_righe; i++) // partiamo dalla riga 1 perchè la riga 0 non ha lettere
                {
                    for (int j = 0; j < num_colonne; j++)
                    {
                        if (char.IsLetter(tastiere[tastieraCorrente].matriceTasti[i, j].contenuto[0]))
                            tastiere[tastieraCorrente].matriceTasti[i, j].contenuto =
                                tastiere[tastieraCorrente].matriceTasti[i, j].contenuto.ToUpper();
                    }
                }
                if (completamentoI != -1 && completamentoJ != -1)
                { // rendiamo maiuscoli anche i tasti del completamento aperto
                    foreach (Tasto t in tastiere[tastieraCorrente].matriceTasti[completamentoI, completamentoJ].completamento)
                        t.contenuto = t.contenuto.ToUpper();
                }
            }
            else if (flagShift)
            { // allora tutti i tasti devono avere la prima lettera maiuscola
                for (int i = 1; i < num_righe; i++) // partiamo dalla riga 1 perchè la riga 0 non ha lettere
                {
                    for (int j = 0; j < num_colonne; j++)
                    {
                        if (char.IsLetter(tastiere[tastieraCorrente].matriceTasti[i, j].contenuto[0]))
                        {
                            tastiere[tastieraCorrente].matriceTasti[i, j].contenuto =
                                tastiere[tastieraCorrente].matriceTasti[i, j].contenuto[0].ToString().ToUpper() + tastiere[tastieraCorrente].matriceTasti[i, j].contenuto.Substring(1).ToString().ToLower();
                        }
                    }
                }
                if (completamentoI != -1 && completamentoJ != -1)
                { // mettiamo la maiuscola anche nei tasti del completamento aperto
                    foreach (Tasto t in tastiere[tastieraCorrente].matriceTasti[completamentoI, completamentoJ].completamento)
                        t.contenuto = t.contenuto[0].ToString().ToUpper() + t.contenuto.Substring(1).ToString().ToLower();
                }
            }
            else
            { // allora dobbiamo far tornare i tasti normali
                for (int i = 1; i < num_righe; i++) // partiamo dalla riga 1 perchè la riga 0 non ha lettere
                {
                    for (int j = 0; j < num_colonne; j++)
                    {
                        if (char.IsLetter(tastiere[tastieraCorrente].matriceTasti[i, j].contenuto[0]))
                        {
                            tastiere[tastieraCorrente].matriceTasti[i, j].contenuto =
                                tastiere[tastieraCorrente].matriceTasti[i, j].contenuto.ToLower();
                        }
                    }
                }
                if (completamentoI != -1 && completamentoJ != -1)
                { // facciamo tornare normali anche i tasti del completamento aperto
                    foreach (Tasto t in tastiere[tastieraCorrente].matriceTasti[completamentoI, completamentoJ].completamento)
                        t.contenuto = t.contenuto.ToLower();
                }
            }
            tastiere[tastieraCorrente].disegnaTastiera(e.Graphics, orth_fg_col, orth_bg_col, rorth_fg_col, rorth_bg_col,
                corth_fg_col, corth_bg_col, other_fg_col, other_bg_col, indic_col, presel0_col, presel1_col, completamentoI, completamentoJ);
        }



        // -------------------------------------------------
        //	a seconda della posizione del puntatore e dello stato 
        //	dei tasti viene gestita la funzionalita della tastiera,
        //	utilizata da tutti i metodi ModificaTasto,gestisce tutti g
        //	li stati,deformaz esagonali
        // -------------------------------------------------


        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {

        }

        // -------------------------------------------------
        //	leggi file config.cfg
        // -------------------------------------------------

        //metodo per leggere il file di configurazione

        public void leggiConfigurazione(String s)
        {
            bool chiudi = false;
            bool err = false;
            int cont = 1; // tiene conto della riga a cui siamo arrivati a leggere
            int mod = 1;
            int k = 0; // tiene conto di quale tastiera stiamo "riempiendo"

            try
            {
                //string path = Path.Combine(Glob.CartellaLocale_text, s);
                string path = "..\\..\\..\\" + s; // supponiamo di avere il file di configurazione nella cartella "iniziale" del progetto (dove c'è Skip.sln)
                StreamReader leggi = new StreamReader(File.OpenRead(path));
                string riga = "";
                while (!leggi.EndOfStream)
                {
                    riga = leggi.ReadLine();
                    if (!riga.StartsWith("#") && !riga.StartsWith(";") && riga.Length > 0)   //se la riga comincia con # non la leggi (è come se fosse commentata) 
                    {                                               //se la riga non ha elem non viene letta (posso schiacciare invio quante volte voglio)
                        string rigaLetta = riga;
                        cont++;
                        if (cont == 2) // font
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                fontTasto = new Font(dati[0], Convert.ToSingle(dati[1]), FontStyle.Bold);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 3) // orth_colors
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                orth_fg_col = Color.FromName(dati[0]);
                                orth_bg_col = Color.FromName(dati[1]);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 4) // rorth_colors
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                rorth_fg_col = Color.FromName(dati[0]);
                                rorth_bg_col = Color.FromName(dati[1]);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 5) // corth_colors
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                corth_fg_col = Color.FromName(dati[0]);
                                corth_bg_col = Color.FromName(dati[1]);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 6) // other_colors
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                other_fg_col = Color.FromName(dati[0]);
                                other_bg_col = Color.FromName(dati[1]);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 7) // sel_colors
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                indic_col = Color.FromName(dati[0]);
                                presel0_col = Color.FromName(dati[1]);
                                presel1_col = Color.FromName(dati[2]);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 8) //tempo di preselezione
                        {
                            try
                            {
                                tempoPreselezione = Convert.ToInt32(rigaLetta);      
                                timer1.Interval = tempoPreselezione;
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 9) //tempo di attivazione
                        {
                            try
                            {
                                tempoAttivazione = Convert.ToInt32(rigaLetta);         
                                timer2.Interval = tempoAttivazione;
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == 10) // modo attuazione
                        {
                            try
                            {
                                mod = Convert.ToInt32(rigaLetta);
                                Glob.click = mod;
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        // inizio parte configurazione tastiera
                        if (cont == 11)
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                num_tastiere = Convert.ToInt32(dati[0]);
                                num_righe = Convert.ToInt32(dati[1]);
                                num_colonne = Convert.ToInt32(dati[2]);
                                // Bisogna creare la classe tastiera
                                for (int i = 0; i < num_tastiere; i++)
                                { // creiamo tutte le tastiere (per adesso non avranno ancora tasti dentro)
                                    tastiere.Add(new Tastiera(num_righe, num_colonne, fontTasto));
                                }
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        //tastiere.
                        if (cont == 12)
                        {
                            try
                            {
                                string[] dati = rigaLetta.Split(',');
                                prima_riga = Convert.ToInt32(dati[0]);
                                ultima_riga = Convert.ToInt32(dati[1]);
                                prima_colonna = Convert.ToInt32(dati[2]);
                                ultima_colonna = Convert.ToInt32(dati[3]);
                                // calcoliamo per dopo le righe e colonne su cui fare complet ort
                                num_righe_complOrt = ultima_riga - prima_riga + 1;
                                num_colonne_complOrt = ultima_colonna - prima_colonna + 1;
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        // (12 < cont < 13 + num_tastiere )
                        if (cont > 12 && cont < (13 + num_tastiere))    // leggiamo tutte le tastiere, una per una
                        {
                            try
                            {
                                int xCentro, yCentro; // coordinate assolute nella form del tasto letto
                                for (int i = 0; i < num_righe; i++) // leggiamo riga per riga la tastiera
                                {
                                    if (i != 0) // bisogna leggere la riga successiva (tranne la prima volta)
                                    {
                                        do
                                            rigaLetta = leggi.ReadLine(); // leggi la prossima riga valida
                                        while (rigaLetta.StartsWith("#") || rigaLetta.StartsWith(";") || rigaLetta.Length == 0);
                                    }
                                    string[] tastiLetti = rigaLetta.Split('\t');
                                    for (int j = 0; j < num_colonne; j++) // istanziamo tutti i tasti di questa riga
                                    {
                                        // istanziare nuovo tasto passandogli cosa deve esserci scritto (tastiLetti[j]) e la posizione (dipenderà da i e da j)
                                        //--------------- posizione nuovo tasto ---------------
                                        xCentro = tastiere[k].origineX + j * Tasto.xDimension + Tasto.xDimension / 2; // coord centro = offset + (j + 0.5) volte la dimensione del tasto
                                        yCentro = tastiere[k].origineY + i * Tasto.yDimension + Tasto.yDimension / 2; // coord centro = offset + (i + 0.5) volte la dimensione del tasto
                                        //-----------------------------------------------------
                                        // -------------- che tipo di tasto è? ----------------
                                        Tasto.TipoTasto tipo;
                                        if ((i < prima_riga || i > ultima_riga) || (j < prima_colonna || j > ultima_colonna))
                                            tipo = Tasto.TipoTasto.Altro; // sono i tasti non facenti parte dell'area ortogonale
                                        else if (i == prima_riga && j == prima_colonna)
                                            tipo = Tasto.TipoTasto.Altro; // è l'intersezione di prima riga e prima colonna (è vuoto)
                                        else if (i == prima_riga)
                                            tipo = Tasto.TipoTasto.Riga; // i tasti riga del completamento
                                        else if (j == prima_colonna)
                                            tipo = Tasto.TipoTasto.Colonna; // i tasti colonna del completamento
                                        else
                                            tipo = Tasto.TipoTasto.Ortogonale; // i rimanenti sono quelli nell'area ortogonale
                                        //-----------------------------------------------------
                                        tastiere[k].matriceTasti[i, j] = new Tasto(i, j, tastiLetti[j], xCentro, yCentro, tipo, tastiere[k]); //aggiungiamo alla tastiera corrente il nuovo tasto appena creato
                                        tastiere[k].aggiungiTasto(new Tasto(i, j, tastiLetti[j], xCentro, yCentro, tipo, tastiere[k]));
                                        // TODO: in realtà l'ultima riga della tastiera è da gestire in modo diverso, a causa delle dimensioni diverse dei tasti...
                                    }
                                }
                                tastiere[k].aggiungiTastiSpeciali(); // aggiunge l'ultima riga di tasti (sono uguali per tutte le tastiere)
                                k++; // finito di riempire una tastiera, al prossimo giro dobbiamo riempire la prossima
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == (13 + num_tastiere))    // finito di istanziare i tasti delle 4 tastiere
                        {
                            try
                            {
                                k = 0; // ci servirà di nuovo dopo
                                completamentoOrt = (rigaLetta == "1" ? true : false);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        if (cont == (14 + num_tastiere))
                        {
                            try
                            {
                                num_completamento = Convert.ToInt32(rigaLetta);
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                        // adesso bisogna leggere cosa mettere nei menu di completamento ortogonale
                        // (probabilmente verrà comodo gestire tutto tramite la classe Tastiera)
                        if (cont > (14 + num_tastiere) && cont < (15 + 2*num_tastiere))
                        {
                            try
                            {
                                for (int i = 0; i < num_righe_complOrt; i++) // leggiamo riga per riga la tastiera
                                {
                                    for (int j = 0; j < num_colonne_complOrt; j++) // istanziamo tutti i tasti di questa riga
                                    {
                                        if (i == 0 && j == 0)
                                            j++; // questo perchè il tasto in alto a sx del compl ort è sempre vuoto
                                        if ((j != 1 && i == 0) || i > 0) // bisogna leggere la riga successiva (tranne la prima volta)
                                        {
                                            do
                                                rigaLetta = leggi.ReadLine(); // leggi la prossima riga valida
                                            while (rigaLetta.StartsWith("#") || rigaLetta.StartsWith(";") || rigaLetta.Length == 0);
                                        }
                                        string[] tastiOrtLetti = rigaLetta.Split('\t');
                                        // istanzio nuovo complet ort e lo assegno al tasto corrispondente
                                        if (tastiOrtLetti.Length > 1) // se il completamento è definito per questo tasto
                                        {
                                            tastiere[k].matriceTasti[prima_riga + i, prima_colonna + j].aggiungiCompletamento(tastiOrtLetti);
                                            tastiere[k].matriceTasti[prima_riga + i, prima_colonna + j].haCompletamento = true;
                                        }
                                    }
                                }
                                k++; // finito di riempire una tastiera, al prossimo giro dobbiamo riempire la prossima
                            }
                            catch
                            {
                                err = true;
                            }
                        }
                    }
                }
                this.Refresh(); //refresh finestra per far si che compaiano tutti i tasti subito! 
                leggi.Close();
            }
            catch //Se non riesce ad aprire il file di Config: danneggiato o mancante.
            {          
                err = true;
            }
            if (err)
            {
                MessageBox.Show(ForegroundWindow.Instance, "File di configurazione con errori o non letto! Caricamento valori di default!", "Errore nel file di configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                defzoom = Convert.ToInt32(Glob.defzoom);
                BackColor = Color.FromName(Glob.backgroundTastiera);
                coloreBackground = BackColor;
                coloreFont = Color.FromName(Glob.coloreScrittaTasto);
                coloreTastoInattivo = Color.FromName(Glob.coloreTastoInattivo);
                colorcontornobott = new SolidBrush(Color.FromName(Glob.coloreBordoTastiMenu));
                coloreTastoTemporaneo = Color.FromName(Glob.coloreTastoCorrente);
                coloreTastoAttivo = Color.FromName(Glob.coloreTastoAttesa0);
                colconfig1 = coloreTastoAttivo;
                coloreTastoAttivo1 = Color.FromName(Glob.coloreTastoAttesa1);
                colconfig2 = coloreTastoAttivo1;
                coloreTriangoliDinamici = Color.FromName(Glob.coloreTriangoli);
                colormenu = Color.FromName(Glob.coloreBackgroundMenu);
                colormenuattivo = Color.FromName(Glob.coloreTastoAttivoMenu);
                colorfontmenu = Color.FromName(Glob.coloreForegroundTestoMenu);
                string[] dati = Glob.fontTasto.Split('\t');
                fontTasto = new Font(dati[0], Convert.ToSingle(dati[1]), GraphicsUnit.Point);
                mod = Convert.ToInt32(Glob.modalitaAttuazione);
                Glob.click = mod;
                flagTriangoliDinamici = Glob.triang;
                tempoPreselezione = Convert.ToInt32(Glob.tempoPreselezione);      //tempo di preselezione
                timer1.Interval = tempoPreselezione;
                tempoAttivazione = Convert.ToInt32(Glob.tempoPreselezione);         //tempo di attivazione
                timer2.Interval = tempoAttivazione;
                Glob.poslettere = Convert.ToInt32(Glob.poslettere_mouse);
            }
            switch (mod)
            {
                case 0:
                    modalita = Modalita.zeroClick;
                    panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
                    Glob.poslettere = Convert.ToInt32(Glob.poslettere_mouse);
                    break;
                case 1:
                    modalita = Modalita.unClick;
                    panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
                    coloreTastoAttivo = colconfig2;
                    Glob.poslettere = Convert.ToInt32(Glob.poslettere_mouse);
                    break;
                case 2:
                    panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
                    Glob.poslettere = Convert.ToInt32(Glob.poslettere_touch);
                    modalita = Modalita.touch;
                    coloreTastoAttivo = colconfig1;
                    this.clickMouseToolStripMenuItem1.BackColor = colormenu;
                    this.clickMouseToolStripMenuItem.BackColor = colormenu;
                    this.touchToolStripMenuItem1.BackColor = colormenuattivo;
                    break;
                default:
                    modalita = Modalita.unClick;
                    panel1.MouseClick += new MouseEventHandler(panel1_MouseClick);
                    coloreTastoAttivo = colconfig2;
                    break;
            }

            if (modalita != Modalita.touch)
                if (Glob.click == 1)
                {

                    this.clickMouseToolStripMenuItem1.BackColor = colormenuattivo;
                    this.clickMouseToolStripMenuItem.BackColor = colormenu;
                }
                else
                {
                    this.clickMouseToolStripMenuItem.BackColor = colormenuattivo;
                    this.clickMouseToolStripMenuItem1.BackColor = colormenu;
                }
            TopMost = true;

            if (chiudi == true)
                Close();
        }

        // -------------------------------------------------
        //	secondo timer per gestire la modalita zero-click
        // -------------------------------------------------

        void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
          
        }



        // -------------------------------------------------
        //	gestione del click
        // -------------------------------------------------

        void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int savedI = 0;
                int savedJ = 0;
                string tastoCliccato = "";
                bool clickValido = false;

                if (completamentoI == -1 && completamentoJ == -1)
                { // cerchiamo tra i tasti normali, non c'è un menu completamento attivo
                    for (int i = 0; i < num_righe; i++)
                    {
                        for (int j = 0; j < num_colonne; j++)
                        {
                            if (tastiere[tastieraCorrente].matriceTasti[i, j].perimetro.IsVisible(e.Location))
                            { // il punto dove ha cliccato è all'interno di questo tasto
                                tastoCliccato = tastiere[tastieraCorrente].matriceTasti[i, j].contenuto;
                                savedI = i;
                                savedJ = j;
                                clickValido = true;
                                break;
                            }
                        }
                        if (clickValido)
                            break;
                    }
                    if (clickValido)
                    {
                        if (tastiere[tastieraCorrente].matriceTasti[savedI, savedJ].haCompletamento)
                        { // allora dobbiamo mostrare il suo menu di completamento
                            completamentoI = savedI; // sarà poi la funzione che disegna la tastiera ad occuparsi di disegnarlo
                            completamentoJ = savedJ;
                            panel1.Refresh(); // fa ridisegnare la tastiera
                        }
                        else
                        { // è LOCK, TAB, o un tasto normale?
                            if (tastoCliccato == "LOCK")
                            {
                                flagLock = !flagLock; // invertiamo il flag
                                panel1.Refresh();
                            }
                            else if (tastoCliccato == "TAB")
                            {
                                if (aggancio != handleTastiera)
                                    SendKeys.Send("{TAB}");
                            }
                            // altrimenti è un tasto normale senza completamento => bisogna scriverlo in output
                            else if (aggancio != handleTastiera)
                            {
                                foreach (char c in tastoCliccato)
                                {
                                    //Thread.Sleep(Glob.Sleep_TIME);
                                    SendKeys.Send(c.ToString());
                                    //Thread.Sleep(Glob.Sleep_TIME);
                                }
                                flagShift = false;
                                num_ultima_sillaba = tastoCliccato.Length;
                            }
                            panel1.Refresh();
                        }
                    }
                }
                else
                { // altrimenti vuol dire che è attivo un menu completamento
                  // dunque prima guardiamo se è stato cliccato un tasto del menu:
                    bool cliccatoCompletamento = false;
                    foreach (Tasto t in tastiere[tastieraCorrente].matriceTasti[completamentoI, completamentoJ].completamento)
                    {
                        if (t.perimetro.IsVisible(e.Location))
                        { // è stato cliccato proprio uno dei tasti del menu completamento
                            tastoCliccato = t.contenuto;
                            cliccatoCompletamento = true;
                            break;
                        }
                    }
                    if (cliccatoCompletamento)
                    { // bisogna scrivere in uscita il tasto premuto
                        if (aggancio != handleTastiera)
                        {
                            foreach (char c in tastoCliccato)
                            {
                                //Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.Send(c.ToString());
                                //Thread.Sleep(Glob.Sleep_TIME);
                            }
                            flagShift = false;
                            num_ultima_sillaba = tastoCliccato.Length;
                        }
                        completamentoI = -1; // chiudiamo il menu
                        completamentoJ = -1;
                        panel1.Refresh();
                    }
                    else
                    { // allora un menu completamento è aperto, ma è stato cliccato qualcos'altro:
                        for (int i = 0; i < num_righe; i++)
                        {
                            for (int j = 0; j < num_colonne; j++)
                            {
                                if (tastiere[tastieraCorrente].matriceTasti[i, j].perimetro.IsVisible(e.Location))
                                { // il punto dove ha cliccato è all'interno di questo tasto
                                    tastoCliccato = tastiere[tastieraCorrente].matriceTasti[i, j].contenuto;
                                    savedI = i;
                                    savedJ = j;
                                    clickValido = true;
                                    break;
                                }
                            }
                            if (clickValido)
                                break;
                        }
                        if (clickValido)
                        {
                            if (tastiere[tastieraCorrente].matriceTasti[savedI, savedJ].haCompletamento)
                            { // allora dobbiamo mostrare il suo menu di completamento
                                completamentoI = savedI; // sarà poi la funzione che disegna la tastiera ad occuparsi di disegnarlo
                                completamentoJ = savedJ;
                                panel1.Refresh(); // fa ridisegnare la tastiera
                            }
                            else
                            { // altrimenti è un tasto senza completamento => bisogna scriverlo in output
                                if (aggancio != handleTastiera)
                                {
                                    foreach (char c in tastoCliccato)
                                    {
                                        //Thread.Sleep(Glob.Sleep_TIME);
                                        SendKeys.Send(c.ToString());
                                        //Thread.Sleep(Glob.Sleep_TIME);
                                    }
                                    flagShift = false;
                                    num_ultima_sillaba = tastoCliccato.Length;
                                }
                                completamentoI = -1; // chiudiamo il menu
                                completamentoJ = -1;
                                panel1.Refresh();
                            }
                        }
                    }
                }
                clickValido = false;
                // proviamo a cercare tra i tasti speciali (ultima riga di tasti)
                foreach (Tasto tSpeciale in tastiere[tastieraCorrente].tastSpeciali)
                {
                    if (tSpeciale.perimetro.IsVisible(e.Location))
                    { // il punto dove ha cliccato è all'interno di questo tasto
                        tastoCliccato = tSpeciale.contenuto;
                        clickValido = true;
                        break;
                    }
                }
                if (clickValido)
                { // è stato premuto un tasto speciale... Cosa fare?
                    switch (tastoCliccato)
                    {
                        case "SHIFT":
                            flagShift = true;
                            panel1.Refresh();
                            break;
                        case "FR":
                            tastieraCorrente = tastieraCorrente == 1 ? 0 : 1;
                            panel1.Refresh();
                            break;
                        case "CF":
                            tastieraCorrente = tastieraCorrente == 2 ? 0 : 2;
                            panel1.Refresh();
                            break;
                        case "CR":
                            tastieraCorrente = tastieraCorrente == 3 ? 0 : 3;
                            panel1.Refresh();
                            break;
                        case " ": // SPACE
                            //Thread.Sleep(Glob.Sleep_TIME);
                            SendKeys.Send(" ");
                            //Thread.Sleep(Glob.Sleep_TIME);
                            break;
                        case "DEL":
                            //Thread.Sleep(Glob.Sleep_TIME);
                            SendKeys.Send("{BACKSPACE}");
                            //Thread.Sleep(Glob.Sleep_TIME);
                            break;
                        case "P-DEL":
                            for (int i = 0; i < num_ultima_sillaba; i++)
                            { // cancelliamo tutta l'ultima sillaba inserita
                                //Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.Send("{BACKSPACE}");
                                //Thread.Sleep(Glob.Sleep_TIME);
                            }
                            break;
                        case "INVIO":
                            //Thread.Sleep(Glob.Sleep_TIME);
                            SendKeys.Send("{ENTER}");
                            //Thread.Sleep(Glob.Sleep_TIME);
                            break;
                        default:
                            break;
                    }
                }
            }
        }


        // -------------------------------------------------
        //	salvataggio su file delle statistiche alla fine
        // -------------------------------------------------


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StreamWriter sw = new StreamWriter("statistiche.txt");
            sw.Write("Pseudo-sillabe consonantiche formate da un carattere: ");
            sw.WriteLine(Convert.ToString(cont_c1));
            sw.Write("Pseudo-sillabe consonantiche formate da due caratteri: ");
            sw.WriteLine(Convert.ToString(cont_c2));
            sw.Write("Pseudo-sillabe consonantiche formate da tre caratteri: ");
            sw.WriteLine(Convert.ToString(cont_c3));
            sw.Write("Pseudo-sillabe vocaliche formate da un carattere: ");
            sw.WriteLine(Convert.ToString(cont_v1));
            sw.Write("Pseudo-sillabe vocaliche formate da due caratteri: ");
            sw.WriteLine(Convert.ToString(cont_v2));
            sw.Write("Pseudo-sillabe vocaliche formate da tre caratteri: ");
            sw.WriteLine(Convert.ToString(cont_v3));
            sw.Close();


            if (change)
            {
                //MessageBox.Show(ForegroundWindow.Instance, "Aggiornato il file config con le impostazioni correnti", "Aggiornamento config", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                if (modalita == Modalita.zeroClick)
                    Glob.modalitaAttuazione = "0";
                if (modalita == Modalita.unClick)
                    Glob.modalitaAttuazione = "1";
                if (modalita == Modalita.touch)
                    Glob.modalitaAttuazione = "2";

                if (flagTriangoliDinamici == 0)
                    Glob.visualizzaTriangoli = "0";
                if (flagTriangoliDinamici == 1)
                    Glob.visualizzaTriangoli = "1";

                if (defzoom > 0.96 && defzoom < 1.03)
                    defzoom = 1;
                Glob.defzoom = Convert.ToString(defzoom);
            }
        }


        private void esciToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // file exit
            Application.Exit();
        }

        private void zoomToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void zoomToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void clickMouseToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clickMouseToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }


        private void touchToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(ForegroundWindow.Instance, ".......", "About...", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void modificaToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void touchToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }



    }



    // =================================================
    //	Class ForegroundWindow 
    //	recupero finestra in primo piano
    // =================================================

    public class ForegroundWindow : IWin32Window
    {
        private static ForegroundWindow _window = new ForegroundWindow();

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private ForegroundWindow() { }

        public static IWin32Window Instance
        {
            get { return _window; }
        }

        IntPtr IWin32Window.Handle
        {
            get { return GetForegroundWindow(); }
        }
    }

}
