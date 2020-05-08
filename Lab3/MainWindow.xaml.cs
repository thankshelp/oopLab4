using System;
using System.Collections.Generic;
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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Device.Location;

namespace Lab3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<CMapObject> objs = new List<CMapObject>();

        List<PointLatLng> pts = new List<PointLatLng>();

        public MainWindow()
        {
            InitializeComponent();
        }


        private void MapLoaded(object sender, RoutedEventArgs e)
        {
            // настройка доступа к данным
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // установка провайдера карт
            Map.MapProvider = OpenStreetMapProvider.Instance;

            // установка зума карты
            Map.MinZoom = 2;
            Map.MaxZoom = 17;
            Map.Zoom = 15;
            // установка фокуса карты
            Map.Position = new PointLatLng(55.012823, 82.950359);

            // настройка взаимодействия с картой
            Map.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            Map.CanDragMap = true;
            Map.DragButton = MouseButton.Left;

        }
        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (searchDist.IsChecked == true)
            {
                PointLatLng point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);

                pts.Add(point);
                
                double distance1;
                double distance2;

                for (int i = 0; i < objs.Count; i++)
                {
                    int min_i = i;

                    for (int j = i + 1; j < objs.Count; j++)
                    {
                        distance1 = objs[j].getDistance(point);
                        distance2 = objs[min_i].getDistance(point);

                        if (distance2 > distance1)
                        {
                            min_i = j;
                        }
                    }

                    CMapObject t = objs[i];
                    objs[i] = objs[min_i];
                    objs[min_i] = t;
                }

                objectList.Items.Clear();

                foreach (CMapObject cm in objs)
                {
                    objectList.Items.Add(cm.getTitle() + " " + Math.Round(cm.getDistance(point)).ToString());
                }
            }
            else
            {

                PointLatLng point = Map.FromLocalToLatLng((int)e.GetPosition(Map).X, (int)e.GetPosition(Map).Y);

                pts.Add(point);
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(objectList.SelectedIndex > -1)
            {
                PointLatLng p = objs[objectList.SelectedIndex + 1].getFocus();

                double distance1;
                double distance2;

                for (int n = 0; n < objs.Count; n++)
                {
                    int min_i = n;

                    for (int j = n + 1; j < objs.Count; j++)
                    {
                        distance1 = objs[j].getDistance(p);
                        distance2 = objs[min_i].getDistance(p);

                        if (distance2 > distance1)
                        {
                            min_i = j;
                        }
                    }

                    CMapObject t = objs[n];
                    objs[n] = objs[min_i];
                    objs[min_i] = t;
                }

                objectList.Items.Clear();

                foreach (CMapObject cm in objs)
                {
                    if(cm.getDistance(p) != 0)
                    objectList.Items.Add(cm.getTitle() + " " + Math.Round(cm.getDistance(p)).ToString());
                }

                Map.Position = p;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (objType.SelectedIndex > -1)
            {
                if (objType.SelectedIndex == 0)
                {
                    CCar car = new CCar(objTitle.Text, pts[0]);
                    objs.Add(car);
                    Map.Markers.Add(car.getMarker());
                }
            }
            if (objType.SelectedIndex == 4)
            {
                CRoute r = new CRoute(objTitle.Text, pts);
                objs.Add(r);
                Map.Markers.Add(r.getMarker());
            }
            if (objType.SelectedIndex == 1)
            {
                CHuman h = new CHuman(objTitle.Text, pts[0]);
                objs.Add(h);
                Map.Markers.Add(h.getMarker());
            }
            if (objType.SelectedIndex == 2)
            {
                CLocation l = new CLocation(objTitle.Text, pts[0]);
                objs.Add(l);
                Map.Markers.Add(l.getMarker());
            }
            if (objType.SelectedIndex == 3)
            {
                CArea a = new CArea(objTitle.Text, pts);
                objs.Add(a);
                Map.Markers.Add(a.getMarker());
            }

            pts.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            objectList.Items.Clear();

            int i = 0;
            PointLatLng p = Map.Position;

            foreach (CMapObject cm in objs)
            {
                if (searchTitle.Text == cm.getTitle())
                {
                    i += 1;
                    p = cm.getFocus();

                }
            }

            if (i == 1)
            {
                Map.Position = p;

                double distance1;
                double distance2;

                for (int n = 0; n < objs.Count; n++)
                {
                    int min_i = n;

                    for (int j = n + 1; j < objs.Count; j++)
                    {
                        distance1 = objs[j].getDistance(p);
                        distance2 = objs[min_i].getDistance(p);

                        if (distance2 > distance1)
                        {
                            min_i = j;
                        }
                    }

                    CMapObject t = objs[n];
                    objs[n] = objs[min_i];
                    objs[min_i] = t;
                }

                objectList.Items.Clear();

                foreach (CMapObject cm in objs)
                { 
                    if(cm.getDistance(p) != 0)
                    objectList.Items.Add(cm.getTitle() + " " + Math.Round(cm.getDistance(p)).ToString());
                }
            }

            if (i>1)
            {
                foreach (CMapObject cm in objs)
                {
                    if (searchTitle.Text == cm.getTitle())
                    {
                        objectList.Items.Add(cm.getTitle());
                    }
                }
            }
        }
    }
}
