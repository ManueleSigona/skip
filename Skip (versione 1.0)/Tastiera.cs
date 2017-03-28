using System;
using System.Collections.Generic;
using System.Drawing;

namespace Skip
{
    internal class Tastiera
    {
        public int numRighe;
        public int numColonne;
        //pensavo di creare una struttura (matrice array o lista) di tasti che contenga i tasti della specifica tastiera
        public Tasto[,] matriceTasti;
        public List<Tasto> listaTasti;
        public int origineX, origineY; // la posizione assoluta della tastiera nella form

        public Font font;
        public Tastiera()
        {
            this.numColonne = 0;
            this.numRighe = 0;
            matriceTasti = new Tasto[9, 18]; // 9 x 18 è la tastiera di default
            origineX = 0;
            origineY = 0;
        }
        public Tastiera(int righe, int colonne, Font f)
        {
            this.numRighe = righe;
            this.numColonne = colonne;
            matriceTasti = new Tasto[righe, colonne];
            origineX = 10; // valori di prova
            origineY = 40;
            font = f;
        }
        public void aggiungiTasto(Tasto tasto)
        {
            this.listaTasti.Add(tasto);
        }

        public void disegnaTastiera(Graphics graphics, Color orth_fg_col, Color orth_bg_col, Color rorth_fg_col, Color rorth_bg_col, Color corth_fg_col, Color corth_bg_col, Color other_fg_col, Color other_bg_col, Color indic_col, Color presel0_col, Color presel1_col)
        {
            SizeF dimensioneTesto; // dimensione del testo da scrivere sul tasto
            PointF posizioneTesto = new PointF(); // dove scrivere il testo sul tasto
            Color coloreSfondo, coloreTesto; // colori di testo e sfondo del tasto

            for (int i = 0; i < numRighe; i++) // disegnamo i tasti uno per uno
            {
                for (int j = 0; j < numColonne; j++)
                { // TODO: bisogna gestire il fatto che un tasto può essere premuto (dunque avrà un colore diverso)
                    // e eventualmente bisognerà disegnare il suo completamento
                    dimensioneTesto = graphics.MeasureString(matriceTasti[i, j].contenuto, font);
                    switch (matriceTasti[i, j].tipo)
                    {
                        case Tasto.TipoTasto.Ortogonale:
                            coloreSfondo = orth_bg_col;
                            coloreTesto = orth_fg_col;
                            break;
                        case Tasto.TipoTasto.Riga:
                            coloreSfondo = rorth_bg_col;
                            coloreTesto = rorth_fg_col;
                            break;
                        case Tasto.TipoTasto.Colonna:
                            coloreSfondo = corth_bg_col;
                            coloreTesto = corth_fg_col;
                            break;
                        case Tasto.TipoTasto.Altro:
                            coloreSfondo = other_bg_col;
                            coloreTesto = other_fg_col;
                            break;
                        default:
                            coloreSfondo = Color.White;
                            coloreTesto = Color.Black;
                            break;
                    }
                    graphics.FillPath(new SolidBrush(coloreSfondo), matriceTasti[i, j].perimetro); // riempie lo sfondo del tasto
                    posizioneTesto.X = matriceTasti[i,j].xCentro - (dimensioneTesto.Width / 2);
                    posizioneTesto.Y = matriceTasti[i, j].yCentro - (dimensioneTesto.Height / 2);
                    graphics.DrawString(matriceTasti[i, j].contenuto, font, new SolidBrush(coloreTesto), posizioneTesto);
                    graphics.DrawPath(new Pen(Color.Black, 2), matriceTasti[i, j].perimetro); // disegna il contorno del tasto
                }
            }
        }
    }
}
