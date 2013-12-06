using System;
using System.Linq;
using System.Windows.Forms;

namespace NMCB_Launcher.classes
{
    public static class MyExtensions
    {
        public static bool isURL(this String str)
        {
            return (str.IndexOf("http") > -1 && str.IndexOf("//") > -1);
        }

        public static string getFile(this String str)
        {
            str = Uri.UnescapeDataString(Uri.UnescapeDataString(str));
            str = str.Split('/').Last().Split('&').First();
            return str;
        }

        public static void addLine(this RichTextBox rtb, string str)
        {
            rtb.Text = rtb.Text + "\r\n" + str;
        }
    }
}
