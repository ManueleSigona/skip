using System.Collections.Generic;

namespace Skip
{
    internal class Tastiera
    {
        public int numRighe;
        public int numColonne;
        //in teoria questo numTastiere non serve
        //si può creare direttamente una lista di tastiere dentro MainForm.cs
        public int numTastiere;
        //pensavo di creare una struttura (matrice array o lista) di tasti che contenga i tasti della specifica tastiera
        public Tasto[,] matriceTasti;
        public List<Tasto> listaTasti;
        public Tastiera()
        {
            this.numColonne = 0;
            this.numColonne = 0;
            this.numTastiere = 0;
        }
        public Tastiera(int righe, int colonne)
        {
            this.numColonne = righe;
            this.numColonne = colonne;
            this.numTastiere = 0;
        }
        //anche questo costruttore si può togliere
        public Tastiera(int tastiere)
        {
            this.numColonne = 0;
            this.numColonne = 0;
            this.numTastiere = tastiere;
        }
        public void aggiungiTasto(Tasto tasto)
        {
            this.listaTasti.Add(tasto);
        }
    }
}
