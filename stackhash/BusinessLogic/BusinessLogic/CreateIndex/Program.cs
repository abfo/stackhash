using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CreateIndex
{
    class Program
    {
        private static void Synopsis()
        {
            Console.WriteLine("CreateIndex V1.0. Cucku, Inc. www.stackhash.com");
            Console.WriteLine("CreateIndex <indextype> <Folder> <IndexName> <NumProducts> <NumFiles> <NumEvents> <NumEventInfos> <NumCabs> <CabFileName>");
            Console.WriteLine("  <Sql>           - type of index to create - SQL or XML");
            Console.WriteLine("  <Folder>        - destination folder must not exist");
            Console.WriteLine("  <IndexName>     - index name used as a subfolder");
            Console.WriteLine("  <NumProducts>   - number of products to create");
            Console.WriteLine("  <NumFiles>      - number of files to create in each product");
            Console.WriteLine("  <NumEvents>     - number of events to create in each file");
            Console.WriteLine("  <NumEventInfos> - number of event infos to create in each event");
            Console.WriteLine("  <NumCabs>       - number of CabInfos and cab files to create in each event");
            Console.WriteLine("  <CabFileName>   - the specified cab will be copied - must exist");
            Console.WriteLine(" The total number of events will be NumProducts * NumFiles * NumEvents");
            Console.WriteLine(" The total number of cabs will be NumProducts * NumFiles * NumEvents * NumCabs");
        }

        static void Main(string[] args)
        {
            if (args.Length != 9)
            {
                Synopsis();
                return;
            }

            int numProducts = Int32.Parse(args[3]);
            int numFiles = Int32.Parse(args[4]);
            int numEvents = Int32.Parse(args[5]);
            int numEventInfos = Int32.Parse(args[6]);
            int numCabs = Int32.Parse(args[7]);

            bool sqlIndex = false;

            if ((args[0] == "SQL") || (args[0] == "sql"))
                sqlIndex = true;

            TestIndex.CreateTestIndex(sqlIndex, args[1], args[2], numProducts, numFiles, numEvents, numEventInfos, numCabs, args[8]);

        }
    }
}
