using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace GoL
{
    class Cell
    {
        public bool alive;
        public int age;
        public Color color;
        public int id;
        public int value;
        public int width;
        public int height;

        public Cell(int _id)
        {
            this.age = 0;
            this.id = _id;
            this.color = Color.White;
            this.alive = false;
            this.value = 0;
            this.width = -1;
            this.height = -1;
    }

        public Cell(int _id, int _w, int _h)
        {
            this.age = 0;
            this.id = _id;
            this.color = Color.White;
            this.alive = false;
            this.value = 0;
            this.width = _w;
            this.height = _h;
        }

        public Cell Clone()
        {
            var newCell = new Cell(this.id);
            newCell.age = this.age;
            newCell.color = this.color;
            newCell.alive = this.alive;
            newCell.value = this.value;
            newCell.width = this.width;
            newCell.height = this.height;
            return newCell;
        }

    }
}
