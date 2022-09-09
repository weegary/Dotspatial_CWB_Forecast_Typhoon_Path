using System.Text.Json;
using System.Text.Json.Nodes;
using DotSpatial.Controls;
using DotSpatial.Data;
using DotSpatial.Symbology;

namespace Dotspatial_CWB_Forecast_Typhoon_Path
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadCountriesBoundary();
            LoadTyphoons();
            Extent extent = new Extent(118, 18, 130, 30);
            map1.ViewExtents = extent;
        }
        private void LoadCountriesBoundary()
        {
            string[] shapefiles = { @"shapefile\east-asia-boundaries.shp", @"shapefile\southeast-asia-boundaries.shp" };
            foreach (string shapefile in shapefiles)
            { 
                IFeatureSet shp = Shapefile.Open(shapefile);
                var layer = map1.Layers.Add(shp);
                layer.Symbolizer = new PolygonSymbolizer(Color.Transparent, Color.Black);
            }
        }
        private void LoadTyphoons()
        {
            string file_name = @"W-C0034-005.json";
            JsonNode typhoons = GetTyphoons(file_name);
            if (typhoons.GetType().Name == "JsonArray")
            {
                JsonArray _typhoons = (JsonArray)typhoons;
                foreach (var typhoon in _typhoons)
                {
                    DrawForecastTyphoonPath((JsonObject)typhoon);
                }
            }
            else
                DrawForecastTyphoonPath((JsonObject)typhoons);
        }
        private JsonNode GetTyphoons(string file_name)
        {
            string[] json_file = File.ReadAllLines(file_name);
            string jsonText;
            if (json_file.Length != 1)  //判斷json檔案是否為一行字串
                jsonText = string.Concat(json_file);  //如果過不是，則合併成一行
            else
                jsonText = json_file[0];  //如果是，就取矩陣第一個值
            JsonObject json = JsonSerializer.Deserialize<JsonObject>(jsonText);
            JsonObject data = (JsonObject)json["cwbopendata"];
            JsonNode typhoons = data["dataset"]["tropicalCyclones"]["tropicalCyclone"];
            return typhoons;
        }
        private void DrawForecastTyphoonPath(JsonObject typhoon)
        {
            List<NetTopologySuite.Geometries.Coordinate> coordinates = new List<NetTopologySuite.Geometries.Coordinate>();
            foreach (var i in (JsonArray)typhoon["forecast_data"]["fix"])
            {
                string coordinate = i["coordinate"].ToString();
                double lon = Convert.ToDouble(coordinate.Split(',')[0]),
                       lat = Convert.ToDouble(coordinate.Split(',')[1]);
                coordinates.Add(new NetTopologySuite.Geometries.Coordinate(lon, lat));
            }
            NetTopologySuite.Geometries.LineString line = new NetTopologySuite.Geometries.LineString(coordinates.ToArray());
            FeatureSet fs = new FeatureSet(DotSpatial.Data.FeatureType.Line);
            fs.AddFeature(line);
            MapLineLayer layer = new MapLineLayer(fs);
            map1.MapFrame.DrawingLayers.Add(layer);
        }
    }
}