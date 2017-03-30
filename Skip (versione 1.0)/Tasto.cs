
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Skip
{
    internal class Tasto
    {
        public Tastiera tastiera; // è un riferimento alla tastiera a cui appartiene il tasto
        public int relPosX; // indici che tengono conto della posizione del tasto nella tastiera
        public int relPosY; // (esempio: il tasto in alto a sx avrà relPosX == 0 e relPosY == 0)
        //per ora le dimensioni dei tasti sono settate tutte a zero: bisogna fare i casi (altezza dovrebbe essere uguale per tutti) 
        //ma per esempio per maiusc/spazio avrò dimensione orizzontale diversa
        public static int xDimension = 45; // static così vi si può accedere senza necessariamente istanziarne un oggetto
        public static int yDimension = 25;
        public string contenuto;
        public List<Tasto> completamento;
        public bool haCompletamento = false;
        public enum Stato { Attivo, Premuto }
        public Stato stato;
        public enum TipoTasto { Ortogonale, Riga, Colonna, Altro, Completamento }
        public TipoTasto tipo;
        public GraphicsPath perimetro;
        public Point[] vertici;
        public int xCentro, yCentro;

        public Tasto()
        {
            this.relPosX = 0;
            this.relPosY = 0;
            xDimension = 0;
            yDimension = 0;
            this.contenuto = "";
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
        }
        public Tasto(int x, int y)
        {
            this.relPosX = x;
            this.relPosY = y;
            xDimension = 0;
            yDimension = 0;
            this.contenuto = "";
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
        }
        public Tasto(int x, int y, string tasto)
        {
            this.relPosX = x;
            this.relPosY = y;
            this.contenuto = tasto;
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
            vertici = new Point[4];
            perimetro = new GraphicsPath();
        }
        // questo è il costruttore che è effettivamente usato
        public Tasto(int x, int y, string tasto, int xCentro, int yCentro, TipoTasto tipo, Tastiera tastiera)
        {
            this.relPosX = x;
            this.relPosY = y;
            this.contenuto = tasto;
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
            vertici = new Point[4];
            perimetro = new GraphicsPath();
            vertici[0] = new Point(xCentro - xDimension / 2, yCentro - yDimension / 2); // vertice in alto a sx
            vertici[1] = new Point(xCentro + xDimension / 2, yCentro - yDimension / 2); // vertice in alto a dx
            vertici[2] = new Point(xCentro + xDimension / 2, yCentro + yDimension / 2); // vertice in basso a dx
            vertici[3] = new Point(xCentro - xDimension / 2, yCentro + yDimension / 2); // vertice in basso a sx
            perimetro.AddLine(vertici[0], vertici[1]);
            perimetro.AddLine(vertici[1], vertici[2]);
            perimetro.AddLine(vertici[2], vertici[3]);
            perimetro.AddLine(vertici[3], vertici[0]);
            this.xCentro = xCentro;
            this.yCentro = yCentro;
            this.tipo = tipo;
            this.tastiera = tastiera;
        }
        // costruttore usato solo per i tasti speciali
        public Tasto(string tasto, int xCentro, int yCentro, TipoTasto tipo, Tastiera tastiera, int xDim, int yDim)
        {
            this.contenuto = tasto;
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
            vertici = new Point[4];
            perimetro = new GraphicsPath();
            vertici[0] = new Point(xCentro - xDim / 2, yCentro - yDim / 2); // vertice in alto a sx
            vertici[1] = new Point(xCentro + xDim / 2, yCentro - yDim / 2); // vertice in alto a dx
            vertici[2] = new Point(xCentro + xDim / 2, yCentro + yDim / 2); // vertice in basso a dx
            vertici[3] = new Point(xCentro - xDim / 2, yCentro + yDim / 2); // vertice in basso a sx
            perimetro.AddLine(vertici[0], vertici[1]);
            perimetro.AddLine(vertici[1], vertici[2]);
            perimetro.AddLine(vertici[2], vertici[3]);
            perimetro.AddLine(vertici[3], vertici[0]);
            this.xCentro = xCentro;
            this.yCentro = yCentro;
            this.tipo = tipo;
            this.tastiera = tastiera;
        }
        // aggiunge a questo tasto il completamento da visualizzare quando è selezionato
        public void aggiungiCompletamento(string[] tasti)
        {
            haCompletamento = true;
            // l'ordine dei tasti nel completamento sarà il seguente:
            // 1    2   3
            // 4    5   6
            // 7    8   9
            // per calcolare la posizione dei nuovi tasti usiamo la posizione del tasto corrente della tastiera
            completamento.Add(new Tasto(relPosX - 1, relPosY - 1, tasti[0], tastiera.matriceTasti[relPosX - 1, relPosY - 1].xCentro,
                tastiera.matriceTasti[relPosX - 1, relPosY - 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 1
            completamento.Add(new Tasto(relPosX - 1, relPosY, tasti[1], tastiera.matriceTasti[relPosX - 1, relPosY].xCentro,
                tastiera.matriceTasti[relPosX - 1, relPosY].yCentro, TipoTasto.Completamento, tastiera)); // tasto 2
            completamento.Add(new Tasto(relPosX - 1, relPosY + 1, tasti[2], tastiera.matriceTasti[relPosX - 1, relPosY + 1].xCentro,
                tastiera.matriceTasti[relPosX - 1, relPosY + 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 3
            completamento.Add(new Tasto(relPosX, relPosY - 1, tasti[3], tastiera.matriceTasti[relPosX, relPosY - 1].xCentro,
                tastiera.matriceTasti[relPosX, relPosY - 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 4
            completamento.Add(new Tasto(relPosX, relPosY, tasti[4], tastiera.matriceTasti[relPosX, relPosY].xCentro,
                tastiera.matriceTasti[relPosX, relPosY].yCentro, TipoTasto.Completamento, tastiera)); // tasto 5
            completamento.Add(new Tasto(relPosX, relPosY + 1, tasti[5], tastiera.matriceTasti[relPosX, relPosY + 1].xCentro,
                tastiera.matriceTasti[relPosX, relPosY + 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 6
            completamento.Add(new Tasto(relPosX + 1, relPosY - 1, tasti[6], tastiera.matriceTasti[relPosX + 1, relPosY - 1].xCentro,
                tastiera.matriceTasti[relPosX + 1, relPosY - 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 7
            completamento.Add(new Tasto(relPosX + 1, relPosY, tasti[7], tastiera.matriceTasti[relPosX + 1, relPosY].xCentro,
                tastiera.matriceTasti[relPosX + 1, relPosY].yCentro, TipoTasto.Completamento, tastiera)); // tasto 8
            completamento.Add(new Tasto(relPosX + 1, relPosY + 1, tasti[8], tastiera.matriceTasti[relPosX + 1, relPosY + 1].xCentro,
                tastiera.matriceTasti[relPosX + 1, relPosY + 1].yCentro, TipoTasto.Completamento, tastiera)); // tasto 9
        }
    }
}
