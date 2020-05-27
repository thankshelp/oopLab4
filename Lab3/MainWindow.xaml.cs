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
        List<CCar> cars = new List<CCar>();

        List<PointLatLng> pts = new List<PointLatLng>();

        CCar car = null;
        CHuman h = null;
        CLocation l = null;
        public GMapMarker carMarker = null;
        public GMapMarker humMarker = null;
        public GMapMarker locMarker = null;

        //GMapMarker dst;

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

                //pts.Add(point);

                sorting(objs, point);

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

                if (objType.SelectedIndex > -1)
                {
                    if (objType.SelectedIndex == 0)
                    {
                        //if (car == null)
                        
                            car = new CCar(objTitle.Text, point, Map);
                            objs.Add(car);
                            cars.Add(car);
                            carMarker = car.getMarker();

                            if (h != null)
                            {
                                car.Arrived += h.CarArrived;
                                h.passSeated += car.passSeated;

                            }
                            else
                            {
                                car.setPosition(point);
                                carMarker.Position = point;
                            }
                        
                    }

                    if (objType.SelectedIndex == 1)
                    {
                        if (h == null)
                        {
                            h = new CHuman(objTitle.Text, point);
                            objs.Add(h);
                            humMarker = h.getMarker();

                            if (car != null)
                            {
                                car.Arrived += h.CarArrived;
                                h.passSeated += car.passSeated;
                            }
                            else
                            {
                                h.setPosition(point);
                                humMarker.Position = point;
                            }
                        }

                    }

                    if (objType.SelectedIndex == 2)
                    {
                        if ((h != null) && (l == null))
                        {
                            h.moveTo(point);

                            
                            l = new CLocation(objTitle.Text, point);
                            objs.Add(l);
                            locMarker = l.getMarker();
                            
                        }
                    }

                    if (objType.SelectedIndex == 3)
                    {
                        CArea a = new CArea(objTitle.Text, pts);
                        objs.Add(a);
                        Map.Markers.Add(a.getMarker());
                    }

                    Map.Markers.Clear();

                    foreach (CMapObject cm in objs)
                    {
                        Map.Markers.Add(cm.getMarker());
                    }

                    //Map.Markers.Add(dst);

                    pts.Clear();
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(objectList.SelectedIndex > -1)
            {
                PointLatLng p = objs[objectList.SelectedIndex + 1].getFocus();

                sorting(objs, p);

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
            //foreach(CCar cr in cars)
            
                double distance1;
                double distance2;

                for (int n = 0; n < cars.Count; n++)
                {
                    int min_i = n;

                    for (int j = n + 1; j < cars.Count; j++)
                    {
                        distance1 = cars[j].getDistance(h.getFocus());
                        distance2 = cars[min_i].getDistance(h.getFocus());

                        if (distance2 > distance1)
                        {
                            min_i = j;
                        }
                    }

                    CCar t = cars[n];
                    cars[n] = cars[min_i];
                    cars[min_i] = t;
                }

                //sorting(cr, h.getFocus());
            

            Map.Markers.Add(cars[0].moveTo(h.getFocus()));
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

                sorting(objs, p);

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

        public List<CMapObject> sorting(List<CMapObject> l, PointLatLng p)
        {
            double distance1;
            double distance2;

            for (int n = 0; n < l.Count; n++)
            {
                int min_i = n;

                for (int j = n + 1; j < l.Count; j++)
                {
                    distance1 = l[j].getDistance(p);
                    distance2 = l[min_i].getDistance(p);

                    if (distance2 > distance1)
                    {
                        min_i = j;
                    }
                }

                CMapObject t = l[n];
                l[n] = l[min_i];
                l[min_i] = t;
            }

            return l;
        }
    }
}
