using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;

namespace Steganography
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string path;
        private BitmapImage myImage;
        private Bitmap bmp;
        public MainWindow()
        {
            InitializeComponent();
            RB_encrypt.IsChecked = true;
        }
        /*konwertowanie obrazu*/
        private Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        public BitmapSource ToBitmapSource(Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return bitSrc;
        }

        /*Konwertowanie stringów do binarek*/
        public byte[] ConvertToByteArray(string str)
        {
            return Encoding.GetEncoding("windows-1250").GetBytes(str);
        }

        public String byteToBinary(Byte[] data)
        {
            return string.Join("", data.Select(byt => Convert.ToString(byt, 2).PadLeft(8, '0')));
        }

        string getBits(string txt)
        {
            byte[] bytes = Encoding.GetEncoding("windows-1250").GetBytes(txt);
            return byteToBinary(bytes);
        }

        /*Konwertowanie binrek do stringów*/
        private byte[] binarytoBytes(string bitString)
        {
            return Enumerable.Range(0, bitString.Length / 8).
                Select(pos => Convert.ToByte(
                    bitString.Substring(pos * 8, 8),
                    2)
                ).ToArray();
        }

        private String binaryToString(String binary)
        {
            byte[] bArr = binarytoBytes(binary);
            return Encoding.GetEncoding("windows-1250").GetString(bArr);
        }

        /*Szyfrowanie i deszyfrowanie informacji*/
        void encrypt()
        {
            var message = TB_toEncrypt.Text;
            System.Drawing.Color Pixels;
            System.Drawing.Color color;
            string R, G, B, check = "";
            message = getBits(message);
            StringBuilder sb;
            int int_index = 0, count_zeros = 0;
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Pixels = bmp.GetPixel(j, i);

                    R = getBits(Pixels.R.ToString());
                    G = getBits(Pixels.G.ToString());
                    B = getBits(Pixels.B.ToString());
                    /*R*/
                    sb = new StringBuilder(R);
                    if (int_index < message.Length)
                    {
                        sb[R.Length - 1] = message[int_index];
                        R = sb.ToString();
                        check += R[R.Length - 1];
                    }
                    else
                    {
                        sb[R.Length - 1] = '0';
                        R = sb.ToString();
                        count_zeros++;
                        check += R[R.Length - 1];
                    }

                    /*G*/
                    sb = new StringBuilder(G);
                    if (int_index + 1 < message.Length)
                    {
                        sb[G.Length - 1] = message[int_index + 1];
                        G = sb.ToString();
                        check += G[G.Length - 1];
                    }
                    else
                    {
                        sb[G.Length - 1] = '0';
                        G = sb.ToString();
                        count_zeros++;
                        check += G[G.Length - 1];
                    }

                    /*B*/
                    sb = new StringBuilder(B);
                    if (int_index + 2 < message.Length)
                    {
                        sb[B.Length - 1] = message[int_index + 2];
                        B = sb.ToString();
                        check += B[B.Length - 1];
                    }
                    else
                    {
                        sb[B.Length - 1] = '0';
                        B = sb.ToString();
                        count_zeros++;
                        check += B[B.Length - 1];
                    }

                    if (count_zeros >= 8)
                    {
                        R = binaryToString(R);
                        G = binaryToString(G);
                        B = binaryToString(B);
                        color = System.Drawing.Color.FromArgb(int.Parse(R), int.Parse(G), int.Parse(B));
                        bmp.SetPixel(j, i, color);
                        break;
                    }

                    int_index += 3;
                    R = binaryToString(R);
                    G = binaryToString(G);
                    B = binaryToString(B);
                    color = System.Drawing.Color.FromArgb(int.Parse(R), int.Parse(G), int.Parse(B));
                    bmp.SetPixel(j, i, color);
                }
                if (count_zeros == 8)
                    break;
            }
            img_result.Source = ToBitmapSource(bmp);
            System.Drawing.Image img = (System.Drawing.Image)bmp;
            img.Save("encrypted.png", ImageFormat.Png);  // Correct PNG save
            MessageBox.Show("It's done");
            img_start.Source = null;
        }

        void decrypt()
        {
            string message = "";
            System.Drawing.Color Pixels;
            int count_zero = 0;
            string R, G, B;
            for (int i = 0; i < bmp.Height; i++)
            {
                for (int j = 0; j < bmp.Width; j++)
                {
                    Pixels = bmp.GetPixel(j, i);
                    R = getBits(Pixels.R.ToString());
                    G = getBits(Pixels.G.ToString());
                    B = getBits(Pixels.B.ToString());
                    if (R[R.Length - 1] != '0')
                        count_zero = 0;
                    else
                        count_zero++;

                    if (G[G.Length - 1] != '0')
                        count_zero = 0;
                    else
                        count_zero++;

                    if (B[B.Length - 1] != '0')
                        count_zero = 0;
                    else
                        count_zero++;

                    if (count_zero >= 8)
                        break;
                    message += R[R.Length - 1].ToString() + G[G.Length - 1].ToString() + B[B.Length - 1].ToString();
                }
                if (count_zero >= 8)
                    break;
            }
            message = binaryToString(message);
            TB_result.Text = message;
            MessageBox.Show(message);
        }

        /*Obsługa kontrolek*/
        private void BT_load_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".jpg";
            dlg.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.bmp) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png; *.bmp";

            var result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    myImage = new BitmapImage();
                    path = dlg.FileName;
                    myImage.BeginInit();
                    myImage.UriSource = new Uri(path);
                    myImage.EndInit();
                    img_start.Source = myImage;
                    bmp = BitmapImage2Bitmap(myImage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void RB_encrypt_Checked(object sender, RoutedEventArgs e)
        {
            TB_toEncrypt.IsEnabled = true;
            TB_toEncrypt.ToolTip = "Enter your message to encrypt";
            BT_do.Content = "Let's encrypt the message";
            img_help.ToolTip = "1. Load image by pressing \"Load image\"\n2. Enter your message\n3. Press \"Let's encrypt the message\" or check \"Decrypt\" to decrypt image";
            img_start.Source = null;
            img_result.Source = null;
        }

        private void RB_decrypt_Checked(object sender, RoutedEventArgs e)
        {
            TB_toEncrypt.IsEnabled = false;
            TB_toEncrypt.ToolTip = "If you wanna encrypt, please check Encrypt RadioButton";
            BT_do.Content = "Let's decrypt the image";
            img_help.ToolTip = "1. Press \"Let's decrypt the image\" or check \"Encrypt\" to encrypt image ";
            img_start.Source = null;
            img_result.Source = null;
        }

        private void BT_do_Click(object sender, RoutedEventArgs e)
        {
            if (RB_encrypt.IsChecked == true)
            {
                if (myImage != null && TB_toEncrypt.Text != "")
                {
                    encrypt();
                }
                else
                    MessageBox.Show("No image or message to encrypt");
            }
            else
            {
                if (myImage != null)
                {
                    decrypt();
                }
                else
                    MessageBox.Show("No image to decrypt");
            }
        }
    }

}
