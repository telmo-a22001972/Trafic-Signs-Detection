using System;
using System.Collections.Generic;
using System.Text;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Threading;
using System.Drawing;

namespace CG_OpenCV
{
    class ImageClass
    {

        /// <summary>
        /// Image Negative using EmguCV library
        /// Slower method
        /// </summary>
        /// <param name="img">Image</param>
        public static void Negative(Image<Bgr, byte> img)
        {
            int x, y;

            Bgr aux;
            for (y = 0; y < img.Height; y++)
            {
                for (x = 0; x < img.Width; x++)
                {
                    // acesso directo : mais lento 
                    aux = img[y, x];
                    img[y, x] = new Bgr(255 - aux.Blue, 255 - aux.Green, 255 - aux.Red);
                }
            }
        }

        /// <summary>
        /// Convert to gray
        /// Direct access to memory - faster method
        /// </summary>
        /// <param name="img">image</param>
        public static void ConvertToGray(Image<Bgr, byte> img)
        {
            unsafe
            {
                // direct access to the image memory(sequencial)
                // direcion top left -> bottom right

                MIplImage m = img.MIplImage;
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                byte blue, green, red, gray;

                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int padding = m.widthStep - m.nChannels * m.width; // alinhament bytes (padding)
                int x, y;

                if (nChan == 3) // image in RGB
                {
                    for (y = 0; y < height; y++)
                    {
                        for (x = 0; x < width; x++)
                        {
                            //retrive 3 colour components
                            blue = dataPtr[0];
                            green = dataPtr[1];
                            red = dataPtr[2];

                            // convert to gray
                            gray = (byte)Math.Round(((int)blue + green + red) / 3.0);

                            // store in the image
                            dataPtr[0] = gray;
                            dataPtr[1] = gray;
                            dataPtr[2] = gray;

                            // advance the pointer to the next pixel
                            dataPtr += nChan;
                        }

                        //at the end of the line advance the pointer by the aligment bytes (padding)
                        dataPtr += padding;
                    }
                }
            }
        }


        public static void mostrarListaEtiquetas(int[,] matrizEtiquetas, int height, int width)
        {
            IDictionary<int, int> etiquetas_e_quantidade = new Dictionary<int, int>();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matrizEtiquetas[y, x] != 0)
                    {
                        //Guardar vezes que etiqueta aparece
                        if (etiquetas_e_quantidade.ContainsKey(matrizEtiquetas[y, x]))
                        {
                            etiquetas_e_quantidade[matrizEtiquetas[y, x]]++;
                        }
                        else
                        {
                            etiquetas_e_quantidade[matrizEtiquetas[y, x]] = 1;
                        }
                    }
                }
            }

            Console.WriteLine("Lista de etiquetas e vezes que aparece");
            foreach (var entry in etiquetas_e_quantidade)
            {
                Console.WriteLine("Etiqueta: " + entry.Key + ": " + entry.Value);
            }
            Console.WriteLine();


        }
        public static void hsv(Image<Bgr, byte> img)
        {
            unsafe
            {
                MIplImage m = img.MIplImage;
                byte* dataPtrCopy = (byte*)img.MIplImage.imageData.ToPointer();
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                int width = img.Width;
                int height = img.Height;
                int nChan = m.nChannels; // number of channels - 3
                int x, y;
                int widthStep = m.widthStep;
                double h=0.0, s=0.0, v = 0.0;
                double red = 0.0, green = 0.0, blue = 0.0;

                for(y = 0; y < height; y++)
                {
                    for(x = 0; x < width; x++)
                    {
                        blue = (dataPtr + nChan * x + widthStep * y)[0] / 255.0;
                        green = (dataPtr + nChan * x + widthStep * y)[1] / 255.0;
                        red = (dataPtr + nChan * x + widthStep * y)[2] / 255.0;

                        double max = Math.Max(red, Math.Max(green, blue));

                        double min = Math.Min(red, Math.Min(green, blue));
                        
                        //Obter valor do H
                        if(max == red && green >= blue)
                        {
                            h = 60 * ((green - blue) / (max - min)) + 0;
                        }else if (max == red && green < blue)
                        {
                            h = 60 * ((green - blue) / (max - min)) + 360;
                        }else if (max == green)
                        {
                            h = 60 * ((blue - red) / (max - min)) + 120;
                        }else if (max == blue) {
                            h = 60 * ((red - green) / (max - min)) + 240;
                        }

                        //Obter valor do S
                        if (max > 0)
                        {
                            s = ((max - min) / max);
                        }else
                        {
                            s = 0;
                        }

                        //Obter valor do V
                        v = max;

                        if ((h < 11 || h > 339) && s > 0.45)
                        {
                            (dataPtr + nChan * x + widthStep * y)[0] = 255;
                            (dataPtr + nChan * x + widthStep * y)[1] = 255;
                            (dataPtr + nChan * x + widthStep * y)[2] = 255;
                        }
                        else
                        {
                            (dataPtr + nChan * x + widthStep * y)[0] = 0;
                            (dataPtr + nChan * x + widthStep * y)[1] = 0;
                            (dataPtr + nChan * x + widthStep * y)[2] = 0;
                        }

                    }
                }
                
            }
        }

        public static int[,] etiquetarHSV(Image<Bgr, byte> img_hsv)
        {
            int count = 1;

            unsafe
            {
                MIplImage m = img_hsv.MIplImage;
                byte* dataPtrCopy = (byte*)img_hsv.MIplImage.imageData.ToPointer();
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image

                int height = img_hsv.Height;
                int width = img_hsv.Width;

                int nChan = m.nChannels; // number of channels - 3
                int widthStep = m.widthStep;

                double blue = 0.0;

                int[,] matrizEtiquetas = new int[height, width];

                for (int y = 0; y<height; y++)
                {
                    for(int x = 0; x < width; x++)
                    {
                        blue = (dataPtr + nChan * x + widthStep * y)[0];
                        
                        if(blue == 255)
                        {
                            matrizEtiquetas[y, x] = count;
                            count ++;
                        }

                    }
                }
               

                return matrizEtiquetas;
            }
        }

         public static void fazerDilatacaoHSV(Image<Bgr, byte> img_hsv, Image<Bgr, byte> img_hsv_copy)
         {
            unsafe
            {
                MIplImage m = img_hsv.MIplImage;
                byte* dataPtrCopy = (byte*)img_hsv_copy.MIplImage.imageData.ToPointer();
                byte* dataPtr = (byte*)m.imageData.ToPointer(); // Pointer to the image
                int width = img_hsv.Width;
                int height = img_hsv.Height;
                int nChan = m.nChannels; // number of channels - 3
                int x, y;
                int widthStep = m.widthStep;
                double red = 0.0, green = 0.0, blue = 0.0;

       

                Emgu.CV.CvInvoke.cvDilate(img_hsv_copy, img_hsv, IntPtr.Zero, 3);
                //Emgu.CV.CvInvoke.cvErode(img_hsv_copy, img_hsv, IntPtr.Zero, 1);
  
            }
         }
        public static IDictionary<int, int> ligarEtiquetas_classico(int[,] matrizEtiquetas, int height, int width)
        {
            IDictionary<int, List<int>> tabelaEquivalencia = new Dictionary<int, List<int>>();

            IDictionary<int, int> etiquetas_e_quantidade = new Dictionary<int, int>();


            for (int y = 1; y < height; y++)
            {
                if(y == height - 1)
                {
                    break;
                }
                for(int x = 1; x < width; x++)
                {
                    
                    var vizinhos = new List<int>();
                    if(x == width - 1)
                    {
                        break;
                    }

                    //Guardar valores da máscara 4x4, (Cima, baixo, esquerda e direita)
                    if(matrizEtiquetas[y, x] != 0)
                    {
                        if(matrizEtiquetas[y-1, x] != 0)
                        {
                            vizinhos.Add(matrizEtiquetas[y - 1, x]);
                        }

                        if (matrizEtiquetas[y,x+1] != 0)
                        {
                            vizinhos.Add(matrizEtiquetas[y, x+1]);
                        }

                        if(matrizEtiquetas[y+1, x] != 0)
                        {
                            vizinhos.Add(matrizEtiquetas[y+1, x]);
                        }

                        if (matrizEtiquetas[y,x-1] != 0)
                        {
                            vizinhos.Add(matrizEtiquetas[y, x-1]);
                        }
                        matrizEtiquetas[y, x] = Math.Min(matrizEtiquetas[y,x],vizinhos.Min());


                        //Guardar equivalencia
                        if(matrizEtiquetas[y,x] < matrizEtiquetas[y, x - 1])
                        {
                            if(tabelaEquivalencia.ContainsKey(matrizEtiquetas[y, x]))
                            {
                                if (!tabelaEquivalencia[matrizEtiquetas[y, x]].Contains(matrizEtiquetas[y, x - 1]))
                                {
                                    tabelaEquivalencia[matrizEtiquetas[y, x]].Add(matrizEtiquetas[y, x - 1]);
                                }

                            }else
                            {
                                List<int> value = new List<int> { matrizEtiquetas[y, x-1] };
                                tabelaEquivalencia[matrizEtiquetas[y, x]] = value;
                            }
                        }
 
                    }
                }
            }

            //Fazer equivalencia
            for(int y = 1; y < height; y++)
            {
                if (y == height - 1)
                {
                    break;
                }
                for (int x = 1; x < width; x++)
                {
                    if (x == width - 1)
                    {
                        break;
                    }

                    //A cada chave ver se o nosso pixel pertence ao seu arrayList
                    int boo = 0;
                    while (boo == 0)
                    {
                        boo++;
                        foreach (var entry in tabelaEquivalencia)
                        {
                            if (entry.Value.Contains(matrizEtiquetas[y, x]))
                            {
                                matrizEtiquetas[y, x] = entry.Key;
                                boo = 0;
                                break;
                            }
                        }
                    }
                }
            }
            
           

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matrizEtiquetas[y,x] != 0)
                    {
                        //Guardar vezes que etiqueta aparece
                        if (etiquetas_e_quantidade.ContainsKey(matrizEtiquetas[y, x]))
                        {
                            etiquetas_e_quantidade[matrizEtiquetas[y, x]]++;
                        }else
                        {
                            etiquetas_e_quantidade[matrizEtiquetas[y, x]] = 1;
                        }
                    }
                }
            }

            //Dar print à etiqueta e às suas equivalentes
            /*
            foreach (var entry in tabelaEquivalencia)
            {
                Console.Write("Chave: " + entry.Key + " Equivalentes: ");
                
                foreach(var item in entry.Value)
                {
                    Console.Write(item+",");
                }
                Console.WriteLine();
            }
            */

            return etiquetas_e_quantidade;
        }


        public static void fazerLimpezaEtiquetas(int[,] matrizEtiquetas, IDictionary<int, int> etiquetas_e_quantidade, int height, int width)
        {
            //Ordenar Dicionario
            //etiquetas_e_quantidade = etiquetas_e_quantidade.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            List<int> etiquetasARemover = new List<int>();

            //Mostrar lista de etiquetas existentes
            //mostrarListaEtiquetas(matrizEtiquetas, height, width);

            //Valor máximo de etiquetas 
            var maxEtiquetas = etiquetas_e_quantidade.Values.Max();


            foreach(var entry in etiquetas_e_quantidade)
            {
                if(entry.Value < (maxEtiquetas * 0.3))
                {
                    var key = entry.Key;
                    etiquetasARemover.Add(key);
                }
            }


            //Pode ser apagado
            /*
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    if (etiquetasARemover.Contains(matrizEtiquetas[y, x]))
                    {
                        matrizEtiquetas[y, x] = 0;
                    }
                }
            }
            */


            //Mostrar etiquetas removidas e removê-las
            
            foreach(var key in etiquetasARemover)
            {
                //Console.WriteLine("Etiqueta removida "+key);
                etiquetas_e_quantidade.Remove(key);
            }
            //Console.WriteLine();
            

            //mostrarListaEtiquetas(matrizEtiquetas, height, width);
        }

        public static void procurarSinal(int[,] matrizEtiquetas, List<string[]> sinaisComEtiquetaECoords, int etiqueta, int height, int width)
        {
            Console.WriteLine("Thread "+Thread.CurrentThread.Name);
            Console.WriteLine("Etiqueta: "+etiqueta);
            Console.WriteLine();

            string[] dummy_vector1 = new string[5];
            dummy_vector1[0] = etiqueta.ToString();  // Etiqueta
            //XL = Maximo
            //XR = 0
            //TOPY = MAX
            //BOTY = 0

            int xL = width-1, xR = 0, yT = height-1, yB = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var pixel = matrizEtiquetas[y, x];

                    if (pixel == etiqueta)
                    {
                        if (x < xL)
                        {
                            xL = x;
                        }
                        if(x > xR)
                        {
                            xR = x;
                        }
                        if(y < yT)
                        {
                            yT = y;
                        }
                        if(y > yB)
                        {
                            yB = y;
                        }
                    }
                }
            }
            dummy_vector1[1] = xL.ToString();  // Left-x
            dummy_vector1[2] = yT.ToString(); // Top-y
            dummy_vector1[3] = xR.ToString(); // Right-x
            dummy_vector1[4] = yB.ToString();  // Bottom-y

            sinaisComEtiquetaECoords.Add(dummy_vector1);

        }

        public static Image<Bgr, byte> Signs(Image<Bgr, byte> img, Image<Bgr, byte> imgCopy, out List<string[]> limitSign, out List<string[]> warningSign, out List<string[]> prohibitionSign, int level)
        {

            limitSign = new List<string[]>();
            warningSign = new List<string[]>();
            prohibitionSign = new List<string[]>();

            List<string[]> limitSignThread = new List<string[]>(); //TESTING ONLY
            List<string[]> warningSignThread = new List<string[]>(); //TESTING ONLY
            List<string[]> prohibitionSignThread = new List<string[]>(); //TESTING ONLY
            
            List<string[]> sinaisComEtiquetaECoords = new List<string[]>();


            Image<Bgr, byte> img_hsv = img.Copy();
            Image<Bgr, byte> signalTeste = img.Copy();

            int height = img.Height;
            int width = img.Width;

            int[,] matrizEtiquetas = new int[img.Height, img.Width];

            hsv(img_hsv);

            fazerDilatacaoHSV(img_hsv, img_hsv.Copy());

            matrizEtiquetas = etiquetarHSV(img_hsv);

            IDictionary<int, int> etiquetas = ligarEtiquetas_classico(matrizEtiquetas, height, width);

            fazerLimpezaEtiquetas(matrizEtiquetas, etiquetas, height, width);

            //mostrarListaEtiquetas(matrizEtiquetas, height, width);
            
            var keys = etiquetas.Keys.ToList();

            if(keys.Count > 1)
            {
                for(int i=0; i<keys.Count(); i++)
                {
                    if(i == keys.Count() - 1)
                    {
                        var chave = keys.ElementAt(i);
                        var tLast = new Thread(() => procurarSinal(matrizEtiquetas, sinaisComEtiquetaECoords, chave, height, width));
                        tLast.Name = i.ToString();
                        tLast.Start();
                        tLast.Join();
                    }else
                    {
                        var chave = keys.ElementAt(i);
                        var t = new Thread(() => procurarSinal(matrizEtiquetas, sinaisComEtiquetaECoords, chave, height, width));
                        t.Name = i.ToString();
                        t.Start();
                    }
                    
                }   
            }else
            {
                var etiqueta = keys.ElementAt(0);
                procurarSinal(matrizEtiquetas, sinaisComEtiquetaECoords, etiqueta, height, width);
            }



            foreach(var sinal in sinaisComEtiquetaECoords)
            {
                int xL = Convert.ToInt32(sinal[1]);
                int yT = Convert.ToInt32(sinal[2]);
                int rectWidth = Convert.ToInt32(sinal[3]) - xL;
                int rectHeight = Convert.ToInt32(sinal[4]) - yT;

                Rectangle rect = new Rectangle(xL, yT, rectWidth, rectHeight);

                signalTeste = img_hsv.Copy(rect);
                

                Console.WriteLine(sinal[0]);
                Console.WriteLine(sinal[1]);
                Console.WriteLine(sinal[2]);
                Console.WriteLine(sinal[3]);
                Console.WriteLine(sinal[4]);
            }



            string[] dummy_vector1 = new string[5];
            dummy_vector1[0] = "70";   // Speed limit
            dummy_vector1[1] = "1160"; // Left-x
            dummy_vector1[2] = "330";  // Top-y
            dummy_vector1[3] = "1200"; // Right-x
            dummy_vector1[4] = "350";  // Bottom-y

            string[] dummy_vector2 = new string[5];
            dummy_vector2[0] = "-1";  // value -1
            dummy_vector2[1] = "669"; // Left-x
            dummy_vector2[2] = "469"; // Top-y
            dummy_vector2[3] = "680"; // Right-x
            dummy_vector2[4] = "480"; // Bottom-y

            string[] dummy_vector3 = new string[5];
            dummy_vector3[0] = "-1";  // value -1
            dummy_vector3[1] = "669"; // Left-x
            dummy_vector3[2] = "469"; // Top-y
            dummy_vector3[3] = "680"; // Right-x
            dummy_vector3[4] = "480"; // Bottom-y

            limitSign.Add(dummy_vector1);
            warningSign.Add(dummy_vector2);
            prohibitionSign.Add(dummy_vector3);


            return signalTeste;

        }



    }
}
