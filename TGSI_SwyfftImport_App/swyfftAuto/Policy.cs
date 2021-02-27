using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace swyfftAuto
{
    public class Policy : Iswyfft
    {
        public List<string> readfiles()
        {
            
            string fileName = string.Empty;
            string destFile = string.Empty;
            string sourcePath = ConfigurationManager.AppSettings["sourcePath"].ToString();
            string targetPath = ConfigurationManager.AppSettings["targetPath"].ToString();
            List<string> lstPDF = new List<string>();
            // To copy all the files in one directory to another directory. 
            // Get the files in the source folder. (To recursively iterate through 
            // all subfolders under the current directory, see 
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously 
            //       in this code example. 
            if (Directory.Exists(sourcePath))
            {
                Thread.Sleep(3500);
                string[] files = Directory.GetFiles(sourcePath);
                // Copy the files and overwrite destination files if they already exist. 
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    if (s.Contains(".pdf"))
                    {
                        fileName = Path.GetFileName(s);
                        lstPDF.Add(fileName);
                        //destFile = System.IO.Path.Combine(targetPath, fileName);
                        //System.IO.File.Copy(s, destFile, true);
                    }
                }
            }
            else
            {
                Log.Write("Read file - Source path or Destination path does not exist! ");
                Console.WriteLine("Source path does not exist!");
            }
            return lstPDF;
        }
        public void movefiles()
        {            
            string fileName = string.Empty;
            string destFile = string.Empty;
            string sourcePath = ConfigurationManager.AppSettings["sourcePath"].ToString();
            string targetPath = ConfigurationManager.AppSettings["targetPath"].ToString();
            
            // To copy all the files in one directory to another directory. 
            // Get the files in the source folder. (To recursively iterate through 
            // all subfolders under the current directory, see 
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously 
            // in this code example. 
            if (Directory.Exists(sourcePath))
            {
                Thread.Sleep(3500);
                string[] files = Directory.GetFiles(sourcePath);
                // Copy the files and overwrite destination files if they already exist. 
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    if (s.Contains(".pdf"))
                    {
                        fileName = Path.GetFileName(s);
                        destFile = Path.Combine(targetPath, fileName);
                        File.Copy(s, destFile, true);
                    }
                }
            }
            else
            {
                Log.Write("Move file - Source path or Destination path does not exist! ");
                Console.WriteLine("Source path does not exist!");
            }
            
        }

        public void moveSUfiles()
        {
            string fileName = string.Empty;
            string destFile = string.Empty;
            string sourcePath = ConfigurationManager.AppSettings["sourcePath"].ToString();
            string targetPath = ConfigurationManager.AppSettings["targetPath"].ToString();

            // To copy all the files in one directory to another directory. 
            // Get the files in the source folder. (To recursively iterate through 
            // all subfolders under the current directory, see 
            // "How to: Iterate Through a Directory Tree.")
            // Note: Check for target path was performed previously 
            // in this code example. 
            if (Directory.Exists(sourcePath))
            {
                Thread.Sleep(3500);
                string[] files = Directory.GetFiles(sourcePath);
                // Copy the files and overwrite destination files if they already exist. 
                foreach (string s in files)
                {
                    // Use static Path methods to extract only the file name from the path.
                    if (s.Contains(".pdf"))
                    {
                        fileName = Path.GetFileNameWithoutExtension(s);
                        fileName = fileName + "-SU.pdf";
                        destFile = Path.Combine(targetPath, fileName);
                        File.Copy(s, destFile, true);
                    }
                }
            }
            else
            {
                Log.Write("Move file - Source path or Destination path does not exist! ");
                Console.WriteLine("Source path does not exist!");
            }

        }

        public void deletefiles()
        {
            Thread.Sleep(2500);
            string sourcePath = @"C:\Users\administrator.TSAT\Downloads"; // "C:\\Users\\ttran\\Downloads";            
            string fileName = string.Empty;
            string destFile = string.Empty;
            sourcePath = ConfigurationManager.AppSettings["sourcePath"].ToString();

            if (Directory.Exists(sourcePath))
            {
                string[] files = Directory.GetFiles(sourcePath);
                // Copy the files and overwrite destination files if they already exist. 
                var tpp = Directory.GetFiles(sourcePath).ToList(); //.ForEach(File.Delete);
                foreach (var item in tpp)
                {
                    if (item.Contains(".pdf"))
                    {
                        File.Delete(item);
                    }
                }

            }
            else
            {
                Log.Write("Deletefile - Source path does not exist! ");
                Console.WriteLine("Source path does not exist!");
            }
        }

    }
}
