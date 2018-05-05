using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Core;


namespace Milestone_2
{
    /// <summary>
    /// Interaction logic for MapWindow.xaml
    /// </summary>
    public partial class MapWindow : Window
    {
        public MapWindow()
        {
            InitializeComponent();
        }

        public void AddPushPins(double latitude, double longitude, int zoomLevel)
        {
            Pushpin pin = new Pushpin();
            pin.Location = new Location(latitude, longitude);
            map.Children.Add(pin);
            map.SetView(new Location(latitude, longitude), zoomLevel);
        }

        private void switchModesButton_Click(object sender, RoutedEventArgs e)
        {
            if (map.Mode.ToString() == "Microsoft.Maps.MapControl.WPF.RoadMode")
            {
                switchModesButton.Content = "Switch To Street View";
                map.Mode = new AerialMode(true);
            }
            else
            {
                switchModesButton.Content = "Switch To Aerial View";
                map.Mode = new RoadMode();
            }
        }
    }
}
