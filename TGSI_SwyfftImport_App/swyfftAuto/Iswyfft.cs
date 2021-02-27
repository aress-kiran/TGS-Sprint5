using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swyfftAuto
{
    interface Iswyfft
    {
        List<string> readfiles();
        void movefiles();
        void moveSUfiles();
        void deletefiles();
    }
}
