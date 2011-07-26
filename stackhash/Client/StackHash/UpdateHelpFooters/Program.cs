using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using HtmlAgilityPack;

namespace UpdateHelpFooters
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Make sure StackHash Help is checked out and press any key to continue...");
            Console.ReadKey();
            
            DirectoryInfo helpDir = new DirectoryInfo(@"R:\StackHash\Client\StackHash\StackHash\Help");
            FileInfo[] htmlFiles = helpDir.GetFiles("*.htm");
            foreach (FileInfo htmlFile in htmlFiles)
            {
                Console.WriteLine(htmlFile.FullName);

                HtmlDocument doc = new HtmlDocument();
                doc.Load(htmlFile.FullName);

                foreach (HtmlNode div in doc.DocumentNode.SelectNodes("//div"))
                {
                    if (div.Attributes["id"] != null)
                    {
                        if (div.Attributes["id"].Value == "helpfooter")
                        {
                            div.InnerHtml = Properties.Resources.FooterContent;
                        }
                    }
                }

                doc.Save(htmlFile.FullName);
            }

            Console.WriteLine("Done, press any key to exit...");
            Console.ReadKey();
        }
    }
}
