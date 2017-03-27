
namespace Skip
{
    internal class Tasto
    {
        public int relPosX;
        public int relPosY;
        public string contenuto;

        public Tasto()
        {
            this.relPosX = 0;
            this.relPosY = 0;
            this.contenuto = "";
        }
        public Tasto(int x, int y)
        {
            this.relPosX = x;
            this.relPosX = y;
            this.contenuto = "";
        }
        public Tasto(int x, int y, string tasto)
        {
            this.relPosX = x;
            this.relPosX = y;
            this.contenuto = tasto;
        }
    }
}
