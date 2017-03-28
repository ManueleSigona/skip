
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Skip
{
    internal class Tasto
    {
        public int relPosX; // indici che tengono conto della posizione del tasto nella tastiera
        public int relPosY; // (esempio: il tasto in alto a sx avrà relPosX == 0 e relPosY == 0)
        //per ora le dimensioni dei tasti sono settate tutte a zero: bisogna fare i casi (altezza dovrebbe essere uguale per tutti) 
        //ma per esempio per maiusc/spazio avrò dimensione orizzontale diversa
        public static int xDimension; // static così vi si può accedere senza necessariamente istanziarne un oggetto
        public static int yDimension;
        public string contenuto;
        public List<Tasto> completamento;
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
            xDimension = 30; // sono prove, più avanti vedremo di aggiustare questi valori
            yDimension = 15;
            this.contenuto = tasto;
            completamento = new List<Tasto>();
            stato = Stato.Attivo;
            vertici = new Point[4];
            perimetro = new GraphicsPath();
        }
        public Tasto(int x, int y, string tasto, int xCentro, int yCentro)
        {
            this.relPosX = x;
            this.relPosY = y;
            xDimension = 30; // sono prove, più avanti vedremo di aggiustare questi valori
            yDimension = 15;
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
        }
        // aggiunge a questo tasto il completamento da visualizzare quando è selezionato
        public void aggiungiCompletamento(string[] tasti)
        {
            // l'ordine dei tasti nel completamento sarà il seguente:
            // 1    2   3
            // 4    5   6
            // 7    8   9
            completamento.Add(new Tasto(relPosX - 1, relPosY - 1, tasti[0])); // tasto 1
            completamento.Add(new Tasto(relPosX - 1, relPosY, tasti[1])); // tasto 2
            completamento.Add(new Tasto(relPosX - 1, relPosY + 1, tasti[2])); // tasto 3
            completamento.Add(new Tasto(relPosX, relPosY - 1, tasti[3])); // tasto 4
            completamento.Add(new Tasto(relPosX, relPosY, tasti[4])); // tasto 5
            completamento.Add(new Tasto(relPosX, relPosY + 1, tasti[5])); // tasto 6
            completamento.Add(new Tasto(relPosX + 1, relPosY - 1, tasti[6])); // tasto 7
            completamento.Add(new Tasto(relPosX + 1, relPosY, tasti[7])); // tasto 8
            completamento.Add(new Tasto(relPosX + 1, relPosY + 1, tasti[8])); // tasto 9
            // probabilmente si può fare in modo più furbo, per adesso lo lascio così
        }
    }
}
