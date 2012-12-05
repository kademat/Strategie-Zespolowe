using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Threading;
using System.Collections;

namespace SZ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Point[] wezel = new Point[1000];
        List<int>[] potomkowie = new List<int>[1000];
        List<int>[] odleglosciWezlow = new List<int>[1000];
        int iteratorWezlow = 0, wezelOjciec = 0;
        int wezelSymulacji = 0;
        double x, y;
        Ellipse el = new Ellipse();
        Image kropla = new Image();

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 1000; i++) {
                potomkowie[i] = new List<int>();
                odleglosciWezlow[i] = new List<int>();
            }
            
        }
        /// <summary>
        /// Kliknięcie przycisku "start symulacji". Po kliknięciu napis zmienia
        /// się na "następny krok". Zakładamy, że symulacja rozpoczyna się już po zakończeniu
        /// budowy grafu/
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startSymulacji_Click(object sender, RoutedEventArgs e)
        {
            DodajKrople();//na pierwszym wezle pojawia sie kropla wody. Jeśli ona istnieje to zastępujemy ją nową kroplą
            kropla.Margin = new Thickness(wezel[wezelSymulacji].X-15,wezel[wezelSymulacji].Y-10,0,0);//ułożenie początkowe kropli
            opis.Text = "Jesteśmy w węźle: " + wezelSymulacji + " .Węzeł ma potmoków: ";//informacja początkowa
            int maxOdleglosc = -1;
            int wybrany = -1;//to będzie do zmiany
            int iter = 0;
            foreach (int value in potomkowie[wezelSymulacji])
            {
                opis.Text += value + " (" + odleglosciWezlow[wezelSymulacji].ElementAt<int>(iter) + ") ";
                if (maxOdleglosc < odleglosciWezlow[wezelSymulacji].ElementAt<int>(iter))
                {
                    maxOdleglosc = odleglosciWezlow[wezelSymulacji].ElementAt<int>(iter);
                    wybrany = value;
                }
                iter++;
            }
            opis.Text += "Wybieramy nr " + wybrany + " i poruszamy się do " + wybrany;//informacja o tym dlaczego ten węzeł wybrany
            if (wybrany != -1)//jeśli coś zostało wybrane.
            {
                AnimacjaRuchu(kropla, wybrany, wezelSymulacji);
                wezelSymulacji = wybrany;
            }
            startSymulacji.Content = "Następny krok";
        }

        /// <summary>
        /// Kolejne kroki symulacji
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            wezelSymulacji = 0;
            DodajKrople();//na pierwszym wezle pojawia sie kropla wody. Jeśli ona istnieje to zastępujemy ją nową kroplą
            kropla.Margin = new Thickness(wezel[wezelSymulacji].X - 15, wezel[wezelSymulacji].Y - 10, 0, 0);//ułożenie początkowe kropli
            opis.Text += "";
            startSymulacji.Content = "Rozpocznij symulację";
        }

        /// <summary>
        /// Kliknięcie lewym klawiszem myszy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rysowanie_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            pobranieXYzPlanszy(out x, out y);
            DodajWezel(x, y);
            wezel[iteratorWezlow] = new Point(x, y);

            if (iteratorWezlow != wezelOjciec) {
                potomkowie[wezelOjciec].Add(iteratorWezlow);
                odleglosciWezlow[wezelOjciec].Add(odleglosc(wezelOjciec, iteratorWezlow));
            }

            iteratorWezlow++;

            RysowanieLinii();
        }
        /// <summary>
        /// Kliknięcie prawym klawiszem myszy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rysowanie_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            pobranieXYzPlanszy(out x, out y);
            if(iteratorWezlow>=1)
                ZaznaczenieWezla();
        }
        /// <summary>
        /// Odległość węzłów
        /// </summary>
        /// <param name="a">Nr pierwszego węzła</param>
        /// <param name="b">Nr drugiego węzła</param>
        /// <returns>x*x + y*y = odległość do kwadratu</returns>
        private int odleglosc(int a, int b)
        {
            int x = (int)(wezel[a].X - wezel[b].X);
            int y = (int)(wezel[a].Y - wezel[b].Y);
            int wynik = x * x + y * y;
            return wynik;
        }
        /// <summary>
        /// Czerwone linie łączące węzły
        /// </summary>
        private void RysowanieLinii()
        {
            Line l = new Line();
            l.Stroke = Brushes.Red;
            l.StrokeThickness = 2;
            l.X1 = wezel[wezelOjciec].X;
            l.X2 = x;
            l.Y1 = wezel[wezelOjciec].Y;
            l.Y2 = y;
            rysowanie.Children.Add(l);
        }
        /// <summary>
        /// Czerwone kółko jest umieszczane nad zaznaczoną skałką - po wcześniejszym kliknięciu
        /// prawym klawiszem myszy
        /// </summary>
        private void ZaznaczenieWezla()
        {
            el.Width = 40;
            el.Height = 40;
            el.Opacity = 0.2;
            el.Fill = new SolidColorBrush(Colors.Red);
            wezelOjciec = wyszukajNajblizszego(x, y);
            el.Margin = new Thickness(wezel[wezelOjciec].X - el.Width / 2, wezel[wezelOjciec].Y - el.Height / 2, 0, 0);
            if (rysowanie.Children.Contains(el))
                rysowanie.Children.Remove(el);
            rysowanie.Children.Add(el);
        }
        /// <summary>
        /// Po kliknięciu prawym klawiszem myszy zaznaczany jest najbliżej położony węzeł
        /// </summary>
        /// <param name="x">Miejsce x kliknięcia</param>
        /// <param name="y">Miejsce y kliknięcia</param>
        /// <returns>Zwraca numer najbliżej położonego węzła</returns>
        private int wyszukajNajblizszego(double x, double y)
        {
            int najblizszyWezel = 0;
            double najmniejszaOdleglosc = Double.MaxValue;
            for (int i = 0; i < iteratorWezlow; i++)
            {
                double odleglosc = (x-wezel[i].X)*(x-wezel[i].X)+(y-wezel[i].Y)*(y-wezel[i].Y);
                if (odleglosc < najmniejszaOdleglosc)
                {
                    najblizszyWezel = i;
                    najmniejszaOdleglosc = odleglosc;
                }
            }
            return najblizszyWezel;
        }

        private void pobranieXYzPlanszy(out double x, out double y)
        {
            x = Mouse.GetPosition(rysowanie).X;
            y = Mouse.GetPosition(rysowanie).Y;
        }

        /// <summary>
        /// Dodawany jest obrazek skałki
        /// </summary>
        /// <param name="x">Położenie X</param>
        /// <param name="y">Położenie Y</param>
        private void DodajWezel(double x, double y)
        {
            string src = @"skala.png";
            Image img = new Image();
            img.Source = new ImageSourceConverter().ConvertFromString(src) as ImageSource;
            img.Margin = new Thickness(x-15, y-10, 0, 0);
            rysowanie.Children.Add(img);
        }
        /// <summary>
        /// Sprawdza zcy kropla istnieje - jeśli tak - usuwa ją i dodaje nową
        /// </summary>
        private void DodajKrople()
        {
            string src = @"drop.gif";
            
            kropla.Source = new ImageSourceConverter().ConvertFromString(src) as ImageSource;
            if (rysowanie.Children.Contains(kropla))
                rysowanie.Children.Remove(kropla);
            rysowanie.Children.Add(kropla);
        }
        /// <summary>
        /// Funkcja do animacji kropli wody
        /// </summary>
        /// <param name="target">Tutaj podajemy obiekt, który chcemy animować - w tym przypadku przesuwać wzdłuż osi X i Y</param>
        /// <param name="animacjaDo">Iterator węzła od którego zaczynamy animacje</param>
        /// <param name="animacjaOd">Iterator węzła do którego idzie animacja</param>
        private void AnimacjaRuchu(Image target, int animacjaDo,int animacjaOd)
        {
            double newX = wezel[animacjaDo].X - wezel[animacjaOd].X;
            double newY = wezel[animacjaDo].Y - wezel[animacjaOd].Y;
            TranslateTransform trans = new TranslateTransform();
            target.RenderTransform = trans;
            DoubleAnimation anim1 = new DoubleAnimation(newX, TimeSpan.FromSeconds(2));
            DoubleAnimation anim2 = new DoubleAnimation(newY, TimeSpan.FromSeconds(2));
            trans.BeginAnimation(TranslateTransform.XProperty, anim1);
            trans.BeginAnimation(TranslateTransform.YProperty, anim2);
        }
    }
}
