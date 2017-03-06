using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace QWOP_AI_interface_3
{
    class Database
    {
        public static Point failedPoint { get { return new Point(-1, -1); } }
        public static Size scopeSize { get { return new Size(100, 100); } }
        public static Size feedBackSize { get { return new Size(100, 45); } }
        public static Point scopeLocation = failedPoint;
    }
}
