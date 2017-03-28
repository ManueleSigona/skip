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
        List<Tastiera> tastiere = new List<Tastiera>(); // le tastiere (bisogna creare la classe tastiera, che deve contenere i tasti)
        bool completamentoOrt = true; // se usare o no il completamento ortogonale
        int num_completamento; // il numero di tasti da inserire nel completamento ortogonale
        // ---------------------------------------------


        Point posizioneMouse;
        BufferedGraphicsContext myContext;
        BufferedGraphics myBuffer;
        int tastoDaAttivare = 0;
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
        int numTasti;       // num dei tasti esagonali
        TastiEsagonali[] tasti;

        int getSettore;     // servono per la nuova modifica di curat
        int getTraccia;

        int indice = 0;
        IntPtr aggancio = IntPtr.Zero;      // rappresenta un handle inizializz a 0
        public enum Modalita { zeroClick, unClick, touch }     // perchè Modalita.zeroClick restituisce 0 mentre Modalita.unClick = 1
        public Modalita modalita;
        int tracciaCorrente, settoreCorrente, tracciaSalvata, settoreSalvato;
        string psillabaCorrente, psillabaSalvata;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        System.Timers.Timer timer2 = new System.Timers.Timer();
        IntPtr handleTastiera;
        int numTastiRotondi;
        TastiRotondi[] tastiRotondi;
        int indiceRotondi = 0;
        int tastoRotondoDaAttivare = 0;
        string carattereSalvato, carattereCorrente;

        int numTastiRettangolari;
        string coloreTestoMobile = "red"; //XYXYXY e uso 
        string coloreTestoTrasp = "black";  //XYXYXY e uso

        Space tastoSpace;
        //int indiceSpace = 0;        // serviva solam quando c erano 2 tasti spazio in leggiConfig
        int tastoSpaceDaAttivare = 0;

        int indiceTastiRettangolari = 0;

        BackSpace tastoBackSpace;
        int tastoBackSpaceDaAttivare = 0;

        Invio tastoInvio;
        int tastoInvioDaAttivare = 0;

        Shift tastoShift;       // servira come il tasto lock a chiamare metodi come perimetro o testo dei tasti shift
        int tastoShiftDaAttivare = 0;

        Lock tastoLock;
        int tastoLockDaAttivare = 0;

        bool flagshift = false;
        bool flaglock = false;
        SizeF dimensionetestoShift;
        PointF posizioneTestoShift;
        SizeF dimensionetestoLock;
        PointF posizioneTestoLock;
        int cont_c1 = 0;
        int cont_c2 = 0;
        int cont_c3 = 0;

        int cont_v1 = 0;
        int cont_v2 = 0;
        int cont_v3 = 0;

        static Cursor cur;  // sta var serve a portare il cursore custom in tasti esag e nelle altre classi (con 1 metodo)


        int numeroTracce = 0;       //PROVA per vedere se si sbaglia a vedere la pos del mouse nelle tracce anke qui (usato in "leggi config" e in "VisualTasto"

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
            if (tastoDaAttivare >= 0)   // se c'è un tasto da attivare ( se il ptatore si trova sopra il tasto)...
            {
                for (int i = 0; i < numTasti; i++)
                {   // "tastoDaAttivare" = indice del tasto esag da attivare
                    if (tastoDaAttivare == i)   //... lo attiva ( c è una var ke dice quel'è)...
                        tasti[i].attivaTasto();
                    else                        // ...e quindi mette tutti gli altri tasti trasparenti...
                        tasti[i].rendiTrasparente();
                }
            }

            for (int i = 0; i < numTastiRotondi; i++)
            {
                if (tastoRotondoDaAttivare >= 0)        // se cè un tasto rotondo da attivare lo attivi
                {
                    if (tastoRotondoDaAttivare == i)
                    {
                        tastiRotondi[i].attivaTasto();
                    }
                }
                else if (tastoDaAttivare >= 0)               // e tutti gli altri li rendi trasparenti. (Usa sto tasto da attivare >=0 perchè magari per errore possono esserci + tasti attivi se passi il mouse troppo in fretta forse)
                {
                    tastiRotondi[i].rendiTrasparente();
                }
            }

            //XXX  if (tastoSpace.stato == Space.Stato.Temporaneo)
            if (tastoSpaceDaAttivare >= 1)
            {
                tastoSpace.attivaTasto();
            }
            else if (tastoDaAttivare >= 0)
            {
                tastoSpace.rendiTrasparente();
            }

            // per il tasto backspace ke è uno solo non cè bisogno del for
            //XXX if (tastoBackSpace.stato == BackSpace.Stato.Temporaneo) // se lo stato del backspace è temporaneo lo porti attivo...
            if (tastoBackSpaceDaAttivare >= 1)
            {
                tastoBackSpace.attivaTasto();
            }
            else if (tastoDaAttivare >= 0)   // e tutti gli altri trasparenti
            {
                tastoBackSpace.rendiTrasparente();
            }

            //XXX if (tastoInvio.stato == Hex.Invio.Stato.Temporaneo)   // serve perchè sennò appena vai in preselez non diventa subito trasparente
            if (tastoInvioDaAttivare >= 1)
            {
                tastoInvio.attivaTasto();
            }
            else if (tastoDaAttivare >= 0)
            {
                tastoInvio.rendiTrasparente();
            }

            //XXX if (tastoShift.stato == Shift.Stato.Temporaneo)
            if (tastoShiftDaAttivare >= 1)
            {
                tastoShift.attivaTasto();
            }
            else if (tastoDaAttivare >= 0)
            {
                tastoShift.rendiTrasparente();
            }

            //XXX if (tastoLock.stato == Lock.Stato.Temporaneo)   // se il Lock è temporaneo diventa attivo perchè questo evento è sollevato quando finisce timer1
            if (tastoLockDaAttivare >= 1)
            {
                tastoLock.attivaTasto();
            }
            else if (tastoDaAttivare >= 0)
            {
                tastoLock.rendiTrasparente();
            }

            timer1.Stop();  // una volta attivato il tasto da attivare stoppi l'evento timer abbiamo il nostro tasto attivo quindi...

            if (modalita == Modalita.zeroClick)     //... se la modalita settata nel config è la "zeroClick"...
            {
                timer2.Start();                 // parte un secondo timer usato per Attivazione una scelta


                if (tastoDaAttivare >= 0)   // se è anc lì il ptatore
                {
                    tracciaSalvata = tracciaCorrente;
                    settoreSalvato = settoreCorrente;
                    psillabaSalvata = psillabaCorrente;
                }
                if (tastoRotondoDaAttivare >= 0)
                {
                    carattereSalvato = carattereCorrente;
                }

            }
            Invalidate();     //forza il ridisegno della tastiera così il cambiam di stato ha 1 riscontro visivo       
        }



        // -------------------------------------------------
        //	operazioni da eseguire al caricamento della form, 
        //	PRIMO EVENTO SOLLEVATO
        // -------------------------------------------------


        private void Form1_Load(object sender, EventArgs e)
        {

            leggiConfigurazione("prova.cfg"); // leggi configurazione e tastiera

            textBox1.Text = "Tp: " + tempoPreselezione.ToString() + " ms";
            textBox2.Text = "Ta: " + tempoAttivazione.ToString() + " ms";
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);  // genera l'evento ogni 2000 ms (dal file config)
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);
            //this.LostFocus += new EventHandler(Form1_LostFocus);    //gestisce il cambio di foregroundWindow (finestra in primo piano)
            TopMost = true;
            this.menuStrip1.BackColor = colormenu;
            this.menuStrip1.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.menuStrip1.ForeColor = colorfontmenu;


            indice = 0;
            indiceRotondi = 0;
            indiceTastiRettangolari = 0;
            Glob.poslettere = Convert.ToInt32(Glob.poslettere * defzoom);
            if (Glob.wina > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height * 0.35 && Glob.winl > System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width * 0.35)
            {

                for (int i = 0; i < numTastiRotondi; i++)
                    tastiRotondi[i].ModTastRot(defzoom, Glob.dimcar);
                for (int i = 0; i < numTasti; i++)
                    tasti[i].ModTastEsag(defzoom, Glob.dimcar);
                tastoSpace.ModTastSpace(defzoom, Glob.dimcar);
                tastoBackSpace.ModTastBackSpace(defzoom, Glob.dimcar);
                tastoShift.ModTastShift(defzoom, Glob.dimcar);
                tastoLock.ModTastLock(defzoom, Glob.dimcar);
                tastoInvio.ModTastInvio(defzoom, Glob.dimcar);

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


        private void panel1_Paint(object sender, PaintEventArgs e)      //panel: oggetto grafico invisibile ke permette di inserire facilmente altri controlli
        {

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
        //	gestione del doppio buffer per ridisegnare la tastiera,
        //	(non fa parte del mouseMove)
        // -------------------------------------------------

        public void visualizzaTastiera()
        {
            myContext = new BufferedGraphicsContext();  // crei un oggetto di ql tipo
            myBuffer = myContext.Allocate(panel1.CreateGraphics(), panel1.DisplayRectangle);    // la prima crea l'oggetto ke controllerò la seconda l'area associata a quell ogg x controllarlo, faccio tutti questi con allocate, PRATICAM SAREBBE L'AREA DELLA FORM1...
            myBuffer.Graphics.FillRectangle(new SolidBrush(coloreBackground), panel1.DisplayRectangle); // INFATTI COLORA L'AREA DI MYBUFFER CON "coloreBackground" NEL FILE CONFIG KE SAREBBE L'AREA DELLA FORM1

            /*foreach (TastiEsagonali t in tasti)
            {
               visualizzaTasto(myBuffer, t);
            }

            foreach (TastiRotondi t in tastiRotondi)
            {
               visualizzaTastoRotondo(myBuffer, t);
            }*/

            visualizzaTastoBackSpace(myBuffer, tastoBackSpace);
            visualizzaTastoShift(myBuffer, tastoShift);
            visualizzaTastoLock(myBuffer, tastoLock);

            visualizzaTastoInvio(myBuffer, tastoInvio);
            visualizzaTastoSpazio(myBuffer, tastoSpace);


            foreach (TastiRotondi t in tastiRotondi)
            {
                if (t.perimetro.IsVisible(posizioneMouse))
                {
                    visualizzaTastoRotondo(myBuffer, t);
                }
            }

            foreach (TastiRotondi t in tastiRotondi)    // metto tutti i tasti rotondi in myBuffer appunto x visualizzarli fisicam nella form
            {
                if (!t.perimetro.IsVisible(posizioneMouse))
                {
                    visualizzaTastoRotondo(myBuffer, t);
                }

            }



            foreach (TastiEsagonali t in tasti)
            {
                if (t.stato != TastiEsagonali.Stato.Attivo)// || t.perimetri[1].IsVisible(posizioneMouse))       // se il mouse si trova sul tasto..
                {

                    //Cursor.Current = this.Cursor;
                    visualizzaTasto(myBuffer, t);
                }
            }

            foreach (TastiEsagonali t in tasti)     //DISEGNI PER ULTIMO IL TASTO ATTIVO, ALMENO VIENE SOVRAPPOSTO A TUTTI GLI ALTRI
            {
                if (t.stato == TastiEsagonali.Stato.Attivo)// && t.perimetri[1].IsVisible(posizioneMouse))       // guardi se il ptatore è dentro a 1 tasto esag
                {
                    // se è dentro lo disegni per primo altrimenti non fai niente

                    visualizzaTasto(myBuffer, t);

                    //X = t.xCentro;
                    //Y = t.yCentro;
                }
            }

            myBuffer.Render(panel1.CreateGraphics());   // dovrebbe scrivere il tutti fisicamente
            myBuffer.Dispose();     // si libera delle risorse inutili prima ke lo faccia il garbage collector
            myContext.Dispose();
        }



        // -------------------------------------------------
        //	visualizza tasto
        // -------------------------------------------------

        public void visualizzaTasto(BufferedGraphics mB, TastiEsagonali t)  // si parla di tasti esag
        {
            switch (t.stato)
            {
                case TastiEsagonali.Stato.Inattivo:
                    t.dimensioneTesto = mB.Graphics.MeasureString(t.testo[0][0], t.f);       // calcolo dimTesto

                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetri[0]);

                    t.posizioneTesto.X = t.xCentro - (t.dimensioneTesto.Width / 2);      // calcolo posTesto
                    t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2);

                    if (flagshift == false && flaglock == false)    // mette tutti la string minuscola
                        mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(coloreFont), t.posizioneTesto);

                    else
                    {
                        if (flagshift == true && flaglock == false)
                        {
                            // mette solo 1 lettera del vettore di char maiuscola le altre minuscole
                            string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                            mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                        }
                        if (flaglock == true)
                        {
                            // mette tutti il vettore di char maiuscolo
                            string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                            mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                        }
                    }

                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetri[0]);    // colore il perimetro del tasto di nero sbolly
                    break;

                case TastiEsagonali.Stato.Attivo:       // come l'inattivo, cambia solo il colore dell'area del tasto
                                                        //-----------------------------

                    Cursor.Current = this.Cursor;   // mette il puntino
                    t.posizioneTesto.Y -= Glob.poslettere;


                    //-----------------------------
                    if (modalita == Modalita.unClick)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo1), t.perimetri[0]);
                    }
                    else
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetri[0]);
                    }

                    if (modalita == Modalita.touch)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo1), t.perimetri[0]);
                    }
                    else
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetri[0]);
                    }


                    if (flagshift == false && flaglock == false)
                    {
                        //ddbb
                        mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto); //111111111111111111111111

                    }
                    else
                    {
                        if (flagshift == true && flaglock == false)
                        {
                            string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                            mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                        }
                        if (flaglock == true)
                        {
                            //ddbb
                            string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                            mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);

                        }
                    }
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetri[0]);

                    getTraccia = tasti[tastoDaAttivare].GetTraccia(posizioneMouse);     // viene 0 e quello sotto 1 ( e quindi viene quasi giusto) perchè non viene chiamato il metodo "modificaTastoEsag" quindi traccia non è aggiornato e l'errore non si verifica
                    getTraccia = tasti[tastoDaAttivare].getTracciaGiusta();     // nuove modifiche, serve per risolvere il problema sett 0-5
                    getSettore = tasti[tastoDaAttivare].getSettoreGiusto();

                    /*if (tasti[tastoDaAttivare].stato == TastiEsagonali.Stato.Attivo)      //PROVA per vedere se legge la traccia giusta  --- SBAGLIATA non mi legge + i triangolini + flickering)
                    {
                        for (int i = 1; i < numeroTracce; i++) 
                        {
                            if (t.perimetri[i].IsVisible(posizioneMouse))
                                getTraccia = i;
                        }
                    }*/

                    for (int j = 0; j < 6; j++)
                    {
                        if ((j == getSettore) && (getTraccia == 1))
                        {
                            /*  PROVA  foreach (TastiEsagonali tastiEsag in tasti)     // creo i tasti esagonali per la modalita 1 click e zero click
                            {
                                if (modalita == Modalita.unClick)
                                    tastiEsag.disegna(panel1.CreateGraphics(), coloreTastoInattivo, coloreTastoTemporaneo, coloreTastoAttivo1, coloreBackground);    // passi i parametri letti dal file config al metodo disegna nella classe TastiEsagonali e te li disegna
                                if (modalita == Modalita.zeroClick)
                                    tastiEsag.disegna(panel1.CreateGraphics(), coloreTastoInattivo, coloreTastoTemporaneo, coloreTastoAttivo, coloreBackground);
                            }*/
                            for (int i = 0; i < 3; i++)
                            {
                                //mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetri1[j, 1, i]);
                                if (flagTriangoliDinamici == 1)
                                {
                                    mB.Graphics.DrawPath(new Pen(coloreTriangoliDinamici, 1), t.perimetri1[j, 1, i]);
                                }

                                if (flagshift == false && flaglock == false)
                                    mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto); //2222222222222222222222222
                                else
                                {
                                    if (flagshift == true && flaglock == false)
                                    {
                                        string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                    }
                                    if (flaglock == true)
                                    {
                                        string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                                        mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                    }
                                }
                            }

                            if (j == 0)   // ho tentato di far apparire il bezier sopra i triangolini
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[0, 0], posizioneMouse, posizioneMouse, t.vertici[1, 0]);

                                //posizioneMouse.X = posizioneMouse.X - 1;    //PROVA

                                t.perimetri[0].AddLine(t.vertici[0, 0].X, t.vertici[0, 0].Y, posizioneMouse.X + 1, posizioneMouse.Y);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse.X + 1, posizioneMouse.Y, t.vertici[1, 0].X, t.vertici[1, 0].Y);
                                //posizioneMouse.X = posizioneMouse.X + 1;
                            }
                            if (j == 1)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[1, 0], posizioneMouse, posizioneMouse, t.vertici[2, 0]);

                                t.perimetri[0].AddLine(t.vertici[1, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[2, 0]);

                            }
                            if (j == 2)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[2, 0], posizioneMouse, posizioneMouse, t.vertici[3, 0]);

                                t.perimetri[0].AddLine(t.vertici[2, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[3, 0]);

                            }
                            if (j == 3)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[3, 0], posizioneMouse, posizioneMouse, t.vertici[4, 0]);

                                t.perimetri[0].AddLine(t.vertici[3, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[4, 0]);

                            }
                            if (j == 4)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[4, 0], posizioneMouse, posizioneMouse, t.vertici[5, 0]);

                                t.perimetri[0].AddLine(t.vertici[4, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[5, 0]);
                            }
                            if (j == 5)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[5, 0], posizioneMouse, posizioneMouse, t.vertici[0, 0]);

                                t.perimetri[0].AddLine(t.vertici[5, 0].X, t.vertici[5, 0].Y, posizioneMouse.X + 1, posizioneMouse.Y);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse.X + 1, posizioneMouse.Y, t.vertici[0, 0].X, t.vertici[0, 0].Y);
                            }
                        }
                    }
                    for (int j = 0; j < 6; j++)
                    {
                        if ((getSettore == j) && (getTraccia == 2))
                        {
                            for (int i = 0; i < 3; i++)
                            {
                                //mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetri1[j, 1, i]);
                                if (flagTriangoliDinamici == 1)
                                {
                                    mB.Graphics.DrawPath(new Pen(coloreTriangoliDinamici, 1), t.perimetri1[j, 1, i]);
                                }

                                if (flagshift == false && flaglock == false)
                                    mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                else
                                {
                                    if (flagshift == true && flaglock == false)
                                    {
                                        string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                    }
                                    if (flaglock == true)
                                    {
                                        //if (t.triangolo >= 0 && t.triangolo < 9)//ddbb
                                        //{
                                        string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                                        mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);

                                        //}
                                    }
                                }
                            }

                            for (int i = 0; i < 5; i++)
                            {
                                //mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetri2[j, 2, i]);
                                if (flagTriangoliDinamici == 1)
                                {
                                    mB.Graphics.DrawPath(new Pen(coloreTriangoliDinamici, 1), t.perimetri2[j, 2, i]);
                                }

                                if (Shift.tastoAttivo == 0 && Lock.tastoAttivo == 0)
                                    mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto); //3333333333333333333333333333
                                else
                                {
                                    if (Shift.tastoAttivo == 1 && Lock.tastoAttivo == 0)
                                    {
                                        string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                    }
                                    if (Lock.tastoAttivo == 1)
                                    {
                                        string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                                        mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(Color.FromName(coloreTestoMobile)), t.posizioneTesto);
                                    }
                                }
                            }

                            if (j == 0)   // ho tentato di far apparire il bezier sopra i triangolini
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[0, 0], posizioneMouse, posizioneMouse, t.vertici[1, 0]);

                                t.perimetri[0].AddLine(t.vertici[0, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[1, 0]);
                            }
                            if (j == 1)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[1, 0], posizioneMouse, posizioneMouse, t.vertici[2, 0]);

                                t.perimetri[0].AddLine(t.vertici[1, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[2, 0]);
                            }
                            if (j == 2)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[2, 0], posizioneMouse, posizioneMouse, t.vertici[3, 0]);

                                t.perimetri[0].AddLine(t.vertici[2, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[3, 0]);
                            }
                            if (j == 3)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[3, 0], posizioneMouse, posizioneMouse, t.vertici[4, 0]);

                                t.perimetri[0].AddLine(t.vertici[3, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[4, 0]);
                            }
                            if (j == 4)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[4, 0], posizioneMouse, posizioneMouse, t.vertici[5, 0]);

                                t.perimetri[0].AddLine(t.vertici[4, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[5, 0]);
                            }
                            if (j == 5)
                            {
                                mB.Graphics.DrawPath(new Pen(Brushes.Transparent, 2), t.perimetri[0]);
                                //t.perimetri[0].AddBezier(t.vertici[5, 0], posizioneMouse, posizioneMouse, t.vertici[0, 0]);

                                t.perimetri[0].AddLine(t.vertici[5, 0], posizioneMouse);     // al posto di bezier uso questi!!
                                t.perimetri[0].AddLine(posizioneMouse, t.vertici[0, 0]);
                            }
                        }
                    }
                    break;

                case TastiEsagonali.Stato.Trasparente:
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), t.perimetri[0]);        //sbolly non cè bisogno di scrivere la string dentro il tasto (saltano tutti gli if di prima) o settare il colore, coloro solo il perimetro in nero ma meno spesso ke negli altri stati
                    break;

                case TastiEsagonali.Stato.Temporaneo:       // come l'inattivo e l'attivo, cambia solo il colore dell'area del tasto
                    t.dimensioneTesto = mB.Graphics.MeasureString(t.testo[0][0], t.f);
                    t.posizioneTesto.X = t.xCentro - (t.dimensioneTesto.Width / 2);
                    //t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2 + Glob.poslettere);
                    t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2);
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), t.perimetri[0]);
                    if (flagshift == false && flaglock == false)
                        mB.Graphics.DrawString(t.testo[t.settore][t.triangolo], t.f, new SolidBrush(Color.FromName(coloreTestoTrasp)), t.posizioneTesto);
                    else
                    {
                        if (flagshift == true && flaglock == false)
                        {
                            string testoShift = char.ToUpper(t.testo[t.settore][t.triangolo][0]) + t.testo[t.settore][t.triangolo].Substring(1, t.testo[t.settore][t.triangolo].Length - 1);
                            mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(Color.FromName(coloreTestoTrasp)), t.posizioneTesto);
                        }
                        if (flaglock == true)
                        {
                            string testoLock = t.testo[t.settore][t.triangolo].ToUpper();
                            mB.Graphics.DrawString(testoLock, t.f, new SolidBrush(Color.FromName(coloreTestoTrasp)), t.posizioneTesto);
                        }
                    }
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetri[0]); //sbolly
                    break;
            }

            t.dimensioneTesto = mB.Graphics.MeasureString(t.testo[t.settore][t.triangolo], t.f);

            /** XXXXXXXX prove per  visualizzazione finestra accumulo

//             mB.Graphics.FillRectangle(Brushes.Black, 
//	          t.posCharStringAccumulo.X, t.posCharStringAccumulo.Y,
//		  t.dimCharStringAccumulo.Width - t.empty.Width / 2, 
//		  t.dimCharStringAccumulo.Height);
//             mB.Graphics.DrawString(sb[i].ToString(), fontTasto, 
//		  Brushes.White, t.posCharStringAccumulo);

                    PointF pos = new PointF(20, 20);
                           mB.Graphics.FillRectangle(Brushes.White, 
                          300,30,
                      200, 
                      50);
                           mB.Graphics.DrawString("Hello World", fontTasto, 
                      Brushes.Black, pos);

             *********************************************************/


        }



        // -------------------------------------------------
        //	visualizza tasto rotondo
        // -------------------------------------------------

        public void visualizzaTastoRotondo(BufferedGraphics mB, TastiRotondi t)
        {
            switch (t.stato)
            {
                case TastiRotondi.Stato.Inattivo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);     // se il tasto è inattivo coloro così la sua area

                    if (flagshift == false && flaglock == false)      // se lo shift e il lock non sono attivi allora scrivo la stringa minuscola dentro il tasto...
                        mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);      // posizTesto gia calcol prima
                    else         // altrimenti maiuscola
                    {
                        string testoShift = t.testo.ToUpper();
                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    }
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);   // sbolly in ogni caso coloro di nero i perimetro del tasto
                    break;
                case TastiRotondi.Stato.Attivo:     // stessa cosa se è attivo ma con l'area colorata diversam
                    //-----------------------------
                    //Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    if (flagshift == false && flaglock == false)
                        mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    else
                    {
                        string testoShift = t.testo.ToUpper();
                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    }
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);// sbolly
                    break;
                case TastiRotondi.Stato.Trasparente:    // se è trasparente non coloro l'area e non scrivo stringa dentro, coloro solo il perimetro di nero ma meno spesso ke negli altri casi
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), t.perimetro); //sbolly
                    break;
                case TastiRotondi.Stato.Temporaneo:     // come nel caso inattivo e attivo, cambio solo il colore dell'area
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), t.perimetro);
                    if (flagshift == false && flaglock == false)
                        mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    else
                    {
                        string testoShift = t.testo.ToUpper();
                        mB.Graphics.DrawString(testoShift, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    }
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro); //sbolly
                    break;
            }
        }



        // -------------------------------------------------
        //	 visualizza tasto spazio
        // -------------------------------------------------

        public void visualizzaTastoSpazio(BufferedGraphics mB, Space t)
        {
            t.dimensioneTesto = mB.Graphics.MeasureString(t.testo, t.f);
            t.posizioneTesto.X = t.xCentro - (t.dimensioneTesto.Width / 2);
            t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2);

            switch (t.stato)
            {
                case Space.Stato.Inattivo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro); //sbolly
                    break;
                case Space.Stato.Attivo:
                    //-----------------------------
                    //Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
                case Space.Stato.Trasparente:
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), t.perimetro);
                    break;
                case Space.Stato.Temporaneo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
            }
        }



        // -------------------------------------------------
        //	 visualizza tasto backspace
        // -------------------------------------------------

        public void visualizzaTastoBackSpace(BufferedGraphics mB, BackSpace t)
        {
            t.dimensioneTesto = mB.Graphics.MeasureString(t.testo, t.f);
            t.posizioneTesto.X = t.xCentro - (t.dimensioneTesto.Width / 2);
            t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2);
            switch (t.stato)
            {
                case BackSpace.Stato.Inattivo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
                case BackSpace.Stato.Attivo:
                    //-----------------------------
                    //Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
                case BackSpace.Stato.Trasparente:
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), t.perimetro);
                    break;
                case BackSpace.Stato.Temporaneo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
            }
        }



        // -------------------------------------------------
        //	 visualizza tasto Invio
        // -------------------------------------------------

        public void visualizzaTastoInvio(BufferedGraphics mB, Invio t)
        {
            t.dimensioneTesto = mB.Graphics.MeasureString(t.testo, t.f);
            t.posizioneTesto.X = t.xCentro - (t.dimensioneTesto.Width / 2);
            t.posizioneTesto.Y = t.yCentro - (t.dimensioneTesto.Height / 2);
            switch (t.stato)
            {
                case Hex.Invio.Stato.Inattivo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
                case Hex.Invio.Stato.Attivo:
                    //-----------------------------
                    //Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
                case Hex.Invio.Stato.Trasparente:
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), t.perimetro);
                    break;
                case Hex.Invio.Stato.Temporaneo:
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), t.posizioneTesto);
                    mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), t.perimetro);
                    break;
            }
        }



        // -------------------------------------------------
        //	 visualizza tasto shift
        // -------------------------------------------------

        public void visualizzaTastoShift(BufferedGraphics mB, Shift t)
        {
            dimensionetestoShift = mB.Graphics.MeasureString(t.testo, fontTasto);
            posizioneTestoShift.X = tastoShift.xCentro - (dimensionetestoShift.Width / 2);
            posizioneTestoShift.Y = tastoShift.yCentro - (dimensionetestoShift.Height / 2);

            switch (tastoShift.stato)
            {
                case Shift.Stato.Inattivo:
                    if (flagshift == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoShift.perimetro);
                    }
                    break;

                case Shift.Stato.Temporaneo:
                    if (flagshift == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);

                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoInattivo), 4), tastoShift.perimetro);

                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);
                    }
                    else if (flagshift == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoShift.perimetro);
                    }

                    break;

                case Shift.Stato.Attivo:
                    if (flagshift == false)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), tastoShift.perimetro);
                        mB.Graphics.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoShift.perimetro);
                    }

                    break;

                case Shift.Stato.Trasparente:
                    if (flagshift == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoShift.perimetro);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoInattivo), 3), tastoShift.perimetro);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoShift.perimetro);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoAttivo), 3), tastoShift.perimetro);
                    }

                    break;
            }
            /*switch (t.stato)
            {
                case Shift.Stato.Attivo:
                    //-----------------------------
                    Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), posizioneTestoShift);   // qui avevo scritto: t.posizioneTesto, era questo ke mi sparava una scritta shift in alto
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                    break;
            }
            if (flagshift == false)
            {
                
                if (TastiEsagonali.tastiAttivi == 1)    // lo shift è trasparente (niente stringa dentro il tasto)...
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreBackground), 3), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 1), t.perimetro);
                }
                else     // ... altrim è normale (come quando apri la form1) stesso colore del background ( con stringa dentro il tasto)
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                }
            }
            if (flagshift == true)
            {
                if (TastiEsagonali.tastiAttivi == 1)    // se flagshift = true  e tastoEsag attivo --> shift è trasparente e il perimetro colorato di cyan...
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoAttivo), 3), t.perimetro);
                }
                else                              // se flagshift = true e il tasto esag non è attivo l'area del tasto shift rimane attiva
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), posizioneTestoShift);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                }
            }*/
        }



        // -------------------------------------------------
        //	 visualizza tasto lock
        // -------------------------------------------------

        public void visualizzaTastoLock(BufferedGraphics mB, Lock t)
        {
            dimensionetestoLock = mB.Graphics.MeasureString(t.testo, fontTasto);
            posizioneTestoLock.X = tastoLock.xCentro - (dimensionetestoLock.Width / 2);
            posizioneTestoLock.Y = tastoLock.yCentro - (dimensionetestoLock.Height / 2);

            switch (tastoLock.stato)
            {
                case Lock.Stato.Inattivo:
                    if (flaglock == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoLock.perimetro);
                    }
                    break;

                case Lock.Stato.Temporaneo:
                    if (flaglock == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);

                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoInattivo), 4), tastoLock.perimetro);

                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }
                    else if (flaglock == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoTemporaneo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoLock.perimetro);
                    }

                    break;

                case Lock.Stato.Attivo:
                    if (flaglock == false)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), tastoLock.perimetro);
                        mB.Graphics.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoTemporaneo), 4), tastoLock.perimetro);
                    }

                    break;

                case Lock.Stato.Trasparente:
                    if (flaglock == false)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoLock.perimetro);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoInattivo), 3), tastoLock.perimetro);
                        mB.Graphics.DrawPath(new Pen(colorcontornobott, 1), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), tastoLock.perimetro);
                        mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoAttivo), 3), tastoLock.perimetro);
                    }

                    break;
            }
            /*switch (t.stato)
            {
                case Lock.Stato.Attivo:
                    //-----------------------------
                    Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), posizioneTestoLock);   // qui avevo scritto: t.posizioneTesto, era questo ke mi sparava una scritta shift in alto
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                    break;
            }

            if (flaglock == false)
            {
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreBackground), 3), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 1), t.perimetro);
                }
                else
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoInattivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                }
            }
            if (flaglock == true)
            {
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreBackground), t.perimetro);
                    mB.Graphics.DrawPath(new Pen(new SolidBrush(coloreTastoAttivo), 3), t.perimetro);
                }
                else
                {
                    mB.Graphics.FillPath(new SolidBrush(coloreTastoAttivo), t.perimetro);
                    mB.Graphics.DrawString(t.testo, t.f, new SolidBrush(coloreFont), posizioneTestoLock);
                    mB.Graphics.DrawPath(new Pen(Brushes.Black, 2), t.perimetro);
                }
            }*/
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

                StreamReader leggi = new StreamReader(File.OpenRead(Path.Combine(Glob.CartellaLocale_text, s)));
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
                                fontTasto = new Font(dati[0], Convert.ToSingle(dati[1]), GraphicsUnit.Point);
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
                                //tastiere = new Tastiera[num_tastiere];
                                for (int i = 0; i < num_tastiere; i++)
                                { // creiamo tutte le tastiere (per adesso non avranno ancora tasti dentro)
                                    tastiere.Add(new Tastiera(num_righe, num_colonne));
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
                                for (int i = 0; i < num_righe - 1; i++) // leggiamo riga per riga la tastiera
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
                                        // TODO: istanziare nuovo tasto passandogli cosa deve esserci scritto (tastiLetti[j]) e la posizione (dipenderà da i e da j)
                                        // bisogna prima creare la classe TastoRettangolare

                                        //tastiere[k].tasti[i, j] = nuovoTasto; //aggiungiamo alla tastiera corrente il nuovo tasto appena creato
                                        tastiere[k].aggiungiTasto(new Tasto(i, j, tastiLetti[j]));
                                    }
                                }
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
                                        // TODO: istanziare nuovo complet ort e assegnarlo al tasto corrispondente
                                        tastiere[k].tasti[prima_riga + i, prima_colonna + j].completamento = new Completamento(tastiOrtLetti);
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

            if (numTasti != indice)
            {
                MessageBox.Show(ForegroundWindow.Instance, "Il numero di tasti esaogonali dichiarato nella tastiera non corrisponde al numero di tasti correttamente descritti!", "Errore nel file di configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                chiudi = true;
            }

            if (numTastiRotondi != indiceRotondi)
            {
                MessageBox.Show(ForegroundWindow.Instance, "Il numero di tasti rotondi dichiarato nella tastiera non corrisponde al numero di tasti correttamente descritti!", "Errore nel file di configurazione", MessageBoxButtons.OK, MessageBoxIcon.Error);
                chiudi = true;
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
            string testo = "";
            if (tastoDaAttivare >= 0)
            {
                testo = psillabaCorrente;
                if (tastoDaAttivare < 15)       // consonanti
                {
                    if (testo.Length == 1)
                        cont_c1 += 1;
                    if (testo.Length == 2)
                        cont_c2 += 1;
                    if (testo.Length == 3)
                        cont_c3 += 1;
                }
                else
                {
                    if (testo.Length == 1)
                        cont_v1 += 1;
                    if (testo.Length == 2)
                        cont_v2 += 1;
                    if (testo.Length == 3)
                        cont_v3 += 1;
                }
                tasti[tastoDaAttivare].disattiva();
            }
            if (tastoRotondoDaAttivare >= 0)
            {
                testo = carattereCorrente;
                tastiRotondi[tastoRotondoDaAttivare].disattiva();
            }
            if (tastoSpaceDaAttivare == 1)
            {
                testo = " ";
                tastoSpace.disattiva();
            }

            if (tastoBackSpaceDaAttivare == 1)
            {
                tastoBackSpace.disattiva();
            }

            if (tastoInvioDaAttivare == 1)
            {
                tastoInvio.disattiva();
            }

            if (tastoShiftDaAttivare == 1)
            {
                //tastoShift.rendiTemporaneo();
                tastoShift.disattiva();         //quasi risolto
                panel1.CreateGraphics().FillPath(new SolidBrush(coloreTastoTemporaneo), tastoShift.perimetro);

            }

            if (tastoLockDaAttivare == 1)
            {
                //tastoLock.rendiTemporaneo();
                tastoLock.disattiva();         //quasi risolto
                panel1.CreateGraphics().FillPath(new SolidBrush(coloreTastoTemporaneo), tastoLock.perimetro);
            }

            timer2.Stop();
            timer1.Stop();
            if (aggancio != handleTastiera && testo != "")  // infatti con sendmessage gli invii il testo, ma se non cè testo non invii quindi niente
            {
                if (flagshift == false && flaglock == false)
                {
                    Thread.Sleep(Glob.Sleep_TIME);
                    //SetForegroundWindow(aggancio);
                    foreach (char c in testo)
                    {
                        Thread.Sleep(Glob.Sleep_TIME);
                        SendKeys.SendWait(c.ToString());    // gli manda ogni carattere del testo
                        Thread.Sleep(Glob.Sleep_TIME);
                    }
                    //SetForegroundWindow(this.handleTastiera);
                }
                else
                {
                    if (flagshift == true && flaglock == false)
                    {
                        if (tastoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);   //Consente di bloccare il thread corrente per il numero specificato di millisecondi.
                            //SetForegroundWindow(aggancio);      //SetForegroundWindow() attiva una finestra e forza la finestra in primo piano
                            string testoShift = char.ToUpper(testo[0]) + testo.Substring(1, testo.Length - 1);
                            foreach (char c in testoShift)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.handleTastiera);
                        }
                        if (tastoRotondoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoShift = testo.ToUpper();
                            foreach (char c in testoShift)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.handleTastiera);
                        }
                        /************ ????????? ****************	
                                        if (tastoSpaceDaAttivare == 1)
                                        {
                                            Thread.Sleep(Glob.Sleep_TIME);
                                            //SetForegroundWindow(aggancio);
                                            foreach (char c in testo)
                                            {
                                                Thread.Sleep(Glob.Sleep_TIME);
                                                SendKeys.SendWait(c.ToString());
                                                Thread.Sleep(Glob.Sleep_TIME);
                                            }
                                            //SetForegroundWindow(this.handleTastiera);
                                        }
                         ************ ????????? ****************/
                        flagshift = false;
                        Shift.tastoAttivo = 0;
                    }
                    if (flaglock == true)
                    {
                        if (tastoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoLock = testo.ToUpper();
                            foreach (char c in testoLock)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());    //possibile confezionare e spedire appositi messaggi alla finestra target.
                                Thread.Sleep(Glob.Sleep_TIME);                   //SendWait, che dopo aver inviato il messaggio attende che il thread destinatario abbia finito di processarlo
                            }
                            //SetForegroundWindow(this.handleTastiera);
                        }
                        if (tastoRotondoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoLock = testo.ToUpper();
                            foreach (char c in testoLock)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.handleTastiera);
                        }
                        /************ ????????? ****************	
                                        if (tastoSpaceDaAttivare == 1)
                                        {
                                            Thread.Sleep(Glob.Sleep_TIME);
                                            //SetForegroundWindow(aggancio);
                                            foreach (char c in testo)
                                            {
                                                Thread.Sleep(Glob.Sleep_TIME);
                                                SendKeys.SendWait(c.ToString());
                                                Thread.Sleep(Glob.Sleep_TIME);
                                            }
                                            //SetForegroundWindow(this.handleTastiera);
                                        }
                         ************ ????????? ****************/
                    }
                }
            }

            if (aggancio != handleTastiera && testo == "" && tastoBackSpace.perimetro.IsVisible(posizioneMouse))
            {
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(aggancio);
                Thread.Sleep(Glob.Sleep_TIME);
                SendKeys.SendWait("{BACKSPACE}");   //possibile confezionare e spedire appositi messaggi alla finestra target.
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(handleTastiera);

            }

            if (aggancio != handleTastiera && testo == "" && tastoInvio.perimetro.IsVisible(posizioneMouse))
            {
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(aggancio);
                Thread.Sleep(Glob.Sleep_TIME);
                SendKeys.SendWait("{ENTER}");   // {ENTER} fa parte dei caratteri speciali inviabili attraverso la classe SendKey
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(handleTastiera);

            }
            if (tastoDaAttivare >= 0)
            {
                timer1.Stop();
                timer1.Start();
            }
            if (tastoRotondoDaAttivare >= 0)
            {
                timer1.Stop();
                timer1.Start();
            }
            if (tastoSpaceDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }
            if (tastoBackSpaceDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoInvioDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoShiftDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoLockDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoShift.perimetro.IsVisible(posizioneMouse))
            {
                switch (flagshift)
                {
                    case false:
                        flagshift = true;
                        Shift.tastoAttivo = 1;
                        break;
                    case true:
                        flagshift = false;
                        Shift.tastoAttivo = 0;
                        break;
                }
                timer1.Stop();
                timer1.Start();
            }


            if (tastoLock.perimetro.IsVisible(posizioneMouse))
            {
                switch (flaglock)
                {
                    case false:
                        flaglock = true;
                        Lock.tastoAttivo = 1;
                        break;
                    case true:
                        flaglock = false;
                        Lock.tastoAttivo = 0;
                        break;
                }
                timer1.Stop();
                timer1.Start();
            }

            // XXXXXX Necessario per visualizzare il colore di tasto temporaneo dopo preselezione 0-click

            foreach (TastiEsagonali t in tasti)
            {
                t.modificaTasto(posizioneMouse.X, posizioneMouse.Y);
            }
            foreach (TastiRotondi t in tastiRotondi)
            {
                t.modificaTasto(posizioneMouse.X, posizioneMouse.Y);
            }

            tastoBackSpace.modificaTasto(posizioneMouse.X, posizioneMouse.Y);

            tastoInvio.modificaTasto(posizioneMouse.X, posizioneMouse.Y);
            tastoShift.modificaTasto(posizioneMouse.X, posizioneMouse.Y);
            tastoLock.modificaTasto(posizioneMouse.X, posizioneMouse.Y);
            tastoSpace.modificaTasto(posizioneMouse.X, posizioneMouse.Y);

            // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            Invalidate();
        }



        // -------------------------------------------------
        //	gestione del click
        // -------------------------------------------------

        void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            string testo = "";
            for (int i = 0; i < numTasti; i++)  // num tasti esag
            {
                if (tasti[i].controlloStato() == 1 && tasti[i].GetSettore(new Point(e.X, e.Y)) >= 0 && tasti[i].GetTraccia(new Point(e.X, e.Y)) >= 0)
                {
                    testo = tasti[i].ritornaStringa(tasti[i].GetSettore(new Point(e.X, e.Y)), tasti[i].getTriangolo(new Point(e.X, e.Y)));

                    if (i < 15)     // gestisce le consonanti
                    {
                        if (testo.Length == 1)
                            cont_c1 += 1;
                        if (testo.Length == 2)
                            cont_c2 += 1;
                        if (testo.Length == 3)
                            cont_c3 += 1;
                    }
                    else         // gestisce le vocali
                    {
                        if (testo.Length == 1)
                            cont_v1 += 1;
                        if (testo.Length == 2)
                            cont_v2 += 1;
                        if (testo.Length == 3)
                            cont_v3 += 1;
                    }
                    tasti[i].disattiva();       // disattiva perchè è inserita la pseudo-sillaba
                    i = numTasti;
                }
                else
                {
                    if (TastiEsagonali.tastiAttivi == 0 && tasti[i].perimetri[0].IsVisible(posizioneMouse)) //PROVO A RISCRIVERE COSì LA SECONDA PARTE  /*&& tasti[i].GetTraccia(new Point(e.X, e.Y)) == 0)*/   // ossia primo settore e non c sono tasti attivi
                    {
                        testo = tasti[i].ritornaStringa(tasti[i].GetSettore(new Point(e.X, e.Y)), tasti[i].GetTraccia(new Point(e.X, e.Y)));
                        if (i < 15)
                        {
                            if (testo.Length == 1)
                                cont_c1 += 1;
                            if (testo.Length == 2)
                                cont_c2 += 1;
                            if (testo.Length == 3)
                                cont_c3 += 1;
                        }
                        else
                        {
                            if (testo.Length == 1)
                                cont_v1 += 1;
                            if (testo.Length == 2)
                                cont_v2 += 1;
                            if (testo.Length == 3)
                                cont_v3 += 1;
                        }
                        timer2.Stop();
                        timer1.Stop();
                    }
                }

            }
            for (int i = 0; i < numTastiRotondi; i++)
            {

                if (tastiRotondi[i].stato == TastiRotondi.Stato.Attivo) // se è attivo
                {
                    testo = tastiRotondi[i].testo;
                    tastiRotondi[i].disattiva();
                    i = numTastiRotondi;
                }
                else
                {
                    // se i tasti rotondi non sono attivi e se sopra l'area cè il ptatore e il testo è ??
                    if (TastiRotondi.tastiAttivi == 0 && tastiRotondi[i].perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                    {
                        testo = tastiRotondi[i].testo;
                        timer2.Stop();
                        timer1.Stop();
                    }
                }
            }



            if (tastoSpace.stato == Space.Stato.Attivo)
            {
                testo = " ";    // ci piazzi 1 spazio se è attivo e ci clicchi su
                tastoSpace.disattiva();
            }
            else
            {   // se non è attivo ma il mouse è sopra alla sua area e ci clicchi su va lo stesso
                if (Space.tastiAttivi == 0 && tastoSpace.perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                {
                    testo = " ";
                    timer2.Stop();  // disattivi i 2 timer perchè la pseudo-sillaba è inserita
                    timer1.Stop();
                }
            }


            if (tastoBackSpace.stato == BackSpace.Stato.Attivo)
            {
                tastoBackSpace.disattiva();
            }
            else
            {
                if (BackSpace.tastoAttivo == 0 && tastoBackSpace.perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                {
                    timer2.Stop();
                    timer1.Stop();
                }
            }

            if (tastoInvio.stato == Hex.Invio.Stato.Attivo)
            {
                tastoInvio.disattiva();
            }
            else
            {
                if (Hex.Invio.Stato.Attivo == 0 && tastoInvio.perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                {
                    timer2.Stop();
                    timer1.Stop();
                }
            }

            if (tastoShift.stato == Shift.Stato.Attivo)
            {
                tastoShift.disattiva();
            }
            else
            {       //tastoAttivo vuol dire 1 altra cosa con lo shift
                if (Shift.tastoAttiv == 0 && tastoShift.perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                {
                    timer2.Stop();
                    timer1.Stop();
                }
            }

            if (tastoLock.stato == Lock.Stato.Attivo)
            {
                tastoLock.disattiva();
            }
            else
            {       //tastoAttivo vuol dire 1 altra cosa con lo shift
                if (Lock.tastoAttiv == 0 && tastoLock.perimetro.IsVisible(new Point(e.X, e.Y)) && testo == "")
                {
                    timer2.Stop();
                    timer1.Stop();
                }
            }

            if (aggancio != handleTastiera && testo != "")
            {
                if (flagshift == false && flaglock == false)
                {
                    Thread.Sleep(Glob.Sleep_TIME);
                    //SetForegroundWindow(aggancio);
                    foreach (char c in testo)
                    {
                        Thread.Sleep(Glob.Sleep_TIME);
                        SendKeys.SendWait(c.ToString());
                        Thread.Sleep(Glob.Sleep_TIME);
                    }
                    //SetForegroundWindow(this.Handle);
                }
                else
                {
                    if (flagshift == true && flaglock == false)
                    {
                        if (tastoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoShift = char.ToUpper(testo[0]) + testo.Substring(1, testo.Length - 1);   // se il flagshift è true allora manda le pseudo-sillabe con la prima lettera maiuscola

                            foreach (char c in testoShift)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);
                        }
                        if (tastoRotondoDaAttivare >= 0)    // se non ci sono tasti da attivare è impostato a "-1"
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoShift = testo.ToUpper();
                            foreach (char c in testoShift)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);
                        }
                        if (tastoSpaceDaAttivare == 1)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);  // allo spazio gli rimanda i caratteri senza la maiuscola anke se flagShift = true
                            foreach (char c in testo)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);
                        }
                        flagshift = false;
                        Shift.tastoAttivo = 0;
                    }
                    if (flaglock == true)
                    {
                        if (tastoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoLock = testo.ToUpper();
                            foreach (char c in testoLock)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);   // la foregroundWindow torna la solita
                        }
                        if (tastoRotondoDaAttivare >= 0)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            string testoLock = testo.ToUpper();
                            foreach (char c in testoLock)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);
                        }
                        if (tastoSpaceDaAttivare == 1)
                        {
                            Thread.Sleep(Glob.Sleep_TIME);
                            //SetForegroundWindow(aggancio);
                            foreach (char c in testo)
                            {
                                Thread.Sleep(Glob.Sleep_TIME);
                                SendKeys.SendWait(c.ToString());
                                Thread.Sleep(Glob.Sleep_TIME);
                            }
                            //SetForegroundWindow(this.Handle);
                        }
                    }
                }
            }

            if (aggancio != handleTastiera && testo == "" && tastoBackSpace.perimetro.IsVisible(posizioneMouse))
            {
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(aggancio);
                Thread.Sleep(Glob.Sleep_TIME);
                SendKeys.SendWait("{BACKSPACE}");
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(handleTastiera);
            }

            if (aggancio != handleTastiera && testo == "" && tastoInvio.perimetro.IsVisible(posizioneMouse))
            {
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(aggancio);
                Thread.Sleep(Glob.Sleep_TIME);
                SendKeys.SendWait("{ENTER}");
                Thread.Sleep(Glob.Sleep_TIME);
                //SetForegroundWindow(handleTastiera);
            }

            if (tastoDaAttivare >= 0)
            {
                timer1.Stop();
                timer1.Start();
            }
            if (tastoRotondoDaAttivare >= 0)
            {
                timer1.Stop();
                timer1.Start();
            }
            if (tastoSpaceDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoBackSpaceDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoInvioDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoShiftDaAttivare == 1)
            {
                timer1.Stop();
                timer1.Start();
            }

            if (tastoShift.perimetro.IsVisible(new Point(e.X, e.Y)))
            {
                switch (flagshift)
                {
                    case false:
                        flagshift = true;
                        Shift.tastoAttivo = 1;  //si lascia altrimenti appena clicchi sullo shift cambia in continuaz
                        break;
                    case true:
                        flagshift = false;
                        Shift.tastoAttivo = 0;
                        break;
                }
            }

            if (tastoLock.perimetro.IsVisible(posizioneMouse))
            {
                switch (flaglock)
                {
                    case false:
                        flaglock = true;
                        Lock.tastoAttivo = 1;
                        break;
                    case true:
                        flaglock = false;
                        Lock.tastoAttivo = 0;
                        break;
                }
                timer1.Stop();
                timer1.Start();
            }
            Invalidate();
        }



        // -------------------------------------------------
        //	visualizzazione tasto shift
        // -------------------------------------------------

        void visualizzaShift(Graphics dove, Color inattivo, Color temporaneo, Color attivo, Color trasparente)
        {
            dimensionetestoShift = dove.MeasureString(tastoShift.testo, fontTasto);
            posizioneTestoShift.X = tastoShift.xCentro - (dimensionetestoShift.Width / 2);
            posizioneTestoShift.Y = tastoShift.yCentro - (dimensionetestoShift.Height / 2);

            /*switch (tastoShift.stato)
            {
                case Shift.Stato.Attivo:
                    //-----------------------------
                    Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    dove.FillPath(new SolidBrush (attivo), tastoShift.perimetro);
                    dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                    dove.DrawPath(new Pen(Brushes.Black, 2), tastoShift.perimetro);

                    break;
            }*/
            switch (tastoShift.stato)
            {
                case Shift.Stato.Inattivo:

                    if (flagshift == false)
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoShift.perimetro);
                    }
                    break;

                case Shift.Stato.Temporaneo:
                    if (flagshift == false)
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);

                        dove.DrawPath(new Pen(new SolidBrush(inattivo), 4), tastoShift.perimetro);    // cornice per nascondere cyan

                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);

                    }

                    else if (flagshift == true)
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoShift.perimetro);
                    }

                    break;

                case Shift.Stato.Attivo:
                    if (flagshift == false)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        dove.FillPath(new SolidBrush(attivo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        dove.FillPath(new SolidBrush(attivo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoShift.perimetro);
                    }

                    break;

                case Shift.Stato.Trasparente:
                    if (flagshift == false)
                    {
                        dove.FillPath(new SolidBrush(trasparente), tastoShift.perimetro);
                        dove.DrawPath(new Pen(new SolidBrush(trasparente), 3), tastoShift.perimetro);
                        dove.DrawPath(new Pen(colorcontornobott, 1), tastoShift.perimetro);
                    }

                    else if (flagshift == true)
                    {
                        dove.FillPath(new SolidBrush(trasparente), tastoShift.perimetro);
                        dove.DrawPath(new Pen(new SolidBrush(attivo), 3), tastoShift.perimetro);
                    }

                    break;
            }
            /*if (flagshift == false)
            {
                
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    dove.FillPath(new SolidBrush(trasparente), tastoShift.perimetro);
                    dove.DrawPath(new Pen(new SolidBrush(trasparente), 3), tastoShift.perimetro);
                    dove.DrawPath(new Pen(Brushes.Black, 1), tastoShift.perimetro);
                }
                else
                {
                    if (tastoShift.perimetro.IsVisible(posizioneMouse))
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(Brushes.Black, 2), tastoShift.perimetro);
                    }

                    
                    else
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoShift.perimetro);
                        dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                        dove.DrawPath(new Pen(Brushes.Black, 2), tastoShift.perimetro);
                    }
                }
            }
            if (flagshift == true)
            {
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    dove.FillPath(new SolidBrush(trasparente), tastoShift.perimetro);
                    dove.DrawPath(new Pen(new SolidBrush(attivo), 3), tastoShift.perimetro);
                }
                else
                {
                    dove.FillPath(new SolidBrush(attivo), tastoShift.perimetro);
                    dove.DrawString(tastoShift.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoShift);
                    dove.DrawPath(new Pen(Brushes.Black, 2), tastoShift.perimetro);
                }
            }*/
        }



        // -------------------------------------------------
        //	visualizzazione tasto lock
        // -------------------------------------------------

        void visualizzaLock(Graphics dove, Color inattivo, Color temporaneo, Color attivo, Color trasparente)
        {
            dimensionetestoLock = dove.MeasureString(tastoLock.testo, fontTasto);
            posizioneTestoLock.X = tastoLock.xCentro - (dimensionetestoLock.Width / 2);
            posizioneTestoLock.Y = tastoLock.yCentro - (dimensionetestoLock.Height / 2);

            switch (tastoLock.stato)
            {
                case Lock.Stato.Inattivo:
                    if (flaglock == false)
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoLock.perimetro);
                    }
                    break;

                case Lock.Stato.Temporaneo:
                    if (flaglock == false)
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);

                        dove.DrawPath(new Pen(new SolidBrush(inattivo), 4), tastoLock.perimetro);    // cornice per nascondere cyan

                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoLock.perimetro);
                    }

                    break;

                case Lock.Stato.Attivo:
                    if (flaglock == false)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        dove.FillPath(new SolidBrush(attivo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(colorcontornobott, 2), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        //-----------------------------
                        //Cursor.Current = this.Cursor;   // mette il puntino
                        //-----------------------------
                        dove.FillPath(new SolidBrush(attivo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(new SolidBrush(temporaneo), 4), tastoLock.perimetro);
                    }
                    break;

                case Lock.Stato.Trasparente:
                    if (flaglock == false)
                    {
                        dove.FillPath(new SolidBrush(trasparente), tastoLock.perimetro);
                        dove.DrawPath(new Pen(new SolidBrush(trasparente), 3), tastoLock.perimetro);
                        dove.DrawPath(new Pen(colorcontornobott, 1), tastoLock.perimetro);
                    }

                    else if (flaglock == true)
                    {
                        dove.FillPath(new SolidBrush(trasparente), tastoLock.perimetro);
                        dove.DrawPath(new Pen(new SolidBrush(attivo), 3), tastoLock.perimetro);
                    }

                    break;
            }
            /*switch (tastoLock.stato)
            {
                case Lock.Stato.Attivo:
                    //-----------------------------
                    Cursor.Current = this.Cursor;   // mette il puntino
                    //-----------------------------
                    dove.FillPath(new SolidBrush(attivo), tastoLock.perimetro);
                    dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                    dove.DrawPath(new Pen(Brushes.Black, 2), tastoLock.perimetro);
                    break;
            }

            if (flagshift == false)
            {
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    dove.FillPath(new SolidBrush(trasparente), tastoLock.perimetro);
                    dove.DrawPath(new Pen(new SolidBrush(trasparente), 3), tastoLock.perimetro);
                    dove.DrawPath(new Pen(Brushes.Black, 1), tastoLock.perimetro);
                }
                else
                {
                    if (tastoLock.perimetro.IsVisible(posizioneMouse))
                    {
                        dove.FillPath(new SolidBrush(temporaneo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(Brushes.Black, 2), tastoLock.perimetro);
                    }
                    else
                    {
                        dove.FillPath(new SolidBrush(inattivo), tastoLock.perimetro);
                        dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                        dove.DrawPath(new Pen(Brushes.Black, 2), tastoLock.perimetro);
                    }
                }
            }
            if (flaglock == true)
            {
                if (TastiEsagonali.tastiAttivi == 1)
                {
                    dove.FillPath(new SolidBrush(trasparente), tastoLock.perimetro);
                    dove.DrawPath(new Pen(new SolidBrush(attivo), 3), tastoLock.perimetro);
                }
                else
                {
                    dove.FillPath(new SolidBrush(attivo), tastoLock.perimetro);
                    dove.DrawString(tastoLock.testo, fontTasto, new SolidBrush(coloreFont), posizioneTestoLock);
                    dove.DrawPath(new Pen(Brushes.Black, 2), tastoLock.perimetro);
                }
            }*/
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
