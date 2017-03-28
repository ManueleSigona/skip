
namespace Skip
{
    internal class Tasto
    {
        public int relPosX;
        public int relPosY;
        //per ora le dimensioni dei tasti sono settate tutte a zero: bisogna fare i casi (altezza dovrebbe essere uguale per tutti) 
        //ma per esempio per maiusc/spazio avrò dimensione orizzontale diversa
        public int xDimension;
        public int yDimension;
        public string contenuto;

        public Tasto()
        {
            this.relPosX = 0;
            this.relPosY = 0;
            this.xDimension = 0;
            this.yDimension = 0;
            this.contenuto = "";
        }
        public Tasto(int x, int y)
        {
            this.relPosX = x;
            this.relPosX = y;
            this.xDimension = 0;
            this.yDimension = 0;
            this.contenuto = "";
        }
        public Tasto(int x, int y, string tasto)
        {
            this.relPosX = x;
            this.relPosX = y;
            this.xDimension = 0;
            this.yDimension = 0;
            this.contenuto = tasto;
        }
    }
}
