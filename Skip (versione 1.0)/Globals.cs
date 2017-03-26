
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;



namespace Globals
{

    public class Glob
    {
        public static string Versione = "1.0";
        public static string Mese = "";//XYXYXY "Luglio";
        public static string Anno = "";//XYXYXY "2013";

        public static string Programma_text = "Skip";
        public static string Versione_text = "(Versione " + Versione + ", " + Mese + " " + Anno + ")";
        public static string Autori_text = "";
        public static string Ditta_text = "Universita di Genova";
        public static string Contatto_text = "";
        public static string Collab_text = "";

        public static string ApplicationData_text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string EastLab_text = "EAST-Lab";
        public static string CartellaLocale_text = Path.Combine(ApplicationData_text, EastLab_text, Programma_text);

        public static string Licenza_text = "LICENZA FREEWARE - Questo programma software è distribuito gratuitamente per uso personale e non commerciale, ed è rilasciato così com'è, senza garanzie di alcun tipo, esplicite o implicite (incluse, senza limitazioni, le garanzie implicite di buona qualita ed idoneita ad un uso specifico). Tutti i rischi derivanti dal funzionamento o dal mancato funzionamento del programma software sono a carico dell'Utente.  In nessun caso gli Autori potranno essere considerati responsabili per qualsiasi danno diretto o indiretto di qualsiasi tipo (inclusi, senza limitazioni, danni per perdita di profitti, di interruzione di servizi, perdite di dati, o ogni altra perdita pecuniaria) derivanti dall'utilizzo o dalla impossibilita di utilizzo del programma software, anche se gli Autori sono stati avvisati sulla possibilita che si verifichino questi danni.  Installando, copiando o utilizzando in qualsiasi modo il programma software l'Utente accetta implicitamente ed in tutte le loro parti le suddette condizioni.";

        public static string Help = "Visualizzazione area" + Environment.NewLine + "     consente di visualizzare la struttura di tracce" + Environment.NewLine + "     e settori per l'individuazione delle varie sillabe." + Environment.NewLine + Environment.NewLine + "La tastiera può essere utilizzata in tre modalita differenti:" + Environment.NewLine + "0-Click, 1-Click e Touch." + Environment.NewLine + Environment.NewLine + "Modalita 0-Click" + Environment.NewLine + "     Posizionare il mouse sulla lettera da digitare, attendere" + Environment.NewLine + "     il tempo di preselezione e muoversi nell'intorno del" + Environment.NewLine + "     tasto per scegliere la sillaba voluta. Attendere nuovamente" + Environment.NewLine + "     per far si che la sillaba selezionata venga scritta." + Environment.NewLine + Environment.NewLine + "Modalita 1-Click" + Environment.NewLine + "    Posizionare il mouse sulla lettera da digitare, attendere il" + Environment.NewLine + "     tempo di preselezione e muoversi nell'intorno del tasto per" + Environment.NewLine + "     scegliere la sillaba voluta. Cliccare per far si che la sillaba" + Environment.NewLine + "     selezionata venga scritta." + Environment.NewLine + Environment.NewLine + "Modalita Touch" + Environment.NewLine + "     Per schermi touchscreen, premere la lettera da digitare," + Environment.NewLine + "     attendere il tempo di preselezione e muoversi nell'intorno del" + Environment.NewLine + "     tasto per scegliere la sillaba voluta. Rilasciare il tasto per" + Environment.NewLine + "     far si che la sillaba selezionata venga scritta." + Environment.NewLine + Environment.NewLine + "Utilizzare i bottoni zoom + e zoom - per ridimensionare la finestra a" + Environment.NewLine + " piacimento.";


        //IMPOSTAZIONI DEFAULT
        public static float zoomp = 1.05f;
        public static float zoomm = 0.95f;
        public static float wina = 0;
        public static float winl = 0;
        public static float dimcar = 20;
        public static int click = 1;
        public static int triang = 0;

        //public static int poslettere_mouse = 20;
        //public static int poslettere_touch = 30;
        public static int poslettere;

        public static string defzoom = "1";
        public static string backgroundTastiera = "lightblue";
        public static string coloreScrittaTasto = "black";
        public static string coloreTastoInattivo = "lightblue";
        public static string coloreTastoCorrente = "Cyan";
        public static string coloreTastoAttesa0 = "orange"; //XYXYXY "red";
        public static string coloreTastoAttesa1 = "orange";
        public static string coloreTriangoli = "red";
        public static string coloreBackgroundMenu = "MediumTurquoise";
        public static string coloreTastoAttivoMenu = "yellow";
        public static string coloreForegroundTestoMenu = "black";
        public static string coloreBordoTastiMenu = "black";
        public static string fontTasto = "VERdana	15";
        public static string modalitaAttuazione = "1";
        public static string poslettere_mouse = "20";
        public static string poslettere_touch = "30";
        public static string visualizzaTriangoli = "0";
        public static string tempoPreselezione = "1000";
        public static string tempoAttivazione = "1000";

        // inizializza costante tempo di attesa (modificare solo se necessario!!!)
        public static int Sleep_TIME = 50;   //FC MO

    }

}


