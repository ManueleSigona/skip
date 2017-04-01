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
        public List<Tasto> listaTasti = new List<Tasto>();
        public List<Tasto> tastSpeciali = new List<Tasto>();
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
            origineX = 50; // valori di prova
            origineY = 100;
            font = f;
        }
        public void aggiungiTasto(Tasto tasto)
        {
            this.listaTasti.Add(tasto);
        }

        public void aggiungiTastiSpeciali()
        {
            int xCentro, yCentro;
            // la yCentro sarà la stessa per tutti i tasti speciali, invece la xCentro sarà diversa
            yCentro = matriceTasti[numRighe - 1, 0].yCentro + Tasto.yDimension; // prendiamo la y dell'ultima riga di tasti e la "abbassiamo" della dimensione di un tasto
            xCentro = origineX + Tasto.xDimension;
            Tasto SHIFT = new Tasto("SHIFT", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension); // la larghezza (xDim) è doppia rispetto ai tasti normali
            xCentro += Tasto.xDimension * 2;
            Tasto FR = new Tasto("FR", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            xCentro += Tasto.xDimension * 2;
            Tasto CF = new Tasto("CF", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            xCentro += Tasto.xDimension * 2;
            Tasto CR = new Tasto("CR", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            xCentro += Tasto.xDimension * 3;
            Tasto SPACE = new Tasto(" ", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 4, Tasto.yDimension);
            xCentro += Tasto.xDimension * 3;
            Tasto DEL = new Tasto("DEL", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            xCentro += Tasto.xDimension * 2;
            Tasto P_DEL = new Tasto("P-DEL", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            xCentro += Tasto.xDimension * 2;
            Tasto INVIO = new Tasto("INVIO", xCentro, yCentro, Tasto.TipoTasto.Altro, this, Tasto.xDimension * 2, Tasto.yDimension);
            // non rimane che aggiungerli alla lista apposita
            tastSpeciali.Add(SHIFT);
            tastSpeciali.Add(FR);
            tastSpeciali.Add(CF);
            tastSpeciali.Add(CR);
            tastSpeciali.Add(SPACE);
            tastSpeciali.Add(DEL);
            tastSpeciali.Add(P_DEL);
            tastSpeciali.Add(INVIO);
        }

        public void disegnaTastiera(Graphics graphics, Color orth_fg_col, Color orth_bg_col, Color rorth_fg_col, Color rorth_bg_col, Color corth_fg_col, Color corth_bg_col, Color other_fg_col, Color other_bg_col, Color indic_col, Color presel0_col, Color presel1_col, int completamentoi, int completamentoj)
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
                    graphics.DrawPath(new Pen(Color.Black, 1), matriceTasti[i, j].perimetro); // disegna il contorno del tasto
                }
            }
            // rimangono da disegnare i tasti speciali
            foreach (Tasto t in tastSpeciali)
            {
                dimensioneTesto = graphics.MeasureString(t.contenuto, font);
                graphics.FillPath(new SolidBrush(other_bg_col), t.perimetro); // riempie lo sfondo del tasto
                posizioneTesto.X = t.xCentro - (dimensioneTesto.Width / 2);
                posizioneTesto.Y = t.yCentro - (dimensioneTesto.Height / 2);
                graphics.DrawString(t.contenuto, font, new SolidBrush(other_fg_col), posizioneTesto);
                graphics.DrawPath(new Pen(Color.Black, 1), t.perimetro); // disegna il contorno del tasto
            }
            // bisogna vedere se c'è un tasto con relativo completamento da disegnare:
            if (completamentoi != -1 && completamentoj != -1)
            {
                foreach (Tasto tCompl in matriceTasti[completamentoi, completamentoj].completamento)
                {
                    dimensioneTesto = graphics.MeasureString(tCompl.contenuto, font);
                    graphics.FillPath(new SolidBrush(presel0_col), tCompl.perimetro); // riempie lo sfondo del tasto
                    posizioneTesto.X = tCompl.xCentro - (dimensioneTesto.Width / 2);
                    posizioneTesto.Y = tCompl.yCentro - (dimensioneTesto.Height / 2);
                    graphics.DrawString(tCompl.contenuto, font, new SolidBrush(orth_fg_col), posizioneTesto);
                    graphics.DrawPath(new Pen(Color.Black, 1), tCompl.perimetro); // disegna il contorno del tasto
                }
            }
        }
    }
}
