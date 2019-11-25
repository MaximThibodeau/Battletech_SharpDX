using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp4
{
    class BRadder
    {
        public void Button(string path)
        {
            string[] data = System.IO.File.ReadAllLines(path);

            string br = "<BR>";

            int cntr = 0;
            foreach(string s in data)
            {
                data[cntr] += br;
                cntr++;
            }

            System.IO.File.WriteAllLines(path + "2.txt", data);
        }
    }
}
